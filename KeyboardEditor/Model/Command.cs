using System;
using System.Collections.Generic;
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
        Delay = 0,
        Keyboard = 1,
        Consumer = 2,
        System = 3
    }

    public class Command
    {
        public PressType PressType;
        public CommandType KeyCodeType;
        public UInt16 KeyCode;

        public Command()
        {
            PressType = PressType.PressAndRelease;
        }

        public override string ToString()
        {
            var cmdString = "";
            switch (KeyCodeType)
            {
                case CommandType.Keyboard:
                    cmdString = Enum.GetName(typeof(KeyboardKeycode), KeyCode) + " " + Enum.GetName(typeof(PressType), PressType);

                    break;

                case CommandType.Consumer:
                    cmdString = Enum.GetName(typeof(ConsumerKeycode), KeyCode);
                    break;

                case CommandType.System:
                    cmdString = Enum.GetName(typeof(SystemKeycode), KeyCode);
                    break;

                default:
                    break;
            }

            return cmdString;
        }
    }
}