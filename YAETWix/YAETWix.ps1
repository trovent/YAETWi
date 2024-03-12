$YAETWix = @"

using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

public class YAETWix
    {
        [StructLayout(LayoutKind.Sequential)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        private enum dwCreationFlags
        {
            CREATE_SUSPENDED = 0x04
        }

        public static void Main(string[] args)
        {

            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            bool res = CreateProcess(null, 
                args[0] + "\0", 
                IntPtr.Zero, 
                IntPtr.Zero, 
                false, 
                ((uint)dwCreationFlags.CREATE_SUSPENDED), 
                IntPtr.Zero, 
                null, 
                ref si, 
                out pi);

            Console.WriteLine(String.Format("[!] Process has been created\n" +
                "[*] PID: {0}\n[*] to resume process -> enter 'r'\n" +
                "[*] to terminate process -> enter CTRL+C", pi.dwProcessId)); 

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                Console.WriteLine(String.Format("[!] Terminating process {0}", pi.dwProcessId));
                TerminateProcess(pi.hProcess, 0);
                Environment.Exit(0);
            };

            while (true)
            {
                var cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.R:
                        Console.WriteLine(String.Format("\n[!] Process {0} has been resumed\n" +
                            "[*] to terminate from inside the process -> Ctrl+C", pi.dwProcessId));
                        ResumeThread(pi.hThread);
                        break;
                }
            }

        }
}
"@

Add-Type $YAETWix


