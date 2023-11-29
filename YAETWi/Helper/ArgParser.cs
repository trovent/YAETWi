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
            pids,
            externalIP,
            provider,
            verbose,
            kernel,
            all
        }

        public static Dictionary<string, string> parse(string[] args)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var arg in args)
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

        public static void readPids(string csl, HashSet<int> pids)
        {
            try
            {
                int[] p = Array.ConvertAll(csl.Split(','), int.Parse);
                for (int i = 0; i < p.Length; i++)
                {
                    pids.Add(p[i]);
                }
            }
            catch
            {
                Logger.printNCFailure("Error parsing input. Example: 1,2,3");
            }
        }
    }
}
