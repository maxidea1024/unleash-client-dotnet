namespace Ganpa.Strategies
{
    using System.Collections.Generic;
    using Ganpa.Internal;

    /// <summary>
    /// Defines a strategy for enabling a feature.
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Gets the strategy name 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Calculates if the strategy is enabled for a given context
        /// </summary>
        bool IsEnabled(Dictionary<string, string> parameters, GanpaContext context);

        /// <summary>
        /// Calculates if the strategy is enabled for a given context and constraints
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="context"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        bool IsEnabled(Dictionary<string, string> parameters, GanpaContext context,
            IEnumerable<Constraint> constraints);
    }
}