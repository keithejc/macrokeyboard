#include <Encoder.h>
#include <EEPROM.h>
#include "HID-Project.h"
#include <AceButton.h>
#include "version.h"

using namespace ace_button;

constexpr auto DEBOUNCE_DELAY = 10;
constexpr auto SERIAL_BAUD_RATE = 9600;
constexpr byte EEPROM_INTEGRITY_BYTE = 123;
constexpr byte EEPROM_DATA_START = 0x10;

constexpr byte SERIAL_START_BYTE = 0xEE;
constexpr byte SERIAL_END_BYTE = 0xFF;
constexpr byte SERIAL_SUCCESS_BYTE = 0xBB;

constexpr byte QuerySettingsCommand = 0x00;
constexpr byte ReadButtonCommand = 0x01;
constexpr byte ReadEncoderCommand = 0x02;
constexpr byte WriteButtonCommand = 0x03;
constexpr byte WriteEncoderButtonCommand = 0x04;
constexpr byte WriteEncoderClockwiseCommand = 0x05;
constexpr byte WriteEncoderAntiClockwiseCommand = 0x06;

constexpr byte KeyboardKeycodeType = 0x01;
constexpr byte ConsumerKeycodeType = 0x02;
constexpr byte SystemKeycodeType = 0x03;
constexpr byte DelayKeycodeType = 0x00;

constexpr byte StorageBytesPerCommand = 3;

////===========================================================
//keyboard hardware definition - enter config for your hardware
//=============================================================
constexpr byte NumButtons = 9;
constexpr byte ButtonPins[] = { 10, 9,8,7,6,5,4,3,2 }; //normal pressy buttons
constexpr byte NumEncoders = 1;
constexpr byte EncoderPins[] = { 16, 15, 14 }; //encoder clicky button, then the 2 rotary encoder pins

constexpr unsigned short EepromSize = 1024; //size for your arduino
constexpr auto MAX_COMMANDS_PER_BUTTON = (EepromSize - 24) / ((NumButtons + NumEncoders * 3) * StorageBytesPerCommand);

//suitable for a single keystroke array
constexpr auto SERIAL_BUFFER_LENGTH = MAX_COMMANDS_PER_BUTTON * 3 + 10;

//=================================
//=================================

typedef enum
{
	press = 0,
	release = 1,
	pressAndRelease = 2
} PressType;

typedef struct
{
	PressType pressType : 4;
	unsigned int commandType : 4;
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

	//buttons[0].keystrokes[0].commandType = ConsumerKeycodeType;
	//buttons[0].keystrokes[0].command = MEDIA_PREV;

	//buttons[1].keystrokes[0].commandType = ConsumerKeycodeType;
	//buttons[1].keystrokes[0].command = MEDIA_NEXT;

	//buttons[2].keystrokes[0].commandType = ConsumerKeycodeType;
	//buttons[2].keystrokes[0].command = CONSUMER_CALCULATOR;

	//buttons[7].keystrokes[0].commandType = SystemKeycodeType;
	//buttons[7].keystrokes[0].command = SYSTEM_SLEEP;

	//buttons[8].keystrokes[0].commandType = SystemKeycodeType;
	//buttons[8].keystrokes[0].command = SYSTEM_POWER_DOWN;

	//encoders[0].buttonKeystrokes[0].commandType = ConsumerKeycodeType;
	//encoders[0].buttonKeystrokes[0].command = MEDIA_PLAY_PAUSE;
	//encoders[0].clockwiseKeystrokes[0].commandType = ConsumerKeycodeType;
	//encoders[0].clockwiseKeystrokes[0].command = MEDIA_VOLUME_UP;
	//encoders[0].antiClockwiseKeystrokes[0].commandType = ConsumerKeycodeType;
	//encoders[0].antiClockwiseKeystrokes[0].command = MEDIA_VOLUME_DOWN;

	//buttons[3].keystrokes[0].commandType = KeyboardKeycodeType;
	//buttons[3].keystrokes[0].command = KEY_LEFT_CTRL;
	//buttons[3].keystrokes[0].pressType = press;
	//buttons[3].keystrokes[1].commandType = KeyboardKeycodeType;
	//buttons[3].keystrokes[1].command = KEY_E;
	//buttons[3].keystrokes[1].pressType = press;
	//buttons[3].keystrokes[2].commandType = KeyboardKeycodeType;
	//buttons[3].keystrokes[2].command = KEY_C;
	//buttons[3].keystrokes[2].pressType = pressAndRelease;

	//SaveConfigToEEPROM();
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
		break;
	}
}

void PressKeystrokes(keystroke* keystrokes, int numKeystrokes)
{
	for (byte ksIndex = 0; ksIndex < numKeystrokes; ksIndex++)
	{
		if (keystrokes[ksIndex].command == 0)
		{
			break;
		}

		switch (keystrokes[ksIndex].commandType)
		{
		case SystemKeycodeType:
			System.write((SystemKeycode)keystrokes[ksIndex].command);
			break;
		case ConsumerKeycodeType:
			Consumer.write((ConsumerKeycode)keystrokes[ksIndex].command);
			break;
		case KeyboardKeycodeType:
			switch (keystrokes[ksIndex].pressType)
			{
			case press:
				Keyboard.press((KeyboardKeycode)keystrokes[ksIndex].command);
				break;
			case release:
				Keyboard.release((KeyboardKeycode)keystrokes[ksIndex].command);
				break;
			case pressAndRelease:
				Keyboard.write((KeyboardKeycode)keystrokes[ksIndex].command);
				break;
			default:
				break;
			}

			break;
		case DelayKeycodeType:
			delay(keystrokes[ksIndex].command);
			break;
		default:
			break;
		}
	}

	//cleanup if a button is left pressed
	Keyboard.releaseAll();
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
			encoders[encoderIndex].encoder->write(0);
		}
		else if (encoderVal > 0)
		{
			PressKeystrokes(encoders[encoderIndex].antiClockwiseKeystrokes, MAX_COMMANDS_PER_BUTTON);
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
	if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == ReadButtonCommand)
	{
		SendButtonConfig(serialBuffer[2]);
	}
	if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == ReadEncoderCommand)
	{
		SendEncoderConfig(serialBuffer[2]);
	}
	else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == WriteButtonCommand)
	{
		ReceiveButtonConfig(serialBuffer[2]);
	}
	else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == WriteEncoderButtonCommand)
	{
		ReceiveEncoderButtonConfig(serialBuffer[2]);
	}
	else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == WriteEncoderClockwiseCommand)
	{
		ReceiveEncoderClockwiseConfig(serialBuffer[2]);
	}
	else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == WriteEncoderAntiClockwiseCommand)
	{
		ReceiveEncoderAntiClockwiseConfig(serialBuffer[2]);
	}
	else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == QuerySettingsCommand)
	{
		SendKeyboardSettings();
	}

	WipeSerialBuffer();
}

//send button config to the pc
void SendButtonConfig(int buttonIndex)
{
	if (buttonIndex < NumButtons)
	{
		Serial.write((byte)0xEE);
		Serial.write((byte)ReadButtonCommand);
		Serial.write((byte)buttonIndex);
		sendKeystrokes(buttons[buttonIndex].keystrokes);
	}
}

//read encoder button config from the serial buffer
void ReceiveEncoderButtonConfig(int encoderIndex)
{
	if (encoderIndex < NumEncoders)
	{
		byte serialIndex = 3;
		DecodeKeystrokes(&serialIndex, encoders[encoderIndex].buttonKeystrokes);

		SendSuccess(encoderIndex);

		SaveConfigToEEPROM();
	}
}

void sendKeystrokes(keystroke keystrokes[])
{
	for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
	{
		Serial.write(keystrokes[cmdIndex].pressType);
		Serial.write(keystrokes[cmdIndex].commandType);
		Serial.write((byte)((keystrokes[cmdIndex].command & 0xFF00) >> 8));
		Serial.write((byte)(keystrokes[cmdIndex].command & 0xFF));
	}
}

//send encoder config command. doesn't use the serial buffer as the buffer
//would have to be too big for the memory
void SendEncoderConfig(byte encoderIndex)
{
	if (encoderIndex < NumEncoders)
	{
		Serial.write((byte)0xEE);
		Serial.write((byte)ReadEncoderCommand);
		Serial.write((byte)encoderIndex);
		sendKeystrokes(encoders[encoderIndex].buttonKeystrokes);
		sendKeystrokes(encoders[encoderIndex].clockwiseKeystrokes);
		sendKeystrokes(encoders[encoderIndex].antiClockwiseKeystrokes);
	}
}

//read encoder clockwise config from the serial buffer
void ReceiveEncoderClockwiseConfig(int encoderIndex)
{
	if (encoderIndex < NumEncoders)
	{
		byte serialIndex = 3;
		DecodeKeystrokes(&serialIndex, encoders[encoderIndex].clockwiseKeystrokes);

		SendSuccess(encoderIndex);

		SaveConfigToEEPROM();
	}
}

//read encoder clockwise config from the serial buffer
void ReceiveEncoderAntiClockwiseConfig(int encoderIndex)
{
	if (encoderIndex < NumEncoders)
	{
		byte serialIndex = 3;
		DecodeKeystrokes(&serialIndex, encoders[encoderIndex].antiClockwiseKeystrokes);

		SendSuccess(encoderIndex);

		SaveConfigToEEPROM();
	}
}

//read button config from the serial buffer
void ReceiveButtonConfig(int buttonIndex)
{
	if (buttonIndex < NumButtons)
	{
		byte serialIndex = 3;
		DecodeKeystrokes(&serialIndex, buttons[buttonIndex].keystrokes);

		SendSuccess(buttonIndex);

		SaveConfigToEEPROM();
	}
}

void DecodeKeystrokes(byte* serialIndex, keystroke* keystrokes)
{
	WipeKeystrokes(keystrokes);
	for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
	{
		//get command type then command from 2 bytes of serial data
		keystrokes[cmdIndex].pressType = (PressType)serialBuffer[*serialIndex];
		keystrokes[cmdIndex].commandType = serialBuffer[*serialIndex + 1];
		keystrokes[cmdIndex].command = serialBuffer[*serialIndex + 2] << 8;
		keystrokes[cmdIndex].command |= serialBuffer[*serialIndex + 3];
		*serialIndex += 4;
	}
}
void WipeKeystrokes(keystroke* keystrokes)
{
	for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
	{
		keystrokes[cmdIndex].pressType = press;
		keystrokes[cmdIndex].commandType = 0;
		keystrokes[cmdIndex].command = 0;
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
	serialBuffer[1] = QuerySettingsCommand;
	serialBuffer[2] = NumButtons;
	serialBuffer[3] = NumEncoders;
	serialBuffer[4] = MAX_COMMANDS_PER_BUTTON;
	for (int i = 0; i < version.length(); i++)
	{
		serialBuffer[i + 5] = version.charAt(i);
	}
	Serial.write(serialBuffer, 5 + version.length());
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
		arr[i].pressType = pressAndRelease;
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
			EEPROM.put(address, buttons[buttonIndex].keystrokes[cmdIndex]);
			address += sizeof(keystroke);
		}
	}
	for (byte encoderIndex = 0; encoderIndex < NumEncoders; encoderIndex++)
	{
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			EEPROM.put(address, encoders[encoderIndex].buttonKeystrokes[cmdIndex]);
			address += sizeof(keystroke);
		}
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			EEPROM.put(address, encoders[encoderIndex].clockwiseKeystrokes[cmdIndex]);
			address += sizeof(keystroke);
		}
		for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
		{
			EEPROM.put(address, encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex]);
			address += sizeof(keystroke);
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
				EEPROM.get(address, buttons[buttonIndex].keystrokes[cmdIndex]);
				address += sizeof(keystroke);
			}
		}
		for (byte encoderIndex = 0; encoderIndex < NumEncoders; encoderIndex++)
		{
			for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
			{
				EEPROM.get(address, encoders[encoderIndex].buttonKeystrokes[cmdIndex]);
				address += sizeof(keystroke);
			}
			for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
			{
				EEPROM.get(address, encoders[encoderIndex].clockwiseKeystrokes[cmdIndex]);
				address += sizeof(keystroke);
			}
			for (byte cmdIndex = 0; cmdIndex < MAX_COMMANDS_PER_BUTTON; cmdIndex++)
			{
				EEPROM.get(address, encoders[encoderIndex].antiClockwiseKeystrokes[cmdIndex]);
				address += sizeof(keystroke);
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

	Serial.println();
	Serial.print("Version ");
	Serial.println(version);

	Serial.print("MAX_COMMANDS_PER_BUTTON ");
	Serial.println(MAX_COMMANDS_PER_BUTTON);

	Serial.print("SERIAL_BUFFER_LENGTH ");
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