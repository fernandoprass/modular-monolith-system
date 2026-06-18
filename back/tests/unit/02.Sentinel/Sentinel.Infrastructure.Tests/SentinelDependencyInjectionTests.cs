using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentinel.Infrastructure;
using Sentinel.Infrastructure.BackgroundServices;

namespace Sentinel.Infrastructure.Tests;

public class SentinelDependencyInjectionTests
{
   [Fact]
   public void AddSentinelInfrastructure_WhenHostedServicesAreEnabled_ShouldRegisterConsumers()
   {
      var services = new ServiceCollection();
      var configuration = CreateConfiguration(hostedServicesEnabled: true);

      services.AddSentinelInfrastructure(configuration);

      Assert.Contains(services, IsHostedService<AuditLogConsumer>);
      Assert.Contains(services, IsHostedService<SystemLogConsumer>);
   }

   [Fact]
   public void AddSentinelInfrastructure_WhenHostedServicesAreDisabled_ShouldNotRegisterConsumers()
   {
      var services = new ServiceCollection();
      var configuration = CreateConfiguration(hostedServicesEnabled: false);

      services.AddSentinelInfrastructure(configuration);

      Assert.DoesNotContain(services, IsHostedService<AuditLogConsumer>);
      Assert.DoesNotContain(services, IsHostedService<SystemLogConsumer>);
   }

   private static IConfiguration CreateConfiguration(bool hostedServicesEnabled)
   {
      return new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string?>
         {
            ["ConnectionStrings:SentinelDb"] = "mongodb://localhost:27017",
            ["ConnectionStrings:Redis"] = "localhost:6379",
            ["Sentinel:HostedServicesEnabled"] = hostedServicesEnabled.ToString()
         })
         .Build();
   }

   private static bool IsHostedService<TImplementation>(ServiceDescriptor descriptor)
   {
      return descriptor.ServiceType == typeof(IHostedService)
         && descriptor.ImplementationType == typeof(TImplementation);
   }
}
