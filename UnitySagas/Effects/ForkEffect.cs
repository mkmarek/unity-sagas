namespace UnitySagas.Effects
{
    using Base;
    using Data;
    using Runner;
    using System;
    using System.Collections;

    public class ForkEffect : IEffect
    {
        private IEnumerator enumerator;

        public ForkEffect(IEnumerator enumerator)
        {
            this.enumerator = enumerator;
        }

        ActionInfo IEffect.Resolve(SagaProcess process)
        {
            process.EnqueueSubProcess(new SagaProcess($"Fork_{Guid.NewGuid()}", process.Saga, this.enumerator, false));

            return new ActionInfo(ActionInfoType.Noop, null);
        }
    }
}