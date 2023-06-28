namespace ContactSync.NewApp.Subscriber.Subscribers.Entities;

public class Contact : IIdentifier<int>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool NoCallList { get; set; }
    public Address? Address { get; set; }
}

public class Address
{
    public string? Street1 { get; set; }
    public string? Street2 { get; set; }
}
