namespace MacroKeysWriter
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonGetButtons = new System.Windows.Forms.Button();
            this.listBoxButtons = new System.Windows.Forms.ListBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.panelStatus = new System.Windows.Forms.Label();
            this.listBoxKeystrokes = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxKeyCodes = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboKeyCodeType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonWrite = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.labelKeyboardVersion = new System.Windows.Forms.Label();
            this.textBoxKeyboard = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // buttonGetButtons
            // 
            this.buttonGetButtons.Location = new System.Drawing.Point(12, 331);
            this.buttonGetButtons.Name = "buttonGetButtons";
            this.buttonGetButtons.Size = new System.Drawing.Size(108, 60);
            this.buttonGetButtons.TabIndex = 0;
            this.buttonGetButtons.Text = "Read Buttons";
            this.buttonGetButtons.UseVisualStyleBackColor = true;
            this.buttonGetButtons.Click += new System.EventHandler(this.ButtonGetButtons_Click);
            // 
            // listBoxButtons
            // 
            this.listBoxButtons.FormattingEnabled = true;
            this.listBoxButtons.ItemHeight = 20;
            this.listBoxButtons.Location = new System.Drawing.Point(12, 43);
            this.listBoxButtons.Name = "listBoxButtons";
            this.listBoxButtons.Size = new System.Drawing.Size(212, 264);
            this.listBoxButtons.TabIndex = 1;
            this.listBoxButtons.SelectedIndexChanged += new System.EventHandler(this.ListBoxButtons_SelectedIndexChanged);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(282, 331);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(56, 20);
            this.labelStatus.TabIndex = 2;
            this.labelStatus.Text = "Status";
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.SystemColors.Window;
            this.panelStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelStatus.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.panelStatus.Location = new System.Drawing.Point(286, 358);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(349, 49);
            this.panelStatus.TabIndex = 3;
            this.panelStatus.Text = "Ready..";
            // 
            // listBoxKeystrokes
            // 
            this.listBoxKeystrokes.AllowDrop = true;
            this.listBoxKeystrokes.FormattingEnabled = true;
            this.listBoxKeystrokes.ItemHeight = 20;
            this.listBoxKeystrokes.Location = new System.Drawing.Point(241, 43);
            this.listBoxKeystrokes.Name = "listBoxKeystrokes";
            this.listBoxKeystrokes.Size = new System.Drawing.Size(344, 264);
            this.listBoxKeystrokes.TabIndex = 4;
            this.listBoxKeystrokes.DragDrop += new System.Windows.Forms.DragEventHandler(this.ListBoxKeystrokes_DragDrop);
            this.listBoxKeystrokes.DragEnter += new System.Windows.Forms.DragEventHandler(this.ListBoxKeystrokes_DragEnter);
            this.listBoxKeystrokes.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListBoxKeystrokes_MouseDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Buttons";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(237, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Key Strokes";
            // 
            // listBoxKeyCodes
            // 
            this.listBoxKeyCodes.AllowDrop = true;
            this.listBoxKeyCodes.FormattingEnabled = true;
            this.listBoxKeyCodes.ItemHeight = 20;
            this.listBoxKeyCodes.Location = new System.Drawing.Point(663, 103);
            this.listBoxKeyCodes.Name = "listBoxKeyCodes";
            this.listBoxKeyCodes.Size = new System.Drawing.Size(352, 304);
            this.listBoxKeyCodes.TabIndex = 9;
            this.listBoxKeyCodes.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropDeleteKeyStroke);
            this.listBoxKeyCodes.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragEnterShowIcon);
            this.listBoxKeyCodes.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListBoxKeyCodes_MouseDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(659, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 20);
            this.label3.TabIndex = 11;
            this.label3.Text = "Type";
            // 
            // comboKeyCodeType
            // 
            this.comboKeyCodeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboKeyCodeType.FormattingEnabled = true;
            this.comboKeyCodeType.Location = new System.Drawing.Point(663, 46);
            this.comboKeyCodeType.Name = "comboKeyCodeType";
            this.comboKeyCodeType.Size = new System.Drawing.Size(149, 28);
            this.comboKeyCodeType.TabIndex = 10;
            this.comboKeyCodeType.SelectedIndexChanged += new System.EventHandler(this.ComboKeyCodeType_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(659, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 20);
            this.label4.TabIndex = 12;
            this.label4.Text = "Key Code";
            // 
            // buttonWrite
            // 
            this.buttonWrite.Location = new System.Drawing.Point(144, 331);
            this.buttonWrite.Name = "buttonWrite";
            this.buttonWrite.Size = new System.Drawing.Size(108, 60);
            this.buttonWrite.TabIndex = 13;
            this.buttonWrite.Text = "Write Buttons";
            this.buttonWrite.UseVisualStyleBackColor = true;
            this.buttonWrite.Click += new System.EventHandler(this.buttonWrite_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(586, 201);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 20);
            this.label5.TabIndex = 14;
            this.label5.Text = "<- drag ->";
            // 
            // labelKeyboardVersion
            // 
            this.labelKeyboardVersion.AutoSize = true;
            this.labelKeyboardVersion.Location = new System.Drawing.Point(16, 418);
            this.labelKeyboardVersion.Name = "labelKeyboardVersion";
            this.labelKeyboardVersion.Size = new System.Drawing.Size(0, 20);
            this.labelKeyboardVersion.TabIndex = 15;
            // 
            // textBoxKeyboard
            // 
            this.textBoxKeyboard.Location = new System.Drawing.Point(323, 593);
            this.textBoxKeyboard.Name = "textBoxKeyboard";
            this.textBoxKeyboard.Size = new System.Drawing.Size(340, 26);
            this.textBoxKeyboard.TabIndex = 16;
            this.textBoxKeyboard.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxKeyboard_KeyDown);
            this.textBoxKeyboard.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyboard_KeyPress);
            this.textBoxKeyboard.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxKeyboard_KeyUp);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Location = new System.Drawing.Point(663, 782);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(200, 100);
            this.flowLayoutPanel1.TabIndex = 17;
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1528, 1450);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.textBoxKeyboard);
            this.Controls.Add(this.labelKeyboardVersion);
            this.Controls.Add(this.buttonWrite);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboKeyCodeType);
            this.Controls.Add(this.listBoxKeyCodes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxKeystrokes);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.listBoxButtons);
            this.Controls.Add(this.buttonGetButtons);
            this.Controls.Add(this.label5);
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.ShowIcon = false;
            this.Text = "Macro Keyboard Editor";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropDeleteKeyStroke);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragEnterShowIcon);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonGetButtons;
        private System.Windows.Forms.ListBox listBoxButtons;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label panelStatus;
        private System.Windows.Forms.ListBox listBoxKeystrokes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxKeyCodes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboKeyCodeType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonWrite;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelKeyboardVersion;
        private System.Windows.Forms.TextBox textBoxKeyboard;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}

