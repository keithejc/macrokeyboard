#include <FastLED.h>
#include <FastCRC_tables.h>
#include <FastCRC_cpu.h>
#include <FastCRC.h>
#include <Encoder.h>
#include <EEPROM.h>
#include <AceButton.h>

#define TEENSY_BUILD

#ifdef TEENSY_BUILD
#include <Keyboard.h>

#else
#include "HID-Project.h"
#endif // asds

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
		EEPROM.get(2, SettingsTableStartAddress);
		EEPROM.get(4, LookupTableStartAddress);

		byte NumLeds = EEPROM.read(6);
		NumButtons = EEPROM.read(7);
		NumEncoders = EEPROM.read(8);

		if (NumLeds > 0)
		{
			ledArray = new CRGB[NumLeds];
			FastLED.addLeds<APA102, LED_DATA_PIN, LED_CLOCK_PIN, BGR>(ledArray, NumLeds);
		}

		if (NumButtons > 0)
		{
			ButtonPins = new byte[NumButtons];

			for (size_t i = 0; i < NumButtons; i++)
			{
				ButtonPins[i] = EEPROM.read(9 + i);
			}

			buttons = new ButtonStruct[NumButtons];
		}

		if (NumEncoders > 0)
		{
			EncoderPins = new byte[NumEncoders * 3];
			encoders = new EncoderStruct[NumEncoders];

			for (size_t i = 0; i < NumEncoders * 3; i++)
			{
				EncoderPins[i] = EEPROM.read(9 + NumButtons + i);
			}
		}

		//settings table
		for (size_t i = 0; i < NumEncoders; i++)
		{
			encoders[i].step = EEPROM.read(SettingsTableStartAddress + i);
		}

		//initialise buttons and encoders
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
uint16_t LookupButtonCommandArrayAddress(byte buttonIndex)
{
	uint16_t address = 0;
	if (buttonIndex < 100)
	{
		address = LookupTableStartAddress + (2 * buttonIndex);
	}
	else
	{
		//after the buttons - encoders.
		address = LookupTableStartAddress + (2 * NumButtons) + (6 * (buttonIndex - 100));
	}

	uint16_t commandArrayStart;
	EEPROM.get(address, commandArrayStart);
	return commandArrayStart;
}

//encoders are after the buttons, encoder button then clockwise step, then anti-clickwise step
uint16_t LookupEncoderCommandArrayAddress(byte encoderIndex, bool isClockwise)
{
	uint16_t lookupTableAddress = LookupTableStartAddress + (2 * NumButtons);

	lookupTableAddress += (encoderIndex * 6) + 2 + (isClockwise ? 0 : 2);

	uint16_t commandArrayStart;
	EEPROM.get(lookupTableAddress, commandArrayStart);
	return commandArrayStart;
}

//find start of this button's commands in the eeprom
//and run the commands for it
void RunButtonCommands(byte buttonIndex)
{
	uint16_t commandAddress = LookupButtonCommandArrayAddress(buttonIndex);
	//get a command type first, then whatever data follows it
	do
	{
		commandAddress = GetCommandAndRunIt(commandAddress);
	} while (commandAddress > 0);
}

void RunEncoderCommands(byte encoderIndex, bool isClockwise)
{
	uint16_t commandAddress = LookupEncoderCommandArrayAddress(encoderIndex, isClockwise);
	//get a command type first, then whatever data follows it
	do
	{
		commandAddress = GetCommandAndRunIt(commandAddress);
	} while (commandAddress > 0);
}

//get a command, run it if applicable, then return the
//address of the next command
//or zero if no command next
uint16_t GetCommandAndRunIt(uint16_t address)
{
	uint16_t nextAddress = 0;

	CommandType commandType = (CommandType)EEPROM.read(address);

	switch (commandType)
	{
	case NoCommand:
		nextAddress = 0;
		break;
	case KeyboardCommand:
		KeyboardCommandStruct keyboardCommand;
		keyboardCommand.PressType = (PressTypeEnum)EEPROM.read(address + 1);
		EEPROM.get(address + 2, keyboardCommand.KeyCode);
		nextAddress = address + 4;
		PressKeys(&keyboardCommand);
		break;
	case DelayCommand:
		uint16_t delayms;
		EEPROM.get(address + 1, delayms);
		nextAddress = address + 3;
		delay(delayms);
		break;
	case LedLightSingle:
		ledArray[EEPROM.read(address + 1)] = CRGB(
			EEPROM.read(address + 2),
			EEPROM.read(address + 3),
			EEPROM.read(address + 4));
		FastLED.show();
		nextAddress = address + 5;
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
			byte replyBuffer[2];
			//echo command back
			replyBuffer[0] = SERIAL_START_BYTE;
			replyBuffer[1] = ResetCommand;
			Serial.write(replyBuffer, 2);
			FastCRC16 CRC16;
			uint16_t crcReply = CRC16.kermit((uint8_t*)&replyBuffer, 2);
			SendInt16(crcReply);
			delay(2000);
			SCB_AIRCR = 0x05FA0004; // software reset
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

	//write the crc response

	Serial.write(((byte)((crc & 0xFF00) >> 8)));
	Serial.write(((byte)(crc & 0xFF)));
}

//get everything and write to eeprom
void WriteEEPROM()
{
	int address = EEPROM_DATA_START;

	//get first char and start crc
	char serialBuffer[1];
	Serial.readBytes(serialBuffer, 1);
	do
	{
		EEPROM.write(address, serialBuffer[0]);
		address++;
	} while (Serial.readBytes(serialBuffer, 1) > 0);

	//read back eeprom and crc
	FastCRC16 CRC16;
	uint8_t data = EEPROM.read(EEPROM_DATA_START);
	uint16_t crc = CRC16.kermit(&data, 1);
	for (size_t i = EEPROM_DATA_START + 1; i < address; i++)
	{
		data = EEPROM.read(i);
		crc = CRC16.kermit_upd(&data, 1);
	}

	byte replyBuffer[4];
	//echo command back
	replyBuffer[0] = SERIAL_START_BYTE;
	replyBuffer[1] = WriteEepromCommand;
	//send the eeprom crc
	replyBuffer[2] = (crc & 0xFF00) >> 8;
	replyBuffer[3] = crc & 0xFF;
	Serial.write(replyBuffer, 4);
	uint16_t crcReply = CRC16.kermit((uint8_t*)&replyBuffer, 4);
	SendInt16(crcReply);
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
// led functions
// ===========================================
void clearLeds()
{
	if (NumLeds > 0)
	{
		for (size_t i = 0; i < NumLeds; i++)
		{
			ledArray[i] = CRGB(0, 0, 0);
		}
		FastLED.show();
	}
}

void setLed(byte iLed)
{
	ledArray[iLed] = CHSV(2, 255, 255);
	FastLED.show();
}

void fadeLeds()
{
	fadeToBlackBy(ledArray, NumLeds, 10);
	FastLED.show();
}

//do something with the leds
void doLedScreensaver()
{
	//addGlitter(100);
	rainbow();
	//sinelon(50, 100);
	FastLED.show();
}

void addGlitter(uint8_t intensity)
{
	fadeToBlackBy(ledArray, NumLeds, 50);
	CRGB color = CRGB::White;
	color.nscale8_video(intensity);
	ledArray[random16(NumLeds)] = color;
}

void sinelon(uint8_t hue, uint8_t intensity)
{
	// a colored dot sweeping back and forth, with fading trails
	fadeToBlackBy(ledArray, NumLeds, 50);
	int pos = beatsin16(13, 0, NumLeds - 1);
	ledArray[pos] = CHSV(hue, 255, intensity);
}
uint8_t rainbowInitialHue = 0;
void rainbow()
{
	// FastLED's built-in rainbow generator
	fill_rainbow(ledArray, NumLeds, rainbowInitialHue, 25);
	fadeToBlackBy(ledArray, NumLeds, 200);
	rainbowInitialHue++;
	if (rainbowInitialHue > 255)
	{
		rainbowInitialHue = 0;
	}
}

// ===========================================
// main arduino functions
// ===========================================
void setup()
{
	Serial.begin(SERIAL_BAUD_RATE);
	Serial.setTimeout(1000);

	Serial.println();
	Serial.print("Version ");
	Serial.println(version);

	Initialise();

	clearLeds();

	if (NumLeds > 0)
	{
		for (size_t i = 0; i < NumLeds; i++)
		{
			ledArray[i] = CRGB(random(0, 255), random(0, 255), random(0, 255));
			FastLED.show();
			delay(100);
		}
		delay(500);
		clearLeds();
	}
}

void loop()
{
	EVERY_N_MILLISECONDS(50) { doLedScreensaver(); }
	CheckButtons();
	ProcessIncomingCommands();
}