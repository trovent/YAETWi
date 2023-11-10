# YAETWi

Yet Another ETW implementation;

### Description
Imagine, you're writing your own detection rules based on the Windows log events.\
v0.2 (see tags) allows you to trace the connections from particular IP address by protocolling their PIDs and dump all event and opcode IDs of the particular ETW provider.

### Example
- Tracing impacket/rdp_check.py executed from Kali VM:
![rdp_check.py_test](./_README/01_testing_impacket_rdp_check.py.png)

### MAN pages
```
.\YAETWi.exe 
	/externalIP=<IP, the connection to be monitored from> 
	/provider=<ETW Provider by name> 
	/verbose=<true -> resolve description, if provided, for Event and Opcode nummerical IDs>

To dump event and opcode IDs -> enter 'd' while program execution;
```
