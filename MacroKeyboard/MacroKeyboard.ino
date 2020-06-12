#include <Encoder.h>
#include <EEPROM.h>
#include "HID-Project.h"
#include <AceButton.h>

using namespace ace_button;

constexpr auto DEBOUNCE_DELAY = 10;
constexpr auto SERIAL_BAUD_RATE = 9600;
constexpr byte EEPROM_INTEGRITY_BYTE = 123;
constexpr byte EEPROM_DATA_START = 0x10;

constexpr byte SERIAL_START_BYTE = 0xEE;
constexpr byte SERIAL_END_BYTE = 0xFF;
constexpr byte SERIAL_SUCCESS_BYTE = 0xBB;

constexpr byte SERIAL_READ_BUTTON_COMMAND = 0x00;
constexpr byte SERIAL_WRITE_BUTTON_COMMAND = 0x01;
constexpr byte SERIAL_READ_ENCODER_COMMAND = 0x02;
constexpr byte SERIAL_WRITE_ENCODER_COMMAND = 0x03;
constexpr byte SERIAL_QUERY_SETTINGS_COMMAND = 0x04;

constexpr byte KeyboardKeycodeType = 0x01;
constexpr byte ConsumerKeycodeType = 0x02;
constexpr byte SystemKeycodeType = 0x03;

constexpr byte StorageBytesPerCommand = 2;

////===========================================================
//keyboard hardware definition - enter config for your hardware
//=============================================================
constexpr byte NumButtons = 9;
constexpr byte ButtonPins[] = { 10, 9,8,7,6,5,4,3,2 }; //normal pressy buttons
constexpr byte NumEncoders = 1;
constexpr byte EncoderPins[] = { 16, 15, 14 }; //encoder clicky button, then the 2 rotary encoder pins

constexpr unsigned short EepromSize = 1024; //size for your arduino
constexpr auto MAX_COMMANDS_PER_BUTTON = (EepromSize - 24) / ((NumButtons + NumEncoders * 3) * StorageBytesPerCommand);

constexpr auto SERIAL_BUFFER_LENGTH = MAX_COMMANDS_PER_BUTTON * 3 + 10; //suitable for a button

//=================================
//=================================

typedef struct
{
	byte commandType;
	uint16_t command;
} keystroke;

typedef struct
{
	AceButton aceButton;
	keystroke keystrokes[MAX_COMMANDS_PER_BUTTON];
	byte pin;
	byte index;
} button;

typedef struct
{
	AceButton aceButton;
	keystroke buttonKeystrokes[MAX_COMMANDS_PER_BUTTON];

	Encoder* encoder;
	keystroke clockwiseKeystrokes[MAX_COMMANDS_PER_BUTTON];
	keystroke antiClockwiseKeystrokes[MAX_COMMANDS_PER_BUTTON];
} encoder;

void HandleButtonEvent(AceButton*, uint8_t, uint8_t);

encoder encoders[NumEncoders];
button buttons[NumButtons];

// ===========================================
// Button functions
// ===========================================

void InitButtons()
{
	for (byte buttonIndex = 0; buttonIndex < NumButtons; buttonIndex++)
	{
		//set button pin for input
		pinMode(ButtonPins[buttonIndex], INPUT_PULLUP);
		buttons[buttonIndex].aceButton.init(ButtonPins[buttonIndex], HIGH, buttonIndex);
		buttons[buttonIndex].aceButton.setEventHandler(HandleButtonEvent);

		WipeKeyStrokeArray(buttons[buttonIndex].keystrokes);

		buttons[buttonIndex].index = buttonIndex;
	}

	int encoderPinIndex = 0;
	for (byte encoderIndex = 0; encoderIndex < NumEncoders; encoderIndex++)
	{
		//set button pin for input
		pinMode(EncoderPins[encoderPinIndex], INPUT_PULLUP);
		pinMode(EncoderPins[encoderPinIndex + 1], INPUT_PULLUP);
		pinMode(EncoderPins[encoderPinIndex + 2], INPUT_PULLUP);

		encoders[encoderIndex].aceButton.init(EncoderPins[encoderIndex], HIGH, encoderIndex + 100);
		encoders[encoderIndex].aceButton.setEventHandler(HandleButtonEvent);

		WipeKeyStrokeArray(encoders[encoderIndex].antiClockwiseKeystrokes);
		WipeKeyStrokeArray(encoders[encoderIndex].clockwiseKeystrokes);
		WipeKeyStrokeArray(encoders[encoderIndex].buttonKeystrokes);

		encoders[encoderIndex].encoder = new Encoder(EncoderPins[encoderPinIndex + 1], EncoderPins[encoderPinIndex + 2]);
		encoderPinIndex += 3;
	}

	buttons[0].keystrokes[0].commandType = ConsumerKeycodeType;
	buttons[0].keystrokes[0].command = MEDIA_PREV;

	buttons[1].keystrokes[0].commandType = ConsumerKeycodeType;
	buttons[1].keystrokes[0].command = MEDIA_NEXT;

	buttons[2].keystrokes[0].commandType = ConsumerKeycodeType;
	buttons[2].keystrokes[0].command = CONSUMER_CALCULATOR;

	buttons[7].keystrokes[0].commandType = SystemKeycodeType;
	buttons[7].keystrokes[0].command = SYSTEM_SLEEP;

	buttons[8].keystrokes[0].commandType = SystemKeycodeType;
	buttons[8].keystrokes[0].command = SYSTEM_POWER_DOWN;

	encoders[0].buttonKeystrokes[0].commandType = ConsumerKeycodeType;
	encoders[0].buttonKeystrokes[0].command = MEDIA_PLAY_PAUSE;
	encoders[0].clockwiseKeystrokes[0].commandType = ConsumerKeycodeType;
	encoders[0].clockwiseKeystrokes[0].command = MEDIA_VOLUME_UP;
	encoders[0].antiClockwiseKeystrokes[0].commandType = ConsumerKeycodeType;
	encoders[0].antiClockwiseKeystrokes[0].command = MEDIA_VOLUME_DOWN;

	SaveConfigToEEPROM();
}

void HandleButtonEvent(AceButton* aceButton, uint8_t eventType, uint8_t /* buttonState */)
{
	switch (eventType)
	{
	case AceButton::kEventPressed:
		if (aceButton->getId() >= 100)
		{
			PressKeystrokes(encoders[aceButton->getId() - 100].buttonKeystrokes, MAX_COMMANDS_PER_BUTTON);
		}
		else
		{
			PressKeystrokes(buttons[aceButton->getId()].keystrokes, MAX_COMMANDS_PER_BUTTON);
		}
		break;
	case AceButton::kEventReleased:
		if (aceButton->getId() >= 100)
		{
			ReleaseKeystrokes(encoders[aceButton->getId() - 100].buttonKeystrokes, MAX_COMMANDS_PER_BUTTON);
		}
		else
		{
			ReleaseKeystrokes(buttons[aceButton->getId()].keystrokes, MAX_COMMANDS_PER_BUTTON);
		}
		break;
	}
}

void PressKeystrokes(keystroke* keystrokes, int numKeystrokes)
{
	for (byte ksIndex = 0; ksIndex < numKeystrokes; ksIndex++)
	{
		if (keystrokes[ksIndex].commandType == 0)
		{
			break;
		}

		switch (keystrokes[ksIndex].commandType)
		{
			//system keys don't support chords so they are just written, not pressed down and then released later
		case SystemKeycodeType:
			System.write((SystemKeycode)keystrokes[ksIndex].command);
			break;
		case ConsumerKeycodeType:
			Consumer.press((ConsumerKeycode)keystrokes[ksIndex].command);
			break;
		case KeyboardKeycodeType:
			Keyboard.press(keystrokes[ksIndex].command);
			break;
		default:
			break;
		}
	}
}

void ReleaseKeystrokes(keystroke* keystrokes, int numKeystrokes)
{
	for (byte ksIndex = 0; ksIndex < numKeystrokes; ksIndex++) {
		switch (keystrokes[ksIndex].commandType)
		{
			//system keys don't support chords so releasing isn't relevant
		case ConsumerKeycodeType:
			Consumer.release((ConsumerKeycode)keystrokes[ksIndex].command);
			break;
		case KeyboardKeycodeType:
			Keyboard.release(keystrokes[ksIndex].command);
			break;
		default:
			break;
		}
	}
}

//read encoders and make the buttons check for presses
void CheckButtons()
{
	for (byte encoderIndex = 0; encoderIndex < NumEncoders; encoderIndex++)
	{
		int encoderVal = encoders[encoderIndex].encoder->read();
		if (encoderVal < 0)
		{
			PressKeystrokes(encoders[encoderIndex].clockwiseKeystrokes, MAX_COMMANDS_PER_BUTTON);
			ReleaseKeystrokes(encoders[encoderIndex].clockwiseKeystrokes, MAX_COMMANDS_PER_BUTTON);
			encoders[encoderIndex].encoder->write(0);
		}
		else if (encoderVal > 0)
		{
			PressKeystrokes(encoders[encoderIndex].antiClockwiseKeystrokes, MAX_COMMANDS_PER_BUTTON);
			ReleaseKeystrokes(encoders[encoderIndex].antiClockwiseKeystrokes, MAX_COMMANDS_PER_BUTTON);
			encoders[encoderIndex].encoder->write(0);
		}

		encoders[encoderIndex].aceButton.check();
	}

	for (byte buttonIndex = 0; buttonIndex < NumButtons; buttonIndex++)
	{
		buttons[buttonIndex].aceButton.check();
	}
}

// ===========================================
// Serial comms functions
// ===========================================

byte serialBuffer[SERIAL_BUFFER_LENGTH];
bool ReadSerial()
{
	if (Serial.available() > 0)
	{
		Serial.readBytes(serialBuffer, SERIAL_BUFFER_LENGTH);
		return true;
	}

	return false;
}

//get commands from serial buffer
void ProcessIncomingCommands()
{
	if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == SERIAL_READ_BUTTON_COMMAND)
	{
		SendButtonConfig(serialBuffer[2]);
	}
	else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == SERIAL_WRITE_BUTTON_COMMAND)
	{
		ReceiveButtonConfig(serialBuffer[2]);
	}
	if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == SERIAL_READ_ENCODER_COMMAND)
	{
		SendEncoderConfig(serialBuffer[2]);
	}
	else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == SERIAL_WRITE_ENCODER_COMMAND)
	{
		ReceiveEncoderConfig(serialBuffer[2]);
	}
	else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == SERIAL_QUERY_SETTINGS_COMMAND)
	{
		SendKeyboardSettings();
	}

	WipeSerialBuffer();
}

//send button config command
void SendButtonConfig(int buttonIndex)
{
	if (buttonIndex < NumButtons)
	{
		WipeSerialBuffer();
		serialBuffer[0] = 0xEE;
		serialBuffer[1] = SERIAL_READ_BUTTON_COMMAND;
		serialBuffer[2] = buttonIndex;
		int serialBufferIndex = 3;
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			serialBuffer[serialBufferIndex] = buttons[buttonIndex].keystrokes[cmdIndex].commandType;
			serialBuffer[serialBufferIndex + 1] = (buttons[buttonIndex].keystrokes[cmdIndex].command & 0xFF00) >> 8;
			serialBuffer[serialBufferIndex + 2] = buttons[buttonIndex].keystrokes[cmdIndex].command & 0xFF;
			serialBufferIndex += 3;
		}
		Serial.write(serialBuffer, MAX_COMMANDS_PER_BUTTON * 3 + 3);
	}
}

//send encoder config command
void SendEncoderConfig(byte encoderIndex)
{
	if (encoderIndex < NumEncoders)
	{
		Serial.write((byte)0xEE);
		Serial.write((byte)SERIAL_READ_ENCODER_COMMAND);
		Serial.write((byte)encoderIndex);
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			Serial.write(encoders[encoderIndex].buttonKeystrokes[cmdIndex].commandType);
			Serial.write((byte)((encoders[encoderIndex].buttonKeystrokes[cmdIndex].command & 0xFF00) >> 8));
			Serial.write((byte)(encoders[encoderIndex].buttonKeystrokes[cmdIndex].command & 0xFF));
		}
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			Serial.write(encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].commandType);
			Serial.write((byte)((encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].command & 0xFF00) >> 8));
			Serial.write((byte)(encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].command & 0xFF));
		}
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			Serial.write(encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].commandType);
			Serial.write((byte)((encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].command & 0xFF00) >> 8));
			Serial.write((byte)(encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].command & 0xFF));
		}
	}
}

//read button config from the serial buffer
void ReceiveButtonConfig(int buttonIndex)
{
	if (buttonIndex < NumButtons)
	{
		byte serialIndex = 3;
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			//detect end of data
			if (serialBuffer[serialIndex] == SERIAL_END_BYTE)
			{
				break;
			}
			else
			{
				//get command type then command from 2 bytes of serial data
				buttons[buttonIndex].keystrokes[cmdIndex].commandType = serialBuffer[serialIndex];
				buttons[buttonIndex].keystrokes[cmdIndex].command = serialBuffer[serialIndex + 1] << 8;
				buttons[buttonIndex].keystrokes[cmdIndex].command |= serialBuffer[serialIndex + 2];
				serialIndex += 3;
			}
		}

		SendSuccess(buttonIndex);

		SaveConfigToEEPROM();
	}
}

//read encoder config from the serial buffer
void ReceiveEncoderConfig(int encoderIndex)
{
	if (encoderIndex < NumEncoders)
	{
		byte serialIndex = 3;
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			//detect end of data
			if (serialBuffer[serialIndex] == SERIAL_END_BYTE)
			{
				serialIndex++;
				break;
			}
			else
			{
				//get command type then command from 2 bytes of serial data
				encoders[encoderIndex].buttonKeystrokes[cmdIndex].commandType = serialBuffer[serialIndex];
				encoders[encoderIndex].buttonKeystrokes[cmdIndex].command = serialBuffer[serialIndex + 1] << 8;
				encoders[encoderIndex].buttonKeystrokes[cmdIndex].command |= serialBuffer[serialIndex + 2];
				serialIndex += 3;
			}
		}
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			//detect end of data
			if (serialBuffer[serialIndex] == SERIAL_END_BYTE)
			{
				serialIndex++;
				break;
			}
			else
			{
				//get command type then command from 2 bytes of serial data
				encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].commandType = serialBuffer[serialIndex];
				encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].command = serialBuffer[serialIndex + 1] << 8;
				encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].command |= serialBuffer[serialIndex + 2];
				serialIndex += 3;
			}
		}
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			//detect end of data
			if (serialBuffer[serialIndex] == SERIAL_END_BYTE)
			{
				break;
			}
			else
			{
				//get command type then command from 2 bytes of serial data
				encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].commandType = serialBuffer[serialIndex];
				encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].command = serialBuffer[serialIndex + 1] << 8;
				encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].command |= serialBuffer[serialIndex + 2];
				serialIndex += 3;
			}
		}

		SendSuccess(encoderIndex);

		SaveConfigToEEPROM();
	}
}

//send a success command
void SendSuccess(int index)
{
	WipeSerialBuffer();
	serialBuffer[0] = 0xEE;
	serialBuffer[1] = SERIAL_SUCCESS_BYTE;
	serialBuffer[2] = index;

	Serial.write(serialBuffer, 3);
}

void SendKeyboardSettings()
{
	WipeSerialBuffer();
	serialBuffer[0] = 0xEE;
	serialBuffer[1] = SERIAL_QUERY_SETTINGS_COMMAND;
	serialBuffer[2] = NumButtons;
	serialBuffer[3] = NumEncoders;
	serialBuffer[4] = MAX_COMMANDS_PER_BUTTON;
	Serial.write(serialBuffer, 5);
}

void WipeSerialBuffer()
{
	for (int i = 0; i < SERIAL_BUFFER_LENGTH; i++)
	{
		serialBuffer[i] = 0;
	}
}

void WipeKeyStrokeArray(keystroke* arr)
{
	for (int i = 0; i < MAX_COMMANDS_PER_BUTTON; i++)
	{
		arr[i].command = 0;
		arr[i].commandType = 0;
	}
}

// ===========================================
// EEPROM functions
// ===========================================

void WipeEEPROM()
{
	for (int i = 0; i < EEPROM.length(); i++) {
		EEPROM.write(i, 0);
	}
}

void SaveConfigToEEPROM()
{
	EEPROM.update(0x00, EEPROM_INTEGRITY_BYTE);
	int address = EEPROM_DATA_START;
	for (byte buttonIndex = 0; buttonIndex < NumButtons; buttonIndex++)
	{
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			EEPROM.update(address, (buttons[buttonIndex].keystrokes[cmdIndex].commandType << 6) |
				(buttons[buttonIndex].keystrokes[cmdIndex].command & 0x3F00) >> 8);
			EEPROM.update(address + 1, buttons[buttonIndex].keystrokes[cmdIndex].command & 0xFF);
			address += 2;
		}
	}
	for (byte encoderIndex = 0; encoderIndex < NumEncoders; encoderIndex++)
	{
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			EEPROM.update(address, (encoders[encoderIndex].buttonKeystrokes[cmdIndex].commandType << 6) |
				(encoders[encoderIndex].buttonKeystrokes[cmdIndex].command & 0x3F00) >> 8);
			EEPROM.update(address + 1, encoders[encoderIndex].buttonKeystrokes[cmdIndex].command & 0xFF);
			address += 2;
		}
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			EEPROM.update(address, (encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].commandType << 6) |
				(encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].command & 0x3F00) >> 8);
			EEPROM.update(address + 1, encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].command & 0xFF);
			address += 2;
		}
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			EEPROM.update(address, (encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].commandType << 6) |
				(encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].command & 0x3F00) >> 8);
			EEPROM.update(address + 1, encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].command & 0xFF);
			address += 2;
		}
	}
}

void LoadConfigFromEEPROM()
{
	if (EEPROM.read(0x00) == EEPROM_INTEGRITY_BYTE)
	{
		int address = EEPROM_DATA_START;
		for (byte buttonIndex = 0; buttonIndex < NumButtons; buttonIndex++)
		{
			for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
			{
				buttons[buttonIndex].keystrokes[cmdIndex].commandType = (EEPROM.read(address) & 0xC0) >> 6;
				buttons[buttonIndex].keystrokes[cmdIndex].command = (EEPROM.read(address) & 0x3F) << 8;
				buttons[buttonIndex].keystrokes[cmdIndex].command |= EEPROM.read(address + 1);
				address += 2;
			}
		}
		for (byte encoderIndex = 0; encoderIndex < NumEncoders; encoderIndex++)
		{
			for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
			{
				encoders[encoderIndex].buttonKeystrokes[cmdIndex].commandType = (EEPROM.read(address) & 0xC0) >> 6;
				encoders[encoderIndex].buttonKeystrokes[cmdIndex].command = (EEPROM.read(address) & 0x3F) << 8;
				encoders[encoderIndex].buttonKeystrokes[cmdIndex].command |= EEPROM.read(address + 1);
				address += 2;
			}
			for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
			{
				encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].commandType = (EEPROM.read(address) & 0xC0) >> 6;
				encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].command = (EEPROM.read(address) & 0x3F) << 8;
				encoders[encoderIndex].clockwiseKeystrokes[cmdIndex].command |= EEPROM.read(address + 1);
				address += 2;
			}
			for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
			{
				encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].commandType = (EEPROM.read(address) & 0xC0) >> 6;
				encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].command = (EEPROM.read(address) & 0x3F) << 8;
				encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex].command |= EEPROM.read(address + 1);
				address += 2;
			}
		}
	}
	else
	{
		WipeEEPROM();
		SaveConfigToEEPROM();
	}
}

// ===========================================
// main arduino functions
// ===========================================
void setup()
{
	Consumer.begin();
	Keyboard.begin();
	System.begin();

	Serial.begin(SERIAL_BAUD_RATE, SERIAL_8N1);
	Serial.setTimeout(10);

	InitButtons();
	LoadConfigFromEEPROM();

	Serial.println(" MAX_COMMANDS_PER_BUTTON * 3 + 3 ");
	Serial.println(MAX_COMMANDS_PER_BUTTON * 3 + 3);
	Serial.println(SERIAL_BUFFER_LENGTH);
}

void loop()
{
	CheckButtons();

	if (ReadSerial())
	{
		ProcessIncomingCommands();
	}
}