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
        static void Main(string[] args)
        {
            //string dllName = @"C:\Users\Zergatul\Documents\Visual Studio 2012\Projects\FakeFiles\Debug\FakeFiles.dll";
            string dllName = @"C:\Users\Zergatul\Documents\visual studio 2012\Projects\Bootstrap\Debug\Bootstrap.dll";
            byte[] buf = new byte[dllName.Length + 1];
            Array.Copy(Encoding.ASCII.GetBytes(dllName), buf, dllName.Length);

            var pp = Process.GetProcessesByName("ConsoleC++");
            //var pp = Process.GetProcessesByName("notepad");
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
            WinAPI.WaitForSingleObject(thread, 0xFFFFFFFF);

            var modules = new IntPtr[30];
            int bytesNeeded;
            WinAPI.EnumProcessModulesEx(process, modules, 30 * 4, out bytesNeeded, EnumProcessFlagsFilter.LIST_MODULES_32BIT);

            IntPtr bootstrapModuleHandle = IntPtr.Zero;
            for (int i = 0; i < bytesNeeded / 4; i++)
            {
                var sb = new StringBuilder();
                WinAPI.GetModuleBaseNameW(process, modules[i], sb, 65536);
                if (sb.ToString().Equals("Bootstrap.dll", StringComparison.CurrentCultureIgnoreCase))
                {
                    bootstrapModuleHandle = modules[i];
                    break;
                }
            }

            var localBootstrap = WinAPI.LoadLibraryW(dllName);
            var localProcAddr = WinAPI.GetProcAddress(localBootstrap, "LoadDotNetAssembly");
            WinAPI.FreeLibrary(localBootstrap);

            int delta = (int)localProcAddr - (int)localBootstrap;

            int remoteProcAddr = (int)bootstrapModuleHandle + delta;

            thread = WinAPI.CreateRemoteThread(process, IntPtr.Zero, 0, (IntPtr)remoteProcAddr, IntPtr.Zero, 0, out threadId);
        }
    }
}
