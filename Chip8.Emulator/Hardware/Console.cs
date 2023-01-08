/*
    Chip8 Emulator: Hardware Implementation
    - Console    

    Written By: Ryan Smith
*/

using System.IO;

namespace Emulators.Chip8;

public class Console
{
    /* Constructors */
    public Console(ushort romStartAddress = 0x200)
    {
        this._ROMStartAddress = romStartAddress;
        this.CPU = new CPU(romStartAddress);
        // Write FONTSET to Memory
        for (var i = 0; i < Console.FONTSET.Length; ++i)
            this.Memory[i] = Console.FONTSET[i];
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
            this.Memory[this._ROMStartAddress + stream.Position - 1] = (byte)_value;
    }
    public void Reset(bool resetROM = false)
    {
        this.CPU.Reset(this._ROMStartAddress);
        // Inputs
        for (var i = 0; i < this.Inputs.Length; ++i)
            this.Inputs[i] = false;
        // Memory
        if (!resetROM)
            return;
        for (var i = this._ROMStartAddress; i < this.Memory.Length; ++i)
            this.Memory[i] = 0;
    }
    public void SetKey(byte position, bool flag = false) => this.Inputs[position] = flag;
    public void Tick()
    {
        this.CPU.Tick(this.Memory, this.GFXBuffer, this.Inputs);
    }
    /* Static Properties */
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
    private readonly ushort _ROMStartAddress;
    public readonly CPU CPU;
    public readonly byte[] Memory = new byte[4096];
    public readonly byte[,] GFXBuffer = new byte[64, 32];
    public readonly bool[] Inputs = new bool[16];
}
