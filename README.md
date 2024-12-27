Consider that it might be easier to make your own extended `ComboBoxEx` and make it behave exactly the way you want it to. One of the easier ways to make the list view is by using a borderless top-level form with a docked flow layout panel. Other than that, you need a text entry (a single-line RichTextBox works well for this) and a control for the drop down icon to toggle the visible state of the container.

It is important to track the location of the drop down item container, for example if the parent form moves.

~~~
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
                if (Selectables[e.NewIndex] is Control control)
                {
                    control.BackColor = Color.White;
                    control.Margin = new Padding(0, 1, 0, 0);
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
                    _flowLayoutPanel.Controls.Add(control);
                }
                Height = Selectables.OfType<Control>().Sum(_ => _.Height);
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
~~~

___

In this implementation, basic types like {zebra, Zebra, ZEBRA} or {1, 2, 3} are wrapped in a `DefaultTemplate`, but it leaves room for expansion with any `ISelectable` control class.

~~~
public interface ISelectable
{
    bool IsSelected { get; set; }
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
~~~

I'll put a full example repo in the comments so you can play around with it as a starting point for your project.
