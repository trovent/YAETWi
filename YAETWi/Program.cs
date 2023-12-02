using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

using YAETWi.Helper;
using YAETWi.Core;

namespace YAETWi
{
    class Program
    {
        private static Dictionary<string, string> parameters = new Dictionary<string, string>();
        public static HashSet<int> pids = new HashSet<int>();
        private static int extConn = 0;
        public static int events = 0;
        public static bool kernel = false;
        public static bool verbose = false;

        static void Main(string[] args)
        {
            parameters = Helper.ArgParser.parse(args);
            if (parameters.ContainsKey(ArgParser.Parameters.verbose.ToString()))
                verbose = true;
            if (parameters.ContainsKey(ArgParser.Parameters.kernel.ToString()))
                kernel = true;

            TraceEventSession tcpipKernelSession = null;
            if (parameters.ContainsKey(ArgParser.Parameters.externalIP.ToString()))
            {
                tcpipKernelSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName);
                tcpipKernelSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
                Logger.printInfo("Starting TCPIP Session");

                tcpipKernelSession.Source.Kernel.TcpIpAccept += ((TcpIpConnectTraceData data) =>
                {
                    Logger.logKernel(data);
                    Logger.printEvent(String.Format("conn: {0} -> :{1}\tproc: {2} -> {3}\n", data.daddr, data.sport, data.ProcessID, data.ProcessName));

                    if (data.daddr.ToString() == parameters[ArgParser.Parameters.externalIP.ToString()])
                    {
                        pids.Add(data.ProcessID);
                        extConn++;
                    }
                });
                Task.Run(() => tcpipKernelSession.Source.Process());

            } else if (parameters.ContainsKey(ArgParser.Parameters.pids.ToString()))
            {
                ArgParser.readPids(parameters[ArgParser.Parameters.pids.ToString()], pids);
            }
            else
            {
                Helper.Help.print();
                Environment.Exit(0);
            }

            TraceEventSession kernelSession = null;
            TraceEventSession allProvidersSession = null;
            if (kernel)
            {
                ETW.traceKernel(kernelSession);
            }
            else
            {
                ETW.traceAllProviders(allProvidersSession);
            }

            var start = DateTime.Now;

            while (true)
            {
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    var end = DateTime.Now;
                    Console.WriteLine(String.Format("\n[*] Session started: {0}\n[*] Session ended: {1}\n[*] Overall time: {2}\n[*] Overall external connections: {3}\n[*] # of events: {4}\n",
                        start,
                        end,
                        end - start,
                        extConn,
                        events));
                    try
                    {
                        tcpipKernelSession?.Dispose();
                        allProvidersSession?.Dispose();
                        kernelSession?.Dispose();
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                    }
                    Environment.Exit(0);
                };

                var cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.R:
                        {
                            Console.Write("Enter provider name:");
                            string p = Console.ReadLine();
                            foreach (int pid in pids)
                            {
                                Logger.dumpETWProvider(p, pid);
                            }
                            break;
                        }
                    case ConsoleKey.D:
                        {
                            Logger.printPids();
                            if (kernel)
                                Logger.dumpKernelEvents();
                            else
                                Logger.dumpETWProviders();
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
                    case ConsoleKey.C:
                        {
                            Program.pids = new HashSet<int>();
                            ETW.refreshCollection();
                            Logger.printInfo("Purged collections");
                        }
                        break;
                    case ConsoleKey.P:
                        {
                            Console.Write("Enter comma-separated list of pids to monitor:");
                            string input = Console.ReadLine();
                            ETW.refreshCollection();
                            ArgParser.readPids(input, pids);
                            break;
                        }
                    case ConsoleKey.H:
                        {
                            Helper.Help.keystrokes();
                            break;
                        }
                }
            }
        }
    }
}
