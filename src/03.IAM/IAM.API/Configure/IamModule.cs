using IAM.Application.Contracts;
using IAM.Application.Orchestrators;
using IAM.Application.Services;
using IAM.Application.Validators;
using IAM.Domain;
using IAM.Domain.Interfaces;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using IAM.Infrastructure;
using IAM.Infrastructure.QueryRepositories;
using IAM.Infrastructure.Repositories;
using IAM.Infrastructure.UoW;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Contracts;
using Shared.Infrastructure;
using Shared.Infrastructure.Security;

namespace IAM.API.Configure;

public static class IamModule
{
   public static IServiceCollection AddIamModule(
      this IServiceCollection services,
      IConfiguration configuration)
   {
      var connectionString = configuration.GetConnectionString(IamConst.Database.ConnectionString);

      services.AddDbContext<IamDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
      services.AddSharedInfrastructure(configuration, connectionString ?? string.Empty);

      services.AddHttpContextAccessor();
      services.AddScoped<IUserContext, AspNetUserContext>();

      services.AddScoped<IIamUnitOfWork, IamUnitOfWork>();
      services.AddScoped<IOrganizationRepository, OrganizationRepository>();
      services.AddScoped<IOrganizationQueryRepository, OrganizationQueryRepository>();
      services.AddScoped<IRoleRepository, RoleRepository>();
      services.AddScoped<IRoleQueryRepository, RoleQueryRepository>();
      services.AddScoped<IUserRepository, UserRepository>();
      services.AddScoped<IUserQueryRepository, UserQueryRepository>();
      services.AddScoped<IPermissionRepository, PermissionRepository>();
      services.AddScoped<IPermissionQueryRepository, PermissionQueryRepository>();

      services.AddScoped<IRegisterOrchestrator, RegisterOrchestrator>();

      services.AddScoped<IAuthService, AuthService>();
      services.AddScoped<IIamEventPublisher, IamEventPublisher>();
      services.AddScoped<IOrganizationService, OrganizationService>();
      services.AddScoped<IRoleService, RoleService>();
      services.AddScoped<IUserService, UserService>();
      services.AddScoped<IPermissionService, PermissionService>();
      services.AddScoped<IPermissionAuthorizationService, PermissionAuthorizationService>();

      services.AddScoped<IOrganizationValidator, OrganizationValidator>();
      services.AddScoped<IUserValidator, UserValidator>();
      services.AddScoped<IRoleValidator, RoleValidator>();
      services.AddScoped<IPermissionValidator, PermissionValidator>();

      return services;
   }
}
