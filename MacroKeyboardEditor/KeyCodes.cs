﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroKeysWriter
{
    internal enum SystemKeycode : byte
    {
        SYSTEM_POWER_DOWN = 0x81,
        SYSTEM_SLEEP = 0x82,
        SYSTEM_WAKE_UP = 0x83,

        // System control mappings
        HID_SYSTEM_UNASSIGNED = 0x00,

        HID_SYSTEM_POWER_DOWN = 0x81,   // HID type OSC
        HID_SYSTEM_SLEEP = 0x82,    // HID type OSC
        HID_SYSTEM_WAKE_UP = 0x83,  // HID type OSC
        HID_SYSTEM_CONTEXT_MENU = 0x84, // HID type OSC
        HID_SYSTEM_MAIN_MENU = 0x85,    // HID type OSC
        HID_SYSTEM_APP_MENU = 0x86, // HID type OSC
        HID_SYSTEM_MENU_HELP = 0x87,    // HID type OSC
        HID_SYSTEM_MENU_EXIT = 0x88,    // HID type OSC
        HID_SYSTEM_MENU_SELECT = 0x89,  // HID type OSC
        HID_SYSTEM_MENU_RIGHT = 0x8A,   // HID type RTC
        HID_SYSTEM_MENU_LEFT = 0x8B,    // HID type RTC
        HID_SYSTEM_MENU_UP = 0x8C,  // HID type RTC
        HID_SYSTEM_MENU_DOWN = 0x8D,    // HID type RTC
        HID_SYSTEM_COLD_RESTART = 0x8E, // HID type OSC
        HID_SYSTEM_WARM_RESTART = 0x8F, // HID type OSC
        HID_D_PAD_UP = 0x90,    // HID type OOC
        HID_D_PAD_DOWN = 0x91,  // HID type OOC
        HID_D_PAD_RIGHT = 0x92, // HID type OOC
        HID_D_PAD_LEFT = 0x93,  // HID type OOC

        // 0x94-0x9F are reserved
        HID_SYSTEM_DOCK = 0xA0, // HID type OSC

        HID_SYSTEM_UNDOCK = 0xA1,   // HID type OSC
        HID_SYSTEM_SETUP = 0xA2,    // HID type OSC
        HID_SYSTEM_BREAK = 0xA3,    // HID type OSC
        HID_SYSTEM_DEBUGGER_BREAK = 0xA4,   // HID type OSC
        HID_APPLICATION_BREAK = 0xA5,   // HID type OSC
        HID_APPLICATION_DEBUGGER_BREAK = 0xA6,  // HID type OSC
        HID_SYSTEM_SPEAKER_MUTE = 0xA7, // HID type OSC
        HID_SYSTEM_HIBERNATE = 0xA8,    // HID type OSC

        // 0xA9-0xAF are reserved
        HID_SYSTEM_DISPLAY_INVERT = 0xB0,   // HID type OSC

        HID_SYSTEM_DISPLAY_INTERNAL = 0xB1, // HID type OSC
        HID_SYSTEM_DISPLAY_EXTERNAL = 0xB2, // HID type OSC
        HID_SYSTEM_DISPLAY_BOTH = 0xB3, // HID type OSC
        HID_SYSTEM_DISPLAY_DUAL = 0xB4, // HID type OSC
        HID_SYSTEM_DISPLAY_TOGGLE_INT_SLASH_EXT = 0xB5, // HID type OSC
        HID_SYSTEM_DISPLAY_SWAP_PRIMARY_SLASH_SECONDARY = 0xB6, // HID type OSC
        HID_SYSTEM_DISPLAY_LCD_AUTOSCALE = 0xB7,    // HID type OSC
    };

    internal enum ConsumerKeycode : UInt16
    {
        // Some keys might only work with linux
        CONSUMER_POWER = 0x30,

        CONSUMER_SLEEP = 0x32,

        MEDIA_RECORD = 0xB2,
        MEDIA_FAST_FORWARD = 0xB3,
        MEDIA_REWIND = 0xB4,
        MEDIA_NEXT = 0xB5,
        MEDIA_PREVIOUS = 0xB6,
        MEDIA_PREV = 0xB6, // Alias
        MEDIA_STOP = 0xB7,
        MEDIA_PLAY_PAUSE = 0xCD,
        MEDIA_PAUSE = 0xB0,

        MEDIA_VOLUME_MUTE = 0xE2,
        MEDIA_VOL_MUTE = 0xE2, // Alias
        MEDIA_VOLUME_UP = 0xE9,
        MEDIA_VOL_UP = 0xE9, // Alias
        MEDIA_VOLUME_DOWN = 0xEA,
        MEDIA_VOL_DOWN = 0xEA, // Alias

        CONSUMER_SCREENSAVER = 0x19e,

        CONSUMER_PROGRAMMABLE_BUTTON_CONFIGURATION = 0x182,
        CONSUMER_CONTROL_CONFIGURATION = 0x183,
        CONSUMER_EMAIL_READER = 0x18A,
        CONSUMER_CALCULATOR = 0x192,
        CONSUMER_EXPLORER = 0x194,

        CONSUMER_BROWSER_HOME = 0x223,
        CONSUMER_BROWSER_BACK = 0x224,
        CONSUMER_BROWSER_FORWARD = 0x225,
        CONSUMER_BROWSER_REFRESH = 0x227,
        CONSUMER_BROWSER_BOOKMARKS = 0x22A,
    };

    internal enum KeyboardKeycode : byte
    {
        KEY_RESERVED = 0,
        KEY_ERROR_ROLLOVER = 1,
        KEY_POST_FAIL = 2,
        KEY_ERROR_UNDEFINED = 3,
        KEY_A = 4,
        KEY_B = 5,
        KEY_C = 6,
        KEY_D = 7,
        KEY_E = 8,
        KEY_F = 9,
        KEY_G = 10,
        KEY_H = 11,
        KEY_I = 12,
        KEY_J = 13,
        KEY_K = 14,
        KEY_L = 15,
        KEY_M = 16,
        KEY_N = 17,
        KEY_O = 18,
        KEY_P = 19,
        KEY_Q = 20,
        KEY_R = 21,
        KEY_S = 22,
        KEY_T = 23,
        KEY_U = 24,
        KEY_V = 25,
        KEY_W = 26,
        KEY_X = 27,
        KEY_Y = 28,
        KEY_Z = 29,
        KEY_1 = 30,
        KEY_2 = 31,
        KEY_3 = 32,
        KEY_4 = 33,
        KEY_5 = 34,
        KEY_6 = 35,
        KEY_7 = 36,
        KEY_8 = 37,
        KEY_9 = 38,
        KEY_0 = 39,
        KEY_ENTER = 40,
        KEY_RETURN = 40, // Alias
        KEY_ESC = 41,
        KEY_BACKSPACE = 42,
        KEY_TAB = 43,
        KEY_SPACE = 44,
        KEY_MINUS = 45,
        KEY_EQUAL = 46,
        KEY_LEFT_BRACE = 47,
        KEY_RIGHT_BRACE = 48,
        KEY_BACKSLASH = 49,
        KEY_NON_US_NUM = 50,
        KEY_SEMICOLON = 51,
        KEY_QUOTE = 52,
        KEY_TILDE = 53,
        KEY_COMMA = 54,
        KEY_PERIOD = 55,
        KEY_SLASH = 56,
        KEY_CAPS_LOCK = 0x39,
        KEY_F1 = 0x3A,
        KEY_F2 = 0x3B,
        KEY_F3 = 0x3C,
        KEY_F4 = 0x3D,
        KEY_F5 = 0x3E,
        KEY_F6 = 0x3F,
        KEY_F7 = 0x40,
        KEY_F8 = 0x41,
        KEY_F9 = 0x42,
        KEY_F10 = 0x43,
        KEY_F11 = 0x44,
        KEY_F12 = 0x45,
        KEY_PRINT = 0x46,
        KEY_PRINTSCREEN = 0x46, // Alias
        KEY_SCROLL_LOCK = 0x47,
        KEY_PAUSE = 0x48,
        KEY_INSERT = 0x49,
        KEY_HOME = 0x4A,
        KEY_PAGE_UP = 0x4B,
        KEY_DELETE = 0x4C,
        KEY_END = 0x4D,
        KEY_PAGE_DOWN = 0x4E,
        KEY_RIGHT_ARROW = 0x4F,
        KEY_LEFT_ARROW = 0x50,
        KEY_DOWN_ARROW = 0x51,
        KEY_UP_ARROW = 0x52,
        KEY_RIGHT = 0x4F, // Alias
        KEY_LEFT = 0x50, // Alias
        KEY_DOWN = 0x51, // Alias
        KEY_UP = 0x52, // Alias
        KEY_NUM_LOCK = 0x53,
        KEYPAD_DIVIDE = 0x54,
        KEYPAD_MULTIPLY = 0x55,
        KEYPAD_SUBTRACT = 0x56,
        KEYPAD_ADD = 0x57,
        KEYPAD_ENTER = 0x58,
        KEYPAD_1 = 0x59,
        KEYPAD_2 = 0x5A,
        KEYPAD_3 = 0x5B,
        KEYPAD_4 = 0x5C,
        KEYPAD_5 = 0x5D,
        KEYPAD_6 = 0x5E,
        KEYPAD_7 = 0x5F,
        KEYPAD_8 = 0x60,
        KEYPAD_9 = 0x61,
        KEYPAD_0 = 0x62,
        KEYPAD_DOT = 0x63,
        KEY_NON_US = 0x64,
        KEY_APPLICATION = 0x65, // Context menu/right click
        KEY_MENU = 0x65, // Alias

        // Most of the following keys will only work with Linux or not at all.
        // F13+ keys are mostly used for laptop functions like ECO key.
        KEY_POWER = 0x66, // PowerOff (Ubuntu)

        KEY_PAD_EQUALS = 0x67, // Dont confuse with KEYPAD_EQUAL_SIGN
        KEY_F13 = 0x68, // Tools (Ubunutu)
        KEY_F14 = 0x69, // Launch5 (Ubuntu)
        KEY_F15 = 0x6A, // Launch6 (Ubuntu)
        KEY_F16 = 0x6B, // Launch7 (Ubuntu)
        KEY_F17 = 0x6C, // Launch8 (Ubuntu)
        KEY_F18 = 0x6D, // Launch9 (Ubuntu)
        KEY_F19 = 0x6E, // Disabled (Ubuntu)
        KEY_F20 = 0x6F, // AudioMicMute (Ubuntu)
        KEY_F21 = 0x70, // Touchpad toggle (Ubuntu)
        KEY_F22 = 0x71, // TouchpadOn (Ubuntu)
        KEY_F23 = 0x72, // TouchpadOff Ubuntu)
        KEY_F24 = 0x73, // Disabled (Ubuntu)
        KEY_EXECUTE = 0x74, // Open (Ubuntu)
        KEY_HELP = 0x75, // Help (Ubuntu)
        KEY_MENU2 = 0x76, // Disabled (Ubuntu)
        KEY_SELECT = 0x77, // Disabled (Ubuntu)
        KEY_STOP = 0x78, // Cancel (Ubuntu)
        KEY_AGAIN = 0x79, // Redo (Ubuntu)
        KEY_UNDO = 0x7A, // Undo (Ubuntu)
        KEY_CUT = 0x7B, // Cut (Ubuntu)
        KEY_COPY = 0x7C, // Copy (Ubuntu)
        KEY_PASTE = 0x7D, // Paste (Ubuntu)
        KEY_FIND = 0x7E, // Find (Ubuntu)
        KEY_MUTE = 0x7F,
        KEY_VOLUME_MUTE = 0x7F, // Alias
        KEY_VOLUME_UP = 0x80,
        KEY_VOLUME_DOWN = 0x81,
        KEY_LOCKING_CAPS_LOCK = 0x82, // Disabled (Ubuntu)
        KEY_LOCKING_NUM_LOCK = 0x83, // Disabled (Ubuntu)
        KEY_LOCKING_SCROLL_LOCK = 0x84, // Disabled (Ubuntu)
        KEYPAD_COMMA = 0x85, // .
        KEYPAD_EQUAL_SIGN = 0x86, // Disabled (Ubuntu), Dont confuse with KEYPAD_EQUAL
        KEY_INTERNATIONAL1 = 0x87, // Disabled (Ubuntu)
        KEY_INTERNATIONAL2 = 0x88, // Hiragana Katakana (Ubuntu)
        KEY_INTERNATIONAL3 = 0x89, // Disabled (Ubuntu)
        KEY_INTERNATIONAL4 = 0x8A, // Henkan (Ubuntu)
        KEY_INTERNATIONAL5 = 0x8B, // Muhenkan (Ubuntu)
        KEY_INTERNATIONAL6 = 0x8C, // Disabled (Ubuntu)
        KEY_INTERNATIONAL7 = 0x8D, // Disabled (Ubuntu)
        KEY_INTERNATIONAL8 = 0x8E, // Disabled (Ubuntu)
        KEY_INTERNATIONAL9 = 0x8F, // Disabled (Ubuntu)
        KEY_LANG1 = 0x90, // Disabled (Ubuntu)
        KEY_LANG2 = 0x91, // Disabled (Ubuntu)
        KEY_LANG3 = 0x92, // Katana (Ubuntu)
        KEY_LANG4 = 0x93, // Hiragana (Ubuntu)
        KEY_LANG5 = 0x94, // Disabled (Ubuntu)
        KEY_LANG6 = 0x95, // Disabled (Ubuntu)
        KEY_LANG7 = 0x96, // Disabled (Ubuntu)
        KEY_LANG8 = 0x97, // Disabled (Ubuntu)
        KEY_LANG9 = 0x98, // Disabled (Ubuntu)
        KEY_ALTERNATE_ERASE = 0x99, // Disabled (Ubuntu)
        KEY_SYSREQ_ATTENTION = 0x9A, // Disabled (Ubuntu)
        KEY_CANCEL = 0x9B, // Disabled (Ubuntu)
        KEY_CLEAR = 0x9C, // Delete (Ubuntu)
        KEY_PRIOR = 0x9D, // Disabled (Ubuntu)
        KEY_RETURN2 = 0x9E, // Disabled (Ubuntu), Do not confuse this with KEY_ENTER
        KEY_SEPARATOR = 0x9F, // Disabled (Ubuntu)
        KEY_OUT = 0xA0, // Disabled (Ubuntu)
        KEY_OPER = 0xA1, // Disabled (Ubuntu)
        KEY_CLEAR_AGAIN = 0xA2, // Disabled (Ubuntu)
        KEY_CRSEL_PROPS = 0xA3, // Disabled (Ubuntu)
        KEY_EXSEL = 0xA4, // Disabled (Ubuntu)

        KEY_PAD_00 = 0xB0, // Disabled (Ubuntu)
        KEY_PAD_000 = 0xB1, // Disabled (Ubuntu)
        KEY_THOUSANDS_SEPARATOR = 0xB2, // Disabled (Ubuntu)
        KEY_DECIMAL_SEPARATOR = 0xB3, // Disabled (Ubuntu)
        KEY_CURRENCY_UNIT = 0xB4, // Disabled (Ubuntu)
        KEY_CURRENCY_SUB_UNIT = 0xB5, // Disabled (Ubuntu)
        KEYPAD_LEFT_BRACE = 0xB6, // (
        KEYPAD_RIGHT_BRACE = 0xB7, // )
        KEYPAD_LEFT_CURLY_BRACE = 0xB8, // Disabled (Ubuntu)
        KEYPAD_RIGHT_CURLY_BRACE = 0xB9, // Disabled (Ubuntu)
        KEYPAD_TAB = 0xBA, // Disabled (Ubuntu)
        KEYPAD_BACKSPACE = 0xBB, // Disabled (Ubuntu)
        KEYPAD_A = 0xBC, // Disabled (Ubuntu)
        KEYPAD_B = 0xBD, // Disabled (Ubuntu)
        KEYPAD_C = 0xBE, // Disabled (Ubuntu)
        KEYPAD_D = 0xBF, // Disabled (Ubuntu)
        KEYPAD_E = 0xC0, // Disabled (Ubuntu)
        KEYPAD_F = 0xC1, // Disabled (Ubuntu)
        KEYPAD_XOR = 0xC2, // Disabled (Ubuntu)
        KEYPAD_CARET = 0xC3, // Disabled (Ubuntu)
        KEYPAD_PERCENT = 0xC4, // Disabled (Ubuntu)
        KEYPAD_LESS_THAN = 0xC5, // Disabled (Ubuntu)
        KEYPAD_GREATER_THAN = 0xC6, // Disabled (Ubuntu)
        KEYPAD_AMPERSAND = 0xC7, // Disabled (Ubuntu)
        KEYPAD_DOUBLEAMPERSAND = 0xC8, // Disabled (Ubuntu)
        KEYPAD_PIPE = 0xC9, // Disabled (Ubuntu)
        KEYPAD_DOUBLEPIPE = 0xCA, // Disabled (Ubuntu)
        KEYPAD_COLON = 0xCB, // Disabled (Ubuntu)
        KEYPAD_POUND_SIGN = 0xCC, // Disabled (Ubuntu)
        KEYPAD_SPACE = 0xCD, // Disabled (Ubuntu)
        KEYPAD_AT_SIGN = 0xCE, // Disabled (Ubuntu)
        KEYPAD_EXCLAMATION_POINT = 0xCF, // Disabled (Ubuntu)
        KEYPAD_MEMORY_STORE = 0xD0, // Disabled (Ubuntu)
        KEYPAD_MEMORY_RECALL = 0xD1, // Disabled (Ubuntu)
        KEYPAD_MEMORY_CLEAR = 0xD2, // Disabled (Ubuntu)
        KEYPAD_MEMORY_ADD = 0xD3, // Disabled (Ubuntu)
        KEYPAD_MEMORY_SUBTRACT = 0xD4, // Disabled (Ubuntu)
        KEYPAD_MEMORY_MULTIPLY = 0xD5, // Disabled (Ubuntu)
        KEYPAD_MEMORY_DIVIDE = 0xD6, // Disabled (Ubuntu)
        KEYPAD_PLUS_MINUS = 0xD7, // Disabled (Ubuntu)
        KEYPAD_CLEAR = 0xD8, // Delete (Ubuntu)
        KEYPAD_CLEAR_ENTRY = 0xD9, // Disabled (Ubuntu)
        KEYPAD_BINARY = 0xDA, // Disabled (Ubuntu)
        KEYPAD_OCTAL = 0xDB, // Disabled (Ubuntu)
        KEYPAD_DECIMAL = 0xDC, // Disabled (Ubuntu)
        KEYPAD_HEXADECIMAL = 0xDD, // Disabled (Ubuntu)

        KEY_LEFT_CTRL = 0xE0,
        KEY_LEFT_SHIFT = 0xE1,
        KEY_LEFT_ALT = 0xE2,
        KEY_LEFT_GUI = 0xE3,
        KEY_LEFT_WINDOWS = 0xE3, // Alias
        KEY_RIGHT_CTRL = 0xE4,
        KEY_RIGHT_SHIFT = 0xE5,
        KEY_RIGHT_ALT = 0xE6,
        KEY_RIGHT_GUI = 0xE7,
        KEY_RIGHT_WINDOWS = 0xE7 // Alias
    };

    internal class KeyCodes
    {
    }
}