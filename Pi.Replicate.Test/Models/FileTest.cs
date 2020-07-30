using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Models
{
	[TestClass]
    public class FileTest
    {

		[TestInitialize]
		public void InitializeTest()
		{
			PathBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
		}

		[TestMethod]
		public void Update_ShouldIncreaseVersion()
		{
			var fileModel = Helper.GetFileModel("FileFolder", "test1.txt");

			fileModel.Update(new System.IO.FileInfo(fileModel.GetFullPath()));

			Assert.AreEqual(2,fileModel.Version);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Update_OtherFile_ShouldThrowException()
		{
			var fileModel = Helper.GetFileModel("FileFolder", "test1.txt");
			var otherModel = Helper.GetFileModel("FileFolder", "test2.txt");

			fileModel.Update(new System.IO.FileInfo(otherModel.GetFullPath()));
		}

		[TestMethod]
		[ExpectedException(typeof(System.IO.FileNotFoundException))]
		public void Update_NonExistingFile_ShouldThrowException()
		{
			var fileModel = Helper.GetFileModel("FileFolder", "test1.txt");
			var nonExistingFile = new System.IO.FileInfo("doesNotExists.txt");

			fileModel.Update(new System.IO.FileInfo(nonExistingFile.FullName));
		}

		[TestMethod]
        public async Task CompressTo_ShouldCompressFile()
		{
			var compressedFilePath = System.IO.Path.Combine(PathBuilder.BasePath, "DropLocation", "test1.txt");
			var fileModel = Helper.GetFileModel("FileFolder", "test1.txt");

			await fileModel.CompressTo(compressedFilePath);
			var compressedModelFile = new System.IO.FileInfo(compressedFilePath);

			Assert.IsTrue(compressedModelFile.Exists);
			Assert.IsTrue(fileModel.Size > compressedModelFile.Length);
		}

		[TestMethod]
		public async Task CompressTo_ShouldDecompressFile()
		{
			var originalFilePath = System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1.txt");
			var fileModel = Helper.GetFileModel("DropLocation", "readme.txt"); //read_me.txt is an empty file

			await fileModel.DecompressFrom(System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1_compressed.txt"));

			Assert.AreEqual(System.IO.File.ReadAllText(originalFilePath), System.IO.File.ReadAllText(fileModel.GetFullPath()));
		}

		[TestMethod]
		public void CreateSignature_ShouldCreateCorrectSignature()
		{
			var signatureStringForTest1File = "T0NUT1NJRwEEU0hBMQdBZGxlcjMyPj4+AAhfipQW/xz4tz64/0ottTotenbeWg5gqmEACBmh1dCBj3o/ZrY6w86y9FkrvdgGwQ/OFQAIYo3cp+ve1p89nOOddNymOeFrT1jYMdytAAiMhZOmqvj+QLYguL1iJ6E1hUGk33FUWN0ACFues71qYNnVpZF7TFiFDpyn4pka0U91OwAIrJIpW6VmzEkSNlDh3xHDfvRKx9Vkf5pZAAizgzy+2D+dIW0TXYPwuLB/M4yru3Vd3R4ACC+et/uHSiv9K8bgEA5TAoj2zkEc2hTWQQAIapL/CKG44BhnTnKgIRDXlFAwXO/LHcPTAAh7igYFsEPBOBQoMTG4gpQ3nC/yv4eIydsACMqVFg/8AsqS1bBi32UG/0xmKP5FYzyd8AAI+5iS9vxoun00H5kEsmCQNI70Upp5bTTrwwFNiqoR7sZ9x68cXTDjWaQNPhcbNQbtJQo=";
			var fileModel = Helper.GetFileModel("FileFolder", "test1.txt");

			var signature = fileModel.CreateSignature();
			var converted = Convert.ToBase64String(signature);

			Assert.AreEqual(signatureStringForTest1File, converted);

		}

		[TestMethod]
		public void CreateDelta_ShouldCreateCorrectDelta()
		{
			var signatureStringForTest1File = "T0NUT1NJRwEEU0hBMQdBZGxlcjMyPj4+AAhfipQW/xz4tz64/0ottTotenbeWg5gqmEACBmh1dCBj3o/ZrY6w86y9FkrvdgGwQ/OFQAIYo3cp+ve1p89nOOddNymOeFrT1jYMdytAAiMhZOmqvj+QLYguL1iJ6E1hUGk33FUWN0ACFues71qYNnVpZF7TFiFDpyn4pka0U91OwAIrJIpW6VmzEkSNlDh3xHDfvRKx9Vkf5pZAAizgzy+2D+dIW0TXYPwuLB/M4yru3Vd3R4ACC+et/uHSiv9K8bgEA5TAoj2zkEc2hTWQQAIapL/CKG44BhnTnKgIRDXlFAwXO/LHcPTAAh7igYFsEPBOBQoMTG4gpQ3nC/yv4eIydsACMqVFg/8AsqS1bBi32UG/0xmKP5FYzyd8AAI+5iS9vxoun00H5kEsmCQNI70Upp5bTTrwwFNiqoR7sZ9x68cXTDjWaQNPhcbNQbtJQo=";
			var deltaStringChangedTest1File = "T0NUT0RFTFRBAQRTSEExFAAAANac9rTgKmLzjc4OB0r15ZY/G5LXPj4+YAAAAAAAAAAAw2EAAAAAAACAOQAAAAAAAAANClRoaXMgaXMgc29tZSBhZGRlZCB0ZXh0IHRvIG1ha2UgYSBjaGFuZ2UgdG8gdGhpcyBmaWxlDQo=";
			var fileModel = Helper.GetFileModel("FileFolder", "test1_changed.txt");

			var delta = fileModel.CreateDelta(Convert.FromBase64String(signatureStringForTest1File));
			var converted = Convert.ToBase64String(delta.ToArray());

			Assert.AreEqual(deltaStringChangedTest1File, converted);
		}


		[TestMethod]
		public void ApplyDelta_ShouldApplyChange()
		{
			var originalFilePath = System.IO.Path.Combine(PathBuilder.BasePath, "FileFolder", "test1_changed.txt");
			var deltaStringChangedTest1File = "T0NUT0RFTFRBAQRTSEExFAAAANac9rTgKmLzjc4OB0r15ZY/G5LXPj4+YAAAAAAAAAAAw2EAAAAAAACAOQAAAAAAAAANClRoaXMgaXMgc29tZSBhZGRlZCB0ZXh0IHRvIG1ha2UgYSBjaGFuZ2UgdG8gdGhpcyBmaWxlDQo=";
			var fileModel = Helper.GetFileModel("FileFolder", "test1_copy.txt");

			fileModel.ApplyDelta(Convert.FromBase64String(deltaStringChangedTest1File));

			Assert.AreEqual(System.IO.File.ReadAllText(originalFilePath), System.IO.File.ReadAllText(fileModel.GetFullPath()));
		}



	}
}
