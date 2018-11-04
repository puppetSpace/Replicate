using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Processing.Notification;
using Pi.Replicate.Queueing;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Queueing
{
    [TestClass]
    public class QueueTest
    {
        [TestMethod]
        public async Task Enqueue_TwoItems_CountShouldBeTwo()
        {
            var moq = new Mock<IWorkEventAggregator>();
            moq.Setup(x => x.Publish(It.IsAny<WorkEventData>()));

            var factory = new WorkItemQueueFactory(moq.Object);
            var queue = factory.GetQueue<File>(Processing.QueueKind.Incoming);

            await queue.Enqueue(new File());
            await queue.Enqueue(new File());

            Assert.AreEqual(2, queue.Count);

        }

        [TestMethod]
        public async Task Dequeue_TwoItemsAdded_TwoDequeued_CountShouldBeZero()
        {
            var moq = new Mock<IWorkEventAggregator>();
            moq.Setup(x => x.Publish(It.IsAny<WorkEventData>()));

            var factory = new WorkItemQueueFactory(moq.Object);
            var queue = factory.GetQueue<File>(Processing.QueueKind.Incoming);

            await queue.Enqueue(new File());
            await queue.Enqueue(new File());

            Assert.AreEqual(2, queue.Count);

            await queue.Dequeue();
            await queue.Dequeue();

            Assert.AreEqual(0, queue.Count);

        }
    }
}
