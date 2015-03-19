CREATE PROCEDURE dbo.SystemAccessLog_CleanUp
		@KeepFromDate	DATETIME
AS
BEGIN
	SET NOCOUNT ON
	
	DELETE
	FROM	dbo.SystemAccessLog
	WHERE	LogStamp < @KeepFromDate
END
GO
 