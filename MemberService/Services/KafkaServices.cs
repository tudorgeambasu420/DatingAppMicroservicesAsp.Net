using System;
using System.Text.Json;
using Confluent.Kafka;
using MemberService.DTOs;
using MemberService.Entities;
using MemberService.ExtensionMethods;
using MemberService.Repositories;
using Microsoft.OpenApi;

namespace MemberService.Services;

public class KafkaServices : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScope;

    public KafkaServices(IServiceScopeFactory serviceScopeFactory) : base()
    {
        _serviceScope = serviceScopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "members",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };


        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("UserCreatedEvent");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult == null)
                    {
                        continue;
                    }
                    var result = JsonSerializer.Deserialize<UserCreatedEventDTO>(consumeResult.Message.Value);
                    if (result is null)
                    {
                        throw new Exception("Something went wrong with the user/Kafka");
                    }
                    var member = result.ToMemberEntity();
                    using var scope = _serviceScope.CreateScope();
                    var memberRepo = scope.ServiceProvider.GetRequiredService<MemberRepository>();
                    var ok = await memberRepo.Create(member);
                    if (ok == null)
                    {
                        throw new Exception("Couldn't create the member");
                    }
                    else
                    {
                        consumer.Commit(consumeResult);
                        Console.Write($"Member was created with:\nuserId:{member.UserId}\nUsername:{member.UserName} ");
                    }


                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                }


            }
        }

        finally
        {
            consumer.Close();
        }

    }

}
