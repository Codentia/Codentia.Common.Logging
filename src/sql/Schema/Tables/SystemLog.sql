 CREATE TABLE dbo.SystemLog
 (
	 SystemLogId	INT IDENTITY(1,1),
	 LogStamp		DATETIME NOT NULL DEFAULT GETDATE(),
	 LogMessageType	INT NOT NULL,
	 Source			NVARCHAR(200),
	 Message		NVARCHAR(MAX)
)
GO
