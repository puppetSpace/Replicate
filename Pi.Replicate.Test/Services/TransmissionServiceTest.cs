using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Test.Services
{
	[TestClass]
    public class TransmissionServiceTest
    {
        
		[TestMethod]
		public async Task SendFile_ShouldCallTransmissionLinkSendFile()
		{
			Recipient transmittedRecipient = null;
			Folder transmittedFolder = null;
			File transmittedFile = null;
			var transmissionLinkMock = new Mock<ITransmissionLink>();
			transmissionLinkMock.Setup(x => x.SendFile(It.IsAny<Recipient>(), It.IsAny<Folder>(), It.IsAny<File>()))
				.Callback<Recipient, Folder, File>((x, y, z) =>
				{
					transmittedRecipient = x;
					transmittedFolder = y;
					transmittedFile = z;

				});

			var transmissionRepositoryMock = new Mock<ITransmissionRepository>();
			transmissionRepositoryMock.Setup(x => x.AddFailedFileTransmission(It.IsAny<Guid>(), It.IsAny<Guid>()));

			var recipient = Helper.GetRecipientModel("testRecipient");
			var folder = Helper.GetFolderModel("testFolder");
			var file = Helper.GetFileModel();
			var transmissionService = new TransmissionService(transmissionLinkMock.Object, transmissionRepositoryMock.Object);
			await transmissionService.SendFile(recipient, folder,file);

			Assert.AreSame(recipient, transmittedRecipient);
			Assert.AreSame(folder, transmittedFolder);
			Assert.AreSame(file, transmittedFile);
		}

		[TestMethod]
		public async Task SendFile_Exception_ShouldAddToFailedTransmission()
		{
			var transmissionLinkMock = new Mock<ITransmissionLink>();
			transmissionLinkMock.Setup(x => x.SendFile(It.IsAny<Recipient>(), It.IsAny<Folder>(), It.IsAny<File>()))
				.Callback<Recipient, Folder, File>((x, y, z) =>
				{
					throw new InvalidOperationException();
				});

			Guid transmittedFileId = Guid.Empty;
			Guid transmittedRecipientId = Guid.Empty;
			var transmissionRepositoryMock = new Mock<ITransmissionRepository>();
			transmissionRepositoryMock.Setup(x => x.AddFailedFileTransmission(It.IsAny<Guid>(), It.IsAny<Guid>()))
				.Callback<Guid,Guid>((x,y)=> {
					transmittedFileId = x;
					transmittedRecipientId = y;
				})
				.ReturnsAsync(()=>Result.Success());

			var recipient = Helper.GetRecipientModel("testRecipient");
			var folder = Helper.GetFolderModel("testFolder");
			var file = Helper.GetFileModel();
			var transmissionService = new TransmissionService(transmissionLinkMock.Object, transmissionRepositoryMock.Object);
			var canContinue = await transmissionService.SendFile(recipient, folder, file);

			Assert.AreEqual(recipient.Id, transmittedRecipientId);
			Assert.AreEqual(file.Id,transmittedFileId);
			Assert.IsTrue(canContinue);
		}

		[TestMethod]
		public async Task SendFile_ExceptionDuringAddFailedTransmission_ShouldNotContinue()
		{
			var transmissionLinkMock = new Mock<ITransmissionLink>();
			transmissionLinkMock.Setup(x => x.SendFile(It.IsAny<Recipient>(), It.IsAny<Folder>(), It.IsAny<File>()))
				.Callback<Recipient, Folder, File>((x, y, z) =>
				{
					throw new InvalidOperationException();
				});

			var transmissionRepositoryMock = new Mock<ITransmissionRepository>();
			transmissionRepositoryMock.Setup(x => x.AddFailedFileTransmission(It.IsAny<Guid>(), It.IsAny<Guid>()))
				.ReturnsAsync(() => Result.Failure());

			var recipient = Helper.GetRecipientModel("testRecipient");
			var folder = Helper.GetFolderModel("testFolder");
			var file = Helper.GetFileModel();
			var transmissionService = new TransmissionService(transmissionLinkMock.Object, transmissionRepositoryMock.Object);
			var canContinue = await transmissionService.SendFile(recipient, folder, file);

			Assert.IsFalse(canContinue);
		}


		[TestMethod]
		public async Task SendEofMessage_ShouldCallTransmissionLinkSendEofMessage()
		{
			Recipient transmittedRecipient = null;
			EofMessage transmittedEofMessage = null;
			var transmissionLinkMock = new Mock<ITransmissionLink>();
			transmissionLinkMock.Setup(x => x.SendEofMessage(It.IsAny<Recipient>(), It.IsAny<EofMessage>()))
				.Callback<Recipient, EofMessage>((x, y) =>
				{
					transmittedRecipient = x;
					transmittedEofMessage = y;

				});

			var transmissionRepositoryMock = new Mock<ITransmissionRepository>();
			transmissionRepositoryMock.Setup(x => x.AddFailedFileTransmission(It.IsAny<Guid>(), It.IsAny<Guid>()));

			var recipient = Helper.GetRecipientModel("testRecipient");
			var eofMesage = Helper.GetEofMessageModel();
			var transmissionService = new TransmissionService(transmissionLinkMock.Object, transmissionRepositoryMock.Object);
			await transmissionService.SendEofMessage(recipient, eofMesage);

			Assert.AreSame(recipient, transmittedRecipient);
			Assert.AreSame(eofMesage, transmittedEofMessage);
		}

		[TestMethod]
		public async Task SendEofMessage_Exception_ShouldAddToFailedTransmission()
		{
			var transmissionLinkMock = new Mock<ITransmissionLink>();
			transmissionLinkMock.Setup(x => x.SendEofMessage(It.IsAny<Recipient>(), It.IsAny<EofMessage>()))
				.Callback<Recipient, EofMessage>((x, y) =>
				{
					throw new InvalidOperationException();

				});

			Guid transmittedEofMessageId = Guid.Empty;
			Guid transmittedRecipientId = Guid.Empty;
			var transmissionRepositoryMock = new Mock<ITransmissionRepository>();
			transmissionRepositoryMock.Setup(x => x.AddFailedEofMessageTransmission(It.IsAny<Guid>(), It.IsAny<Guid>()))
				.Callback<Guid, Guid>((x, y) => {
					transmittedEofMessageId = x;
					transmittedRecipientId = y;
				})
				.ReturnsAsync(() => Result.Success());

			var recipient = Helper.GetRecipientModel("testRecipient");
			var eofMesage = Helper.GetEofMessageModel();
			var transmissionService = new TransmissionService(transmissionLinkMock.Object, transmissionRepositoryMock.Object);
			var canContinue = await transmissionService.SendEofMessage(recipient, eofMesage);

			Assert.AreEqual(recipient.Id, transmittedRecipientId);
			Assert.AreEqual(eofMesage.Id, transmittedEofMessageId);
			Assert.IsTrue(canContinue);
		}

		[TestMethod]
		public async Task SendEofMessage_ExceptionDuringAddFailedTransmission_ShouldNotContinue()
		{
			var transmissionLinkMock = new Mock<ITransmissionLink>();
			transmissionLinkMock.Setup(x => x.SendEofMessage(It.IsAny<Recipient>(), It.IsAny<EofMessage>()))
				.Callback<Recipient, EofMessage>((x, y) =>
				{
					throw new InvalidOperationException();

				});

			var transmissionRepositoryMock = new Mock<ITransmissionRepository>();
			transmissionRepositoryMock.Setup(x => x.AddFailedEofMessageTransmission(It.IsAny<Guid>(), It.IsAny<Guid>()))
				.ReturnsAsync(() => Result.Failure());

			var recipient = Helper.GetRecipientModel("testRecipient");
			var eofMesage = Helper.GetEofMessageModel();
			var transmissionService = new TransmissionService(transmissionLinkMock.Object, transmissionRepositoryMock.Object);
			var canContinue = await transmissionService.SendEofMessage(recipient, eofMesage);

			Assert.IsFalse(canContinue);
		}
	}
}
