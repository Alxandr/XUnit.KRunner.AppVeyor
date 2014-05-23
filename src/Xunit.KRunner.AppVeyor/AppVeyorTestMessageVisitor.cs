using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Framework.ConfigurationModel;
using Xunit.Abstractions;

namespace Xunit.KRunner.AppVeyor
{
    public class AppVeyorTestMessageVisitor : TestMessageVisitor
    {
        readonly HttpClient _client;

        public AppVeyorTestMessageVisitor(IConfiguration configuration)
        {
            string url = configuration.Get("APPVEYOR_API_URL");
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("APPVEYOR_API_URL is not set");

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                throw new ArgumentException("APPVEYOR_API_URL is not a valid URI");

            _client = new HttpClient();
            _client.BaseAddress = uri;
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            _client.SendAsync(new TestMessage(false)
            {
                TestName = testStarting.TestDisplayName,
                Outcome = Outcome.Running
            }).Wait();
            
            return base.Visit(testStarting);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            NotifyTestFinished(testSkipped, Outcome.Skipped, stderr:  "Skipped: " + testSkipped.Reason);
            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            NotifyTestFinished(testFailed, Outcome.Failed, 
                exception: ExceptionUtility.CombineMessages(testFailed), 
                stackTrace: ExceptionUtility.CombineStackTraces(testFailed));
            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            NotifyTestFinished(testPassed, Outcome.Passed);
            return base.Visit(testPassed);
        }

        private void NotifyTestFinished(ITestResultMessage message, Outcome outcome, 
            string stderr = null, string exception = null, string stackTrace = null)
        {
            _client.SendAsync(new TestMessage(true)
            {
                TestName = message.TestDisplayName,
                Outcome = outcome,
                StdOut = message.Output,
                StdErr = stderr,
                ErrorMessage = exception,
                ErrorStackTrace = stackTrace,
                Duration = TimeSpan.FromSeconds((double)message.ExecutionTime)
            }).Wait();
        }

        public override void Dispose()
        {
            _client.Dispose();
            base.Dispose();
        }
    }
}
