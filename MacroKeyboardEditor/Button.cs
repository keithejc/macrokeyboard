using System;
using System.Collections.Generic;

namespace MacroKeysWriter
{
    public class Button
    {
        public List<KeyStroke> KeyStrokes { get; set; }
        public int ButtonIndex { get; set; }

        public Button()
        {
            KeyStrokes = new List<KeyStroke>();
        }

        public override string ToString()
        {
            return "Button " + (ButtonIndex + 1);
        }
    }
}