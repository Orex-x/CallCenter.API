using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLongPolling.Models;

namespace MyLongPolling.Controllers;


[Route("api/[controller]")]
public class DataController : Controller
{
    private readonly ApplicationContext _db;
    public DataController(ApplicationContext context)
    {
        _db = context;
    }
    
    [Route("CreateClient")] 
    public async Task<bool> CreateClient([FromBody] Client client)
    {
        if (client != null)
        {
            try
            {
                _db.Clients.Add(client);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
            
            }
        }
        return false;
    }
    
    [Route("DeleteClient")] 
    public async Task<bool> DeleteClient(int id)
    {
        if (ModelState.IsValid)
        {
            var client = await _db.Clients
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (client != null)
            {
                _db.Clients.Remove(client);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }
    
    [Route("UpdateClient")] 
    public async Task<bool> UpdateClient([FromBody] Client newClient)
    {
        if (newClient != null)
        {
            var oldClient = await _db.Clients
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x => x.Id == newClient.Id);

            if (oldClient != null)
            {
                oldClient.Name = newClient.Name;
                oldClient.Email = newClient.Email;
                oldClient.Phone = newClient.Phone;
                oldClient.City = newClient.City;
                oldClient.Status = newClient.Status;
                oldClient.Description = newClient.Description;
                oldClient.DateOfBirth = newClient.DateOfBirth;

                _db.Clients.Update(oldClient);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }

    
    [Route("AddEvent")] 
    public async Task<bool> AddEvent(int idClient, [FromBody] Event _event)
    {
        var client = await _db.Clients
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.Id == idClient);
        if (client != null)
        {            
            client.Events.Add(@_event);
            _db.Clients.Update(client);
            await _db.SaveChangesAsync();
            return true;    
        }
        return false;
    }
    
    
    [Route("DeleteEvent")] 
    public async Task<bool> DeleteEvent(int idClient, int idEvent)
    {
        
        var client = await _db.Clients
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.Id == idClient);
        if (client != null)
        {
            var @event = await _db.Events.FirstOrDefaultAsync(x => x.Id == idEvent);
            client.Events.Remove(@event);
            _db.Clients.Update(client);
            await _db.SaveChangesAsync();
            return true;    
        }
        return false;
    }
    
    [Route("GetAllClients")] 
    public async Task<List<Client>> GetAllClients()
    {
        var clients = await _db.Clients
            .Include(x => x.Events)
            .ToListAsync();
        return clients;
    }

    
    [Route("UpdateUser")] 
    public async Task<Boolean> UpdateUser([FromBody] User user)
    {
        try
        {
            var oldUser = _db.Users.FirstOrDefault(x => x.Id == user.Id);
            oldUser.Name = user.Name;
            oldUser.Surname = user.Surname;
            oldUser.Lastname = user.Lastname;
            oldUser.Login = user.Login;
            oldUser.CountBlocked = user.CountBlocked;
            oldUser.CountCalls = user.CountCalls;
            oldUser.CountTransferred = user.CountTransferred;
            _db.Users.Update(oldUser);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            
        }

        return false;
    }
    
    [Route("UpdateUserCountFields")] 
    public async Task<Boolean> UpdateUserCountFields([FromBody] User user)
    {
        try
        {
            var oldUser = _db.Users.FirstOrDefault(x => x.Id == user.Id);
            oldUser.CountBlocked = user.CountBlocked;
            oldUser.CountCalls = user.CountCalls;
            oldUser.CountTransferred = user.CountTransferred;
            _db.Users.Update(oldUser);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            
        }

        return false;
    }

}