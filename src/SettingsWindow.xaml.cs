using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Controls = System.Windows.Controls;

namespace Medoz.KoeKan;

public partial class SettingsWindow : Window
{

    public SettingsWindow(SettingsWindowViewModel settingsWindowViewModel)
    {
        DataContext = settingsWindowViewModel;
        InitializeComponent();
        InitializeViewComponents();

        settingsWindowViewModel.RequestClose += (s, e) => this.Close();
        
        ShowPanel("General"); // 初期表示パネルを設定
    }

    GeneralSettingsView? _generalSettingsView;

    private void InitializeViewComponents()
    {
        var g = new GeneralSettingsView();
        g.DataContext = DataContext;
        _generalSettingsView = g;
    }

    /// <summary>
    /// メニューリストの選択変更イベント
    /// </summary>
    private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MenuListBox.SelectedItem is ListBoxItem selectedItem)
        {
            string tag = selectedItem.Tag?.ToString();
            if (!string.IsNullOrEmpty(tag))
            {
                ShowPanel(tag);
            }
        }
    }

    /// <summary>
    /// 指定されたパネルを表示する
    /// </summary>
    /// <param name="panelName">表示するパネル名</param>
    private void ShowPanel(string panelName)
    {
        System.Windows.Controls.UserControl? controlToShow = null;

        switch (panelName)
        {
            case "General":
                controlToShow = _generalSettingsView;
                break;
        }

        if (controlToShow != null)
        {
            // ContentControlに選択されたUserControlを設定
            MainContent.Content = controlToShow;
        }
    }

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as Controls.TextBox;
        if (textBox == null) return;

        // 現在のテキストと入力文字を結合
        string currentText = textBox.Text;
        string newText = currentText.Insert(textBox.SelectionStart, e.Text);

        // 選択範囲がある場合は置換
        if (textBox.SelectionLength > 0)
        {
            newText = currentText.Remove(textBox.SelectionStart, textBox.SelectionLength);
            newText = newText.Insert(textBox.SelectionStart, e.Text);
        }

        // double値として有効かチェック
        if (!IsValidDoubleInputRegex(newText))
        {
            e.Handled = true; // 入力をキャンセル
        }
    }

    private void NumericTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        // ペースト操作をブロック（必要に応じて）
        if (e.Command == ApplicationCommands.Paste)
        {
            var textBox = sender as Controls.TextBox;
            if (textBox != null)
            {
                string pasteText = System.Windows.Clipboard.GetText();
                if (!IsValidDoubleInputRegex(pasteText))
                {
                    e.Handled = true;
                }
            }
        }
    }

    // より厳密な正規表現を使用する場合（オプション）
    private bool IsValidDoubleInputRegex(string input, bool allowNegative = false)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        string pattern = @"(\d+\.?\d*|\.\d+)([eE][-+]?\d+)?$";
        if (allowNegative)
        {
            pattern = $"^-?{pattern}";
        }
        else
        {
            pattern = $"^{pattern}";
        }
        return Regex.IsMatch(input, pattern) || input == "-" || input == ".";
    }
}