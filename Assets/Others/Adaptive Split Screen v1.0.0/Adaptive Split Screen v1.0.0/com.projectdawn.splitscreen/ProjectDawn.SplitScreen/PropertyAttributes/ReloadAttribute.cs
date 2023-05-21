using UnityEngine;

namespace ProjectDawn.SplitScreen
{
    public enum ReloadScope
    {
        Default,
        BuiltinResources,
        BuiltinExtraResources,
    }

    /// <summary>
    /// Automatically loads asset from specified path into the property.
    /// </summary>
    public class ReloadAttribute : PropertyAttribute
    {
        public string Path;
        public ReloadScope Scope;

        public ReloadAttribute(string path, ReloadScope scope = ReloadScope.Default)
        {
            Path = path;
            Scope = scope;
        }
    }
}
