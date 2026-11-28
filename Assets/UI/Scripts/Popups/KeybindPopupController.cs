using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class KeybindPopupController
{
    public static void Show(
        PlayerInput keybinds,
        InputBindingDisplayData displayData,
        VisualTreeAsset contentAsset,
        VisualTreeAsset rowAsset,
        Action onApplied
    )
    {
        if (keybinds == null || displayData == null)
            return;
        
        InputBindingTarget[] targets = displayData.BindingTargets;
        if (targets == null || targets.Length == 0)
            return;
        if (contentAsset == null || rowAsset == null)
        {
            Debug.LogError("Keybind popup UXML assets have not been assigned.");
            return;
        }

        TemplateContainer content = contentAsset.CloneTree();
        VisualElement rows = content.Q<VisualElement>("BindingRows");
        if (rows == null)
        {
            Debug.LogError("Keybind popup content is missing BindingRows.");
            return;
        }

        DropdownSelector[] selectors = new DropdownSelector[targets.Length];
        InputBindingOption[][] optionsByRow = new InputBindingOption[targets.Length][];
        for (int i = 0; i < targets.Length; i++)
        {
            InputBindingTarget target = targets[i];
            if (target == null || target.ApplyPath == null)
            {
                Debug.LogError("A keybind popup target is invalid.");
                return;
            }

            TemplateContainer row = rowAsset.CloneTree();
            DropdownSelector selector = row.Q<DropdownSelector>("BindingSelector");
            Button recordButton = row.Q<Button>("RecordButton");
            if (selector == null || recordButton == null)
            {
                Debug.LogError("Keybind popup row is missing its selector or Record button.");
                return;
            }

            Label partLabel = row.Q<Label>("BindingPartLabel");
            if (partLabel != null)
            {
                if (string.Equals(target.ComponentName, "binding", StringComparison.OrdinalIgnoreCase))
                    partLabel.style.display = DisplayStyle.None;
                else
                {
                    partLabel.text = target.ComponentName.ToUpperInvariant();
                    partLabel.style.display = DisplayStyle.Flex;
                }
            }

            optionsByRow[i] = keybinds.GetAvailableBindingOptions(target);
            List<string> labels = new();
            foreach (InputBindingOption option in optionsByRow[i])
                labels.Add(option.Label);
            
            string currentLabel = labels.Count > 0
                ? labels[0] : null;
            selector.SetOptions(labels, currentLabel);
            selectors[i] = selector;
            rows.Add(row);
        }

        InputBindingTarget firstTarget = targets[0];
        string popupId = $"keybind-{firstTarget.InputName}-{firstTarget.SourceIndex}";
        PopupManager.ShowComplex(
            popupId, displayData.ControlName, content,
            new PopupButton("Confirm", () => Apply(
                keybinds, targets, selectors, optionsByRow, onApplied
            )),
            new PopupButton("Cancel", null)
        );
    }

    private static void Apply(
        PlayerInput keybinds,
        InputBindingTarget[] targets,
        DropdownSelector[] selectors,
        InputBindingOption[][] optionsByRow,
        Action onApplied
    )
    {
        if (keybinds == null)
            return;
        
        string[] selectedPaths = new string[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            if (selectors[i] == null || optionsByRow[i] == null)
                return;
            
            int selectedIndex = selectors[i].index;
            if (selectedIndex < 0 || selectedIndex >= optionsByRow[i].Length)
                return;
            
            selectedPaths[i] = optionsByRow[i][selectedIndex].Path;
        }
        for (int i = 0; i < targets.Length; i++)
            targets[i].ApplyPath(selectedPaths[i]);
        
        keybinds.RebuildActions();
        onApplied?.Invoke();
    }
}
