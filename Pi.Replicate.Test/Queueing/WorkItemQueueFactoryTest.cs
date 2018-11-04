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
    public class WorkItemQueueFactoryTest
    {
        
        [TestMethod]
        public void Get_IncomingQueueKind_NewQueueCreated()
        {
            var moq = new Mock<IWorkEventAggregator>();
            moq.Setup(x => x.Subscribe(It.IsAny<IWorkSubscriber>()));

            var factory = new WorkItemQueueFactory(moq.Object);
            var queue = factory.GetQueue<File>(Processing.QueueKind.Incoming);

            Assert.IsNotNull(queue);
        }

        [TestMethod]
        public void Get_IncomingQueueKind_SecondCallGetsSameQueue()
        {
            var moq = new Mock<IWorkEventAggregator>();
            moq.Setup(x => x.Subscribe(It.IsAny<IWorkSubscriber>()));

            var factory = new WorkItemQueueFactory(moq.Object);
            var queue1 = factory.GetQueue<File>(Processing.QueueKind.Incoming);
            var queue2 = factory.GetQueue<File>(Processing.QueueKind.Incoming);

            Assert.AreSame(queue1,queue2);
        }

        [TestMethod]
        public void Get_DifferentQueueKind_TwoQueuesAreDifferent()
        {
            var moq = new Mock<IWorkEventAggregator>();
            moq.Setup(x => x.Subscribe(It.IsAny<IWorkSubscriber>()));

            var factory = new WorkItemQueueFactory(moq.Object);
            var queue1 = factory.GetQueue<File>(Processing.QueueKind.Incoming);
            var queue2 = factory.GetQueue<File>(Processing.QueueKind.Outgoing);

            Assert.AreNotSame(queue1, queue2);
        }

    }
}
