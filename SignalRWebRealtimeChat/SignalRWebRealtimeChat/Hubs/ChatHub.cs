using Microsoft.AspNetCore.SignalR;
using SignalRWebRealtimeChat.Data;
using SignalRWebRealtimeChat.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRWebRealtimeChat.Hubs
{
    public class ChatHub : Hub
    {
        public async Task GetNickName(string nickName) 
        {
            Client client = new Client
            {
                ConnectionId = Context.ConnectionId,
                NickName = nickName,
            };   
            
            ClientSource.Clients.Add(client);
            await Clients.Others.SendAsync("clientJoined",nickName);
            await Clients.All.SendAsync("clients",ClientSource.Clients);
        }

        public async Task SendMessageAsync(string message , string clientName) 
        {
            clientName = clientName.Trim();
            Client senderclient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);           
            if (clientName == "Tümü")
            {
                await Clients.Others.SendAsync("receiveMessage", message, senderclient.NickName);
            }
            else 
            {
                Client client = ClientSource.Clients.FirstOrDefault(c => c.NickName == clientName);
                await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message, senderclient.NickName);            
            }
          
        
        
        }

        public async Task AddGroup(string groupName) 
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            

            Group group = new Group { GroupName = groupName };
            group.Clients.Add(ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId));

            GroupSource.Groups.Add(group);

            await Clients.All.SendAsync("groups", GroupSource.Groups);            
        
        } 


        public async Task AddClientGroup(IEnumerable<string> groupNames) 
        {
            Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            foreach (var group in groupNames) 
            {
                Group _group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == group);

                var result = _group.Clients.Any(c => c.ConnectionId == Context.ConnectionId) ;
                if (!result)
                {
                    _group.Clients.Add(client);
                    await Groups.AddToGroupAsync(Context.ConnectionId, group);

                }
            }
        }


        public async Task GetClientToGroup(string groupName) 
        { 
            if(groupName == "-1") 
            {
                await Clients.Caller.SendAsync("clients", 
                    ClientSource.Clients);
            }


            Group group = GroupSource.Groups.FirstOrDefault(group => group.GroupName == groupName);

            await Clients.Caller.SendAsync("clients",groupName == "-1" ? ClientSource.Clients : group.Clients);        
        }

        public async Task SendMessageToGroupAsync(string groupName, string message) 
        {
            await Clients.Group(groupName).SendAsync("receiveMessage",
                message, ClientSource.Clients.FirstOrDefault(c =>
                c.ConnectionId == Context.ConnectionId).NickName);
        
        
        
        }




    }
}
