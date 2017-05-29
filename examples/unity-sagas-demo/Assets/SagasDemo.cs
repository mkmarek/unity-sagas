using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnitySagas;
using UnitySagas.Data;

public class SagasDemo : MonoBehaviour {

    private int GetAmazinglyAccurateAnswerToLifeTheUniverseAndEverything(params object[] args)
    {
        Thread.Sleep(5000);

        return 42;
    }

    private int SleepForSecond(params object[] args)
    {
        Thread.Sleep(1000);

        return 1;
    }

    private IEnumerator Calulation()
    {
        var output = new Ref<int>();

        yield return Do.ThreadCall(this.GetAmazinglyAccurateAnswerToLifeTheUniverseAndEverything, output);

        yield return Do.Take("TADAA");

        yield return Do.Put(new SagaAction<int>("THE_ANSWER", output.Value));
    }

    private IEnumerator ElapsedTime()
    {
        for (var i = 5; i > 0; i--)
        {
            yield return Do.ThreadCall(this.SleepForSecond, new Ref<int>());
            yield return Do.Put(new SagaAction<int>("COUNTDOWN", i));
        }

        yield return Do.Put(new SagaAction("TADAA"));
    }

    private IEnumerator RootSaga()
    {
        yield return Do.Fork(ElapsedTime());
        yield return Do.Fork(Calulation());
    }

	// Use this for initialization
	void Start () {
        var saga = new Saga();

        saga.OnActionEvent += Saga_OnActionEvent;

        this.StartCoroutine(saga.Run(this.RootSaga()));
	}

    private void Saga_OnActionEvent(SagaAction data)
    {
        Debug.Log(data.Type + ": " + data.GetPayload<int>());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
