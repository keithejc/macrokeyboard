using System.Collections.Generic;
using static MacroKeysWriter.Button;

namespace MacroKeysWriter
{
    public class MacroKeyboard
    {
        public KeyboardSettings KeyboardSettings { get; set; }
        private readonly Comms Comms;
        public List<Button> Buttons;

        public MacroKeyboard()
        {
            Comms = new Comms();
            Buttons = new List<Button>();
        }

        public string ReadKeyboard()
        {
            var port = Comms.FindKeyboardPort();
            if (!string.IsNullOrEmpty(port))
            {
                if (Comms.OpenPort(port))
                {
                    Buttons = new List<Button>();
                    KeyboardSettings = Comms.GetKeyboardSettings();
                    if (KeyboardSettings != null)
                    {
                        for (byte buttonIndex = 0; buttonIndex < KeyboardSettings.NumButtons; buttonIndex++)
                        {
                            var key = Comms.ReadButton(buttonIndex, KeyboardSettings.MaxNumKeystrokesPerButton);
                            if (key != null)
                            {
                                Buttons.Add(key);
                            }
                            else
                            {
                                return "Failed to read a button. Comms Problem?";
                            }
                        }
                        for (byte encoderIndex = 0; encoderIndex < KeyboardSettings.NumEncoders; encoderIndex++)
                        {
                            var encoderControls = Comms.ReadEncoder(encoderIndex, KeyboardSettings.MaxNumKeystrokesPerButton);
                            if (encoderControls.Length == 3)
                            {
                                Buttons.AddRange(encoderControls);
                            }
                            else
                            {
                                return "Failed to read an encoder. Comms Problem?";
                            }
                        }
                    }
                    else
                    {
                        return "Failed to read keyboard settings. Comms Problem?";
                    }
                }
                else
                {
                    return "Can't open keyboard port: " + port;
                }
            }
            else
            {
                return "Can't find keyboard";
            }

            return "Ready...";
        }
    }
}