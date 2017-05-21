namespace BrainfuckCompiler.Compiler.Model
{
    public class DataType
    {
        internal DataType(int id, string name, string moveLeft, string moveRight, string clear)
        {
            this.Name = name;
            this.Id = id;
            this.MoveLeft = moveLeft;
            this.MoveRight = moveRight;
            this.Clear = clear;
        }

        public int Id { get; }

        public string Name { get; }

        public string MoveLeft { get; }

        public string MoveRight { get; }

        public string Clear { get; }

        public override string ToString() => this.Name;
    }
}
