using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Common
{
    /// <summary>
    /// Extension methods that wrap calls to the XML Serializer.
    /// </summary>
    public static class ZetaXml
    {

        /// <summary>
        /// Serializes to a Utf8 string. Throws an exception if error occurs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeUtf8<T>(T obj)
        {
            byte[] result = Serialize(obj);
            return Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// Throws a exception if error occurs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T obj)
        {
            byte[] result = null;

            XmlSerializer s = new XmlSerializer(typeof(T));

            try
            {
                //TextWriter w = new StreamWriter(filename);
                using (MemoryStream stream = new MemoryStream())
                {
                    s.Serialize(stream, obj);
                    stream.Seek(0, SeekOrigin.Begin);
                    result = stream.ToArray();
                }
            }
            catch (Exception e)
            {
                throw new XmlException($"Error while serializing type of: {typeof(T)}. ", e);
            }

            return result;
        }

        /// <summary>
        /// Throws exception on error.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="errorOnNullResult">Default: true.  Throws an exception if the deserialized object is null.</param>
        /// <param name="allowedArrayTypes"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml, bool errorOnNullResult = true, Type[] allowedArrayTypes = null)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(xml);

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                T result = Deserialize<T>(stream, errorOnNullResult, allowedArrayTypes);
                return result;
            }
        }

        /// <summary>
        /// Deserializes to target type T. Throws exception on error. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="errorOnNullResult">Default: true.  Throws an exception if the deserialized object is null.</param>
        /// <param name="allowedArrayTypes">If the XML includes arrays, then object Types allowed in the array must be specified with this parameter</param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream stream, bool errorOnNullResult = true, Type[] allowedArrayTypes = null)
        {

            object o = Deserialize(typeof(T), stream, errorOnNullResult, allowedArrayTypes);

            try
            {
                return (T)o;
            }
            catch (Exception e)
            {
                throw new XmlException($"Error with deserialization result. Expected type: {typeof(T)}. Got type of {o?.GetType()}.");
            }
        }

        /// <summary>
        /// Deserializes to targeted object type specified by first parameter. Throws exception on error.
        /// </summary>
        /// <param name="objectType">The type of resulting object expected from deserialization. This is the type of return object.</param>
        /// <param name="xml"></param>
        /// <param name="errorOnNullResult">Default: true.  Throws an exception if the deserialized object is null.</param>
        /// <param name="allowedArrayTypes">If the XML includes arrays, then object Types allowed in the array must be specified with this parameter</param>
        /// <returns>An object of Type specified by the input parameter objectType</returns>
        public static object Deserialize(Type objectType, string xml, bool errorOnNullResult = true, Type[] allowedArrayTypes = null)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(xml);

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                return Deserialize(objectType, stream, errorOnNullResult, allowedArrayTypes);
            }
        }


        /// <summary>
        /// Deserializes to targeted object type specified by first parameter. Throws exception on error.
        /// </summary>
        /// <param name="objectType">The type of resulting object expected from deserialization. This is the type of return object.</param>
        /// <param name="stream">Don't forget a USING() statement for your stream to Dispose it.</param>
        /// <param name="errorOnNullResult">Default: true.  Throws an exception if the deserialized object is null.</param>
        /// <param name="allowedArrayTypes">If the XML includes arrays, then object Types allowed in the array must be specified with this parameter</param>
        /// <returns>An object of Type specified by the input parameter objectType</returns>
        public static object Deserialize(Type objectType, Stream stream, bool errorOnNullResult = true, Type[] allowedArrayTypes = null)
        {
            object result;

            try
            {
                XmlSerializer x;

                if (allowedArrayTypes == null)
                    x = new XmlSerializer(objectType);
                else
                    x = new XmlSerializer(objectType, allowedArrayTypes);

                result = x.Deserialize(stream);
            }
            catch (Exception e)
            {
                throw new XmlException($"Error while deserializing to target type: {objectType}. ", e);
            }

            if (errorOnNullResult && result == null)
                throw new XmlException($"Error while deserializing to target type: {objectType}. Result object is NULL.");

            return result;
        }

        /// <summary>
        /// Constructs an XmlReader and parses the given xml. Skips over initial content nodes to get to the first real body element
        /// </summary>
        /// <returns></returns>
        public static string GetRootElement(string xml)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(xml);

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                using (XmlReader xmlReader = XmlReader.Create(stream))
                {
                    //Move to first content node.
                    XmlNodeType type = xmlReader.MoveToContent();

                    //If nodeType is "None", it means we reached the end of the stream.
                    while (type != XmlNodeType.Element && type != XmlNodeType.None)
                    {
                        type = xmlReader.MoveToContent(); //Go to next content node.
                    }

                    if (type == XmlNodeType.None)
                        throw new XmlException("Root element not found.");
                    else
                        return xmlReader.Name; //Return the name of the current node
                }
            }
        }
    }
}
