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
		if (this.SoundTimer > 0)
			this.SoundTimer--;
		// Instruction handling
		this.OpCode = new OpCode(
			this.Console.Memory[this.ProgramCounter++],
			this.Console.Memory[this.ProgramCounter++]
		);
		System.Console.WriteLine($"[{this.ProgramCounter - 2:X4}]: {this.OpCode}");
		switch(this.OpCode.UNibble)
		{
			case 0x0: this.OpCode0___(); break;
			case 0x1: this.OpCode1nnn(); break;  // Jump
			case 0x2: this.OpCode2nnn(); break;  // Call
			case 0x3: this.OpCode3xkk(); break;  // Skip > v[X] ==   kk
			case 0x4: this.OpCode4xkk(); break;  // Skip > v[X] !=   kk
			case 0x5: this.OpCode5xy0(); break;  // Skip > v[X] == v[Y]
			case 0x6: this.OpCode6xkk(); break;  // v[X] = kk
			case 0x7: this.OpCode7xkk(); break;  // x[X] += kk
			case 0x8: this.OpCode8___(); break;
			case 0x9: this.OpCode9xy0(); break;  // Skip > v[X] != v[Y]
			case 0xA: this.OpCodeAnnn(); break;  // I = nnn
			case 0xC: this.OpCodeCxkk(); break;  // v[X] = RND & kk
			case 0xD: this.OpCodeDxyn(); break;  // Draw
			case 0xE: this.OpCodeE___(); break;
			case 0xF: this.OpCodeF___(); break;
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	// Instructions
	private void OpCode0___()
	{
		switch (this.OpCode.Address)
		{
			case 0x0E0: this.OpCode00E0(); break;  // Clear Screen
			case 0x0EE: this.OpCode00EE(); break;  // Return
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	private void OpCode00E0()
	{
		for (var x = 0; x < this.Console.GFXMemory.GetLength(0); ++x)
			for (var y = 0; y < this.Console.GFXMemory.GetLength(1); ++y)
				this.Console.GFXMemory[x, y] = false;
	}
	private void OpCode00EE()
	{
		if (this.StackPointer <= 0)
			throw new ArgumentOutOfRangeException("Chip8 CPU Stack Underflow");
		this.ProgramCounter = this.Stack[--this.StackPointer];
	}
	private void OpCode1nnn() => this.ProgramCounter = this.OpCode.Address;
	private void OpCode2nnn()
	{
		if (this.StackPointer > this.Stack.Length)
			throw new ArgumentOutOfRangeException("Chip8 CPU Stack Overflow");
		this.Stack[this.StackPointer++] = this.ProgramCounter;
		this.ProgramCounter = this.OpCode.Address;
	}
	private void OpCode3xkk() => this.ProgramCounter += (ushort)(this.Registers[this.OpCode.XNibble] == this.OpCode.LByte ? 2 : 0);
	private void OpCode4xkk() => this.ProgramCounter += (ushort)(this.Registers[this.OpCode.XNibble] != this.OpCode.LByte ? 2 : 0);
	private void OpCode5xy0() => this.ProgramCounter += (ushort)(this.Registers[this.OpCode.XNibble] == this.Registers[this.OpCode.YNibble] ? 2 : 0);
	private void OpCode6xkk() => this.Registers[this.OpCode.XNibble] = this.OpCode.LByte;
	private void OpCode7xkk() => this.Registers[this.OpCode.XNibble] += this.OpCode.LByte;
	private void OpCode8___()
	{
		switch (this.OpCode.LNibble)
		{
			case 0x0: this.OpCode8xy0(); break; // v[X]  = v[Y]
			case 0x1: this.OpCode8xy1(); break; // v[X] |= v[Y]
			case 0x2: this.OpCode8xy2(); break; // v[X] &= v[Y]
			case 0x3: this.OpCode8xy3(); break; // v[X] ^= v[Y]
			case 0x4: this.OpCode8xy4(); break; // v[X] += v[Y]
			case 0x5: this.OpCode8xy5(); break; // v[X] -= v[Y]
			case 0x6: this.OpCode8xy6(); break; // v[X] = v[Y] >> 1
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	private void OpCode8xy0() => this.Registers[this.OpCode.XNibble]  = this.Registers[this.OpCode.YNibble];
	private void OpCode8xy1() => this.Registers[this.OpCode.XNibble] |= this.Registers[this.OpCode.YNibble];
	private void OpCode8xy2() => this.Registers[this.OpCode.XNibble] &= this.Registers[this.OpCode.YNibble];
	private void OpCode8xy3() => this.Registers[this.OpCode.XNibble] ^= this.Registers[this.OpCode.YNibble];
	private void OpCode8xy4()
	{
		this.Registers[0xF] = 0;
		var rX = this.Registers[this.OpCode.XNibble];
		var rY = this.Registers[this.OpCode.YNibble];
		if (rY > 0xFF - rX)
			this.Registers[0xF] = 1;
		this.Registers[this.OpCode.XNibble] += rY;
	}
	private void OpCode8xy5()
	{
		var rX = this.Registers[this.OpCode.XNibble];
		var rY = this.Registers[this.OpCode.YNibble];
		this.Registers[0xF] = (byte)(rX > rY ? 1 : 0);
		this.Registers[this.OpCode.XNibble] -= rY;
	}
	private void OpCode8xy6()
	{
		var rY = this.Registers[this.OpCode.YNibble];
		this.Registers[this.OpCode.XNibble] = rY;
		this.Registers[0xF] = (byte)(rY & 0b00000001);
		this.Registers[this.OpCode.XNibble] >>= 1;
	}
	private void OpCode9xy0() => this.ProgramCounter += (ushort)(this.Registers[this.OpCode.XNibble] != this.Registers[this.OpCode.YNibble] ? 2 : 0);
	private void OpCodeAnnn() => this.AddressPointer = this.OpCode.Address;
	private void OpCodeCxkk() => this.Registers[this.OpCode.XNibble] = (byte)(this.Console.Random.Next(256) & this.OpCode.LByte);
	private void OpCodeDxyn()
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
	private void OpCodeE___()
	{
		switch (this.OpCode.LByte)
		{
			case 0x9E: this.OpCodeExA1(); break; // Skip >  CONSOLE.INPUTS[v[X]]
			case 0xA1: this.OpCodeExA1(); break; // Skip > !CONSOLE.INPUTS[v[X]]
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	private void OpCodeEx9E()
	{
		var keyLocation = this.Registers[this.OpCode.XNibble];
		if (this.Console.Inputs[keyLocation])
			this.ProgramCounter += 2;
	}
	private void OpCodeExA1()
	{
		var keyLocation = this.Registers[this.OpCode.XNibble];
		if (!this.Console.Inputs[keyLocation])
			this.ProgramCounter += 2;
	}
	private void OpCodeF___()
	{
		switch (this.OpCode.LByte)
		{
			case 0x07: this.OpCodeFx07(); break;  // v[X] = Delay
			case 0x15: this.OpCodeFx15(); break;  // Delay = v[X]
			case 0x1E: this.OpCodeFx1E(); break;  // I += v[X]
			case 0x29: this.OpCodeFx29(); break;  // I = font[X]
			case 0x33: this.OpCodeFx33(); break;  // I[0..2] = BCD(v[X])
			default: throw new ArgumentException($"Unknown opcode '{this.OpCode}'");
		}
	}
	private void OpCodeFx07() => this.Registers[this.OpCode.XNibble] = this.DelayTimer;
	private void OpCodeFx15() => this.DelayTimer = this.Registers[this.OpCode.XNibble];
	private void OpCodeFx1E() => this.AddressPointer += this.Registers[this.OpCode.XNibble];
	private void OpCodeFx29() => this.AddressPointer = (ushort)(this.Registers[this.OpCode.XNibble] * 5);
	private void OpCodeFx33()
	{
		var rX = this.Registers[this.OpCode.XNibble];
		this.Console.Memory[this.AddressPointer + 0] = (byte)(rX / 100);
		this.Console.Memory[this.AddressPointer + 1] = (byte)(rX / 10 % 10);
		this.Console.Memory[this.AddressPointer + 2] = (byte)(rX % 10);
	}
	/* Properties */
	public ushort ProgramCounter { get; private set; }
	public readonly byte[] Registers = new byte[16];
    public ushort AddressPointer { get; private set; } = 0;
	public byte StackPointer { get; private set; } = 0;
	public readonly ushort[] Stack = new ushort[16];
	public byte DelayTimer { get; private set; } = 0;
	public byte SoundTimer { get; private set; } = 0;
	private OpCode OpCode;
	private readonly Console Console;
}
