using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAETWi.Helper
{
    public class Logger
    {
        public enum Log
        {
            timestamp,
            process,
            providerId,
            eventName,
            opcodeName
        }

        private static string listStringifier(List<string> list)
        {
            var count = list.Count();
            StringBuilder sb = new StringBuilder();
            foreach(string s in list)
            {
                count--;
                if (count != 0)
                {
                    sb.AppendLine(String.Format("{0} ->", s));
                } 
                else
                {
                    sb.AppendLine(s);
                }
            }
            sb.AppendLine("*************************");
            return sb.ToString();
        }

        public static void ticker(int pid, Dictionary<int, Dictionary<string, object>> pidAggregator)
        {
            Console.WriteLine(String.Format("{0} pid: {1} -> {2}, providerId: {3}\n{4}\n{5}\n",
            pidAggregator[pid][Logger.Log.timestamp.ToString()],
            pid,
            pidAggregator[pid][Logger.Log.process.ToString()],
            ((Nullable<System.Guid>)pidAggregator[pid][Logger.Log.providerId.ToString()]).ToString() ?? "",
            listStringifier((List<string>)pidAggregator[pid][Logger.Log.eventName.ToString()]),
            listStringifier((List<string>)pidAggregator[pid][Logger.Log.opcodeName.ToString()])
            ));
        }
    }
}
