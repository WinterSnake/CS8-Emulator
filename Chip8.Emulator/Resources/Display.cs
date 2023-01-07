/*
    Chip8 Emulator: Resources
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
        public bool[,] Buffer { get; private set; } = new bool[64, 32];
    }
}
