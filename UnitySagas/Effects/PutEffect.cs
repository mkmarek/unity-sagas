namespace UnitySagas.Effects
{
    using Base;
    using Data;
    using Runner;

    public class PutEffect : IEffect
    {
        private SagaAction data;

        public PutEffect(SagaAction data)
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