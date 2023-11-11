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

        private enum Occurrence
        {
            EventID,
            OpcodeID
        }

        public enum KernelLogger
        {
            kernelFileIOCreate,
            kernelProcess,
            kernelImageLoad,
            kernelRegistry
        }

        public static void logKernel(string dataStream, Dictionary<int, Dictionary<string, object>> pidAggr, TraceEvent data)
        {
            Console.WriteLine(String.Format("{0}\tStream: {1}\tEvent: {2}\tOpcode: {3}", data.TimeStamp, dataStream, data.EventName, data.OpcodeName));
        }

        private static string listStringifier(List<int> list, String occurrence)
        {
            var count = list.Count();
            StringBuilder sb = new StringBuilder(String.Format("{0}: ", occurrence));
            foreach (int s in list)
            {
                count--;
                if (count != 0)
                {
                    sb.Append(String.Format(" {0} ->", s));
                }
                else
                {
                    sb.Append(String.Format(" {0}\n", s));
                }
            }
            sb.AppendLine("**************************************************");
            return sb.ToString();
        }

        private static string listAggregator(int pid, 
            Dictionary<int, Dictionary<string, object>> pidAggregator,
            Dictionary<int, string> descriptor,
            String occurrence)
        {
            List<int> occurs;
            if (occurrence.Equals(Occurrence.EventID.ToString()))
            {
                occurs = (List<int>)pidAggregator[pid][Logger.Log.eventId.ToString()];
            }
            else if (occurrence.Equals(Occurrence.OpcodeID.ToString()))
            {
                occurs = (List<int>)pidAggregator[pid][Logger.Log.opcodeId.ToString()];
            }
            else
            {
                occurs = new List<int>();
            }
            StringBuilder sb = new StringBuilder();
            foreach (int id in occurs)
            {
                if (descriptor.ContainsKey(id))
                {
                    sb.AppendLine(String.Format("{0}: {1} -> {2}", occurrence, id, descriptor[id]));
                }
                else
                {
                    sb.AppendLine(String.Format("{0}: {1} -> ", occurrence, id));
                }
            }
            sb.AppendLine("**************************************************");
            return sb.ToString();
        }

        public static void ticker(int pid, Dictionary<int, Dictionary<string, object>> pidAggregator)
        {
            Console.WriteLine(String.Format("{0} pid: {1} -> {2}, providerId: {3}\n{4}\n{5}\n",
            pidAggregator[pid][Logger.Log.timestamp.ToString()],
            pid,
            pidAggregator[pid][Logger.Log.process.ToString()],
            ((Nullable<System.Guid>)pidAggregator[pid][Logger.Log.providerId.ToString()]).ToString() ?? "",
            listStringifier((List<int>)pidAggregator[pid][Logger.Log.eventId.ToString()],  Occurrence.EventID.ToString()),
            listStringifier((List<int>)pidAggregator[pid][Logger.Log.opcodeId.ToString()], Occurrence.OpcodeID.ToString())
            ));
        }
        public static void ticker(int pid, 
            Dictionary<int, Dictionary<string, object>> pidAggregator, 
            Dictionary<int, string> eventDescriptor, 
            Dictionary<int, string> opcodeDescriptor)
        {
            Console.WriteLine(String.Format("{0} pid: {1} -> {2}, providerId: {3}\n{4}\n{5}\n",
            pidAggregator[pid][Logger.Log.timestamp.ToString()],
            pid,
            pidAggregator[pid][Logger.Log.process.ToString()],
            ((Nullable<System.Guid>)pidAggregator[pid][Logger.Log.providerId.ToString()]).ToString() ?? "",
            listAggregator(pid, pidAggregator, eventDescriptor,  Occurrence.EventID.ToString()),
            listAggregator(pid, pidAggregator, opcodeDescriptor, Occurrence.OpcodeID.ToString())
            ));
        }
    }
}
