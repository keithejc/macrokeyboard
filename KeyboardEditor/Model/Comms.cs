using System;
using System.Collections.Generic;
using KeyboardEditor.Model;
using RJCP.IO.Ports;

namespace KeyboardEditor
{
    public class Comms
    {
        private const byte QuerySettingsCommand = 0x01;
        private const byte ReadEepromCommand = 0x02;
        private const byte WriteEepromCommand = 0x03;
        private const byte ResetCommand = 0x04;
        private const byte SerialStartByte = 0xfe;

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

        public bool SendCommand(byte command, byte[] commandData, out byte[] replyData)
        {
            replyData = null;
            var success = false;
            var data = new byte[4096];
            try
            {
                var serialPort = OpenPort(keyboardPort);
                if (serialPort != null)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();

                    var commandBuffer = new byte[2 + commandData.Length];
                    commandBuffer[0] = SerialStartByte;
                    commandBuffer[1] = command;
                    Array.Copy(commandData, 0, commandBuffer, 2, commandData.Length);
                    serialPort.Write(commandBuffer, 0, 2 + commandData.Length);

                    serialPort.Flush();

                    var replyDataLength = serialPort.Read(data, 0, data.Length);

                    serialPort.Close();
                    serialPort.Dispose();

                    //assume last 2 bytes were the crc of the reply
                    if (replyDataLength >= 4)
                    {
                        if (data[0] == SerialStartByte && data[1] == command)
                        {
                            //read crc - the last 2 bytes
                            ushort crcReply = (ushort)(data[replyDataLength - 2] * 256 + data[replyDataLength - 1]);

                            //calc crc - don't inlcude the crc itself
                            var crc = new CrcKermit16(Crc16Mode.CcittKermit);
                            var crcData = new List<byte>();
                            for (int i = 0; i < replyDataLength - 2; i++)
                            {
                                crcData.Add(data[i]);
                            }
                            success = (crc.ComputeChecksum(crcData.ToArray()) == crcReply);

                            //don't include crc or command in data
                            replyDataLength -= 4;

                            //only reply data
                            if (success && replyDataLength > 0)
                            {
                                replyData = new byte[replyDataLength];
                                Array.Copy(data, 2, replyData, 0, replyDataLength);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in SendCommand " + e.Message);
            }

            return success;
        }

        public byte[] ReadEeprom()
        {
            var eeprom = new byte[0];

            try
            {
                if (SendCommand(ReadEepromCommand, new byte[0], out byte[] data))
                {
                    eeprom = data;
                }
            }
            catch (Exception)
            {
            }

            return eeprom;
        }

        public bool WriteEeprom(byte[] eeprom)
        {
            var success = false;
            try
            {
                var crc = new CrcKermit16(Crc16Mode.CcittKermit);
                var checksum = crc.ComputeChecksum(eeprom);

                success = SendCommand(WriteEepromCommand, eeprom, out byte[] data);

                if (success)
                {
                    //check return is success
                    if (data[0] == SerialStartByte && data[1] == WriteEepromCommand)
                    {
                        success = (data[2] * 256 + data[3]) == checksum;
                    }
                }
            }
            catch (Exception)
            {
            }

            return success;
        }

        public KeyboardSettings GetKeyboardSettings()
        {
            KeyboardSettings settings = null;

            try
            {
                if (SendCommand(QuerySettingsCommand, new byte[0], out byte[] data))
                {
                    //read settings
                    settings = new KeyboardSettings
                    {
                        EEPROMLength = data[2] * 256 + data[3]
                    };
                    settings.Version = "";
                    for (int i = 4; i < data.Length - 2; i++)
                    {
                        settings.Version += (char)data[i];
                    }
                }
            }
            catch (Exception)
            {
            }

            return settings;
        }
    }
}