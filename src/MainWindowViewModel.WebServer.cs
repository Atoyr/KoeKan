using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Server;

namespace Medoz.KoeKan;

public partial class MainWindowViewModel
{
    private WebServer? _webServer;
    private void StartWebServer()
    {
        // if (_webServer is null)
        // {
        //     _webServer = new WebServer(
        //         (sender, content) => AddLogMessage(ChatMessageType.LogInfo, $"Received message from {sender}: {content}"),
        //         () => AddLogMessage(ChatMessageType.LogInfo, "Client connected."),
        //         () => AddLogMessage(ChatMessageType.LogInfo, "Client disconnected.")
        //     );
        //     _webServer.Start(8080);
        //     AddLogMessage(ChatMessageType.LogInfo, "Web server started.");
        // }
    }
}