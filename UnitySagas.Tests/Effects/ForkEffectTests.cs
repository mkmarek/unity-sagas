namespace UnitySagas.Tests
{
    using Data;
    using NUnit.Framework;
    using System.Collections;
    using System.Collections.Generic;

    [TestFixture]
    public class ForkEffectTests : BaseTest
    {
        [Test]
        public void RunsTwoParallelThreadsAndSwitchesBetweenThemEachIteration()
        {
            //Arrange
            var runner = new Saga();
            var actions = new List<SagaAction>();

            //Act
            runner.OnActionEvent += (data) =>
            {
                actions.Add(data);
            };

            this.RunSaga(runner, this.TwoForkedSagasWithOneCommandBeforeAndOneAfter());

            //Assert
            Assert.AreEqual("START", actions[0].Type);
            Assert.AreEqual(0, actions[1].GetPayload<int>());
            Assert.AreEqual(1, actions[2].GetPayload<int>());
            Assert.AreEqual("END", actions[3].Type);
            Assert.AreEqual(0, actions[4].GetPayload<int>());
            Assert.AreEqual(1, actions[5].GetPayload<int>());
            Assert.AreEqual(0, actions[6].GetPayload<int>());
            Assert.AreEqual(1, actions[7].GetPayload<int>());
            Assert.AreEqual(0, actions[8].GetPayload<int>());
            Assert.AreEqual(1, actions[9].GetPayload<int>());
        }

        [Test]
        public void SwitchesBetweenParallelSagaAndMainSaga()
        {
            //Arrange
            var runner = new Saga();
            var actions = new List<SagaAction>();

            //Act
            runner.OnActionEvent += (data) =>
            {
                actions.Add(data);
            };

            this.RunSaga(runner, this.OneForkedSagaWithCycleInTheMainSaga());

            //Assert
            Assert.AreEqual("START", actions[0].Type);
  
            Assert.AreEqual(1, actions[1].GetPayload<int>());
            Assert.AreEqual(0, actions[2].GetPayload<int>());
            Assert.AreEqual(1, actions[3].GetPayload<int>());
            Assert.AreEqual(0, actions[4].GetPayload<int>());
            Assert.AreEqual(1, actions[5].GetPayload<int>());
            Assert.AreEqual(0, actions[6].GetPayload<int>());
            Assert.AreEqual(1, actions[7].GetPayload<int>());
            Assert.AreEqual(0, actions[8].GetPayload<int>());
        }

        [Test]
        public void OneSagaEndsSooner()
        {
            //Arrange
            var runner = new Saga();
            var actions = new List<SagaAction>();

            //Act
            runner.OnActionEvent += (data) =>
            {
                actions.Add(data);
            };

            this.RunSaga(runner, this.OneSagaEndsSoonerSaga());

            //Assert
            Assert.AreEqual("START", actions[0].Type);
            Assert.AreEqual(0, actions[1].GetPayload<int>());
            Assert.AreEqual(1, actions[2].GetPayload<int>());
            Assert.AreEqual("END", actions[3].Type);
            Assert.AreEqual(0, actions[4].GetPayload<int>());
            Assert.AreEqual(1, actions[5].GetPayload<int>());
            Assert.AreEqual(0, actions[6].GetPayload<int>());
            Assert.AreEqual(0, actions[7].GetPayload<int>());
        }


        private IEnumerator PutZeros(int iterations)
        {
            for (var i = 0; i < iterations; i++)
                yield return Do.Put(new SagaAction<int>("COUNT", 0));
        }

        private IEnumerator PutOnes(int iterations)
        {
            for (var i = 0; i < iterations; i++)
                yield return Do.Put(new SagaAction<int>("COUNT", 1));
        }

        private IEnumerator TwoForkedSagasWithOneCommandBeforeAndOneAfter()
        {
            yield return Do.Put(new SagaAction("START"));

            yield return Do.Fork(PutZeros(4));
            yield return Do.Fork(PutOnes(4));

            yield return Do.Put(new SagaAction("END"));
        }

        private IEnumerator OneSagaEndsSoonerSaga()
        {
            yield return Do.Put(new SagaAction("START"));

            yield return Do.Fork(PutZeros(4));
            yield return Do.Fork(PutOnes(2));

            yield return Do.Put(new SagaAction("END"));
        }

        private IEnumerator OneForkedSagaWithCycleInTheMainSaga()
        {
            yield return Do.Put(new SagaAction("START"));

            yield return Do.Fork(PutZeros(4));

            for (var i = 0; i < 4; i++)
                yield return Do.Put(new SagaAction<int>("COUNT", 1));

            yield return Do.Put(new SagaAction("END"));
        }
    }
}