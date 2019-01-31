using System.Configuration;

namespace Elekto
{
    /// <summary>
    /// Obtém as configurações a partir de um arquivo de configuração.
    /// </summary>
    public class ConfigFileGeneralProperties : GeneralPropertiesBase
    {
        /// <summary>
        /// Gets the general property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override string GetGeneralProperty(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Sets the general property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual bool SetGeneralProperty(string key, string value)
        {
            ConfigurationManager.AppSettings[key] = value;
            return true;
        }

        /// <summary>
        /// Deletes the property.
        /// </summary>
        /// <param name="key">The key.</param>
        public virtual void Delete(string key)
        {
            ConfigurationManager.AppSettings.Remove(key);
        }
    }
}