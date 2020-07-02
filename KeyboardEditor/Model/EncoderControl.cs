using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardEditor.Model
{
    public enum EncoderControlType
    {
        Button,
        Clockwise,
        AntiClockwise
    }

    public class EncoderControl : Button
    {
        public EncoderControlType EncoderControlType { get; set; }
        public int EncoderIndex { get; set; }

        public override string ToString()
        {
            var str = "Encoder " + (EncoderIndex + 1);
            switch (EncoderControlType)
            {
                case EncoderControlType.Button:
                    str += " Button";
                    break;

                case EncoderControlType.Clockwise:
                    str += " Clockwise";
                    break;

                case EncoderControlType.AntiClockwise:
                    str += " AntiClockwise";
                    break;

                default:
                    break;
            }
            return str;
        }
    }
}