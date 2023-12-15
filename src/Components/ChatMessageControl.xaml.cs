using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Medoz.MessageTransporter.Data;

namespace Medoz.MessageTransporter.Components;

public partial class ChatMessageControl : UserControl
{
    public ChatMessageControl()
    {
        InitializeComponent();
        Loaded += ChatMessage_Loaded;
    }

    private void ChatMessage_Loaded(object sender, RoutedEventArgs e)
    {
        ChannelTextBlock.Visibility = ChannelVisible;
        Profile.Visibility = ProfileVisible;
        ProfileIcon.Visibility = ProfileVisible;
        AccentLine.Background = Accent;
    }

    public Visibility ChannelVisible
    {
        get
        {
            if (DataContext is not null && DataContext is ChatMessage cm && string.IsNullOrWhiteSpace(cm.Channel))
            {
                return Visibility.Collapsed;
            }
            return ProfileVisible;
        }
    }

    public Visibility ProfileVisible
    {
        get 
        {
            if (DataContext is not null && DataContext is ChatMessage cm && !cm.IsMessageOnly)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
    }

    public Brush Accent
    {
        get
        {
            if (DataContext is ChatMessage cm)
            {
                var color = cm.MessageType switch {
                    ChatMessageType.Text => (Color)ColorConverter.ConvertFromString("#ff7b2e"), 
                    ChatMessageType.LogSuccess => (Color)ColorConverter.ConvertFromString("#FFADFF2F"), 
                    ChatMessageType.LogInfo => (Color)ColorConverter.ConvertFromString("#FF1E90FF"), 
                    ChatMessageType.LogWarning => (Color)ColorConverter.ConvertFromString("#FFDAA520"), 
                    ChatMessageType.LogFatal => (Color)ColorConverter.ConvertFromString("#FFFF0000"), 
                    ChatMessageType.DiscordText => (Color)ColorConverter.ConvertFromString("#5865F2"),
                    ChatMessageType.DiscordVoice => (Color)ColorConverter.ConvertFromString("#57F287"),
                    ChatMessageType.Twitch => (Color)ColorConverter.ConvertFromString("#9147ff"), 
                    _ => (Color)ColorConverter.ConvertFromString("#FFFF00"),



                };
                return new SolidColorBrush(color);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}