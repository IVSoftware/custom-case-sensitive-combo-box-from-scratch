using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace custom_case_sensitive_combo_box_from_scratch
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            comboBox.Items.Add("zebra");
            comboBox.Items.Add("Zebra");
            comboBox.Items.Add("ZEBRA");
        }
    }
}
