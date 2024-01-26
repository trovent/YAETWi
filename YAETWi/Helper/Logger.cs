using Microsoft.Diagnostics.Tracing;
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using YAETWi.Core;
using System.Collections.Concurrent;

namespace YAETWi.Helper
{
    public class Logger
    {
        public static void printInfo(string text)
        {
            Console.WriteLine(String.Format("\n[*] {0}", text));
        }

        public static void printEvent(string text)
        {
            Console.WriteLine(String.Format("\n\t[!] {0}", text));
        }
        public static void printWarn(string text)
        {
            Console.WriteLine(String.Format("\n[!] {0}", text));
        }

        public static void printVerbose(string text)
        {
            Console.WriteLine(String.Format("[+] {0}", text));
        }
        public static void printNCFailure(string text)
        {
            Console.WriteLine(String.Format("[-] {0}", text));
        }

        public static void printSeparator()
        {
            Console.Write("====================================================");
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
            Console.WriteLine(String.Format(
                "\n{0}\tPID: {1}" +
                "\n\t[*] Stream: {2}" +
                "\n\t[*] Event: {3}" +
                "\n\t[*] Opcode: {4} -> {5}",
                data.TimeStamp,
                data.ProcessID,
                data.GetType(),
                data.EventName,
                (int)data.Opcode,
                data.OpcodeName
                ));
        }
        public static void printPids()
        {
            Console.WriteLine("\nPIDs: ");
            foreach (int pid in Program.pids)
            {
                Console.WriteLine(String.Format("{0} -> {1}", pid, Process.GetProcessById(pid).ProcessName));
            }
        }

        public static void dumpKernelEvents()
        {
            Console.WriteLine("\nKernel Events:");
            foreach (KeyValuePair<string, HashSet<string>> kvp in ETW.kProviderTimestampMap)
            {
                Console.WriteLine(String.Format("{0}\n\t{1}", kvp.Key, String.Join("\n\t", kvp.Value)));
            }
        }

        public static void dumpETWProvider(string p)
        {
            foreach (int pid in Program.pids)
            {
                try
                {
                    string guid = ETW.provider.providersAll[p];
                    Logger.printSeparatorStart();
                    ETW.providerTracerMap[guid].print(pid);
                    Logger.printSeparatorEnd();
                }
                catch (Exception e)
                {
                    Logger.printNCFailure(e.ToString());
                }
            }
        }

        public static void writeETWProvider(string provider, string directory)
        {
            foreach (int pid in Program.pids)
            {
                try
                {
                    string guid = ETW.provider.providersAll[provider];
                    ETW.providerTracerMap[guid].write(pid, provider, directory);
                }
                catch (Exception e)
                {
                    Logger.printNCFailure(e.ToString());
                }
            }
        }

        public static void dumpETWProviders()
        {
            Console.WriteLine("\nETW Providers: ");
            foreach (KeyValuePair<string, Data.Tracer> kvp in ETW.providerTracerMap)
            {
                /* dump timestamps of all triggered events for particular provider. Helps to make correlations with the testing events */
                HashSet<string> timestamps = new HashSet<string>();
                if (kvp.Value.isTraced)
                {
                    foreach (KeyValuePair<int, ConcurrentQueue<Data.Event>> ikvp in kvp.Value.pidToEvent)
                    {
                        foreach (Data.Event e in ikvp.Value)
                        {
                            timestamps.Add(String.Format("[{0}]", e.timestamp.ToString()));
                        }
                    }
                    /* dump output */
                    Console.WriteLine(String.Format("{0}: [{1}]\n\t{2}", kvp.Value.provider, String.Join(",", kvp.Value.pidToEvent.Keys), String.Join("\n\t", timestamps)));
                }
            }
        }
    }
}
