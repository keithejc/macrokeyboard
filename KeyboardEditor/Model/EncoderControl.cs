using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardEditor.Model
{
    public class EncoderControl : INotifyPropertyChanged
    {
        private Button button;
        private Button clockwiseStep;
        private Button antiClockwiseStep;
        private byte stepSize;

        public int EncoderIndex { get; set; }
        public Button Button { get => button; set { button = value; OnPropertyChanged("Button"); } }
        public Button ClockwiseStep { get => clockwiseStep; set { clockwiseStep = value; OnPropertyChanged("ClockwiseStep"); } }
        public Button AntiClockwiseStep { get => antiClockwiseStep; set { antiClockwiseStep = value; OnPropertyChanged("AntiClockwiseStep"); } }
        public byte StepSize { get => stepSize; set { stepSize = value; OnPropertyChanged("StepSize"); } }

        public EncoderControl()
        {
            button = new Button();
            clockwiseStep = new Button();
            antiClockwiseStep = new Button();
        }

        public string Name => this.ToString();

        public override string ToString()
        {
            var str = "Encoder " + (EncoderIndex + 1);
            return str;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}