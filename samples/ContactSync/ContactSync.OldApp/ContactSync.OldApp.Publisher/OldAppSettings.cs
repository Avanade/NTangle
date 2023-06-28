namespace ContactSync.OldApp.Publisher;

/// <summary>
/// Provides the <b>ContactSync.OldApp</b> settings.
/// </summary>
public class OldAppSettings : SettingsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OldAppSettings"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public OldAppSettings(IConfiguration configuration) : base(configuration, "ContactSync.OldApp") { }

    /// <summary>
    /// Gets the SQL Server database connection string.
    /// </summary>
    public string DatabaseConnectionString => GetRequiredValue<string>("ConnectionStrings__SqlDb");

    /// <summary>
    /// Gets the Azure Service Bus connection string.
    /// </summary>
    public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");
}