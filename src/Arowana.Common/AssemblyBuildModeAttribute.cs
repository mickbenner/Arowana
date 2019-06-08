using System;

namespace Arowana.Common
{
    /// <summary>A simple assembly attribute indicating the build mode that the assembly was built with. This class cannot be inherited.</summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AssemblyBuildModeAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="AssemblyBuildModeAttribute" /> class.</summary>
        /// <param name="buildMode">The build mode.</param>
        public AssemblyBuildModeAttribute(string buildMode)
        {
            BuildMode = buildMode;
        }

        /// <summary>Gets the build mode.</summary>
        /// <value>The build mode.</value>
        public string BuildMode { get; }
    }
}