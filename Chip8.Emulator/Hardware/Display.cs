/*
    Chip8 Emulator: Hardware Resource
    - Display

    Written By: Ryan Smith
*/
using System;

namespace Emulators.Chip8;

public abstract class Display
{
    /* Instance Methods */
    public virtual void Draw() { throw new NotImplementedException("Display.Draw()"); }
    public virtual void Reset()
    {
        for (var x = 0; x < this.Buffer.GetLength(0); ++x)
        {
            for (var y = 0; y < this.Buffer.GetLength(1); ++y)
                this.Buffer[x, y] = false;
        }
    }
    /* Properties */
    public readonly bool[,] Buffer = new bool[64, 32];
}
