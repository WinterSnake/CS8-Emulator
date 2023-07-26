/*
    Chip8 Disassembler

    Written By: Ryan Smith
*/
#pragma warning disable CS0660, CS0661

using System;
using System.Collections.Generic;
using System.IO;

internal class Program
{
    public static void Main(string[] args)
    {
        // Error Handling: File
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: ./Chip8.NCurses <rom.ch8> [flags]");
            return;
        }
        if (!File.Exists(args[0]) || File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
        {
            Console.Error.WriteLine($"File '{args[0]}' does not exist or is not a file.");
            return;
        }
        // Load ROM
        byte[] bin = File.ReadAllBytes(args[0]);
        List<string> output = new List<string>();
        // Decode ROM
        for (var i = 0; i < bin.Length; i += 2)
        {
            var opcode = new OpCode(bin[i], bin[i + 1]);
            Console.WriteLine($"0x{opcode}");
        }
        // Output Assembly
    }
}

internal struct OpCode
{
    /* Constructor */
    public OpCode(byte upperByte, byte lowerByte) { this.Instruction = (ushort)((upperByte << 8) | lowerByte); }
    /* Instance Methods */
    public override string ToString() => this.Instruction.ToString("X4");
    /* Static Methods */
    public static bool operator == (OpCode opcode, ushort _value) => opcode.Instruction == _value;
    public static bool operator != (OpCode opcode, ushort _value) => opcode.Instruction != _value;
    /* Properties */
    public readonly ushort Instruction;
    public ushort Address { get { return (ushort)((this.Instruction & 0x0FFF) >>  0); } }
    public byte UpperByte { get { return (byte)  ((this.Instruction & 0xFF00) >>  8); } }
    public byte LowerByte { get { return (byte)  ((this.Instruction & 0x00FF) >>  0); } }
    public byte   UNibble { get { return (byte)  ((this.Instruction & 0xF000) >> 12); } }
    public byte   XNibble { get { return (byte)  ((this.Instruction & 0x0F00) >>  8); } }
    public byte   YNibble { get { return (byte)  ((this.Instruction & 0x00F0) >>  4); } }
    public byte   LNibble { get { return (byte)  ((this.Instruction & 0x000F) >>  0); } }
}
