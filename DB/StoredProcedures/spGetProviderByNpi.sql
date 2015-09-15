DROP PROCEDURE [dbo].spGetProviderByNpi
GO

CREATE PROCEDURE [dbo].spGetProviderByNpi
	@npi varChar(255)
AS
	BEGIN
		SET NOCOUNT ON;
		BEGIN TRY 
			SELECT [npi]
				  ,[nppes_provider_last_org_name]
				  ,[nppes_provider_first_name]
				  ,[nppes_provider_mi]
				  ,[nppes_credentials]
				  ,[nppes_provider_gender]
				  ,[nppes_entity_code]
				  ,[provider_type]
			FROM [MediCost].[dbo].[MedicarePhysician2012_Provider]
			where npi = @npi
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