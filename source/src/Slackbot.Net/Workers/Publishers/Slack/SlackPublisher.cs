using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Slackbot.Net.Workers.Publishers.Slack
{
    public class SlackPublisher : IPublisher
    {
        private readonly SlackSender _sender;
        private readonly ILogger<SlackPublisher> _logger;

        public SlackPublisher(SlackSender sender, ILogger<SlackPublisher> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public async Task Publish(Notification notification)
        {
            _logger.LogInformation(notification.Msg);
            await _sender.Send($"Dev - " + notification.Msg,  notification.Recipient);
        }
    }
}