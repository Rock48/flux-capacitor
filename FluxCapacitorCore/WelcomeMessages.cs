using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluxCapacitorCore
{
    class WelcomeMessages
    {
        public static String helpMessage = "Uh, hey! I heard you requested my documentation, so...\n" +
        "\n" +
        "**.bestpony -** Outputs test message, meant to test if bot is responding\n" +
        "\n" +
        "**.iam {role} -**Adds {role} to user, as long as it’s on the list of self-assignable roles\n" +
        "*Example:* .iam Not a Robot\n" +
        "\n" +
        "**.iamn {role} -** Removes self-assignable role from user, if they have it\n" +
        "*Example:* .iamn Not a Robot\n" +
        "\n" +
        "**.ct -** Displays channel topic in current channel\n" +
        "\n" +
        "**.compare {string1} {string2} -** Outputs whether two strings are considered equal by .iam and .iamn\n" +
        "*Example:* .compare \"Not a Robot\" \"notarobot\"";
        public static String adminHelpMessage = "Uh, hey! I heard you requested my documentation, so...\n" +
        "\n" +
        "**.bestpony -** Outputs test message, meant to test if bot is responding\n" +
        "\n" +
        "**.initialize -** Resets all settings for server, defaulting tolerance to 2 and removing all self-assignable roles\n" +
        "\n" +
        "**.purge {number} [channel] -**Deletes most recent *number* messages in *channel*, or in the channel the command was run in if no channel is provided\n" +
        "*Example:* .purge 50 general\n" +
        "\n" +
        "**.asar {role} -**Adds {role} to list of self-assignable roles, so users can add them with .iam\n" +
        "*Example:* .iam Not a Robot\n" +
        "\n" +
        "**.iam {role} -**Adds {role} to user, as long as it’s on the list of self-assignable roles\n" +
        "*Example:* .iam Not a Robot\n" +
        "\n" +
        "**.iamn {role} -** Removes self-assignable role from user, if they have it\n" +
        "*Example:* .iamn Not a Robot\n" +
        "\n" +
        "**.ct -** Displays channel topic in current channel\n" +
        "\n" +
        "**.say {channel} {message} -** Makes bot speak **message** in **channel**\n" +
        "*Example:* .say lobby \"Uh, hey! I heard you requested my documentation, so...\"\n" +
        "\n" +
        "**.settolerance {newtolerance} -** Sets server tolerance to **newtolerance**, so users can misspell role names by **newtolerance** and still have .iam work\n" +
        "*Example:* .say lobby \"Uh, hey! I heard you requested my documentation, so...\"\n" +
        "\n" +
        "**.ban {user} {reason} - **Bans user, then logs the ban and the reason in server log channel\n" +
        "*Example:* .ban \"Hitler\" \"Controversial ideology\"\n" +
        "\n" +
        "**.setlogchannel {channel} -** Sets server log channel to the first channel named *channel*. This is where ban logs are stored.\n" +
        "*Example:* .setlogchannel ban-log\n";
    }
}
