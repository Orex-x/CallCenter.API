
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyLongPolling.Models;

namespace MyLongPolling.Hubs;

public class ChatHub : Hub
{
    private ApplicationContext db;
    public ChatHub(ApplicationContext context)
    {
        db = context;
    }

    
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    

    public async Task SendMeMessage(string message)
    {
        var context = Context.GetHttpContext();
        string _token = context.Request.Headers["Authorization"];
        var user = db.Users
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
        var user = db.Users
            .Include(x => x.Connections)
            .FirstOrDefault(x => x.Token.Equals(_token.Substring(7)));
        var tasks = new List<Task>();
        foreach (var connection in user.Connections)
        {
            tasks.Add(Clients.Client(connection.ConnectionID).SendAsync("ReceiveCallPhone", phone));
        }
        Task.WhenAll(tasks);
    }

    
    public override Task OnConnectedAsync()
    {
        
        var context = Context.GetHttpContext();
        string _token = context.Request.Headers["Authorization"];
        var user = db.Users
            .Include(x => x.Connections)
            .FirstOrDefault(x => x.Token.Equals(_token.Substring(7)));
        SendMessage("id вошел в чат: ", user.Name);

       
      
        
        user.Connections.Add(new Connection()
        {
            ConnectionID = Context.ConnectionId
        });
        db.Users.Update(user);
        db.SaveChanges();
        
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var coId = Context.ConnectionId;
        var connection = db.Connections.FirstOrDefault(x 
            => x.ConnectionID.Equals(coId));
        db.Connections.Remove(connection);
        db.SaveChanges();
        return base.OnDisconnectedAsync(exception);
    }
}
