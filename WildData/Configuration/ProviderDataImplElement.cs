using System.Configuration;

namespace ModernRoute.WildData.Configuration
{
    public class ProviderDataImplElement : ConfigurationElement
    {
        private const string _ProviderName = "providerName";
        private const string _DataImplAssembly = "dataImplAssembly";

        [ConfigurationProperty(_ProviderName, DefaultValue = "", IsKey = true, IsRequired = true)]
        public string ProviderName
        {
            get { return ((string)(base[_ProviderName])); }
            protected set { base[_ProviderName] = value; }
        }

        [ConfigurationProperty(_DataImplAssembly, DefaultValue = "", IsKey = false, IsRequired = true)]
        public string DataImplAssembly
        {
            get { return ((string)(base[_DataImplAssembly])); }
            protected set { base[_DataImplAssembly] = value; }
        }
    }
}
