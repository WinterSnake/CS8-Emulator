/*
    Chip8 Emulator: Hardware Implementations
    Chip8 CPU

    Written By: Ryan Smith
*/
#pragma warning disable CS0660, CS0661
using System;

namespace Emulators.Chip8
{
    internal struct OpCode
    {
        /* Constructor */
        public OpCode(ushort instruction) { this.Instruction = instruction; }
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
        public byte UNibble   { get { return (byte)  ((this.Instruction & 0xF000) >> 12); } }
        public byte XNibble   { get { return (byte)  ((this.Instruction & 0x0F00) >>  8); } }
        public byte YNibble   { get { return (byte)  ((this.Instruction & 0x00F0) >>  4); } }
        public byte LNibble   { get { return (byte)  ((this.Instruction & 0x000F) >>  0); } }
    }

    public class CPU
    {
        /* Constructors */
        public CPU(ushort startAddress = 0x200)
        {
            this.ProgramCounter = startAddress;
        }
        /* Instance Methods */
        public bool Tick(byte[] memory, bool[,] display, bool[] inputs)
        {
            bool drawFlag = false;
            ushort instruction = (ushort)((memory[this.ProgramCounter] << 8) + memory[this.ProgramCounter + 1]);
            OpCode opcode = new OpCode(instruction);
            // 00E0: clear
            if (opcode == 0x00E0)
            {
                drawFlag = true;
                for (var x = 0; x < display.GetLength(0); ++x)
                {
                    for (var y = 0; y < display.GetLength(1); ++y)
                        display[x, y] = false;
                }
            }
            // 00EE: return
            else if (opcode == 0x00EE)
            {
                // Handle stack underflow
                if (this.StackPointer <= 0)
                    throw new InvalidOperationException();
                this.ProgramCounter = this.Stack[--this.StackPointer];
                this.Stack[this.StackPointer] = 0;
            }
            // 1nnn: goto nnn
            else if (opcode.UNibble == 0x1)
                this.ProgramCounter = (ushort)(opcode.Address - 2);
            // 2nnn: call nnn
            else if (opcode.UNibble == 0x2)
            {
                // Handle stack overflow
                if (this.StackPointer >= this.Stack.Length)
                    throw new StackOverflowException();
                this.Stack[this.StackPointer++] = this.ProgramCounter;
                this.ProgramCounter = (ushort)(opcode.Address - 2);
            }
            // 3xnn: Vx == nn
            else if (opcode.UNibble == 0x3)
            {
                if (this.Registers[opcode.XNibble] == opcode.LowerByte)
                    this.ProgramCounter += 2;
            }
            // 4xnn: Vx != nn
            else if (opcode.UNibble == 0x4)
            {
                if (this.Registers[opcode.XNibble] != opcode.LowerByte)
                    this.ProgramCounter += 2;
            }
            // 6xnn: Vx = nn
            else if (opcode.UNibble == 0x6)
                this.Registers[opcode.XNibble] = opcode.LowerByte;
            // 7xnn: Vx += nn
            else if (opcode.UNibble == 0x7)
                this.Registers[opcode.XNibble] += opcode.LowerByte;
            // 8xy0: Vx = Vy
            else if (opcode.UNibble == 0x8 && opcode.LNibble == 0x0)
                this.Registers[opcode.XNibble] = this.Registers[opcode.YNibble];
            // 8xy2: Vx &= Vy
            else if (opcode.UNibble == 0x8 && opcode.LNibble == 0x2)
                this.Registers[opcode.XNibble] &= this.Registers[opcode.YNibble];
            else if (opcode.UNibble == 0x9 && opcode.LNibble == 0x0)
            {
                if (this.Registers[opcode.XNibble] != this.Registers[opcode.YNibble])
                    this.ProgramCounter += 2;
            }
            // Annn: I = NNN
            else if (opcode.UNibble == 0xA)
                this.AddressPointer = opcode.Address;
            // TODO: Dxyn: Display x=Vx ; y=Vy; width=8 ; height = n
            else if (opcode.UNibble == 0xD) {}
            // Ex9E: if (key[x])
            else if (opcode.UNibble == 0xE && opcode.LowerByte == 0x9E)
            {
                if (inputs[opcode.XNibble])
                    this.ProgramCounter += 2;
            }
            // Ex9E: if !(key[x])
            else if (opcode.UNibble == 0xE && opcode.LowerByte == 0xA1)
            {
                if (!inputs[opcode.XNibble])
                    this.ProgramCounter += 2;
            }
            // Fx1E: I += Vx
            else if (opcode.UNibble == 0xF && opcode.LowerByte == 0x1E)
                this.AddressPointer += this.Registers[opcode.XNibble];
            // Fx29: I = FontAddress[Vx]
            else if (opcode.UNibble == 0xF && opcode.LowerByte == 0x29)
                this.AddressPointer = (ushort)(this.Registers[opcode.XNibble] * 0x5);
            // Fx55: I = Vx [where 0 : x] ; I++
            else if (opcode.UNibble == 0xF && opcode.LowerByte == 0x55)
            {
                for (uint i = 0; i <= opcode.XNibble; i++)
                    memory[this.AddressPointer + i] = this.Registers[i];
            }
            // Fx65: Vx = I [where 0 : x] ; I++
            else if (opcode.UNibble == 0xF && opcode.LowerByte == 0x65)
            {
                for (uint i = 0; i < opcode.XNibble; i++)
                    this.Registers[i] = memory[this.AddressPointer++];
            }
            else
            {
                throw new NotSupportedException($"Unknown OpCode found: [PC=0x{this.ProgramCounter.ToString("X4")}]: 0x{opcode}");
            }
            this.ProgramCounter += 2;
            return drawFlag;
        }
        /* Properties */
        public ushort ProgramCounter { get; private set; } = 0;
        public byte[] Registers { get; private set; } = new byte[16];
        public ushort AddressPointer { get; private set; } = 0;
        public byte StackPointer { get; private set; } = 0;
        public ushort[] Stack { get; private set; } = new ushort[16];
        public bool DrawFlag { get; private set; } = false;
    }
}
