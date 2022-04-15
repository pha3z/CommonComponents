    using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Common
{
    /// <summary>
    /// This was originally called "System" but that caused conflicts with the System namespace in Blazor
    /// </summary>
    public static class ZetaSys
    {
        public static string GetStartupPath()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            return global::System.IO.Path.GetDirectoryName(assembly.Location);
        }
    }
}