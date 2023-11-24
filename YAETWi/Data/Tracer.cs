using Microsoft.Diagnostics.Tracing;
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
        public TraceEvent data { get; set; }

        public bool isTraced = false;

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

        public Tracer() { }

        private string resolveEventMap()
        {
            string desc = "";
            StringBuilder builder = new StringBuilder();
            builder.Append("EventIDs:\n");
            foreach (int e in events)
            {
                if (eventMap.TryGetValue(e, out desc))
                {
                    // ...
                }
                else
                {
                    desc = "";
                }
                builder.Append(String.Format("\t{0} -> {1}\n", e, desc));
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
                if (opcodeMap.TryGetValue(o, out desc))
                {
                    // ...
                }
                else
                {
                    desc = "";
                }
                builder.Append(String.Format("\t{0} -> {1}\n", o, desc));
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("[*] Provider: {0} <-> GUID: {1}\n", provider, ETW.provider.providersAll?[provider] ?? ""));
            builder.Append(resolveEventMap());
            builder.AppendLine();
            builder.Append(resolveOpcodeMap());
            return builder.ToString();
        }

        public void print()
        {
            Console.WriteLine(this.ToString());
        }
    }
}
