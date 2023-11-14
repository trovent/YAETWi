﻿using System;
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
        private static Dictionary<string, string> parameters = new Dictionary<string, string>();
        private static int extConn = 0;
        private static bool verbose = false;
        private static bool kernel = false;

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
                if (!parameters.ContainsKey(ArgParser.Parameters.externalIP.ToString()))
                {
                    Helper.Help.usage();
                    Environment.Exit(0);
                }

                if (parameters.ContainsKey(ArgParser.Parameters.verbose.ToString()))
                    verbose = true;
                if (parameters.ContainsKey(ArgParser.Parameters.kernel.ToString()))
                    kernel = true;
            }

            Dictionary<int, string> eventDescriptor  = new Dictionary<int, string>();
            Dictionary<int, string> opcodeDescriptor = new Dictionary<int, string>();

            if (parameters.ContainsKey(ArgParser.Parameters.provider.ToString()))
            {
                eventDescriptor = ETW.describeEvents(parameters[ArgParser.Parameters.provider.ToString()]);
                opcodeDescriptor = ETW.describeOpcodes(parameters[ArgParser.Parameters.provider.ToString()]);
            }

            var tcpipKernelSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName);
            tcpipKernelSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
            Console.WriteLine(String.Format("[*] starting kernel session"));

            tcpipKernelSession.Source.Kernel.TcpIpAccept += ((TcpIpConnectTraceData data) =>
            {
                Logger.logKernel(Logger.KernelLogger.kernelTcpIPAccept.ToString(), data);
                Console.WriteLine(String.Format("\t\t\tconn: {0} -> :{1}\tproc: {2} -> {3}\n", data.daddr, data.sport, data.ProcessID, data.ProcessName));

                if (data.daddr.ToString() == parameters[ArgParser.Parameters.externalIP.ToString()])
                {
                    extConn++;
                    ETW.pidAggr = new Dictionary<int, Dictionary<string, object>>();
                    /* clear on every event, otherwise check on condition (!pidAggregator.ContainsKey(data.ProcessID)) */
                    var dict = new Dictionary<string, object>();
                    dict[Logger.Log.timestamp.ToString()] = new Nullable<DateTime>();
                    dict[Logger.Log.process.ToString()] = data.ProcessName;
                    dict[Logger.Log.providerId.ToString()] = new Nullable<System.Guid>();
                    dict[Logger.Log.eventId.ToString()] = new List<int>();
                    dict[Logger.Log.opcodeId.ToString()] = new List<int>();
                    ETW.pidAggr.Add(data.ProcessID, dict);
                }
            });
            Task.Run(() => tcpipKernelSession.Source.Process());

            TraceEventSession enhancedKernelSession = null;
            ETW.traceKernel(enhancedKernelSession, kernel);

            TraceEventSession customSession = null;

            if (parameters.ContainsKey(ArgParser.Parameters.provider.ToString()))
            {
                customSession = new TraceEventSession("custom session");
                customSession.EnableProvider(parameters[ArgParser.Parameters.provider.ToString()]);
                Console.WriteLine(String.Format("[*] starting custom session: {0}", 
                    new ProviderMetadata(parameters[ArgParser.Parameters.provider.ToString()]).Name));

                customSession.Source.AllEvents += ((TraceEvent data) =>
                {
                    /* check all events, otherwise check on condition: (data.ProviderGuid.ToString().Equals("1139c61b-b549-4251-8ed3-27250a1edec8")) */
                    if (ETW.pidAggr.ContainsKey(data.ProcessID))
                    {
                        Dictionary<string, object> dict = ETW.pidAggr[data.ProcessID];
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
                    tcpipKernelSession.Dispose();
                    if (customSession != null)
                        customSession.Dispose();
                    if (enhancedKernelSession != null)
                        enhancedKernelSession.Dispose();
                    Environment.Exit(0);
                };

                var cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.D:
                        if (parameters.ContainsKey(ArgParser.Parameters.provider.ToString()))
                        {
                            if (ETW.pidAggr != null)
                            {
                                foreach (int pid in ETW.pidAggr.Keys)
                                {
                                    if (!verbose)
                                    {
                                        Logger.ticker(pid, ETW.pidAggr);
                                    }
                                    else
                                    {
                                            Logger.ticker(pid, 
                                                ETW.pidAggr, 
                                                eventDescriptor, 
                                                opcodeDescriptor);
                                    }
                                    if (kernel)
                                    {
                                        ETW.dumpKernelEvents();
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
                    case ConsoleKey.K:
                        if (!kernel)
                        {
                            kernel = true;
                            Console.WriteLine("[*] Enabled enhanced kernel logging");
                            ETW.traceKernel(enhancedKernelSession, kernel);
                        }
                        else
                        {
                            kernel = false;
                            Console.WriteLine("[*] Disabled enhanced kernel logging");
                            ETW.traceKernel(enhancedKernelSession, kernel);
                        }
                        break;
                }
            }
        }
    }
}
