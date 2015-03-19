CREATE PROCEDURE dbo.SystemLog_WriteSingleMessage
	@LogStamp			DATETIME,
	@LogMessageType		INT,
	@Source				NVARCHAR(200),
	@Message			NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON
	
	INSERT INTO dbo.SystemLog
	(
		LogStamp,
		LogMessageType,
		Source,
		Message
	)
	VALUES
	(
		@LogStamp,
		@LogMessageType,
		@Source,
		@Message
	)
END
GO
