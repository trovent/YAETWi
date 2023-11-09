using System;
using System.Collections.Generic;
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
            eventIndex,
            eventName,
            opcodeId,
            opcodeName
        }
        public static void ticker(int pid, Dictionary<int, Dictionary<string, object>> pidAggregator)
        {
            Console.WriteLine(String.Format("{0} pid: {1} -> {2}, providerId: {3}\neventId: {4} -> {5}\nopcodeId: {6} -> {7}",
            pidAggregator[pid][Logger.Log.timestamp.ToString()],
            pid,
            pidAggregator[pid][Logger.Log.process.ToString()],
            ((Nullable<System.Guid>)pidAggregator[pid][Logger.Log.providerId.ToString()]).ToString() ?? "",
            pidAggregator[pid][Logger.Log.eventIndex.ToString()],
            pidAggregator[pid][Logger.Log.eventName.ToString()],
            pidAggregator[pid][Logger.Log.opcodeId.ToString()],
            pidAggregator[pid][Logger.Log.opcodeName.ToString()]
            ));
        }
    }
}
