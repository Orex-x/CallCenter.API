namespace MyLongPolling.Models;

public class RegistrationModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Surname { get; set; }
    public string Lastname { get; set; }
    
    public int CountCalls { get; set; }
    
    public int CountTransferred{ get; set; }
    
    public int CountBlocked{ get; set; }
}