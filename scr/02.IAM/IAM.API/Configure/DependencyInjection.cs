using IAM.Application.Contracts;
using IAM.Application.Orchestrators;
using IAM.Application.Services;
using IAM.Application.Validators;
using IAM.Domain.Interfaces;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using IAM.Infrastructure.DatabaseSeeder;
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

      builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
      builder.Services.AddScoped<IOrganizationQueryRepository, OrganizationQueryRepository>();
      builder.Services.AddScoped<IRoleRepository, RoleRepository>();
      builder.Services.AddScoped<IRoleQueryRepository, RoleQueryRepository>();
      builder.Services.AddScoped<IUserRepository, UserRepository>();
      builder.Services.AddScoped<IUserQueryRepository, UserQueryRepository>();
      builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
      builder.Services.AddScoped<IPermissionQueryRepository, PermissionQueryRepository>();
   }

   private static void RegisterOrchestrators(WebApplicationBuilder builder)
   {
      builder.Services.AddScoped<IRegisterOrchestrator, ResgisterOrchestrator>();
   }

   private static void RegisterServices(WebApplicationBuilder builder)
   {
      builder.Services.AddScoped<IAuthService, AuthService>();
      builder.Services.AddScoped<IOrganizationService, OrganizationService>();
      builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
      builder.Services.AddScoped<IRoleService, RoleService>();
      builder.Services.AddScoped<IUserService, UserService>();
      builder.Services.AddScoped<IPermissionService, PermissionService>();
   }

   private static void RegisterValidators(WebApplicationBuilder builder)
   {
      builder.Services.AddScoped<IOrganizationValidator, OrganizationValidator>();
      builder.Services.AddScoped<IUserValidator, UserValidator>();
      builder.Services.AddScoped<IRoleValidator, RoleValidator>();
      builder.Services.AddScoped<IPermissionValidator, PermissionValidator>();
   }

   private static void RegisterUserContext(WebApplicationBuilder builder)
   {
      builder.Services.AddHttpContextAccessor();
      builder.Services.AddScoped<IUserContext, AspNetUserContext>();
   }
}
