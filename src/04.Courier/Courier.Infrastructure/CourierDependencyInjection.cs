using Courier.Domain;
using Courier.Domain.Interfaces.Repositories;
using Courier.Application.Contracts;
using Courier.Application.Services;
using Courier.Application.Validators;
using Courier.Infrastructure.BackgroundServices;
using Courier.Infrastructure.EmailSenders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Courier.Infrastructure.Repositories;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.ExceptionHandling;
using Shared.Infrastructure.Messaging;
using StackExchange.Redis;

namespace Courier.Infrastructure;

public static class CourierDependencyInjection
{
   public static IServiceCollection AddCourierInfrastructure(this IServiceCollection services, IConfiguration configuration)
   {
      ConfigureDbContext(configuration);
      ConfigureRedis(services, configuration);

      services.AddSingleton<CourierDbContext>();
      services.AddScoped<IEmailRepository, EmailRepository>();
      services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
      services.AddScoped<IEmailTemplateWriteRepository, EmailTemplateRepository>();
      services.AddScoped<IEmailService, EmailService>();
      services.AddScoped<IEmailValidator, EmailValidator>();
      services.AddScoped<IEmailTemplateService, EmailTemplateService>();
      services.AddScoped<IEmailTemplateRenderer, SimpleEmailTemplateRenderer>();
      services.AddScoped<IEmailTemplateValidator, EmailTemplateValidator>();
      services.AddScoped<IEmailOutboxService, EmailOutboxService>();
      services.AddScoped<IEmailSender, NoopEmailSender>();
      services.AddScoped<ICourierLogger, CourierLogger>();
      services.AddScoped<IEventPublisher, RedisEventPublisher>();
      services.AddScoped<IExceptionSystemLogPublisher, ExceptionSystemLogPublisher>();
      services.AddHostedService<CourierIndexInitializer>();
      services.AddHostedService<EmailRequestConsumer>();
      services.AddHostedService<EmailDeliveryWorker>();

      return services;
   }

   private static void ConfigureDbContext(IConfiguration configuration)
   {
      var connectionString = configuration.GetConnectionString(CourierConst.Database.ConnectionString);

      if (string.IsNullOrWhiteSpace(connectionString))
      {
         throw new InvalidOperationException("Courier MongoDB connection string is required.");
      }
   }

   private static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
   {
      var redisConnectionString = configuration.GetConnectionString("Redis");

      if (string.IsNullOrWhiteSpace(redisConnectionString))
      {
         throw new InvalidOperationException("Redis connection string is required for Courier.");
      }

      services.AddSingleton<IConnectionMultiplexer>(_ =>
      {
         var options = ConfigurationOptions.Parse(redisConnectionString);
         return ConnectionMultiplexer.Connect(options);
      });
   }
}
