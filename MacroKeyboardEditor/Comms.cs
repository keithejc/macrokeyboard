using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows.Forms;
using static MacroKeysWriter.Button;
using RJCP.IO.Ports;

namespace MacroKeysWriter
{
    public class Comms
    {
        private SerialPortStream serialPort;

        private const byte SerialStartByte = 0xee;
        private const byte ReadButtonsCommand = 0x00;
        private const byte WriteButtonsCommand = 0x01;
        private const byte ReadEncodersCommand = 0x02;
        private const byte WriteEncodersCommand = 0x03;
        private const byte GetSettingsCommand = 0x04;

        public string FindKeyboardPort()
        {
            var keyboardPort = "";

            if (serialPort != null)
            {
                serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
            }

            var ports = GetPorts(false);
            foreach (var port in ports)
            {
                Console.WriteLine("Investigate port " + port);
                OpenPort(port);
                if (GetKeyboardSettings() != null)
                {
                    Console.WriteLine("Found keyboard " + port);
                    keyboardPort = port;
                }
                serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
            }

            return keyboardPort;
        }

        public bool OpenPort(string portName)
        {
            var success = false;
            var retries = 3;

            while (retries > 0)
            {
                try
                {
                    serialPort = new SerialPortStream(portName, 9600, 8, Parity.None, StopBits.One);
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    serialPort.ReadTimeout = 1000;
                    serialPort.Open();
                    success = true;
                    break;
                }
                catch (Exception)
                {
                    retries--;
                }
            }
            return success;
        }

        ~Comms()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
                serialPort.Close();
            }
        }

        public List<string> GetPorts(bool showDescription)
        {
            var ports = new List<string>();

            try
            {
                foreach (PortDescription desc in SerialPortStream.GetPortDescriptions())
                {
                    if (string.IsNullOrEmpty(desc.Description) || !showDescription)
                    {
                        ports.Add(desc.Port);
                    }
                    else
                    {
                        ports.Add(desc.Port + ": " + desc.Description);
                    }
                }
            }
            catch (Exception)
            {
            }
            return ports;
        }

        public int SendCommand(byte[] command, out byte[] replyBuffer)
        {
            var replyLength = 0;
            replyBuffer = new byte[1024];
            try
            {
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();

                serialPort.Write(command, 0, command.Length);

                serialPort.Flush();

                replyLength = serialPort.Read(replyBuffer, 0, replyBuffer.Length);
            }
            catch (Exception)
            {
            }
            return replyLength;
        }

        public Button ReadButton(byte button, int maxNumKeyCodes)
        {
            Button key = null;

            try
            {
                var sendBuffer = new byte[3];
                sendBuffer[0] = SerialStartByte;
                sendBuffer[1] = ReadButtonsCommand;
                sendBuffer[2] = button;
                var numBytes = SendCommand(sendBuffer, out byte[] replyBuffer);

                if (numBytes > 0)
                {
                    if (replyBuffer[0] == SerialStartByte && replyBuffer[1] == ReadButtonsCommand)
                    {
                        key = new Button
                        {
                            ButtonIndex = button
                        };

                        byte bufferIndex = 3;
                        for (int keycodeIndex = 0; keycodeIndex < maxNumKeyCodes; keycodeIndex++)
                        {
                            if (replyBuffer[bufferIndex] == 0)
                            {
                                //no further data
                                break;
                            }

                            var keyStroke = new KeyStroke
                            {
                                KeyCodeType = (KeyCodeType)replyBuffer[bufferIndex],
                                KeyCode = (UInt16)((replyBuffer[bufferIndex + 1] << 8) | replyBuffer[bufferIndex + 2])
                            };
                            bufferIndex += 3;

                            key.KeyStrokes.Add(keyStroke);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return key;
        }

        public EncoderControl[] ReadEncoder(byte encoderIndex, byte maxNumKeystrokesPerButton)
        {
            var encoderControls = new List<EncoderControl>();

            try
            {
                var sendBuffer = new byte[3];
                sendBuffer[0] = SerialStartByte;
                sendBuffer[1] = ReadEncodersCommand;
                sendBuffer[2] = encoderIndex;
                var numBytes = SendCommand(sendBuffer, out byte[] replyBuffer);

                if (numBytes > 0)
                {
                    if (replyBuffer[0] == SerialStartByte && replyBuffer[1] == ReadEncodersCommand)
                    {
                        //do the button
                        var encoderControl = new EncoderControl
                        {
                            EncoderIndex = encoderIndex,
                            EncoderControlType = EncoderControlType.Button
                        };

                        byte bufferIndex = 3;
                        for (int keystrokeIndex = 0; keystrokeIndex < maxNumKeystrokesPerButton; keystrokeIndex++)
                        {
                            if (replyBuffer[bufferIndex] != 0)
                            {
                                var keyStroke = new KeyStroke
                                {
                                    KeyCodeType = (KeyCodeType)replyBuffer[bufferIndex],
                                    KeyCode = (UInt16)((replyBuffer[bufferIndex + 1] << 8) | replyBuffer[bufferIndex + 2])
                                };

                                encoderControl.KeyStrokes.Add(keyStroke);
                            }

                            bufferIndex += 3;
                        }
                        //add the button
                        encoderControls.Add(encoderControl);

                        //do the clockwise control
                        encoderControl = new EncoderControl
                        {
                            EncoderIndex = encoderIndex,
                            EncoderControlType = EncoderControlType.Clockwise
                        };

                        for (int keystrokeIndex = 0; keystrokeIndex < maxNumKeystrokesPerButton; keystrokeIndex++)
                        {
                            if (replyBuffer[bufferIndex] != 0)
                            {
                                var keyStroke = new KeyStroke
                                {
                                    KeyCodeType = (KeyCodeType)replyBuffer[bufferIndex],
                                    KeyCode = (UInt16)((replyBuffer[bufferIndex + 1] << 8) | replyBuffer[bufferIndex + 2])
                                };

                                encoderControl.KeyStrokes.Add(keyStroke);
                            }
                            bufferIndex += 3;
                        }
                        //add the clockwise control
                        encoderControls.Add(encoderControl);

                        //do the anticlockwise control
                        encoderControl = new EncoderControl
                        {
                            EncoderIndex = encoderIndex,
                            EncoderControlType = EncoderControlType.AntiClockwise
                        };

                        for (int keystrokeIndex = 0; keystrokeIndex < maxNumKeystrokesPerButton; keystrokeIndex++)
                        {
                            if (replyBuffer[bufferIndex] != 0)
                            {
                                var keyStroke = new KeyStroke
                                {
                                    KeyCodeType = (KeyCodeType)replyBuffer[bufferIndex],
                                    KeyCode = (UInt16)((replyBuffer[bufferIndex + 1] << 8) | replyBuffer[bufferIndex + 2])
                                };

                                encoderControl.KeyStrokes.Add(keyStroke);
                            }
                            bufferIndex += 3;
                        }
                        //add the anticlockwise control
                        encoderControls.Add(encoderControl);
                    }
                }
            }
            catch (Exception)
            {
            }

            return encoderControls.ToArray();
        }

        public KeyboardSettings GetKeyboardSettings()
        {
            KeyboardSettings settings = null;

            try
            {
                var sendBuffer = new byte[2];
                sendBuffer[0] = SerialStartByte;
                sendBuffer[1] = GetSettingsCommand;

                var numBytes = SendCommand(sendBuffer, out byte[] replyBuffer);

                if (numBytes > 0)
                {
                    if (replyBuffer[0] == SerialStartByte && replyBuffer[1] == GetSettingsCommand)
                    {
                        settings = new KeyboardSettings
                        {
                            NumButtons = replyBuffer[2],
                            NumEncoders = replyBuffer[3],
                            MaxNumKeystrokesPerButton = replyBuffer[4]
                        };
                    }
                }
            }
            catch (Exception)
            {
            }

            return settings;
        }

        public bool WriteButton(byte buttonIndex, Button key)
        {
            var replyLength = 0;

            try
            {
                var buffer = new byte[40];
                buffer[0] = SerialStartByte;
                buffer[1] = WriteButtonsCommand;
                buffer[2] = buttonIndex;

                var bufferIndex = 3;
                foreach (var cmd in key.KeyStrokes)
                {
                    buffer[bufferIndex] = (byte)cmd.KeyCodeType;
                    buffer[bufferIndex + 1] = (byte)((cmd.KeyCode & 0xFF00) >> 8);
                    buffer[bufferIndex + 2] = (byte)((cmd.KeyCode & 0xFF));
                    bufferIndex += 3;
                }
                replyLength = SendCommand(buffer, out _);
            }
            catch (Exception)
            {
            }

            return replyLength > 0;
        }
    }
}