/*
	Chip8 Emulator: Hardware
	- Console

	Written By: Ryan Smith
*/
using System;
using System.IO;

namespace Emulators.Chip8.Hardware;

public class Console
{
	/* Constructors */
	public Console(ushort startAddress = 0x0200)
	{
		this.CPU = new CPU(this, startAddress);
		this._StartAddress = startAddress;
		Array.Copy(Console.FONTSET, this.Memory, Console.FONTSET.Length);
	}
	/* Instance Methods */
	public void LoadROM(string filePath)
	{
		using var stream = File.Open(filePath, FileMode.Open);
		int bytesRead = 0;
		while (bytesRead < stream.Length)
			bytesRead = stream.Read(this.Memory, this.CPU.ProgramCounter + bytesRead, (int)(stream.Length - bytesRead));
	}
	public void Reset() => this.CPU.Reset(this._StartAddress);
	public void Tick() => this.CPU.Tick();
	/* Properties */
	public readonly CPU CPU;
	public readonly bool[] Inputs = new bool[16];
	public readonly byte[]  Memory = new byte[4096];
    public readonly bool[,] GFXMemory = new bool[64, 32];
	internal readonly Random Random = new Random();
	private ushort _StartAddress;
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
}
