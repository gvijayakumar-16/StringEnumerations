using System;
using System.Reflection;

namespace StringEnum
{
    public class StringValueAttribute : Attribute
    {
        private string _value;

        /// <summary>
        /// Creates a new <see cref="StringValueAttribute"/> instance.
        /// </summary>
        /// <param name="value">Value.</param>
        public StringValueAttribute(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public string Value
        {
            get { return _value; }
        }
    }

    public static class EnumExtensions
    {
        /// <summary>
        /// Gets a string value for a particular enum value.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>String Value associated via a <see cref="StringValueAttribute"/> attribute, or null if not found.</returns>
        public static string ToStringValue(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;
            var attribute = (StringValueAttribute)fieldInfo.GetCustomAttribute(typeof(StringValueAttribute));
            return attribute == null ? null : attribute.Value;
        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value (case sensitive).
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="stringValue">String value.</param>
        /// <returns>Enum value associated with the string value, or null if not found.</returns>
        public static TEnum Parse<TEnum>(this string stringValue) where TEnum : struct, IConvertible
        {
            return Parse<TEnum>(stringValue, false);
        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="stringValue">String value.</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Enum value associated with the string value, or null if not found.</returns>
        public static TEnum Parse<TEnum>(this string stringValue, bool ignoreCase) where TEnum : struct, IConvertible
        {
            object output = null;
            string enumStringValue = null;
            var type = typeof(TEnum);

            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException(string.Format("Supplied type must be an Enum.  Type was {0}", type.ToString()));

            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in type.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs.Length > 0)
                    enumStringValue = attrs[0].Value;

                //Check for equality then select actual enum value.
                if (string.Compare(enumStringValue, stringValue, ignoreCase) == 0)
                {
                    output = Enum.Parse(type, fi.Name);
                    break;
                }
            }

            return (TEnum)output;
        }

        /// <summary>
        /// Get the based on string
        /// </summary>
        /// <typeparam name="TEnum">Type of enum</typeparam>
        /// <param name="intValue">Integer value.</param>
        /// <returns></returns>
        public static TEnum Parse<TEnum>(this int intValue) where TEnum : struct, IConvertible
        {
            var type = typeof(TEnum);

            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException(string.Format("Supplied type must be an Enum.  Type was {0}", type.ToString()));

            if (Enum.IsDefined(type, intValue))
                return (TEnum)Enum.ToObject(typeof(TEnum), intValue);

            throw new InvalidOperationException($"Error - {intValue} is not an underlying value of the {type} enumeration.");
        }
    }
}
