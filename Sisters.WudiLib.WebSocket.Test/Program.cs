﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var cqWebSocketEvent = new CqHttpWebSocketEvent("ws://[::1]:6700/event", "");
            var httpApiClient = new HttpApiClient("http://[::1]:5700", "");
            cqWebSocketEvent.ApiClient = httpApiClient;
            cqWebSocketEvent.MessageEvent += (api, e) =>
            {
                Console.WriteLine(e.Content.Raw);
                Console.WriteLine(api is null);
            };
            cqWebSocketEvent.FriendRequestEvent += (api, e) =>
            {
                return true;
            };
            cqWebSocketEvent.GroupInviteEvent += (api, e) =>
            {
                return true;
            };
            cqWebSocketEvent.AnonymousMessageEvent += (api, e) =>
            {
                Console.WriteLine("id|name|flag:{0}|{1}|{2}", e.Anonymous.Id, e.Anonymous.Name, e.Anonymous.Flag);
                api.BanMessageSource(e.GroupId, e.Source, 1);
            };
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Available: {0}, Listening {1}", cqWebSocketEvent.IsAvailable, cqWebSocketEvent.IsListening);
                }
            });
            Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            var cancellationTokenSource = new CancellationTokenSource();
            cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
            Console.ReadLine();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
            Task.Delay(-1).Wait();
        }
    }
}
