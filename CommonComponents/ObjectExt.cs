using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Common
{

    /// <summary>
    /// A static class for common object extensions
    /// </summary>
    public static class ObjectExt
    {
        public static bool HasValue(this object o) => o != null;
        public static bool IsNull(this object o) => o == null;
        public static string IsNullAsString(this object o) => (o == null) ? "true" : "false";

        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyProperties(this object source, object destination)
        {
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");

            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();
            // Collect all the valid properties to map
            var results = from srcProp in typeSrc.GetProperties()
                          let targetProperty = typeDest.GetProperty(srcProp.Name)
                          where srcProp.CanRead
                          && targetProperty != null
                          && (targetProperty.GetSetMethod(true) != null && !targetProperty.GetSetMethod(true).IsPrivate)
                          && (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                          && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                          select new { sourceProperty = srcProp, targetProperty = targetProperty };
            //map the properties
            foreach (var props in results)
            {
                props.targetProperty.SetValue(destination, props.sourceProperty.GetValue(source, null), null);
            }
        }

        public static string PrettyPrintSingleLine(this object src, int depth = 4, int indentSize = 2, char indentChar = ' ')
        {
            return ObjectDumper.Dump(singleLine: true, src, depth, indentSize, indentChar);
        }

        public static string PrettyPrint(this object src, int depth = 4, int indentSize = 2, char indentChar = ' ')
        {
            return ObjectDumper.Dump(singleLine: false, src, depth, indentSize, indentChar);
        }


    }
}
