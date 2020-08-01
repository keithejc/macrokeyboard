using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardEditor.Model
{
    public class KeyboardSettings
    {
        public int EEPROMLength { get; set; }
        public byte NumLeds { get; set; }
        public string Version { get; set; }
    }
}