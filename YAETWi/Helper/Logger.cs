using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            eventId,
            opcodeId
        }

        private static string opcodeStringifier(List<int> list)
        {
            var count = list.Count();
            StringBuilder sb = new StringBuilder();
            foreach (int s in list)
            {
                count--;
                if (count != 0)
                {
                    sb.AppendLine(String.Format("OpcodeID: {0} ->", s));
                }
                else
                {
                    sb.AppendLine(String.Format("OpcodeID: {0}", s.ToString()));
                }
            }
            sb.AppendLine("**************************************************");
            return sb.ToString();
        }

        private static string eventStringifier(List<int> list)
        {
            var count = list.Count();
            StringBuilder sb = new StringBuilder();
            foreach (int s in list)
            {
                count--;
                if (count != 0)
                {
                    sb.AppendLine(String.Format("EventID: {0} ->", s));
                }
                else
                {
                    sb.AppendLine(String.Format("EventID: {0}", s.ToString()));
                }
            }
            sb.AppendLine("**************************************************");
            return sb.ToString();
        }

        private static string eventAggregator(int pid, Dictionary<int, Dictionary<string, object>> pidAggregator, Dictionary<int, string> eventDescriptor)
        {
            List<int> eventIds = (List<int>)pidAggregator[pid][Logger.Log.eventId.ToString()];
            StringBuilder sb = new StringBuilder();
            foreach (int id in eventIds)
            {
                if (eventDescriptor.ContainsKey(id))
                {
                    sb.AppendLine(String.Format("EventID: {0} -> {1}", id, eventDescriptor[id]));
                }
                else
                {
                    sb.AppendLine(String.Format("EventID: {0} -> ", id));
                }
            }
            sb.AppendLine("**************************************************");
            return sb.ToString();
        }

        private static string opcodeAggregator(int pid, Dictionary<int, Dictionary<string, object>> pidAggregator, Dictionary<int, string> opcodeDescriptor)
        {
            List<int> opcodeIds = (List<int>)pidAggregator[pid][Logger.Log.opcodeId.ToString()];
            StringBuilder sb = new StringBuilder();
            foreach (int id in opcodeIds)
            {
                if (opcodeDescriptor.ContainsKey(id))
                {
                    sb.AppendFormat("OpcodeID: {0} -> {1}\n", id, opcodeDescriptor[id]);
                }
                else
                {
                    sb.AppendFormat("OpcodeID: {0} -> \n", id);
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
            eventStringifier((List<int>)pidAggregator[pid][Logger.Log.eventId.ToString()]),
            opcodeStringifier((List<int>)pidAggregator[pid][Logger.Log.opcodeId.ToString()])
            ));
        }
        public static void ticker(int pid, Dictionary<int, Dictionary<string, object>> pidAggregator, Dictionary<int, string> eventDescriptor, Dictionary<int, string> opcodeDescriptor)
        {
            Console.WriteLine(String.Format("{0} pid: {1} -> {2}, providerId: {3}\n{4}\n{5}\n",
            pidAggregator[pid][Logger.Log.timestamp.ToString()],
            pid,
            pidAggregator[pid][Logger.Log.process.ToString()],
            ((Nullable<System.Guid>)pidAggregator[pid][Logger.Log.providerId.ToString()]).ToString() ?? "",
            eventAggregator(pid, pidAggregator, eventDescriptor),
            opcodeAggregator(pid, pidAggregator, opcodeDescriptor)
            ));
        }
    }
}
