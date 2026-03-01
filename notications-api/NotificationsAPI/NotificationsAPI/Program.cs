using Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationsAPI.Consumers;
using NotificationsAPI.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

# region DB
builder.Services.AddDbContext<NotificationsDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});
# endregion

# region MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<PaymentProcessedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"];
        var user = builder.Configuration["RabbitMq:Username"];
        var pass = builder.Configuration["RabbitMq:Password"];
        var vhost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";

        cfg.Host(host, vhost, h =>
        {
            h.Username(user);
            h.Password(pass);
        });
     
        cfg.Message<UserCreatedEventV1>(x =>
        {
            x.SetEntityName("fcg.users");
        });

        cfg.ReceiveEndpoint("notifications.user-created", e =>
        {
            e.ConfigureConsumeTopology = false;

            e.Bind("fcg.users", s =>
            {
                s.ExchangeType = "topic";
                s.RoutingKey = "v1.user-created";
            });

            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("notifications.payment-processed", e =>
        {
            e.ConfigureConsumeTopology = false;

            e.Bind("fcg.payments", s =>
            {
                s.ExchangeType = "topic";
                s.RoutingKey = "v1.payment-processed";
            });

            e.ConfigureConsumer<PaymentProcessedConsumer>(context);
        });
    });
});
# endregion

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotificationsDbContext>("notificationsdb");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "NotificationsAPI", status = "ok" }));

app.Run();