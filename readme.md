# About

A framework for hosting a Slack bot in a .NET Core 2.2 host

# Install
Download it from NuGet:[![NuGet](https://img.shields.io/nuget/dt/slackbot.net.svg)](https://www.nuget.org/packages/slackbot.net/)

`$ dotnet add package slackbot.net`


# Host setup

Sample setup, using .NET Core DI extensions:

```
services.AddSlackbotWorker(o => { o.Slackbot_SlackApiKey_BotUser = "sometoken"  })
    .AddPublisher<SlackPublisher>()
    .AddPublisher<LoggerPublisher>()
    .AddPublisher<MyCustomPublisher>()
    .AddRecurring<MyEightOClockRecurrer>(c => c.Cron = "0 0 8 * * *")
    .AddHandler<MyHandlerOfIncomingMessages>()
})
```

# Features
* Workers: 
  - Listen to all incoming messages, and execute a handler given a specific message is incoming
  - Execute code on regular intervals 
* Endpoints:
  - Receive payloads from Slack and execute code.

## Workers
`SlackbotWorkers` use the [Slack Real Time API](https://api.slack.com/rtm), meaning they would have to be always running on a host. You could of course host in a Web host along with Kestrel, but it would mean it would be turning off when idle in many hosting scenarios (behind IIS for example). The worker listens to incoming messages in channels the bot is invited to, and executes any action that you define:

### Execute a handler on received messages
If you want to run code given a certain message was posted to Slack, register an implementation of `IHandleMessages` using `.AddHandler<T>`.

The `IHandleMessages` interface requires that you provide when the handler should execute (`ShouldHandle`), and what code it should run if `ShoulHandle` returns true.

```
public interface IHandleMessages
{
      Task<HandleResponse> Handle(SlackMessage message);
      bool ShouldHandle(SlackMessage message);
}
```

Sample implementation:

```
  public async Task<HandleResponse> Handle(SlackMessage message)
  {
      // run any code
      return new HandleResponse("OK");
  }

  public bool ShouldHandle(SlackMessage message)
  {
      return message.MentionsBot;
  }
```

### Recurring actions
If you want to run code snippets at recurring intervals (every day at 8AM, or every minute), register an implementation of `RecurringAction` using `.AddRecurring<T>(..)` and configure the interval using a Cron expression. This is not tied to Slack in any way, but you may for example post a message to Slack at these intervals if you want.

### Publishers
Publishers is an abstraction to send a simple message to a recipient. This library does not force you to use them, but if you want you can make use of included `IPublish` implementations.

* `SlackPublisher` => (message => slack channel)
* `LoggerPublisher` => (message => log output)

Possible future implementations:
* `Email` => (message => email recipient)
* `Teams` => (message => teams channel)
* `SMS` => (message => phone)
* `Callback` => (message => your API endpoint)

These are available via DI and can be used in any `IHandle` or `RecurringAction` implementation. You can also create your own.

Sample handler using all `IPublisher`s registered:

```
public class NotifyAllHandler : IHandleMessages
{
    private readonly IEnumerable<IPublisher> _publishers;

    public NotifyAllHandler(IEnumerable<IPublisher> publishers)
    {
        _publishers = publishers;
    }

    public async Task<HandleResponse> Handle(SlackMessage message)
    {
        foreach (var publisher in _publishers)
        {
            var notification = new Notification
            {
                Msg = aggr,
                Channel = message.ChatHub.Id //here: replying to the same channel
            };
            await publisher.Publish(notification);
        }
        return new HandleResponse("OK");
    }

    public bool ShouldHandle(SlackMessage message)
    {
        return message.MentionsBot
    }
}
```

```
public class NotifyOnlyUsingSpecificPublisherHandler : IHandleMessages
{
    private readonly IPublisher _publisher;

    public NotifyOnlyUsingSpecificPublisherHandler(IPublisherFactory publisherFactory)
    {
        _publisher = publisherFactory.GetPublisher<SlackPublisher>();
    }

    public async Task<HandleResponse> Handle(SlackMessage message)
    {  
        var notification = new Notification
        {
            Msg = aggr,
            Channel = message.ChatHub.Id //here: replying to the same channel
        };
        await _publisher.Publish(notification);
        
        return new HandleResponse("OK");
    }

    public bool ShouldHandle(SlackMessage message)
    {
        return message.MentionsBot
    }
}
```


Similarly in an `RecurringAction`:

```
public class SampleRecurringAction : RecurringAction
{
    private readonly IEnumerable<IPublisher> _publishers;

    public SampleRecurringAction(IEnumerable<IPublisher> publishers,
        ILogger<SampleRecurringAction> logger,
        IOptionsSnapshot<CronOptions> options)
        : base(options,logger)
    {
        _publishers = publishers;
    }

    public override async Task Process()
    {
        foreach (var p in _publishers)
        {
            var notification = new Notification
            {
                Msg = $"Sample message",
                IconEmoji = ":cake:",
                Channel = "#somechannel"
            };
            await p.Publish(notification);
        }
    }
}
```

