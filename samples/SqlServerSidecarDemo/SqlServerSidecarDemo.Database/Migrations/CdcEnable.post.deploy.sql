-- Enable CDC for table: [Legacy].[Posts]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Posts')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'Posts',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO

-- Enable CDC for table: [Legacy].[Comments]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Comments')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'Comments',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO

-- Enable CDC for table: [Legacy].[Tags]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Tags')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'Tags',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO

-- Enable CDC for table: [Legacy].[Contact]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Contact')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'Contact',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO

-- Enable CDC for table: [Legacy].[Address]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Address')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'Address',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO

-- Enable CDC for table: [Legacy].[Cust]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Cust')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'Cust',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO