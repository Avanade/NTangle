{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
{{#each CdcEnabledTables}}
  {{#unless @first}}


  {{/unless}}
-- Enable CDC for table: [{{Schema}}].[{{Table}}]
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'{{Schema}}.{{Table}}')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'{{Schema}}',  
    @source_name = N'{{Table}}',  
    @role_name = NULL,
    @supports_net_changes = 0
END

GO{{/each}}