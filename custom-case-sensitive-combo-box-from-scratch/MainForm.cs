using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace custom_case_sensitive_combo_box_from_scratch
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
    }
    public class CaseSensitiveComboBox : Panel
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
            _richTextBox.TextChanged += (sender, e) => OnTextChanged(EventArgs.Empty);
            tableLayoutPanel.Controls.Add(_richTextBox,0,0);
            tableLayoutPanel.Controls.Add(_dropDownIcon, 1,0);
            _dropDownIcon.MouseDown += (sender, e) => 
                BeginInvoke(()=> DroppedDown = !DroppedDown);
            Items.ListChanged += OnItemsChanged;
        }

        private void OnItemsChanged(object? sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    break;
                case ListChangedType.ItemAdded:
                    break;
                case ListChangedType.ItemDeleted:
                    break;
                case ListChangedType.ItemMoved:
                    break;
                case ListChangedType.ItemChanged:
                    break;
                default:
                    break;
            }
        }

        private readonly RichTextBoxEx _richTextBox = new();
        private readonly DropDownIcon _dropDownIcon = new();
        private readonly DropDownContainer _dropDownContainer = new();
        private string? _caseSensitiveText = null;
        private int _caseSensitiveIndex = -1;
        public int SelectedIndex
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
        int _selectedIndex = default;
        protected override void OnTextChanged(EventArgs e)
        {
            var aspirant =
                Items
                .OfType<object>()
                .FirstOrDefault(_ =>
                    (_?.ToString() ?? string.Empty)
                    .IndexOf(Text) == 0, StringComparison.Ordinal);
            Debug.Write($"{aspirant}");
            if (aspirant != null)
            {
                _caseSensitiveIndex = Items.IndexOf(aspirant);
                _caseSensitiveText = aspirant.ToString();
                Debug.WriteLine($" {_caseSensitiveIndex}");
            }
            base.OnTextChanged(e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent is not null)
            {
                _dropDownContainer.MinimumSize = ClientRectangle.Size; // Allows for border
                _dropDownContainer.VisibleChanged += (sender, e) =>
                {
                    localOnParentMoved(Parent, EventArgs.Empty);
                    localOnParentSizeChanged(Parent, EventArgs.Empty);
                };
                Parent.SizeChanged -= localOnParentSizeChanged;
                Parent.SizeChanged += localOnParentSizeChanged;
                Parent.Move -= localOnParentMoved;
                Parent.Move += localOnParentMoved;
                void localOnParentSizeChanged(object? sender, EventArgs e) =>
                    MinimumSize = Size;
                void localOnParentMoved(object? sender, EventArgs e)
                {
                    _dropDownContainer.Location = PointToScreen(new Point(0, Height));
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
                    if (value && !_dropDownContainer.Visible) _dropDownContainer.Show(this);
                    else _dropDownContainer.Hide();
                }
            }
        }
        bool _droppedDown = default;

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

        class DropDownContainer : Form
        {
            public DropDownContainer()
            {
                Visible = false;
                AutoSizeMode = AutoSizeMode.GrowAndShrink;
                AutoSize = true;
                BackColor = Color.White;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public BindingList<object> Items { get; } = new BindingList<object>();
    }
    public class Item
    {
        public static implicit operator Item(string text) =>
            new Item { Text = text };
        public string Text { get; set; } = "Item";

        [Category("Appearance")]
        public Color BackColor { get; set; } = Color.White;

        [Category("Appearance")]
        public Color ForeColor { get; set; } = Color.Black;

        [Browsable(false)]
        internal CheckBox? Control { get; set; }

        public override string ToString() => Text;
    }
}
