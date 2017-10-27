using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Serie1
{
    public class Pairing<T, U>  where T : class 
                                where U :class
    {
        private Object monitor = new Object();
        private List<MyTupleWrapper> list = new List<MyTupleWrapper>();
        private class MyTupleWrapper
        {
            internal T tValue;
            internal U uValue; 

            internal Tuple<T, U> tuple;
            
            internal bool IsFulfilled()
            {
                return tValue != null & uValue != null;
            }
        }

        Tuple<T, U> tuple = new Tuple<T, U>(null, null);

        public Tuple<T,U> Provide(T value, int timeout)
        {
            lock (monitor)
            {
                MyTupleWrapper myTupleWrapper = list.Find(mtw => mtw.uValue != null);
                if (myTupleWrapper != null)
                {
                    list.Remove(myTupleWrapper);
                    monitor.Signal(myTupleWrapper);
                    myTupleWrapper.tuple = new Tuple<T, U>(value, myTupleWrapper.uValue);
                    return myTupleWrapper.tuple;
                }
                myTupleWrapper = new MyTupleWrapper();
                myTupleWrapper.tValue = value;
                list.Add(myTupleWrapper);
                do
                {
                    timeout = SynchUtils.RemainingTimeout(Environment.TickCount, timeout);
                    try
                    {
                        if (!monitor.Await(myTupleWrapper, timeout))
                            if(!myTupleWrapper.IsFulfilled())
                                throw new TimeoutException();
                    }
                    catch
                    {
                        if (myTupleWrapper.IsFulfilled())
                        {
                            Thread.CurrentThread.Interrupt();
                            return myTupleWrapper.tuple;
                        }
                        list.Remove(myTupleWrapper);
                        throw;
                    }

                } while (!myTupleWrapper.IsFulfilled());
                return myTupleWrapper.tuple;
            }
        }

        public Tuple<T, U> Provide(U value, int timeout)
        {
            lock (monitor)
            {
                MyTupleWrapper myTupleWrapper = list.Find(mtw => mtw.tValue != null);
                if(myTupleWrapper != null)
                {
                    list.Remove(myTupleWrapper);
                    monitor.Signal(myTupleWrapper);
                    myTupleWrapper.tuple = new Tuple<T, U>(myTupleWrapper.tValue, value);
                    return myTupleWrapper.tuple;
                }
                myTupleWrapper = new MyTupleWrapper();
                myTupleWrapper.uValue = value;
                list.Add(myTupleWrapper);
                do
                {
                    timeout = SynchUtils.RemainingTimeout(Environment.TickCount, timeout);
                    try
                    {
                        if (!monitor.Await(myTupleWrapper))
                            if (!myTupleWrapper.IsFulfilled())
                                throw new TimeoutException();
                    }
                    catch
                    {
                        if (myTupleWrapper.IsFulfilled())
                        {
                            Thread.CurrentThread.Interrupt();
                            return myTupleWrapper.tuple;
                        }
                        list.Remove(myTupleWrapper);
                        throw;
                    }
                } while (!myTupleWrapper.IsFulfilled());
                return myTupleWrapper.tuple;
            }
        }
    }
}
