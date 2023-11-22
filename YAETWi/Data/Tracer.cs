using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAETWi.Helper;

namespace YAETWi.Data
{
    public class Tracer
    {
        public string provider { get; set; }
        public Dictionary<int, string> eventMap { get; set; }
        public Dictionary<int, string> opcodeMap { get; set; }
        public ConcurrentQueue<int> events { get; set; }
        public ConcurrentQueue<int> opcodes { get; set; }

        public DateTime timestamp;
        public int pid;
        public string processName;

        public Tracer(string provider)
        {
            this.provider = provider;
            this.eventMap = new Dictionary<int, string>();
            this.opcodeMap = new Dictionary<int, string>();
            this.events = new ConcurrentQueue<int>();
            this.opcodes = new ConcurrentQueue<int>();
        }

        private string resolveEventMap()
        {
            string desc = "";
            StringBuilder builder = new StringBuilder();
            builder.Append("EventIDs:\n");
            foreach (int e in events)
            {
                builder.Append(String.Format("{0} -> {1}\n", e, eventMap.TryGetValue(e, out desc) ? desc : ""));
            }
            return builder.ToString();
        }

        private String resolveOpcodeMap()
        {
            string desc = "";
            StringBuilder builder = new StringBuilder();
            builder.Append("OpcodeIDs:\n");
            foreach (int o in opcodes)
            {
                builder.Append(String.Format("{0} -> {1}\n", o, opcodeMap.TryGetValue(o, out desc) ? desc : ""));
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("Provider: {0} <-> GUID: {1}\n", provider, ETW.providerMap?[provider] ?? ""));
            builder.Append(resolveEventMap());
            builder.Append(resolveOpcodeMap());
            return builder.ToString();
        }
    }
}
