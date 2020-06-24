using System;
using System.IO;

namespace AutoDebug
{
    class Program
    {
        /// <summary>
        ///  Create this command list file in the same folder
        /// </summary>
        static string commandListFileName = "commandlist.txt";
        
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine($"Usage: AutoDebug [memory-dump-filename]");
                Environment.Exit(1);
            }

            string DumpFileName = args[0];
            if (!File.Exists(DumpFileName))
            {
                Console.WriteLine($"Unable to open [{DumpFileName}] or the file is missing.");
                Console.WriteLine($"Usage: AutoDebug [memory-dump-filename]");
                Environment.Exit(1);
            }

            /// Get complete path of the command list file
            commandListFileName = Path.Combine(Directory.GetCurrentDirectory(),commandListFileName);

            /// Create Debugger instance and call Execute any Windbg Command
            using (DbgEngine dbg = new DbgEngine(DumpFileName))
            {
                #region Command list file
                /// Check if command file exists
                if (File.Exists(commandListFileName))
                {
                    /// Read file one line at time
                    foreach (string cmd in File.ReadLines(commandListFileName))
                    {
                        if (!string.IsNullOrEmpty(cmd))
                        {
                            Console.WriteLine($"[Command]> {cmd}");
                            Console.WriteLine(dbg.Execute(cmd));
                        }
                    }
                }
                #endregion

                /// Sample ClrMD v2 snippets to mimic SOS Windbg extension commands
                /// Check the sample code in ClrDebugger.cs file
                dbg.ClrDacInfo();
                dbg.DumpHeap(StatsOnly : false);
                dbg.DumpModules();
            }

            #region Sample Output of above 3 commands
            //Debug session time: Fri Jun 12 23:46:40.000 2020(UTC + 5:30)
            //System Uptime: 1 days 20:32:03.277
            //Process Uptime: 0 days 0:06:13.000
            //  Kernel time: 0 days 0:00:01.000
            //  User time: 0 days 0:00:05.000

            //.  0  Id: 13ac.1350 Suspend: 0 Teb: 00000081`b83dc000 Unfrozen
            //   1  Id: 13ac.380 Suspend: 0 Teb: 00000081`b83de000 Unfrozen
            //   2  Id: 13ac.1bc4 Suspend: 0 Teb: 00000081`b83e0000 Unfrozen
            //   3  Id: 13ac.1088 Suspend: 0 Teb: 00000081`b83e6000 Unfrozen
            //   4  Id: 13ac.dc8 Suspend: 0 Teb: 00000081`b83e8000 Unfrozen
            //   5  Id: 13ac.462c Suspend: 0 Teb: 00000081`b83ec000 Unfrozen
            //   6  Id: 13ac.5788 Suspend: 0 Teb: 00000081`b83ee000 Unfrozen
            //   7  Id: 13ac.1adc Suspend: 0 Teb: 00000081`b83f0000 Unfrozen
            //   8  Id: 13ac.3b6c Suspend: 0 Teb: 00000081`b83f2000 Unfrozen
            //   9  Id: 13ac.4450 Suspend: 0 Teb: 00000081`b83f4000 Unfrozen
            //  10  Id: 13ac.41d8 Suspend: 0 Teb: 00000081`b83f6000 Unfrozen
            //  11  Id: 13ac.4c3c Suspend: 0 Teb: 00000081`b83f8000 Unfrozen
            //  12  Id: 13ac.566c Suspend: 0 Teb: 00000081`b83fa000 Unfrozen
            //  13  Id: 13ac.1df8 Suspend: 0 Teb: 00000081`b83fc000 Unfrozen

            //Symbol search path is: srv*
            //Expanded Symbol search path is: cache *; SRV* https://msdl.microsoft.com/download/symbols
            #endregion
        }
    }
}
