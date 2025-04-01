using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Command;

public class WriteCommand : ICommand
{
    public async Task ExecuteCommandAsync(CommandArgs args)
    {
        if (args.Args.Length == 0 || args.Args[0] == string.Empty)
        {
            try
            {
                args.Config.Save();
                args.Listener.AddLogMessage(ChatMessageType.LogInfo, "Config saved.");
            }
            catch (System.Exception)
            {
                args.Listener.AddLogMessage(ChatMessageType.LogWarning, "Save config is faild.");
            }
        }
        await Task.CompletedTask;
    }
}

