namespace Ganpa.Variants
{
    public class Payload
    {
        public string Type { get; }

        public string Value { get; }

        public Payload(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (!(obj is Payload otherPayload))
            {
                return false;
            }

            return Equals(otherPayload.Type, Type) && Equals(otherPayload.Value, Value);
        }

        public override int GetHashCode()
        {
            return new { Type, Value }.GetHashCode();
        }
    }
}