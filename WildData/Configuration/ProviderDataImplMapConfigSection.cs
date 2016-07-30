using System.Configuration;

namespace ModernRoute.WildData.Configuration
{
    public class ProviderDataImplMapConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("providerDataImpls")]
        public ProviderDataImplCollection ProviderDataImpls
        {
            get { return ((ProviderDataImplCollection)(base["providerDataImpls"])); }
        }
    }
}
