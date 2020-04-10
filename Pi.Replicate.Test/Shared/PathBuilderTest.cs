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
			var rootPath = System.IO.Directory.GetCurrentDirectory();
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			Assert.AreEqual(rootPath, pathBuilder.BasePath);
		}

		[TestMethod]
		public void BuildPath_BaseIsEmpty_PathShouldBeRelative()
		{
			var rootPath = @"";
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			File file = File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),"FileFolder","test1.txt")),null, @"D:\Test");
			var filePath = pathBuilder.BuildPath(file);

			Assert.AreEqual(file.Path, filePath);
		}

		[TestMethod]
		public void BuildPath_Folder_PathIsBuild()
		{
			var rootPath = System.IO.Directory.GetCurrentDirectory();
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
			var rootPath = System.IO.Directory.GetCurrentDirectory();
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
			var rootPath = System.IO.Directory.GetCurrentDirectory();
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			File file = null;
			var filePath = pathBuilder.BuildPath(file);

			Assert.AreEqual(pathBuilder.BasePath, filePath);
		}

		[TestMethod]
		public void BuildPath_File_PathIsBuild()
		{
			var rootPath = System.IO.Directory.GetCurrentDirectory();
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			File file = File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(rootPath, "FileFolder", "test1.txt")), null, pathBuilder.BasePath);
			var filePath = pathBuilder.BuildPath(file);

			Assert.AreEqual(System.IO.Path.Combine(rootPath, file.Path), filePath);
		}
	}
}
