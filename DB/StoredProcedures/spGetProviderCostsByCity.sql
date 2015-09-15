DROP PROCEDURE [dbo].spGetProviderCostsByCity
GO

CREATE PROC [dbo].spGetProviderCostsByCity
	@city varChar(255),
	@specialty varChar(255)
AS
	BEGIN
		SET NOCOUNT ON;
		BEGIN TRY 
			SELECT	
				   map.[npi]
				  ,map.[AddressID]
				  ,map.[medicare_participation_indicator]
				  ,map.[place_of_service]
				  ,map.[hcpcs_code]
				  ,map.[line_srvc_cnt]
				  ,map.[bene_unique_cnt]
				  ,map.[bene_day_srvc_cnt]
				  ,map.[average_Medicare_allowed_amt]
				  ,map.[stdev_Medicare_allowed_amt]
				  ,map.[average_submitted_chrg_amt]
				  ,map.[stdev_submitted_chrg_amt]
				  ,map.[average_Medicare_payment_amt]
				  ,map.[stdev_Medicare_payment_amt]
				  ,addres.[nppes_provider_street1]
				  ,addres.[nppes_provider_street2]
				  ,addres.[nppes_provider_city]
				  ,addres.[nppes_provider_zip]
				  ,addres.[nppes_provider_state]
				  ,addres.[nppes_provider_country]				
				  ,addres.[lat]
				  ,addres.[lng]
				  ,code.[hcpcs_code]
				  ,code.[hcpcs_description]				  
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
			  LEFT JOIN  [dbo].[MedicarePhysician2012_Code] code
						on code.[hcpcs_code]= map.[hcpcs_code]
			  LEFT JOIN  [dbo].[MedicarePhysician2012_Provider]	 provider
						on provider.npi=map.npi			
			 WHERE 	addres.nppes_provider_city = @city	
					and addres.lat is not null
					and	provider.provider_type = @specialty
			ORDER BY average_submitted_chrg_amt DESC
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