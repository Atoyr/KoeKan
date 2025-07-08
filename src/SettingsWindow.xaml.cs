using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Medoz.KoeKan;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        // DataContextにバインドされたViewModelがDialogViewModelであることを前提に購読
        if (DataContext is SettingsWindowViewModel vm)
        {
            vm.RequestClose += (s, e) => this.Close();
        }
    }

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as TextBox;
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
            var textBox = sender as TextBox;
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