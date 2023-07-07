namespace AppName.Publisher;

/// <summary>
/// Provides the <b>AppName</b> settings.
/// </summary>
public class DomainNameSettings : SettingsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainNameSettings"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public DomainNameSettings(IConfiguration configuration) : base(configuration, "DomainName") { }

    /// <summary>
    /// Gets the SQL Server database connection string.
    /// </summary>
    public string DatabaseConnectionString => GetRequiredValue<string>("ConnectionStrings__SqlDb");

    /// <summary>
    /// Gets the Azure Service Bus connection string.
    /// </summary>
    public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");
}