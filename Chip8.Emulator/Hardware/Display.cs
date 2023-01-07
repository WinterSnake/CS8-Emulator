/*
    Chip8 Emulator: Hardware Resource
    - Display

    Written By: Ryan Smith
*/
using System;

namespace Emulators.Chip8
{
    public abstract class Display
    {
        /* Instance Methods */
        public virtual void Draw() { throw new NotImplementedException("Display.Draw()"); }
        /* Properties */
        public readonly bool[,] Buffer = new bool[64, 32];
    }
}
