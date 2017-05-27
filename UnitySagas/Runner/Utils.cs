namespace UnitySagas.Runner
{
    using System.Collections;

    public static class Utils
    {
        public static IEnumerator Flatten(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                IEnumerator candidate = enumerator.Current as IEnumerator;
                if (candidate != null)
                {
                    var flat = Flatten(candidate);
                    while (flat.MoveNext())
                    {
                        yield return flat.Current;
                    }
                }
                else
                {
                    yield return enumerator.Current;
                }
            }
        }
    }
}