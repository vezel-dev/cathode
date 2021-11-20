namespace System.Input;

public class MemoryTerminalHistory : TerminalHistory
{
    public override int Count => _history.Count;

    readonly List<string> _history = new();

    public override void Add(string value)
    {
        _history.Add(value);
    }

    public override bool Remove(int index)
    {
        if (index < _history.Count)
            return false;

        _history.RemoveAt(_history.Count - index);

        return true;
    }

    public override string? Get(int index)
    {
        return index < _history.Count ? _history[^index] : null;
    }

    public override bool Set(int index, string value)
    {
        if (index < _history.Count)
            return false;

        _history[^index] = value;

        return true;
    }

    public override void Clear()
    {
        _history.Clear();
    }

    public override IEnumerator<string> GetEnumerator()
    {
        return _history.AsEnumerable().Reverse().GetEnumerator();
    }
}
