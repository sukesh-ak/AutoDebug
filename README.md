# AutoDebug
Simple Automated Debugger to run Windbg Commands and also query .NET CLR Runtime data in C#

### Overview
[Microsoft.Diagnostics.Runtime (ClrMD)](https://github.com/microsoft/clrmd) is a set of APIs for introspecting processes and dumps.
AutoDebug project make use of ClrMD v2 API's to build the underlying debugger.

### Why AutoDebug?
Quite often when you have large memory dumps, you need to run multiple debugger commands first to start looking deeper into the issues. This project would provide an easy way to automate for running a set of commands on the memory dumps at hand.

Check the [Wiki for documentation](https://github.com/sukesh-ak/AutoDebug/wiki)

### Task list
- [x] Functional debugger to run Windbg commands
- [x] Run native Windbg Commands from text file
- [ ] Add CLR sample functions like SOS commands

### Feedback
Feel free to provide feedback on how it would help and what needs to be added
- Open issues
- Send PR
- Other ideas and suggestions are welcome
