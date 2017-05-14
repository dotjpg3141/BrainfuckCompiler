using System;

namespace BrainfuckCompiler.Compiler
{
    public abstract class ObjectStream<T>
    {
        private StreamState currentState;
        private StreamState? peekState;

        public T Current => this.currentState.Item;

        public bool EndOfStream => this.currentState.EndOfStream;

        public bool MoveNext()
        {
            this.currentState = this.peekState ?? this.DoMoveNext();
            this.peekState = null;
            return !this.EndOfStream;
        }

        public bool CanPeek()
        {
            if (this.currentState.EndOfStream)
            {
                return false;
            }

            this.peekState = this.peekState ?? this.DoMoveNext();
            return !this.peekState.Value.EndOfStream;
        }

        public T Peek()
        {
            if (!this.CanPeek())
            {
                throw EndOfStreamError();
            }

            // assume peekState != null
            return this.peekState.Value.Item;
        }

        protected abstract bool OnMoveNext(out T next);

        private StreamState DoMoveNext()
        {
            var state = this.peekState ?? this.currentState;

            if (state.EndOfStream)
            {
                throw EndOfStreamError();
            }

            var nextState = default(StreamState);
            nextState.EndOfStream = !this.OnMoveNext(out nextState.Item);
            return nextState;
        }

        private static Exception EndOfStreamError() =>
            new InvalidOperationException("end of stream reached");

        private struct StreamState
        {
            public bool EndOfStream;
            public T Item;
        }
    }
}
