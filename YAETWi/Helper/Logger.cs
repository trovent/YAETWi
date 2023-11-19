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
            EventIDs,
            OpcodeIDs
        }

        public enum KernelLogger
        {
            kernelFileIOCreate,
            kernelProcess,
            kernelImageLoad,
            kernelRegistry,
            kernelTcpIPAccept
        }

        public static void printSeparatorStart()
        {
            Console.Write("\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n");
        }

        public static void printSeparatorEnd()
        {
            Console.Write("\n<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
        }

        public static void logKernel(TraceEvent data)
        {
            Console.WriteLine(String.Format("\n{0}\tPID: {1}\n\t[*] Stream: {2}\n\t[*] Event: {3}\n\t[*] Opcode: {4} -> {5}", 
                data.TimeStamp, 
                data.ProcessID, 
                data.GetType(), 
                data.EventName,
                (int)data.Opcode,
                data.OpcodeName));
        }

        private static string listStringifier(List<int> list, String occurrence)
        {
            var count = list.Count();
            StringBuilder sb = new StringBuilder(String.Format("\n{0}: ", occurrence));
            foreach (int s in list)
            {
                count--;
                if (count != 0)
                {
                    sb.Append(String.Format(" {0} ->", s));
                }
                else
                {
                    sb.Append(String.Format(" {0}", s));
                }
            }
            return sb.ToString();
        }

        private static string listAggregator(int pid, 
            Dictionary<int, Dictionary<string, object>> pidAggregator,
            Dictionary<int, string> descriptor,
            String occurrence)
        {
            List<int> occurs;
            if (occurrence.Equals(Occurrence.EventIDs.ToString()))
            {
                occurs = (List<int>)pidAggregator[pid][Logger.Log.eventId.ToString()];
            }
            else if (occurrence.Equals(Occurrence.OpcodeIDs.ToString()))
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
            return sb.ToString();
        }

        public static void ticker(int pid, 
            Dictionary<int, Dictionary<string, object>> pidAggregator)
        {
            Nullable<System.Guid> guid = (Nullable<System.Guid>)pidAggregator?[pid][Logger.Log.providerId.ToString()];
            Console.WriteLine(String.Format("\n{0} pid: {1} -> {2}; providerId: <{3}>\n\n{4}\n{5}\n",
            pidAggregator[pid][Logger.Log.timestamp.ToString()],
            pid,
            pidAggregator[pid][Logger.Log.process.ToString()],
            guid?.ToString() ?? "should be specified via /provider=",
            listStringifier((List<int>)pidAggregator[pid][Logger.Log.eventId.ToString()],  Occurrence.EventIDs.ToString()),
            listStringifier((List<int>)pidAggregator[pid][Logger.Log.opcodeId.ToString()], Occurrence.OpcodeIDs.ToString())
            ));
        }
        public static void ticker(int pid, 
            Dictionary<int, Dictionary<string, object>> pidAggregator, 
            Dictionary<int, string> eventDescriptor, 
            Dictionary<int, string> opcodeDescriptor)
        {
            Nullable<System.Guid> guid = (Nullable<System.Guid>)pidAggregator?[pid][Logger.Log.providerId.ToString()];
            Console.WriteLine(String.Format("\n{0} pid: {1} -> {2}; providerId: <{3}>\n\n{4}\n{5}\n",
            pidAggregator[pid][Logger.Log.timestamp.ToString()],
            pid,
            pidAggregator[pid][Logger.Log.process.ToString()],
            guid?.ToString() ?? "should be specified via /provider=",
            listAggregator(pid, pidAggregator, eventDescriptor,  Occurrence.EventIDs.ToString()),
            listAggregator(pid, pidAggregator, opcodeDescriptor, Occurrence.OpcodeIDs.ToString())
            ));
        }
    }
}
