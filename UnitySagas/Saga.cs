namespace UnitySagas
{
    using Data;
    using Runner;
    using System.Collections;
    using System.Collections.Generic;

    public delegate void OnAction(SagaAction data);

    public class Saga
    {
        public event OnAction OnActionEvent;

        public void OnAction(SagaAction data)
        {
            this.OnActionEvent?.Invoke(data);
        }

        public IEnumerator<ActionInfo> Run(IEnumerator rootSaga)
        {
            var rootProcess = new SagaProcess("Root", this, rootSaga, true);

            while (!rootProcess.HasFinished)
            {
                yield return rootProcess.MoveNext();
            }
        }
    }
}