namespace UnitySagas.Tests
{
    using Data;
    using Effects;
    using NUnit.Framework;
    using System;
    using System.Collections;

    [TestFixture]
    public class TryEffectTests : BaseTest
    {
        [Test]
        public void ExceptionWithoutTryShouldCrashTheProcess()
        {
            //Arrange
            var runner = new Saga();

            //Assert
            Assert.Catch<NotImplementedException>(
                () => this.RunSaga(runner, this.DoStuff(new Ref<int>(), true)));
        }

        [Test]
        public void RunTryEffectWithFiredException()
        {
            //Arrange
            var runner = new Saga();

            //Act
            SagaAction returnData = null;

            runner.OnActionEvent += (data) =>
            {
                returnData = data;
            };

            this.RunSaga(runner, this.SimpleSaga(true));

            //Assert
            Assert.AreEqual("error", returnData.Type);
            Assert.IsNotNull(returnData.GetPayload<NotImplementedException>());
        }

        [Test]
        public void RunTryEffectWithFiredExceptionWithFinally()
        {
            //Arrange
            var runner = new Saga();

            //Act
            SagaAction returnData = null;

            runner.OnActionEvent += (data) =>
            {
                returnData = data;
            };

            this.RunSaga(runner, this.SimpleSagaWithFinally(true));

            //Assert
            Assert.AreEqual("finally", returnData.Type);
        }

        [Test]
        public void RunTryEffectWithNoFiredException()
        {
            //Arrange
            var runner = new Saga();

            //Act
            SagaAction returnData = null;

            runner.OnActionEvent += (data) =>
            {
                returnData = data;
            };

            this.RunSaga(runner, this.SimpleSaga(false));

            //Assert
            Assert.AreEqual("success", returnData.Type);
        }

        [Test]
        public void RunTryEffectWithNoFiredExceptionWithFinally()
        {
            //Arrange
            var runner = new Saga();

            //Act
            SagaAction returnData = null;

            runner.OnActionEvent += (data) =>
            {
                returnData = data;
            };

            this.RunSaga(runner, this.SimpleSagaWithFinally(false));

            //Assert
            Assert.AreEqual("finally", returnData.Type);
        }

        private IEnumerator CatchStuff(Exception exception)
        {
            yield return Do.Put(new SagaAction<Exception>("error", exception));
        }

        private IEnumerator DoStuff(Ref<int> data, bool fireException)
        {
            CallEffect<int>.CallTarget target = TestCallMethod;

            if (fireException == false)
                target = TestCallMethodNoException;

            yield return Do.Call(target, data);

            yield return Do.Put<int>(new SagaAction<int>("success", data.Value));
        }

        private IEnumerator FinallyStuff()
        {
            yield return Do.Put(new SagaAction<int>("finally", 1));
        }

        private IEnumerator SimpleSaga(bool fireException)
        {
            var data = new Ref<int>();

            yield return Do.Try(DoStuff(data, fireException), CatchStuff);
        }

        private IEnumerator SimpleSagaWithFinally(bool fireException)
        {
            var data = new Ref<int>();

            yield return Do.Try(DoStuff(data, fireException), CatchStuff, FinallyStuff());
        }

        private int TestCallMethod(params object[] args)
        {
            throw new NotImplementedException();
        }

        private int TestCallMethodNoException(params object[] args)
        {
            return 1;
        }
    }
}