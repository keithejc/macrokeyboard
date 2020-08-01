using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;

namespace KeyboardEditor.Model
{
    internal class Keyboard : INotifyPropertyChanged
    {
        private KeyboardSettings keyboardSettings;
        private IList<Button> buttons;
        private IList<EncoderControl> encoders;
        private byte numLeds;

        public KeyboardSettings KeyboardSettings
        {
            get { return keyboardSettings; }
            set
            {
                keyboardSettings = value;
                OnPropertyChanged("KeyboardSettings");
            }
        }

        public IList<Button> Buttons
        {
            get { return buttons; }
            set
            {
                buttons = value;
                OnPropertyChanged("Buttons");
            }
        }

        public IList<EncoderControl> Encoders
        {
            get { return encoders; }
            set
            {
                encoders = value;
                OnPropertyChanged("Encoders");
            }
        }

        public byte NumLeds { get => numLeds; set { numLeds = value; OnPropertyChanged("NumLeds"); } }

        private readonly Comms Comms;

        public Keyboard()
        {
            Comms = new Comms();
            Buttons = new List<Button>();

            Buttons.Add(new Button() { ButtonIndex = 0, Pin = 10 });
            Buttons[0].Commands.Add(new KeyboardCommand() { CommandType = CommandType.Keyboard, PressType = PressType.PressAndRelease, KeyCode = KeyboardKeycode.KEY_A });

            Encoders = new List<EncoderControl>();
        }

        public string ReadKeyboard()
        {
            if (Comms.FindKeyboardPort())
            {
                Buttons = new List<Button>();
                KeyboardSettings = Comms.GetKeyboardSettings();
                if (KeyboardSettings != null)
                {
                    if (!DecodeEeprom(Comms.ReadEeprom()))
                    {
                        return "Failed to decode eeprom";
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

        private void SetupDefaults()
        {
            NumLeds = 10;
            buttons.Clear();

            var b = new Button { ButtonIndex = 0, Pin = 23 };
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_MEDIA_PREV_TRACK, PressType = PressType.PressAndRelease });
            buttons.Add(b);

            b = new Button { ButtonIndex = 1, Pin = 22 };
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_MEDIA_NEXT_TRACK, PressType = PressType.PressAndRelease });
            buttons.Add(b);

            b = new Button { ButtonIndex = 2, Pin = 21 };
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_V, PressType = PressType.PressAndRelease });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_A, PressType = PressType.PressAndRelease });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_R, PressType = PressType.PressAndRelease });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_A, PressType = PressType.PressAndRelease });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_N, PressType = PressType.PressAndRelease });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_U, PressType = PressType.PressAndRelease });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_S, PressType = PressType.PressAndRelease });
            buttons.Add(b);

            b = new Button { ButtonIndex = 3, Pin = 20 };
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_CTRL, PressType = PressType.Press });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_SHIFT, PressType = PressType.Press });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_ALT, PressType = PressType.Press });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_H, PressType = PressType.PressAndRelease });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_CTRL, PressType = PressType.Release });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_SHIFT, PressType = PressType.Release });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_ALT, PressType = PressType.Release });
            buttons.Add(b);

            b = new Button { ButtonIndex = 4, Pin = 19 };
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_CTRL, PressType = PressType.Press });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_SHIFT, PressType = PressType.Press });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_ALT, PressType = PressType.Press });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_S, PressType = PressType.PressAndRelease });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_CTRL, PressType = PressType.Release });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_SHIFT, PressType = PressType.Release });
            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_LEFT_ALT, PressType = PressType.Release });
            buttons.Add(b);

            b = new Button { ButtonIndex = 5, Pin = 18 };
            //            b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode., PressType = PressType.PressAndRelease });
            buttons.Add(b);

            b = new Button { ButtonIndex = 6, Pin = 17 };
            //          b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode., PressType = PressType.PressAndRelease });
            buttons.Add(b);

            b = new Button { ButtonIndex = 7, Pin = 16 };
            //        b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode., PressType = PressType.PressAndRelease });
            buttons.Add(b);

            b = new Button { ButtonIndex = 8, Pin = 15 };
            //      b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode., PressType = PressType.PressAndRelease });
            buttons.Add(b);

            b = new Button { ButtonIndex = 9, Pin = 14 };
            //    b.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode., PressType = PressType.PressAndRelease });
            buttons.Add(b);

            var e = new EncoderControl { StepSize = 1, EncoderIndex = 0 };
            e.Button = new Button { Pin = 5 };
            e.Button.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_MEDIA_PLAY_PAUSE, PressType = PressType.PressAndRelease });

            e.ClockwiseStep = new Button { Pin = 7 };
            e.ClockwiseStep.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_MEDIA_VOLUME_DEC, PressType = PressType.PressAndRelease });

            e.AntiClockwiseStep = new Button { Pin = 6 };
            e.AntiClockwiseStep.Commands.Add(new KeyboardCommand { CommandType = CommandType.Keyboard, KeyCode = KeyboardKeycode.KEY_MEDIA_VOLUME_INC, PressType = PressType.PressAndRelease });
            encoders.Add(e);
        }

        public string WriteKeyboard()
        {
            SetupDefaults();
            var eeprom = EncodeEeprom();

            return Comms.WriteEeprom(eeprom);
        }

        private const ushort EEPROM_INITIALISED = 0x1DEA;

        private bool DecodeEeprom(byte[] eeprom)
        {
            var success = false;

            if (eeprom.Length == 0)
            {
                return true;
            }

            //is this a valid eeprom? check first 2 bytes
            if (eeprom[0] == (EEPROM_INITIALISED & 0xFF00) >> 8 &&
                eeprom[1] == (EEPROM_INITIALISED & 0xFF))
            {
                var lookupTableAddress = eeprom[2] * 256 + eeprom[3];
                var settingsTableAddress = eeprom[4] * 256 + eeprom[5];

                var numLeds = eeprom[6];
                var numButtons = eeprom[7];
                buttons.Clear();
                for (int i = 0; i < numButtons; i++)
                {
                    buttons.Add(new Button() { ButtonIndex = i, Pin = eeprom[8 + i] });
                }

                var numEncoders = eeprom[8 + numButtons];
                encoders.Clear();
                for (int i = 0; i < numEncoders; i++)
                {
                    var e = new EncoderControl()
                    {
                        EncoderIndex = i
                    };
                    e.Button = new Button() { Pin = eeprom[9 + (i * 3)] };
                    e.Button = new Button() { Pin = eeprom[9 + (i * 3) + 1] };
                    e.Button = new Button() { Pin = eeprom[9 + (i * 3) + 2] };
                    encoders.Add(e);
                }

                //settings table
                for (int i = 0; i < numEncoders; i++)
                {
                    encoders[i].StepSize = eeprom[settingsTableAddress + i];
                }

                //button command arrays referenced in lookup table
                for (int buttonIndex = 0; buttonIndex < buttons.Count; buttonIndex++)
                {
                    var lookupAddress = lookupTableAddress + 2 * buttonIndex;
                    var commandsAddress = eeprom[lookupAddress] * 256 + eeprom[lookupAddress + 1];
                    ReadButtonCommands(buttons[buttonIndex], eeprom, commandsAddress);
                }
                //encoder command arrays referenced next in lookup table
                for (int encoderIndex = 0; encoderIndex < buttons.Count; encoderIndex++)
                {
                    var lookupAddress = lookupTableAddress + (2 * numButtons) + (6 * encoderIndex);
                    ReadEncoderCommands(encoders[encoderIndex], eeprom, lookupAddress);
                }
            }
            else
            {
                success = true;
                buttons.Clear();
                encoders.Clear();
                keyboardSettings.NumLeds = 0;
            }

            return success;
        }

        /// <summary>
        /// encoder command arrays refernced in the encoders part of the lookup table
        /// </summary>
        /// <param name="encoderControl"></param>
        /// <param name="eeprom"></param>
        /// <param name="lookupAddress"></param>
        private void ReadEncoderCommands(EncoderControl encoderControl, byte[] eeprom, int lookupAddress)
        {
            var buttonCommandsAddress = eeprom[lookupAddress] * 256 + eeprom[lookupAddress + 1];
            ReadButtonCommands(encoderControl.Button, eeprom, buttonCommandsAddress);
            var clockwiseCommandsAddress = eeprom[lookupAddress + 2] * 256 + eeprom[lookupAddress + 3];
            ReadButtonCommands(encoderControl.ClockwiseStep, eeprom, clockwiseCommandsAddress);
            var antiClockwiseCommandsAddress = eeprom[lookupAddress + 4] * 256 + eeprom[lookupAddress + 5];
            ReadButtonCommands(encoderControl.ClockwiseStep, eeprom, antiClockwiseCommandsAddress);
        }

        /// <summary>
        /// read a list of commands from this address
        /// commands are terminated with a null command type
        ///
        /// </summary>
        /// <param name="button"></param>
        /// <param name="eeprom"></param>
        /// <param name="startAddress"></param>
        private void ReadButtonCommands(Button button, byte[] eeprom, int startAddress)
        {
            var address = startAddress;
            var commandType = (CommandType)eeprom[address];
            do
            {
                switch (commandType)
                {
                    case CommandType.NoCommand:
                        return;

                    case CommandType.Keyboard:
                        var kc = new KeyboardCommand()
                        {
                            CommandType = commandType,
                            PressType = (PressType)(eeprom[address + 1]),
                            KeyCode = (KeyboardKeycode)(eeprom[address + 2] * 256 + eeprom[address + 3]),
                        };
                        button.Commands.Add(kc);
                        address += 3;
                        break;

                    case CommandType.Delay:
                        var dc = new DelayCommand
                        {
                            CommandType = commandType,
                            DelayMs = (ushort)(eeprom[address + 1] * 256 + eeprom[address + 2])
                        };
                        button.Commands.Add(dc);
                        address += 2;
                        break;

                    case CommandType.LedLightSingle:
                        break;

                    case CommandType.LedClearAll:
                        break;

                    case CommandType.LedCustomPattern:
                        break;

                    default:
                        break;
                }

                //'null' terminated list of commands
                commandType = (CommandType)eeprom[address];
            } while (commandType != CommandType.NoCommand);
        }

        private byte[] EncodeEeprom()
        {
            var eeprom = new byte[2048];//keyboardSettings.EEPROMLength];
            eeprom[0] = EEPROM_INITIALISED & 0xFF;
            eeprom[1] = (EEPROM_INITIALISED & 0xFF00) >> 8;

            //settings table  - after button pins and encoder pins
            var settingsTableAddress = 9 + buttons.Count + encoders.Count * 3;
            eeprom[2] = (byte)(settingsTableAddress & 0xFF);
            eeprom[3] = (byte)((settingsTableAddress & 0xFF00) >> 8);

            //lookuptable address - after settings
            var lookupTableAddress = settingsTableAddress + encoders.Count;
            eeprom[4] = (byte)(lookupTableAddress & 0xFF);
            eeprom[5] = (byte)((lookupTableAddress & 0xFF00) >> 8);

            //number of leds
            eeprom[6] = (byte)NumLeds;
            //number of buttons
            eeprom[7] = (byte)buttons.Count;
            //number of encoders
            eeprom[8] = (byte)encoders.Count;
            //button pins
            for (int i = 0; i < buttons.Count; i++)
            {
                eeprom[9 + i] = buttons[i].Pin;
            }
            //encoder pins
            for (int i = 0; i < encoders.Count; i++)
            {
                eeprom[9 + buttons.Count + i * 3] = encoders[i].Button.Pin;
                eeprom[9 + buttons.Count + i * 3 + 1] = encoders[i].ClockwiseStep.Pin;
                eeprom[9 + buttons.Count + i * 3 + 2] = encoders[i].AntiClockwiseStep.Pin;
            }

            //settings
            for (int i = 0; i < encoders.Count; i++)
            {
                eeprom[settingsTableAddress + i] = encoders[i].StepSize;
            }

            //write lookup table as the command arrays are written
            //first array is after the lookup table
            var lookuptableSize = 2 * buttons.Count + 6 * encoders.Count;
            var commandArrayAddress = lookupTableAddress + lookuptableSize;

            //button command arrays
            for (int i = 0; i < buttons.Count; i++)
            {
                //write lookup table
                eeprom[lookupTableAddress + i * 2] = (byte)(commandArrayAddress & 0xFF);
                eeprom[lookupTableAddress + i * 2 + 1] = (byte)((commandArrayAddress & 0xFF00) >> 8);
                commandArrayAddress = WriteButtonCommandArray(buttons[i], eeprom, commandArrayAddress);
            }

            //encoder command arrays
            for (int i = 0; i < encoders.Count; i++)
            {
                eeprom[lookupTableAddress + buttons.Count * 2 + i * 6] = (byte)(commandArrayAddress & 0xFF);
                eeprom[lookupTableAddress + buttons.Count * 2 + i * 6 + 1] = (byte)((commandArrayAddress & 0xFF00) >> 8);
                commandArrayAddress = WriteButtonCommandArray(encoders[i].Button, eeprom, commandArrayAddress);

                eeprom[lookupTableAddress + buttons.Count * 2 + i * 6 + 2] = (byte)(commandArrayAddress & 0xFF);
                eeprom[lookupTableAddress + buttons.Count * 2 + i * 6 + 3] = (byte)((commandArrayAddress & 0xFF00) >> 8);
                commandArrayAddress = WriteButtonCommandArray(encoders[i].ClockwiseStep, eeprom, commandArrayAddress);

                eeprom[lookupTableAddress + buttons.Count * 2 + i * 6 + 4] = (byte)(commandArrayAddress & 0xFF);
                eeprom[lookupTableAddress + buttons.Count * 2 + i * 6 + 5] = (byte)((commandArrayAddress & 0xFF00) >> 8);
                commandArrayAddress = WriteButtonCommandArray(encoders[i].AntiClockwiseStep, eeprom, commandArrayAddress);
            }

            // eeprom Size Used = buttonArrayAddress;
            var resizedEeprom = new byte[commandArrayAddress];
            Array.Copy(eeprom, 0, resizedEeprom, 0, commandArrayAddress);
            return resizedEeprom;
        }

        private int WriteButtonCommandArray(Button button, byte[] eeprom, int buttonArrayAddress)
        {
            var address = buttonArrayAddress;
            foreach (var command in button.Commands)
            {
                eeprom[address] = (byte)command.CommandType;
                address++;

                switch (command.CommandType)
                {
                    case CommandType.NoCommand:
                        break;

                    case CommandType.Keyboard:
                        eeprom[address] = (byte)((KeyboardCommand)command).PressType;
                        eeprom[address + 1] = (byte)(((ushort)((KeyboardCommand)command).KeyCode & 0xFF));
                        eeprom[address + 2] = (byte)(((ushort)((KeyboardCommand)command).KeyCode & 0xFF00) >> 8);
                        address += 3;
                        break;

                    case CommandType.Delay:
                        eeprom[address] = (byte)(((ushort)((DelayCommand)command).DelayMs & 0xFF));
                        eeprom[address + 1] = (byte)(((ushort)((DelayCommand)command).DelayMs & 0xFF00) >> 8);
                        address += 2;
                        break;

                    case CommandType.LedLightSingle:
                        break;

                    case CommandType.LedClearAll:
                        break;

                    case CommandType.LedCustomPattern:
                        break;

                    default:
                        break;
                }
            }

            //'null' terminated
            eeprom[address] = (byte)CommandType.NoCommand;
            return address + 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}