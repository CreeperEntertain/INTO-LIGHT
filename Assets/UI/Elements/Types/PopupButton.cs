

using System;

public sealed class PopupButton
{
    public string Text { get; private set; }
    public Action OnClick { get; private set; }

    public PopupButton(string text, Action onClick)
    {
        Text = text;
        OnClick = onClick;
    }
}
