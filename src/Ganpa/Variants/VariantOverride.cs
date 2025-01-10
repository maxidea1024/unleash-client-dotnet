namespace Ganpa.Variants
{
    public class VariantOverride
    {
        public string ContextName { get; }

        public string[] Values { get; }

        public VariantOverride(string contextName, params string[] values)
        {
            ContextName = contextName;
            Values = values;
        }
    }
}