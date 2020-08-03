using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardEditor.Model
{
    public enum PressType
    {
        Press = 0,
        Release = 1,
        PressAndRelease = 2
    }

    public enum CommandType
    {
        NoCommand = 0,
        Keyboard = 1,
        Delay = 2,
        LedLightSingle = 3,
        LedClearAll = 4,
        LedCustomPattern = 5
    }

    public class Command
    {
        public CommandType CommandType;
    }

    public class KeyboardCommand : Command
    {
        public PressType PressType;
        public KeyboardKeycode KeyCode;

        public KeyboardCommand()
        {
            CommandType = CommandType.Keyboard;
        }

        public string Name => this.ToString();

        public override string ToString()
        {
            var cmdString = Enum.GetName(typeof(KeyboardKeycode), KeyCode) + " " + Enum.GetName(typeof(PressType), PressType);
            return cmdString;
        }
    }

    public class DelayCommand : Command
    {
        public ushort DelayMs;

        public DelayCommand()
        {
            CommandType = CommandType.Delay;
        }

        public string Name => this.ToString();

        public override string ToString()
        {
            var cmdString = "Delay " + DelayMs / 1000 + "s";
            return cmdString;
        }
    }

    public class LedCommand : Command
    {
        public byte LedIndex;
        public byte[] LedColour = new byte[3];

        public LedCommand()
        {
            CommandType = CommandType.LedLightSingle;
        }

        public string Name => this.ToString();

        public override string ToString()
        {
            var cmdString = "LED " + (LedIndex + 1) + " [" + LedColour[0] + " " + LedColour[1] + " " + LedColour[2] + "]";
            return cmdString;
        }
    }
}