using System;
using Microsoft.Extensions.Options;

namespace Trail365
{
    public class SimpleOptionsMonitor<T> : IOptionsMonitor<T>
    where T : class, new()
    {
        private class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public SimpleOptionsMonitor(T currentValue)
        {
            this.CurrentValue = currentValue ?? throw new ArgumentNullException(nameof(currentValue));
        }

        public T Get(string name)
        {
            return this.CurrentValue;
        }

        public IDisposable OnChange(Action<T, string> listener)
        {
            return new Disposable();
        }

        public T CurrentValue { get; }
    }
}
