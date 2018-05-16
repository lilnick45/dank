﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Dank.Discord.Labs
{
  class Program
  {
    private const string InnervationChannelId = "445719805807296532";
    private const string DankUserId = "445634069439578132";

    private static readonly string infoFile = @"PrivateDiscordInfo.txt";
    private static readonly string authToken = File.ReadAllLines(infoFile)[0];

    private static DiscordClient client;
    private static Commands commands;

    static void Main()
    {
      commands = new Commands(DateTimeOffset.UtcNow);

      using (client = new DiscordClient(DankUserId, authToken))
      {
        using (client.Events.Subscribe(
          next => Debug.WriteLine("RECEIVED: " + next),
          ex => Debug.WriteLine(ex),
          () => Debug.WriteLine("DONE")))
        using ((from message in client.UserMessages
                let response = commands.TryProcessCommand(message.Content)
                where response != null
                from _ in client.PostMessageAsync(message.ChannelId, response)
                select Unit.Default)
                .Subscribe())
        {
          client.Connect();

          Console.Write("> ");

          string line = null;
          while ((line = Console.ReadLine()).Length > 0)
          {
            var reply = client.PostMessageAsync(InnervationChannelId, line).Result;

            Console.WriteLine("RESULT: " + reply);
            Console.WriteLine();
            Console.Write("> ");
          }
        }
      }

      Console.ReadKey();
    }
  }
}
