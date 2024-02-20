/*
	Chip8 Emulator: Assembly
	- OpCode

	Written By: Ryan Smith
*/
using System;

namespace Emulators.Chip8.Assembly;

public struct OpCode
{
	/* Constructors */
	public OpCode(ushort instruction) { this.Instruction = instruction; }
	public OpCode(byte upper, byte lower) { this.Instruction = (ushort)((upper << 8) | (lower << 0)); }
	/* Instance Methods */
	public override string ToString() => $"0x{this.Instruction:X4}";
	public override bool Equals(object o) => false;
	public override int GetHashCode() => this.Instruction.GetHashCode();
	/* Static Methods */
	public static bool operator == (OpCode op, ushort @value) => op.Instruction == @value;
	public static bool operator != (OpCode op, ushort @value) => op.Instruction != @value;
	/* Properties */
	public readonly ushort Instruction;
	public ushort Address { get { return (ushort)((this.Instruction & 0x0FFF) >>  0); }}
	public byte UByte     { get { return   (byte)((this.Instruction & 0xFF00) >>  8); }}
	public byte LByte     { get { return   (byte)((this.Instruction & 0x00FF) >>  0); }}
	public byte UNibble   { get { return   (byte)((this.Instruction & 0xF000) >> 12); }}
	public byte XNibble   { get { return   (byte)((this.Instruction & 0x0F00) >>  8); }}
	public byte YNibble   { get { return   (byte)((this.Instruction & 0x00F0) >>  4); }}
	public byte LNibble   { get { return   (byte)((this.Instruction & 0x000F) >>  0); }}
}
