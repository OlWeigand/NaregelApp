//------------------------------------------------------------------------------------
// <copyright file="About.cs" company="Regel Partners B.V.">
//     All rights are reserved. Reproduction or transmission in whole or in part, in
//     any form or by any means, electronic, mechanical or otherwise, is prohibited
//     without the prior written consent of the copyright owner.
// </copyright>
//------------------------------------------------------------------------------------
namespace RegelPartners.Protocols
{
    // directives
    using System.Reflection;

    /// <summary>
    /// Initializes the About class.
    /// </summary>
    public static class About
    {
        // fields
        private static Assembly assembly = Assembly.GetExecutingAssembly();
        private static AssemblyName assemblyName = assembly.GetName();

        /// <summary>
        /// Gets the title of the assembly.
        /// </summary>
        public static string Title { get; } = ((AssemblyTitleAttribute)AssemblyTitleAttribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute))).Title;

        /// <summary>
        /// Gets the version of the assembly.
        /// </summary>
        public static string Version { get; } = assemblyName.Version.Major.ToString("0") + "." + assemblyName.Version.Minor.ToString("0") + "." + assemblyName.Version.Build.ToString("000") + "." + assemblyName.Version.Revision.ToString("0");

        /// <summary>
        /// Gets the filename of the assembly.
        /// </summary>
        public static string Filename { get; } = Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public static new string ToString()
        {
            // return string
            return Title + " " + Version;
        }
    }
}