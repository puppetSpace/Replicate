use ReplicateDb;
GO
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
	CONSTRAINT FK_File_Folder FOREIGN KEY(FolderId) REFERENCES dbo.Folder(Id) on DELETE CASCADE,
);
GO
EXEC sys.sp_addextendedproperty @name=N'Comment', @value=N'0=Local, 1=Remote' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'File', @level2type=N'COLUMN',@level2name=N'Source'
GO

EXEC sys.sp_addextendedproperty @name=N'Comment', @value=N'0=Normal,
1=Failed,
2=Assembled,
3=Received' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'File', @level2type=N'COLUMN',@level2name=N'Status'
GO

create table dbo.EofMessage(
	Id uniqueidentifier NOT NULL,
	FileId uniqueidentifier NOT NULL,
	AmountOfChunks int NULL,
	CreationTime datetime NOT NULL DEFAULT GETUTCDATE()
	CONSTRAINT PK_EofMessage PRIMARY KEY(Id),
	--CONSTRAINT FK_EofMessage_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id) ON DELETE CASCADE, --no foreign key. Could be that eofmessage comes in without fileinfo
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
	CONSTRAINT FK_FailedTransmission_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id) ON DELETE CASCADE,
	CONSTRAINT FK_FailedTransmission_EofMessage FOREIGN KEY(EofMessageId) REFERENCES dbo.EofMessage(Id),
	CONSTRAINT FK_FailedTransmission_FileChunk FOREIGN KEY(FileChunkId) REFERENCES dbo.FileChunk(Id),
	CONSTRAINT FK_FailedTransmission_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id) ON DELETE CASCADE
);
GO

create table dbo.TransmissionResult(
	Id uniqueidentifier NOT NULL,
	RecipientId uniqueidentifier NOT NULL,
	FileId uniqueIdentifier NOT NULL,
	FileChunkSequenceNo int NOT NULL,
	[Source] int NOT NULL,
	CreationTime datetime NOT NULL DEFAULT GETUTCDATE()
	CONSTRAINT PK_TransmissionResult PRIMARY KEY(Id),
	-- CONSTRAINT FK_TransmissionResult_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id) ON DELETE CASCADE, --Filechunk can come in without file details. these will be added later when retry happens
	CONSTRAINT FK_TransmissionResult_Recipient FOREIGN KEY(RecipientId) REFERENCES dbo.Recipient(Id) ON DELETE CASCADE
);
GO
EXEC sys.sp_addextendedproperty @name=N'Comment', @value=N'0=Local, 1=Remote' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TransmissionResult', @level2type=N'COLUMN',@level2name=N'Source'
GO


create table dbo.SystemSetting(
	Id uniqueidentifier NOT NULL,
	[Key] varchar(255) NOT NULL,
	[Value] varchar(max) NOT NULL,
	DataType varchar(20) NOT NULL,
	[Description] varchar(500) NOT NULL,
	CONSTRAINT PK_SystemSetting PRIMARY KEY(Id),
	CONSTRAINT UQ_SystemSetting_Key UNIQUE([Key])
);
GO

create table dbo.WebhookType(
	Id uniqueidentifier NOT NULL,
	[Name] varchar(50) NOT NULL,
	[Description] varchar(max) NULL,
	MessageStructure varchar(max) NULL
	CONSTRAINT PK_WebhookType PRIMARY KEY(Id)
)
GO

create table dbo.FolderWebhook(
	Id uniqueidentifier NOT NULL,
	FolderId uniqueidentifier NOT NULL,
	WebhookTypeId uniqueidentifier NOT NULL,
	CallbackUrl varchar(500) NOT NULL,
	CONSTRAINT PK_FolderWebhook PRIMARY KEY(Id),
	CONSTRAINT FK_FolderWebhook_Folder FOREIGN KEY(FolderId) REFERENCES dbo.Folder(Id) ON DELETE CASCADE,
	CONSTRAINT FK_FolderWebhook_WebhookType FOREIGN KEY(WebhookTypeId) REFERENCES dbo.WebhookType(Id)
)
GO

create table dbo.FileConflict(
	Id uniqueidentifier NOT NULL,
	FileId uniqueidentifier NOT NULL,
	[Type] smallint NOT NULL,
	CONSTRAINT PK_FileConflict PRIMARY KEY(Id),
	CONSTRAINT FK_FileConflict_File FOREIGN KEY(FileId) REFERENCES dbo.[File](Id)
)
GO

--insert into dbo.SystemSetting VALUES(NEWID(),'BaseFolder','D:\Temp','text','This is the folder that will contain all the folders you create within the application + the files that need to be synced');
insert into dbo.SystemSetting VALUES(NEWID(),'TriggerIntervalFolderCrawl','10','number','The amount of time to wait between each moment the application crawls to the folders to search for new and changed files');
insert into dbo.SystemSetting VALUES(NEWID(),'TriggerIntervalFileAssembly','10','number','The amount of time to wait to between each moment the application checks if all chunks have arrived for a file and assembly of files');
insert into dbo.SystemSetting VALUES(NEWID(),'TriggerIntervalRetry','10','number','The amount of time to wait between each moment the application retries sending failed data to the recipients');
insert into dbo.SystemSetting VALUES(NEWID(),'SizeOfChunksInBytes','1000000','number','A file will be split up into chunks. This setting defines the size of that chunk');
insert into dbo.SystemSetting VALUES(NEWID(),'ConcurrentFileDisassemblyJobs','10','number', 'Amount of files that ares allowed to be disassembled at the same time');
insert into dbo.SystemSetting VALUES(NEWID(),'ConcurrentFileAssemblyJobs','10','number','Amount of files that ares allowed to be assembled at the same time');
GO

insert into dbo.WebhookType VALUES(NEWID(),'FileDisassembled','When a file is completely processed and ready for transmission','{
    "name":"myfile.ext",
    "path":"d:\\examples\\Folder1\\myfile.ext",
	"version":"1",
    "folder":"Folder1"
}');
insert into dbo.WebhookType VALUES(NEWID(),'FileAssembled','When a file is completely received and assembled','{
    "name":"myfile.ext",
    "path":"d:\\examples\\Folder1\\myfile.ext",
	"version":"1",
    "folder":"Folder1"
}');
insert into dbo.WebhookType VALUES(NEWID(),'FileFailed','When processing of a file fails','{
    "name":"myfile.ext",
    "path":"d:\\examples\\Folder1\\myfile.ext",
	"version":"1",
    "folder":"Folder1"
}');

GO

create view dbo.V_AmountOfFilesSentByRecipient
as
select a.RecipientId,a.FolderId, count(a.FileId) AmountOfFilesSent
from(
	select distinct re.Id RecipientId, fre.FolderId,fi.Id FileId
	,sum(trt.FileChunkSequenceNo) over (partition by re.id,fi.Id)  FileTransmisionChunkSequenceNoSum
	, (em.AmountOfChunks*(em.AmountOfChunks + 1)) / 2 ChunksChecksum
	from dbo.Recipient re
	inner join dbo.FolderRecipient fre on fre.RecipientId = re.Id
	left join dbo.[File] fi on fi.FolderId = fre.FolderId and fi.Source = 0
	left join dbo.TransmissionResult trt on trt.FileId = fi.Id and trt.RecipientId = re.Id and trt.Source = 0
	left join dbo.EofMessage em on em.FileId = fi.Id
	where re.Verified = 1) a
where a.FileTransmisionChunkSequenceNoSum = a.ChunksChecksum
group by a.RecipientId,a.FolderId;

GO

create view dbo.V_AmountOfFilesReceivedByRecipient
as
select a.RecipientId,a.FolderId, count(a.FileId) AmountOfFilesReceived
from(
	select distinct re.Id RecipientId, fre.FolderId,fi.Id FileId
	,sum(trt.FileChunkSequenceNo) over (partition by re.id,fi.Id)  FileTransmisionChunkSequenceNoSum
	, (em.AmountOfChunks*(em.AmountOfChunks + 1)) / 2 ChunksChecksum
	from dbo.Recipient re
	inner join dbo.FolderRecipient fre on fre.RecipientId = re.Id
	left join dbo.[File] fi on fi.FolderId = fre.FolderId and fi.Source = 1
	left join dbo.TransmissionResult trt on trt.FileId = fi.Id and trt.RecipientId = re.Id and trt.Source = 1
	left join dbo.EofMessage em on em.FileId = fi.Id
	where re.Verified = 1) a
where a.FileTransmisionChunkSequenceNoSum = a.ChunksChecksum
group by a.RecipientId,a.FolderId;

GO

