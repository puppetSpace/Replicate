create table dbo.Folder(
	Id uniqueidentifier NOT NULL,
	[Name] varchar(30) NOT NULL,
	FolderOptions_DeleteAfterSent bit NOT NULL DEFAULT 0,
	CONSTRAINT PK_Folder PRIMARY KEY(Id),
	CONSTRAINT UQ_Folder_Name UNIQUE([Name])
);
GO
create table dbo.Recipient(
	Id uniqueidentifier NOT NULL,
	[Name] varchar(30) NOT NULL,
	[Address] varchar(max) NOT NULL,
	CONSTRAINT PK_Recipient PRIMARY KEY(Id)
);
GO
create table dbo.FolderRecipient(
	FolderId uniqueidentifier NOT NULL,
	RecipientId uniqueidentifier NOT NULL,
	CONSTRAINT PK_FolderRecipient PRIMARY KEY(FolderId,RecipientId),
	CONSTRAINT FK_FolderRecipient_Folder FOREIGN KEY(FolderId) REFERENCES dbo.Folder(Id),
	CONSTRAINT FK_FolderRecipient_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id)
);
GO

create table dbo.[File](
	Id uniqueidentifier NOT NULL,
	FolderId uniqueidentifier NOT NULL,
	[Name] varchar(255) NOT NULL,
	[Version] int NOT NULL,
	Size bigint NOT NULL,
	[LastModifiedDate] datetime NOT NULL,
	[Path] varchar(max) NOT NULL,
	[Signature] varbinary(max),
	[Source] int NOT NULL,
	CONSTRAINT PK_File PRIMARY KEY(Id),
	CONSTRAINT FK_File_Folder FOREIGN KEY(FolderId) REFERENCES dbo.Folder(Id),
);
GO

create table dbo.EofMessage(
	Id uniqueidentifier NOT NULL,
	FileId uniqueidentifier NOT NULL,
	AmountOfChunks int NULL,
	CONSTRAINT PK_EofMessage PRIMARY KEY(Id),
	CONSTRAINT FK_EofMessage_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id),
);
GO

create table dbo.FileChunk(
	Id uniqueidentifier NOT NULL,
	FileId uniqueidentifier NOT NULL,
	SequenceNo decimal(8,4) NOT NULL,
	[Value] varbinary(max) NOT NULL,
	ChunkSource int NOT NULL,
	CONSTRAINT PK_FileChunk PRIMARY KEY(Id),
	CONSTRAINT FK_FileChunk_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id)
);
GO

create table dbo.FailedTransmission(
	Id uniqueidentifier NOT NULL,
	RecipientId uniqueidentifier NOT NULL,
	FileId uniqueIdentifier NULL,
	EofMessageId uniqueidentifier NULL,
	FileChunkId uniqueidentifier NULL,
	CONSTRAINT PK_FailedTransmission PRIMARY KEY(Id),
	CONSTRAINT FK_FailedTransmission_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id),
	CONSTRAINT FK_FailedTransmission_EofMessage FOREIGN KEY(EofMessageId) REFERENCES dbo.EofMessage(Id),
	CONSTRAINT FK_FailedTransmission_FileChunk FOREIGN KEY(FileChunkId) REFERENCES dbo.FileChunk(Id),
	CONSTRAINT FK_FailedTransmission_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id)
);
GO

create table dbo.TransmissionResult(
	RecipientId uniqueidentifier NOT NULL,
	FileChunkId uniqueIdentifier NOT NULL,
	CONSTRAINT PK_TransmissionResult PRIMARY KEY(RecipientId, FileChunkId),
	CONSTRAINT FK_TransmissionResult_FileChunk FOREIGN KEY(FileChunkId) REFERENCES dbo.FileChunk(Id),
	CONSTRAINT FK_TransmissionResult_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id)
);
GO


create table dbo.SystemSetting(
	Id uniqueidentifier NOT NULL,
	[Key] varchar(255) NOT NULL,
	[Value] varchar(max) NOT NULL,
	CONSTRAINT PK_SystemSetting PRIMARY KEY(Id),
	CONSTRAINT UQ_SystemSetting_Key UNIQUE([Key])
);
GO

insert into dbo.SystemSetting VALUES(NEWID(),'ReplicateBasePath','D:\Temp');
insert into dbo.SystemSetting VALUES(NEWID(),'FolderCrawlTriggerInterval','10');
insert into dbo.SystemSetting VALUES(NEWID(),'RetryTriggerInterval','10');
insert into dbo.SystemSetting VALUES(NEWID(),'FileSplitSizeOfChunksInBytes','1000000');

