using System.Windows;

using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Medoz.MessageTransporter.Clients;
using Medoz.MessageTransporter.Data;


namespace Medoz.MessageTransporter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow 
{
    protected async Task ExecuteCommand(string str)
    {
        var split = str.Split(' ', 2);
        var command = split[0];
        string text = "";
        if (split.Length > 1)
        {
            text = split[1];
        }

        bool isExecuted = false;
        switch(command)
        {
            case "w":
                WriteCommand(text);
                break;
            case "q":
                isExecuted = true;
                Close();
                break;
            case "discord":
                await DiscordCommand(text);
                break;
        }
    }

    private void WriteCommand(string text)
    {
        if (text == "config")
        {
            _config.Save();
            ChatListBox.Items.Add("Save config successed.");
        }
        else
        {
            // TODO ERROR

        }
    }

    private void QuitCommand(string text)
    {
        if(string.IsNullOrEmpty(text))
        {
            Close();
        }
        else
        {
            // TODO ERROR
        }
    }

    private async Task DiscordCommand(string text)
    {
        var strs = text.Split(' ');
        if (strs[0] == "start" && strs.Length == 1)
        {
            SetDiscordClient();
            ChatListBox.Items.Add("Start Discord Connections");
            return;
        }

        if (_discordClient is null)
        {
            ChatListBox.Items.Add("Discord is not started");
            return;
        }
        if (strs[0] == "channels" && strs.Length == 1)
        {
            foreach(var c in _discordClient.GetChannels())
            {
                ChatListBox.Items.Add($"{c.GuildName} | {c.Id} : {c.Name}");
            }
        }
        else if (strs[0] == "channel" && strs.Length == 2)
        {
            if (ulong.TryParse(strs[1], out ulong id))
            {
                _discordClient.SetChannel(id);
            }
            else
            {
                ChatListBox.Items.Add("Id is not validated");
            }
        }
        else if (strs[0] == "guilds" && strs.Length == 1)
        {
            var guilds = await _discordClient.GetGuildsAsync();
            foreach(var g in guilds)
            {
                ChatListBox.Items.Add($"{g.Id} : {g.Name}");
            }
        }
    }

}