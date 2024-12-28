using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace custom_case_sensitive_combo_box_from_scratch
{
    public class CaseSensitiveComboBox : Panel
    {
        public CaseSensitiveComboBox()
        {
            _dropDownContainer = new DropDownContainer(this);
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.White;
            var tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, Width = 80));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, Width = 20));
            Controls.Add(tableLayoutPanel);
            _richTextBox.TextChanged += (sender, e) =>
            {
                Text = _richTextBox.Text;
                if (_richTextBox.IsPlaceholderText)
                {
                    _dropDownContainer.SelectedIndex = -1;
                }
                else
                {
                    var aspirant =
                        _dropDownContainer.Selectables
                        .OfType<object>()
                        .FirstOrDefault(_ =>
                            (_?.ToString() ?? string.Empty)
                            .IndexOf(Text) == 0, StringComparison.Ordinal)
                        as ISelectable;
                    Debug.Write($"{aspirant}");
                    if (aspirant is null || string.IsNullOrWhiteSpace(Text))
                    {
                        _dropDownContainer.SelectedIndex = -1;
                    }
                    else
                    {
                        _dropDownContainer.SelectedIndex = _dropDownContainer.Selectables.IndexOf(aspirant);
                        _caseSensitiveText = aspirant.ToString();
                        Debug.WriteLine($" {_dropDownContainer.SelectedIndex}");
                    }
                }
            };
            tableLayoutPanel.Controls.Add(_richTextBox, 0, 0);
            tableLayoutPanel.Controls.Add(_dropDownIcon, 1, 0);
            _dropDownIcon.MouseDown += (sender, e) =>
                BeginInvoke(() => DroppedDown = !DroppedDown);
            Items.ListChanged += OnItemsChanged;
            HandleCreated += (sender, e) => BeginInvoke(() => Focus());
            _dropDownContainer.ItemClicked += OnItemClicked;

            // Metrics for list
            HandleCreated +=  _dropDownContainer.UpdateMetrics;
            SizeChanged += _dropDownContainer.UpdateMetrics;
            Move += _dropDownContainer.UpdateMetrics;
            ParentChanged += (sender, e) =>
            {
                if (Parent is not null)
                {
                    Parent.Move -= _dropDownContainer.UpdateMetrics;
                    Parent.Move += _dropDownContainer.UpdateMetrics;
                }
            };
        }

        private void OnItemClicked(object? sender, ItemClickedEventArgs e)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Equals(Items[i].ToString(), e.StringExact))
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
                                _dropDownContainer.Selectables.Add(view);
                                break;
                        }
                    }
                    else Debug.Fail("Unexpected text is whitespace or null");
                    break;
            }
        }

        private readonly RichTextBoxEx _richTextBox = new();
        private readonly DropDownIcon _dropDownIcon = new();
        private readonly DropDownContainer _dropDownContainer;
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
                IsPlaceholderText = true;
                LostFocus += Commit;
                KeyDown += Commit;
                BackColor = DesignMode ? Color.LightGray : Color.White;
            }
            private void Commit(object? sender, EventArgs e)
            {
                if (e is KeyEventArgs eKey)
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
                    IsPlaceholderText = true;
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
                if (IsPlaceholderText)
                {
                    Text = string.Empty;
                    IsPlaceholderText = false;
                }
            }
            public bool IsPlaceholderText { get; private set; } = true;
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
            public DefaultView() => BackColor = Color.White;
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
            public DropDownContainer(Control control)
            {
                Visible = false;
                BackColor = Color.White;
                AutoSize = false;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                Controls.Add(_flowLayoutPanel);
                Selectables.ListChanged += OnSelectablesChanged;
                Control = control;
            }
            private Control Control { get; }
            protected virtual void OnSelectablesChanged(object? sender, ListChangedEventArgs e)
            {
                switch (e.ListChangedType)
                {
                    case ListChangedType.Reset:
                        Controls.Clear();
                        break;
                    case ListChangedType.ItemAdded:
                        if (Selectables[e.NewIndex] is Control control)
                        {
                            control.Click += (sender, e) =>
                            {
                                switch (sender)
                                {
                                    case ISelectable selectable:
                                        ItemClicked?.Invoke(this, new ItemClickedEventArgs(selectable.Text));
                                        break;
                                    default:
                                        Debug.Fail("ADVISORY TODO: This is much easier if your data template inherits ISelectable");
                                        break;
                                }
                            };
                            _flowLayoutPanel.Controls.Add(control);
                        }
                        break;
                }
                UpdateMetrics(this, EventArgs.Empty);
            }

            private readonly FlowLayoutPanel _flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Coral,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new()
            };
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

            internal void UpdateMetrics(object? sender, EventArgs e)
            {
                if(Control.IsHandleCreated)
                {
                    var screen = Control.RectangleToScreen(Control.ClientRectangle);
                    // "Same" location as control, offset by the height of the control.
                    Location = new Point(screen.Location.X, screen.Location.Y + screen.Height);
                }

                foreach (var control in Selectables.OfType<Control>())
                {
                    control.Margin = new Padding(0, 1, 0, 0);
                    using (var graphics = control.CreateGraphics())
                    {
                        var sizeF = graphics.MeasureString(control.Text, control.Font);
                        control.Width = Convert.ToInt32(sizeF.Width);
                    }
                    //control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                    //switch (control.Width.CompareTo(_flowLayoutPanel.Width))
                    //{
                    //    case -1:
                    //        control.Width = _flowLayoutPanel.Width;
                    //        break;
                    //    case 1:
                    //        _flowLayoutPanel.Width = control.Width;
                    //        break;
                    //}
                    //Height = Selectables.OfType<Control>().Sum(_ => _.Height);
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

}
