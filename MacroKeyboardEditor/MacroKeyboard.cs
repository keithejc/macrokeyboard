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

        public string WriteButton(Button button)
        {
            if (!Comms.WriteButton(button))
            {
                return "Failed to write " + button.ToString() + " Try again, comms problem?";
            }

            return "Buttons written";
        }

        public string ReadKeyboard()
        {
            if (Comms.FindKeyboardPort())
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
                return "Can't find keyboard";
            }

            return "Ready...";
        }
    }
}