/*
    Chip8 Emulator: Hardware Implementation
    - Display [NCurses]

    Written By: Ryan Smith
*/
#pragma warning disable CA1416

using System.Threading;
using Sharpie;
using Sharpie.Backend;
using Chip8 = Emulators.Chip8;

public class NCursesDisplay : Chip8::Display
{
    /* Constructors */
    public NCursesDisplay()
    {
        var backend = CursesBackend.Load();
        var options = new TerminalOptions(UseMouse: false);
        this._Terminal = new Terminal(backend, options);
    }
    ~NCursesDisplay() {}
    /* Instance Methods */
    public override void Draw() {}
    /* Properties */
    private readonly Terminal _Terminal;
}
