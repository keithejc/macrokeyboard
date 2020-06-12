using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using static MacroKeysWriter.Button;

namespace MacroKeysWriter
{
    public partial class FormMain : Form
    {
        private readonly MacroKeyboard MacroKeyboard = new MacroKeyboard();

        public FormMain()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            this.Text += ("  -  " + version);
            Console.WriteLine("Version: " + version);
        }

        private void ButtonGetButtons_Click(object sender, EventArgs e)
        {
            LoadButtons();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadButtons();

            comboKeyCodeType.Items.Clear();
            foreach (var item in Enum.GetNames(typeof(KeyCodeType)))
            {
                comboKeyCodeType.Items.Add(Enum.Parse(typeof(KeyCodeType), item));
            }
            comboKeyCodeType.SelectedItem = KeyCodeType.Keyboard;
        }

        private void LoadKeyCodes()
        {
            listBoxKeyCodes.Items.Clear();
            switch ((KeyCodeType)comboKeyCodeType.SelectedItem)
            {
                case KeyCodeType.Keyboard:
                    foreach (var item in Enum.GetValues(typeof(KeyboardKeycode)))
                    {
                        listBoxKeyCodes.Items.Add(item);
                    }
                    break;

                case KeyCodeType.Consumer:
                    foreach (var item in Enum.GetValues(typeof(ConsumerKeycode)))
                    {
                        listBoxKeyCodes.Items.Add(item);
                    }
                    break;

                case KeyCodeType.System:
                    foreach (var item in Enum.GetValues(typeof(SystemKeycode)))
                    {
                        listBoxKeyCodes.Items.Add(item);
                    }
                    break;

                default:
                    break;
            }
        }

        private void LoadButtons()
        {
            panelStatus.Text = MacroKeyboard.ReadKeyboard();
            listBoxButtons.Items.Clear();
            foreach (var item in MacroKeyboard.Buttons)
            {
                listBoxButtons.Items.Add(item);
            }
            if (listBoxButtons.Items.Count > 0)
            {
                listBoxButtons.SelectedIndex = 0;
            }
        }

        private void ListBoxButtons_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadButtonMacro();
        }

        private void LoadButtonMacro()
        {
            listBoxKeystrokes.Items.Clear();
            foreach (var cmd in ((Button)listBoxButtons.SelectedItem).KeyStrokes)
            {
                listBoxKeystrokes.Items.Add(cmd);
            }
        }

        private void ComboKeyCodeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadKeyCodes();
        }

        private void ListBoxKeystrokes_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DragData)))
            {
                var data = (DragData)e.Data.GetData(typeof(DragData));

                //if dragged from this listbox to outside it then delete
                if (data.IsAdding)
                {
                    //add to current button macro
                    var button = (Button)listBoxButtons.SelectedItem;

                    if (button != null)
                    {
                        if (button.KeyStrokes.Count < MacroKeyboard.KeyboardSettings.MaxNumKeystrokesPerButton)
                        {
                            button.KeyStrokes.Add(data.KeyStroke);
                            LoadButtonMacro();
                        }
                    }
                }
            }
        }

        private struct DragData
        {
            public bool IsAdding;
            public KeyStroke KeyStroke;
        }

        private void ListBoxKeyCodes_MouseDown(object sender, MouseEventArgs e)
        {
            ListBox lst = sender as ListBox;

            // Only use the right mouse button.
            if (e.Button != MouseButtons.Left) return;

            // Find the item under the mouse.
            int index = lst.IndexFromPoint(e.Location);
            lst.SelectedIndex = index;
            if (index < 0)
            {
                return;
            }

            var data =
                new DragData
                {
                    IsAdding = true,
                    KeyStroke = new KeyStroke()
                    {
                        KeyCodeType = (KeyCodeType)comboKeyCodeType.SelectedItem
                    }
                };

            switch ((KeyCodeType)comboKeyCodeType.SelectedItem)
            {
                case KeyCodeType.Keyboard:

                    data.KeyStroke.KeyCode = (byte)lst.SelectedItem;

                    break;

                case KeyCodeType.Consumer:

                    data.KeyStroke.KeyCode = (UInt16)lst.SelectedItem;

                    break;

                case KeyCodeType.System:

                    data.KeyStroke.KeyCode = (byte)lst.SelectedItem;

                    break;

                default:
                    break;
            }

            // Drag the item.
            lst.DoDragDrop(data, DragDropEffects.Copy);
        }

        /// <summary>
        /// dragging something over here. either dragging out to delete
        /// or dragging in to add
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxKeystrokes_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            //don't allow drags from other places

            if (e.Data.GetDataPresent(typeof(DragData)))
            {
                var data = (DragData)e.Data.GetData(typeof(DragData));
                if (!data.IsAdding)
                {
                    //removing
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    //are we adding?
                    var button = (Button)listBoxButtons.SelectedItem;

                    if (button != null)
                    {
                        //only if there aren't too many keystrokes already
                        if (button.KeyStrokes.Count < MacroKeyboard.KeyboardSettings.MaxNumKeystrokesPerButton)
                        {
                            // Allow it.
                            e.Effect = DragDropEffects.Copy;
                        }
                        else
                        {
                            e.Effect = DragDropEffects.None;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// dragging out of the keystrokes list - to delete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxKeystrokes_MouseDown(object sender, MouseEventArgs e)
        {
            var listboxKeystrokes = sender as ListBox;

            // Only use the left mouse button.
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // Find the item under the mouse.
            int index = listboxKeystrokes.IndexFromPoint(e.Location);
            listboxKeystrokes.SelectedIndex = index;
            if (index < 0)
            {
                return;
            }

            // Drag the item.
            listboxKeystrokes.DoDragDrop(new DragData { IsAdding = false }, DragDropEffects.Move);
        }

        /// <summary>
        /// maintain dragging icon over the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragEnterShowIcon(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            //don't allow drags from other places

            if (e.Data.GetDataPresent(typeof(DragData)))
            {
                var data = (DragData)e.Data.GetData(typeof(DragData));
                if (!data.IsAdding)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void DragDropDeleteKeyStroke(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DragData)))
            {
                var data = (DragData)e.Data.GetData(typeof(DragData));

                if (!data.IsAdding)
                {
                    var dragButton = (Button)listBoxButtons.SelectedItem;

                    if (dragButton != null)
                    {
                        listBoxKeystrokes.Items.RemoveAt(listBoxKeystrokes.SelectedIndex);
                        dragButton.KeyStrokes.Clear();
                        foreach (var item in listBoxKeystrokes.Items)
                        {
                            dragButton.KeyStrokes.Add((KeyStroke)item);
                        }
                    }
                }
            }
        }

        private void buttonWrite_Click(object sender, EventArgs e)
        {
            foreach (var item in listBoxButtons.Items)
            {
                panelStatus.Text = MacroKeyboard.WriteButton((Button)item);
            }
        }
    }
}