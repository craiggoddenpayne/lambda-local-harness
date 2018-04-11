using System;
using Amazon.Lambda.Core;

namespace LambdaHarnessApi
{
    class ConsoleLambdaLogger : ILambdaLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void LogLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}