using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace YAETWi.Helper
{
    public static class MetaProvider
    {
        public static Dictionary<int, string> describeEvents(string provider)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            ProviderMetadata meta = new ProviderMetadata(provider);
            IEnumerable<EventMetadata> events = meta.Events;
            foreach (EventMetadata m in events)
            {
                dict[(int)m.Id] = m.Description;
            }
            return dict;
        }

        public static Dictionary<int, string> describeOpcodes(string provider)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            ProviderMetadata meta = new ProviderMetadata(provider);
            IList<System.Diagnostics.Eventing.Reader.EventOpcode> opcodes = meta.Opcodes;
            foreach (System.Diagnostics.Eventing.Reader.EventOpcode o in opcodes)
            {
                dict[o.Value] = o.DisplayName;
            }
            return dict;
        }
    }
}
