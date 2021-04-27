using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AvaloniaCodeRunner
{
    public class CodeExecutionView : UserControl
    {
        string defaultCode = @"
                using System;

                namespace AvaloniaCodeRunner
                {
                    public class Runnable: IRunnable
                    {
                        public string Run(string message)
                        {
                            return message + ""test"";
                        }
                    }
                }";
        public CodeExecutionView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var code = this.FindControl<TextBox>("Code");
            code.Text = defaultCode;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var codeText = this.FindControl<TextBox>("Code").Text;
            var results = this.FindControl<TextBox>("Results");
            var parameter = this.FindControl<TextBox>("Parameter");
            

            var executionResult = CodeRunner.RunCode(
                string.IsNullOrEmpty(codeText) ? defaultCode : codeText,
                typeof(IRunnable),
                new object[] {parameter.Text});
            if (!(executionResult.Errors is null))
            {
                results.Text = executionResult.Errors;
            }
            else
            {
                results.Text = (string) executionResult.Result!;
            }
        }
    }
}