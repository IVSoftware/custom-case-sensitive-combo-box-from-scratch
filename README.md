
At first I tried subclassing a `ComboBox` to try and bend it to have the case sensitive qualities you describe, but similar to the reporting in the comments I found this particular behavior to be especially challenging even though I do this kind of thing a lot. In cases like this it's fairly straightforward to create a custom "combo box" from scratch, i.e. _without_ subclassing `ComboBox` or its `TextBox` and `ListBox` components.


[![case-sensitive tracking][1]][1]

___

**The Essentials**

The general idea is that you just need the basic elements of text entry, a toggle for the visibility of a top-level container (i.e. a borderless `Form`, configured to display list items), and a dynamic means to track any movement of the custom control (or more likely, the parent form of the custom) so that the "list box" stays stuck to the main control.

- Using a single-line `RichTextBox` is a good way to get text entry without the "focus underline" artifact of `TextBox`. 

- Using something like a `TableLayoutPanel` to contain the text entry control and the drop down icon leaves open the possibility for extra functional icons. You could, for example, implement a dynamic add behavior, represented by a '+' symbol.

- The "drop down triangle" can be a label, with a simple unicode ▼ symbol as text, that toggles the visibility of a borderless, top-level form.

- The location of the form, which can be used flexibly to display a list of items, is bound (in some respects) to the size of the main control, and to movements of the main control's containing form.

- Using something like a `FlowLayoutPanel` docked (fill) to the form provides a flexible container for a variety of data templates. You could, for example, have items with check boxes, functional color schemes, or a dynamic delete behavior.

___

You can browse a [full example repo](https://github.com/IVSoftware/custom-case-sensitive-combo-box-from-scratch.git) and you can play around with it as a starting point for your project. See also: https://github.com/IVSoftware/custom-combo-box-from-scratch.git which demonstrates a version with dynamic features.

The TL;DR is that we're going to track text changes on the `RichTextBox` to search the view templates in the `_dropDownContainer_` and visually select the first case-sensitive match. Clicking on a line item:

- Sets the `SelectedIndex` property
- Copies the line item text to `richTexBox`
- Closes the drop list.

~~~
public class CaseSensitiveComboBox : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Bindable(true)]
    public BindingList<object> Items { get; } = new BindingList<object>();

    public CaseSensitiveComboBox()
    {
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
        _dropDownContainer.ItemClicked += OnItemClicked;
    }
    ...
 ~~~

 ___

 **Displaying the List Items**

 In this case, we made it so that basic types like strings like "zebra" or values like 1, 2, 3 will be wrapped in a selectable data template while providing a hook for more full-featured `ISelectable` implementations.

~~~ 
public interface ISelectable
{
    bool IsSelected { get; set; }
    public string Text { get; set; }
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




  [1]: https://i.sstatic.net/WzDhxzwX.png