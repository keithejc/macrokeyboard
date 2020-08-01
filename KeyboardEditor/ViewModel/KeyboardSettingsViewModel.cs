using System;
using System.Collections.Generic;
using KeyboardEditor.Model;

namespace KeyboardEditor.ViewModel
{
    internal class KeyboardSettingsViewModel
    {
        private readonly Keyboard keyboard;

        public KeyboardSettingsViewModel(Keyboard keyboard)
        {
            this.keyboard = keyboard;
        }

        public byte NumLeds { get => Keyboard.NumLeds; set => Keyboard.NumLeds = value; }

        public int NumButtons
        {
            get => Keyboard.Buttons.Count;
            set
            {
                if (value < Keyboard.Buttons.Count)
                {
                    for (int i = Keyboard.Buttons.Count - 1; i >= value; i++)
                    {
                        Keyboard.Buttons.RemoveAt(i);
                    }
                }
                else if (value > Keyboard.Buttons.Count)
                {
                    for (int i = Keyboard.Buttons.Count; i <= value; i++)
                    {
                        Keyboard.Buttons.Add(new Button() { ButtonIndex = i });
                    }
                }
            }
        }

        public int NumEncoders
        {
            get => Keyboard.Encoders.Count;
            set
            {
                if (value < Keyboard.Encoders.Count)
                {
                    for (int i = Keyboard.Encoders.Count - 1; i >= value; i++)
                    {
                        Keyboard.Encoders.RemoveAt(i);
                    }
                }
                else if (value > Keyboard.Encoders.Count)
                {
                    for (int i = Keyboard.Encoders.Count; i <= value; i++)
                    {
                        Keyboard.Encoders.Add(new EncoderControl() { EncoderIndex = i });
                    }
                }
            }
        }

        private System.Windows.Input.ICommand mUpdater;

        public System.Windows.Input.ICommand UpdateCommand
        {
            get
            {
                if (mUpdater == null)
                    mUpdater = new Updater();
                return mUpdater;
            }
            set
            {
                mUpdater = value;
            }
        }

        public Keyboard Keyboard => keyboard;

        private class Updater : System.Windows.Input.ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
            }
        }
    }
}