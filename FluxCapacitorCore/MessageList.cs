using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.API.Client;
using Discord.Commands;

namespace FluxCapacitorCore
{
    class MessageList
    {
        public Discord.Server server;
        List<Discord.Message> list;
        int maxMessages;

        public MessageList(Discord.Server id, Discord.Message firstMessage)
        {
            server = id;
            list = new List<Discord.Message>() { firstMessage };
            maxMessages = 1000;
        }

        public MessageList(Discord.Server id, Discord.Message firstMessage, int maxmess)
        {
            server = id;
            list = new List<Discord.Message>() { firstMessage };
            maxMessages = maxmess;
        }

        public void add(Discord.Message message)
        {
            list.Add(message);
            if(list.Count > maxMessages)
            {
                purge();
            }
        }

        public void wipe(string user, int num)
        {
            int count = 0;
            for(int i = list.Count - 1; i >= 0; i--)
            {
                if(list.ElementAt(i).User.Name == user)
                {
                    list.ElementAt(i).Delete();
                    count++;
                    if(count == num)
                    {
                        break;
                    }
                }
            }
        }

          public void purge()
        {
            list.RemoveRange(0, list.Count - maxMessages);
        }
    }
}