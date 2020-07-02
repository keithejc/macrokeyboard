using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace KeyboardEditor.Model
{
    public class Button : INotifyPropertyChanged
    {
        private IList<Command> keyStrokes;
        private int buttonIndex;

        public IList<Command> KeyStrokes { get => keyStrokes; set { keyStrokes = value; OnPropertyChanged("KeyStrokes"); } }
        public int ButtonIndex { get => buttonIndex; set { buttonIndex = value; OnPropertyChanged("ButtonIndex"); } }
        public string Name => this.ToString();

        public Button()
        {
            KeyStrokes = new List<Command>();
        }

        public string CommandString
        {
            get
            {
                var cmdString = "";
                foreach (var item in KeyStrokes)
                {
                    cmdString += item.ToString() + " ";
                }
                return cmdString;
            }
        }

        public override string ToString()
        {
            return "Button " + (ButtonIndex + 1);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}