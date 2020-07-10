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
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
			Assert.AreEqual(rootPath, PathBuilder.BasePath);
		}

		[TestMethod]
		public void BuildPath_BaseIsEmpty_PathShouldBeRelative()
		{
			PathBuilder.SetBasePath("");
			File file = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "FileFolder", "test1.txt")), System.Guid.Empty, @"D:\Test");
			var filePath = PathBuilder.BuildPath(file.Path);

			Assert.AreEqual(file.Path, filePath);
		}

		[TestMethod]
		public void BuildPath_Folder_PathIsBuild()
		{
			var rootPath = System.IO.Directory.GetCurrentDirectory();
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());

			var folder = new Folder() { Name = "FolderTest" };
			var folderPath = PathBuilder.BuildPath(folder.Name);

			Assert.AreEqual(System.IO.Path.Combine(rootPath, folder.Name), folderPath);
		}

		[TestMethod]
		public void BuildPath_NullFolder_BuiltPathIsBasePath()
		{
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());

			var folderPath = PathBuilder.BuildPath(null);

			Assert.AreEqual(PathBuilder.BasePath, folderPath);
		}

		[TestMethod]
		public void BuildPath_File_BuiltPathIsBasePath()
		{
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());

			var filePath = PathBuilder.BuildPath(null);

			Assert.AreEqual(PathBuilder.BasePath, filePath);
		}

		[TestMethod]
		public void BuildPath_File_PathIsBuild()
		{
			var rootPath = System.IO.Directory.GetCurrentDirectory();
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());

			File file = File.Build(new System.IO.FileInfo(System.IO.Path.Combine(rootPath, "FileFolder", "test1.txt")), System.Guid.Empty, PathBuilder.BasePath);
			var filePath = PathBuilder.BuildPath(file.Path);

			Assert.AreEqual(System.IO.Path.Combine(rootPath, file.Path), filePath);
		}
	}
}
