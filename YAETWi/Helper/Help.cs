using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAETWi.Helper
{
    public static class Help
    {
        public static void usage()
        {
            Console.WriteLine("Usage:\n\t YAETWi.exe " +
                "/externalIP=<IP> | /pid=<PID> " +
                "[/verbose]\n" +
                "Keystrokes:\n" +
                "\t 'd' -> dump all traced providers\n" +
                "\t 'r' -> entry provider to print detailed output for\n" +
                "\t 'p' -> purge all events\n" +
                "\t 'v' -> switch verbose mode\n");
        }
    }
}
