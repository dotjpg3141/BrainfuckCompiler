namespace BrainfuckCompiler.Compiler.Model
{
    public class DataType
    {
        internal DataType(int id, string name, string moveLeft, string moveRight)
        {
            this.Name = name;
            this.Id = id;
            this.MoveLeft = moveLeft;
            this.MoveRight = moveRight;
        }

        public int Id { get; }

        public string Name { get; }

        public string MoveLeft { get; }

        public string MoveRight { get; }

        public override string ToString()
        {
            return $"{this.Name}({this.Id})";
        }
    }
}
