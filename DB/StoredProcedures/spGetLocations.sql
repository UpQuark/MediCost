CREATE PROC [dbo].[spGetLocations]
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY 
		SELECT * FROM [MediCost].[dbo].[MedicareLocations2012]
	END TRY 
	BEGIN CATCH
		DECLARE @ErrorMessage varchar(1000)
		DECLARE @ErrorNumber int
		DECLARE @ErrorState int
		DECLARE @ErrorSeverity int
	 
		SELECT	@ErrorNumber = ERROR_NUMBER(),
				@ErrorSeverity = ERROR_SEVERITY(),
				@ErrorState = ERROR_STATE()
 
		SET @ErrorMessage = 'Msg ' + convert(varchar(10),@ErrorNumber) + ', Level ' + convert(varchar(10),@ErrorSeverity) + ', ' + ERROR_MESSAGE()
		RAISERROR(@ErrorMessage, 18, 1)	
	END CATCH
END