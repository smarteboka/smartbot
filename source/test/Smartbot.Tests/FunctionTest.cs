using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Newtonsoft.Json;
using Oldbot.Utilities;
using Oldbot.Utilities.EventAPIModels;
using Oldbot.Utilities.SlackAPI.Extensions;
using Oldbot.Utilities.SlackAPIFork;
using SlackConnector.Models;
using Smartbot;

namespace Oldbot.OldFunction.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task EmptyBodyWorks()
        {
            var request = new SlackMessage
            {
                Text = null,
                User = new SlackUser()
            };

            var validateOldness = new OldnessValidator(new MockClient(), new NoopLogger());
            var response = await validateOldness.Validate(request);
            Assert.Equal("IGNORED", response);
        }
     
        
        [Fact]
        public async Task FindingUrlsWorks()
        {
            await TestIt("OLD", "la oss se litt på https://www.juggel.no", "det var noe https://www.juggel.no greier da");
            await TestIt("OLD", "<https://ilaks.no/na-kan-du-kjope-norsk-laks-pa-automater-i-singapore/>", "keen på https://ilaks.no/na-kan-du-kjope-norsk-laks-pa-automater-i-singapore");
            await TestIt("OLD", "https://ilaks.no/na-kan-du-kjope-norsk-laks-pa-automater-i-singapore", "alle vil ha laks https://ilaks.no/na-kan-du-kjope-norsk-laks-pa-automater-i-singapore");
            await TestIt("OLD", "GI meg gi meg lox <https://ilaks.no/na-kan-du-kjope-norsk-laks-pa-automater-i-singapore/>", "https://ilaks.no/na-kan-du-kjope-norsk-laks-pa-automater-i-singapore");
        }
        
        [Fact]
        public async Task NoText()
        {
            await TestIt("IGNORED", null, "");
            await TestIt("IGNORED", "", "");
        }

        [Fact]
        public async Task SkipsBotMessages()
        {
            var request = new SlackMessage
            {
                Text = "OLD! https://www.aftenposten.no/norge/i/L08awV/Haper-pa-mer-enn-ti-tusen-barn-og-unge-i-norske-klimastreiker?utm_source=my-unit-test",
                User = new SlackUser
                {
                    IsBot = true
                }
            };

            var validateOldness = new OldnessValidator(new MockClient(), new NoopLogger());

            var response = await validateOldness.Validate(request);
            Assert.Equal("BOT", response);
        }

        [Fact]
        public async Task DoesNotOldIfIsSameAuthor()
        {
            var mockClient = new MockClient();
            
            var existingMessage = new Event
            {
                Text = "A historic tale. I told you about http://db.no some time ago",
                User = "U0F3P72QM",
                Ts = "1550000000.000000" //
            };
            mockClient.SetSearchResponse(existingMessage);
        
            var request = new SlackMessage
            {
                Text = "Woot, me, U0F3P72QM, is repeating the url http://db.no some time later",
                User = new SlackUser
                {
                    Id = "U0F3P72QM"
                },
                Timestamp = 1660000000.000000,
                ChatHub = new SlackChatHub
                {
                    Id = "123"
                }
            };
            
            var validateOldness = new OldnessValidator(mockClient, new NoopLogger());

            var response = await validateOldness.Validate(request);
            Assert.Equal("OLD-BUT-SAME-USER-SO-IGNORING", response);            
        }

        [Fact]
        public void TestFinders()
        {
            AssertChannelRegex("tests", "say this is a thing <#CGY1XJRM1|tests> with other things");
        }

        [Fact]
        public void TestUrlFinder()
        {
            var expedtedUrl = "https://itunes.apple.com/no/podcast/272-build-tech-anett-andreassen-digitalisering-i-byggebransjen/id1434899825?i=1000431627999&amp;l=nb&amp;mt=2";
            var message = "https://itunes.apple.com/no/podcast/272-build-tech-anett-andreassen-digitalisering-i-byggebransjen/id1434899825?i=1000431627999&amp;l=nb&amp;mt=2";
            AssertUrlRegex(expedtedUrl, message);
            AssertUrlRegex("https://edition-m.cnn.com/2019/03/24/politics/mueller-report-release/index.html", "<https://edition-m.cnn.com/2019/03/24/politics/mueller-report-release/index.html>");
            AssertUrlRegex("https://www.linkedin.com/feed/update/urn:li:activity:6545200308086284288", "https://www.linkedin.com/feed/update/urn:li:activity:6545200308086284288");
        }

        private static void AssertChannelRegex(string expected, string input)
        {
            Assert.Equal(expected, RegexHelper.FindChannelName(input));
        }

        private static void AssertUrlRegex(string expected, string input)
        {
            Assert.Equal(expected, RegexHelper.FindStringURl(input));
        }

        private static async Task TestIt(string expected, string slackMessage, string historicMessage)
        {
            var mock = new MockClient();
            mock.SetSearchResponse( new Event {
                Channel = "CGWGZ90KV", // private channel #bottestsmore
                Text = historicMessage,
                Ts = "1552671370.000200", //older
                User = "another-smarting"
            });
            var validateOldness = new OldnessValidator(mock, new NoopLogger());

            var response = await validateOldness.Validate(new SlackMessage
            {
                Text = slackMessage,
                User = new SlackUser
                {
                    
                },
                ChatHub = new SlackChatHub
                {
                    Id = "123"
                }
            });
            Assert.Equal(expected, response);
        }
    }

    public class NoopLogger : ILogger<OldnessValidator>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            
        } 

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }


    public class MockClient : ISlackClient
        {
            public MockClient()
            {
                
            }
            
            public Task<SearchResponseMessages> SearchMessagesAsync(string query, SearchSort? sorting = null, SearchSortDirection? direction = null, bool enableHighlights = false, int? count = null, int? page = null)
            {
                return Task.FromResult(SearchResponse);
            }

            public SearchResponseMessages SearchResponse { get; set; }

            public Task<HttpResponseMessage> SendMessage(string channel, string message, string eventTs, string permalink)
            {
                var httpResponseMessage = new HttpResponseMessage
                {
                    Content = new StringContent("{}")
                };
                return Task.FromResult(httpResponseMessage);
            }

            public Task<HttpResponseMessage[]> AddReactions(string channelId, string thread_ts)
            {
                var httpResponseMessage = new []
                { 
                    new HttpResponseMessage
                    {
                        Content = new StringContent("{}"),
                        
                    }
                };
                return Task.FromResult(httpResponseMessage);
            }

            public void SetSearchResponse(Event newMessage)
            {
                SearchResponse = new SearchResponseMessages
                {
                    messages = new SearchResponseMessagesContainer
                    {
                        matches = new[]
                        {
                            new ContextMessage
                            {
                                text = newMessage.Text,
                                ts = newMessage.Ts,
                                user = newMessage.User
                            }
                        }
                    }
                };
            }
        }
}