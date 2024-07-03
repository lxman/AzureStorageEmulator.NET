using System.Collections.Concurrent;

namespace AzureStorageEmulator.NET.Queue.Models
{
    public class ConcurrentActiveCountableQueue<T> : ConcurrentQueue<T>
    {
        public EventHandler? CountChanged;

        public ConcurrentActiveCountableQueue() : base()
        {
        }

        public ConcurrentActiveCountableQueue(IEnumerable<T> collection) : base(collection)
        {
        }

        public new int Count => base.Count;

        public new void Clear()
        {
            base.Clear();
            CountChanged?.Invoke(this, EventArgs.Empty);
        }

        public new bool TryPeek(out T? result)
        {
            return base.TryPeek(out result);
        }

        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            CountChanged?.Invoke(this, EventArgs.Empty);
        }

        public new bool TryDequeue(out T? result)
        {
            bool success = base.TryDequeue(out result);
            if (success)
            {
                CountChanged?.Invoke(this, EventArgs.Empty);
            }
            return success;
        }
    }
}