/*
	Chip8 Emulator: Raylib

	Written By: Ryan Smith
*/
using System;
using System.Collections.Immutable;
using Raylib_cs;
using Chip8 = Emulators.Chip8.Hardware;

internal static class Program
{
	/* Static Methods */
	private static void Main(string[] args)
	{
		// Load Rom
		if (args.Length != 1)
		{
			Console.Error.WriteLine("No ROM file detected - please pass a rom file when calling the executable..");
			return;
		}
		Chip8.LoadROM(args[0]);
		// Initialize Raylib
        Raylib.InitWindow(Chip8.GFXBuffer.GetLength(0) * SCALE, Chip8.GFXBuffer.GetLength(1) * SCALE, "Chip8 Emulator");

        while (!Raylib.WindowShouldClose())
        {
			// Render
            Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.White);
				// Draw gfx buffer
				for (var x = 0; x < Chip8.GFXBuffer.GetLength(0); ++x)
				{
					for (var y = 0; y < Chip8.GFXBuffer.GetLength(1); ++y)
					{
						if (Chip8.GFXBuffer[x, y]) Raylib.DrawRectangle(x * SCALE, y * SCALE, SCALE, SCALE, Color.Red);
					}
				}
            Raylib.EndDrawing();
			// Inputs
			// CPU
			Chip8.Tick();
        }

		Raylib.CloseWindow();
	}
	/* Class Properties */
	private static Chip8.Console Chip8 = new Chip8.Console();
	private static int SCALE = 20;
}
