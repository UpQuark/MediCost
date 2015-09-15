DROP PROCEDURE [dbo].spGetOfficesByCity
GO

CREATE PROCEDURE [dbo].spGetOfficesByCity
	@city varChar(255)
AS
	BEGIN
		SET NOCOUNT ON;
		BEGIN TRY 
			SELECT DISTINCT ([AddressID]),
				[nppes_provider_street1]
				,[nppes_provider_street2]
				,[nppes_provider_city]
				,[nppes_provider_zip]
				,[nppes_provider_state]
				,[nppes_provider_country]				
				,[lat]
				,[lng]
			FROM [MediCost].[dbo].[MedicarePhysician2012_Addresses]			
			WHERE nppes_provider_city = @city
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