using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Elekto
{
    /// <summary>
    ///     Implementação genérica do provedor, gravador de propriedades
    /// </summary>
    public abstract class GeneralPropertiesBase
    {
        /// <summary>
        ///     Gets the general property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>String.Empty if not configured.</returns>
        public abstract string GetGeneralProperty(string key);


        /// <summary>
        ///     Gets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Get<T>(string key, T defaultValue)
        {
            var s = GetGeneralProperty(key);
            if (string.IsNullOrEmpty(s))
            {
                return defaultValue;
            }

            var value = ConvertFromString<T>(s, out var success);
            if (!success)
            {
                return defaultValue;
            }

            return value;
        }


        /// <summary>
        ///     Converts from string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <returns></returns>
        private static T ConvertFromString<T>(string s, out bool success)
        {
            success = false;
            object o = default(T);

            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
            {
                if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                {
                    return (T) o;
                }

                success = true;
                o = i;
                return (T) o;
            }

            if (typeof(T) == typeof(bool))
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    success = true;
                    return (T) (object) false;
                }

                s = s.ToLowerInvariant().Trim();
                if (string.IsNullOrWhiteSpace(s))
                {
                    success = true;
                    return (T) (object) false;
                }

                if (s == "v" || s == "true" || s == "1" || s == "on" || s == "verdadeiro")
                {
                    success = true;
                    return (T) (object) true;
                }

                if (s == "f" || s == "false" || s == "0" || s == "off" || s == "falso")
                {
                    success = true;
                    return (T) (object) false;
                }

                return (T) o;
            }

            if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
            {
                if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                {
                    return (T) o;
                }

                success = true;
                o = d;
                return (T) o;
            }

            if (o is DateTime)
            {
                if (!DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var d))
                {
                    return (T) o;
                }

                success = true;
                o = d;
                return (T) o;
            }

            if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
            {
                if (!Guid.TryParse(s, out var g))
                {
                    return (T) o;
                }

                success = true;
                o = g;
                return (T) o;
            }

            if (typeof(T) == typeof(string))
            {
                o = s;
                success = true;
                return (T) o;
            }

            if (o is Enum)
            {
                Enum e;
                try
                {
                    e = (Enum) Enum.Parse(typeof(T), s);
                }
                catch
                {
                    return (T) o;
                }

                success = true;
                o = e;
                return (T) o;
            }

            if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
            {
                if (!decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                {
                    return (T) o;
                }

                success = true;
                o = d;
                return (T) o;
            }


            // Assume que é um objeto serializado binariamente e colocado em uma string Base64
            try
            {
                var byteArray = Convert.FromBase64String(s);
                var memoryStream = new MemoryStream(byteArray);
                var formatter = new BinaryFormatter();
                o = formatter.Deserialize(memoryStream);
            }
            catch
            {
                return (T) o;
            }

            success = true;
            return (T) o;
        }
    }
}