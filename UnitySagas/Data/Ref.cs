namespace UnitySagas.Data
{
    public sealed class Ref<T>
    {
        private T value;

        public T Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}