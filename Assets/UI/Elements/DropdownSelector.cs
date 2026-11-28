using System.Collections.Generic;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DropdownSelector : DropdownField
{
    [UxmlAttribute]
    public string[] Options
    {
        get => choices.ToArray();
        set => SetOptions(value);
    }

    public DropdownSelector()
    {
        AddToClassList("dropdown-selector");
    }


    public void SetOptions(IEnumerable<string> options, string selectedOption = null)
    {
        List<string> newChoices = new();
        if (options != null)
            foreach (string option in options)
                if (!string.IsNullOrWhiteSpace(option) && !newChoices.Contains(option))
                    newChoices.Add(option);
        choices = newChoices;
        int selectedIndex = selectedOption != null
            ? choices.IndexOf(selectedOption) : -1;
        index = choices.Count == 0
            ? -1 : selectedIndex >= 0
                ? selectedIndex : 0;
    }
}