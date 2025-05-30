﻿using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LiveSplit.AliceASL.Memory
{
    public enum GameVersion: int
    {
        Invalid = -1,
        DVDROM,
        Steam,
        DolphinPAL,
        DolphinNTSC
    };

    public partial class AliceMemory
    {
        public IntPtr Mem1 { get; private set; } = IntPtr.Zero;
        public IntPtr Mem2 { get; private set; } = IntPtr.Zero;
        public Process Proc { get; private set; }
        private AliceAddresses Addresses { get; set; }
        private Dictionary<string, object> Pointers { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> PreviousValues { get; private set; } = new Dictionary<string, object>();
        public Dictionary<string, object> CurrentValues { get; private set; } = new Dictionary<string, object>();
        public GameVersion Version { get; private set; } = GameVersion.Invalid;
        private DateTime lastHooked = DateTime.MinValue;
        public bool IsHooked { get; private set; } = false;

        public AliceMemory()
        {
            this.lastHooked = DateTime.MinValue;
        }

        public bool HookProcess()
        {
            this.Mem1 = IntPtr.Zero;
            this.Mem2 = IntPtr.Zero;
            this.IsHooked = this.Proc != null && !this.Proc.HasExited;
            if (!this.IsHooked && DateTime.Now > this.lastHooked.AddSeconds(1))
            {
                this.lastHooked = DateTime.Now;
                Process[] processes = Process.GetProcessesByName("Alice");
                if (processes.Length == 0)
                    processes = Process.GetProcessesByName("Dolphin");
                this.Proc = processes.Length == 0 ? null : processes[0];
                if (this.Proc != null && !this.Proc.HasExited)
                {
                    this.IsHooked = true;
                    if (this.Proc.ProcessName.StartsWith("Alice"))
                        // TODO: Check for DVDROM aswell
                        this.Version = GameVersion.Steam;
                    else if (this.Proc.ProcessName.StartsWith("Dolphin"))
                    {
                        foreach (MemoryBasicInformation item in this.Proc.MemoryPages(true))
                        {
                            if (item.Type == MemPageType.MEM_MAPPED && item.AllocationProtect == MemPageProtect.PAGE_READWRITE &&
                                item.State == MemPageState.MEM_COMMIT && item.Protect == MemPageProtect.PAGE_READWRITE && (int)item.RegionSize == 0x2000000)
                            {
                                this.Mem1 = item.BaseAddress;
                                this.Mem2 = (IntPtr)(
                                    long.Parse(item.BaseAddress.ToString(), System.Globalization.NumberStyles.HexNumber) +
                                    long.Parse(item.RegionSize.ToString(), System.Globalization.NumberStyles.HexNumber)
                                );
                                break;
                            }
                        }
                        if (this.Mem1 == IntPtr.Zero || this.Mem2 == IntPtr.Zero)
                            this.Version = GameVersion.Invalid;
                        else
                        {
                            switch (this.Proc.ReadString(this.Mem1, 6))
                            {
                                case "SALP4Q":
                                    this.Version = GameVersion.DolphinPAL;
                                    break;
                                case "SALE4Q":
                                    this.Version = GameVersion.DolphinNTSC;
                                    break;
                                default:
                                    this.Version = GameVersion.Invalid;
                                    break;
                            }
                        }
                    }
                    else if (this.Proc == null || this.Proc.HasExited)
                    {
                        this.IsHooked = false;
                        this.Version = GameVersion.Invalid;
                    }
                }
            }
            this.Addresses = null;
            if (this.IsHooked)
            {
                this.Addresses = new AliceAddresses(this.Proc, this.Version, this.Mem1);
                this.PopulatePointers();
            }
            return this.IsHooked;
        }

        private void PopulatePointers()
        {
            this.Pointers.Clear();
            this.Pointers.Add("MapID", this.Addresses.MapIDPtr);
            this.Pointers.Add("MapSector", this.Addresses.MapSectorPtr);
            this.Pointers.Add("AudioStatus", this.Addresses.AudioStatusPtr);
            this.Pointers.Add("GameTime", this.Addresses.GameTimePtr);
            this.Pointers.Add("AliceID", this.Addresses.AliceIDPtr);
            this.Pointers.Add("BandersnatchPhase", this.Addresses.BandersnatchPhasePtr);
            this.Pointers.Add("StayneHealth", this.Addresses.StayneHealthPtr);
            this.Pointers.Add("JabberwockyPhase", this.Addresses.JabberwockyPhasePtr);
            this.Pointers.Add("JabberwockyP4Counter", this.Addresses.JabberwockyPhase4CounterPtr);
        }

        public void UpdatePointerValues()
        {
            foreach (KeyValuePair<string, object> item in Pointers)
            {
                string key = item.Key;
                object value = item.Value;
                switch (key)
                {
                    case "MapID":
                    case "MapSector":
                        Pointer<int> intPtr = (Pointer<int>)value;
                        int previous = this.CurrentValues.ContainsKey(key) ? (int)this.CurrentValues[key] : default;
                        this.PreviousValues[key] = previous;
                        this.CurrentValues[key] = intPtr.Read(this.Proc);
                        break;
                    case "AliceID":
                    case "BandersnatchPhase":
                    case "JabberwockyPhase":
                    case "JabberwockyP4Counter":
                    case "AudioStatus":
                        Pointer<uint> uintPtr = (Pointer<uint>)value;
                        uint previousU = this.CurrentValues.ContainsKey(key) ? (uint)this.CurrentValues[key] : default;
                        this.PreviousValues[key] = previousU;
                        this.CurrentValues[key] = uintPtr.Read(this.Proc);
                        break;
                    case "GameTime":
                    case "StayneHealth":
                        Pointer<float> floatPtr = (Pointer<float>)value;
                        float previousF = this.CurrentValues.ContainsKey(key) ? (float)this.CurrentValues[key] : default;
                        this.PreviousValues[key] = previousF;
                        this.CurrentValues[key] = floatPtr.Read(this.Proc);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
