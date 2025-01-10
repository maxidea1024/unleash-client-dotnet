namespace Ganpa
{
    using Internal;
    using Logging;
    using Strategies;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Utilities;
    using Variants;

    /// <inheritdoc />
    public class DefaultGanpa : IGanpa
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(DefaultGanpa));

        private static readonly UnknownStrategy UnknownStrategy = new UnknownStrategy();

        private static int INITIALIZED_INSTANCE_COUNT = 0;

        private const int ERROR_ON_INSTANCE_COUNT = 10;

        private static readonly IStrategy[] DefaultStrategies =
        {
            new DefaultStrategy(),
            new UserWithIdStrategy(),
            new GradualRolloutUserIdStrategy(),
            new GradualRolloutRandomStrategy(),
            new ApplicationHostnameStrategy(),
            new GradualRolloutSessionIdStrategy(),
            new RemoteAddressStrategy(),
            new FlexibleRolloutStrategy()
        };

        private readonly GanpaSettings _settings;
        private readonly Dictionary<string, IStrategy> _strategyMap;
        internal readonly GanpaServices Services;
        private readonly WarnOnce _warnOnce;

        ///// <summary>
        ///// Initializes a new instance of Ganpa client with a set of default strategies.
        ///// </summary>
        ///// <param name="config">Ganpa settings</param>
        ///// <param name="strategies">Additional custom strategies.</param>
        public DefaultGanpa(GanpaSettings settings, params IStrategy[] strategies)
            : this(settings, overrideDefaultStrategies: false, strategies)
        {
        }

        ///// <summary>
        ///// Initializes a new instance of Ganpa client.
        ///// </summary>
        ///// <param name="config">Ganpa settings</param>
        ///// <param name="overrideDefaultStrategies">When true, it overrides the default strategies.</param>
        ///// <param name="strategies">Custom strategies.</param>
        public DefaultGanpa(GanpaSettings settings, bool overrideDefaultStrategies, params IStrategy[] strategies)
        {
            var currentInstanceNo = Interlocked.Increment(ref INITIALIZED_INSTANCE_COUNT);

            _settings = settings;

            _warnOnce = new WarnOnce(Logger);

            GanpaSettingsValidator.Validate(_settings);

            strategies = SelectStrategies(strategies, overrideDefaultStrategies);
            _strategyMap = BuildStrategyMap(strategies);

            Services = new GanpaServices(settings, EventConfig, _strategyMap);

            Logger.Info(() =>
                $"GANPA: Ganpa instance number {currentInstanceNo} is initialized and configured with: {_settings}");

            if (currentInstanceNo >= ERROR_ON_INSTANCE_COUNT)
            {
                Logger.Error(() => $"GANPA: Ganpa instance count for this process is now {currentInstanceNo}.");
                Logger.Error(() =>
                    "Ideally you should only need 1 instance of Ganpa per app/process, we strongly recommend setting up Ganpa as a singleton.");
            }
        }

        /// <inheritdoc />
        public ICollection<FeatureToggle> FeatureToggles => Services.ToggleCollection.Instance.Features;

        private EventCallbackConfig EventConfig { get; } = new EventCallbackConfig();

        /// <inheritdoc />
        public bool IsEnabled(string toggleName)
        {
            return IsEnabled(toggleName, false);
        }

        /// <inheritdoc />
        public bool IsEnabled(string toggleName, bool defaultSetting)
        {
            return IsEnabled(toggleName, Services.ContextProvider.Context, defaultSetting);
        }

        public bool IsEnabled(string toggleName, GanpaContext context)
        {
            return IsEnabled(toggleName, context, false);
        }

        public bool IsEnabled(string toggleName, GanpaContext context, bool defaultSetting)
        {
            var enabled = CheckIsEnabled(toggleName, context, defaultSetting).Enabled;
            RegisterCount(toggleName, enabled);
            return enabled;
        }

        private FeatureEvaluationResult CheckIsEnabled(
            string toggleName,
            GanpaContext context,
            bool defaultSetting,
            Variant defaultVariant = null)
        {
            var featureToggle = GetToggle(toggleName);
            var enhancedContext = context.ApplyStaticFields(_settings);
            var enabled = DetermineIsEnabledAndStrategy(toggleName, featureToggle, enhancedContext, defaultSetting,
                out var strategy);
            var variant = DetermineVariant(enabled, featureToggle, strategy, enhancedContext, defaultVariant);
            if (variant != null)
            {
                variant.FeatureEnabled = enabled;
            }

            if (featureToggle?.ImpressionData ?? false)
            {
                EmitImpressionEvent("isEnabled", enhancedContext, enabled, featureToggle.Name);
            }

            return new FeatureEvaluationResult { Enabled = enabled, Variant = variant };
        }

        private bool DetermineIsEnabledAndStrategy(
            string toggleName,
            FeatureToggle featureToggle,
            GanpaContext enhancedContext,
            bool defaultSetting,
            out ActivationStrategy? strategy)
        {
            strategy = null;

            if (featureToggle == null)
            {
                Logger.Warn(() =>
                    $"GANPA: Feature flag {toggleName} not present, returning default setting: {defaultSetting}");
                return defaultSetting;
            }
            else if (!featureToggle.Enabled)
            {
                // Overall false
                return false;
            }
            else if (featureToggle.Strategies.Count == 0)
            {
                return true;
            }
            else
            {
                strategy = featureToggle.Strategies.FirstOrDefault(s =>
                {
                    var uniqueConstraints = new HashSet<Constraint>(ResolveConstraints(s));
                    uniqueConstraints.UnionWith(s.Constraints);
                    return GetStrategyOrUnknown(s.Name).IsEnabled(s.Parameters, enhancedContext, uniqueConstraints);
                });
            }

            if (featureToggle.Dependencies.Any() && !ParentDependenciesAreSatisfied(featureToggle, enhancedContext))
            {
                return false;
            }

            return strategy != null;
        }

        private bool ParentDependenciesAreSatisfied(FeatureToggle featureToggle, GanpaContext context)
        {
            return featureToggle.Dependencies.All(d => DependenciesSatisfied(featureToggle, d, context));
        }

        private bool DependenciesSatisfied(FeatureToggle featureToggle, Dependency dependency, GanpaContext context)
        {
            var parentToggle = GetToggle(dependency.Feature);
            if (parentToggle == null)
            {
                _warnOnce.Warn(dependency.Feature + featureToggle.Name,
                    $"GANPA: Parent feature toggle {dependency.Feature} was not found in the cache, the evaluation of this dependency will always be false");
                return false;
            }

            if (parentToggle.Dependencies.Any())
            {
                return false;
            }

            if (!dependency.Enabled)
            {
                return !CheckIsEnabled(dependency.Feature, context, false).Enabled;
            }

            if (dependency.Variants == null || !dependency.Variants.Any())
            {
                return CheckIsEnabled(dependency.Feature, context, false).Enabled;
            }

            var checkResult = CheckIsEnabled(dependency.Feature, context, false, Variant.DISABLED_VARIANT);
            return checkResult.Enabled && dependency.Variants.Contains(checkResult.Variant.Name);
        }

        private static Variant DetermineVariant(bool enabled,
            FeatureToggle featureToggle,
            ActivationStrategy strategy,
            GanpaContext context,
            Variant defaultVariant)
        {
            if (enabled)
            {
                if (strategy == null)
                {
                    return VariantUtils.SelectVariant(featureToggle, context, defaultVariant);
                }

                strategy.Parameters.TryGetValue("groupId", out var groupId);
                groupId = groupId ?? featureToggle.Name;
                var variant = VariantUtils.SelectVariant(groupId, context, strategy.Variants, strategy.Parameters);
                return variant ?? VariantUtils.SelectVariant(featureToggle, context, defaultVariant);
            }
            else
            {
                return defaultVariant;
            }
        }

        public Variant GetVariant(string toggleName)
        {
            return GetVariant(toggleName, Services.ContextProvider.Context, Variant.DISABLED_VARIANT);
        }

        public Variant GetVariant(string toggleName, Variant defaultVariant)
        {
            return GetVariant(toggleName, Services.ContextProvider.Context, defaultVariant);
        }

        public Variant GetVariant(string toggleName, GanpaContext context)
        {
            return GetVariant(toggleName, context, Variant.DISABLED_VARIANT);
        }

        public Variant GetVariant(string toggleName, GanpaContext context, Variant defaultValue)
        {
            var toggle = GetToggle(toggleName);

            var evaluationResult = CheckIsEnabled(toggleName, context, false, defaultValue);

            RegisterCount(toggleName, evaluationResult.Enabled);

            RegisterVariant(toggleName, evaluationResult.Variant);

            var enhancedContext = context.ApplyStaticFields(_settings);

            if (toggle?.ImpressionData ?? false)
            {
                EmitImpressionEvent("getVariant", enhancedContext, evaluationResult.Enabled, toggle.Name,
                    evaluationResult.Variant?.Name);
            }

            return evaluationResult.Variant;
        }

        public IEnumerable<VariantDefinition> GetVariants(string toggleName)
        {
            return GetVariants(toggleName, Services.ContextProvider.Context);
        }

        public IEnumerable<VariantDefinition> GetVariants(string toggleName, GanpaContext context)
        {
            if (!IsEnabled(toggleName, context))
            {
                return null;
            }

            var toggle = GetToggle(toggleName);

            return toggle?.Variants;
        }

        private FeatureToggle? GetToggle(string toggleName)
        {
            return Services
                .ToggleCollection
                .Instance
                .GetToggleByName(toggleName);
        }

        private void RegisterCount(string toggleName, bool enabled)
        {
            if (Services.IsMetricsDisabled)
            {
                return;
            }

            Services.MetricsBucket.RegisterCount(toggleName, enabled);
        }

        private void RegisterVariant(string toggleName, Variant variant)
        {
            if (Services.IsMetricsDisabled)
            {
                return;
            }

            Services.MetricsBucket.RegisterCount(toggleName, variant.Name);
        }

        private static IStrategy[] SelectStrategies(IStrategy[] strategies, bool overrideDefaultStrategies)
        {
            if (overrideDefaultStrategies)
            {
                return strategies ?? Array.Empty<IStrategy>();
            }
            else
            {
                return DefaultStrategies.Concat(strategies).ToArray();
            }
        }

        private static Dictionary<string, IStrategy> BuildStrategyMap(IStrategy[] strategies)
        {
            var map = new Dictionary<string, IStrategy>(strategies.Length);

            foreach (var strategy in strategies)
            {
                map.Add(strategy.Name, strategy);
            }

            return map;
        }

        private IStrategy GetStrategyOrUnknown(string strategy)
        {
            return _strategyMap.TryGetValue(strategy, out var value)
                ? value
                : UnknownStrategy;
        }

        private IEnumerable<Constraint> ResolveConstraints(ActivationStrategy activationStrategy)
        {
            foreach (var segment in activationStrategy.Segments.Select(segmentId =>
                         Services.ToggleCollection.Instance.GetSegmentById(segmentId)))
            {
                if (segment != null)
                {
                    foreach (var constraint in segment.Constraints)
                    {
                        yield return constraint;
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        public void ConfigureEvents(Action<EventCallbackConfig> callback)
        {
            if (callback == null)
            {
                Logger.Error(() => $"GANPA: Ganpa->ConfigureEvents parameter callback is null");
                return;
            }

            try
            {
                callback(EventConfig);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    () => $"GANPA: Ganpa->ConfigureEvents executing callback threw exception: {ex.Message}");
            }
        }

        private void EmitImpressionEvent(string type, GanpaContext context, bool enabled, string name,
            string? variant = null)
        {
            if (EventConfig?.ImpressionEvent == null)
            {
                Logger.Error(() => $"GANPA: Ganpa->ImpressionData callback is null, unable to emit event");
                return;
            }

            try
            {
                EventConfig.ImpressionEvent(new ImpressionEvent
                {
                    Type = type,
                    Context = context,
                    EventId = Guid.NewGuid().ToString(),
                    Enabled = enabled,
                    FeatureName = name,
                    Variant = variant
                });
            }
            catch (Exception ex)
            {
                Logger.Error(() => $"GANPA: Emitting impression event callback threw exception: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Services?.Dispose();
        }
    }
}