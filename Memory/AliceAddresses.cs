using System;
using System.Diagnostics;

namespace LiveSplit.AliceASL.Memory
{
    public class Pointer<T> where T : struct
    {
        private IntPtr BaseAddress { get; set; }
        private int[] Offsets { get; set; }

        public Pointer(IntPtr baseAddress) : this(baseAddress, new int[0]) { }
        public Pointer(IntPtr baseAddress, params int[] offsets)
        {
            this.BaseAddress = baseAddress;
            this.Offsets = offsets;
        }
        public T Read(Process process)
        {
            return process.Read<T>(this.BaseAddress, this.Offsets);
        }
    };

    public class AliceAddresses
    {
        private GameVersion GameVersion { get; set; }
        private IntPtr Mem1 { get; set; }
        private Process Process { get; set; }
        public AliceAddresses(Process proc, GameVersion version) : this(proc, version, IntPtr.Zero) { }
        public AliceAddresses(Process proc, GameVersion version, IntPtr mem1)
        {
            if (version == GameVersion.Steam && proc == null)
                throw new ArgumentException("Steam version requires a Process");
            if ((version == GameVersion.DolphinPAL || version == GameVersion.DolphinNTSC) && mem1 == IntPtr.Zero)
                throw new ArgumentException("Dolphin version requires Mem1");
            this.GameVersion = version;
            this.Mem1 = mem1;
            this.Process = proc;
        }

        public Pointer<Single> GameTimePtr
        {
            get
            {
                Pointer<Single> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<Single>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x8C, 0x44, 0x10);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<Single>(this.Mem1, 0x61CAE4);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<Single>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<Single>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }
        public Pointer<Int32> MapIDPtr
        {
            get
            {
                Pointer<Int32> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<Int32>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x8C, 0x2BC);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<Int32>(this.Mem1, 0x77F870);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<Int32>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<Int32>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }

        public Pointer<Int32> MapSectorPtr
        {
            get
            {
                Pointer<Int32> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<Int32>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x8C, 0x18, 0x18);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<Int32>(this.Mem1, 0x77676C);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<Int32>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<Int32>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }

        public Pointer<UInt32> AliceIDPtr
        {
            get
            {
                Pointer<UInt32> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<UInt32>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x8C, 0x8, 0x28, 0x9D0);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<UInt32>(this.Mem1, 0x7DE5D4, 0x9D0);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<UInt32>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<UInt32>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }

        public Pointer<UInt32> AudioStatusPtr
        {
            get
            {
                Pointer<UInt32> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<UInt32>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x1C, 0xC, 0x0, 0x4, 0x16C);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<UInt32>(this.Mem1, 0x7FCA10);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<UInt32>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<UInt32>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }

        public Pointer<UInt32> BandersnatchPhasePtr
        {
            get
            {
                Pointer<UInt32> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<UInt32>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x90, 0x54, 0x15C, 0x1C);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<UInt32>(this.Mem1, 0x7DA380);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<UInt32>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<UInt32>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }

        public Pointer<Single> StayneHealthPtr
        {
            get
            {
                Pointer<Single> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<Single>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x8C, 0x4, 0x58, 0x3D0);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<Single>(this.Mem1, 0x6F4FE4, 0x4F8);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<Single>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<Single>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }

        public Pointer<UInt32> JabberwockyPhasePtr
        {
            get
            {
                Pointer<UInt32> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<UInt32>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x9C, 0xC, 0x1C, 0x4);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<UInt32>(this.Mem1, 0x5f48d8, 0x9C, 0xC, 0x1C, 0x4);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<UInt32>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<UInt32>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }

        public Pointer<UInt32> JabberwockyPhase4CounterPtr
        {
            get
            {
                Pointer<UInt32> pointer;
                switch (this.GameVersion)
                {
                    case GameVersion.Steam:
                        pointer = new Pointer<UInt32>(this.Process.MainModule.BaseAddress, 0x44B8A8, 0x9C, 0xC, 0xC, 0x4);
                        break;
                    case GameVersion.DolphinPAL:
                        pointer = new Pointer<UInt32>(this.Mem1, 0x5f48d8, 0x9C, 0xC, 0xC, 0x4);
                        break;
                    case GameVersion.DolphinNTSC:
                        pointer = new Pointer<UInt32>(IntPtr.Zero); // TODO: Find this
                        break;
                    case GameVersion.Invalid:
                    default:
                        pointer = new Pointer<UInt32>(IntPtr.Zero);
                        break;
                }
                return pointer;
            }
        }
    }
}
