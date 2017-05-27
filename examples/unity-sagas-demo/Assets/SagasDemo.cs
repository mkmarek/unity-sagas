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

    private IEnumerator RootSaga()
    {
        var output = new Ref<int>();

        yield return Do.ThreadCall(this.GetAmazinglyAccurateAnswerToLifeTheUniverseAndEverything, output);

        yield return Do.Put(new SagaAction<int>("THE_ANSWER", output.Value));
    }

	// Use this for initialization
	void Start () {
        var saga = new Saga();

        saga.OnActionEvent += Saga_OnActionEvent;

        this.StartCoroutine(saga.Run(this.RootSaga()));
	}

    private void Saga_OnActionEvent(SagaAction data)
    {
        Debug.Log("The answer is: " + data.GetPayload<int>());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
