﻿using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Console.WriteLine(String.Format("\n{0}\tPID: {1}\n\t[*] Stream: {2}\n\t[*] Event: {3}\n\t[*] Opcode: {4} -> {5}", 
                data.TimeStamp, 
                data.ProcessID, 
                data.GetType(), 
                data.EventName,
                (int)data.Opcode,
                data.OpcodeName));
        }
    }
}
