using System.Collections;
using System.Collections.Generic;

namespace System.Input
{
    public abstract class TerminalHistory : IEnumerable<string>
    {
        sealed class EmptyTerminalHistory : TerminalHistory
        {
            public override int Count => 0;

            public override void Add(string value)
            {
            }

            public override bool Remove(int index)
            {
                return false;
            }

            public override string? Get(int index)
            {
                return null;
            }

            public override bool Set(int index, string value)
            {
                return false;
            }

            public override void Clear()
            {
            }

            public override IEnumerator<string> GetEnumerator()
            {
                yield break;
            }
        }

        public static TerminalHistory None { get; } = new EmptyTerminalHistory();

        public abstract int Count { get; }

        public abstract void Add(string value);

        public abstract bool Remove(int index);

        public abstract string? Get(int index);

        public abstract bool Set(int index, string value);

        public abstract void Clear();

        public abstract IEnumerator<string> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
