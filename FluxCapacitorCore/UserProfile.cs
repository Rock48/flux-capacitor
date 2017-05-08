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
using FluxCapacitorCore;

namespace FluxCapacitorCore
{
    class UserProfile
    {
        public Discord.User user;
        public double pressure;
        public Discord.Message lastMessage;

        public UserProfile(Discord.User u, double p, Discord.Message m)
        {
            user = u;
            pressure = p;
            lastMessage = m;
        }

        public void setMessage(Discord.Message message)
        {
            Debug.WriteLine("Set last message for user " + user.Name + " to " + message.Text);
            lastMessage = message;
        }
    }
}
