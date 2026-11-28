using System.Collections.Generic;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DropdownSelector : DropdownField
{
    [UxmlAttribute]
    public List<string> Options
    {
        get => choices;
        set
        {
            choices = value ?? new List<string>();
            if (choices.Count > 0 && !choices.Contains(this.value))
                index = 0;
        }
    }

    public DropdownSelector()
    {
        AddToClassList("dropdown-slector");
    }


    public void SetOptions(IEnumerable<string> options, string selectedOption = null)
    {
        foreach (string option in options)
            if (option != null)
                choices.Add(option);
        if (choices.Count == 0)
            return;
        int selectedIndex = selectedOption != null
            ? choices.IndexOf(selectedOption) : -1;
        index = selectedIndex >= 0
            ? selectedIndex : 0;
    }
}
