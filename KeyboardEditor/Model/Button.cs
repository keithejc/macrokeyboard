using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace KeyboardEditor.Model
{
    public class Button : INotifyPropertyChanged
    {
        private IList<Command> commands;
        private int buttonIndex;
        private byte pin;

        public IList<Command> Commands { get => commands; set { commands = value; OnPropertyChanged("KeyStrokes"); } }
        public int ButtonIndex { get => buttonIndex; set { buttonIndex = value; OnPropertyChanged("ButtonIndex"); } }
        public byte Pin { get => pin; set { pin = value; OnPropertyChanged("Pin"); } }

        public string Name => this.ToString();

        public Button()
        {
            Commands = new List<Command>();
        }

        public string CommandString
        {
            get
            {
                var cmdString = "";
                foreach (var item in Commands)
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