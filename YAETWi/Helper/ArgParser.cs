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
        public enum Parameters
        { 
            pid,
            externalIP,
            provider,
            verbose,
            kernel
        }

        public static Dictionary<string, string> parse(string[] args)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach(var arg in args)
            {
                string[] split = arg.Split('=');
                var r = split[0].Replace("/", string.Empty);
                if (split.Length == 1)
                {
                    parameters[r] = "true";
                }
                if (split.Length == 2)
                {
                    parameters[r] = split[1];
                }
            }
            return parameters;
        }
    }
}
