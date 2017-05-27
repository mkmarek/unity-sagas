namespace UnitySagas
{
    using Data;
    using Effects;
    using System.Collections;

    public static class Do
    {
        public static CallEffect<TReturnData> Call<TReturnData>(
            CallEffect<TReturnData>.CallTarget target, Ref<TReturnData> returnValue)
        {
            return new CallEffect<TReturnData>(target, returnValue, new object[] { });
        }

        public static CallEffect<TReturnData> Call<TReturnData>(
            CallEffect<TReturnData>.CallTarget target, Ref<TReturnData> returnValue, params object[] args)
        {
            return new CallEffect<TReturnData>(target, returnValue, args);
        }

        public static PutEffect<TData> Put<TData>(SagaAction<TData> data)
        {
            return new PutEffect<TData>(data);
        }

        public static TakeEffect Take(string action, Ref<SagaAction> actionRef)
        {
            return new TakeEffect(action, actionRef);
        }

        public static CallEffect<TReturnData> ThreadCall<TReturnData>(
            CallEffect<TReturnData>.CallTarget target, Ref<TReturnData> returnValue)
        {
            return new ThreadCallEffect<TReturnData>(target, returnValue, new object[] { });
        }

        public static CallEffect<TReturnData> ThreadCall<TReturnData>(
            CallEffect<TReturnData>.CallTarget target, Ref<TReturnData> returnValue, params object[] args)
        {
            return new ThreadCallEffect<TReturnData>(target, returnValue, args);
        }

        public static TryEffect Try(
            IEnumerator sagaToTry,
            TryEffect.CatchEnumeratorSignature catchSaga,
            IEnumerator finallySaga = null)
        {
            return new TryEffect(sagaToTry, catchSaga, finallySaga);
        }
    }
}