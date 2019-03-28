using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            //todo add WorkEventAggregator test
            var mock = new Moq.Mock<IWorkEventAggregator>();
            var factory = new WorkItemQueueFactory(mock.Object);
            var queue = factory.GetQueue<File>(Processing.QueueKind.Incoming);

            Assert.IsNotNull(queue);
        }

        [TestMethod]
        public void Get_IncomingQueueKind_SecondCallGetsSameQueue()
        {
            //todo add WorkEventAggregator test
            var mock = new Moq.Mock<IWorkEventAggregator>();
            var factory = new WorkItemQueueFactory(mock.Object);
            var queue1 = factory.GetQueue<File>(Processing.QueueKind.Incoming);
            var queue2 = factory.GetQueue<File>(Processing.QueueKind.Incoming);

            Assert.AreSame(queue1,queue2);
        }

        [TestMethod]
        public void Get_DifferentQueueKind_TwoQueuesAreDifferent()
        {
            //todo add WorkEventAggregator test
            var mock = new Moq.Mock<IWorkEventAggregator>();
            var factory = new WorkItemQueueFactory(mock.Object);
            var queue1 = factory.GetQueue<File>(Processing.QueueKind.Incoming);
            var queue2 = factory.GetQueue<File>(Processing.QueueKind.Outgoing);

            Assert.AreNotSame(queue1, queue2);
        }

    }
}
