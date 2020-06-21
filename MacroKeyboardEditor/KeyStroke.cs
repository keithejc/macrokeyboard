using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroKeysWriter
{
    public enum PressType
    {
        Press = 0,
        Release = 1,
        PressAndRelease = 2
    }

    public enum KeyCodeType
    {
        Delay = 0,
        Keyboard = 1,
        Consumer = 2,
        System = 3
    }

    public class KeyStroke
    {
        public PressType PressType;
        public KeyCodeType KeyCodeType;
        public UInt16 KeyCode;

        public KeyStroke()
        {
            PressType = PressType.PressAndRelease;
        }

        public override string ToString()
        {
            var cmdString = "";
            switch (KeyCodeType)
            {
                case KeyCodeType.Keyboard:
                    cmdString = "Keyboard: " + Enum.GetName(typeof(KeyboardKeycode), KeyCode) + " " + Enum.GetName(typeof(PressType), PressType);

                    break;

                case KeyCodeType.Consumer:
                    cmdString = "Consumer: " + Enum.GetName(typeof(ConsumerKeycode), KeyCode);
                    break;

                case KeyCodeType.System:
                    cmdString = "System: " + Enum.GetName(typeof(SystemKeycode), KeyCode);
                    break;

                default:
                    break;
            }

            return cmdString;
        }
    }
}