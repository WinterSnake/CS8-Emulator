/*
	Chip8 Emulator: Assembly
	- CPU

	Written By: Ryan Smith
*/
using System;
using Emulators.Chip8.Assembly;

namespace Emulators.Chip8.Hardware;

public class CPU
{
	/* Constructor */
	public CPU(Console console, ushort programCounter = 0x200)
	{
		this.Console = console;
		this.ProgramCounter = programCounter;
	}
	/* Instance Methods */
	public void Tick()
	{
		this.OpCode = new OpCode(
			this.Console.Memory[this.ProgramCounter++],
			this.Console.Memory[this.ProgramCounter++]
		);
		if (this.OpCode == 0x00E0)
			this.ClearScreen();
		else
			switch(this.OpCode.UNibble)
			{
				case 0x1: this.Jump(); break;
				case 0x3: this.SkipEqual(); break;
				case 0x6: this.LoadX(); break;
				case 0x7: this.AddX(); break;
				case 0xA: this.LoadI(); break;
				case 0xD: this.Draw(); break;
				case 0xF:
				{
					switch(this.OpCode.LByte)
					{
						case 0x1E: this.AddIX(); break;
						default: throw new ArgumentException($"Unknown opcode: '{this.OpCode}'");
					}
				} break;
				default: throw new ArgumentException($"Unknown opcode: '{this.OpCode}'");
			}
	}
	// Instructions
	// -0x00E0
	private void ClearScreen()
	{
		for (var x = 0; x < this.Console.GFXMemory.GetLength(0); ++x)
			for (var y = 0; y < this.Console.GFXMemory.GetLength(1); ++y)
				this.Console.GFXMemory[x, y] = false;
	}
	// -0x1nnn
	private void Jump() => this.ProgramCounter = this.OpCode.Address;
	// -0x3xkk
	private void SkipEqual()
	{
		if (this.Registers[this.OpCode.XNibble] == this.OpCode.LByte)
			this.ProgramCounter += 2;
	}
	// -0x6xkk
	private void LoadX() => this.Registers[this.OpCode.XNibble] = this.OpCode.LByte;
	// -0x7xkk
	private void AddX() => this.Registers[this.OpCode.XNibble] += this.OpCode.LByte;
	// -0xAnnn
	private void LoadI() => this.AddressPointer = this.OpCode.Address;
	// -0xDxyk
	private void Draw()
	{
		var rX = this.OpCode.XNibble;
		var rY = this.OpCode.YNibble;
		var height = this.OpCode.LNibble;
		var startX = this.Registers[rX] % this.Console.GFXMemory.GetLength(0); // % 64
		var startY = this.Registers[rY] % this.Console.GFXMemory.GetLength(1); // % 32
		this.Registers[0xF] = 0;
		for (var y = 0; y < height; ++y)
		{
			var posY = (byte)(startY + y);
			var spriteData = this.Console.Memory[this.AddressPointer + y];
			for (var x = 0; x < 8; ++x)
			{
				var posX = (byte)(startX + x);
				if ((spriteData & (0b10000000 >> x)) == 0) continue;
				if (this.Console.GFXMemory[posX, posY])
					this.Registers[0xF] = 1;
				this.Console.GFXMemory[posX, posY] = !this.Console.GFXMemory[posX, posY];
			}
		}
	}
	// -0xFx1E
	private void AddIX() => this.AddressPointer += this.Registers[this.OpCode.XNibble];
	/* Properties */
	public ushort ProgramCounter { get; private set; }
	public readonly byte[] Registers = new byte[16];
    public ushort AddressPointer { get; private set; } = 0;
	private OpCode OpCode;
	private readonly Console Console;
}
