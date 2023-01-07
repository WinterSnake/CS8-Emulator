/*
    Chip8 Emulator
    - Frontend: Ncurses

    Written By: Ryan Smith
*/
using Chip8 = Emulators.Chip8;

internal class Program
{
    public static void Main(string[] args)
    {
        Chip8::Console chip8 = new Chip8::Console(new NCursesDisplay());
    }
}
