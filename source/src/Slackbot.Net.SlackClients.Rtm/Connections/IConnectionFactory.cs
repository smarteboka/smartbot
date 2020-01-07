﻿using System.Threading.Tasks;
using Slackbot.Net.SlackClients.Rtm.Connections.Clients.Handshake;
using Slackbot.Net.SlackClients.Rtm.Connections.Sockets;

namespace Slackbot.Net.SlackClients.Rtm.Connections
{
    internal interface IConnectionFactory
    {
        Task<IWebSocketClient> CreateConnectedWebSocketClient(string url);
        IHandshakeClient CreateHandshakeClient();
    }
}