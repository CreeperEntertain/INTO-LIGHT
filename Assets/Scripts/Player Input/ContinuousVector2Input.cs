using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContinuousVector2Input : MonoBehaviour
{
    [Serializable]
    public class Source
    {
        public string name;
        public InputActionReference action;
        public bool isRelative;
    }

    [SerializeField] private Source[] sources;

    public Vector2 Value { get; private set; }
    public bool IsRelative { get; private set; }

    private InputAction activeAction;


    private void OnEnable()
    {
        if (sources == null)
            return;
        foreach (Source source in sources)
        {
            if (source?.action?.action == null)
                continue;
            InputAction action = source.action.action;
            action.performed += OnInput;
            action.canceled += OnInput;
            action.Enable();
        }
    }

    private void OnDisable()
    {
        if (sources != null)
            foreach (Source source in sources)
            {
                if (source?.action?.action == null)
                    continue;
                InputAction action = source.action.action;
                action.performed -= OnInput;
                action.canceled -= OnInput;
                action.Disable();
            }
        activeAction = null;
        Value = Vector2.zero;
    }

    private void OnInput(InputAction.CallbackContext context)
    {
        activeAction = context.action;
        Value = context.ReadValue<Vector2>();
        foreach (Source source in sources)
            if (source?.action?.action == activeAction)
            {
                IsRelative = source.isRelative;
                break;
            }
    }
}
