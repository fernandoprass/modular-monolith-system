namespace Shared.Domain.Interfaces;

internal interface ISharedUnitOfWork : IUnitOfWork
{
   ILookupRepository Lookups { get; }
   IParameterRepository Parameters { get; }
   IParameterOverrideRepository ParameterOverrides { get; }
}
