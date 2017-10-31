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

        [TestMethod]
        public void ExpirableLazyTimeToLiveReached()
        {
            ExpirableLazy<Wrapper> expirableLazy =
                new ExpirableLazy<Wrapper>(() => { return new Wrapper(3); }, TimeSpan.FromMilliseconds(10));
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
            Thread.Sleep(20);
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.IsNotNull(valueT1);
            Assert.IsNotNull(valueT2);
            Assert.AreNotSame(valueT1, valueT2);
            Assert.AreNotEqual(valueT1.currentTicks, valueT2.currentTicks);
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


        [TestMethod]
        public void TransferQueueTranferAndTakeTest()
        {
            TransferQueue<int> tranferQueue = new TransferQueue<int>();
            int msg = 0;
            bool transfered = false, taken = false;
            Thread t1 = new Thread(() =>
            {
                transfered = tranferQueue.Transfer(22, Timeout.Infinite);
            });
            Thread t2 = new Thread(() =>
            {
                taken = tranferQueue.Take(Timeout.Infinite, out msg);
            });
            t1.Start();
            Thread.Sleep(10);
            t2.Start();
            t2.Join();
            t1.Join();
            Assert.IsTrue(taken);
            Assert.IsTrue(transfered);
            Assert.AreEqual(msg, 22);
        }

        [TestMethod]
        public void TransferQueueOneTakeTimeout()
        {
            TransferQueue<int> tranferQueue = new TransferQueue<int>();
            int msg1 = 0, msg2 = 0;
            bool transfered = false, taken1 = false, taken2 = true;
            Thread t1 = new Thread(() =>
            {
                transfered = tranferQueue.Transfer(22, 50);
            });
            Thread t2 = new Thread(() =>
            {
                taken1 = tranferQueue.Take(50, out msg1);
            });
            Thread t3 = new Thread(() =>
            {
                taken2 = tranferQueue.Take(50, out msg2);
            });
            t1.Start();
            t2.Start();
            Thread.Sleep(10);
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();
            Assert.IsTrue(taken1);
            Assert.IsFalse(taken2);
            Assert.IsTrue(transfered);
            Assert.AreEqual(22, msg1);
            Assert.AreEqual(default(int), msg2);
        }
        #endregion

        #region PairingTests

        private class Integer
        {
            int Value { get; set; }

            public Integer(int value)
            {
                Value = value;
            }
        }

        [TestMethod]
        public void PairingSimpleTest()
        {
            String s = "QQcoisa";
            Integer i = new Integer(2);
            Pairing<Integer, String> pairing = new Pairing<Integer, String>();
            Tuple<Integer, String> tuple1 = null;
            Tuple<Integer, String> tuple2 = null;
            Thread thread1 = new Thread(() =>
            {
                tuple1 = pairing.Provide(s, Timeout.Infinite);
            });
            Thread thread2 = new Thread(() =>
            {
                tuple2 = pairing.Provide(i, Timeout.Infinite);
            });
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();
            Assert.IsNotNull(tuple1);
            Assert.AreSame(tuple1, tuple2);
        }

        #endregion
    }

}
