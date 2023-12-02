using System;

namespace YAETWi.Helper
{
    public static class Help
    {
        private static string version = "\nv2.3.2\n";
        public static void usage()
        {
            Console.WriteLine("Version:" + version + "\n" +
            "Usage:\n\t YAETWi.exe\n" +
            "\t\t/externalIP=<IP> | /pids=<comma-separated list of pids to be traced>\n" +
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
                "\t 'p' -> manually provide comma-separated pids to be traced -> all collections and pids will be purged; (!) list is not immune to pids discovered through 'externalIP' parameter afterwards\n" +
                "\t 'v' -> switch (verbose) mode\n" +
                "\t 'h' -> show (help) menu");
        }
    }
}
