using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Slackbot.Net.Workers.Publishers;
using SlackConnector.Models;

namespace Slackbot.Net.Workers.Handlers
{
    public class HandlerSelector
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<HandlerSelector> _logger;

        public HandlerSelector(IServiceProvider provider, ILogger<HandlerSelector> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task HandleIncomingMessage(SlackMessage message)
        {
            var allHandlers = _provider.GetServices<IHandleMessages>();
            var publishers = _provider.GetServices<IPublisher>();
            var helpHandler = new HelpHandler(publishers, allHandlers);

            if (helpHandler.ShouldHandle(message))
            {
                await helpHandler.Handle(message);
                return;
            }

            var handlers = SelectHandler(allHandlers,message);
            foreach (var handler in handlers)
            {
                try
                {
                    await handler.Handle(message);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }

        private IEnumerable<IHandleMessages> SelectHandler(IEnumerable<IHandleMessages> handlers, SlackMessage message)
        {
            var matchingHandlers = handlers.Where(s => s.ShouldHandle(message));
            foreach (var handler in matchingHandlers)
            {
                yield return handler;
            }

            yield return new NoOpHandler();
        }
    }
}