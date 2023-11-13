using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAETWi.Helper
{
    public static class Help
    {
        public static void usage()
        {
            Console.WriteLine("Usage:\n\t exe /externalIP=<IP> [/provider=<name>] [/verbose={true,false}]\n" +
                "Keystrokes:\n" +
                "\t 'v' -> switch verbose mode \n" +
                "\t 'd' -> dump output");
        }
    }
}
