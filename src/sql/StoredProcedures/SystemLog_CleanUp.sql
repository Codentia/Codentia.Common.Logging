CREATE PROCEDURE dbo.SystemLog_CleanUp
		@LogMessageType INT,
		@KeepFromDate	DATETIME
AS
BEGIN
	SET NOCOUNT ON
	
	DELETE
	FROM	dbo.SystemLog
	WHERE	LogMessageType = @LogMessageType
			AND LogStamp < @KeepFromDate
END
GO
