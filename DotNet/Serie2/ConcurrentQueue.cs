using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serie2
{
    public class ConcurrentQueue<T> where T: class
    {
        private class Node
        {
            internal readonly T item;
            internal volatile Node next;

            internal Node(T item, Node next)
            {
                this.item = item;
                this.next = next;
            }
        }

        private readonly Node dummy;
        private volatile Node head;
        private volatile Node tail;

        public ConcurrentQueue()
        {
            dummy = new Node(null, null);
            head = dummy;
            tail = dummy;

        }

        public void Put(T t)
        {
            Node newNode = new Node(t, null);
            while (true)
            {
                Node currTail = tail;
                Node tailNext = currTail.next;
                if(currTail == tail)
                {
                    if(tailNext != null)
                    {
                        Interlocked.CompareExchange(ref tail, tailNext, currTail);
                    }
                    else
                    {
                        if(Interlocked.CompareExchange(ref currTail.next, newNode, null) == newNode)
                        {
                            Interlocked.CompareExchange(ref tail, newNode, currTail);
                            return;
                        }
                    }
                }
            }
        }

        public T TryTake()
        {
            while (true)
            {
                Node currHead = head;
                Node headNext = currHead.next;
                if(headNext == null)
                {
                    return null;
                }
                if(Interlocked.CompareExchange(ref head, headNext, currHead) == currHead)
                {
                    return currHead.item;
                }
            }
        }

        public bool IsEmpty()
        {
            return head.Equals(tail);
        }
    }
}
