#include <FastLED.h>
#include <FastCRC_tables.h>
#include <FastCRC_cpu.h>
#include <FastCRC.h>
#include <Encoder.h>
#include <EEPROM.h>

#define TEENSY_BUILD

#ifdef TEENSY_BUILD
#include <Keyboard.h>

#else
#include "HID-Project.h"
#endif // asds

#include <AceButton.h>
#include "version.h"

using namespace ace_button;

constexpr auto DEBOUNCE_DELAY = 10;
constexpr auto SERIAL_BAUD_RATE = 9600;
constexpr byte EEPROM_DATA_START = 0x0;
constexpr uint16_t EEPROM_INITIALISED = 0x1DEA;
////===========================================================
//keyboard hardware definition - enter config for your hardware
//=============================================================

#ifdef TEENSY_BUILD

#else

constexpr byte NumButtons = 9;
constexpr byte ButtonPins[] = { 10, 9,8,7,6,5,4,3,2 }; //normal pressy buttons
constexpr byte NumEncoders = 1;
constexpr byte EncoderPins[] = { 16, 15, 14 }; //encoder clicky button, then the 2 rotary encoder pins

constexpr unsigned short EepromSize = 1024; //size for your arduino
constexpr auto MAX_COMMANDS_PER_BUTTON = (EepromSize - 24) / ((NumButtons + NumEncoders * 3) * StorageBytesPerCommand);

//suitable for a single keystroke array
constexpr auto SERIAL_BUFFER_LENGTH = MAX_COMMANDS_PER_BUTTON * 3 + 10;

#endif // TEENSY_BUILD

//=================================
//=================================

//Commands
typedef	enum
{
	NoCommand = 0,
	KeyboardCommand = 1,
	DelayCommand = 2,
	LedLightSingle = 3,
	LedClearAll = 4,
	LedCustomPattern = 5
} CommandType;

typedef enum
{
	Press = 0,
	Release = 1,
	PressAndRelease = 2
} PressTypeEnum;

typedef struct
{
	PressTypeEnum PressType;
	uint16_t KeyCode;
} KeyboardCommandStruct;

typedef struct
{
	byte LedIndex;
	CRGB Colour;
	byte DurationTenthsSec;
} LedLightSingleStruct;

typedef struct
{
	AceButton aceButton;
	byte pin;
	byte index;
} ButtonStruct;

typedef struct
{
	AceButton aceButton;
	Encoder* encoder;
	byte step;
} EncoderStruct;

void HandleButtonEvent(AceButton*, uint8_t, uint8_t);

uint16_t LookupTableStartAddress;
uint16_t SettingsTableStartAddress;

constexpr byte NumLeds = 10;
constexpr uint8_t LED_CLOCK_PIN = 11;
constexpr uint8_t LED_DATA_PIN = 12;

byte NumButtons = 0;
byte* ButtonPins;// = { 23,22, 21, 20, 19, 18, 17, 16, 15, 14 }; //normal pressy buttons
byte NumEncoders = 0;
byte* EncoderPins;// [] = { 5, 6, 7, 8, 9, 10 }; //encoder clicky button, then the 2 rotary encoder pins

EncoderStruct* encoders;
ButtonStruct* buttons;
CRGB* ledArray;
byte ledChipset;
void Initialise()
{
	uint16_t initialised;
	EEPROM.get(0, initialised);
	if (initialised == EEPROM_INITIALISED)
	{
		//get settings
		EEPROM.get(2, LookupTableStartAddress);
		EEPROM.get(4, SettingsTableStartAddress);

		byte NumLeds = EEPROM.read(6);
		if (NumLeds > 0)
		{
			ledArray = new CRGB[NumLeds];
			FastLED.addLeds<APA102, LED_DATA_PIN, LED_CLOCK_PIN, BGR>(ledArray, NumLeds);
		}

		NumButtons = EEPROM.read(7);
		if (NumButtons > 0)
		{
			ButtonPins = new byte[NumButtons];
			EEPROM.get(8, ButtonPins);
			buttons = new ButtonStruct[NumButtons];
		}

		NumEncoders = EEPROM.read(8 + NumButtons);
		if (NumEncoders > 0)
		{
			EncoderPins = new byte[NumEncoders * 3];
			EEPROM.get(9 + NumButtons, EncoderPins);
			encoders = new EncoderStruct[NumEncoders];
		}

		//settings table
		for (size_t i = 0; i < NumEncoders; i++)
		{
			encoders[i].step = EEPROM.read(SettingsTableStartAddress + i);
		}

		//initilaise buttons and encoders
		for (byte buttonIndex = 0; buttonIndex < NumButtons; buttonIndex++)
		{
			//set button pin for input
			pinMode(ButtonPins[buttonIndex], INPUT_PULLUP);
			buttons[buttonIndex].aceButton.init(ButtonPins[buttonIndex], HIGH, buttonIndex);
			buttons[buttonIndex].aceButton.setEventHandler(HandleButtonEvent);
			buttons[buttonIndex].index = buttonIndex;
		}

		int encoderPinIndex = 0;
		for (byte encoderIndex = 0; encoderIndex < NumEncoders; encoderIndex++)
		{
			//set button pin for input
			pinMode(EncoderPins[encoderPinIndex], INPUT_PULLUP);
			pinMode(EncoderPins[encoderPinIndex + 1], INPUT_PULLUP);
			pinMode(EncoderPins[encoderPinIndex + 2], INPUT_PULLUP);

			encoders[encoderIndex].aceButton.init(EncoderPins[encoderPinIndex], HIGH, encoderIndex + 100);
			encoders[encoderIndex].aceButton.setEventHandler(HandleButtonEvent);

			encoders[encoderIndex].encoder = new Encoder(EncoderPins[encoderPinIndex + 1], EncoderPins[encoderPinIndex + 2]);
			encoderPinIndex += 3;
		}
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
	//buttons[3].keystrokes[0].pressType = Press;
	//buttons[3].keystrokes[1].commandType = KeyboardKeycodeType;
	//buttons[3].keystrokes[1].command = KEY_E;
	//buttons[3].keystrokes[1].pressType = Press;
	//buttons[3].keystrokes[2].commandType = KeyboardKeycodeType;
	//buttons[3].keystrokes[2].command = KEY_C;
	//buttons[3].keystrokes[2].pressType = pressAndRelease;

	//SaveConfigToEEPROM();
}

void clearLeds()
{
	for (size_t i = 0; i < 10; i++)
	{
		ledArray[i] = CRGB(0, 0, 0);
	}
	FastLED.show();
}

void HandleButtonEvent(AceButton* aceButton, uint8_t eventType, uint8_t /* buttonState */)
{
	switch (eventType)
	{
	case AceButton::kEventPressed:
		RunButtonCommands(aceButton->getId());

		break;
	case AceButton::kEventReleased:
		break;
	}
}

//get the eeprom address of this button's commands
//found in a lookup table
//buttons first, then encoder buttons
int LookupButtonCommandArrayAddress(byte buttonIndex)
{
	int address = 0;
	if (buttonIndex < 100)
	{
		address = LookupTableStartAddress + (2 * buttonIndex);
	}
	else
	{
		//after the buttons - encoders.
		address = LookupTableStartAddress + (2 * NumButtons) + (6 * (buttonIndex - 100));
	}

	int commandArrayStart;
	EEPROM.get(address, commandArrayStart);
	return commandArrayStart;
}

//encoders are after the buttons, encoder button then clockwise step, then anti-clickwise step
int LookupEncoderCommandArrayAddress(byte encoderIndex, bool isClockwise)
{
	int lookupTableAddress = LookupTableStartAddress + (2 * NumButtons);

	lookupTableAddress += (encoderIndex * 6) + 2 + (isClockwise ? 0 : 2);

	int commandArrayStart;
	EEPROM.get(lookupTableAddress, commandArrayStart);
	return commandArrayStart;
}

//find start of this button's commands in the eeprom
//and run the commands for it
void RunButtonCommands(byte buttonIndex)
{
	int commandAddress = LookupButtonCommandArrayAddress(buttonIndex);
	//get a command type first, then whatever data follows it
	do
	{
		commandAddress = GetCommandAndRunIt(commandAddress);
	} while (commandAddress > 0);
}

void RunEncoderCommands(byte encoderIndex, bool isClockwise)
{
	int commandAddress = LookupEncoderCommandArrayAddress(encoderIndex, isClockwise);
	//get a command type first, then whatever data follows it
	do
	{
		commandAddress = GetCommandAndRunIt(commandAddress);
	} while (commandAddress > 0);
}

//get a command, run it if applicable, then return the
//address of the next command
//or zero if no command next
int GetCommandAndRunIt(int address)
{
	int nextAddress = 0;

	CommandType commandType = NoCommand;
	EEPROM.get(address, commandType);

	switch (commandType)
	{
	case NoCommand:
		nextAddress = 0;
		break;
	case KeyboardCommand:
		KeyboardCommandStruct keyboardCommand;
		EEPROM.get(address + 1, keyboardCommand);
		nextAddress = address + sizeof(KeyboardCommandStruct) + 1;
		PressKeys(&keyboardCommand);
		break;
	case DelayCommand:
		uint16_t delayms;
		EEPROM.get(address + 1, delayms);
		nextAddress = address + 3;
		delay(delayms);
		break;
	case LedLightSingle:
		break;
	case LedClearAll:
		break;
	case LedCustomPattern:
		break;

	default:
		break;
	}

	return nextAddress;
}

void PressKeys(KeyboardCommandStruct* keyboardCommand)
{
	switch (keyboardCommand->PressType)
	{
	case Press:
		Keyboard.press(keyboardCommand->KeyCode);
		break;
	case Release:
		Keyboard.release(keyboardCommand->KeyCode);
		break;
	case PressAndRelease:
		Keyboard.press(keyboardCommand->KeyCode);
		Keyboard.release(keyboardCommand->KeyCode);
		break;
	default:
		break;
	}
}

//read encoders and make the buttons check for presses
void CheckButtons()
{
	for (byte encoderIndex = 0; encoderIndex < NumEncoders; encoderIndex++)
	{
		int encoderVal = encoders[encoderIndex].encoder->read();
		if (encoderVal > encoders[encoderIndex].step)
		{
			//+ve so clockwise
			RunEncoderCommands(encoderIndex, true);
			encoders[encoderIndex].encoder->write(0);
		}
		else if (encoderVal < -encoders[encoderIndex].step)
		{
			//-ve so anti-clockwise
			RunEncoderCommands(encoderIndex, false);
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

constexpr byte QuerySettingsCommand = 0x01;
constexpr byte ReadEepromCommand = 0x02;
constexpr byte WriteEepromCommand = 0x03;
constexpr byte ResetCommand = 0x04;
constexpr byte SERIAL_START_BYTE = 0xfe;

void(*resetFunc) (void) = 0;

//get commands from serial buffer
void ProcessIncomingCommands()
{
	if (Serial.available() > 0)
	{
		//read the command
		char serialBuffer[2];
		Serial.readBytes(serialBuffer, 2);

		if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == ReadEepromCommand)
		{
			//send the eeprom to the PC
			ReadEEPROM();
		}
		else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == WriteEepromCommand)
		{
			//recieve the eeprom from the PC
			WriteEEPROM();
		}
		else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == QuerySettingsCommand)
		{
			//send basic settings, also ised to detect if the keyboard is connected
			SendKeyboardSettings();
		}
		else if (serialBuffer[0] == SERIAL_START_BYTE && serialBuffer[1] == ResetCommand)
		{
			//reset
			resetFunc();
		}
	}
}

//send config to the pc
void ReadEEPROM()
{
	FastCRC16 CRC16;
	uint16_t crc;

	//echo command back
	byte serialBuffer[2];
	serialBuffer[0] = SERIAL_START_BYTE;
	serialBuffer[1] = ReadEepromCommand;
	Serial.write(serialBuffer, 2);;

	crc = CRC16.kermit(serialBuffer, 2);

	//write the rest of the eeprom
	for (int i = EEPROM_DATA_START; i < EEPROM.length(); i++) {
		serialBuffer[0] = EEPROM.read(i);
		Serial.write(serialBuffer, 1);
		crc = CRC16.kermit_upd(serialBuffer, 1);
	}

	//write the crc
	Serial.write(((byte)((crc & 0xFF00) >> 8)));
	Serial.write(((byte)(crc & 0xFF)));
}

//get everything and write to eeprom
void WriteEEPROM()
{
	FastCRC16 CRC16;

	int address = EEPROM_DATA_START;
	char serialBuffer;

	uint16_t crc;

	//get first char and start crc
	Serial.readBytes(&serialBuffer, 1);
	crc = CRC16.ccitt((uint8_t*)&serialBuffer, 1);
	do
	{
		EEPROM.write(address, serialBuffer);
		address++;

		crc = CRC16.ccitt_upd((uint8_t*)&serialBuffer, 1);
	} while (Serial.readBytes(&serialBuffer, 1) > 0);

	//echo command back
	Serial.write((byte)0xEE);
	Serial.write((byte)WriteEepromCommand);
	SendInt16(crc);
}

//send a crc back
void SendInt16(uint16_t crc)
{
	byte serialBuffer[2];
	serialBuffer[0] = ((byte)((crc & 0xFF00) >> 8));
	serialBuffer[1] = ((byte)(crc & 0xFF));

	Serial.write(serialBuffer, 2);
}

void SendKeyboardSettings()
{
	FastCRC16 CRC16;
	uint16_t crc;
	uint16_t eepromLen = EEPROM.length();
	byte serialBuffer[4];

	serialBuffer[0] = SERIAL_START_BYTE;
	serialBuffer[1] = QuerySettingsCommand;
	serialBuffer[2] = ((byte)((eepromLen & 0xFF00) >> 8));
	serialBuffer[3] = ((byte)(eepromLen & 0xFF));

	Serial.write(serialBuffer, 4);

	crc = CRC16.kermit((uint8_t*)serialBuffer, 4);

	for (unsigned int i = 0; i < version.length(); i++)
	{
		serialBuffer[0] = version.charAt(i);
		Serial.write(serialBuffer, 1);
		crc = CRC16.kermit_upd((uint8_t*)&serialBuffer, 1);
	}

	SendInt16(crc);
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

// ===========================================
// main arduino functions
// ===========================================
void setup()
{
#ifdef TEENSY_BUILD
#else
	Consumer.begin();
	Keyboard.begin();
	System.begin();
#endif

	Serial.begin(SERIAL_BAUD_RATE);
	Serial.setTimeout(10);

	Initialise();

	Serial.println();
	Serial.print("Version ");
	Serial.println(version);

	FastCRC16 CRC16a;

	//initialise with 0
	uint8_t c[] = { 254 };
	uint16_t crc = CRC16a.kermit(&c[0], 1);
	//c[0] = 254;
	//crc = CRC16a.kermit_upd(&c[0], 1);
	Serial.println(crc);
}

void loop()
{
	CheckButtons();

	ProcessIncomingCommands();
}