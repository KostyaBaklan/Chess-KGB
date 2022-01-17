namespace Engine.DataStructures
{
    class Node<T>
    {
        public Node(T value)
        {
            Value = value;
        }

        public T Value { get; }
        public Node<T> Next { get; set; }
    }
}