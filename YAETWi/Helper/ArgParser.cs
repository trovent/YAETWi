using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YAETWi.Helper
{
    public static class ArgParser
    {
        public static Dictionary<string, string> parse(string[] args)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach(var arg in args)
            {
                string[] split = arg.Split('=');
                parameters[split[0]] = split[1];
            }
            if (!parameters.ContainsKey("/provider") || !parameters.ContainsKey("/externalIP"))
            {
                Helper.Help.usage();
                Environment.Exit(0);
            }
            return parameters;
        }
    }
}
