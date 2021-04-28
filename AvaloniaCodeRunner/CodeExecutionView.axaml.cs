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
        private string parameter;

        public Runnable(string p)
        {
            parameter = p;
        }

        public string Run(string message)
        {
            return message + parameter;
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
            var methodParameter = this.FindControl<TextBox>("MethodParameter");
            var constructorParameter = this.FindControl<TextBox>("ConstructorParameter");

            var compilationResult = CodeRunner.Compile(string.IsNullOrEmpty(codeText) ? defaultCode : codeText, typeof(IRunnable));
            
            if (!(compilationResult.Errors is null))
            {
                results.Text = compilationResult.Errors;
            }
            else
            {
                var executionResult = CodeRunner.Run(
                    compilationResult.Assembly,
                    typeof(IRunnable), new []{ constructorParameter.Text },
                    new object[] {methodParameter.Text});
                results.Text = (string) executionResult!;
            }
        }
    }
}