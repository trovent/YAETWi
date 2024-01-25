using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Microsoft.Diagnostics.Tracing;

namespace YAETWi.Data
{
    public class Event
    {
        public DateTime timestamp;
        public int id;
        public int pid;
        public string processName;
        public int len;
        public string payload;

        public Event(TraceEvent data)
        {
            this.timestamp = data.TimeStamp;
            this.id = (int)data.ID;
            this.pid = data.ProcessID;
            this.processName = Process.GetProcessById(data.ProcessID).ProcessName;
            this.len = data.EventData().Length;

            StringBuilder payloadBuilder = new StringBuilder();
            foreach (string p in data.PayloadNames)
            {
                payloadBuilder.Append(String.Format("{0}:{1} ;", p, data.PayloadByName(p)));
            }
            payload = payloadBuilder.ToString();
        }

        public string resolveEventMap(Dictionary<int, string> eventMap)
        {
            string desc = "";
            if (eventMap.TryGetValue(id, out desc)) 
            { }
            else 
            { 
                desc = ""; 
            }
            return String.Format("\t[{0}][{1} -> {2}] {3} -> {4}\n\t[{5}]\n", timestamp, pid, processName, id, desc, payload);
        }
    }
}
