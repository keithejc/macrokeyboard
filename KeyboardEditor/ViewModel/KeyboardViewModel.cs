using System;
using System.Collections.Generic;
using System.Text;

using KeyboardEditor.Model;

namespace KeyboardEditor.ViewModel
{
    public class KeyboardViewModel
    {
        private readonly Keyboard keyboard;

        public KeyboardViewModel()
        {
            keyboard = new Keyboard();
            Status = keyboard.ReadKeyboard();
        }

        public KeyboardSettings KeyboardSettings
        {
            get
            {
                return keyboard.KeyboardSettings;
            }
            set
            {
                keyboard.KeyboardSettings = value;
            }
        }

        public IList<Button> Buttons
        {
            get
            {
                return keyboard.Buttons;
            }
            set
            {
                keyboard.Buttons = value;
            }
        }
        public string KeyboardType { get { return KeyboardSettings.NumButtons + " buttons, " + KeyboardSettings.NumEncoders + " Encoders " + KeyboardSettings.MaxNumKeystrokesPerButton + " commands per button"; } }
        public string Status { get; set; }

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