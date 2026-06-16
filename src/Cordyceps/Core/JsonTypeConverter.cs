using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cordyceps.Core
{
    /// <summary>
    /// Converts between JSON Schema types and CLR types, with cross-type coercion
    /// for MCP clients that may send string-encoded numbers or vice versa.
    /// </summary>
    public static class JsonTypeConverter
    {
        /// <summary>
        /// Maps a CLR type to its JSON Schema type string.
        /// </summary>
        public static string GetJsonType(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int) || type == typeof(long)) return "integer";
            if (type == typeof(double) || type == typeof(float)) return "number";
            if (type == typeof(bool)) return "boolean";
            return "string";
        }

        /// <summary>
        /// Converts a JToken to the specified CLR type, with cross-type coercion.
        /// Handles cases where MCP clients send string-encoded numbers ("300" instead of 300).
        /// </summary>
        public static object ConvertJsonValue(JToken element, Type targetType)
        {
            if (targetType == typeof(string))
                return element.Type == JTokenType.String
                    ? element.Value<string>()
                    : element.ToString(Formatting.None);

            if (targetType == typeof(int))
            {
                if (element.Type == JTokenType.Integer || element.Type == JTokenType.Float)
                    return element.Value<int>();
                if (element.Type == JTokenType.String &&
                    int.TryParse(element.Value<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var intVal))
                    return intVal;
                throw new InvalidOperationException(
                    $"Cannot convert {element.Type} '{element}' to int");
            }

            if (targetType == typeof(long))
            {
                if (element.Type == JTokenType.Integer || element.Type == JTokenType.Float)
                    return element.Value<long>();
                if (element.Type == JTokenType.String &&
                    long.TryParse(element.Value<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var longVal))
                    return longVal;
                throw new InvalidOperationException(
                    $"Cannot convert {element.Type} '{element}' to long");
            }

            if (targetType == typeof(double))
            {
                if (element.Type == JTokenType.Integer || element.Type == JTokenType.Float)
                    return element.Value<double>();
                if (element.Type == JTokenType.String &&
                    double.TryParse(element.Value<string>(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var dblVal))
                    return dblVal;
                throw new InvalidOperationException(
                    $"Cannot convert {element.Type} '{element}' to double");
            }

            if (targetType == typeof(float))
            {
                if (element.Type == JTokenType.Integer || element.Type == JTokenType.Float)
                    return (float)element.Value<double>();
                if (element.Type == JTokenType.String &&
                    float.TryParse(element.Value<string>(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var fltVal))
                    return fltVal;
                throw new InvalidOperationException(
                    $"Cannot convert {element.Type} '{element}' to float");
            }

            if (targetType == typeof(bool))
            {
                if (element.Type == JTokenType.Boolean) return element.Value<bool>();
                if (element.Type == JTokenType.String && bool.TryParse(element.Value<string>(), out var boolVal))
                    return boolVal;
                if (element.Type == JTokenType.Integer || element.Type == JTokenType.Float)
                    return element.Value<int>() != 0;
                throw new InvalidOperationException(
                    $"Cannot convert {element.Type} '{element}' to bool");
            }

            return element.Type == JTokenType.String
                ? element.Value<string>()
                : element.ToString(Formatting.None);
        }
    }
}
