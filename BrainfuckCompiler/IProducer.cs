namespace BrainfuckCompiler
{
    public interface IProducer<T>
    {
        void Initialize();

        void Begin();

        ProducerState<T> GetNext();

        void End();
    }

    public struct ProducerState<T>
    {
        public bool Done { get; set; }

        public T Value { get; set; }
    }
}
