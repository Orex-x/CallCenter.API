namespace MyLongPolling.Models;

public enum Status
{
    NEW,
    NO_ANSWER,
    NO_DATE,
    CONFIRMED,
    TRANSFERRED,
    BLOCKED
}

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string City { get; set; }
    public Status Status { get; set; }
    public string Description { get; set; }
    public DateTime DateOfBirth { get; set; }
    public ICollection<Event> Events { get; set; }
}