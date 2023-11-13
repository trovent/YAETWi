using System;
using System.Collections.Generic;

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
        private static Dictionary<int, Dictionary<string, object>> pidAggr = new Dictionary<int, Dictionary<string, object>>();
        private static Dictionary<string, string> parameters = new Dictionary<string, string>();
        private static int extConn = 0;
        private static bool verbose = false;
        private static string externalIP;
        private static string provider = null;
        private static void registryDataStream(RegistryTraceData data)
        {
            if (pidAggr.ContainsKey(data.ProcessID))
            {
                Logger.logKernel(Logger.KernelLogger.kernelRegistry.ToString(), data);
                Console.WriteLine(String.Format("\t\t\tregistry: {0}:{1}\n", data.KeyName, data.ValueName));
            }
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Helper.Help.usage();
                Environment.Exit(0);
            }
            else
            {
                parameters = Helper.ArgParser.parse(args);
                if (!parameters.ContainsKey("/externalIP"))
                {
                    Helper.Help.usage();
                    Environment.Exit(0);
                }
                else
                    externalIP = parameters["/externalIP"];

                if (parameters.ContainsKey("/verbose"))
                    verbose = Convert.ToBoolean(parameters?["/verbose"]);
                if (parameters.ContainsKey("/provider"))
                    provider = parameters["/provider"];
            }

            Dictionary<int, string> eventDescriptor  = new Dictionary<int, string>();
            Dictionary<int, string> opcodeDescriptor = new Dictionary<int, string>();

            if (provider != null)
            {
                eventDescriptor = MetaProvider.describeEvents(provider);
                opcodeDescriptor = MetaProvider.describeOpcodes(provider);
            }

            var kernelSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName);

            kernelSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP    |
                                                KernelTraceEventParser.Keywords.ImageLoad      |
                                                KernelTraceEventParser.Keywords.Process        |
                                                KernelTraceEventParser.Keywords.Registry       |
                                                KernelTraceEventParser.Keywords.FileIO);

            Console.WriteLine(String.Format("[*] starting kernel session"));

            kernelSession.Source.Kernel.RegistrySetValue += registryDataStream;

            kernelSession.Source.Kernel.FileIOCreate += ((FileIOCreateTraceData data) =>
            {
                if (pidAggr.ContainsKey(data.ProcessID))
                {
                    Logger.logKernel(Logger.KernelLogger.kernelFileIOCreate.ToString(), data);
                    Console.WriteLine(String.Format("\t\t\tfile: {0}\n", data.FileName));
                }
            });

            kernelSession.Source.Kernel.ProcessStart += ((ProcessTraceData data) =>
            {
                if (pidAggr.ContainsKey(data.ProcessID))
                {
                    Logger.logKernel(Logger.KernelLogger.kernelFileIOCreate.ToString(), data);
                    Console.WriteLine(String.Format("\t\t\timageFile: {0}\tkernelImageFile: {1}\n", data.ImageFileName, data.KernelImageFileName));
                }
            });

            kernelSession.Source.Kernel.ImageLoad += ((ImageLoadTraceData data) =>
            {
                if (pidAggr.ContainsKey(data.ProcessID))
                {
                    Logger.logKernel(Logger.KernelLogger.kernelImageLoad.ToString(), data);
                    Console.WriteLine(String.Format("\t\t\tdll: {0}\n", data.FileName));
                }
            });

            kernelSession.Source.Kernel.TcpIpAccept += ((TcpIpConnectTraceData data) =>
            {
                Logger.logKernel(Logger.KernelLogger.kernelTcpIPAccept.ToString(), data);
                Console.WriteLine(String.Format("\t\t\tconn: {0} -> :{1}\tproc: {2} -> {3}\n", data.daddr, data.sport, data.ProcessID, data.ProcessName));

                if (data.daddr.ToString() == externalIP)
                {
                    extConn++;
                    pidAggr = new Dictionary<int, Dictionary<string, object>>();
                    /* clear on every event, otherwise check on condition (!pidAggregator.ContainsKey(data.ProcessID)) */
                    var dict = new Dictionary<string, object>();
                    dict[Logger.Log.timestamp.ToString()] = new Nullable<DateTime>();
                    dict[Logger.Log.process.ToString()] = data.ProcessName;
                    dict[Logger.Log.providerId.ToString()] = new Nullable<System.Guid>();
                    dict[Logger.Log.eventId.ToString()] = new List<int>();
                    dict[Logger.Log.opcodeId.ToString()] = new List<int>();
                    pidAggr.Add(data.ProcessID, dict);
                }
            });

            Task.Run(() => kernelSession.Source.Process());

            TraceEventSession customSession = null;

            if (provider != null)
            {
                customSession = new TraceEventSession("custom session");
                customSession.EnableProvider(provider);
                Console.WriteLine(String.Format("[*] starting custom session: {0}", new ProviderMetadata(provider).Name));

                customSession.Source.AllEvents += ((TraceEvent data) =>
                {
                    /* check all events, otherwise check on condition: (data.ProviderGuid.ToString().Equals("1139c61b-b549-4251-8ed3-27250a1edec8")) */
                    if (pidAggr.ContainsKey(data.ProcessID))
                    {
                        Dictionary<string, object> dict = pidAggr[data.ProcessID];
                        dict[Logger.Log.timestamp.ToString()]   = data.TimeStamp;
                        dict[Logger.Log.providerId.ToString()]  = data.ProviderGuid;
                        ((List<int>)dict[Logger.Log.eventId.ToString()]).Add(UInt16.Parse(data.EventName.Split('(', ')')[1]));
                        ((List<int>)dict[Logger.Log.opcodeId.ToString()]).Add((int)data.Opcode);
                    }
                });

                Task.Run(() => customSession.Source.Process());
            }

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
                    if (customSession != null)
                        customSession.Dispose();
                    Environment.Exit(0);
                };

                var cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.D:
                        if (provider != null)
                        {
                            if (pidAggr != null)
                            {
                                foreach (int pid in pidAggr.Keys)
                                {
                                    if (!verbose)
                                    {
                                        Logger.ticker(pid, pidAggr);
                                    }
                                    else
                                    {
                                            Logger.ticker(pid, 
                                                pidAggr, 
                                                eventDescriptor, 
                                                opcodeDescriptor);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("[!] Dump is working only for custom sessions by providing a provider argument");
                        }
                        break;
                    case ConsoleKey.V:
                        if (!verbose)
                        {
                            verbose = true;
                            Console.WriteLine("[*] Enabled verbose mode");
                        }
                        else
                        {
                            verbose = false;
                            Console.WriteLine("[*] Disabled verbose mode");
                        }
                        break;
                }

            }
        }
    }
}
