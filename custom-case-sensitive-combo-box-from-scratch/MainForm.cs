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
            _richTextBox.TextChanged += (sender, e) => Text = _richTextBox.Text;
            tableLayoutPanel.Controls.Add(_richTextBox,0,0);
            tableLayoutPanel.Controls.Add(_dropDownIcon, 1,0);
            _dropDownIcon.MouseDown += (sender, e) => 
                BeginInvoke(()=> DroppedDown = !DroppedDown);
            Items.ListChanged += OnItemsChanged;
            HandleCreated += (sender, e) => BeginInvoke(() => Focus());
            _dropDownContainer.ItemClicked += OnItemClicked;
        }

        private void OnItemClicked(object? sender, ItemClickedEventArgs e)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Equals(Items[i], e.StringExact))
                {
                    SelectedIndex = i;
                }
            }
            DroppedDown = false;
        }

        private void OnItemsChanged(object? sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    var item = Items[e.NewIndex];
                    if (item?.ToString() is string text && !string.IsNullOrWhiteSpace(text))
                    {
                        switch (item)
                        {
                            case Control control when control is ISelectable selectable:
                                // Custom data template
                                Debug.Fail("TODO");
                                break;
                            case object o:
                                var view = new DefaultView
                                {
                                    Text = o?.ToString() ?? string.Empty,
                                    TextAlign = ContentAlignment.MiddleLeft,
                                    Font = Font,
                                    Height = Height,
                                };
                                if(Templates.ContainsKey(text))
                                {
                                    throw new InvalidOperationException($"Key '{text}' already exists.");
                                }
                                else
                                {
                                    Templates[text] = view;
                                }
                                _dropDownContainer.Add(view);
                                break;
                        }
                    }
                    else Debug.Fail("Unexpected text is whitespace or null");
                    break;
            }
        }

        private readonly RichTextBoxEx _richTextBox = new();
        private readonly DropDownIcon _dropDownIcon = new();
        private readonly DropDownContainer _dropDownContainer = new();
        private Dictionary<string, object> Templates { get; } = new ();
        private string? _caseSensitiveText = null;

        /// <summary>
        /// This property tracks separately from that of _dropDownCaontainer.
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (!Equals(_dropDownContainer.SelectedIndex, value))
                {
                    _selectedIndex = value;
                    _dropDownContainer.SelectedIndex = value;
                    OnPropertyChanged();
                    _richTextBox.Text =
                        value == -1
                        ? String.Empty
                        : _dropDownContainer.Selectables[value]?.ToString() ?? string.Empty;
                }
            }
        }
        int _selectedIndex = -1;
        protected override void OnTextChanged(EventArgs e)
        {
            var aspirant =
                _dropDownContainer.Selectables
                .OfType<object>()
                .FirstOrDefault(_ =>
                    (_?.ToString() ?? string.Empty)
                    .IndexOf(Text) == 0, StringComparison.Ordinal)
                as ISelectable;
            Debug.Write($"{aspirant}");
            if (aspirant != null)
            {
                _dropDownContainer.SelectedIndex = _dropDownContainer.Selectables.IndexOf(aspirant);
                _caseSensitiveText = aspirant.ToString();
                Debug.WriteLine($" {_dropDownContainer.SelectedIndex}");
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
                    if (value && !_dropDownContainer.Visible)
                    {
                        DropDown?.Invoke(this, EventArgs.Empty);
                        _dropDownContainer.Show(this);
                    }
                    else _dropDownContainer.Hide();
                }
            }
        }
        bool _droppedDown = default;

        public event EventHandler? DropDown;

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
                Text = PlaceholderText;
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
                    Text = PlaceholderText;
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

            public string PlaceholderText { get; set; } = "Select";
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

        class DefaultView : Label, ISelectable
        {
            public override string ToString() => Text;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (!Equals(_isSelected, value))
                    {
                        _isSelected = value;
                        BackColor = value ? Color.CornflowerBlue : Color.White;
                        ForeColor = value ? Color.White : Color.Black;
                    }
                }
            }
            bool _isSelected = default;

        }

        class DropDownContainer : Form
        {
            public DropDownContainer()
            {
                Visible = false;
                BackColor = Color.White;
                AutoSize = false;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                Controls.Add(_flowLayoutPanel);
                Selectables.ListChanged += OnSelectablesChanged;
            }

            protected virtual void OnSelectablesChanged(object? sender, ListChangedEventArgs e)
            {
                switch (e.ListChangedType)
                {
                    case ListChangedType.Reset:
                        Controls.Clear();
                        break;
                    case ListChangedType.ItemAdded:
                        if(Selectables[e.NewIndex] is Control control)
                        {
                            _flowLayoutPanel.Controls.Add(control);
                        }
                        break;
                }
            }

            private readonly FlowLayoutPanel _flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Coral,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new()
            };
            protected override void OnMinimumSizeChanged(EventArgs e)
            {
                base.OnMinimumSizeChanged(e);
                _flowLayoutPanel.MinimumSize = MinimumSize;
            }

            internal void Add<T>(T control) where T: Control, ISelectable
            {
                control.BackColor = Color.White;
                control.Margin = new Padding(0,1,0,0);
                using (var graphics = control.CreateGraphics())
                {
                    var sizeF = graphics.MeasureString(control.Text, control.Font);
                    control.Width = Convert.ToInt32(sizeF.Width);
                }
                control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                switch (control.Width.CompareTo(_flowLayoutPanel.Width))
                {
                    case -1:
                        control.Width = _flowLayoutPanel.Width;
                        break;
                    case 1:
                        _flowLayoutPanel.Width = control.Width;
                        break;
                }
                Selectables.Add(control);
                Height = _flowLayoutPanel.Controls.OfType<Control>().Sum(_=>_.Height);
                control.Click += (sender, e) =>
                {
                    switch (sender)
                    {
                        case Label label:
                            ItemClicked?.Invoke(this, new ItemClickedEventArgs(label.Text));
                            break;
                        default:
                            ItemClicked?.Invoke(this, new ItemClickedEventArgs(sender?.ToString()));
                            break;
                    }
                };
            }
            internal BindingList<ISelectable> Selectables { get; } = new();
            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
                if (Visible)
                {
                    RefreshSelection();
                }
            }

            private void RefreshSelection()
            {
                int index = 0;
                foreach (var item in Selectables)
                {
                    if (item is ISelectable selectable)
                    {
                        selectable.IsSelected = index == SelectedIndex;
                    }
                    index++;
                }
            }

            public int SelectedIndex
            {
                get => _selectedIndex;
                set
                {
                    if (!Equals(_selectedIndex, value))
                    {
                        _selectedIndex = value;
                        RefreshSelection();
                    }
                }
            }
            int _selectedIndex = -1;

            public event EventHandler<ItemClickedEventArgs>? ItemClicked;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public BindingList<object> Items { get; } = new BindingList<object>();

        class ItemClickedEventArgs : EventArgs
        {
            public ItemClickedEventArgs(string? stringExact) => StringExact = stringExact;
            public string? StringExact { get; }
        }
    }
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
}
