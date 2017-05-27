namespace UnitySagas.Effects
{
    using Base;
    using Data;
    using Runner;

    public class PutEffect<TData> : IEffect
    {
        private SagaAction<TData> data;

        public PutEffect(SagaAction<TData> data)
        {
            this.data = data;
        }

        ActionInfo IEffect.Resolve(SagaProcess process)
        {
            process.Saga.OnAction(data);

            return new ActionInfo(ActionInfoType.Effect, this);
        }
    }
}