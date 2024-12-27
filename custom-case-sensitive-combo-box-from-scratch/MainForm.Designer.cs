namespace custom_case_sensitive_combo_box_from_scratch
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            comboBox = new CaseSensitiveComboBox();
            SuspendLayout();
            // 
            // comboBox
            // 
            comboBox.BackColor = Color.White;
            comboBox.BorderStyle = BorderStyle.FixedSingle;
            comboBox.Font = new Font("Segoe UI", 14F);
            comboBox.Location = new Point(85, 83);
            comboBox.Name = "comboBox";
            comboBox.Size = new Size(300, 59);
            comboBox.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(478, 244);
            Controls.Add(comboBox);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main Form";
            ResumeLayout(false);
        }

        #endregion

        private CaseSensitiveComboBox comboBox;
    }
}
