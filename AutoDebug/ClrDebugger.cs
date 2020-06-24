using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoDebug
{
    public partial class DbgEngine : IDebugOutputCallbacks, IDisposable
    {
        /// <summary>
        /// Prints out CLR Summary information
        /// </summary>
        public void ClrDacInfo()
        {
            if (DataTargetInstance == null) return;
            // ClrMD DataTarget instance from dbgeng
            Console.WriteLine(">>> CLR Info");
            Console.WriteLine($"Architecture.: {DataTargetInstance.DataReader.Architecture}");
            Console.WriteLine($"Pointer Size.: {DataTargetInstance.DataReader.PointerSize}");
            Console.WriteLine($"Display Name.: {DataTargetInstance.DataReader.DisplayName}");

            foreach (ClrInfo clr in DataTargetInstance.ClrVersions)
            {
                Console.WriteLine($"Clr Flavor...: {clr.Flavor}");
                Console.WriteLine($"Clr Version..: {clr.Version}");

                // This is the data needed to request the dac from the symbol server:
                DacInfo dacInfo = clr.DacInfo;
                Console.WriteLine("Filesize.....: {0:X}", dacInfo.IndexFileSize);
                Console.WriteLine("Timestamp....: {0:X}", dacInfo.IndexTimeStamp);
                Console.WriteLine("Dac File.....: {0}", dacInfo.PlatformSpecificFileName);

                // If we just happen to have the correct dac file installed on the machine,
                // the "LocalMatchingDac" property will return its location on disk:
                string dacLocation = clr.DacInfo.LocalDacPath;
                if (!string.IsNullOrEmpty(dacLocation))
                    Console.WriteLine("Local DAC....: " + dacLocation);
            }
            Console.WriteLine();

            #region >>> Sample output of above code
                //>>> CLR Info
                //Architecture.: Amd64
                //Pointer Size.: 8
                //Display Name.: DbgEng, IDebugClient = 1a4fe703148
                //  Clr Flavor...: Core
                //  Clr Version..: 4.700.20.20201
                //Filesize.....: 56C000
                //Timestamp....: 5E867108
                //Dac File.....: mscordaccore_Amd64_Amd64_4.700.20.20201.dll
                //Local DAC....: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\mscordaccore.dll

            #endregion
        }

        /// <summary>
        /// Prints out the CLR Heap just like SOS!dumpheap command
        /// </summary>
        /// <param name="StatsOnly">Show Statistics only summary or not</param>
        public void DumpHeap(bool StatsOnly = false)
        {
            if (DataTargetInstance == null) return;
            foreach (ClrInfo clr in DataTargetInstance.ClrVersions)
            {
                Console.WriteLine();

                using ClrRuntime runtime = clr.CreateRuntime();
                ClrHeap heap = runtime.Heap;

                // ignore details and show only summary
                if (!StatsOnly)
                {
                    // !sos.dumpheap
                    Console.WriteLine("{0,16} {1,16} {2,8} {3}", "Object", "MethodTable", "Size", "Type");
                    foreach (ClrObject obj in heap.EnumerateObjects())
                        Console.WriteLine(
                            $"{obj.Address:x16} {obj.Type.MethodTable:x16} {obj.Size,8:D} {obj.Type.Name}");
                }

                // !sos.dumpheap -stat
                Console.WriteLine("\nStatistics:");
                var dumpheapstats = from obj in heap.EnumerateObjects()
                                    group obj by obj.Type into g
                                    let size = g.Sum(p => (long)p.Size)
                                    orderby size
                                    select new
                                    {
                                        g.Key.MethodTable,
                                        Count = g.Count(),
                                        Size = size,
                                        g.Key.Name
                                    };

                Console.WriteLine("{0,16} {1,12} {2,12}\t{3}", "MethodTable", "Count", "Size", "Type");
                foreach (var item in dumpheapstats)
                    Console.WriteLine(
                        $"{item.MethodTable:x16} {item.Count,12:D} {item.Size,12:D}\t{item.Name}");

                Console.WriteLine($"Total {dumpheapstats.Sum(x => x.Count):0} objects");
            }
            Console.WriteLine();

            #region >>> Above code sample output below, similiar to !sos.dumpheap
            //          Object MethodTable          Size Type
            //000001d1445a1000 000001d13e08bf90       24 Free
            //000001d1445a1018 000001d13e08bf90       24 Free
            //000001d1445a1030 000001d13e08bf90       24 Free
            //000001d1445a1048 00007ffb05430638      152 System.RuntimeType + RuntimeTypeCache
            //000001d1445a10e0 00007ffb0552eed0       56 System.RuntimeType + RuntimeTypeCache + MemberInfoCache < System.Reflection.RuntimeConstructorInfo >
            //000001d1445a1118 00007ffb0552ec60      104 System.Reflection.RuntimeConstructorInfo
            //000001d1445a1180 00007ffb0552f078       32 System.Reflection.RuntimeConstructorInfo[]
            //...
            //000001d5745a3030 000001d13e08bf90       32 Free
            //000001d5745a3050 00007ffb05316610     8184 System.Object[]

            //Statistics:
            //     MethodTable        Count         Size  Type
            //00007ffb0577d1f8            1           24  Microsoft.Extensions.Logging.Configuration.LoggerProviderConfigurationFactory
            //00007ffb05a26c00            1           24  Microsoft.Extensions.Options.OptionsCache < Microsoft.Extensions.Logging.Console.ConsoleLoggerOptions >
            //00007ffb05a49c78            1           24  Microsoft.Extensions.Options.OptionsMonitor < Microsoft.Extensions.Logging.Console.ConsoleLoggerOptions >
            //00007ffb05a49dc0            1           24  Microsoft.Extensions.Primitives.ChangeToken + ChangeTokenRegistration < System.String >
            //00007ffb05a49e70            1           24  Microsoft.Extensions.Primitives.ChangeToken + ChangeTokenRegistration < System.String >
            //00007ffb05a4be00            1           24  Microsoft.Extensions.Logging.NullExternalScopeProvider
            //00007ffb05a4cf90            1           24  Microsoft.Extensions.Configuration.ConfigurationBinder +<> c
            //00007ffb05a4cd80            1           24  Microsoft.Extensions.Configuration.BinderOptions...
            //00007ffb053ef090         2010       111364  System.Int32[]
            //00007ffb053d2aa8         2224       126352  System.SByte[]
            //00007ffb05413058         1297       260138  System.Char[]
            //00007ffb05316610         2160       331512  System.Object[]
            //00007ffb053d2360          841       558172  System.Byte[]
            //00007ffb053d1e18         8366       718666  System.String
            //Total 82567 objects
            #endregion
        }

        /// <summary>
        /// Prints out All AppDomains and modules under it
        /// </summary>
        public void DumpModules()
        {
            if (DataTargetInstance == null) return;
            foreach (ClrInfo clr in DataTargetInstance.ClrVersions)
            {
                using ClrRuntime runtime = clr.CreateRuntime();
                foreach (ClrAppDomain domain in runtime.AppDomains)
                {
                    Console.WriteLine(">>> AppDomain & Modules");
                    Console.WriteLine("ID.....: {0}", domain.Id);
                    Console.WriteLine("Name...: {0}", domain.Name);
                    Console.WriteLine("Address: {0:x16}", domain.Address);


                    Console.WriteLine("\t{0,16} {1,16} {2}", "Assembly", "Module", "Module Name");
                    foreach (ClrModule module in domain.Modules)
                    {
                        Console.WriteLine("\t{0:x16} {1:x16} {2}",
                            module.AssemblyAddress, module.Address,
                            string.IsNullOrEmpty(module.Name) ? "[Dynamic Module]" : module.Name);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            #region Output similiar to !dumpdomain
            //>>> AppDomain
            //ID.....: 1
            //Name...: clrhost
            //Address: 000001d13e0423c0
            //        Assembly           Module Module Name
            //000001d143f66b80 00007ffb05214020 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.Private.CoreLib.dll
            //000001d143f63860 00007ffb053ef7e0 C:\DAv2\ASPNETCoreWeb\ASPNETCoreWeb\bin\Debug\netcoreapp3.1\ASPNETCoreWeb.dll
            //000001d143f63560 00007ffb054014e8 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.Runtime.dll
            //...
            //000001d144120bf0 00007ffb05633178 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.Text.Encoding.Extensions.dll
            //000001d14411fb70 00007ffb056628c0 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.IO.FileSystem.Watcher.dll
            //000001d1441215f0 00007ffb05662fe8 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.ComponentModel.Primitives.dll
            //000001d1441292c0 00007ffb05c571c0 C:\DAv2\ASPNETCoreWeb\ASPNETCoreWeb\bin\Debug\netcoreapp3.1\Microsoft.EntityFrameworkCore.Relational.dll
            //000001d58f172680 00007ffb05f0ea18 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.Transactions.Local.dll
            //000001d58f172800 00007ffb05f92040[Dynamic Module]
            //000001d58f172100 00007ffb05ff1c50 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.Security.Cryptography.Csp.dll
            //000001d58f172b00 00007ffb05ff2490 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.Security.Cryptography.Primitives.dll
            //000001d58f171480 00007ffb06122688 C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\3.1.4\Microsoft.AspNetCore.Http.Extensions.dll
            //000001d58f172c00 00007ffb06150020 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.Net.WebSockets.dll
            //000001d58f171500 00007ffb0615bd58 C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.4\System.Security.Principal.Windows.dll
            #endregion
        }

    }
}
