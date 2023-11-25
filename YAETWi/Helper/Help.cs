using System;

namespace YAETWi.Helper
{
    public static class Help
    {
        public static void usage()
        {
            Console.WriteLine("Usage:\n\t YAETWi.exe\n" +
            "\t\t/externalIP=<IP> | /pid=<PID>\n" +
            "\t\t[/verbose]\n" +
            "\t\t[/kernel]\n");
        }
        public static void print()
        {
            usage();
            keystrokes();
        }
        public static void keystrokes()
        {
            Console.WriteLine(
                "Keystrokes:\n" +
                "\t 'd' -> (dump) all traced providers\n" +
                "\t 'r' -> (read) provider name to print detailed output for\n" +
                "\t 'c' -> (clear) all events\n" +
                "\t 'p' -> change (pid) to be traced -> previous collections will be purged\n" +
                "\t 'v' -> switch (verbose) mode\n" +
                "\t 'h' -> show (help) menu");
        }
    }
}
