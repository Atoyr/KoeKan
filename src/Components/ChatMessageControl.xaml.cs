using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Components;

public partial class ChatMessageControl : UserControl
{
    public static readonly DependencyProperty ColorsProperty =
        DependencyProperty.Register("Colors", typeof(ChatMessageColors), typeof(ChatMessageControl),
        new PropertyMetadata(ChatMessageColors.Default()));

    public ChatMessageColors AccentColors
    {
        get { return (ChatMessageColors)GetValue(ColorsProperty); }
        set { SetValue(ColorsProperty, value); }
    }

    private readonly string _defaultColor = "#000000";

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
            if (DataContext is not null && DataContext is ChatMessage cm && !cm.IsConsecutiveMessage)
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
                var color = (Color)ColorConverter.ConvertFromString(AccentColors[cm.MessageType] ?? _defaultColor);
                return new SolidColorBrush(color);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}