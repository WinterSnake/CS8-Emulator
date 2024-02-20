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
	public Console(ushort programCounter = 0x200)
	{
		this.CPU = new CPU(this, programCounter);
	}
	/* Instance Methods */
	public void LoadROM(string filePath)
	{
		using var stream = File.Open(filePath, FileMode.Open);
		int bytesRead = 0;
		while (bytesRead < stream.Length)
			bytesRead = stream.Read(this.Memory, this.CPU.ProgramCounter + bytesRead, (int)(stream.Length - bytesRead));
	}
	public void Tick() => this.CPU.Tick();
	/* Properties */
	public readonly CPU CPU;
	public readonly byte[]  Memory = new byte[4096];
    public readonly bool[,] GFXMemory = new bool[64, 32];
}
