namespace UnitySagas.Tests
{
    using Data;
    using NUnit.Framework;
    using System.Collections;
    using System.Collections.Generic;

    [TestFixture]
    public class TakeEffectTests : BaseTest
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

        [Test]
        public void OneForkedActionWaitingForInputFromOtherForkedAction()
        {
            //Arrange
            var runner = new Saga();
            var actions = new List<SagaAction>();

            //Act
            runner.OnActionEvent += (data) =>
            {
                actions.Add(data);
            };

            this.RunSaga(runner, this.OneForkedActionWaitingForInputFromOtherForkedActionSaga());

            //Assert
            Assert.AreEqual("TEST", actions[0].Type);
            Assert.AreEqual("TEST2", actions[1].Type);
            Assert.AreEqual("TEST3", actions[2].Type);
        }

        private IEnumerator RootSaga()
        {
            var action = new Ref<SagaAction>();

            yield return Do.Take("start", action);

            yield return Do.Put(new SagaAction<int>("test", action.Value.GetPayload<int>()));
        }

        private IEnumerator SagaWithTake()
        {
            yield return Do.Take("TEST", new Ref<SagaAction>());
            yield return Do.Put(new SagaAction("TEST3"));
        }

        private IEnumerator SagaWithPut()
        {
            yield return Do.Put(new SagaAction("TEST"));
            yield return Do.Put(new SagaAction("TEST2"));
        }

        private IEnumerator OneForkedActionWaitingForInputFromOtherForkedActionSaga()
        {
            yield return Do.Fork(SagaWithTake());

            yield return Do.Fork(SagaWithPut());
        }
    }
}