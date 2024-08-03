namespace Penqueen.Types
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GenerateProxiesAttribute : Attribute
    {
        public bool CustomProxies { get; set; } = false;
        public bool ConfigurationMixins { get; set; } = false;
    }
}