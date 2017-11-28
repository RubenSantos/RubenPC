using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serie2
{
    public class ConcurrentQueue<T> where T: class
    {
        Queue<T> queue;

        public ConcurrentQueue()
        {
            queue = new Queue<T>();
        }

        public void Put(T t)
        {
            queue.Enqueue(t);
        }

        public T TryTake()
        {
            if(IsEmpty())
                return null;
            return queue.Dequeue();
        }

        public bool IsEmpty()
        {
            return queue.Count > 0;
        }
    }
}
