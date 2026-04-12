using Shared.Domain.Entities;
using System.Security.Cryptography;

namespace Shared.Domain.Interfaces;

internal interface IParameterRepository
{
   Task<Parameter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
   Task AddAsync(Parameter parameter, CancellationToken cancellationToken = default);
   void Update(Parameter parameter);
   Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
   Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
   Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

   Task<Parameter?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
 }
