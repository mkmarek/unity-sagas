namespace UnitySagas.Tests
{
    using Data;
    using NUnit.Framework;
    using System.Collections;

    [TestFixture]
    public class CallEffectTests : BaseTest
    {
        [Test]
        public void RunCallEffectFollowedByPutWithTheReturnValue()
        {
            //Arrange
            var runner = new Saga();

            //Act
            SagaAction returnData = null;

            runner.OnActionEvent += (data) =>
            {
                returnData = data;
            };

            this.RunSaga(runner, this.SimpleSaga());

            //Assert
            Assert.AreEqual(1, returnData.GetPayload<int>());
        }

        private IEnumerator SimpleSaga()
        {
            var data = new Ref<int>();

            yield return Do.Call(TestCallMethod, data);

            yield return Do.Put(new SagaAction<int>("test", data.Value));
        }

        private int TestCallMethod(params object[] args)
        {
            return 1;
        }
    }
}