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
            internal T t;
            internal U u; 

            internal Tuple<T, U> tuple;
            
            //internal bool IsFulfilled()
            //{
            //    return t != null & u != null;
            //}
        }

        Tuple<T, U> tuple = new Tuple<T, U>(null, null);

        public Tuple<T,U> Provide(T value, int timeout)
        {
            lock (monitor)
            {
                MyTupleWrapper myT = list.Find(t => t.u != null);
                if (myT != null)
                {
                    list.Remove(myT);
                    myT.tuple = new Tuple<T, U>(myT.t, myT.u);
                    monitor.Signal(myT);
                    return myT.tuple;
                }
                myT = new MyTupleWrapper();
                myT.t = value;
                do
                {
                    try
                    {
                        if (!monitor.Await(myT, timeout))
                            throw new TimeoutException();
                        if (myT.tuple != null)
                            return myT.tuple;
                    }
                    catch(TimeoutException)
                    {
                        RemoveT(myT);
                        throw;
                    }
                    catch (ThreadInterruptedException)
                    {
                        RemoveT(myT);
                        throw;
                    }
                   
                } while (true);
            }
        }

        public Tuple<T, U> Provide(U value, int timeout)
        {

            lock (monitor)
            {
                MyTupleWrapper myT = list.Find(t => t.u != null);
                if (myT != null)
                {
                    list.Remove(myT);
                    myT.tuple = new Tuple<T, U>(myT.t, myT.u);
                    monitor.Signal(myT);
                    return myT.tuple;
                }
                myT = new MyTupleWrapper();
                myT.u = value;
                do
                {
                    try
                    {
                        if (!monitor.Await(myT, timeout))
                            throw new TimeoutException();
                        if (myT.tuple != null)
                            return myT.tuple;
                    }
                    catch (TimeoutException)
                    {
                        RemoveU(myT);
                        throw;
                    }
                    catch (ThreadInterruptedException)
                    {
                        RemoveU(myT);
                        throw;
                    }

                } while (true);
            }
        }

        private void RemoveU(MyTupleWrapper myT)
        {
            if (myT.tuple != null)
            {
                myT.tuple = null;
                myT.u = null;
            }
            else
            {
                list.Remove(myT);
            }
        }

        private void RemoveT(MyTupleWrapper myT)
        {
            if (myT.tuple != null)
            {
                myT.tuple = null;
                myT.t = null;
            }
            else
            {
                list.Remove(myT);
            }
        }
    }
}
