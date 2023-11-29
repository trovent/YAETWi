﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YAETWi.Data
{
    public class Event
    {
        public DateTime timestamp;
        public int id;
        public int pid;
        public string processName;

        public Event(DateTime timestamp, int id, int pid)
        {
            this.timestamp = timestamp;
            this.id = id;
            this.pid = pid;
            this.processName = Process.GetProcessById(pid).ProcessName;
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
            return String.Format("\t[{0}][{1} -> {2}] {3} -> {4}\n", timestamp, pid, processName, id, desc);
        }
    }
}
