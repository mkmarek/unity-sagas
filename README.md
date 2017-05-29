# unity-sagas
Port of https://github.com/redux-saga/redux-saga to C# for Unity3d coroutines. This project is currently in POC phase. Any contributions or ideas are welcomed!

## Examples

### Put

Allows to invoke SagaAction that can be received by multiple other sagas

```csharp
private IEnumerator RootSaga()
{
  yield return Do.Put(new SagaAction<int>("ACTION_NAME", 42));
}

void Start ()
{
  var saga = new Saga();

  saga.OnActionEvent += Saga_OnActionEvent;

  this.StartCoroutine(saga.Run(this.RootSaga()));
}

private void Saga_OnActionEvent(SagaAction data)
{
  Debug.Log("The answer is: " + data.GetPayload<int>());
}
```

### Take

Allows to wait for SagaAction to be invoked and then continues execution

```csharp
private IEnumerator RootSaga()
{
  yield return Do.Take("START", action);
}

void Start ()
{
  var saga = new Saga();
  
  this.StartCoroutine(saga.Run(this.RootSaga()));
  
  //nothing happens until following line executes
  saga.OnAction(new SagaAction<int>("START", 11));
}
```

### Call

Invokes method with a set of parameters

```csharp
private int TestCallMethod(params object[] args)
{
  return 1;
}

private IEnumerator RootSaga()
{
  var returnData = new Ref<int>();

  yield return Do.Call(TestCallMethod, returnData);
}

void Start ()
{
  var saga = new Saga();
  
  this.StartCoroutine(saga.Run(this.RootSaga()));
}
```

### ThreadCall

Invokes method with a set of parameters in a separate thread and waits for the result in a non-blocking manner

```csharp
private int TestCallMethod(params object[] args)
{
  Thread.Sleep(4000);
  return 1;
}

private IEnumerator RootSaga()
{
  var returnData = new Ref<int>();

  yield return Do.ThreadCall(TestCallMethod, returnData);
}

void Start ()
{
  var saga = new Saga();
  
  this.StartCoroutine(saga.Run(this.RootSaga()));
}
```

### Try

Yields in try/catch can be tricky. This methods tries to solve that.

```csharp
private int TestCallMethod(params object[] args)
{
  throw new NotImplementedException();
}

private IEnumerator DoStuff()
{
  var data = new Ref<int>();

  yield return Do.Call(TestCallMethod, data);

  yield return Do.Put<int>(new SagaAction<int>("SUCCESS", data.Value));
}

private IEnumerator CatchRoutine(Exception exception)
{
  yield return Do.Put(new SagaAction<Exception>("ERROR", exception));
}

private IEnumerator FinallyRoutine()
{
  yield return Do.Put(new SagaAction<int>("FINALLY", 1));
}

private IEnumerator RootSaga()
{
  yield return Do.Try(DoStuff(), CatchRoutine, FinallyRoutine());
}

void Start ()
{
  var saga = new Saga();
  
  saga.OnActionEvent += Saga_OnActionEvent;
  
  this.StartCoroutine(saga.Run(this.RootSaga()));
}

private void Saga_OnActionEvent(SagaAction data)
{
  Debug.Log("INVOKED_ACTION: " + data.Type);
  // prints out:
  // "INVOKED_ACTION: ERROR"
  // "INVOKED_ACTION: FINALLY"
}

```

### Fork

Forks saga into a separate "subprocess" allowing to run multiple sagas simultaneously.

```csharp
private IEnumerator SagaWithTake()
{
  var action = new Ref<SagaAction>();
  yield return Do.Take("TEST", action);

  yield return Do.Put(new SagaAction<int>("RESULT", action.Value.GetPayload<int>()));
}

private IEnumerator SagaWithPut()
{
  yield return Do.Put(new SagaAction<int>("TEST", 123));
}

private IEnumerator RootSaga()
{
  yield return Do.Fork(SagaWithTake());

  yield return Do.Fork(SagaWithPut());
}

void Start ()
{
  var saga = new Saga();
  
  this.StartCoroutine(saga.Run(this.RootSaga()));
}
```