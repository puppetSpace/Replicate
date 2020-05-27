using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pi.Replicate.WebUi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Common
{
	[TestClass]
    public class RingListTest
    {
		[TestMethod]
        public void AddFirst_MoreThenLimit_CountShouldBeLimit()
		{
			var limit = 100;
			var ringList = new RingList<int>(limit);
			for(var i = 0; i < 150; i++)
			{
				ringList.AddFirst(i);
			}

			Assert.AreEqual(limit, ringList.Count);
		}
    }
}
