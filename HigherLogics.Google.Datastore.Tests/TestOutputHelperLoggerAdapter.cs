using System;
using Grpc.Core.Logging;
using Xunit.Abstractions;

namespace HigherLogics.Google.Datastore.Tests
{
    public class TestOutputHelperLoggerAdapter : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputHelperLoggerAdapter(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger ForType<T>()
        {
            return new TestOutputHelperLoggerAdapter(_testOutputHelper);
        }

        private void Write(string message) => _testOutputHelper.WriteLine(message);
        private void Write(string format, object[] formatArgs) => _testOutputHelper.WriteLine(format, formatArgs);

        public void Debug(string message) => Write(message);

        public void Debug(string format, params object[] formatArgs) => Write(format, formatArgs);


        public void Info(string message) => Write(message);

        public void Info(string format, params object[] formatArgs) => Write(format, formatArgs);

        public void Warning(string message) => Write(message);

        public void Warning(string format, params object[] formatArgs) => Write(format, formatArgs);

        public void Warning(Exception exception, string message) => Write(message);

        public void Error(string message) => Write(message);

        public void Error(string format, params object[] formatArgs) => Write(format, formatArgs);


        public void Error(Exception exception, string message) => Write(message);
    }
}