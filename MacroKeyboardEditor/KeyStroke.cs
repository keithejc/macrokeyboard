using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroKeysWriter
{
    public enum KeyCodeType
    {
        Keyboard = 1,
        Consumer = 2,
        System = 3
    }

    public class KeyStroke
    {
        public KeyCodeType KeyCodeType;
        public UInt16 KeyCode;

        public override string ToString()
        {
            var cmdString = "";
            switch (KeyCodeType)
            {
                case KeyCodeType.Keyboard:
                    cmdString = "Keyboard: " + Enum.GetName(typeof(KeyboardKeycode), KeyCode);

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