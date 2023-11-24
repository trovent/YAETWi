using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

using YAETWi.Helper;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Collections.Concurrent;

namespace YAETWi
{
    class Program
    {
        private static Dictionary<string, string> parameters = new Dictionary<string, string>();
        private static int extConn = 0;
        public static int pid = -1;
        public static bool verbose = false;

        static void Main(string[] args)
        {
            parameters = Helper.ArgParser.parse(args);
            if (parameters.ContainsKey(ArgParser.Parameters.verbose.ToString()))
                verbose = true;

            TraceEventSession tcpipKernelSession = null;
            if (parameters.ContainsKey(ArgParser.Parameters.externalIP.ToString()))
            {
                tcpipKernelSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName);
                tcpipKernelSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
                Console.WriteLine(String.Format("[*] starting tcpip session"));

                tcpipKernelSession.Source.Kernel.TcpIpAccept += ((TcpIpConnectTraceData data) =>
                {
                    Logger.logKernel(data);
                    Logger.printEvent(String.Format("conn: {0} -> :{1}\tproc: {2} -> {3}\n", data.daddr, data.sport, data.ProcessID, data.ProcessName));

                    if (data.daddr.ToString() == parameters[ArgParser.Parameters.externalIP.ToString()])
                    {
                        extConn++;
                        pid = data.ProcessID;
                    }
                });
                Task.Run(() => tcpipKernelSession.Source.Process());

            } else if (parameters.ContainsKey(ArgParser.Parameters.pid.ToString()))
            {
                pid = Convert.ToUInt16(parameters[ArgParser.Parameters.pid.ToString()]);
            }
            else
            {
                Helper.Help.usage();
                Environment.Exit(0);
            }

            TraceEventSession allProvidersSession = null;
            ETW.traceAllProviders(allProvidersSession);

            var start = DateTime.Now;

            while (true)
            {
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    var end = DateTime.Now;
                    Console.WriteLine(String.Format("\n[*] Session started: {0}\n[*] Session ended: {1}\n[*] Overall time: {2}\n[*] Overall external connections: {3}",
                        start,
                        end,
                        end - start,
                        extConn));
                    tcpipKernelSession?.Dispose();
                    allProvidersSession?.Dispose();
                    Environment.Exit(0);
                };

                var cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.R:
                        {
                            Console.Write("Entry provider name:");
                            string p = Console.ReadLine();
                            ETW.dumpETWProvider(p);
                            break;
                        }
                    case ConsoleKey.D:
                        {
                            ETW.dumpETWProviders();
                        }
                        break;
                    case ConsoleKey.V:
                        if (!verbose)
                        {
                            verbose = true;
                            Logger.printInfo("Enabled verbose mode");
                        }
                        else
                        {
                            verbose = false;
                            Logger.printInfo("Disabled verbose mode");
                        }
                        break;
                    case ConsoleKey.P:
                        {
                            ETW.refreshCollection();
                            Logger.printInfo("Purged collections");
                        }
                        break;
                }
            }
        }
    }
}
