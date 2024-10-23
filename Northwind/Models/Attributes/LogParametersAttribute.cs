namespace Northwind.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class LogParametersAttribute : Attribute
    {
        public bool Enabled { get; }

        public LogParametersAttribute(bool enabled = true)
        {
            Enabled = enabled;
        }
    }
}
