/*
    Chip8 Emulator
    - Frontend: Ncurses

    Written By: Ryan Smith
*/
#pragma warning disable CA1416

using System;
using System.IO;
using System.Linq;
using Sharpie;
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
        // Parse additional arguments (single-step, debug)
        bool singleStep = true;
        bool drawCPU = true;
        // Setup ncurses
        using var terminal = new Terminal(
            CursesBackend.Load(), new TerminalOptions(
                UseMouse: false, CaretMode: CaretMode.Invisible
            )
        );
        // Create Chip8
        var Chip8 = new Chip8::Console();
        Chip8.LoadROM(args[0]);
        // Event handler
        foreach (var @event in terminal.Events.Listen(terminal.Screen))
        {
            // Run Emulator Thread
            if (@event is StartEvent) {}
            // Process End
            else if (@event is KeyEvent { Char.Value: 'C', Modifiers: ModifierKey.Ctrl })
                break;
            // Single Step Debugging
            else if (@event is KeyEvent { Char.Value: 'N' } && singleStep) {}
            // Manual CPU Draw Update
            else if (@event is KeyEvent { Char.Value: 'U' } && drawCPU) {}
            // Handle Inputs
            else if (@event is KeyEvent)
            {
                char key = Char.ToUpper((char)(@event as KeyEvent).Char.Value);
                if (Program.KeyInputs.Contains(key))
                {
                    var index = Array.IndexOf(Program.KeyInputs, key);
                    Chip8.SetKey((byte)index, true);
                }
            }
        }
    }
    /* Static Properties */
    private static char[] KeyInputs = new char[] {
        '1', '2', '3', '4',
        'Q', 'W', 'E', 'R',
        'A', 'S', 'D', 'F',
        'Z', 'X', 'C', 'V'
    };
}
