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
		this.OpCode = new OpCode(
			this.Console.Memory[this.ProgramCounter++],
			this.Console.Memory[this.ProgramCounter++]
		);
		#if DEBUG
			System.Console.Write($"[{this.ProgramCounter:X4}] {this.OpCode} ||\t");
		#endif
		if (this.OpCode == 0x00E0)
			this.ClearScreen();
		else if (this.OpCode == 0x00EE)
			this.Return();
		else
			switch(this.OpCode.UNibble)
			{
				case 0x1: this.Jump(); break;
				case 0x2: this.Call(); break;
				case 0x3: this.SkipEqual(); break;
				case 0x4: this.SkipNotEqual(); break;
				case 0x6: this.LoadX(); break;
				case 0x7: this.AddX(); break;
				case 0x8:
				{
					switch(this.OpCode.LNibble)
					{

						case 0x0: this.LoadXY(); break;
						//case 0x3: this.XorXY(); break;
						//case 0x6: this.ShiftRightXY(); break;
						default: throw new ArgumentException($"Unknown opcode: '{this.OpCode}'");
					}
				} break;
				case 0xA: this.LoadI(); break;
				case 0xC: this.Random(); break;
				case 0xD: this.Draw(); break;
				case 0xE:
				{
					switch(this.OpCode.LByte)
					{
						case 0x9E: this.SkipEqualKey(); break;
						case 0xA1: this.SkipNotEqualKey(); break;
						default: throw new ArgumentException($"Unknown opcode: '{this.OpCode}'");
					}
				} break;
				case 0xF:
				{
					switch(this.OpCode.LByte)
					{
						//case 0x0A: this.LoadKeyX(); break;
						case 0x07: this.LoadXD(); break;
						case 0x15: this.LoadDX(); break;
						case 0x1E: this.AddIX(); break;
						//case 0x55: this.StoreIX(); break;
						case 0x65: this.LoadIX(); break;
						default: throw new ArgumentException($"Unknown opcode: '{this.OpCode}'");
					}
				} break;
				default: throw new ArgumentException($"Unknown opcode: '{this.OpCode}'");
			}
		this.DelayTimer--;
	}
	// Instructions
	// -0x00E0
	private void ClearScreen()
	{
		#if DEBUG
			System.Console.WriteLine("CLS");
		#endif
		for (var x = 0; x < this.Console.GFXMemory.GetLength(0); ++x)
			for (var y = 0; y < this.Console.GFXMemory.GetLength(1); ++y)
				this.Console.GFXMemory[x, y] = false;
	}
	// -0x00EE
	private void Return()
	{
		#if DEBUG
			var initialPC = this.ProgramCounter;
			var initialSP = this.StackPointer;
			var initialS = this.Stack[this.StackPointer - 1];
		#endif
		this.ProgramCounter = this.Stack[--this.StackPointer];
		#if DEBUG
			System.Console.WriteLine($"Return 0x{this.OpCode.Address:X4}, SP: {this.StackPointer:X} (initialSP: {initialSP:X}, initialStack[SP-1] = 0x{initialS:X4}, newStack[SP]: 0x{this.Stack[this.StackPointer]:X4}) | PC = 0x{this.ProgramCounter:X4} (prior: 0x{initialPC:X4})");
		#endif
	}
	// -0x1nnn
	private void Jump()
	{
		this.ProgramCounter = this.OpCode.Address;
		#if DEBUG
			System.Console.WriteLine($"PC = 0x{this.ProgramCounter:X4}");
		#endif
	}
	// -0x2nnn
	public void Call()
	{
		#if DEBUG
			var initialPC = this.ProgramCounter;
			var initialSP = this.StackPointer;
			var initialS = this.Stack[this.StackPointer];
		#endif
		if (this.StackPointer > this.Stack.Length)
			throw new ArgumentOutOfRangeException("Stack overflow");
		this.Stack[this.StackPointer++] = this.ProgramCounter;
		this.ProgramCounter = this.OpCode.Address;
		#if DEBUG
			System.Console.WriteLine($"Call 0x{this.OpCode.Address:X4}, SP: {this.StackPointer:X} (initialSP: {initialSP:X}, initialStack[SP] = 0x{initialS:X4}, newStack[SP]: 0x{this.Stack[this.StackPointer - 1]:X4}) | PC = 0x{this.ProgramCounter:X4} (prior: 0x{initialPC:X4})");
		#endif
	}
	// -0x3xkk
	public void SkipEqual()
	{
		#if DEBUG
			var initial = this.ProgramCounter;
		#endif
		if (this.Registers[this.OpCode.XNibble] == this.OpCode.LByte)
			this.ProgramCounter += 2;
		#if DEBUG
			System.Console.WriteLine($"R[{this.OpCode.XNibble:X}] == 0x{this.OpCode.LByte:X2} (register: 0x{this.Registers[this.OpCode.XNibble]:X2}, iPC: 0x{initial:X4}, cPC: 0x{this.ProgramCounter:X4})");
		#endif
	}
	// -0x4xkk
	public void SkipNotEqual()
	{
		#if DEBUG
			var initial = this.ProgramCounter;
		#endif
		if (this.Registers[this.OpCode.XNibble] != this.OpCode.LByte)
			this.ProgramCounter += 2;
		#if DEBUG
			System.Console.WriteLine($"R[{this.OpCode.XNibble:X}] != 0x{this.OpCode.LByte:X2} (register: 0x{this.Registers[this.OpCode.XNibble]:X2}, iPC: 0x{initial:X4}, cPC: 0x{this.ProgramCounter:X4})");
		#endif
	}
	// -0x6xkk
	private void LoadX()
	{
		#if DEBUG
			var initRX = this.Registers[this.OpCode.XNibble];
		#endif
		this.Registers[this.OpCode.XNibble] = this.OpCode.LByte;
		#if DEBUG
			System.Console.WriteLine($"R[{this.OpCode.XNibble:X}] = 0x{this.Registers[this.OpCode.XNibble]:X2} (Init: 0x{initRX:X2}, Expected: 0x{this.OpCode.LByte:X2})");
		#endif
	}
	// -0x7xkk
	private void AddX()
	{
		#if DEBUG
			var initial = this.Registers[this.OpCode.XNibble];
		#endif
		this.Registers[this.OpCode.XNibble] += this.OpCode.LByte;
		#if DEBUG
			System.Console.WriteLine($"R[{this.OpCode.XNibble:X}] += 0x{this.OpCode.LByte:X2} (Before: 0x{initial:X2} | After: 0x{this.Registers[this.OpCode.XNibble]:X2} | Expected: 0x{initial + this.OpCode.LByte:X2})");
		#endif
	}
	// -0x8xy0
	private void LoadXY()
	{
		#if DEBUG
			var initial = this.Registers[this.OpCode.XNibble];
		#endif
		this.Registers[this.OpCode.XNibble] = this.Registers[this.OpCode.YNibble];
		#if DEBUG
			System.Console.WriteLine($"R[{this.OpCode.XNibble:X}] = R[{this.OpCode.YNibble:X}] (Before: 0x{initial:X2} | After: 0x{this.Registers[this.OpCode.XNibble]:X2} | Expected: 0x{this.Registers[this.OpCode.YNibble]:X2})");
		#endif
	}
	// -0x8xy3
	// -0x8xy6
	// -0xAnnn
	private void LoadI()
	{
		this.AddressPointer = this.OpCode.Address;
		#if DEBUG
			System.Console.WriteLine($"I = 0x{this.AddressPointer:X4}");
		#endif
	}
	// -0xCxkk
	private void Random()
	{
		#if DEBUG
			var initial = this.Registers[this.OpCode.XNibble];
		#endif
		this.Registers[this.OpCode.XNibble] = (byte)(this.Console.Random.Next(256) & this.OpCode.LByte);
		#if DEBUG
			System.Console.WriteLine($"R[{this.OpCode.XNibble:X}] = 0x{this.Registers[this.OpCode.XNibble]:X2} (initial: 0x{initial:X2})");
		#endif
	}
	// -0xDxyk
	private void Draw()
	{
		var rX = this.OpCode.XNibble;
		var rY = this.OpCode.YNibble;
		var height = this.OpCode.LNibble;
		var startX = this.Registers[rX] % this.Console.GFXMemory.GetLength(0); // % 64
		var startY = this.Registers[rY] % this.Console.GFXMemory.GetLength(1); // % 32
		#if DEBUG
			System.Console.Write($"Draw: X=0x{startX:X2}, Y=0x{startY:X2}, Height=0x{height:X2}");
		#endif
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
		#if DEBUG
			System.Console.WriteLine($"R[F] = 0x{this.Registers[0xF]}");
		#endif
	}
	// -0xFx0A
	// -0xFx1E
	private void AddIX()
	{
		#if DEBUG
			var initial = this.AddressPointer;
		#endif
		this.AddressPointer += this.Registers[this.OpCode.XNibble];
		#if DEBUG
			System.Console.WriteLine($"I = 0x{this.AddressPointer:X4} (Before: 0x{initial:X4}, Expected: 0x{initial + this.Registers[this.OpCode.XNibble]:X4})");
		#endif
	}
	// -0xEx9E
	private void SkipEqualKey()
	{
		#if DEBUG
			var initial = this.ProgramCounter;
		#endif
		var keyLocation = this.Registers[this.OpCode.XNibble];
		if (this.Console.Inputs[keyLocation])
			this.ProgramCounter += 2;
		#if DEBUG
			System.Console.WriteLine($"Console.Inputs[{keyLocation:X}] == true (register: 0x{this.Registers[this.OpCode.XNibble]:X2}, input: {this.Console.Inputs[keyLocation]}, iPC: 0x{initial:X4}, cPC: 0x{this.ProgramCounter:X4})");
		#endif
	}
	// -0xExA1
	private void SkipNotEqualKey()
	{
		#if DEBUG
			var initial = this.ProgramCounter;
		#endif
		var keyLocation = this.Registers[this.OpCode.XNibble];
		if (!this.Console.Inputs[keyLocation])
			this.ProgramCounter += 2;
		#if DEBUG
			System.Console.WriteLine($"Console.Inputs[{keyLocation:X}] == false (register: 0x{this.Registers[this.OpCode.XNibble]:X2}, input: {this.Console.Inputs[keyLocation]}, iPC: 0x{initial:X4}, cPC: 0x{this.ProgramCounter:X4})");
		#endif
	}
	// -0xFx07
	private void LoadXD()
	{
		#if DEBUG
			var initial = this.Registers[this.OpCode.XNibble];
		#endif
		this.Registers[this.OpCode.XNibble] = this.DelayTimer;
		#if DEBUG
			System.Console.WriteLine($"R[{this.OpCode.XNibble:X}] = 0x{this.Registers[this.OpCode.XNibble]:X2} (initial: 0x{initial:X2}, expected: 0x{this.DelayTimer:X2})");
		#endif
	}
	// -0xFx15
	private void LoadDX()
	{
		this.DelayTimer = this.Registers[this.OpCode.XNibble];
		#if DEBUG
			System.Console.WriteLine($"Delay = 0x{this.Registers[this.OpCode.XNibble]:X2}");
		#endif
	}
	// -0xFx55
	private void StoreIX()
	{
		for (var i = 0; i < this.OpCode.XNibble; ++i)
		{
			this.Console.Memory[this.AddressPointer + i] = this.Registers[i];
			#if DEBUG
				var command = $"R[{i:X}] = Console.Memory[0x{this.AddressPointer + i:X4}]";
				if (i < this.OpCode.XNibble - 1)
					System.Console.Write(command + ", ");
				else
					System.Console.WriteLine(command);
			#endif
		}
	}
	// -0xFx65
	private void LoadIX()
	{
		for (var i = 0; i <= this.OpCode.XNibble; ++i)
		{
			#if DEBUG
				var initial = this.Registers[i];
			#endif
			this.Registers[i] = this.Console.Memory[this.AddressPointer + i];
			#if DEBUG
				var command = $"R[{i:X}] = Console.Memory[0x{this.AddressPointer + i:X4}] (previous: 0x{initial:X2}, new: 0x{this.Registers[i]:X2}, expected: 0x{this.Console.Memory[this.AddressPointer + i]:X2})";
				if (this.OpCode.XNibble == 0 || i == this.OpCode.XNibble - 1)
					System.Console.WriteLine(command);
				else
					System.Console.Write(command + ", ");
			#endif
		}
	}
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
