// HtmlContent.cs
namespace Medoz.KoeKan.Server;
internal static class HtmlContent
{
    /// <summary>
    /// チャットクライアントのHTML
    /// </summary>
    internal static string ChatClientHtml = @"
<!DOCTYPE html>
<html lang='ja'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>WPFチャットクライアント</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f5f5f5;
        }
        .chat-container {
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            height: 100vh;
            display: flex;
            flex-direction: column;
        }
        .chat-header {
            background-color: #007bff;
            color: white;
            padding: 10px 20px;
            border-radius: 5px 5px 0 0;
            font-size: 18px;
            font-weight: bold;
        }
        .connection-status {
            padding: 10px;
            text-align: center;
            margin-bottom: 10px;
        }
        .connected {
            color: green;
        }
        .disconnected {
            color: red;
        }
        .chat-messages {
            flex: 1;
            overflow-y: auto;
            border: 1px solid #ddd;
            border-radius: 5px;
            background-color: white;
            padding: 10px;
            margin-bottom: 10px;
        }
        .message {
            margin-bottom: 10px;
            padding: 8px 10px;
            border-radius: 5px;
            max-width: 80%;
            word-wrap: break-word;
        }
        .server-message {
            background-color: #f1f1f1;
            align-self: center;
            text-align: center;
            font-style: italic;
            color: #666;
            width: 100%;
            max-width: 100%;
            border-bottom: 1px solid #ddd;
            padding: 5px 0;
            margin: 5px 0;
        }
        .user-message {
            background-color: #dcf8c6;
            align-self: flex-end;
            margin-left: auto;
        }
        .other-message {
            background-color: #f1f0f0;
        }
        .message-info {
            display: flex;
            justify-content: space-between;
            font-size: 12px;
            margin-bottom: 5px;
        }
        .sender {
            font-weight: bold;
        }
        .timestamp {
            color: #999;
        }
        .message-input-container {
            display: flex;
            margin-top: 10px;
        }
        .message-input {
            flex: 1;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
            margin-right: 10px;
            font-size: 16px;
        }
        .send-button {
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 5px;
            padding: 10px 15px;
            cursor: pointer;
            font-size: 16px;
        }
        .send-button:hover {
            background-color: #0069d9;
        }
        .nickname-container {
            margin-bottom: 15px;
        }
        .message-container {
            display: flex;
            flex-direction: column;
        }
    </style>
</head>
<body>
    <div class='chat-container'>
        <div class='chat-header'>WPFチャットクライアント</div>
        <div id='connection-status' class='connection-status disconnected'>切断されています</div>

        <div class='nickname-container'>
            <label for='nickname'>ニックネーム: </label>
            <input type='text' id='nickname' value='ゲスト' />
        </div>

        <div id='chat-messages' class='chat-messages'></div>

        <div class='message-input-container'>
            <input type='text' id='message-input' class='message-input' placeholder='メッセージを入力...' />
            <button id='send-button' class='send-button'>送信</button>
        </div>
    </div>

    <script>
        const connectionStatus = document.getElementById('connection-status');
        const chatMessages = document.getElementById('chat-messages');
        const messageInput = document.getElementById('message-input');
        const sendButton = document.getElementById('send-button');
        const nicknameInput = document.getElementById('nickname');

        let socket = null;
        let reconnectAttempts = 0;
        const maxReconnectAttempts = 5;
        const reconnectInterval = 3000;

        function connect() {
            // WebSocketのURLを構築（httpsならwss、httpならws）
            const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
            const wsUrl = `${protocol}//${window.location.host}${window.location.pathname}`;

            socket = new WebSocket(wsUrl);

            socket.onopen = function() {
                connectionStatus.textContent = '接続されました';
                connectionStatus.className = 'connection-status connected';
                reconnectAttempts = 0;
                addServerMessage('サーバーに接続しました');
            };

            socket.onmessage = function(event) {
                const message = JSON.parse(event.data);
                if (message.type === 'message') {
                    addMessage(message.sender, message.content, message.timestamp);
                }
            };

            socket.onclose = function() {
                connectionStatus.textContent = '切断されました';
                connectionStatus.className = 'connection-status disconnected';
                addServerMessage('サーバーから切断されました');

                if (reconnectAttempts < maxReconnectAttempts) {
                    setTimeout(connect, reconnectInterval);
                    reconnectAttempts++;
                    addServerMessage(`再接続を試みています... (${reconnectAttempts}/${maxReconnectAttempts})`);
                } else {
                    addServerMessage('再接続に失敗しました。ページを更新してください。');
                }
            };

            socket.onerror = function(error) {
                console.error('WebSocket error:', error);
                addServerMessage('エラーが発生しました');
            };
        }

        function sendMessage() {
            if (!socket || socket.readyState !== WebSocket.OPEN) {
                addServerMessage('サーバーに接続されていません');
                return;
            }

            const message = messageInput.value.trim();
            if (message === '') return;

            const nickname = nicknameInput.value.trim() || 'ゲスト';

            const messageObj = {
                type: 'message',
                sender: nickname,
                content: message
            };

            socket.send(JSON.stringify(messageObj));
            messageInput.value = '';
        }

        function addMessage(sender, content, timestamp) {
            const nickname = nicknameInput.value.trim() || 'ゲスト';
            const isOwnMessage = sender === nickname;
            const isServerMessage = sender === 'サーバー';

            const messageElement = document.createElement('div');

            if (isServerMessage) {
                messageElement.className = 'message server-message';
                messageElement.textContent = content;
            } else {
                messageElement.className = `message ${isOwnMessage ? 'user-message' : 'other-message'}`;

                const messageInfo = document.createElement('div');
                messageInfo.className = 'message-info';

                const senderElement = document.createElement('span');
                senderElement.className = 'sender';
                senderElement.textContent = sender;

                const timestampElement = document.createElement('span');
                timestampElement.className = 'timestamp';
                timestampElement.textContent = timestamp || new Date().toLocaleTimeString();

                messageInfo.appendChild(senderElement);
                messageInfo.appendChild(timestampElement);

                const contentElement = document.createElement('div');
                contentElement.className = 'message-content';
                contentElement.textContent = content;

                messageElement.appendChild(messageInfo);
                messageElement.appendChild(contentElement);
            }

            chatMessages.appendChild(messageElement);
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }

        function addServerMessage(content) {
            const messageElement = document.createElement('div');
            messageElement.className = 'message server-message';
            messageElement.textContent = content;

            chatMessages.appendChild(messageElement);
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }

        // イベントリスナー
        sendButton.addEventListener('click', sendMessage);

        messageInput.addEventListener('keypress', function(event) {
            if (event.key === 'Enter') {
                sendMessage();
            }
        });

        // 接続開始
        window.addEventListener('load', connect);
    </script>
</body>
</html>
";
}