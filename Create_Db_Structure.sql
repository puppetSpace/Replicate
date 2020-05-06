create table dbo.Folder(
	Id uniqueidentifier NOT NULL,
	[Name] varchar(30) NOT NULL,
	CONSTRAINT PK_Folder PRIMARY KEY(Id),
	CONSTRAINT UQ_Folder_Name UNIQUE([Name])
)
GO
create table dbo.Recipient(
	Id uniqueidentifier NOT NULL,
	[Name] varchar(30) NOT NULL,
	[Address] varchar(max) NOT NULL,
	Verified bit NOT NULL DEFAULT 0
	CONSTRAINT PK_Recipient PRIMARY KEY(Id)
);
GO
create table dbo.FolderRecipient(
	FolderId uniqueidentifier NOT NULL,
	RecipientId uniqueidentifier NOT NULL,
	CONSTRAINT PK_FolderRecipient PRIMARY KEY(FolderId,RecipientId),
	CONSTRAINT FK_FolderRecipient_Folder FOREIGN KEY(FolderId) REFERENCES dbo.Folder(Id) ON DELETE CASCADE,
	CONSTRAINT FK_FolderRecipient_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id) ON DELETE CASCADE
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
	[Status] int NOT NULL DEFAULT 0
	CONSTRAINT PK_File PRIMARY KEY(Id),
	CONSTRAINT FK_File_Folder FOREIGN KEY(FolderId) REFERENCES dbo.Folder(Id),
);
GO

create table dbo.EofMessage(
	Id uniqueidentifier NOT NULL,
	FileId uniqueidentifier NOT NULL,
	AmountOfChunks int NULL,
	CreationTime datetime NOT NULL DEFAULT GETUTCDATE()
	CONSTRAINT PK_EofMessage PRIMARY KEY(Id),
	CONSTRAINT FK_EofMessage_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id),
);
GO

create table dbo.FileChunk(
	Id uniqueidentifier NOT NULL,
	FileId uniqueidentifier NOT NULL,--no foreign key. Could be that chunks are coming in without the file data being present
	SequenceNo int NOT NULL,
	[Value] varbinary(max) NOT NULL,
	CONSTRAINT PK_FileChunk PRIMARY KEY(Id)
);
GO

create table dbo.FailedTransmission(
	Id uniqueidentifier NOT NULL,
	RecipientId uniqueidentifier NOT NULL,
	FileId uniqueIdentifier NULL,
	EofMessageId uniqueidentifier NULL,
	FileChunkId uniqueidentifier NULL,
	CreationTime datetime NOT NULL DEFAULT GETUTCDATE()
	CONSTRAINT PK_FailedTransmission PRIMARY KEY(Id),
	CONSTRAINT FK_FailedTransmission_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id),
	CONSTRAINT FK_FailedTransmission_EofMessage FOREIGN KEY(EofMessageId) REFERENCES dbo.EofMessage(Id),
	CONSTRAINT FK_FailedTransmission_FileChunk FOREIGN KEY(FileChunkId) REFERENCES dbo.FileChunk(Id),
	CONSTRAINT FK_FailedTransmission_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id) ON DELETE CASCADE
);
GO

create table dbo.TransmissionResult(
	Id uniqueidentifier NOT NULL,
	RecipientId uniqueidentifier NOT NULL,
	FileId uniqueIdentifier NOT NULL,
	FileChunkSequenceNo decimal(8,4) NOT NULL,
	CreationTime datetime NOT NULL DEFAULT GETUTCDATE()
	CONSTRAINT PK_TransmissionResult PRIMARY KEY(Id),
	CONSTRAINT FK_TransmissionResult_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id),
	CONSTRAINT FK_TransmissionResult_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id) ON DELETE CASCADE
);
GO


create table dbo.SystemSetting(
	Id uniqueidentifier NOT NULL,
	[Key] varchar(255) NOT NULL,
	[Value] varchar(max) NOT NULL,
	DataType varchar(20) NOT NULL,
	Info varchar(500) NOT NULL,
	CONSTRAINT PK_SystemSetting PRIMARY KEY(Id),
	CONSTRAINT UQ_SystemSetting_Key UNIQUE([Key])
);
GO

insert into dbo.SystemSetting VALUES(NEWID(),'BaseFolder','D:\Temp','text','This is the folder that will contain all the folders you create within the application + the files that need to be synced');
insert into dbo.SystemSetting VALUES(NEWID(),'FolderCrawlTriggerInterval','10','number','The amount of time to wait between each moment the application crawls to the folders to search for new and changed files');
insert into dbo.SystemSetting VALUES(NEWID(),'FileAssemblyTriggerInterval','10','number','The amount of time to wait to between each moment the application checks if all chunks have arrived for a file and assembly of files');
insert into dbo.SystemSetting VALUES(NEWID(),'RetryTriggerInterval','10','number','The amount of time to wait between each moment the application retries sending failed data to the recipients');
insert into dbo.SystemSetting VALUES(NEWID(),'FileSplitSizeOfChunksInBytes','1000000','number','A file will be split up into chunks. This setting defines the size of that chunk');
insert into dbo.SystemSetting VALUES(NEWID(),'ConcurrentFileDisassemblyJobs','10','number', 'Amount of files that ares allowed to be disassembled at the same time');
insert into dbo.SystemSetting VALUES(NEWID(),'ConcurrentFileAssemblyJobs','10','number','Amount of files that ares allowed to be assembled at the same time');

