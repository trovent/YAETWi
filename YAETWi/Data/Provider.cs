using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using YAETWi.Helper;

namespace YAETWi.Data
{
    public class Provider
    {
        public Dictionary<string, string> providersByName { get; set; }
        public Dictionary<string, string> providersByGUID { get; set; }
        public Dictionary<string, string> providersAll { get; set; }

        public Provider()
        {
            providersByName = new Dictionary<string, string>();
            providersByGUID = new Dictionary<string, string>();
            providersAll = new Dictionary<string, string>();
            mapProviders();
            providersAll = mergeProviders();
        }

        void mapProviders()
        {
            Logger.printInfo("Obtaining meta information for providers");
            ProviderMetadata meta;
            foreach (var provider in EventLogSession.GlobalSession.GetProviderNames())
            {
                try
                {
                    meta = new ProviderMetadata(provider);
                    if (!meta.Id.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                    {
                        providersByName.Add(provider, meta.Id.ToString());
                        providersByGUID.Add(meta.Id.ToString(), provider);
                    }
                }
                catch (Exception) 
                {
                    Logger.printNCFailure(String.Format("cannot find meta information for the provider: {0}", provider));
                }
            }
        }

        Dictionary<string, string> mergeProviders()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kvp in providersByName)
            {
                dict[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in providersByGUID)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return dict;
        }
    }
}
