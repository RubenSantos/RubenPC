using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serie2
{
    class SafeRefCountedHolder<T> where T : class
    {
        private class Holder
        {
            internal readonly T value;
            internal readonly int refCount;

            internal Holder(T t, int count)
            {
                value = t;
                refCount = count;
            }
        }

        private Holder holder;

        public SafeRefCountedHolder(T t)
        {
            holder = new Holder(t, 1);
        }

        public void AddRef()
        {
            while (true)
            {
                if (holder.refCount == 0)
                    throw new InvalidOperationException();
                Holder lh = holder;
                if (Interlocked.CompareExchange(ref holder, new Holder(lh.value, lh.refCount + 1), lh) != lh)
                    return;
            }
            
        }

        public void ReleaseRef()
        {

            while (true)
            {
                if (holder.refCount == 0)
                    throw new InvalidOperationException();
                Holder lh = holder;
                IDisposable disposable = lh.value as IDisposable;
                if (Interlocked.CompareExchange(ref holder, new Holder(null, 0), lh) != lh)
                {
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
        }

        public T Value
        {
            get
            {
                if (holder.refCount == 0)
                    throw new InvalidOperationException();
                return holder.value;
            }
        }
    }
}
