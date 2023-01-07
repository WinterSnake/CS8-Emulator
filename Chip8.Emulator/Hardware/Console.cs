/*
    Chip8 Emulator: Hardware Implementation
    - Console

    Written By: Ryan Smith
*/
using System;
using System.IO;

namespace Emulators.Chip8;

public class Console
{
    /* Constructors */
    public Console(Display display, ushort romStartAddress = 0x200)
    {
        this._ROMSTART = romStartAddress;
        this._CPU = new CPU(romStartAddress);
        this._Display = display;
        // Write FONTSET to RAM
        for (var i = 0; i < Console.FONTSET.Length; ++i)
            this.RAM[i] = Console.FONTSET[i];
    }
    /* Instance Methods */
    public void LoadROM(byte[] bytes)
    {
        using (var stream = new MemoryStream(bytes))
            this.LoadROM(stream);
    }
    public void LoadROM(string file_path)
    {
        using (var stream = File.Open(file_path, FileMode.Open))
            this.LoadROM(stream);
    }
    public void LoadROM(Stream stream)
    {
        int _value;
        while ((_value = stream.ReadByte()) >= 0)
            this.RAM[this._ROMSTART + stream.Position - 1] = (byte)_value;
    }
    public void SetKey(byte position, bool _set) => this.Inputs[position] = _set;
    public void Tick()
    {
        bool drawFlag = this._CPU.Tick(this.RAM, this._Display.Buffer, this.Inputs);
        if (drawFlag)
            this._Display.Draw();
    }
    /* Class Properties */
    private readonly static byte[] FONTSET = {
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    };
    /* Properties */
    private readonly ushort _ROMSTART;
    public readonly CPU _CPU;
    public readonly Display _Display;
    public readonly byte[] RAM = new byte[4096];
    public readonly bool[] Inputs = new bool[16];
}
