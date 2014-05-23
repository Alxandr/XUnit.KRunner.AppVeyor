using Xunit.Abstractions;

namespace Xunit.KRunner.AppVeyor
{
    public class TestMessageVisitorFactory : IMessageSinkFactory
    {
        public IMessageSink CreateMessageSink(TestingContext context)
        {
            return new AppVeyorTestMessageVisitor(context.Configuration);
        }
    }
}
