using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.IO;

using YAETWi.Core;
using System.Reflection;

namespace YAETWi.Data
{

    public class Tracer
    {
        public string provider { get; set; }
        public bool isTraced = false;
        public ConcurrentDictionary<int, ConcurrentQueue<Event>> pidToEvent { get; set; }
        public ConcurrentDictionary<int, ConcurrentQueue<Opcode>> pidToOpcode { get; set; }
        public Dictionary<int, string> eventMap { get; set; }
        public Dictionary<int, string> opcodeMap { get; set; }
        public Dictionary<int, string> templateMap { get; set; }


        public Tracer(string provider)
        {
            this.provider = provider;
            this.pidToEvent = new ConcurrentDictionary<int, ConcurrentQueue<Event>>();
            this.pidToOpcode = new ConcurrentDictionary<int, ConcurrentQueue<Opcode>>();
        }

        public Tracer() { }

        private string prepOutput(int pid)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("[*] Provider: {0} <-> GUID: {1}\n", provider, ETW.provider.providersAll?[provider] ?? ""));
            
            /* prepare extensive output */
            builder.Append("EventIDs: \n");
            foreach (Event e in pidToEvent[pid])
            {
                builder.Append(e.resolveEventMap(eventMap));
            }

            builder.Append("\nOpcodeIDs: \n");
            foreach (Opcode o in pidToOpcode[pid])
            {
                builder.Append(o.resolveOpcodeMap(opcodeMap));
            }

            /* prepare short output */
            Dictionary<string, List<int>> shortMapper = new Dictionary<string, List<int>>();
            builder.Append("\nEvent chains:\n");
            foreach (Event e in pidToEvent[pid])
            {
                if (!shortMapper.ContainsKey(e.timestamp.ToString()))
                {
                    shortMapper.Add(e.timestamp.ToString(), new List<int>());
                }
                List<int> arr = shortMapper[e.timestamp.ToString()];
                arr.Add(e.id);
                shortMapper[e.timestamp.ToString()] = arr;
            }
            foreach (KeyValuePair<string, List<int>> entry in shortMapper)
            {
                builder.Append(String.Format("\t[{0}]: {1}\n", entry.Key, String.Join("->", entry.Value)));
            }

            builder.Append("\nOpcode chains:\n");
            foreach (Opcode e in pidToOpcode[pid])
            {
                if (!shortMapper.ContainsKey(e.timestamp.ToString()))
                {
                    shortMapper.Add(e.timestamp.ToString(), new List<int>());
                }
                List<int> arr = shortMapper[e.timestamp.ToString()];
                arr.Add(e.id);
                shortMapper[e.timestamp.ToString()] = arr;
            }
            foreach (KeyValuePair<string, List<int>> entry in shortMapper)
            {
                builder.Append(String.Format("\t[{0}]: {1}\n", entry.Key, String.Join("->", entry.Value)));
            }
            return builder.ToString();
        }

        public void print(int pid)
        {
            Console.WriteLine(prepOutput(pid));
        }

        public void write(int pid, string provider, string directory)
        {
            string o = prepOutput(pid);
            if (String.IsNullOrEmpty(directory))
            {
                directory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            Console.WriteLine("will be written to " + directory);
            try
            {
                using (var f = File.AppendText(String.Format("{0}\\{1}.txt", directory, provider)))
                {
                    f.WriteLine(o);
                    f.Close();
                }
                Console.WriteLine("Dumped to " + provider + ".txt");
            }
            catch (Exception er)
            {
                Console.WriteLine(er);
            }
        }

    }
}
