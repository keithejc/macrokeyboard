using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using KeyboardEditor.Model;

namespace KeyboardEditor.ViewModel
{
    public class KeyboardViewModel : INotifyPropertyChanged
    {
        private readonly Keyboard keyboard;

        public KeyboardViewModel()
        {
            keyboard = new Keyboard();
            Status = keyboard.ReadKeyboard();
        }

        public int SpaceLeft { get { return keyboard.EepromSpaceLeft; } }

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

        public string KeyboardType { get { return ""; } }// KeyboardSettings.NumButtons + " buttons, " + KeyboardSettings.NumEncoders + " Encoders "; } }

        private string status;
        public string Status { get { return status; } set { status = value; OnPropertyChanged("Status"); } }

        private System.Windows.Input.ICommand writeKeyboardCommand;
        private System.Windows.Input.ICommand openKeyboardSettingsCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public System.Windows.Input.ICommand WriteKeyboardCommand
        {
            get
            {
                if (writeKeyboardCommand == null)
                {
                    writeKeyboardCommand = new RelayCommand(param => this.WriteKeyboard(),
                        null);
                }
                return writeKeyboardCommand;
            }
        }

        private void WriteKeyboard()
        {
            Status = keyboard.WriteKeyboard();
        }

        public System.Windows.Input.ICommand OpenKeyboardSettingsCommand
        {
            get
            {
                if (openKeyboardSettingsCommand == null)
                {
                    openKeyboardSettingsCommand = new RelayCommand(param => this.OpenKeyboardSettings(),
                        null);
                }
                return openKeyboardSettingsCommand;
            }
        }

        private void OpenKeyboardSettings()
        {
            var keyboardSettingsWindow = new View.KeyboardSettings
            {
                DataContext = this
            };
            keyboardSettingsWindow.Show();
        }

        public class RelayCommand : System.Windows.Input.ICommand
        {
            public RelayCommand(Action<object> execute) : this(execute, null)
            {
            }

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                if (execute == null)
                    throw new ArgumentNullException("execute");
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute == null ? true : _canExecute(parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add { System.Windows.Input.CommandManager.RequerySuggested += value; }
                remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;
        }
    }
}