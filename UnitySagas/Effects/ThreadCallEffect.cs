namespace UnitySagas.Effects
{
    using Data;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class ThreadCallEffect<TReturnData> : CallEffect<TReturnData>
    {
        public ThreadCallEffect(CallTarget target, Ref<TReturnData> returnValue, params object[] args)
            : base(target, returnValue, args)
        {
        }

        protected override ActionInfo Resolve()
        {
            return new ActionInfo(ActionInfoType.SubProcess, this.ResolveEnumerator());
        }

        protected IEnumerator<ActionInfo> ResolveEnumerator()
        {
            var threadParameters = new ThreadStartParams()
            {
                Args = this.args,
                ReturnValue = new Ref<TReturnData>(),
                Target = this.target
            };

            var thread = this.ResolveThread(threadParameters);

            while (thread.IsAlive)
            {
                yield return new ActionInfo(ActionInfoType.WaitForAsyncOperation, null);
            }

            if (threadParameters.ThrownException != null)
            {
                yield return new ActionInfo(ActionInfoType.Exception, threadParameters.ThrownException);
            }
            else
            {
                this.returnValue.Value = threadParameters.ReturnValue.Value;
                yield return new ActionInfo(ActionInfoType.Effect, this);
            }
        }

        private static void ThreadStart(object input)
        {
            var options = (ThreadStartParams)input;

            try
            {
                options.ReturnValue.Value = options.Target.Invoke(options.Args);
            }
            catch (Exception exception)
            {
                options.ThrownException = exception;
            }
        }

        private Thread ResolveThread(ThreadStartParams parameters)
        {
            var thread = new Thread(ThreadStart);

            thread.Start(parameters);

            return thread;
        }

        private class ThreadStartParams
        {
            public object[] Args { get; set; }
            public Ref<TReturnData> ReturnValue { get; set; }
            public CallTarget Target { get; set; }
            public Exception ThrownException { get; set; }
        }
    }
}