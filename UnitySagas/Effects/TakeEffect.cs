namespace UnitySagas.Effects
{
    using Base;
    using Data;
    using Runner;
    using System.Collections;

    public class TakeEffect : IEffect
    {
        private string action;
        private bool resolved = false;
        private Ref<SagaAction> actionRef;

        public TakeEffect(string action, Ref<SagaAction> actionRef)
        {
            this.action = action;
            this.actionRef = actionRef;
        }

        ActionInfo IEffect.Resolve(SagaProcess process)
        {
            process.Saga.OnActionEvent += OnAction;

            return new ActionInfo(ActionInfoType.SubProcess, this.Resolve(process));
        }

        private void OnAction(SagaAction action)
        {
            if (this.actionRef != null)
            {
                this.actionRef.Value = action;
            }

            resolved = true;
        }

        private IEnumerator Resolve(SagaProcess process)
        {
 
            while (!resolved)
            {
                yield return new ActionInfo(ActionInfoType.Effect, this);
            }

            yield return new ActionInfo(ActionInfoType.Effect, this);

            process.Saga.OnActionEvent -= OnAction;
        }
    }
}