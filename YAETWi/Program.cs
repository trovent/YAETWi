using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

using YAETWi.Helper;

namespace YAETWi
{
    class Program
    {

        private static Dictionary<int, Dictionary<string, object>> pidAggregator = new Dictionary<int, Dictionary<string, object>>();
        
        static void Main(string[] args)
        {
            var kernelSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName);

            kernelSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP    |
                                                KernelTraceEventParser.Keywords.ImageLoad      |
                                                KernelTraceEventParser.Keywords.Handle         |
                                                KernelTraceEventParser.Keywords.Process        |
                                                KernelTraceEventParser.Keywords.Registry);

            Console.WriteLine(String.Format("[*] starting kernel session"));

            kernelSession.Source.Kernel.TcpIpAccept += ((TcpIpConnectTraceData data) =>
            {
                string dataStream = "tcpAcceptConnect";
                Console.WriteLine(String.Format("{0}\tStream: {1}\tEvent: {2}\tOpcode: {3}", data.TimeStamp, dataStream, data.EventName, data.OpcodeName));
                Console.WriteLine(String.Format("\t\t\tconn: {0} -> :{1}\tproc: {2} -> {3}", data.daddr, data.sport, data.ProcessID, data.ProcessName));


                if (data.daddr.ToString() == "192.168.178.20")
                {
                    if (!pidAggregator.ContainsKey(data.ProcessID))
                    {
                        var dict = new Dictionary<string, object>();
                        dict[Logger.Log.timestamp.ToString()]   = new Nullable<DateTime>();
                        dict[Logger.Log.process.ToString()]     = data.ProcessName;
                        dict[Logger.Log.providerId.ToString()]  = new Nullable<System.Guid>();
                        dict[Logger.Log.eventIndex.ToString()]  = null;
                        dict[Logger.Log.eventName.ToString()]   = null;
                        dict[Logger.Log.opcodeId.ToString()]    = null;
                        dict[Logger.Log.opcodeName.ToString()]  = null;
                        pidAggregator.Add(data.ProcessID, dict);
                    }
                }
            });

            Task.Run(() => kernelSession.Source.Process());

            var impacketSession = new TraceEventSession("Impacket session");
            Console.WriteLine("[*] starting impacket session");
            impacketSession.EnableProvider("Microsoft-Windows-RemoteDesktopServices-RdpCoreTS");

            impacketSession.Source.AllEvents += ((TraceEvent data) =>
            {
                if (data.ProviderGuid.ToString().Equals("1139c61b-b549-4251-8ed3-27250a1edec8"))
                {
                    if (pidAggregator.ContainsKey(data.ProcessID))
                    {
                        Dictionary<string, object> dict = pidAggregator[data.ProcessID];
                        dict[Logger.Log.timestamp.ToString()]   = data.TimeStamp;
                        dict[Logger.Log.providerId.ToString()]  = data.ProviderGuid;
                        dict[Logger.Log.eventIndex.ToString()]  = data.EventIndex;
                        dict[Logger.Log.eventName.ToString()]   = data.EventName;
                        dict[Logger.Log.opcodeId.ToString()]    = data.Opcode;
                        dict[Logger.Log.opcodeName.ToString()]  = data.OpcodeName;
                    }
                }
            });

            Task.Run(() => impacketSession.Source.Process());

            var start = DateTime.Now;

            while (true)
            {  
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    var end = DateTime.Now;
                    Console.WriteLine(String.Format("[*] Session started: {0}\n[*] Session ended: {1}\n[*] Overall time: {2}", start, end, end - start));
                    kernelSession.Dispose();
                    impacketSession.Dispose();
                    Environment.Exit(0);
                };

                var cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.D)
                {
                    foreach (int pid in pidAggregator.Keys)
                    {
                        Logger.ticker(pid, pidAggregator);
                    }
                }
            }

        }
    }
}
