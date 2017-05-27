namespace UnitySagas.Tests
{
    using Data;
    using NUnit.Framework;
    using System.Collections;

    [TestFixture]
    public class PutEffectTests
    {
        [Test]
        public void RunsSagaAndPutsSingleAction()
        {
            //Arrange
            var runner = new Saga();
            var enumerator = runner.Run(this.RootSaga());

            //Act
            object returnData = null;

            runner.OnActionEvent += (data) =>
            {
                returnData = data;
            };

            while (enumerator.MoveNext()) ;

            //Assert
            Assert.IsNotNull(returnData);
        }

        private IEnumerator RootSaga()
        {
            yield return Do.Put(new SagaAction<int>("test", 2));
        }
    }
}