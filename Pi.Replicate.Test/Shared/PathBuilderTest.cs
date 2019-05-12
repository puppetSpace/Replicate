using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pi.Replicate.Shared.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Shared
{
	[TestClass]
    public class PathBuilderTest
    {
        
		[TestMethod]
		public void Build_Only_RootFolder()
		{
			var rootPath = @"D:\Test";
			PathBuilder.Initialize(rootPath);
			var path = PathBuilder.Build();

			Assert.AreEqual(rootPath, path);
		}
    }
}
