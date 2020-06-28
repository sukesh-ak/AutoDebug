using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;
using Microsoft.Diagnostics.Runtime.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AutoDebug
{
    public partial class DbgEngine : IDebugOutputCallbacks, IDisposable
    {
        [DllImport("dbgeng.dll")]
        private static extern int DebugCreate(in Guid InterfaceId, out IntPtr pDebugClient);

        private static readonly object s_sync = new object();
        private bool _disposed;

        private readonly IDebugClient _client;
        private readonly IDebugControl _control;
        private readonly StringBuilder _result = new StringBuilder(1024);
        private readonly DEBUG_OUTPUT _mask;

        /// <summary>
        /// Memory dump file name
        /// </summary>
        public string DumpFileName { get; }

        /// <summary>
        /// ClrMD DataTarget instance
        /// </summary>
        public DataTarget DataTargetInstance { get; }

        /// <summary>
        /// Creates and instance of debugger and also initialize DataTarget
        /// </summary>
        /// <param name="dumpfilename">Memory dump file name</param>
        public DbgEngine(string dumpfilename)
        {
            // Use only DEBUG_OUTPUT.NORMAL, if you don't need all output
            _mask = DEBUG_OUTPUT.NORMAL | DEBUG_OUTPUT.SYMBOLS | DEBUG_OUTPUT.ERROR | DEBUG_OUTPUT.WARNING | DEBUG_OUTPUT.DEBUGGEE;

            Guid guid = new Guid("{27fe5639-8407-4f47-8364-ee118fb08ac8}");
            HResult hr = DebugCreate(guid, out IntPtr pDebugClient);
            if (!hr.IsOK)
                throw new Exception($"Failed to create DebugClient, hr={hr:x}.");

            _client = (IDebugClient)Marshal.GetTypedObjectForIUnknown(pDebugClient, typeof(IDebugClient));
            _control = (IDebugControl)_client;

            hr = _client.OpenDumpFile(dumpfilename);
            if (!hr.IsOK)
                throw new Exception($"Failed to OpenDumpFile, hr={hr:x}.");

            hr = _control.WaitForEvent(DEBUG_WAIT.DEFAULT, 10000);
            if (!hr.IsOK)
                throw new Exception($"Failed to attach to dump file, hr={hr:x}.");

            Marshal.Release(pDebugClient);

            // ClrMD API DataTarget instance
            DataTargetInstance = DataTarget.CreateFromDbgEng(pDebugClient);
        }

        /// <summary>
        /// Execute a Windbg command
        /// </summary>
        /// <param name="cmd">Command to execute</param>
        /// <returns>Command output</returns>
        public string Execute(string cmd)
        {
            string result = string.Empty;
            lock (s_sync)
            {
                _client.GetOutputCallbacks(out IDebugOutputCallbacks callbacks);
                try
                {
                    HResult hr = _client.SetOutputCallbacks(this);
                    if (!hr.IsOK)
                        return null;
                    
                    hr = _control.Execute(DEBUG_OUTCTL.THIS_CLIENT, cmd, DEBUG_EXECUTE.DEFAULT);
                    if (!hr.IsOK) _result.Append($"Command encountered an error. HRESULT={hr:x}.");
                    result = _result.ToString();
                }
                finally
                {
                    if (callbacks != null)
                        _client.SetOutputCallbacks(callbacks);
                    _result.Clear();
                }
            }
            return result;
        }

        public int Output(DEBUG_OUTPUT Mask, string Text)
        {
            if ((_mask & Mask) == 0)
                return HResult.S_OK;

            lock (_result)
                _result.Append(Text);

            return HResult.S_OK;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                DataTargetInstance.Dispose();
                _disposed = true;
            }
        }

        public override string ToString() => "Dump filename : " + DumpFileName;
    }
}
