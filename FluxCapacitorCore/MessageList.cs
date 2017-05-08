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
    class MessageList
    {
        public Discord.Server server;
        List<Discord.Message> list;
        List<UserProfile> userProfiles;
        int maxMessages;
 
        public MessageList(Discord.Server id, Discord.Message firstMessage)
        {
            server = id;
            list = new List<Discord.Message>() { firstMessage };
            userProfiles = new List<UserProfile>() { };
            maxMessages = 1000;
        }

        private void incrementPressure(UserProfile up, Discord.Message currentMessage)
        {
            Debug.WriteLine("Last message - " + up.lastMessage.Text);
            Debug.WriteLine("Current message - " + currentMessage.Text);
            double timeSince = TimeSpan.FromTicks(currentMessage.Timestamp.Ticks - up.lastMessage.Timestamp.Ticks).TotalSeconds;
            double factor = 1;
            if (currentMessage.Text.Equals(up.lastMessage.Text, StringComparison.OrdinalIgnoreCase))
            {
                factor *= 1.3;
            }
            if (currentMessage.Embeds.Length > 0)
            {
                factor *= 1.5;
                Debug.WriteLine("Message had embed.");
            }
            else if (currentMessage.Attachments.Length > 0)
            {
                factor *= 1.5;
                Debug.WriteLine("Message has attatchment.");
            }
            if (currentMessage.IsTTS)
            {
                factor *= 1.5;
            }
            if (currentMessage.Text.Length > 200)
            {
                factor *= 1.5;
            }
            up.pressure -= (getSpamCD(server.Id) * timeSince);
            if(factor < getSpamCD(server.Id)/2)
            {
                factor = getSpamCD(server.Id) / 2;
            }
            up.pressure += factor;
            if(up.pressure < 0)
            {
                up.pressure = 0;
            }
            up.setMessage(currentMessage);
            Debug.WriteLine("new pressure for " + up.user.Name + ": " + up.pressure);
            if (up.pressure > getSpamLimit(server.Id))
            {
                server.Ban(up.user, 1);
                try
                {
                    server.FindChannels(getLogChannel(server.Id)).FirstOrDefault().SendMessage("Banned user **" + up.user.Name + "** for spam. Last message sent: " + up.lastMessage.Text);
                }
                catch (NullReferenceException)
                {
                    server.Owner.SendMessage("Banned user **" + up.user.Name + "** for spam. Last message sent: " + up.lastMessage.Text + "\nIf you want me to log these to your channel instead of DM, set a log channel with .setlogchannel {channel name}!");
                }
            }
        }



        public double getSpamCD(ulong server)
        {
            var lines = File.ReadAllLines(server + ".txt");

            if (Array.IndexOf(lines, "spamfalloff") == -1)
            {
                return -1;
            }
            return Convert.ToDouble(lines[Array.IndexOf(lines, "spamfalloff") + 1]);
        }

        public double getSpamLimit(ulong server)
        {
            var lines = File.ReadAllLines(server + ".txt");

            if (Array.IndexOf(lines, "spamtolerance") == -1)
            {
                return -1;
            }
            return Convert.ToDouble(lines[Array.IndexOf(lines, "spamtolerance") + 1]);
        }

        public String getLogChannel(ulong server)
        {
            var lines = File.ReadAllLines(server + ".txt");

            if (Array.IndexOf(lines, "logchannel") == -1)
            {
                return "";
            }
            return lines[Array.IndexOf(lines, "logchannel") + 1];
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
            int index = findUserProfile(message.User);
            if(index == -1)
            {
                userProfiles.Add(new UserProfile(message.User, 1, message));
                index = userProfiles.Count - 1;
                Debug.WriteLine("added user profile for " + message.User.Name + ". Initialized with message " + userProfiles.ElementAt(index).lastMessage.Text);
            } else
            {
                incrementPressure(userProfiles.ElementAt(index), message);
            }
            if (list.Count > maxMessages)
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

        private int findUserProfile(Discord.User user)
        {
            for(int i = 0; i < userProfiles.Count; i++)
            {
                if(userProfiles.ElementAt(i).user.Equals(user))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}