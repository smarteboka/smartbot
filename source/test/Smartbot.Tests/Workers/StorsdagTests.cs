using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Slackbot.Net.SlackClients.Http;
using Slackbot.Net.SlackClients.Http.Models.Requests.ChatPostMessage;
using Slackbot.Net.SlackClients.Http.Models.Responses.ChatPostMessage;
using Slackbot.Net.SlackClients.Http.Models.Responses.UsersList;
using Smartbot.Data.Storage.Events;
using Smartbot.Utilities.SlackQuestions;
using Smartbot.Utilities.Storsdager.RecurringActions;
using Xunit;
using Xunit.Abstractions;

namespace Smartbot.Tests.Workers
{
    public class StorsdagTests
    {
        private readonly ITestOutputHelper _output;

        public StorsdagTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip = "Already seeded")]
        public void SeedStorsdager()
        {
            // var eventStorage = EventsStorage();
            // var nesteStorsdager = new Timing().Get10NextOccurrences(Crons.LastThursdayOfMonthCron);
            // var culture = new CultureInfo("nb-NO");
            //
            // foreach (var storsdagDate in nesteStorsdager)
            // {
            //     var topic = $"Storsdag  {storsdagDate.Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture)}";
            //     await eventStorage.Save(new EventEntity(Guid.NewGuid(), EventTypes.StorsdagEventType)
            //     {
            //         Topic = topic,
            //         EventTime = storsdagDate
            //     });
            // }
            //
            // var knownStorsdag = new DateTime(2019,8,29);
            // var fetched = await eventStorage.GetEventsInRange(EventTypes.StorsdagEventType, knownStorsdag, knownStorsdag.AddDays(1));
            // Assert.NotEmpty(fetched);
            // Assert.Single(fetched);
        }

        [Fact]
        public void GetFutureInviteDates()
        {
            // var nesteStorsdager = Timing.GetNextOccurences(Crons.LastThursdayOfMonthCron, 12).ToArray();
            //
            // var inviteDates = Timing.GetNextOccurences(Crons.ThirdSaturdayOfMonth, 12).ToArray();
            //
            // for (var i = 0; i < 12; i++)
            // {
            //     var stors = nesteStorsdager[i];
            //     var inviteDay = inviteDates[i];
            //     _output.WriteLine($"{stors.Day - inviteDay.Day} days before stors. Storsdag {stors}. Invite {inviteDay}");
            // }
        }

        [Fact]
        public async Task GetStorsdag()
        {
            var knownStorsdag = new DateTime(2019,8,29);
            var fetched = await EventsStorage().GetEventsInRange(
                EventTypes.StorsdagEventType,
                knownStorsdag,
                knownStorsdag.AddDays(1));
            Assert.NotEmpty(fetched);
            Assert.Single(fetched);
        }

        [Fact]
        public async Task GetNextStorsdag()
        {
            var next = await EventsStorage().GetNextEvent(EventTypes.StorsdagEventType);
            Assert.NotNull(next);
        }

        [Fact(Skip = "no testdata")]
        public async Task DeleteTestData()
        {
            var ok = await EventsStorage().DeleteAll("testtype");
            //var ok = await Storage().DeleteAll(EventTypes.StorsdagEventType);
            Assert.True(ok);
        }

        [Fact]
        public async Task GetInvitations()
        {
            var storage = InvitationsStorage();
            var id = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            await storage.Save(new InvitationsEntity(id)
            {
                EventId = eventId.ToString(),
                EventTime = DateTime.UtcNow,
                EventTopic = "testtopic",
                SlackUserId = "some-user-id",
                Rsvp = RsvpValues.Invited
            });
            var next = await storage.GetInvitations();
            Assert.NotEmpty(next);

            var fetched = await storage.GetById(id.ToString());
            Assert.NotNull(fetched);

            var invitationsByEventId = await storage.GetInvitations(eventId.ToString());
            Assert.NotEmpty(invitationsByEventId);
            Assert.Equal("invited", invitationsByEventId.First().Rsvp);

            var rsvpNew = "yolo";
            await storage.Update(id.ToString(), rsvpNew);

            var afterUpdate = await storage.GetById(id.ToString());
            Assert.Equal("yolo", afterUpdate.Rsvp);
        }

        private static EventsStorage EventsStorage()
        {
            var accountKey = Environment.GetEnvironmentVariable("Smartbot_AzureStorage_AccountKey");
            var storage = new EventsStorage(accountKey);
            return storage;
        }

        private static InvitationsStorage InvitationsStorage()
        {
            var accountKey = Environment.GetEnvironmentVariable("Smartbot_AzureStorage_AccountKey");
            var storage = new InvitationsStorage(accountKey);
            return storage;
        }

        [Fact]
        public async Task SendsInvites()
        {
            var inviter = StorsdagRecurrer();
            await inviter.Process(CancellationToken.None);
        }

        private StorsdagInvitationRecurrer StorsdagRecurrer()
        {
            var inviter = CreateInviter();
            return new StorsdagInvitationRecurrer(inviter);
        }

        private StorsdagInviter CreateInviter()
        {
            var inviteLogger = A.Fake<ILogger<StorsdagInviter>>();
            var client = A.Fake<ISlackClient>();
            A.CallTo(() => client.UsersList()).Returns(new UsersListResponse { 
                Ok = true,
                Members = new []
                {
                    new User
                    {
                        Id = "U0EBWMGG4",
                        Name = "johnkors"
                    } 
                }
                
            });
            A.CallTo(() => client.ChatPostMessage(A<ChatPostMessageRequest>._)).Returns(new ChatPostMessageResponse() { 
                Ok = true
            });
            var questioner = new SlackQuestionClient(client);
            var eventsStorage = A.Fake<IEventsStorage>();
            var invitationsStorage = A.Fake<IInvitationsStorage>();
  
            return new StorsdagInviter(eventsStorage, invitationsStorage, client, questioner, inviteLogger);
        }

        [Fact]
        public async Task Reminder()
        {
            var inviter = CreateInviter();
            await inviter.SendRemindersToUnanswered();
        }
    }
}