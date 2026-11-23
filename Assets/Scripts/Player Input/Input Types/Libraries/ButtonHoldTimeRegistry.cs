using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonHoldTimeRegistry
{
    private sealed class Entry
    {
        public int id;
        public string path;
        public float pressTime;
    }

    private static readonly List<Entry> entries = new();
    private static int nextId;


    public static int Register(string path, float pressTime)
    {
        int id = ++nextId;
        entries.Add(new()
        {
            id = id,
            path = path,
            pressTime = Mathf.Max(0.0f, pressTime)
        });
        return id;
    }

    public static void Unregister(int id)
        => entries.RemoveAll(entry => entry.id == id);

    public static bool TryGetEarliestHoldTime(InputControl control, out float pressTime)
    {
        pressTime = float.PositiveInfinity;
        bool found = false;
        foreach (Entry entry in entries)
        {
            if (!InputControlPath.Matches(entry.path, control))
                continue;
            pressTime = Mathf.Min(pressTime, entry.pressTime);
            found = true;
        }
        return found;
    }
}
