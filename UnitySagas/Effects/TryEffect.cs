namespace UnitySagas.Effects
{
    using Base;
    using Data;
    using Runner;
    using System;
    using System.Collections;

    public class TryEffect : IEffect
    {
        private CatchEnumeratorSignature catchSaga;
        private IEnumerator finallySaga;
        private IEnumerator sagaToTry;

        public TryEffect(
            IEnumerator sagaToTry,
            CatchEnumeratorSignature catchSaga,
            IEnumerator finallySaga = null)
        {
            this.sagaToTry = sagaToTry;
            this.catchSaga = catchSaga;
            this.finallySaga = finallySaga;
        }

        public delegate IEnumerator CatchEnumeratorSignature(Exception exception);

        public IEnumerator Resolve(SagaProcess process)
        {
            while (sagaToTry.MoveNext())
            {
                var value = process.ResolveMember(sagaToTry.Current);

                if (value.Type == ActionInfoType.Exception)
                {
                    yield return this.catchSaga(value.GetPayload<Exception>());

                    break;
                }
            }

            if (this.finallySaga != null)
            {
                yield return finallySaga;
            }
        }

        ActionInfo IEffect.Resolve(SagaProcess saga)
        {
            return new ActionInfo(ActionInfoType.SubProcess, Utils.Flatten(this.Resolve(saga)));
        }
    }
}