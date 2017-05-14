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
    class Bot
    {
        DiscordClient discord;
        CommandService commands;
        DateTime lastRestart;
        long restartCD = 3000000000; //5 minutes in nanoseconds
        List<MessageList> messageLists = new List<MessageList>() { };

        String[] defaultConfig = { "Tolerance", "2", "", "sar: 0", "", "aliases", "", "logchannel", "", "spamtolerance", "3", "spamfalloff", "1.5"};

        public Bot()
        {
            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;

            });

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '.';
                x.AllowMentionPrefix = true;
            });

            commands = discord.GetService<CommandService>();

            discord.MessageReceived += (s, e) =>
            {
                if (e.Message.User.IsBot) //can't ban itself for spam
                {
                    return;
                }
                bool added = false;
                for(int i = 0; i < messageLists.Count; i++)
                {
                    if(messageLists.ElementAt(i).server == e.Message.Server)
                    {
                        messageLists.ElementAt(i).add(e.Message);
                        added = true;
                        Debug.WriteLine("added message to the " + messageLists.ElementAt(i).server + " messagelist.");
                    }
                }

                if (!added)
                {
                    Debug.WriteLine("added message list for server " + e.Server.Name);
                    messageLists.Add(new MessageList(e.Message.Server, e.Message));
                }
            };

            initialize();

            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect(File.ReadAllText("token"), TokenType.Bot);
                lastRestart = DateTime.Now;
            });


        }

        private void registerPurgeCmd()
        {
            commands.CreateCommand("purge")
                .Parameter("n", ParameterType.Required)
                .Parameter("channel", ParameterType.Optional)
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Do(async (e) =>
                {
                    int.TryParse(e.GetArg("n"), out int n);
                    if (n > 100)
                    {
                        await e.Channel.SendMessage("Uh... I can only delete up to a hundred messages at a time. Sorry...");
                        return;
                    }
                    else if (e.GetArg("channel") == "")
                    {
                        Discord.Message[] messagesToDelete = await e.Channel.DownloadMessages(n);
                        await e.Channel.DeleteMessages(messagesToDelete);
                    }
                    else
                    {
                        Discord.Channel channel = e.Server.FindChannels(e.GetArg("channel")).FirstOrDefault();
                        Debug.WriteLine(channel.Name);
                        Discord.Message[] messagesToDelete = await channel.DownloadMessages(n);
                        await channel.DeleteMessages(messagesToDelete);
                    }
                    await e.Channel.SendMessage("Got 'em.");
                });
        }

        private void registerBestPonyCmd()
        {
            commands.CreateCommand("bestpony")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("Braeburn for sure");
                });
        }

        private void registerSelfieCmd()
        {
            commands.CreateCommand("selfie")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("https://derpicdn.net/img/view/2016/5/10/1150951.png");
                });
        }

        private void registerDebugCmd()
        {
            commands.CreateCommand("debug")
                .Do((e) =>
                {
                    Debug.WriteLine("Nothing here!");
                });
        }

        private void registerAsarCmd()
        {
            commands.CreateCommand("asar")
                .Parameter("role1", ParameterType.Required)
                .Parameter("role2", ParameterType.Optional)
                .Parameter("role3", ParameterType.Optional)
                .Parameter("role4", ParameterType.Optional)
                .Parameter("role5", ParameterType.Optional)
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Do(async (e) =>
                {
                    if (!File.Exists(e.Server.Id.ToString() + ".txt"))
                    {
                        await e.Channel.SendMessage("You've gotta .initialize me first, because I'm not sure what that is...");
                        return;
                    }

                    String newrole = "";
                    //format arguments as such
                    for (int i = 1; i <= e.Args.Length; i++)
                    {
                        if (e.GetArg("role" + i.ToString()) != "" && i > 1)
                        {
                            newrole += " "; //add spaces so names don't suck, but not in front of the first one because that'd be bad
                        }
                        newrole += e.GetArg("role" + i.ToString()); //append additional strings
                    }

                    IEnumerable<Discord.Role> x = e.Server.Roles;
                    bool t = false;
                    for (int i = 0; i < x.Count(); i++)
                    {
                        if (x.ElementAt(i).Name.Equals(newrole))
                        {
                            t = true;
                            break;
                        }
                    }

                    if (!t)
                    {
                        await e.Channel.SendMessage("That doesn't look like a role...");
                        return;
                    }

                    var lines = File.ReadAllLines(e.Server.Id.ToString() + ".txt");

                    if (Array.IndexOf(lines, newrole) != -1)
                    {
                        await e.Channel.SendMessage("That's already a self-assignable role.");
                        return;
                    }

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Length > 2 && lines[i].Substring(0, 3).Equals("sar"))
                        {
                            Debug.WriteLine(lines[i].Substring(5) + "test");
                            int roleslength = Convert.ToInt32(lines[i].Substring(5)); //how many roles there already are
                            String[] temp = new String[lines.Length + 1]; //add one to length to accomodate new role
                            String[] temp1 = new String[i + roleslength]; //first part of file - everything up through the currently existing roles
                            String[] temp2 = { newrole }; //second part to insert in middle - new role
                            String[] temp3 = new String[lines.Length - i - roleslength]; //third part of file - everything previously after the role list
                            Split(lines, i + roleslength + 1, out temp1, out temp3);
                            temp1[i] = "sar: " + (roleslength + 1).ToString();
                            temp1.CopyTo(temp, 0);
                            temp2.CopyTo(temp, i + roleslength + 1);
                            temp3.CopyTo(temp, i + roleslength + 2);
                            File.WriteAllLines(e.Server.Id.ToString() + ".txt", temp);
                            await e.Channel.SendMessage("Added " + newrole + " as a self-assignable role!");
                            return;
                        }
                    }
                    discord.Log.Log(LogSeverity.Error, "Runtime error", "WARNING: .asar failed execution.");
                });
        }

        private void registerIamCommand()
        {
            commands.CreateCommand("iam")
                .Parameter("role1", ParameterType.Required)
                .Parameter("role2", ParameterType.Optional)
                .Parameter("role3", ParameterType.Optional)
                .Parameter("role4", ParameterType.Optional)
                .Parameter("role5", ParameterType.Optional)
                .Do(async (e) =>
                {
                    String newrole = "";
                    //format arguments as such
                    for (int i = 1; i <= e.Args.Length; i++)
                    {
                        if (e.GetArg("role" + i.ToString()) != "" && i > 1)
                        {
                            newrole += " "; //add spaces so names don't suck, but not in front of the first one because that'd be bad
                        }
                        newrole += e.GetArg("role" + i.ToString()); //append additional strings
                    }

                    int index = filteredCheck(e.Server.Roles, newrole, e.Server.Id);
                    /* IEnumerable<Discord.Role> x = e.Server.Roles; || Old method
                    bool t = false;
                    for (int i = 0; i < x.Count(); i++)
                    {
                        if (x.ElementAt(i).Name.Equals(newrole, StringComparison.OrdinalIgnoreCase))
                        {
                            index = i;
                            t = true;
                            break;
                        }
                    } */

                    if (index == -1)
                    {
                        await e.Channel.SendMessage("That doesn't look like a role...");
                        return;
                    }

                    var lines = File.ReadAllLines(e.Server.Id.ToString() + ".txt");
                    newrole = e.Server.Roles.ElementAt(index).Name;

                    if (Array.IndexOf(lines, newrole) != -1)
                    {
                        await e.User.AddRoles(e.Server.Roles.ElementAt(index));
                        await e.Channel.SendMessage("You now have the **" + newrole + "** role.");
                        return;
                    }

                    await e.Channel.SendMessage("I can't give you that role. Sorry...");
                });
        }

        private void registerIamnCommand()
        {
            commands.CreateCommand("iamn")
                .Parameter("role1", ParameterType.Required)
                .Parameter("role2", ParameterType.Optional)
                .Parameter("role3", ParameterType.Optional)
                .Parameter("role4", ParameterType.Optional)
                .Parameter("role5", ParameterType.Optional)
                .Do(async (e) =>
                {
                    String newrole = "";
                    //format arguments as such
                    for (int i = 1; i <= e.Args.Length; i++)
                    {
                        if (e.GetArg("role" + i.ToString()) != "" && i > 1)
                        {
                            newrole += " "; //add spaces so names don't suck, but not in front of the first one because that'd be bad
                        }
                        newrole += e.GetArg("role" + i.ToString()); //append additional strings
                    }

                    int index = filteredCheck(e.Server.Roles, newrole, e.Server.Id);
                    /* IEnumerable<Discord.Role> x = e.Server.Roles;
                    bool t = false;
                    for (int i = 0; i < x.Count(); i++)
                    {
                        if (x.ElementAt(i).Name.Equals(newrole, StringComparison.OrdinalIgnoreCase))
                        {
                            index = i;
                            t = true;
                            break;
                        }
                    } */

                    if (index == -1)
                    {
                        await e.Channel.SendMessage("That doesn't look like a role...");
                        return;
                    }

                    if (!e.User.HasRole(e.Server.Roles.ElementAt(index)))
                    {
                        await e.Channel.SendMessage("You don't have that role.");
                        return;
                    }

                    var lines = File.ReadAllLines(e.Server.Id.ToString() + ".txt");
                    newrole = e.Server.Roles.ElementAt(index).Name;

                    if (Array.IndexOf(lines, newrole) != -1)
                    {
                        await e.User.RemoveRoles(e.Server.Roles.ElementAt(index));
                        await e.Channel.SendMessage("You no longer have the **" + newrole + "** role.");
                    }
                });
        }

        private void registerBrainwashCmd()
        {
            commands.CreateCommand("initialize")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Do(async (e) =>
                {
                    String name = e.Server.Id.ToString() + ".txt";
                    if (File.Exists(name))
                    {
                        File.Delete(name);
                    }
                    File.CreateText(name).Close();
                    File.WriteAllLines(name, defaultConfig);
                    await e.Channel.SendMessage("...alright, I think I'm set.");
                    discord.Log.Log(LogSeverity.Info, "Initialization", "Initialized on guild " + e.Server.Name + ".");
                });
        }

        private void registerSetAliasCmd()
        {
            commands.CreateCommand("setalias")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Parameter("original", ParameterType.Required)
                .Parameter("newalias", ParameterType.Required)
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage(e.GetArg("original"));
                    await e.Channel.SendMessage(e.GetArg("newalias"));
                });
        }

        private void registerCtCommand()
        {
            commands.CreateCommand("ct")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage(e.Channel.Topic);
                });
        }

        private void registerSayCommand()
        {
            commands.CreateCommand("say")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Parameter("channel", ParameterType.Required)
                .Parameter("message", ParameterType.Required)
                .Do(async (e) =>
                {
                    Discord.Channel channel = e.Server.FindChannels(e.GetArg("channel")).FirstOrDefault();
                    int delay = 0;
                    int length = e.GetArg("message").Length;
                    do
                    {
                        delay += 333;
                        length -= 5;
                    } while (length >= 5);
                    await channel.SendIsTyping();
                    await Task.Delay(delay);
                    await channel.SendMessage(e.GetArg("message"));
                    discord.Log.Log(LogSeverity.Info, "Message sent", "Send message \"" + e.GetArg("message") + "\" on guild " + e.Server.Name + ".");
                });
        }

        private void registerCompareCommand()
        {
            commands.CreateCommand("compare")
                .Parameter("first", ParameterType.Required)
                .Parameter("second", ParameterType.Required)
                .Do(async (e) =>
                {
                    Debug.WriteLine("compareCommand works up until this point");
                    if (closeEnough(e.GetArg("first"), e.GetArg("second"), getTolerance(e.Server.Id)))
                    {
                        await e.Channel.SendMessage("Yep, those are close enough!");
                    }
                    else
                    {
                        await e.Channel.SendMessage("Those are a bit too different...");
                    }
                });
        }

        private void registerSetToleranceCommand()
        {
            commands.CreateCommand("settolerance")
                .Parameter("newtolerance", ParameterType.Required)
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Do(async (e) =>
                {
                    var lines = File.ReadAllLines(e.Server.Id + ".txt");
                    
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == "Tolerance")
                        {
                            lines[i + 1] = e.GetArg("newtolerance");
                            File.WriteAllLines(e.Server.Id.ToString() + ".txt", lines);
                            await e.Channel.SendMessage("Your tolerance is now **" + e.GetArg("newtolerance") + ".**");
                            discord.Log.Log(LogSeverity.Info, "Tolerance change", "Changed tolerance in guild " + e.Server.Name + " to " + e.GetArg("newtolerance") + ".");
                            return;
                        }
                    }
                });
        }
        
        /* private void registerResetCommand()
        {
            commands.CreateCommand("reset")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Do(async (e) =>
                {
                    if (DateTime.Now.Ticks - lastRestart.Ticks > restartCD)
                    {
                        await discord.Disconnect();
                        await discord.Connect(File.ReadAllText("token"), TokenType.Bot);
                        Debug.WriteLine("works after reconnect");
                        lastRestart = DateTime.Now;
                        await e.Channel.SendMessage("`Restart complete. Functionality restored.`");
                        await e.Channel.SendMessage("...huh? What happened?");
                        discord.Log.Log(LogSeverity.Warning, "Successful restart", "ALERT: Bot was restarted from guild " + e.Server.Name + ".");
                    }
                    else
                    {
                        await e.Channel.SendMessage("`ERROR: MEMORY BANKS NOT FULLY CHARGED. TIME UNTIL RESTART IS AVAILABLE: " + ((restartCD - (DateTime.Now.Ticks - lastRestart.Ticks)) / 10000000) + " SECONDS.`");
                        discord.Log.Log(LogSeverity.Warning, "Failed restart", "ALERT: Bot restart was attempted from guild " + e.Server.Name + ".");
                    }
                });
        } */

        private void registerHelpCommand()
        {
            commands.CreateCommand("help")
                .Do(async (e) =>
                {
                    if (!e.User.ServerPermissions.Administrator)
                        await e.User.SendMessage(WelcomeMessages.helpMessage);
                    else
                        await e.User.SendMessage(WelcomeMessages.adminHelpMessage);
                });
        }

        private void registerBanCommand()
        {
            commands.CreateCommand("ban")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Parameter("player", ParameterType.Required)
                .Parameter("reason", ParameterType.Required)
                .Do(async (e) =>
                {
                    Discord.User target = e.Server.FindUsers(e.GetArg("player")).FirstOrDefault();
                    if(getLogChannel(e.Server.Id) == "")
                    {
                        await e.Channel.SendMessage("You've gotta set a banlog channel with .setlogchannel first.");
                        return;
                    }
                    Discord.Channel logChannel = e.Server.FindChannels(getLogChannel(e.Server.Id)).FirstOrDefault();
                    await e.Server.Ban(target);
                    await logChannel.SendMessage("Banned user **" + target.Name + "**. Reason given: " + e.GetArg("reason"));
                });
        }

        private void registerSetLogChannelCommand()
        {
            commands.CreateCommand("setlogchannel")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Parameter("channel", ParameterType.Required)
                .Do(async (e) =>
                {
                    var lines = File.ReadAllLines(e.Server.Id + ".txt.");
                    int index = Array.IndexOf(lines, "logchannel");
                    lines[index + 1] = e.GetArg("channel");
                    File.WriteAllLines(e.Server.Id + ".txt", lines);
                    await e.Channel.SendMessage("Set log channel to **" + e.GetArg("channel") + "**.");
                });
        }

        private void registerSetSpamLimitCommand()
        {
            commands.CreateCommand("spamlimit")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Parameter("n", ParameterType.Required)
                .Do(async (e) =>
                {
                    var lines = File.ReadAllLines(e.Server.Id + ".txt.");
                    int index = Array.IndexOf(lines, "spamtolerance");
                    lines[index + 1] = e.GetArg("n");
                    File.WriteAllLines(e.Server.Id + ".txt", lines);
                    await e.Channel.SendMessage("Set spam limit to **" + e.GetArg("n") + "**.");
                });
        }

        private void registerSetSpamCDCommand()
        {
            commands.CreateCommand("spamcd")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Parameter("n", ParameterType.Required)
                .Do(async (e) =>
                {
                    var lines = File.ReadAllLines(e.Server.Id + ".txt.");
                    int index = Array.IndexOf(lines, "spamfalloff");
                    lines[index + 1] = e.GetArg("n");
                    Debug.WriteLine(e.GetArg("n"));
                    File.WriteAllLines(e.Server.Id + ".txt", lines);
                    await e.Channel.SendMessage("Set spam falloff to **" + e.GetArg("n") + "**.");
                });
        }

        private void registerPurgeUserCommand()
        {
            commands.CreateCommand("purgeuser")
                .AddCheck((cm, u, ch) => u.ServerPermissions.Administrator)
                .Parameter("user", ParameterType.Required)
                .Parameter("num", ParameterType.Required)
                .Do(async (e) =>
                {
                    for(int i = 0; i < messageLists.Count(); i++)
                    {
                        Debug.WriteLine(messageLists.ElementAt(i).server);
                        Debug.WriteLine(e.Server);
                        if(messageLists.ElementAt(i).server == e.Server)
                        {
                            messageLists.ElementAt(i).wipe(e.GetArg("user"), Convert.ToInt32(e.GetArg("num")));
                            await e.Channel.SendMessage("Got 'em.");
                            return;
                        }
                    }
                    await e.Channel.SendMessage("Couldn't find that user.");
                });
        }

        private void initialize()
        {
            registerBestPonyCmd(); //works; displays test message, meant to test if online
            registerSelfieCmd();  //works; displays full art of icon, meant to test if online
            registerDebugCmd(); //works; dummy command that can be altered for use for debugging
            registerBrainwashCmd(); //works; deletes are replaces config file for server, to start bot usage/remove all previous settings
            registerPurgeCmd(); //works; deletes the previous x messages in the specified channel
            registerSetAliasCmd();  //doesn't work; replaces certain strings with other specified strings, meant to solve people misspelling things in commands
            registerAsarCmd(); //works; adds a self-assignable role to the server's list in config file so users can give it to themselves with .iam
            registerIamCommand(); //works; adds the specified role to the user if and only if it's in the approved list in the config file
            registerIamnCommand(); //works; removes the specified role from the user if they have it and it's in the approved lits in the config file
            registerCtCommand(); //works; prints channel topic in current channel for quick access
            registerSayCommand(); //mostly works; prints message in given channel, seems to work in all channels except general
            registerCompareCommand(); //works; tells whether two strings interpreted the same with the server's current tolerance
            registerSetToleranceCommand(); //works; changes the particular server's tolerance level
            // registerResetCommand(); //doesn't work; resets the bot on a 5min cooldown
            registerHelpCommand(); //works; resets the bot on a 5min cooldown
            registerBanCommand();
            registerSetLogChannelCommand();
            registerPurgeUserCommand();
            registerSetSpamCDCommand();
            registerSetSpamLimitCommand();
        }

        private bool closeEnough(String source, String test, int tolerance) //tests if a string 'test' is within 'tolerance' characters of 'source'
        {
            int off = 0; //tracks how many characters off test is from source
            if (Math.Abs(source.Length - test.Length) > tolerance)
            { //if the strings are obviously not equal
                return false;
            }
            for (int i = 0; i < source.Length; i++)
            {
                if (i + tolerance - off >= test.Length) //with zero errors, if the current index checked plus the tolerance is equal to or greater than the length, then even if the last characters are all wrong, it would still work. However, if something is off, you have to check a bit further to make sure off won't exceed tolerance
                {
                    return true;
                }
                if (!(String.Equals(test.ElementAt(i).ToString(), source.ElementAt(i).ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    off++; //increments error number because duh
                    Debug.WriteLine("off: " + off);
                    if (!(i >= test.Length - 1 || i >= source.Length - 1)) //don't do this if the checks would be outside the bounds of the string
                    {
                        if (String.Equals(test.ElementAt(i).ToString(), source.ElementAt(i + 1).ToString(), StringComparison.OrdinalIgnoreCase)) //in case there was a random letter left out from test
                        {
                            test = test.Substring(0, i) + " " + test.Substring(i, test.Length - i); //put in a space there so the other letters should be lined up
                        }
                        if (String.Equals(test.ElementAt(i + 1).ToString(), source.ElementAt(i).ToString(), StringComparison.OrdinalIgnoreCase)) //in case there was a random letter put into test
                        {
                            test = test.Substring(0, i) + test.Substring(i + 1, test.Length - i - 1); //cut out the letter at i
                        }
                    }
                    if (off > tolerance) //if outside of acceptable error level, return false
                    {
                        return false;
                    }
                }
            }
            Debug.WriteLine("Returning true");
            return true;
        }

        private int filteredCheck(IEnumerable<Discord.Role> list, String input, ulong serverid)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                Debug.WriteLine("Checking for similarity to: " + list.ElementAt(i).Name + " with tolerance " + getTolerance(serverid));
                if (closeEnough(list.ElementAt(i).Name, input, getTolerance(serverid)))
                {
                    Debug.WriteLine("Found identical at index " + i);
                    return i;
                }
            }
            return -1;
        }

        public int getTolerance(ulong server)
        {
            var lines = File.ReadAllLines(server + ".txt");

            if (Array.IndexOf(lines, "Tolerance") == -1)
            {
                return -1;
            }
            return Convert.ToInt32(lines[Array.IndexOf(lines, "Tolerance") + 1]);
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

        private void Split<T>(T[] array, int index, out T[] first, out T[] second) //splits array into two arrays at point
        {
            first = array.Take(index).ToArray();
            second = array.Skip(index).ToArray();
        }


        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
