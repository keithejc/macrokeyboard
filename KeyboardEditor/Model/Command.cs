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

        public string Name => this.ToString();

        public override string ToString()
        {
            var cmdString = Enum.GetName(typeof(KeyboardKeycode), KeyCode) + " " + Enum.GetName(typeof(PressType), PressType);
            Debug.WriteLine("Command " + cmdString);
            return cmdString;
        }
    }

    public class DelayCommand : Command
    {
        public ushort DelayMs;

        public string Name => this.ToString();

        public override string ToString()
        {
            var cmdString = "Delay " + DelayMs / 1000 + "s";
            Debug.WriteLine("Command " + cmdString);
            return cmdString;
        }
    }
}