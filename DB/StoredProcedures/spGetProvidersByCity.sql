DROP PROCEDURE [dbo].spGetProvidersByCity
GO

CREATE PROC [dbo].spGetProvidersByCity
	@city varChar(255),
	@specialty varChar(255)
AS
BEGIN
	SET NOCOUNT ON;
		BEGIN TRY 
			SELECT	
				   map.[npi]
				  ,addres.AddressID
				  ,addres.[nppes_provider_street1]
				  ,addres.[nppes_provider_street2]
				  ,addres.[nppes_provider_city]
				  ,addres.[nppes_provider_zip]
				  ,addres.[nppes_provider_state]
				  ,addres.[nppes_provider_country]				
				  ,addres.[lat]
				  ,addres.[lng]				  			  
				  ,provider.[nppes_provider_last_org_name]
				  ,provider.[nppes_provider_first_name]
				  ,provider.[nppes_provider_mi]
				  ,provider.[nppes_credentials]
				  ,provider.[nppes_provider_gender]
				  ,provider.[nppes_entity_code]
				  ,provider.[provider_type]
			  FROM [MediCost].[dbo].[MedicarePhysician2012] map
			  LEFT JOIN  [MediCost].[dbo].[MedicarePhysician2012_Addresses] addres
			  			on addres.[AddressID]= map.[AddressID] 
			  LEFT JOIN  [dbo].[MedicarePhysician2012_Provider]	 provider
				on provider.npi=map.npi 
			WHERE 	addres.nppes_provider_city = @city	
					and addres.lat is not null
					and	provider.provider_type = @specialty
					
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