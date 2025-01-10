using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ganpa.Internal;
using Ganpa.Strategies;

namespace Ganpa.Variants
{
    internal static class VariantUtils
    {
        private static readonly uint VARIANT_NORMALIZATION_SEED = 86028157;

        public static Variant SelectVariant(string groupId, GanpaContext context,
            List<VariantDefinition> variantDefinitions, Dictionary<string, string>? strategyParameters = null)
        {
            var totalWeight = variantDefinitions.Sum(v => v.Weight);

            if (totalWeight == 0)
            {
                return null;
            }

            string? strategyStickiness = null;
            strategyParameters?.TryGetValue("stickiness", out strategyStickiness);
            var stickiness = variantDefinitions.FirstOrDefault()?.Stickiness ?? strategyStickiness ?? "default";
            var target = StrategyUtils.GetNormalizedNumber(GetIdentifier(context, stickiness), groupId,
                VARIANT_NORMALIZATION_SEED, totalWeight);

            var counter = 0;
            Variant? result = null;
            foreach (var variantDefinition in variantDefinitions.Where(variantDefinition =>
                         variantDefinition.Weight != 0))
            {
                if (variantDefinition.Overrides?.Count > 0 &&
                    variantDefinition.Overrides.Any(OverrideMatchesContext(context)))
                {
                    result = variantDefinition.ToVariant();
                    break;
                }

                counter += variantDefinition.Weight;
                if (counter >= target && result == null)
                {
                    result = variantDefinition.ToVariant();
                }
            }

            return result;
        }

        public static Variant SelectVariant(FeatureToggle feature, GanpaContext context,
            Variant? defaultVariant = null)
        {
            if (feature == null)
            {
                return defaultVariant;
            }

            return SelectVariant(feature.Name, context, feature.Variants) ?? defaultVariant;
        }

        private static Func<VariantOverride, bool> OverrideMatchesContext(GanpaContext context)
        {
            return (variantOverride) =>
            {
                string? contextValue = null;
                switch (variantOverride.ContextName)
                {
                    case "userId":
                        contextValue = context.UserId;
                        break;
                    case "sessionId":
                        contextValue = context.SessionId;
                        break;
                    case "remoteAddress":
                        contextValue = context.RemoteAddress;
                        break;
                    default:
                        context.Properties.TryGetValue(variantOverride.ContextName, out contextValue);
                        break;
                }

                return variantOverride.Values.Contains(contextValue ?? "");
            };
        }

        private static string GetIdentifier(GanpaContext context, string stickiness)
        {
            if (stickiness == "default")
            {
                return context.UserId
                       ?? context.SessionId
                       ?? context.RemoteAddress
                       ?? GetRandomValue();
            }

            var stickinessValue = context.GetByName(stickiness);
            return stickinessValue ?? GetRandomValue();
        }

        private static string GetRandomValue()
        {
            return new Random().NextDouble().ToString(CultureInfo.InvariantCulture);
        }
    }
}