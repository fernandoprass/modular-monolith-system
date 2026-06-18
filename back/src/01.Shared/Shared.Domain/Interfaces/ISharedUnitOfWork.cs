namespace Shared.Domain.Interfaces;

internal interface ISharedUnitOfWork : IUnitOfWork
{
   IParameterRepository Parameters { get; }
   IParameterOverrideRepository ParameterOverrides { get; }
}