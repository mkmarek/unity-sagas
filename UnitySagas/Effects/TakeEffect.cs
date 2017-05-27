namespace UnitySagas.Effects
{
    using Base;
    using Data;
    using Runner;
    using System.Collections;

    public class TakeEffect : IEffect
    {
        private string action;
        private Ref<SagaAction> actionRef;

        public TakeEffect(string action, Ref<SagaAction> actionRef)
        {
            this.action = action;
            this.actionRef = actionRef;
        }

        ActionInfo IEffect.Resolve(SagaProcess saga)
        {
            return new ActionInfo(ActionInfoType.SubProcess, this.Resolve(saga));
        }

        private void OnAction(SagaAction action)
        {
            this.actionRef.Value = action;
        }

        private IEnumerator Resolve(SagaProcess process)
        {
            process.Saga.OnActionEvent += OnAction;

            while (this.actionRef.Value == null)
            {
                yield return new ActionInfo(ActionInfoType.Effect, this);
            }

            process.Saga.OnActionEvent -= OnAction;
        }
    }
}