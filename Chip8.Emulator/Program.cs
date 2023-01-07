/*
    Chip8 Emulator

    Written By: Ryan Smith
*/
using System;
using Chip8 = Emulators.Chip8;

internal class Program
{
    public static void Main(string[] args)
    {
        Chip8::Console chip8 = new Chip8::Console(new Canvas());
        chip8.LoadROM(args[0]);
        for (int i = 0; i <= 500; i++)
        {
            // DEBUG: Initial
            string debugString = $"[Counter:{i} | PC: 0x{chip8._CPU.ProgramCounter.ToString("X4")}]"; // DEBUG: Program Counter
            debugString += "OpCode: 0x" + ((chip8.RAM[chip8._CPU.ProgramCounter] << 8) | chip8.RAM[chip8._CPU.ProgramCounter + 1]).ToString("X4") + "\n"; //DEBUG: OpCode
            // DEBUG: Stack - Create
            string debugStackString = "Stack = [";
            for (var j = 0; j < chip8._CPU.Stack.Length; ++j)
            {
                if (chip8._CPU.Stack[j] == 0)
                    break;
                debugStackString += $"0x{chip8._CPU.Stack[j].ToString("X4")}";
                if (j < chip8._CPU.StackPointer - 1)
                    debugStackString += ", ";
            }
            debugStackString += "]";
            // Advance CPU
            chip8.Tick();
            if (i == 236)
                chip8.SetKey(0xD, false);
            // DEBUG: Append
            debugString += $"\tI = 0x{chip8._CPU.AddressPointer.ToString("X4")}\n"; // DEBUG: Address Pointer
            // DEBUG: Stack - Append
            debugString += $"\t{debugStackString}\n";
            // DEBUG: Registers
            for (var j = 0; j < chip8._CPU.Registers.Length; ++j)
            {
                debugString += $"\tV[{j.ToString("X")}] = 0x{chip8._CPU.Registers[j].ToString("X2")}";
                if (j == chip8._CPU.Registers.Length - 1 || (j + 1) % 4 == 0)
                    debugString += "\n";
                else
                    debugString += "\t";
            }
            // DEBUG: Memory[I]
            ushort memoryDumpSize = 0x000F;
            debugString += $"\tMemory[I+{memoryDumpSize}] = [";
            for (var j = 0; j < memoryDumpSize; ++j)
            {
                debugString += $"0x{chip8.RAM[chip8._CPU.AddressPointer + j].ToString("X2")}";
                if (j < memoryDumpSize - 1)
                    debugString += ", ";
            }
            debugString += "]\n";
            // DEBUG: Inputs
            debugString += "\tInputs = [";
            for (var j = 0; j < chip8.Inputs.Length; ++j)
            {
                debugString += $"{chip8.Inputs[j]}";
                if (j < chip8.Inputs.Length - 1)
                    debugString += ", ";
            }
            debugString += "]\n";
            // DEBUG: Print
            Console.WriteLine(debugString);
        }
    }
}

internal class Canvas : Chip8::Display
{
    /* Instance Methods */
    public override void Draw() => Console.WriteLine("Canvas.Draw()");
}
