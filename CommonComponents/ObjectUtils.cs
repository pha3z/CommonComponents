using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
    }
}
