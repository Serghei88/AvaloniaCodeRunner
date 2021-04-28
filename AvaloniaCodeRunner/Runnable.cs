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
}