using IAM.Application.Contracts;
using IAM.Application.Orchestrators;
using IAM.Application.Services;
using IAM.Application.Validators;
using IAM.Domain.Interfaces;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using IAM.Infrastructure;
using IAM.Infrastructure.QueryRepositories;
using IAM.Infrastructure.Repositories;
using IAM.Infrastructure.Security;
using IAM.Infrastructure.UoW;
using Shared.Application.Contracts;

namespace IAM.API.Configure;

public static class DependencyInjection
{
   public static void Configure(WebApplicationBuilder builder)
   {
      RegisterUserContext(builder);

      RegisterRepositories(builder);

      RegisterOrchestrators(builder);

      RegisterServices(builder);

      RegisterValidators(builder);
   }

   private static void RegisterRepositories(WebApplicationBuilder builder)
   {
      builder.Services.AddScoped<IIamUnitOfWork, IamUnitOfWork>();

      builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
      builder.Services.AddScoped<ICustomerQueryRepository, CustomerQueryRepository>();
      builder.Services.AddScoped<IRoleRepository, RoleRepository>();
      builder.Services.AddScoped<IRoleQueryRepository, RoleQueryRepository>();
      builder.Services.AddScoped<IUserRepository, UserRepository>();
      builder.Services.AddScoped<IUserQueryRepository, UserQueryRepository>();
   }

   private static void RegisterOrchestrators(WebApplicationBuilder builder)
   {
      builder.Services.AddScoped<IRegisterOrchestrator, ResgisterOrchestrator>();
   }

   private static void RegisterServices(WebApplicationBuilder builder)
   {
      builder.Services.AddScoped<IAuthService, AuthService>();
      builder.Services.AddScoped<ICustomerService, CustomerService>();
      builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
      builder.Services.AddScoped<IRoleService, RoleService>();
      builder.Services.AddScoped<IUserService, UserService>();
   }

   private static void RegisterValidators(WebApplicationBuilder builder)
   {
      builder.Services.AddScoped<ICustomerValidator, CustomerValidator>();
      builder.Services.AddScoped<IUserValidator, UserValidator>();
      builder.Services.AddScoped<IRoleValidator, RoleValidator>();
   }

   private static void RegisterUserContext(WebApplicationBuilder builder)
   {
      builder.Services.AddHttpContextAccessor();
      builder.Services.AddScoped<IUserContext, AspNetUserContext>();
   }
}
