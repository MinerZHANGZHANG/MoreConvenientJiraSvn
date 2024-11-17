namespace MoreConvenientJiraSvn.Plugin
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PluginAttribute(string developerName, string version, params string[] dependencies)
        : Attribute
    {
        public string DeveloperName { get; } = developerName;
        public string Version { get; } = version;
        public string[] Dependencies { get; } = dependencies;
    }
}
