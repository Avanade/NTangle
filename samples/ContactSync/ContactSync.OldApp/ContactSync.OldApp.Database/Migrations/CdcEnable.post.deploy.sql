-- Enable CDC for table: [old].[contact]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'old.contact')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'old',  
    @source_name = N'contact',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO

-- Enable CDC for table: [old].[contact_address]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'old.contact_address')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'old',  
    @source_name = N'contact_address',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO