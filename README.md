# YAETWi

Yet Another ETW implementation;

### Description
Imagine, you're writing your own detection rules based on the Windows log events.\
YAETWi allows you to trace the connections from particular IP address by protocolling their PIDs and dump all event and opcode IDs of the particular ETW provider.

### Examples
- Tracing impacket/rdp_check.py execution from Kali VM:
![rdp_check.py_test](./_README/01_testing_impacket_rdp_check.py.png)
- Tracing impacket/smbexec.py execution from Kali VM:
![smbexec.py_test](./_README/02_testing_impacket_smbexec.py.png)

### MAN pages
```
Usage:
         .\YAETWi.exe
		/externalIP=<IP>	<- IP address the connections to be protocolled from
	     	[/provider=<name>]  	<- if not provided, only kernel logs available
		[/kernel]		<- enable kernel tracing (can be toggled via keystrokes while process execution)
		[/verbose]		<- enable more verbose output (can be toggled via keystrokes while process execution)
Keystrokes:
         'v' -> switch verbose mode
	 'k' -> enable/disable kernel logging
         'd' -> dump output
```
