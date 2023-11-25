# YAETWi

Yet Another ETW implementation;

YAETWix (supportive tool) -> see description below;

### Description
Imagine, you're writing your own detection rules based on the Windows log events.\
YAETWi allows you to trace a particular process based on its PID (-> YAETWix) or based on the connections from a particular IP address providing a deep tracing based on implementation of the ETW interface. 

### Examples
- Tracing impacket/rdp_check.py execution from Kali VM:
![rdp_check.py_test](./_README/01_testing_impacket_rdp_check.py.png)
- Tracing impacket/smbexec.py execution from Kali VM:
![smbexec.py_test](./_README/02_testing_impacket_smbexec.py.png)

### MAN pages
```
Usage:
         .\YAETWi.exe
		/externalIP=<IP>  | /pid=<PID>		<- IP address the connections to be protocolled from | PID of the process (YAETWix or alternatively WinDBG can be used to suspend the process on its start)
		[/verbose]				<- enables more verbose output (can be toggled via keystroke while process execution)
 		[/kernel]				<- start kernel session 
Keystrokes:
         'd' -> (dump) all traced providers
         'r' -> (read) provider name to print detailed output for
         'c' -> (clear) all events
         'p' -> change (pid) to be traced -> previous collections will be purged
         'v' -> switch (verbose) mode
         'h' -> print keystrokes
```

# YAETWix

### Description
YAETWix is a helper tool, which puts any process into suspended mode, obtains its PID, which in turn can be provided to YAETWi as an argument, and continues its execution, after you're ready.

### MAN pages
```
Usage:
        .\YAETWix.exe <"full_path_to_binary + arguments">
Example:
        .\YAETWix.exe "c:\windows\system32\cmd.exe /c whoami"
Kestrokes:
        r -> resume process
```