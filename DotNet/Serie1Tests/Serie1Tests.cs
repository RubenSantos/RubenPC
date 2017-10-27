using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serie1;
using System.Threading;

namespace Serie1Tests
{
    
    [TestClass]
    public class Serie1Tests
    {
        #region ExpirableLazyTests
        private class Wrapper
        {
            internal int currentTicks;
            int myNumber;
            public Wrapper(int number)
            {
                currentTicks = Environment.TickCount;
                myNumber = number;
            }
        }

        [TestMethod]
        public void ExpirableLazySimpleUseTest()
        {
            ExpirableLazy<Wrapper> expirableLazy =
                new ExpirableLazy<Wrapper>(() => { return new Wrapper(3); }, TimeSpan.MaxValue);
            Wrapper valueT1 = null;
            Wrapper valueT2 = null;

            Thread t1 = new Thread(() =>
            {
                valueT1 = expirableLazy.Value;
            });

            Thread t2 = new Thread(() =>
            {
                valueT2 = expirableLazy.Value;
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.IsNotNull(valueT1);
            Assert.AreSame(valueT1, valueT2);
            Assert.AreEqual(valueT1.currentTicks, valueT2.currentTicks);
        }
        #endregion
        #region TranferQueueUTests
        [TestMethod]
        public void TranferQueuePutAndTakeTest()
        {
            TransferQueue<int> tranferQueue = new TransferQueue<int>();
            int msg = 0;
            bool received = false;
            Thread t1 = new Thread(() =>
            {
                tranferQueue.Put(22);
            });
            Thread t2 = new Thread(() =>
            {
                received = tranferQueue.Take(Timeout.Infinite, out msg);
            });
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.IsTrue(received);
            Assert.IsTrue(msg == 22);
        }

        [TestMethod]
        public void TranferQueueTakeAndPutTest()
        {
            TransferQueue<int> tranferQueue = new TransferQueue<int>();
            int msg = 0;
            bool received = false;
            Thread t1 = new Thread(() =>
            {
                received = tranferQueue.Take(Timeout.Infinite, out msg);
            });
            Thread t2 = new Thread(() =>
            {
                tranferQueue.Put(22);
            });
            t1.Start();
            Thread.Sleep(10);
            t2.Start();
            t2.Join();
            t1.Join();
            Assert.IsTrue(received);
            Assert.IsTrue(msg == 22);
        }
    }
    #endregion

}
