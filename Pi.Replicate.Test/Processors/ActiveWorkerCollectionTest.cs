using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Processors
{
    [TestClass]
    public class ActiveWorkerCollectionTest
    {
        [TestMethod]
        public async Task Add_NewWorker_WorkerShouldBeDeletedAFterTaskCompletes()
        {
            var collection = new ActiveWorkerCollection();

            var moq = new Mock<Worker>();
            moq.Setup(x => x.WorkAsync()).Returns(async () =>
            {
                await Task.Delay(100);
            });

            var activeWorker = new ActiveWorker(moq.Object);
            collection.Add(activeWorker);

            Assert.AreEqual(1, collection.Count);

            await activeWorker.Task;

            Assert.AreEqual(0, collection.Count);
        }
    }
}
