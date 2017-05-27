namespace UnitySagas.Data
{
    public enum ActionInfoType
    {
        SubProcess, Cancel, Exception,
        Noop,
        Effect,
        WaitForAsyncOperation
    }

    public class ActionInfo
    {
        private object payload;

        public ActionInfo(ActionInfoType type, object payload)
        {
            this.Type = type;
            this.payload = payload;
        }

        public ActionInfoType Type { get; private set; }

        internal T GetPayload<T>()
        {
            return (T)payload;
        }
    }
}