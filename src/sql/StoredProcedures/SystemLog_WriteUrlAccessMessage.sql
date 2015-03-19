CREATE PROCEDURE dbo.SystemLog_WriteUrlAccessMessage
	@LogStamp				DATETIME,
	@Languages				NVARCHAR(MAX),
	@HostAddress			NVARCHAR(15),
	@RequestUrl				NVARCHAR(MAX),
	@ReferralUrl			NVARCHAR(MAX),
	@Browser				NVARCHAR(100),
	@BrowserMajorVersion	INT,
	@BrowserMinorVersion	NVARCHAR(10)
AS
BEGIN
	SET NOCOUNT ON
	
	INSERT INTO dbo.SystemAccessLog
	(
		LogStamp,
		Languages,
		HostAddress,
		RequestUrl,
		ReferralUrl,
		Browser,
		BrowserMajorVersion,
		BrowserMinorVersion
	)
	VALUES
	(
		@LogStamp,
		@Languages,
		@HostAddress,
		@RequestUrl,
		@ReferralUrl,
		@Browser,
		@BrowserMajorVersion,
		@BrowserMinorVersion
	)
END
GO
 