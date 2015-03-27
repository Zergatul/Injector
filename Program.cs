using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Injector
{
    class Program
    {
        // test
        static void Main(string[] args)
        {
            string dllName = @"C:\Users\Zergatul\Documents\Visual Studio 2012\Projects\FakeFiles\Debug\FakeFiles.dll";
            byte[] buf = new byte[dllName.Length + 1];
            Array.Copy(Encoding.ASCII.GetBytes(dllName), buf, dllName.Length);

            //var pp = Process.GetProcessesByName("ConsoleC++");
            var pp = Process.GetProcessesByName("skype");
            if (pp.Length == 0)
            {
                Console.WriteLine("no processes!");
                Console.ReadLine();
                return;
            }

            var process = WinAPI.OpenProcess(ProcessAccessFlags.All, false, pp[0].Id);
            var remoteAddr = WinAPI.VirtualAllocEx(process, IntPtr.Zero, dllName.Length + 1, AllocationType.Commit, MemoryProtection.ReadWrite);

            IntPtr bytesWritten;
            var writeResult = WinAPI.WriteProcessMemory(process, remoteAddr, buf, buf.Length, out bytesWritten);

            var kernel32 = WinAPI.GetModuleHandle("kernel32.dll");
            var loadLibAddr = WinAPI.GetProcAddress(kernel32, "LoadLibraryA");
            int threadId;
            var thread = WinAPI.CreateRemoteThread(process, IntPtr.Zero, 0, loadLibAddr, remoteAddr, 0, out threadId);

            Console.WriteLine(Marshal.GetLastWin32Error());
            Console.ReadLine();
        }
    }
}
