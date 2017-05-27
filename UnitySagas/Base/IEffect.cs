namespace UnitySagas.Base
{
    using Data;
    using Runner;

    internal interface IEffect
    {
        ActionInfo Resolve(SagaProcess saga);
    }
}