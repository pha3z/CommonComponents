using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Common
{
    public static class ObjectUtils
    {
        /// <summary>
        /// Returns the Getter for the given property name
        /// </summary>
        /// <typeparam name="TObject">Object Type</typeparam>
        /// <typeparam name="TProperty">Property Type</typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Func<TObject, TProperty> GetPropGetter<TObject, TProperty>(string propertyName)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject), "value");

            Expression propertyGetterExpression = Expression.Property(paramExpression, propertyName);

            Func<TObject, TProperty> result =
                Expression.Lambda<Func<TObject, TProperty>>(propertyGetterExpression, paramExpression).Compile();

            return result;
        }

        /// <summary>
        /// Returns the Setter for the given property name
        /// </summary>
        /// <typeparam name="TObject">Object Type</typeparam>
        /// <typeparam name="TProperty">Property Type</typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Action<TObject, TProperty> GetPropSetter<TObject, TProperty>(string propertyName)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject));

            ParameterExpression paramExpression2 = Expression.Parameter(typeof(TProperty), propertyName);

            MemberExpression propertyGetterExpression = Expression.Property(paramExpression, propertyName);

            Action<TObject, TProperty> result = Expression.Lambda<Action<TObject, TProperty>>
            (
                Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression, paramExpression2
            ).Compile();

            return result;
        }

        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyProperties(object source, object destination)
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

        public static string PrettyPrintSingleLine(object src, int depth = 4, int indentSize = 2, char indentChar = ' ')
        {
            return ObjectDumper.Dump(singleLine: true, src, depth, indentSize, indentChar);
        }

        public static string PrettyPrint(object src, int depth = 4, int indentSize = 2, char indentChar = ' ')
        {
            return ObjectDumper.Dump(singleLine: false, src, depth, indentSize, indentChar);
        }
    }
}
