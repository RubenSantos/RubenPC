using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serie2
{
    class UnsafeRefCountedHolder<T> where T : class
    {
        private T value;
        private int refCount;

        public UnsafeRefCountedHolder(T t)
        {
            value = t;
            refCount = 1;
        }

        public void AddRef()
        {
            if (refCount == 0)
                throw new InvalidOperationException();
            refCount++;
        }

        public void ReleaseRef()
        {
            if (refCount == 0)
                throw new InvalidOperationException();
            if(--refCount == 0)
            {
                IDisposable disposable = value as IDisposable;
                value = null;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        public T Value
        {
            get
            {
                if (refCount == 0)
                    throw new InvalidOperationException();
                return value;
            }
        }

    }
}
