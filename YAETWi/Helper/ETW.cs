using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YAETWi.Helper
{

    public static class ETW
    {

        public static Dictionary<int, Dictionary<string, object>> pidAggr = new Dictionary<int, Dictionary<string, object>>();
        private static List<string> kernelEvents;

        public static Dictionary<int, string> describeEvents(string provider)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            ProviderMetadata meta = new ProviderMetadata(provider);
            IEnumerable<EventMetadata> events = meta.Events;
            foreach (EventMetadata m in events)
            {
                dict[(int)m.Id] = m.Description;
            }
            return dict;
        }

        public static Dictionary<int, string> describeOpcodes(string provider)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            ProviderMetadata meta = new ProviderMetadata(provider);
            IList<System.Diagnostics.Eventing.Reader.EventOpcode> opcodes = meta.Opcodes;
            foreach (System.Diagnostics.Eventing.Reader.EventOpcode o in opcodes)
            {
                dict[o.Value] = o.DisplayName;
            }
            return dict;
        }

        public static void registryDataStream(RegistryTraceData data)
        {
            if (pidAggr.ContainsKey(data.ProcessID))
            {
                Logger.logKernel(Logger.KernelLogger.kernelRegistry.ToString(), data);
                Console.WriteLine(String.Format("\t\t\t: {0}\tregistry: {1}:{2}\n", data.ProcessID, data.KeyName, data.ValueName));
            }
        }

        public static void dumpKernelEvents()
        {
            if (kernelEvents != null)
            {
                Console.WriteLine(String.Format("Kernel Events (unique): \n[*] {0}\n", String.Join("\n[*] ", kernelEvents.Distinct().ToArray())));
            }
        }

        public static void traceKernel(TraceEventSession kernelSession, bool kernel)
        {
            kernelSession = new TraceEventSession("enhanced kernel session");
            kernelEvents = new List<string>();
            if (kernel)
            {
                Console.WriteLine("[*] starting enhanced kernel logging");
                kernelSession.EnableKernelProvider(KernelTraceEventParser.Keywords.All);
                kernelSession.Source.Kernel.All += ((TraceEvent data) =>
                {
                    if (pidAggr.ContainsKey(data.ProcessID))
                    {
                        kernelEvents.Add(data.EventName);
                    }
                });
                Task.Run(() => kernelSession.Source.Process());
            }
            else
            {
                kernelSession.Stop();
                kernelSession.Dispose();
            }
        }
        public static void traceKernelEvents(TraceEventSession kernelSession)
        {
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
        }
    }
}
