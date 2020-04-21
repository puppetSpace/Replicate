using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System;

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

			File file = File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "FileFolder", "test1.txt")), System.Guid.Empty, @"D:\Test", ReadOnlyMemory<byte>.Empty);
			var filePath = pathBuilder.BuildPath(file.Path);

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
			var folderPath = pathBuilder.BuildPath(folder.Name);

			Assert.AreEqual(System.IO.Path.Combine(rootPath, folder.Name), folderPath);
		}

		[TestMethod]
		public void BuildPath_NullFolder_BuiltPathIsBasePath()
		{
			var rootPath = System.IO.Directory.GetCurrentDirectory();
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);
			var folderPath = pathBuilder.BuildPath(null);

			Assert.AreEqual(pathBuilder.BasePath, folderPath);
		}

		[TestMethod]
		public void BuildPath_File_BuiltPathIsBasePath()
		{
			var rootPath = System.IO.Directory.GetCurrentDirectory();
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			var filePath = pathBuilder.BuildPath(null);

			Assert.AreEqual(pathBuilder.BasePath, filePath);
		}

		[TestMethod]
		public void BuildPath_File_PathIsBuild()
		{
			var rootPath = System.IO.Directory.GetCurrentDirectory();
			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x["ReplicateBasePath"]).Returns(rootPath);
			var pathBuilder = new PathBuilder(configurationMock.Object);

			File file = File.BuildPartial(new System.IO.FileInfo(System.IO.Path.Combine(rootPath, "FileFolder", "test1.txt")), System.Guid.Empty, pathBuilder.BasePath, ReadOnlyMemory<byte>.Empty);
			var filePath = pathBuilder.BuildPath(file.Path);

			Assert.AreEqual(System.IO.Path.Combine(rootPath, file.Path), filePath);
		}
	}
}
