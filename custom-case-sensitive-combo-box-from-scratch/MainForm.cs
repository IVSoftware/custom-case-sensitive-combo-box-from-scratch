using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace custom_case_sensitive_combo_box_from_scratch
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
    }
    class CaseSensitiveComboBox : Panel
    {
        public CaseSensitiveComboBox()
        {
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.White;
            var tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, Width=80));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, Width=20));
            Controls.Add(tableLayoutPanel);
            _richTextBox = new RichTextBoxEx();
            _dropDownIcon = new DropDownIcon();
            tableLayoutPanel.Controls.Add(_richTextBox,0,0);
            tableLayoutPanel.Controls.Add(_dropDownIcon, 1,0);
            _dropDownIcon.MouseDown += (sender, e) => 
                BeginInvoke(()=> DroppedDown = !DroppedDown);
        }
        RichTextBox _richTextBox;
        DropDownIcon _dropDownIcon;
        public int selectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (!Equals(_selectedIndex, value))
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool DroppedDown
        {
            get => _droppedDown;
            set
            {
                if (!Equals(_droppedDown, value))
                {
                    _droppedDown = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _droppedDown = default;


        int _selectedIndex = default;

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (IsHandleCreated)
            {
                _dropDownIcon.Height = 
                _dropDownIcon.Width = 
                    Height - Padding.Vertical;
            }
        }
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            BeginInvoke(() => Focus());
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
        class RichTextBoxEx : RichTextBox
        {
            public RichTextBoxEx()
            {
                Multiline = false;
                Anchor = AnchorStyles.Left | AnchorStyles.Right;
                BorderStyle = BorderStyle.None;
                Text = "Placeholder";
                _isPlaceholderText = true;
                LostFocus += Commit;
                KeyDown += Commit;
                BackColor = DesignMode ? Color.LightGray : Color.White;
            }

            private void Commit(object? sender, EventArgs e)
            {
                if(e is KeyEventArgs eKey)
                {
                    if (eKey.KeyData == Keys.Enter)
                    {
                        eKey.SuppressKeyPress = true;
                    }
                    else
                    {
                        return;
                    }
                }
                if (string.IsNullOrWhiteSpace(Text))
                {
                    Text = "Placeholder";
                    _isPlaceholderText = true;
                }
                BeginInvoke(() => SelectAll());
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                using (var graphics = CreateGraphics())
                {
                    Height = Convert.ToInt16(graphics.MeasureString("AZ", Font).Height);
                }
            }
            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if(_isPlaceholderText)
                {
                    Text = string.Empty;
                    _isPlaceholderText = false;
                }
            }
            bool _isPlaceholderText = true;
        }
        class DropDownIcon : Label
        {
            public DropDownIcon()
            {
                BackColor = Color.Azure;
                Anchor = (AnchorStyles)0xF;
                Text = "▼";
                TextAlign = ContentAlignment.MiddleCenter;
            }
        }
    }
}
