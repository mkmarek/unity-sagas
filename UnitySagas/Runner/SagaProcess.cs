namespace UnitySagas.Runner
{
    using Base;
    using Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class SagaProcess
    {
        private IEnumerator processEnumerator;
        private List<SagaProcess> runningProcesses;
        private Queue<SagaProcess> subProcesses;
        private IEnumerator<SagaProcess> processQueue;

        public SagaProcess(string name, Saga saga, IEnumerator enumerator, bool isSynchronous)
        {
            this.Name = name;
            this.Saga = saga;
            this.subProcesses = new Queue<SagaProcess>();
            this.runningProcesses = new List<SagaProcess>();
            this.IsSynchronous = isSynchronous;
            this.HasEnumeratorFinished = false;
            this.processEnumerator = Utils.Flatten(enumerator);
            this.processQueue = this.CreateProcessQueue();
        }

        public bool HasEnumeratorFinished { get; protected set; }

        public bool HasFinished
        {
            get
            {
                return this.HasEnumeratorFinished && this.subProcesses.Count == 0 && this.runningProcesses.Count == 0;
            }
        }

        public bool IsSynchronous { get; private set; }
        public string Name { get; private set; }
        public Saga Saga { get; private set; }

        public void EnqueueSubProcess(SagaProcess process)
        {
            if (this.runningProcesses.Count == 0)
            {
                this.runningProcesses.Add(process);
            }
            else if (this.runningProcesses[0].IsSynchronous)
            {
                this.subProcesses.Enqueue(process);
            }
            else
            {
                this.runningProcesses.Add(process);
            }
        }

        public virtual ActionInfo MoveNext()
        {
            StartPendingProcesses();

            var action = GetActionInfoFromCurrentIteration();

            return action;
        }

        private ActionInfo GetActionInfoFromCurrentIteration()
        {
            var actionInfo = this.InvokeRunningProcesses();
            if (actionInfo != null) return actionInfo;

            if (!this.processEnumerator.MoveNext())
            {
                this.HasEnumeratorFinished = true;
            }
            else
            {
                return HandleCurrentProcess();
            }

            return null;
        }

        public ActionInfo ResolveMember(object current)
        {
            if (current is IEffect)
            {
                var result = this.ResolveEffect(current as IEffect);

                return HandleEffectResult(result);
            }

            return new ActionInfo(ActionInfoType.Noop, null);
        }

        private ActionInfo HandleCurrentProcess()
        {
            var member = this.ResolveMember(this.processEnumerator.Current);

            if (member.Type == ActionInfoType.Cancel)
            {
                this.HasEnumeratorFinished = true;
                this.subProcesses.Clear();
            }

            if (member.Type == ActionInfoType.Exception)
            {
                throw member.GetPayload<Exception>();
            }

            return member;
        }

        private ActionInfo HandleEffectResult(ActionInfo result)
        {
            if (result.Type == ActionInfoType.SubProcess)
            {
                var resolved = Utils.Flatten(result.GetPayload<IEnumerator>());

                this.EnqueueSubProcess(new SagaProcess($"{this.Name} >> Sub", this.Saga, resolved, true));

                return new ActionInfo(ActionInfoType.Noop, null);
            }

            return result;
        }

        private ActionInfo InvokeRunningProcesses()
        {
            if (this.runningProcesses.Count == 0)
            {
                return null;
            }

            if (this.runningProcesses[0].IsSynchronous)
            {
                return RunFirstSynchronousProcess();
            }

            this.processQueue.MoveNext();
            if (this.processQueue.Current != this)
            {
                return RunAsynchronousSubProcess();
            }

            return null;
        }

        private ActionInfo RunFirstSynchronousProcess()
        {
            var subProcess = this.runningProcesses[0];
            var value = subProcess.MoveNext();

            if (subProcess.HasFinished)
            {
                this.runningProcesses.Remove(subProcess);
            }

            return value;
        }

        private ActionInfo RunAsynchronousSubProcess()
        {
            var value = this.processQueue.Current.MoveNext();

            if (this.processQueue.Current.HasFinished)
            {
                this.runningProcesses.Remove(this.processQueue.Current);
            }

            return value;
        }

        private ActionInfo ResolveEffect(IEffect effect)
        {
            return effect.Resolve(this);
        }

        private void StartPendingProcesses()
        {
            if (this.runningProcesses.Count == 0 && this.subProcesses.Count > 0)
            {
                SagaProcess process = null;
                do
                {
                    process = this.subProcesses.Dequeue();

                    this.runningProcesses.Add(process);
                } while (!process.IsSynchronous && this.subProcesses.Count > 0);
            }
        }

        private IEnumerator<SagaProcess> CreateProcessQueue()
        {
            int index = 0;
            int previousProcessCount = 0;
            var previousEnumeratorStatus = this.HasEnumeratorFinished;

            while (!this.HasEnumeratorFinished || this.runningProcesses.Count > 0)
            {
                if (WasProcessRemoved(previousProcessCount, previousEnumeratorStatus))
                {
                    index = this.ModifyIndexOnProcessRemoval(index);
                }

                previousEnumeratorStatus = this.HasEnumeratorFinished;
                previousProcessCount = this.runningProcesses.Count;


                if (index == 0 && !this.HasEnumeratorFinished)
                {
                    yield return this;
                }
                else
                {
                    yield return this.runningProcesses[index - (this.HasEnumeratorFinished ? 0 : 1)];
                }

                index++;
                index %= GetTotalProcessCountIncludingCurrentProcess();
            }
        }

        private bool WasProcessRemoved(int previousProcessCount, bool previousEnumeratorStatus)
        {
            return (!previousEnumeratorStatus && this.HasEnumeratorFinished) ||
                    previousProcessCount > this.runningProcesses.Count;
        }

        private int ModifyIndexOnProcessRemoval(int index)
        {
            index--;

            if (index < 0)
            {
                index += this.GetTotalProcessCountIncludingCurrentProcess();
            }

            return index;
        }

        private int GetTotalProcessCountIncludingCurrentProcess()
        {
            return this.runningProcesses.Count + (this.HasEnumeratorFinished ? 0 : 1);
        }
    }
}