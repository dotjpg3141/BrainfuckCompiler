using System;

namespace BrainfuckCompiler
{
    public class BaseProducer<T> : IProducer<T>
    {
        private readonly IProducer<T> source;

        public BaseProducer(IProducer<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.source = source;
        }

        public virtual void Begin() => this.source.Begin();

        public virtual void End() => this.source.End();

        public virtual ProducerState<T> GetNext() => this.source.GetNext();

        public virtual void Initialize() => this.source.Initialize();
    }
}
