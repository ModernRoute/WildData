using System.Configuration;

namespace ModernRoute.WildData.Configuration
{
    [ConfigurationCollection(typeof(ProviderDataImplElement))]
    public class ProviderDataImplCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderDataImplElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderDataImplElement)(element)).ProviderName;
        }

        public new ProviderDataImplElement this[string idx]
        {
            get { return (ProviderDataImplElement)BaseGet(idx); }
        }
    }
}
