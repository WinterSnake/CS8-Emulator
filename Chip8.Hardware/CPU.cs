/*
	Chip8 Emulator: Hardware
	- CPU

	Written By: Ryan Smith
*/
using System;
using Emulators.Chip8.Assembly;

namespace Emulators.Chip8.Hardware;

public class CPU
{
	/* Constructor */
	public CPU(Console console, ushort programCounter = 0x0200)
	{
		this.Console = console;
		this.ProgramCounter = programCounter;
	}
	/* Instance Methods */
	public void Reset(ushort programCounter = 0x0200)
	{
		this.ProgramCounter = programCounter;
		this.AddressPointer = 0x0000;
		for (var i = 0; i < this.Registers.Length; ++i)
			this.Registers[i] = 0;
	}
	public void Tick()
	{
		if (this.DelayTimer > 0)
			this.DelayTimer--;
		this.OpCode = new OpCode(
			this.Console.Memory[this.ProgramCounter++],
			this.Console.Memory[this.ProgramCounter++]
		);
		switch(this.OpCode.UNibble)
		{
			case 0x0: this.OpCode0___(); break;
			case 0x1: this.Jump(); break;
			case 0x2: this.Call(); break;
			case 0x3: this.SkipEqual(); break;
			case 0x4: this.SkipNotEqual(); break;
			case 0x6: this.LoadXValue(); break;
			case 0x7: this.AddXValue(); break;
			case 0x8: this.OpCode8___(); break;
			case 0xA: this.LoadI(); break;
			case 0xC: this.Random(); break;
			case 0xD: this.Draw(); break;
			case 0xE: this.OpCodeE___(); break;
			case 0xF: this.OpCodeF___(); break;
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	// Instructions
	// -0x0___
	private void OpCode0___()
	{
		switch (this.OpCode.Address)
		{
			case 0x0E0: this.ClearScreen(); break;
			case 0x0EE: this.Return(); break;
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	// -0x00E0
	private void ClearScreen()
	{
		for (var x = 0; x < this.Console.GFXMemory.GetLength(0); ++x)
			for (var y = 0; y < this.Console.GFXMemory.GetLength(1); ++y)
				this.Console.GFXMemory[x, y] = false;
	}
	// -0x00EE
	private void Return()
	{
		if (this.StackPointer <= 0)
			throw new ArgumentOutOfRangeException("Chip8 CPU Stack Underflow");
		this.ProgramCounter = this.Stack[--this.StackPointer];
	}
	// -0x1nnn
	private void Jump() => this.ProgramCounter = this.OpCode.Address;
	// -0x2nnn
	public void Call()
	{
		if (this.StackPointer > this.Stack.Length)
			throw new ArgumentOutOfRangeException("Chip8 CPU Stack Overflow");
		this.Stack[this.StackPointer++] = this.ProgramCounter;
		this.ProgramCounter = this.OpCode.Address;
	}
	// -0x3xkk
	public void SkipEqual() => this.ProgramCounter += (ushort)(this.Registers[this.OpCode.XNibble] == this.OpCode.LByte ? 2 : 0);
	// -0x4xkk
	public void SkipNotEqual() => this.ProgramCounter += (ushort)(this.Registers[this.OpCode.XNibble] != this.OpCode.LByte ? 2 : 0);
	// -0x6xkk
	private void LoadXValue() => this.Registers[this.OpCode.XNibble] = this.OpCode.LByte;
	// -0x7xkk
	private void AddXValue() => this.Registers[this.OpCode.XNibble] += this.OpCode.LByte;
	// -0x8___
	private void OpCode8___()
	{
		switch (this.OpCode.LNibble)
		{
			case 0x0: this.LoadXY(); break;
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	// -0x8xy0
	private void LoadXY() => this.Registers[this.OpCode.XNibble] = this.Registers[this.OpCode.YNibble];
	// -0xAnnn
	private void LoadI() => this.AddressPointer = this.OpCode.Address;
	// -0xCxkk
	private void Random() => this.Registers[this.OpCode.XNibble] = (byte)(this.Console.Random.Next(256) & this.OpCode.LByte);
	// -0xDxyk
	private void Draw()
	{
		this.Registers[0xF] = 0;
		var height = this.OpCode.LNibble;
		var startX = this.Registers[this.OpCode.XNibble] % this.Console.GFXMemory.GetLength(0); // % 64
		var startY = this.Registers[this.OpCode.YNibble] % this.Console.GFXMemory.GetLength(1); // % 32
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
	// -0xE___
	private void OpCodeE___()
	{
		switch (this.OpCode.LByte)
		{
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	// -0xF___
	private void OpCodeF___()
	{
		switch (this.OpCode.LByte)
		{
			case 0x07: this.LoadXD(); break;
			case 0x15: this.LoadDX(); break;
			case 0x1E: this.AddIX(); break;
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	// -0xFx07
	private void LoadXD() => this.Registers[this.OpCode.XNibble] = this.DelayTimer;
	// -0xFx15
	private void LoadDX() => this.DelayTimer = this.Registers[this.OpCode.XNibble];
	// -0xFx1E
	private void AddIX() => this.AddressPointer += this.Registers[this.OpCode.XNibble];
	/* Properties */
	public ushort ProgramCounter { get; private set; }
	public readonly byte[] Registers = new byte[16];
    public ushort AddressPointer { get; private set; } = 0;
	public byte StackPointer { get; private set; } = 0;
	public readonly ushort[] Stack = new ushort[16];
	public byte DelayTimer { get; private set; } = 0;
	private OpCode OpCode;
	private readonly Console Console;
}
