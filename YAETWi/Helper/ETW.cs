using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YAETWi.Helper
{

    public static class ETW
    { 
        public static ConcurrentDictionary<int, ConcurrentDictionary<string, object>> pidAggr = new ConcurrentDictionary<int, ConcurrentDictionary<string, object>>();
        private static ConcurrentDictionary<int, ConcurrentBag<string>> kernelEvents = new ConcurrentDictionary<int, ConcurrentBag<string>>();
        private static ConcurrentDictionary<int, ConcurrentBag<string>> etwProviders = new ConcurrentDictionary<int, ConcurrentBag<string>>();
        private static ConcurrentDictionary<string, string> providersMap = new ConcurrentDictionary<string, string>();

        public static void traceAllProviders(TraceEventSession session)
        {
            session = new TraceEventSession("enhanced ETW session");
            Logger.printInfo("starting enhanced ETW session");
            ProviderMetadata meta;
            
            foreach (var provider in EventLogSession.GlobalSession.GetProviderNames())
            {
                try
                { 
                    meta = new ProviderMetadata(provider);
                    if (!meta.Id.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                    {
                        session.EnableProvider(provider);
                        providersMap.TryAdd(meta.Id.ToString(), provider);
                        if (Program.verbose)
                            Logger.printVerbose(String.Format("added {0}:{1}", 
                                meta.Id.ToString(), provider));
                    }
                }
                catch (Exception)
                {
                    Logger.printNCFailure(String.Format("cannot activate {0}", provider));
                }
            }
            Logger.printInfo("Enabled ETW providers");

            session.Source.AllEvents += ((TraceEvent data) =>
            {
                if (pidAggr.ContainsKey(data.ProcessID))
                {
                    if (!etwProviders.ContainsKey(data.ProcessID))
                    {
                        etwProviders[data.ProcessID] = new ConcurrentBag<string>();
                    }
                    etwProviders[data.ProcessID].Add(data.ProviderGuid.ToString());
                    if (Program.verbose)
                    {
                        traceETWProvider(data);
                    }
                }
            });
            Task.Run(() => session.Source.Process());
        }

        private static void traceETWProvider(TraceEvent data)
        {
            Logger.printVerbose(String.Format("{0}:{1}", data.ProcessID, data.ProviderName));
        }

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

        public static void dumpETWProviders(int pid)
        {
            if (etwProviders.ContainsKey(pid))
            {
                if (etwProviders[pid].Count() != 0)
                {
                    Console.WriteLine("ETW Providers (unique): ");
                    foreach (string provider in etwProviders[pid].Distinct())
                    {
                        Console.WriteLine(String.Format(
                            "[*] {0} -> {1}", provider, providersMap[provider]));
                    }
                }
            }
        }

        public static void dumpKernelEvents(int pid)
        {
            if (kernelEvents.ContainsKey(pid))
            {
                if (kernelEvents[pid].Count() != 0)
                {
                    Console.WriteLine(String.Format("Kernel Events (unique): " +
                        "\n[*] {0}\n", String.Join("\n[*] ",
                        kernelEvents[pid].Distinct())));
                }
            }
        }

        public static void traceKernel(TraceEventSession kernelSession)
        {
            kernelSession = new TraceEventSession("enhanced kernel session");
            if (Program.kernel)
            {
                Logger.printInfo("starting enhanced kernel logging");
                kernelSession.EnableKernelProvider(KernelTraceEventParser.Keywords.All);
                kernelSession.Source.Kernel.All += ((TraceEvent data) =>
                {
                    if (pidAggr.ContainsKey(data.ProcessID))
                    {
                        if (!kernelEvents.ContainsKey(data.ProcessID))
                        {
                            kernelEvents[data.ProcessID] = new ConcurrentBag<string>();
                        }
                        kernelEvents[data.ProcessID].Add(data.EventName);
                        if (Program.verbose)
                        { 
                            traceKernelEvents(data);
                        }
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
        private static void traceKernelEvents(TraceEvent data)
        {
            if (data is ALPCReceiveMessageTraceData)
            {
                Logger.logKernel(data);
            } 
            else if (data is ALPCSendMessageTraceData)
            {
                Logger.logKernel(data);
            } 
            else if (data is ALPCUnwaitTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ALPCWaitForNewMessageTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ALPCWaitForReplyTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is BuildInfoTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is CSwitchTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DequeueTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DiskIOFlushBuffersTraceData)
            {
                Logger.logKernel(data);
            } 
            else if (data is DiskIOInitTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DiskIOTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DispatcherReadyThreadTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DPCTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DriverCompleteRequestReturnTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DriverCompleteRequestTraceData)
            {
                Logger.logKernel(data);
            } 
            else if (data is DriverCompletionRoutineTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DriverMajorFunctionCallTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is DriverMajorFunctionReturnTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is EmptyTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is EnqueueTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is EventTraceHeaderTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOCreateTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is FileIODirEnumTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOInfoTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is FileIONameTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOOpEndTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOReadWriteTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOSimpleOpTraceData)
            {
                Logger.logKernel(data);
            } 
            else if (data is HeaderExtensionTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ImageLoadTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ISRTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is LastBranchRecordTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MapFileTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemInfoTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHardFaultTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHeapRangeCreateTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHeapRangeDestroyTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHeapRangeRundownTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHeapRangeTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryImageLoadBackedTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryPageAccessTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryPageFaultTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryProcessMemInfoTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is MemorySystemMemInfoTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ObjectDuplicateHandleTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ObjectHandleTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ObjectNameTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ObjectTypeNameTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is PMCCounterProfTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ProcessCtrTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ProcessTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is RegistryTraceData)
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\t: {0}\tregistry: {1}:{2}\n", data.ProcessID, ((RegistryTraceData)data).KeyName, ((RegistryTraceData)data).ValueName));
            }
            else if (data is SampledProfileIntervalTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SampledProfileTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SplitIoInfoTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is StackWalkDefTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is StackWalkRefTraceData)
            {
                Logger.logKernel(data);
            } 
            else if (data is StackWalkStackTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is StringTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SysCallEnterTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SysCallExitTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigCPUTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigIDEChannelTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigIRQTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigLogDiskTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigNetworkTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigNICTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigPhyDiskTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigPnPTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigPowerTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigServicesTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigVideoTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is SystemPathsTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpConnectTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpFailTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpSendTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpV6ConnectTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpV6SendTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpV6TraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ThreadSetNameTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is ThreadTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is UdpIpFailTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is UdpIpTraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is UpdIpV6TraceData)
            {
                Logger.logKernel(data);
            }
            else if (data is VirtualAllocTraceData )
            {
                Logger.logKernel(data);
            }
            else if (data is VolumeMappingTraceData)
            {
                Logger.logKernel(data);
            }
            else
            {
                Logger.printNCFailure(String.Format("Unknown TraceData type: {0}", data.GetType()));
            }
        }
    }
}
