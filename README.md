# flux-capacitor
Discord bot mainly meant for role management. Allows users to add and remove roles from a selected list to themselves, with build-in tolerance for misspelling that is customizable to specific servers. Also has basic bot commands like .purge, .say, etc.
        
**.asar {role} -** Adds {role} to list of self-assignable roles, so users can add them with .iam
        
*Example:* .asar Not a Robot

**.ban {user} {reason} -** Bans user, then logs the ban and the reason in server log channel
        
*Example:* .ban TrenderhoofBestPony "Controversial ideology"

**.bestpony -** Outputs test message, meant to test if bot is responding

**.compare {string1} {string2} -** Outputs whether two strings are considered equal by .iam and .iamn
        
*Example:* .compare "Not a Robot" "notarobot"

**.ct -** Displays channel topic in current channel

**.initialize -** Resets all settings for server, defaulting tolerance to 2 and removing all self-assignable roles

**.iam {role} -** Adds {role} to user, as long as it’s on the list of self-assignable roles
        
*Example:* .iam Not a Robot

**.iamn {role} -** Removes self-assignable role from user, if they have it
        
*Example:* .iamn Not a Robot
              
**.purge {number} [channel] -** Deletes most recent *number* messages in *channel*, or in the channel the command was run in if no channel is provided
        
*Example:* .purge 50 general
  
**.reset -** Resets entire bot in case of errors, can only be used once every five minutes

**.say {channel} {message} -** Makes bot speak **message** in **channel**
        
*Example:* .say lobby "Vapor Trail is best pony."

**.setlogchannel {channel} -** Sets server log channel to the first channel named *channel*. This is where ban logs are stored
        
*Example:* .setlogchannel ban-log

**.settolerance {newtolerance} -** Sets server tolerance to **newtolerance**, so users can misspell role names by **newtolerance** and still have .iam work

**.spamCD {number} -** Sets how fast pressure decays for users. A higher SpamCD means spam bans people slower.

*Example:* .spamCD 1.5
     
**.spamlimit {number} -** Sets how much pressure can be accumulated before a user is banned. A higher SpamLimit means spam bans people slower.

*Example:* .spamlimit 3
