using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using YAETWi.Data;
using YAETWi.Helper;

namespace YAETWi.Core
{

    public static class ETW
    {
        public static ConcurrentDictionary<string, Data.Tracer> providerToTracer = new ConcurrentDictionary<string, Data.Tracer>();
        public static HashSet<string> kernelEvents = new HashSet<string>();
        public static Provider provider = new Provider();

        public static void refreshCollection()
        {
            Program.pids = new HashSet<int>();
            Logger.printInfo("successfully refreshed pids");
            Logger.printInfo("refreshing collection...");
            foreach (string p in provider.providersByName.Keys)
            {
                string guid = provider.providersAll[p];
                try
                {
                    Data.Tracer t;
                    if (providerToTracer.ContainsKey(guid))
                    {
                        providerToTracer.TryRemove(guid, out t);
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
                    t.isTraced = false;
                    providerToTracer.TryAdd(guid, t);

                }
                catch (Exception){}
            }
            Logger.printInfo("successfully refreshed the collection");
        }

        public static void traceKernel(TraceEventSession session)
        {
            session = new TraceEventSession("kernel session");
            Logger.printInfo("starting kernel session");
            session.EnableKernelProvider(KernelTraceEventParser.Keywords.All);

            session.Source.Kernel.All += ((TraceEvent data) =>
            {
                if (Program.pids.Contains(data.ProcessID))
                {
                    Program.events++;
                    kernelEvents.Add(data.GetType().ToString());
                    if (Program.verbose)
                    {
                        parseKernelEvents(data);
                    }
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
                    if (Program.verbose)
                        Logger.printVerbose(String.Format("activated provider: {0}", p));
                }
                catch (Exception)
                {
                    Logger.printNCFailure(String.Format("cannot activate {0}", p));
                }
            }

            refreshCollection();

            Logger.printInfo("activated all ETW providers");

            session.Source.AllEvents += ((TraceEvent data) =>
            {
                if (Program.pids.Contains(data.ProcessID))
                {
                    if (!data.ProviderGuid.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                    {
                        try
                        {
                            Program.events++;

                            Data.Tracer t = providerToTracer[data.ProviderGuid.ToString()];
                            int eventID = UInt16.Parse(data.EventName.Split('(', ')')[1]);
                            int opcodeID = (int)data.Opcode;
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
                            events.Enqueue(new Data.Event(data.TimeStamp, eventID, data.ProcessID));
                            opcodes.Enqueue(new Data.Opcode(data.TimeStamp, opcodeID, data.ProcessID));
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
                Console.WriteLine(String.Format("FileName: {0}",
                    ((ImageLoadTraceData)data).FileName));
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
                Console.WriteLine(String.Format("\t\t\t{0}\tregistry: {1}:{2}\n", data.ProcessID, ((RegistryTraceData)data).KeyName, ((RegistryTraceData)data).ValueName));
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
