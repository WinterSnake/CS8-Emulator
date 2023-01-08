/*
    Chip8 Emulator
    - Frontend: Ncurses

    Written By: Ryan Smith
*/
#pragma warning disable CA1416

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Sharpie;
using Sharpie.Abstractions;
using Sharpie.Backend;
using Chip8 = Emulators.Chip8;

internal class Program
{
    public static void Main(string[] args)
    {
        // Error Handling: File
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: ./Chip8.NCurses <rom.ch8> [flags]");
            return;
        }
        if (!File.Exists(args[0]) || File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
        {
            Console.Error.WriteLine($"File '{args[0]}' does not exist or is not a file.");
            return;
        }
        // TODO: Parse additional arguments (single-step, debug)
        bool singleStep = false;
        bool drawCPU = true;
        // Setup ncurses
        using var terminal = new Terminal(
            CursesBackend.Load(), new TerminalOptions(
                UseMouse: false, CaretMode: CaretMode.Invisible, ManagedWindows: false
            )
        );
        // [Graphics Window]
        var graphicsWindow = terminal.Screen.Window(new(0, 0, 66, 33));
        // [CPU Debug Window]
        var cpuWindow = terminal.Screen.Window(new(graphicsWindow.Size.Width + 1, 0, 41, graphicsWindow.Size.Height));
        // [Refresh]
        terminal.Screen.Refresh();
        graphicsWindow.ColorMixture = terminal.Colors.MixColors(StandardColor.Black, StandardColor.White);
        graphicsWindow.DrawBorder();
        var active    = terminal.Colors.MixColors(StandardColor.White, StandardColor.Red);
        var nonactive = terminal.Colors.MixColors(StandardColor.White, StandardColor.Black);
        // Create Chip8
        var Chip8 = new Chip8::Console();
        Chip8.LoadROM(args[0]);
        // Draw
        UpdateDraw(Chip8.GFXBuffer, graphicsWindow, active, nonactive);
        if (drawCPU)
            UpdateCPUDraw(Chip8, cpuWindow);
        bool running = true;
        // Event handler
        foreach (var @event in terminal.Events.Listen(terminal.Screen))
        {
            // Run Emulator Thread
            if (@event is StartEvent && !singleStep)
            {
                new Thread(() => {
                    while (running)
                    {
                        Chip8.Tick();
                        UpdateDraw(Chip8.GFXBuffer, graphicsWindow, active, nonactive);
                        if (drawCPU)
                            UpdateCPUDraw(Chip8, cpuWindow);
                        // Lower Clockrate
                        Thread.Sleep(5);
                    }
                }).Start();
            }
            // Process End
            else if (@event is KeyEvent { Char.Value: 'C', Modifiers: ModifierKey.Ctrl })
                break;
            // Single Step Debugging
            else if (@event is KeyEvent { Char.Value: 'n' } && singleStep)
            {
                Chip8.Tick();
                UpdateDraw(Chip8.GFXBuffer, graphicsWindow, active, nonactive);
                // Debug CPU Draw
                if (drawCPU)
                    UpdateCPUDraw(Chip8, cpuWindow);
            }
            // Handle Inputs
            else if (@event is KeyEvent { Modifiers: ModifierKey.None })
            {
                // Inputs
                for (var i = 0; i < Chip8.Inputs.Length; ++i)
                    Chip8.SetKey((byte)i, false);
                // Set
                char key = Char.ToUpper((char)(@event as KeyEvent).Char.Value);
                if (Program.KeyInputs.Contains(key))
                {
                    var index = Array.IndexOf(Program.KeyInputs, key);
                    Chip8.SetKey((byte)index, true);
                }
            }
        }
        running = false;
    }
    public static void UpdateDraw(
        bool[,] graphicsBuffer, ITerminalSurface surface, ColorMixture active, ColorMixture nonactive
    )
    {
        surface.CaretLocation = new(1, 1);
        for (var y = 0; y < graphicsBuffer.GetLength(1); ++y)
        {
            for (var x = 0; x < graphicsBuffer.GetLength(0); ++x)
            {
                surface.ColorMixture = graphicsBuffer[x, y] ? active : nonactive;
                surface.WriteText(" ");
            }
            surface.CaretLocation = new(1, y + 1);
        }
        surface.Refresh();
    }
    public static void UpdateCPUDraw(Chip8::Console console, ITerminalSurface surface)
    {
        surface.CaretLocation = new(0, 0);
        surface.DeleteLines(surface.Size.Height);
        // Program Counter + OpCode
        var instruction = (console.Memory[console.CPU.ProgramCounter] << 8) | console.Memory[console.CPU.ProgramCounter + 1];
        surface.WriteText($"PC:0x{console.CPU.ProgramCounter.ToString("X4")} ");
        surface.WriteText($"| OpCode: 0x{instruction.ToString("X4")} ");
        surface.WriteText($"| I: 0x{console.CPU.AddressPointer.ToString("X4")}");
        surface.CaretLocation = new(0, surface.CaretLocation.Y + 1);
        // Stack
        surface.WriteText($"Delay: {console.CPU.DelayTimer} | Sound: {console.CPU.SoundTimer} | Stack: [");
        for (var i = 0; i < console.CPU.Stack.Length; ++i)
        {
            // Empty Stack
            if (console.CPU.Stack[i] == 0)
                break;
            // Handle initial breakpoint
            if (i == 0)
                surface.CaretLocation = new(1, surface.CaretLocation.Y + 1);
            // Handle drawing stack
            surface.WriteText($"0x{console.CPU.Stack[i].ToString("X4")}");
            if (i < console.CPU.StackPointer - 1 && (i + 1) % 4 != 0)
                surface.WriteText(", ");
            else
                surface.CaretLocation = new(i == console.CPU.StackPointer - 1 ? 0 : 1, surface.CaretLocation.Y + 1);
        }
        surface.WriteText("]");
        surface.CaretLocation = new(0, surface.CaretLocation.Y + 1);
        // Registers
        string[] registerBuilder = new string[4];
        for (var i = 0; i < console.CPU.Registers.Length; ++i)
        {
            string rgTop = $"V[{i.ToString("x")}]";
            string rgBtm = $"0x{console.CPU.Registers[i].ToString("X2")}";
            if (i < console.CPU.Registers.Length / 2 - 1 || i < console.CPU.Registers.Length - 1)
            {
                rgTop += " ";
                rgBtm += " ";
            }
            if (i < console.CPU.Registers.Length / 2)
            {
                registerBuilder[0] += rgTop;
                registerBuilder[1] += rgBtm;
            }
            else
            {
                registerBuilder[2] += rgTop;
                registerBuilder[3] += rgBtm;
            }
        }
        string registers = String.Join("\n", registerBuilder);
        surface.WriteText(registers);
        surface.CaretLocation = new(0, surface.CaretLocation.Y + 1);
        for (var i = 0; i < console.Inputs.Length; ++i)
        {
            surface.WriteText($"{Convert.ToByte(console.Inputs[i])}");
            if (i < console.Inputs.Length - 1)
                surface.WriteText(",");
        }
        surface.Refresh();
    }
    /* Static Properties */
    private static char[] KeyInputs = new char[] {
        '1', '2', '3', '4',
        'Q', 'W', 'E', 'R',
        'A', 'S', 'D', 'F',
        'Z', 'X', 'C', 'V'
    };
}
