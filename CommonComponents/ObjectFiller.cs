using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common
{
    public class ObjectFiller
    {
        /*
        /// <summary>
        /// Fills all string fields of object o with the string value parameter. VERY SLOW! Uses reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to assign to string fields.</param>
        /// <param name="o">Object with fields to fill</param>
        /// <returns></returns>
        public static T FillStringFields<T>(T o, string value) where T : class
        {
            foreach (var f in typeof(T).GetFields())
            {
                if (f.FieldType == typeof(string))
                    f.SetValue(o, value);
            }

            return o;
        }*/

        /// <summary>
        /// Fills all string fields of object o with the string value parameter. VERY SLOW! Uses reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to assign to string fields.</param>
        /// <param name="o">If o is null, a new instance of T will be created. Otherwise, value will be mapped to fields on o.</param>
        /// /// <param name="CreateAndFillChildren">If true, objects will be instanced for object fields and their string fields will be filled...recursively</param>
        /// <returns></returns>
        public static T FillStringFields<T>(string value, T o = null, bool CreateAndFillChildren = false) where T : class
        {
            if (o == null)
                o = (T)Activator.CreateInstance(typeof(T));

            foreach (FieldInfo f in o.GetType().GetFields())
            {
                if (f.FieldType == typeof(string))
                    f.SetValue(o, value);
                else
                {
                    if (CreateAndFillChildren && f.FieldType.IsClass)
                    {
                        try
                        {
                            object fieldObject = Activator.CreateInstance(f.FieldType);
                            FillStringFields(value, fieldObject, true);
                            f.SetValue(o, fieldObject);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[TEST FILLER] Error while Instancing type: " + f.FieldType.ToString());
                        }
                    }

                }
            }

            return o;
        }

        /// <summary>
        /// Fills all string properties of object o with the string value parameter. VERY SLOW! Uses reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to assign to string properties.</param>
        /// <param name="o">Object with properties to fill</param>
        /// <returns></returns>
        public static T FillStringProperties<T>(T o, string value) where T : class
        {
            foreach (var f in typeof(T).GetProperties())
            {
                if (f.PropertyType == typeof(string))
                    f.SetValue(o, value);
            }


            return o;
        }
    }
}
