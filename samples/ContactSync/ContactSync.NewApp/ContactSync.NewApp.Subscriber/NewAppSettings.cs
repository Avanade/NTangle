namespace ContactSync.NewApp.Subscriber;

/// <summary>
/// Provides the <b>ContactSync.OldApp</b> settings.
/// </summary>
public class NewAppSettings : SettingsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NewAppSettings"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public NewAppSettings(IConfiguration configuration) : base(configuration, "ContactSync.OldApp") { }

    /// <summary>
    /// Gets the SQL Server database connection string.
    /// </summary>
    public string DatabaseConnectionString => GetRequiredValue<string>("ConnectionStrings__SqlDb");
}