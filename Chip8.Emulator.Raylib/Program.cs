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

        Raylib.InitWindow(Chip8.GFXMemory.GetLength(0) * SCALE, Chip8.GFXMemory.GetLength(1) * SCALE, "Chip8 Emulator");

		Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
			/// Render
            Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.White);
				// Draw gfx memory
				for (var x = 0; x < Chip8.GFXMemory.GetLength(0); ++x)
					for (var y = 0; y < Chip8.GFXMemory.GetLength(1); ++y)
						if (Chip8.GFXMemory[x, y])
							Raylib.DrawRectangle(x * SCALE, y * SCALE, SCALE, SCALE, Color.Red);
            Raylib.EndDrawing();
			/// Inputs
			for (var i = 0; i < Program.INPUTS.Length; ++i)
				Program.Chip8.Inputs[i] = Raylib.IsKeyDown(Program.INPUTS[i]);
			/// Console
			Chip8.Tick();
        }

		Raylib.CloseWindow();
	}
	/* Class Properties */
	private static readonly int SCALE = 20;
	private static readonly Chip8.Console Chip8 = new Chip8.Console();
	private static readonly ImmutableArray<KeyboardKey> INPUTS = ImmutableArray.Create(
		KeyboardKey.One, KeyboardKey.Two, KeyboardKey.Three, KeyboardKey.Four,
		KeyboardKey.Q,   KeyboardKey.W,   KeyboardKey.E,     KeyboardKey.R,
		KeyboardKey.A,   KeyboardKey.S,   KeyboardKey.D,     KeyboardKey.F,
		KeyboardKey.Z,   KeyboardKey.X,   KeyboardKey.C,     KeyboardKey.V
	);
}
