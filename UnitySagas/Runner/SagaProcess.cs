namespace UnitySagas.Runner
{
    using Base;
    using Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class SagaProcess
    {
        public Saga Saga;
        private IEnumerator processEnumerator;
        private List<SagaProcess> processesToKill;
        private List<SagaProcess> runningProcesses;
        private Queue<SagaProcess> subProcesses;

        public SagaProcess(string name, Saga saga, IEnumerator enumerator, bool isSynchronous)
        {
            this.Name = name;
            this.Saga = saga;
            this.subProcesses = new Queue<SagaProcess>();
            this.runningProcesses = new List<SagaProcess>();
            this.processesToKill = new List<SagaProcess>();
            this.IsSynchronous = isSynchronous;
            this.HasEnumeratorFinished = false;
            this.processEnumerator = Utils.Flatten(enumerator);
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

        public void EnqueueSubProcess(SagaProcess process)
        {
            this.subProcesses.Enqueue(process);
        }

        public virtual ActionInfo MoveNext()
        {
            KillFinishedProcesses();

            StartPendingProcesses();

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
            foreach (var subProcess in this.runningProcesses)
            {
                var value = subProcess.MoveNext();

                if (subProcess.HasFinished)
                {
                    this.processesToKill.Add(subProcess);
                }

                return value;
            }

            return null;
        }

        private void KillFinishedProcesses()
        {
            foreach (var process in this.processesToKill)
            {
                this.runningProcesses.Remove(process);
            }
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
                } while (!process.IsSynchronous);
            }
        }
    }
}