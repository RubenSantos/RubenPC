using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serie1;
using System.Threading;

namespace Serie1Tests
{
    [TestClass]
    public class ExpirableLazyTests
    {
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
        public void CheckSimpleUse()
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
    }

}
