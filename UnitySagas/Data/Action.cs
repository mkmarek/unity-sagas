namespace UnitySagas.Data
{
    public abstract class SagaAction
    {
        public SagaAction(string type)
        {
            this.Type = type;
        }

        public string Type { get; private set; }

        public abstract T GetPayload<T>();
    }

    public class SagaAction<TPayload> : SagaAction
    {
        public SagaAction(string type, TPayload payload) : base(type)
        {
            this.Payload = payload;
        }

        public TPayload Payload { get; private set; }

        public override T GetPayload<T>()
        {
            if (this.Payload is T)
                return (T)((object)this.Payload);

            return default(T);
        }
    }
}