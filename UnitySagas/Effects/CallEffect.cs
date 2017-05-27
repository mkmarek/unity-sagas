namespace UnitySagas.Effects
{
    using Base;
    using Data;
    using Runner;
    using System;

    public class CallEffect<TReturnData> : IEffect
    {
        protected object[] args;
        protected Ref<TReturnData> returnValue;
        protected CallTarget target;

        public CallEffect(CallTarget target, Ref<TReturnData> returnValue, params object[] args)
        {
            this.returnValue = returnValue;
            this.target = target;
            this.args = args;
        }

        public delegate TReturnData CallTarget(params object[] args);

        ActionInfo IEffect.Resolve(SagaProcess process)
        {
            return Resolve();
        }

        protected virtual ActionInfo Resolve()
        {
            try
            {
                this.returnValue.Value = target.Invoke(args);
            }
            catch (Exception exception)
            {
                return new ActionInfo(ActionInfoType.Exception, exception);
            }

            return new ActionInfo(ActionInfoType.Effect, this);
        }
    }
}