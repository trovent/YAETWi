using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using YAETWi.Data;
using YAETWi.Helper;

namespace YAETWi.Core
{

    public static class ETW
    {
        public static ConcurrentDictionary<string, Data.Tracer> providerTracerMap = new ConcurrentDictionary<string, Data.Tracer>();
        public static ConcurrentDictionary<string, HashSet<string>> kProviderTimestampMap = new ConcurrentDictionary<string, HashSet<string>>();
        public static Provider provider = new Provider();

        public static void refreshCollection()
        {
            Logger.printInfo("successfully refreshed pids");
            Logger.printInfo("refreshing collection...");
            if (Program.kernel)
            {
                kProviderTimestampMap.Clear();
            }
            else
            {
                foreach (string p in provider.providersByName.Keys)
                {
                    string guid = provider.providersAll[p];
                    try
                    {
                        Data.Tracer t;
                        if (providerTracerMap.ContainsKey(guid))
                        {
                            providerTracerMap.TryRemove(guid, out t);
                        }
                        else
                        {
                            t = new Data.Tracer(p);
                        }
                        t.provider = p;
                        t.pidToEvent = new ConcurrentDictionary<int, ConcurrentQueue<Event>>();
                        t.pidToOpcode = new ConcurrentDictionary<int, ConcurrentQueue<Opcode>>();
                        t.eventMap = describeEvents(p);
                        t.opcodeMap = describeOpcodes(p);
                        t.templateMap = describeTemplates(p);
                        t.isTraced = false;
                        providerTracerMap.TryAdd(guid, t);

                    }
                    catch (Exception){}
                }
            }
            Logger.printInfo("successfully refreshed the collection");
        }

        public static void traceKernel(TraceEventSession session)
        {
            session = new TraceEventSession("kernel session");
            Logger.printInfo("starting kernel session");
            Logger.printWarn("see source code to specify the messages on particular Kernel providers -> no support for formatted messages for the Kernel events is provided by the Microsoft.Diagnostics.Tracing.Parsers.Kernel");
            session.EnableKernelProvider(KernelTraceEventParser.Keywords.All);

            session.Source.Kernel.All += ((TraceEvent data) =>
            {
                if (Program.pids.Contains(data.ProcessID))
                {
                    Program.events++;
                    if (!kProviderTimestampMap.ContainsKey(data.GetType().Name))
                    {
                        kProviderTimestampMap.TryAdd(data.GetType().Name, new HashSet<string>());
                    }
                    HashSet<string> timestamps = kProviderTimestampMap[data.GetType().Name];
                    timestamps.Add(String.Format("[{0}]", data.TimeStamp.ToString()));
                    kProviderTimestampMap[data.GetType().Name] = timestamps;

                    parseKernelEvents(data);
                }
            });

            Task.Run(() => session.Source.Process());
        }

        public static void traceAllProviders(TraceEventSession session)
        {
            session = new TraceEventSession("enhanced ETW session");
            Logger.printInfo("starting enhanced ETW session");
            foreach(string p in provider.providersByName.Keys)
            {
                try
                {
                    session.EnableProvider(p);
                    Logger.printVerbose(String.Format("activated provider: {0}", p));
                }
                catch (Exception)
                {
                    Logger.printNCFailure(String.Format("cannot activate {0}", p));
                }
            }

            refreshCollection();

            Logger.printInfo("activated all ETW providers");

            session.Source.Dynamic.All += ((TraceEvent data) =>
            {
                if (Program.pids.Contains(data.ProcessID))
                {
                    if (!data.ProviderGuid.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                    {
                        try
                        {
                            Program.events++;

                            Data.Tracer t = providerTracerMap[data.ProviderGuid.ToString()];
                            ConcurrentQueue<Data.Event> events;
                            ConcurrentQueue<Data.Opcode> opcodes;
                            if (t.pidToEvent.TryGetValue(data.ProcessID, out events))
                            { }
                            else
                            {
                                events = new ConcurrentQueue<Event>();
                            }
                            if (t.pidToOpcode.TryGetValue(data.ProcessID, out opcodes))
                            { }
                            else
                            {
                                opcodes = new ConcurrentQueue<Opcode>();
                            }
                            events.Enqueue(new Data.Event(data));
                            opcodes.Enqueue(new Data.Opcode(data));
                            t.isTraced = true;
                            t.pidToEvent[data.ProcessID] = events;
                            t.pidToOpcode[data.ProcessID] = opcodes;
                        }
                        catch (Exception) {}
                    }
                }
            });

            Task.Run(() => session.Source.Process());
        }

        private static void traceETWProvider(TraceEvent data)
        {
            Logger.printVerbose(String.Format("{0}:{1}", data.ProcessID, data.ProviderName));
        }

        public static Dictionary<int, string> describeTemplates(string provider)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            ProviderMetadata meta = new ProviderMetadata(provider);
            foreach (EventMetadata m in meta.Events)
            {
                dict[(int)m.Id] = m.Template;
            }
            return dict;
        }

        public static Dictionary<int, string> describeEvents(string provider)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            ProviderMetadata meta = new ProviderMetadata(provider);
            foreach (EventMetadata m in meta.Events)
            {
                dict[(int)m.Id] = m.Description;
            }
            return dict;
        }

        public static Dictionary<int, string> describeOpcodes(string provider)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            ProviderMetadata meta = new ProviderMetadata(provider);
            foreach (EventOpcode o in meta.Opcodes)
            {
                dict[o.Value] = o.DisplayName;
            }
            return dict;
        }

        private static void parseKernelEvents(TraceEvent data)
        {
            string kProvider = Program.kProvider;

            if (data is ALPCReceiveMessageTraceData && kProvider.Equals("ALPCReceiveMessageTraceData"))
            {
                Logger.logKernel(data);
            } 
            else if (data is ALPCSendMessageTraceData && kProvider.Equals("ALPCSendMessageTraceData"))
            {
                Logger.logKernel(data);
            } 
            else if (data is ALPCUnwaitTraceData && kProvider.Equals("ALPCUnwaitTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\tsource: {0}\n", ((ALPCUnwaitTraceData)data).Source));
            }
            else if (data is ALPCWaitForNewMessageTraceData && kProvider.Equals("ALPCWaitForNewMessageTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\tportName: {0}",((ALPCWaitForNewMessageTraceData) data).PortName));
            }
            else if (data is ALPCWaitForReplyTraceData && kProvider.Equals("ALPCWaitForReplyTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is BuildInfoTraceData && kProvider.Equals("BuildInfoTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is CSwitchTraceData && kProvider.Equals("CSwitchTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DequeueTraceData && kProvider.Equals("DequeueTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DiskIOFlushBuffersTraceData && kProvider.Equals("DiskIOFlushBuffersTraceData"))
            {
                Logger.logKernel(data);
            } 
            else if (data is DiskIOInitTraceData && kProvider.Equals("DiskIOInitTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DiskIOTraceData && kProvider.Equals("DiskIOTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DispatcherReadyThreadTraceData && kProvider.Equals("DispatcherReadyThreadTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DPCTraceData && kProvider.Equals("DPCTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DriverCompleteRequestReturnTraceData && kProvider.Equals("DriverCompleteRequestReturnTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DriverCompleteRequestTraceData && kProvider.Equals("DriverCompleteRequestTraceData"))
            {
                Logger.logKernel(data);
            } 
            else if (data is DriverCompletionRoutineTraceData && kProvider.Equals("DriverCompletionRoutineTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DriverMajorFunctionCallTraceData && kProvider.Equals("DriverMajorFunctionCallTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is DriverMajorFunctionReturnTraceData && kProvider.Equals("DriverMajorFunctionReturnTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is EmptyTraceData && kProvider.Equals("EmptyTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is EnqueueTraceData && kProvider.Equals("EnqueueTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is EventTraceHeaderTraceData && kProvider.Equals("EventTraceHeaderTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOCreateTraceData && kProvider.Equals("FileIOCreateTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is FileIODirEnumTraceData && kProvider.Equals("FileIODirEnumTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOInfoTraceData && kProvider.Equals("FileIOInfoTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is FileIONameTraceData && kProvider.Equals("FileIONameTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOOpEndTraceData && kProvider.Equals("FileIOOpEndTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOReadWriteTraceData && kProvider.Equals("FileIOReadWriteTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is FileIOSimpleOpTraceData && kProvider.Equals("FileIOSimpleOpTraceData"))
            {
                Logger.logKernel(data);
            } 
            else if (data is HeaderExtensionTraceData && kProvider.Equals("HeaderExtensionTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ImageLoadTraceData && kProvider.Equals("ImageLoadTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("FileName: {0}",((ImageLoadTraceData)data).FileName));
            }
            else if (data is ISRTraceData && kProvider.Equals("ISRTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is LastBranchRecordTraceData && kProvider.Equals("LastBranchRecordTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MapFileTraceData && kProvider.Equals("MapFileTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemInfoTraceData && kProvider.Equals("MemInfoTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHardFaultTraceData && kProvider.Equals("MemoryHardFaultTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHeapRangeCreateTraceData && kProvider.Equals("MemoryHeapRangeCreateTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHeapRangeDestroyTraceData && kProvider.Equals("MemoryHeapRangeDestroyTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHeapRangeRundownTraceData && kProvider.Equals("MemoryHeapRangeRundownTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryHeapRangeTraceData && kProvider.Equals("MemoryHeapRangeTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryImageLoadBackedTraceData && kProvider.Equals("MemoryImageLoadBackedTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemoryPageAccessTraceData && kProvider.Equals("MemoryPageAccessTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\tpageKind: {0}\n", ((MemoryPageAccessTraceData)data).PageKind));
            }
            else if (data is MemoryPageFaultTraceData && kProvider.Equals("MemoryPageFaultTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\tvirtualAddress: {0}\n", ((MemoryPageFaultTraceData)data).VirtualAddress));
            }
            else if (data is MemoryProcessMemInfoTraceData && kProvider.Equals("MemoryProcessMemInfoTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is MemorySystemMemInfoTraceData && kProvider.Equals("MemorySystemMemInfoTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ObjectDuplicateHandleTraceData && kProvider.Equals("ObjectDuplicateHandleTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ObjectHandleTraceData && kProvider.Equals("ObjectHandleTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ObjectNameTraceData && kProvider.Equals("ObjectNameTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ObjectTypeNameTraceData && kProvider.Equals("ObjectTypeNameTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is PMCCounterProfTraceData && kProvider.Equals("PMCCounterProfTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ProcessCtrTraceData && kProvider.Equals("ProcessCtrTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ProcessTraceData && kProvider.Equals("ProcessTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is RegistryTraceData && kProvider.Equals("RegistryTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\t{0}\tregistry: {1}:{2}\n", data.ProcessID, ((RegistryTraceData)data).KeyName, ((RegistryTraceData)data).ValueName));
            }
            else if (data is SampledProfileIntervalTraceData && kProvider.Equals("SampledProfileIntervalTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SampledProfileTraceData && kProvider.Equals("SampledProfileTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SplitIoInfoTraceData && kProvider.Equals("SplitIoInfoTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is StackWalkDefTraceData && kProvider.Equals("StackWalkDefTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is StackWalkRefTraceData && kProvider.Equals("StackWalkRefTraceData"))
            {
                Logger.logKernel(data);
            } 
            else if (data is StackWalkStackTraceData && kProvider.Equals("StackWalkStackTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is StringTraceData && kProvider.Equals("StringTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SysCallEnterTraceData && kProvider.Equals("SysCallEnterTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SysCallExitTraceData && kProvider.Equals("SysCallExitTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigCPUTraceData && kProvider.Equals("SystemConfigCPUTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigIDEChannelTraceData && kProvider.Equals("SystemConfigIDEChannelTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigIRQTraceData && kProvider.Equals("SystemConfigIRQTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigLogDiskTraceData && kProvider.Equals("SystemConfigLogDiskTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigNetworkTraceData && kProvider.Equals("SystemConfigNetworkTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigNICTraceData && kProvider.Equals("SystemConfigNICTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigPhyDiskTraceData && kProvider.Equals("SystemConfigPhyDiskTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigPnPTraceData && kProvider.Equals("SystemConfigPnPTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigPowerTraceData && kProvider.Equals("SystemConfigPowerTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigServicesTraceData && kProvider.Equals("SystemConfigServicesTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemConfigVideoTraceData && kProvider.Equals("SystemConfigVideoTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is SystemPathsTraceData && kProvider.Equals("SystemPathsTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpConnectTraceData && kProvider.Equals("TcpIpConnectTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\trcvwin: {0}\n", ((TcpIpConnectTraceData)data).rcvwin));
            }
            else if (data is TcpIpFailTraceData && kProvider.Equals("TcpIpFailTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpSendTraceData && kProvider.Equals("TcpIpSendTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\tsize:{1} -> {0}\n",((TcpIpSendTraceData)data).size, ((TcpIpSendTraceData)data).sport));
            }
            else if (data is TcpIpTraceData && kProvider.Equals("TcpIpTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\tconnId: {0}\n", ((TcpIpTraceData)data).connid));
            }            
            else if (data is TcpIpV6ConnectTraceData && kProvider.Equals("TcpIpV6ConnectTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpV6SendTraceData && kProvider.Equals("TcpIpV6SendTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is TcpIpV6TraceData && kProvider.Equals("TcpIpV6TraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ThreadSetNameTraceData && kProvider.Equals("ThreadSetNameTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is ThreadTraceData && kProvider.Equals("ThreadTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is UdpIpFailTraceData && kProvider.Equals("UdpIpFailTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is UdpIpTraceData && kProvider.Equals("UdpIpTraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is UpdIpV6TraceData && kProvider.Equals("UpdIpV6TraceData"))
            {
                Logger.logKernel(data);
            }
            else if (data is VirtualAllocTraceData && kProvider.Equals("VirtualAllocTraceData"))
            {
                Logger.logKernel(data);
                Console.WriteLine(String.Format("\t\t\topcodeName: {0}\n", ((VirtualAllocTraceData)data).OpcodeName));
            }
            else if (data is VolumeMappingTraceData && kProvider.Equals("VolumeMappingTraceData"))
            {
                Logger.logKernel(data);
            }
            else
            {
                //do nothing;
            }
        }
    }
}
