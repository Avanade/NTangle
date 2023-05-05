namespace SqlServerDemo.Publisher;

/// <summary>
/// Provides the <b>SqlServerDemo</b> settings.
/// </summary>
public class SqlServerDemoSettings : SettingsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDemoSettings"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public SqlServerDemoSettings(IConfiguration configuration) : base(configuration, "SqlServerDemo") { }

    /// <summary>
    /// Gets the SQL Server database connection string.
    /// </summary>
    public string DatabaseConnectionString => GetRequiredValue<string>("ConnectionStrings__SqlDb");

    /// <summary>
    /// Gets the Azure Service Bus connection string.
    /// </summary>
    public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");
}