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
        private const byte SerialStartByte = 0xee;
        private const byte GetSettingsCommand = 0x00;
        private const byte ReadButtonCommand = 0x01;
        private const byte ReadEncoderCommand = 0x02;
        private const byte WriteButtonCommand = 0x03;
        private const byte WriteEncoderButtonCommand = 0x04;
        private const byte WriteEncoderClockwiseCommand = 0x05;
        private const byte WriteEncoderAntiClockwiseCommand = 0x06;

        private string keyboardPort = "";

        public bool FindKeyboardPort()
        {
            var ports = GetPorts(false);
            foreach (var port in ports)
            {
                Console.WriteLine("Investigate port " + port);
                try
                {
                    //test opening the port
                    var serialPort = OpenPort(port);
                    if (serialPort != null)
                    {
                        Console.WriteLine("Openned port " + port);
                        //success - now close it so sendcommand can open it again
                        serialPort.Close();
                        serialPort.Dispose();
                        serialPort = null;

                        keyboardPort = port;
                        if (GetKeyboardSettings() != null)
                        {
                            Console.WriteLine("Found keyboard " + port);
                            break;
                        }
                        else
                        {
                            keyboardPort = "";
                        }
                    }
                }
                catch (Exception)
                {
                    keyboardPort = "";
                }
            }

            return keyboardPort != "";
        }

        public SerialPortStream OpenPort(string portName)
        {
            SerialPortStream serialPort = null;

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
                    break;
                }
                catch (Exception e)
                {
                    if (serialPort != null)
                    {
                        if (serialPort.IsOpen)
                        {
                            serialPort.Close();
                        }
                        serialPort.Dispose();
                        serialPort = null;
                    }

                    Console.WriteLine("Exception in OpenPort " + e.Message);
                    retries--;
                }
            }

            return serialPort;
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
                var serialPort = OpenPort(keyboardPort);
                if (serialPort != null)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();

                    serialPort.Write(command, 0, command.Length);

                    serialPort.Flush();

                    replyLength = serialPort.Read(replyBuffer, 0, replyBuffer.Length);

                    serialPort.Close();
                    serialPort.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in SendCommand " + e.Message);
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
                sendBuffer[1] = ReadButtonCommand;
                sendBuffer[2] = button;
                var numBytes = SendCommand(sendBuffer, out byte[] replyBuffer);

                if (numBytes > 0)
                {
                    if (replyBuffer[0] == SerialStartByte && replyBuffer[1] == ReadButtonCommand)
                    {
                        key = new Button
                        {
                            ButtonIndex = button
                        };

                        byte bufferIndex = 3;
                        for (int keycodeIndex = 0; keycodeIndex < maxNumKeyCodes; keycodeIndex++)
                        {
                            var keyStroke = new KeyStroke
                            {
                                PressType = (PressType)replyBuffer[bufferIndex],
                                KeyCodeType = (KeyCodeType)replyBuffer[bufferIndex + 1],
                                KeyCode = (UInt16)((replyBuffer[bufferIndex + 2] << 8) | replyBuffer[bufferIndex + 3])
                            };
                            bufferIndex += 4;
                            if (keyStroke.KeyCode == 0)
                            {
                                break;
                            }
                            else
                            {
                                key.KeyStrokes.Add(keyStroke);
                            }
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
                sendBuffer[1] = ReadEncoderCommand;
                sendBuffer[2] = encoderIndex;
                var numBytes = SendCommand(sendBuffer, out byte[] replyBuffer);

                if (numBytes > 0)
                {
                    if (replyBuffer[0] == SerialStartByte && replyBuffer[1] == ReadEncoderCommand)
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
                            var keyStroke = new KeyStroke
                            {
                                PressType = (PressType)replyBuffer[bufferIndex],
                                KeyCodeType = (KeyCodeType)replyBuffer[bufferIndex + 1],
                                KeyCode = (UInt16)((replyBuffer[bufferIndex + 2] << 8) | replyBuffer[bufferIndex + 3])
                            };

                            if (keyStroke.KeyCode == 0)
                            {
                                break;
                            }
                            else
                            {
                                encoderControl.KeyStrokes.Add(keyStroke);
                            }

                            bufferIndex += 4;
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
                            var keyStroke = new KeyStroke
                            {
                                PressType = (PressType)replyBuffer[bufferIndex],
                                KeyCodeType = (KeyCodeType)replyBuffer[bufferIndex + 1],
                                KeyCode = (UInt16)((replyBuffer[bufferIndex + 2] << 8) | replyBuffer[bufferIndex + 3])
                            };

                            if (keyStroke.KeyCode == 0)
                            {
                                break;
                            }
                            else
                            {
                                encoderControl.KeyStrokes.Add(keyStroke);
                            }

                            bufferIndex += 4;
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
                            var keyStroke = new KeyStroke
                            {
                                PressType = (PressType)replyBuffer[bufferIndex],
                                KeyCodeType = (KeyCodeType)replyBuffer[bufferIndex + 1],
                                KeyCode = (UInt16)((replyBuffer[bufferIndex + 2] << 8) | replyBuffer[bufferIndex + 3])
                            };

                            if (keyStroke.KeyCode == 0)
                            {
                                break;
                            }
                            else
                            {
                                encoderControl.KeyStrokes.Add(keyStroke);
                            }
                            bufferIndex += 4;
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
                        settings.Version = "";
                        for (int i = 5; i < numBytes; i++)
                        {
                            settings.Version += (char)replyBuffer[i];
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return settings;
        }

        public bool WriteButton(Button button)
        {
            var replyLength = 0;

            try
            {
                var buffer = new byte[40];
                buffer[0] = SerialStartByte;

                if (button is EncoderControl encoder)
                {
                    switch (encoder.EncoderControlType)
                    {
                        case EncoderControlType.Button:
                            buffer[1] = WriteEncoderButtonCommand;
                            break;

                        case EncoderControlType.Clockwise:
                            buffer[1] = WriteEncoderClockwiseCommand;
                            break;

                        case EncoderControlType.AntiClockwise:
                            buffer[1] = WriteEncoderAntiClockwiseCommand;
                            break;

                        default:
                            break;
                    }
                    buffer[2] = (byte)encoder.EncoderIndex;
                }
                else
                {
                    buffer[1] = WriteButtonCommand;
                    buffer[2] = (byte)button.ButtonIndex;
                }

                var bufferIndex = 3;
                foreach (var cmd in button.KeyStrokes)
                {
                    buffer[bufferIndex] = (byte)cmd.PressType;
                    buffer[bufferIndex + 1] = (byte)cmd.KeyCodeType;
                    buffer[bufferIndex + 2] = (byte)((cmd.KeyCode & 0xFF00) >> 8);
                    buffer[bufferIndex + 3] = (byte)((cmd.KeyCode & 0xFF));
                    bufferIndex += 4;
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