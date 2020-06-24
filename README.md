# AutoDebug
Simple Automated Debugger to run Windbg Commands and also query .NET CLR Runtime data

### Overview
[Microsoft.Diagnostics.Runtime (ClrMD)](https://github.com/microsoft/clrmd) is a set of APIs for introspecting processes and dumps.
AutoDebug project make use of ClrMD v2 API's to build the underlying debugger.

### Why AutoDebug?
Quite often when you have large memory dumps, you need to run multiple debugger commands first to start looking deeper into the issues. This project would provide an easy way to automate for running a set of commands on the memory dumps at hand.

### Usage
```c#
// Create Debugger instance and call Execute for any Windbg Command
using (DbgEngine dbg = new DbgEngine(DumpFileName))
{
	Console.WriteLine(dbg.Execute(".time"));
	Console.WriteLine(dbg.Execute("~"));
	Console.WriteLine(dbg.Execute(".sympath"));
}
```

### Output
> Sample output of the above commands

	Debug session time: Fri Jun 12 23:46:40.000 2020(UTC + 5:30)
	System Uptime: 1 days 20:32:03.277
	Process Uptime: 0 days 0:06:13.000
		Kernel time: 0 days 0:00:01.000
		User time: 0 days 0:00:05.000

	.0  Id: 13ac.1350 Suspend: 0 Teb: 00000081`b83dc000 Unfrozen
	 1  Id: 13ac.380 Suspend: 0 Teb: 00000081`b83de000 Unfrozen
	 2  Id: 13ac.1bc4 Suspend: 0 Teb: 00000081`b83e0000 Unfrozen
	 3  Id: 13ac.1088 Suspend: 0 Teb: 00000081`b83e6000 Unfrozen
	 4  Id: 13ac.dc8 Suspend: 0 Teb: 00000081`b83e8000 Unfrozen
	 5  Id: 13ac.462c Suspend: 0 Teb: 00000081`b83ec000 Unfrozen
	 6  Id: 13ac.5788 Suspend: 0 Teb: 00000081`b83ee000 Unfrozen
	 7  Id: 13ac.1adc Suspend: 0 Teb: 00000081`b83f0000 Unfrozen
	 8  Id: 13ac.3b6c Suspend: 0 Teb: 00000081`b83f2000 Unfrozen
	 9  Id: 13ac.4450 Suspend: 0 Teb: 00000081`b83f4000 Unfrozen
	10  Id: 13ac.41d8 Suspend: 0 Teb: 00000081`b83f6000 Unfrozen
	11  Id: 13ac.4c3c Suspend: 0 Teb: 00000081`b83f8000 Unfrozen
	12  Id: 13ac.566c Suspend: 0 Teb: 00000081`b83fa000 Unfrozen
	13  Id: 13ac.1df8 Suspend: 0 Teb: 00000081`b83fc000 Unfrozen

	Symbol search path is: srv*
	Expanded Symbol search path is: cache *; SRV* https://msdl.microsoft.com/download/symbols

**Command list file**
> Create a text file in the application local folder > commandlist.txt

_Sample file content below, one command per line_

	.time
	.sympath
	~
	~*kb 100
	.dumpdebug

### Task list
- [x] Functional debugger to run Windbg commands
- [x] Run native Windbg Commands from text file
- [ ] Add CLR sample functions like SOS commands

### Feedback
Feel free to provide feedback on how it would help and what needs to be added
- Open issues
- Send PR
- Other ideas and suggestions are welcome

