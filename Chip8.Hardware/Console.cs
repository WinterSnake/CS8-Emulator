/*
	Chip8 Emulator: Assembly
	- OpCode

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
	public bool[] Inputs = new bool[16];
	public readonly byte[]  Memory = new byte[4096];
    public readonly bool[,] GFXMemory = new bool[64, 32];
	public readonly Random Random = new Random();
	private ushort _StartAddress;
}
