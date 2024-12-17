using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using sakuragram.Controls.Core;
using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class PollType
{
    private TdClient _client = App._client;
    private long _chatId;
    private long _messageId;
    
    private List<TdApi.PollOption> _pollOptions;
    private TdApi.PollType _pollType;
    private TdApi.FormattedText _explanation;
    private bool _isVoted;
    private bool _isAnonymous;
    
    private List<Border> _options = [];
    private List<RadioButton> _radioOptions = [];
    private List<CheckBox> _checkOptions = [];
    private List<TextBlock> _answers = [];
    private List<int> _optionIndex = [];
    private int _correctAnswer = -1;
    
    public PollType(TdApi.MessageContent.MessagePoll poll, long chatId, long messageId)
    {
        InitializeComponent();

        _chatId = chatId;
        _messageId = messageId;
        _isAnonymous = poll.Poll.IsAnonymous;
        _pollType = poll.Poll.Type;
        _pollOptions = poll.Poll.Options.ToList();
        
        if (poll.Poll.Type is TdApi.PollType.PollTypeRegular typeRegular)
        {
            if (typeRegular.AllowMultipleAnswers) foreach (var multipleOption in poll.Poll.Options) 
                GenerateMultipleChoiceOptions(multipleOption);
            else foreach (var option in poll.Poll.Options) GenerateRegularOption(option);

            if (_isVoted)
            {
                if (typeRegular.AllowMultipleAnswers)
                    foreach (var checkOption in _checkOptions) checkOption.IsEnabled = false;
                else
                    foreach (var radioOption in _radioOptions) radioOption.IsEnabled = false;
            }
        }
        else if (poll.Poll.Type is TdApi.PollType.PollTypeQuiz typeQuiz)
        {
            _correctAnswer = typeQuiz.CorrectOptionId;
            _explanation = typeQuiz.Explanation;
            foreach (var option in poll.Poll.Options) GenerateQuizOptions(option);
            
            if (_isVoted)
            {
                foreach (var radioOption in _radioOptions) radioOption.IsEnabled = false;
                foreach (var answer in _answers) answer.Visibility = Visibility.Visible;
            }
        }

        if (_isVoted && poll.Poll.IsAnonymous || !_isVoted && poll.Poll.IsAnonymous)
        {
            ButtonVote.Visibility = Visibility.Collapsed;
            TextBlockVoteCount.Text = poll.Poll.TotalVoterCount > 0 ? $"{poll.Poll.TotalVoterCount} votes" : "No votes";
            TextBlockVoteCount.Visibility = Visibility.Visible;
        }
        else if (_isVoted && !poll.Poll.IsAnonymous)
        {
            ButtonVote.Content = "View results";
            ButtonVote.Visibility = Visibility.Visible;
            TextBlockVoteCount.Visibility = Visibility.Collapsed;
        }
        
        void GenerateRegularOption(TdApi.PollOption option)
        {
            _isVoted = option.IsChosen;
            var border = new Border { CornerRadius = new CornerRadius(4), 
                Background = (Brush)Application.Current.Resources["AccentAcrylicBackgroundFillColorDefaultBrush"], 
                Margin = new Thickness(0, 0, 0, 3) };
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(3, 0, 3, 0) };
            var textBlock = new TextBlock { Text = option.Text.Text, FontSize = 12 };
            var radioButton = new RadioButton { Content = textBlock, IsChecked = option.IsChosen };
            
            if (!_isVoted) ButtonVote.Content = option.IsChosen ? "View results" : "Vote";

            radioButton.Checked += async (_, _) =>
            {
                await _client.SetPollAnswerAsync(_chatId, _messageId, [_radioOptions.IndexOf(radioButton)]);
                foreach (var radioOption in _radioOptions) radioOption.IsEnabled = false;
            };
            
            stackPanel.Children.Add(radioButton);
            border.Child = stackPanel;
            StackPanelOptions.Children.Add(border);
            _radioOptions.Add(radioButton);
            _options.Add(border);
        }
        
        void GenerateMultipleChoiceOptions(TdApi.PollOption option)
        {
            _isVoted = option.IsChosen;
            var border = new Border { CornerRadius = new CornerRadius(4), 
                Background = (Brush)Application.Current.Resources["AccentAcrylicBackgroundFillColorDefaultBrush"], 
                Margin = new Thickness(0, 0, 0, 3) };
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(3, 0, 3, 0) };
            var textBlock = new TextBlock { Text = option.Text.Text, FontSize = 12 };
            var checkBox = new CheckBox { Content = textBlock, IsChecked = option.IsChosen };
            
            if (!_isVoted)
            {
                // _isVoted = true;
                ButtonVote.Content = option.IsChosen ? "View results" : "Vote";
            }

            checkBox.Checked += (_, _) =>
            {
                _optionIndex.Add(_checkOptions.IndexOf(checkBox));
                ButtonVote.Visibility = Visibility.Visible;
            };
            checkBox.Unchecked += (_, _) =>
            {
                _optionIndex.Remove(_checkOptions.IndexOf(checkBox));
                if (_optionIndex.Count <= 0) ButtonVote.Visibility = Visibility.Collapsed;
            };
            
            stackPanel.Children.Add(checkBox);
            border.Child = stackPanel;
            StackPanelOptions.Children.Add(border);
            _checkOptions.Add(checkBox);
            _options.Add(border);
        }
        
        void GenerateQuizOptions(TdApi.PollOption option)
        {
            _isVoted = option.IsChosen;
            var border = new Border { CornerRadius = new CornerRadius(4), 
                Background = (Brush)Application.Current.Resources["AccentAcrylicBackgroundFillColorDefaultBrush"], 
                Margin = new Thickness(0, 0, 0, 3) };
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(3, 0, 3, 0) };
            var textBlock = new TextBlock { Text = option.Text.Text, FontSize = 12 };
            var radioButton = new RadioButton { Content = textBlock, IsChecked = option.IsChosen };
            var quizAnswer = new TextBlock { Text = _correctAnswer == _pollOptions.IndexOf(option) ? "✅" : "❌", FontSize = 12 };
            
            if (!_isVoted) ButtonVote.Content = option.IsChosen ? "View results" : "Vote";

            radioButton.Checked += async (_, _) =>
            {
                await _client.SetPollAnswerAsync(_chatId, _messageId, [_radioOptions.IndexOf(radioButton)]);
                foreach (var radioOption in _radioOptions) radioOption.IsEnabled = false;
                foreach (var answer in _answers) answer.Visibility = Visibility.Visible;

                var text = new SakuraTextBlock();
                await text.SetFormattedText(_explanation);
                var dialog = new ContentDialog
                {
                    Title = _radioOptions.IndexOf(radioButton) == _correctAnswer ? "Correct answer!" : "Wrong answer.",
                    Content = text,
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            };
            
            stackPanel.Children.Add(radioButton);
            stackPanel.Children.Add(quizAnswer);
            quizAnswer.Visibility = Visibility.Collapsed;
            border.Child = stackPanel;
            StackPanelOptions.Children.Add(border);
            
            _answers.Add(quizAnswer);
            _radioOptions.Add(radioButton);
            _options.Add(border);
        }
    }

    private async void ButtonVote_OnClick(object sender, RoutedEventArgs e)
    {
        if (_optionIndex.Count <= 0 || _isVoted) return;
        await _client.SetPollAnswerAsync(_chatId, _messageId, _optionIndex.ToArray());
        _isVoted = true;
        _optionIndex.Clear();
        
        if (_isAnonymous) ButtonVote.Visibility = Visibility.Collapsed;
        else ButtonVote.Content = "View results";
        
        ButtonVote.Click -= ButtonVote_OnClick;

        switch (_pollType)
        {
            case TdApi.PollType.PollTypeQuiz:
                break;
            case TdApi.PollType.PollTypeRegular typeRegular:
                if (typeRegular.AllowMultipleAnswers) foreach (var checkOption in _checkOptions) checkOption.IsEnabled = false;
                else foreach (var radioOption in _radioOptions) radioOption.IsEnabled = false;
                break;
        }
    }
}