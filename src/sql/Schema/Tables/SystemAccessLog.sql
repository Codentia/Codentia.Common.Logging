 CREATE TABLE dbo.SystemAccessLog
 (
	SystemAccessLogId		INT IDENTITY(1,1),
	LogStamp				DATETIME,
	Languages				NVARCHAR(MAX),
	HostAddress				NVARCHAR(15),
	RequestUrl				NVARCHAR(MAX),
	ReferralUrl				NVARCHAR(MAX),
	Browser					NVARCHAR(100),
	BrowserMajorVersion		INT,
	BrowserMinorVersion		NVARCHAR(10)
)
GO

	