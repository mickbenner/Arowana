using System;

namespace Arowana.Common
{
    /// <summary>A simple assembly attribute indicating the date and that the assembly was built with. This class cannot be inherited.</summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AssemblyBuildDateAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="AssemblyBuildDateAttribute" /> class.</summary>
        /// <param name="buildDate">The build mode.</param>
        public AssemblyBuildDateAttribute(string buildDate)
        {
            BuildDate = DateTime.Parse(buildDate);
        }

        /// <summary>Gets the build mode.</summary>
        /// <value>The build mode.</value>
        public DateTime BuildDate { get; }
    }
}