namespace UnitySagas.Tests
{
    using System.Collections;

    public class BaseTest
    {
        protected void RunSaga(Saga runner, IEnumerator sagaToRun)
        {
            //Arrange
            var enumerator = runner.Run(sagaToRun);

            while (enumerator.MoveNext()) ;
        }
    }
}