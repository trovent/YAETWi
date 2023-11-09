using System;
using System.Collections.Generic;
using System.Collections;

using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

using YAETWi.Helper;
using System.Diagnostics.Eventing.Reader;

namespace YAETWi
{
    class Program
    {
        private static Dictionary<int, Dictionary<string, object>> pidAggr;
        private static int extConn = 0;
        static void Main(string[] args)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (args.Length != 2)
            {
                Helper.Help.usage();
                Environment.Exit(0);
            }
            else
            {
                parameters = Helper.ArgParser.parse(args);
            }

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

                if (data.daddr.ToString() == parameters["/externalIP"])
                {
                    extConn++;
                    pidAggr = new Dictionary<int, Dictionary<string, object>>();
                    /* clear on every event, otherwise check on condition (!pidAggregator.ContainsKey(data.ProcessID)) */
                    var dict = new Dictionary<string, object>();
                    dict[Logger.Log.timestamp.ToString()] = new Nullable<DateTime>();
                    dict[Logger.Log.process.ToString()] = data.ProcessName;
                    dict[Logger.Log.providerId.ToString()] = new Nullable<System.Guid>();
                    dict[Logger.Log.eventName.ToString()] = new List<string>();
                    dict[Logger.Log.opcodeName.ToString()] = new List<string>();
                    pidAggr.Add(data.ProcessID, dict);
                }
            });

            Task.Run(() => kernelSession.Source.Process());

            var impacketSession = new TraceEventSession("Impacket session");
            impacketSession.EnableProvider(parameters["/provider"]);
            ProviderMetadata meta = new ProviderMetadata(parameters["/provider"]);
            Console.WriteLine(String.Format("[*] starting custom session: {0}", meta.Name));

            impacketSession.Source.AllEvents += ((TraceEvent data) =>
            {
                /* check all events, otherwise check on condition: (data.ProviderGuid.ToString().Equals("1139c61b-b549-4251-8ed3-27250a1edec8")) */
                if (pidAggr.ContainsKey(data.ProcessID))
                {
                    Dictionary<string, object> dict = pidAggr[data.ProcessID];
                    dict[Logger.Log.timestamp.ToString()]   = data.TimeStamp;
                    dict[Logger.Log.providerId.ToString()]  = data.ProviderGuid;
                    ((List<string>)dict[Logger.Log.eventName.ToString()]).Add(data.EventName);
                    ((List<string>)dict[Logger.Log.opcodeName.ToString()]).Add(data.OpcodeName);
                }
            });

            Task.Run(() => impacketSession.Source.Process());

            var start = DateTime.Now;

            while (true)
            {  
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    var end = DateTime.Now;
                    Console.WriteLine(String.Format("[*] Session started: {0}\n[*] Session ended: {1}\n[*] Overall time: {2}\n[*] Overall external connections: {3}", 
                        start, 
                        end, 
                        end - start,
                        extConn));
                    kernelSession.Dispose();
                    impacketSession.Dispose();
                    Environment.Exit(0);
                };

                var cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.D)
                {
                    if (pidAggr != null)
                    {
                        foreach (int pid in pidAggr.Keys)
                        {
                            Logger.ticker(pid, pidAggr);
                        }
                    }
                }
            }

        }
    }
}
