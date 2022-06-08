
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyLongPolling.Models;

namespace MyLongPolling.Hubs;

public class ChatHub : Hub
{
    private readonly ApplicationContext _db;
    
    string connection = "Server=localhost;Port=5432;Database=CallCenter;User Id=postgres;Password=123";
   
    
    
    public ChatHub(ApplicationContext context)
    {
        _db = context;
    }

    
    public async Task SendMessage(Message message)
    {
        string _token = Context.GetHttpContext().Request.Headers["Authorization"];
        var user = await _db.Users
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Token.Equals(_token.Substring(7)));
        if (user != null)
            message.Author = user.Name;
        else
            message.Author = "Unknown";
        
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
    

    public async Task SendMeMessage(string message)
    {
        var context = Context.GetHttpContext();
        string _token = context.Request.Headers["Authorization"];
        var user = _db.Users
            .Include(x => x.Connections)
            .FirstOrDefault(x => x.Token.Equals(_token.Substring(7)));
        foreach (var connection in user.Connections)
        {
            await Clients.Client(connection.ConnectionID).SendAsync("ReceiveMeMessage", message);
        }
    }

    public async Task CallPhone(string phone)
    {
        var context = Context.GetHttpContext();
        string _token = context.Request.Headers["Authorization"];
        var user = _db.Users
            .Include(x => x.Connections)
            .FirstOrDefault(x => x.Token.Equals(_token.Substring(7)));
        var tasks = new List<Task>();
        foreach (var connection in user.Connections)
        {
            tasks.Add(Clients.Client(connection.ConnectionID).SendAsync("ReceiveCallPhone", phone));
        }
        await Task.WhenAll(tasks);
    }

    public async Task CallLog(Call call)
    {
        var context = Context.GetHttpContext();
        string _token = context.Request.Headers["Authorization"];
        var user = _db.Users
            .Include(x => x.Connections)
            .FirstOrDefault(x => x.Token.Equals(_token.Substring(7)));

        var client = _db.Clients.FirstOrDefault(x => x.Phone.Equals(call.Number));
        var clients = _db.Clients.ToList();
        if (client != null)
            call.Name = client.Name;
        
        var tasks = new List<Task>();
        foreach (var connection in user.Connections)
        {
            tasks.Add(Clients.Client(connection.ConnectionID).SendAsync("ReceiveCallLog", call));
        }
        await Task.WhenAll(tasks);
    }


    public async Task GetNewConnection(User user)
    {
        var tasks = new List<Task>();
        foreach (var userConnection in user.Connections)
        {
            tasks.Add(
                Clients.Client(Context.ConnectionId)
                    .SendAsync("ReceiveConnected", userConnection));
        }
        
        foreach (var userConnection in user.Connections)
        {
            tasks.Add(
                Clients.Client(userConnection.ConnectionID)
                    .SendAsync("ReceiveConnected", Context.ConnectionId));
        }
        
        await Task.WhenAll(tasks);
    }

    
    public override Task OnConnectedAsync()
    {
       
        var contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(connection)
            .Options;
        using (ApplicationContext db = new ApplicationContext(contextOptions))
        {
            var context = Context.GetHttpContext();
            string _token = context.Request.Headers["Authorization"];
            var user = db.Users
                .Include(x => x.Connections)
                .FirstOrDefault(x => x.Token.Equals(_token.Substring(7)));
        
        
            SendMessage(new Message()
            {
                Title = "вошел в чат", Created = DateTime.Now
            });
        
      
            foreach (var userConnection in user.Connections)
            {
                Clients.Client(userConnection.ConnectionID)
                    .SendAsync("ReceiveConnected", Context.ConnectionId);
            }
            
            user.Connections.Add(new Connection()
            {
                ConnectionID = Context.ConnectionId
            });
            db.Users.Update(user);
            db.SaveChanges();
            
            foreach (var userConnection in user.Connections)
            {
                Clients.Client(Context.ConnectionId)
                    .SendAsync("ReceiveConnected", userConnection.ConnectionID);
            }


        }
       
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(this.connection)
            .Options;
        using (ApplicationContext db = new ApplicationContext(contextOptions))
        {
            var coId = Context.ConnectionId;
            var connection = db.Connections.FirstOrDefault(x 
                => x.ConnectionID.Equals(coId));
            db.Connections.Remove(connection);
            db.SaveChanges();
            
            var context = Context.GetHttpContext();
            string _token = context.Request.Headers["Authorization"];
            var user = db.Users
                .Include(x => x.Connections)
                .FirstOrDefault(x => x.Token.Equals(_token.Substring(7)));
            
            foreach (var userConnection in user.Connections)
            {
                Clients.Client(userConnection.ConnectionID)
                    .SendAsync("ReceiveDisconnected", connection);
            }
            
        }

       
        
        
        return base.OnDisconnectedAsync(exception);
    }
}
