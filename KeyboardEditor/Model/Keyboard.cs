using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KeyboardEditor.Model
{
    internal class Keyboard : INotifyPropertyChanged
    {
        private KeyboardSettings keyboardSettings;
        private IList<Button> buttons;
        private IList<EncoderControl> encoders;
        private int numLeds;

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

        public int NumLeds { get => numLeds; set { numLeds = value; OnPropertyChanged("NumLeds"); } }

        private readonly Comms Comms;

        public Keyboard()
        {
            Comms = new Comms();
            Buttons = new List<Button>();
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

        public string WriteKeyboard()
        {
            var eeprom = EncodeEeprom();

            if (Comms.WriteEeprom(eeprom))
            {
                return "Written OK";
            }
            else
            {
                return "Failed to write";
            }
        }

        private const ushort EEPROM_INITIALISED = 0x1DEA;

        private bool DecodeEeprom(byte[] eeprom)
        {
            var success = false;

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
                keyboardSettings.NumButtons = 0;
                keyboardSettings.NumEncoders = 0;
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
                            KeyCode = (ushort)(eeprom[address + 2] * 256 + eeprom[address + 3]),
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
            var eeprom = new byte[keyboardSettings.EEPROMLength];
            eeprom[0] = EEPROM_INITIALISED & 0xFF00 >> 8;
            eeprom[1] = EEPROM_INITIALISED & 0xFF;

            //lookuptable address
            var lookupAddress = 8 + buttons.Count + 1 + encoders.Count * 3;
            eeprom[2] = (byte)(lookupAddress & 0xFF00 >> 8);
            eeprom[3] = (byte)(lookupAddress & 0xFF);

            //settings table address
            var settingsAddress = lookupAddress + 2 * buttons.Count + 6 * encoders.Count + 1;
            eeprom[4] = (byte)(settingsAddress & 0xFF00 >> 8);
            eeprom[5] = (byte)(settingsAddress & 0xFF);

            eeprom[6] = (byte)NumLeds;
            eeprom[7] = (byte)buttons.Count;
            for (int i = 0; i < buttons.Count; i++)
            {
                eeprom[8 + i] = buttons[i].Pin;
            }
            eeprom[8 + buttons.Count] = (byte)encoders.Count;
            for (int i = 0; i < encoders.Count; i++)
            {
                eeprom[8 + buttons.Count + 1 + i * 3] = encoders[i].Button.Pin;
                eeprom[8 + buttons.Count + 2 + i * 3] = encoders[i].ClockwiseStep.Pin;
                eeprom[8 + buttons.Count + 3 + i * 3] = encoders[i].AntiClockwiseStep.Pin;
            }

            //settings
            for (int i = 0; i < encoders.Count; i++)
            {
                eeprom[settingsAddress + i] = encoders[i].StepSize;
            }

            //write lookup table as the command arrays are written
            //first array is after the lookup table
            var buttonArrayAddress = lookupAddress + buttons.Count * 2 + encoders.Count * 6;

            //button command arrays
            for (int i = 0; i < buttons.Count; i++)
            {
                int afterButtonArrayAddress = WriteButtonCommandArray(buttons[i], eeprom, buttonArrayAddress);
                //write lookup table
                eeprom[lookupAddress + i * 2] = (byte)(buttonArrayAddress & 0xFF00 >> 8);
                eeprom[lookupAddress + i * 2 + 1] = (byte)(buttonArrayAddress & 0xFF);
                //point to eeprom location for next array
                buttonArrayAddress = afterButtonArrayAddress;
            }

            //encoder command arrays
            for (int i = 0; i < encoders.Count; i++)
            {
                int afterButtonArrayAddress = WriteButtonCommandArray(encoders[i].Button, eeprom, buttonArrayAddress);
                eeprom[lookupAddress + buttons.Count * 2 + i * 6] = (byte)(buttonArrayAddress & 0xFF00 >> 8);
                eeprom[lookupAddress + i * 2 + 1] = (byte)(buttonArrayAddress & 0xFF);
                buttonArrayAddress = afterButtonArrayAddress;

                afterButtonArrayAddress = WriteButtonCommandArray(encoders[i].ClockwiseStep, eeprom, buttonArrayAddress);
                eeprom[lookupAddress + buttons.Count * 2 + i * 6 + 2] = (byte)(buttonArrayAddress & 0xFF00 >> 8);
                eeprom[lookupAddress + i * 2 + 1] = (byte)(buttonArrayAddress & 0xFF);
                buttonArrayAddress = afterButtonArrayAddress;

                afterButtonArrayAddress = WriteButtonCommandArray(encoders[i].AntiClockwiseStep, eeprom, buttonArrayAddress);
                eeprom[lookupAddress + buttons.Count * 2 + i * 6 + 4] = (byte)(buttonArrayAddress & 0xFF00 >> 8);
                eeprom[lookupAddress + i * 2 + 1] = (byte)(buttonArrayAddress & 0xFF);
                buttonArrayAddress = afterButtonArrayAddress;
            }

            // eeprom Size Used = buttonArrayAddress;
            var resizedEeprom = new byte[buttonArrayAddress];
            Array.Copy(eeprom, 0, resizedEeprom, 0, buttonArrayAddress);
            return resizedEeprom;
        }

        private int WriteButtonCommandArray(Button button, byte[] eeprom, int buttonArrayAddress)
        {
            var address = buttonArrayAddress;
            foreach (var command in button.Commands)
            {
                eeprom[address] = (byte)CommandType.Keyboard;
                address++;

                switch (command.CommandType)
                {
                    case CommandType.NoCommand:
                        break;

                    case CommandType.Keyboard:
                        eeprom.SetValue(((KeyboardCommand)command).PressType, address);
                        eeprom.SetValue(((KeyboardCommand)command).KeyCode, address + 1);
                        address += 3;
                        break;

                    case CommandType.Delay:
                        eeprom.SetValue(((DelayCommand)command).DelayMs, address);
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