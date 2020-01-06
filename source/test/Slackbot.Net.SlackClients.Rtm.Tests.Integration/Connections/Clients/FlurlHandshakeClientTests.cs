﻿using System.Threading.Tasks;
using Shouldly;
using Slackbot.Net.SlackClients.Rtm.Connections.Clients;
using Slackbot.Net.SlackClients.Rtm.Connections.Clients.Handshake;
using Slackbot.Net.SlackClients.Rtm.Connections.Responses;
using Slackbot.Net.SlackClients.Rtm.Tests.Integration.Configuration;
using Xunit;

namespace Slackbot.Net.SlackClients.Rtm.Tests.Integration.Connections.Clients
{
    public class FlurlHandshakeClientTests
    {
        [Fact]
        public async Task should_perform_handshake_with_flurl()
        {
            // given
            var config = new ConfigReader().GetConfig();
            var client = new FlurlHandshakeClient(new ResponseVerifier());

            // when
            HandshakeResponse response = await client.FirmShake(config.Slack.ApiToken);

            // then
            response.ShouldNotBeNull();
            response.WebSocketUrl.ShouldNotBeEmpty();
        }
    }
}