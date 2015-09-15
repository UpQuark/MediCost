CREATE PROC spUpdateAddressesWithGeocode
	@lat float,
	@lng float,
	@AddressID varchar(255)
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY 
		UPDATE [MediCost].[dbo].[MedicarePhysician2012_Addresses]
		SET lat=@lat,lng=@lng
		WHERE AddressID IS NOT NULL AND AddressID = @AddressID
			/* Removes overwriting. Probably unnecessary */
			AND lat IS NULL
			AND lng IS NULL
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