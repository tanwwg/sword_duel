using System;
using System.Collections.Generic;

/// <summary>
/// Utility class to track changes of a particular state
/// </summary>
public class StateWatcher<T>
{
    private T _value;
    public T PrevValue { get; private set; }
    public bool IsChanged { get; private set; }

    private Func<T> _getter;

    public StateWatcher(Func<T> getter)
    {
        _getter = getter;
        _value = getter();
        PrevValue = _value;
    }

    public void SaveState()
    {
        this.Value = _getter();
    }
    
    private static bool Eq(T v1, T v2) => EqualityComparer<T>.Default.Equals(v1, v2);

    public T Value
    {
        get => _value;
        set
        {
            PrevValue = _value;
            _value = value;
            IsChanged = Eq(_value, PrevValue);
        }
    }
    
}