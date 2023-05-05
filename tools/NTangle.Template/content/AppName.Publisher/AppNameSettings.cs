namespace AppName.Publisher;

/// <summary>
/// Provides the <b>AppName</b> settings.
/// </summary>
public class AppNameSettings : SettingsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameSettings"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public AppNameSettings(IConfiguration configuration) : base(configuration, "AppName") { }

    /// <summary>
    /// Gets the SQL Server database connection string.
    /// </summary>
    public string DatabaseConnectionString => GetRequiredValue<string>("ConnectionStrings__SqlDb");

    /// <summary>
    /// Gets the Azure Service Bus connection string.
    /// </summary>
    public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");
}