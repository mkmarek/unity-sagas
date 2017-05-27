namespace UnitySagas.Tests
{
    using Data;
    using NUnit.Framework;
    using System.Collections;

    [TestFixture]
    public class TakeEffectTests
    {
        [Test]
        public void RunsTakeSagaWaitingForAction()
        {
            //Arrange
            var runner = new Saga();
            var enumerator = runner.Run(this.RootSaga());

            //Act
            SagaAction returnData = null;

            runner.OnActionEvent += (data) =>
            {
                returnData = data;
            };

            for (var i = 0; i < 100; i++)
                enumerator.MoveNext();

            //Assert
            Assert.IsNull(returnData);

            runner.OnAction(new SagaAction<int>("start", 11));

            for (var i = 0; i < 100; i++)
                enumerator.MoveNext();

            //Assert
            Assert.AreEqual("test", returnData.Type);
            Assert.AreEqual(11, returnData.GetPayload<int>());
        }

        private IEnumerator RootSaga()
        {
            var action = new Ref<SagaAction>();

            yield return Do.Take("start", action);

            yield return Do.Put(new SagaAction<int>("test", action.Value.GetPayload<int>()));
        }
    }
}