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
	Size bigint NOT NULL,
	AmountOfChunk int NULL,
	[Hash] varbinary(max) NULL,
	[Status] int NOT NULL,
	[LastModifiedDate] timestamp NOT NULL,
	[Path] varchar(max) NOT NULL,
	[Signature] varbinary(max),
	CONSTRAINT PK_File PRIMARY KEY(Id),
	CONSTRAINT FK_File_Folder FOREIGN KEY(FolderId) REFERENCES dbo.Folder(Id),
);
GO

create table dbo.FailedFile(
	FileId uniqueidentifier NOT NULL,
	RecipientId uniqueidentifier NOT NULL,
	CONSTRAINT PK_FailedFile PRIMARY KEY(FileId,RecipientId),
	CONSTRAINT FK_FailedFile_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id),
	CONSTRAINT FK_FailedFile_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id)
);
GO

create table dbo.FileChunk(
	Id uniqueidentifier NOT NULL,
	FileId uniqueidentifier NOT NULL,
	SequenceNo decimal NOT NULL,
	[Value] varbinary(max) NOT NULL,
	ChunkSource int NOT NULL,
	CONSTRAINT PK_FileChunk PRIMARY KEY(Id),
	CONSTRAINT FK_FileChunk_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id)
);
GO

create table dbo.ChunkPackage(
	FileChunkId uniqueidentifier NOT NULL,
	RecipientId uniqueidentifier NOT NULL,
	CONSTRAINT PK_ChunkPackage PRIMARY KEY(FileChunkId,RecipientId),
	CONSTRAINT FK_ChunkPackage_File FOREIGN KEY(FileChunkId) REFERENCES dbo.FileChunk(Id),
	CONSTRAINT FK_ChunkPackage_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id)
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

insert into dbo.SystemSetting VALUES(NEWID(),'ReplicateBasePath','C:\Temp');
insert into dbo.SystemSetting VALUES(NEWID(),'FolderCrawlTriggerInterval','10');
insert into dbo.SystemSetting VALUES(NEWID(),'RetryTriggerInterval','10');
insert into dbo.SystemSetting VALUES(NEWID(),'FileSplitSizeOfChunksInBytes','1000000');

