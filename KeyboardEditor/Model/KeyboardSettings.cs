﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardEditor.Model
{
    public class KeyboardSettings
    {
        public byte MaxNumKeystrokesPerButton { get; set; }
        public byte NumButtons { get; set; }
        public byte NumEncoders { get; set; }
        public string Version { get; set; }
    }
}