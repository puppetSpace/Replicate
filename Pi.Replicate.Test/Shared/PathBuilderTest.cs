using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Domain;
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
		public void BasePath_IsCorrectlySet()
		{
			var rootPath = @"D:\Test";
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			Assert.AreEqual(rootPath, pathBuilder.BasePath);
		}

		[TestMethod]
		public void BuildPath_Folder_PathIsBuild()
		{
			var rootPath = @"D:\Test";
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			var folder = new Folder() { Name = "FolderTest" };
			var folderPath = pathBuilder.BuildPath(folder);

			Assert.AreEqual(System.IO.Path.Combine(rootPath,folder.Name), folderPath);
		}

		[TestMethod]
		public void BuildPath_NullFolder_BuiltPathIsBasePath()
		{
			var rootPath = @"D:\Test";
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);
			Folder f = null;
			var folderPath = pathBuilder.BuildPath(f);

			Assert.AreEqual(pathBuilder.BasePath, folderPath);
		}

		[TestMethod]
		public void BuildPath_File_BuiltPathIsBasePath()
		{
			var rootPath = @"D:\Test";
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			File file = null;
			var filePath = pathBuilder.BuildPath(file);

			Assert.AreEqual(pathBuilder.BasePath, filePath);
		}
	}
}
