using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace LiveSplit.AliceASL.Memory
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ModuleInfo
    {
        public IntPtr BaseAddress;
        public uint ModuleSize;
        public IntPtr EntryPoint;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MemInfo
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
        public override string ToString()
        {
            return BaseAddress.ToString("X") + " " + Protect.ToString("X") + " " + State.ToString("X") + " " + Type.ToString("X") + " " + RegionSize.ToString("X");
        }
    }

    public static class WinAPI
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, [Out] IntPtr[] lphModule, uint cb, out uint lpcbNeeded, uint dwFilterFlag);
        [DllImport("psapi.dll", SetLastError = true)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
        [DllImport("psapi.dll")]
        public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInfo lpmodinfo, uint cb);
    }

    public static class MemoryReader
    {
        private static int OffsetAddress(Process targetProcess, ref IntPtr address, params int[] offsets)
        {
            byte[] buffer = new byte[4];
            // Deepcopy so we don't modify the ref param
            IntPtr baseAddr = new IntPtr(address.ToInt64());
            for (int i = 0; i < offsets.Length - 1; i++)
            {
                WinAPI.ReadProcessMemory(targetProcess.Handle, address + offsets[i], buffer, buffer.Length, out int bytesRead);
                if (bytesRead != buffer.Length) { break; }
                if (targetProcess.ProcessName == "Dolphin")
                    Array.Reverse(buffer);
                address = (IntPtr)BitConverter.ToUInt32(buffer, 0);
                if (address == IntPtr.Zero) { break; }
                if (targetProcess.ProcessName == "Dolphin")
                    address = (IntPtr)(baseAddr.ToInt64() + (address.ToInt64() - 0x80000000)); // Dolphin uses a different base address
            }
            return offsets.Length > 0 ? offsets[offsets.Length - 1] : 0;
        }

        public static T Read<T>(Process targetProcess, IntPtr address, params int[] offsets) where T : struct
        {
            if (targetProcess == null || address == IntPtr.Zero) { return default; }

            int last = OffsetAddress(targetProcess, ref address, offsets);
            if (address == IntPtr.Zero) { return default; }

            Type type = typeof(T);
            type = (type.IsEnum ? Enum.GetUnderlyingType(type) : type);

            int count = (type == typeof(bool)) ? 1 : Marshal.SizeOf(type);
            byte[] buffer = Read(targetProcess, address + last, count);

            object obj = ResolveToType(buffer, type);
            return (T)obj;
        }
        public static byte[] Read(Process targetProcess, IntPtr address, int numBytes)
        {
            byte[] buffer = new byte[numBytes];
            if (targetProcess == null || address == IntPtr.Zero) { return buffer; }

            WinAPI.ReadProcessMemory(targetProcess.Handle, address, buffer, numBytes, out int bytesRead);
            if (bytesRead != numBytes) { return default; }
            if (targetProcess.ProcessName == "Dolphin")
                Array.Reverse(buffer);
            return buffer;
        }
        private static object ResolveToType(byte[] bytes, Type type)
        {
            if (type == typeof(int))
            {
                return BitConverter.ToInt32(bytes, 0);
            }
            else if (type == typeof(uint))
            {
                return BitConverter.ToUInt32(bytes, 0);
            }
            else if (type == typeof(float))
            {
                return BitConverter.ToSingle(bytes, 0);
            }
            else if (type == typeof(double))
            {
                return BitConverter.ToDouble(bytes, 0);
            }
            else if (type == typeof(byte))
            {
                return bytes[0];
            }
            else if (type == typeof(sbyte))
            {
                return (sbyte)bytes[0];
            }
            else if (type == typeof(bool))
            {
                return bytes != null && bytes[0] > 0;
            }
            else if (type == typeof(short))
            {
                return BitConverter.ToInt16(bytes, 0);
            }
            else if (type == typeof(ushort))
            {
                return BitConverter.ToUInt16(bytes, 0);
            }
            else if (type == typeof(long))
            {
                return BitConverter.ToInt64(bytes, 0);
            }
            else if (type == typeof(ulong))
            {
                return BitConverter.ToUInt64(bytes, 0);
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                object result = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
                handle.Free();
                return result;
            }
        }

        public static string ReadString(Process targetProcess, IntPtr address, int length, Encoding encoding)
        {
            if (targetProcess == null || address == IntPtr.Zero) { return default; }
            byte[] data = new byte[length];
            WinAPI.ReadProcessMemory(targetProcess.Handle, address, data, length, out int bytesRead);
            if (bytesRead != length) { return default; }
            return encoding.GetString(data);
        }
    }
}
