using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Diagnostics.Tracing;

namespace YAETWi.Data
{
    public class Opcode
    {
        public DateTime timestamp;
        public int id;
        public int pid;
        public string processName;

        public Opcode(TraceEvent data) 
        {
            this.timestamp = data.TimeStamp;
            this.id = (int)data.Opcode;
            this.pid = data.ProcessID;
            this.processName = Process.GetProcessById(data.ProcessID).ProcessName;
        }
        public String resolveOpcodeMap(Dictionary<int, string> opcodeMap)
        {
            string desc = "";
            if (opcodeMap.TryGetValue(id, out desc))
            { }
            else
            {
                desc = "";
            }
            return String.Format("\t[{0}][{1} -> {2}] {3} -> {4}\n", timestamp, pid, processName, id, desc);
        }

    }
}
