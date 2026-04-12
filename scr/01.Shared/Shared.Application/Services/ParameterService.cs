using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.Mappers;
using Shared.Domain.Messages;
using System.Globalization;

namespace Shared.Application.Services;

internal class ParameterService(
    ISharedUnitOfWork unitOfWork,
    IUserContext userContext,
    IParameterValidator parameterValidator,
    IParameterRepository parameterRepository,
    IParameterOverrideRepository parameterOverrideRepository,
    IParameterQueryRepository parameterQueryRepository) : BaseService(userContext), IParameterService
{
   private readonly ISharedUnitOfWork _unitOfWork = unitOfWork;
   private readonly IParameterValidator _parameterValidator = parameterValidator;
   private readonly IParameterRepository _parameterRepository = parameterRepository;
   private readonly IParameterOverrideRepository _parameterOverrideRepository = parameterOverrideRepository;
   private readonly IParameterQueryRepository _parameterQueryRepository = parameterQueryRepository;

   #region Controller Methods
   public async Task<Result<ParameterDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var parameter = await _parameterQueryRepository.GetByIdAsync(id, cancellationToken);

      if (parameter == null) return Result<ParameterDto>.Failure(new NotFoundError(SharedConst.Entity.Parameter));

      return Result<ParameterDto>.Success(parameter);
   }

   public async Task<Result<IEnumerable<ParameterLiteDto>>> GetAsync(ParameterSearchRequest request, CancellationToken cancellationToken = default)
   {
      var requestInternal = request.ToInternal(_userContext.UserOwnerId, _userContext.UserId, _userContext.IsSystemAdmin);

      var parameters = await _parameterQueryRepository.GetAllAsync(requestInternal, cancellationToken);

      return Result<IEnumerable<ParameterLiteDto>>.Success(parameters);
   }

   public async Task<Result<ParameterValueDto>> GetValueAsync(string key, CancellationToken cancellationToken = default)
   {
      var parameter = await _parameterQueryRepository.GetValueAsync(key, _userContext.UserOwnerId, _userContext.UserId, cancellationToken);

      if (parameter == null) return Result<ParameterValueDto>.Failure(new NotFoundError(key));

      return Result<ParameterValueDto>.Success(parameter);
   }

   public async Task<Result> SaveOverrideValueAsync(Guid parameterId, ParameterOwnerUpdateRequest request, CancellationToken cancellationToken = default)
   {
      var parameter = await _parameterRepository.GetByIdAsync(parameterId, cancellationToken);
      var validation = _parameterValidator.ValidateOwnerUpdate(parameter, request);
      if (validation.HasError) return Result.Failure(validation.Messages);

      var ownerId = GetOwnerId(parameter.OverrideType);

      var parameterOverride = await _parameterOverrideRepository.GetByParameterIdAndOwnerIdAsync(parameterId, ownerId, cancellationToken);

      if (parameterOverride == null)
      {
         parameterOverride = ParameterOverride.Create(parameterId, ownerId, request.Value);

         await _unitOfWork.ParameterOverrides.AddAsync(parameterOverride, cancellationToken);
      }
      else
      {
         parameterOverride.Update(request.Value);
         _unitOfWork.ParameterOverrides.Update(parameterOverride);
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> DeleteOverrideValueAsync(Guid parameterId, CancellationToken cancellationToken = default)
   {
      var parameter = await _parameterRepository.GetByIdAsync(parameterId, cancellationToken);

      if (parameter == null) return Result<ParameterValueDto>.Failure(new NotFoundError(SharedConst.Entity.Parameter));

      var ownerId = GetOwnerId(parameter.OverrideType);
      var parameterOverride = await _parameterOverrideRepository.GetByParameterIdAndOwnerIdAsync(parameterId, ownerId, cancellationToken);

      if (parameterOverride == null) return Result.Failure(new NotFoundError(SharedConst.Entity.ParameterOverride));

      await _unitOfWork.ParameterOverrides.DeleteAsync(parameterOverride.Id, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(new SuccessInfo());
   }
   #endregion

   #region Internal Methods for Parameter Management
   public async Task<Result<ParameterDto>> CreateAsync(ParameterCreateRequest request, CancellationToken cancellationToken = default)
   {
      var keyExists = await _parameterQueryRepository.GetByModuleGroupAndKeyAsync(request.Module, request.Group, request.Name, cancellationToken);
      var validation = _parameterValidator.ValidateCreate(request, keyExists != null);
      if (validation.HasError) return Result<ParameterDto>.Failure(validation.Messages);

      var parameter = Parameter.Create(
          request.Module,
          request.Group,
          request.Name,
          request.Title,
          request.Description,
          request.Type,
          request.Value,
          request.ValidationRegex,
          request.ValidationErrorCustomMessage,
          request.ListItems,
          request.ExternalListEndpoint,
          request.OverrideType,
          request.IsVisible);

      await _unitOfWork.Parameters.AddAsync(parameter, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      return Result<ParameterDto>.Success(parameter.ToParameterDto());
   }

   public async Task<Result> UpdateAsync(Guid id, ParameterUpdateRequest request, CancellationToken cancellationToken = default)
   {
      var parameter = await _parameterRepository.GetByIdAsync(id, cancellationToken);
      var keyExists = await _parameterQueryRepository.GetByModuleGroupAndKeyAsync(request.Module, request.Group, request.Name, cancellationToken);
      var validation = _parameterValidator.ValidateUpdate(parameter != null, keyExists != null, request);
      if (validation.HasError) return Result.Failure(validation.Messages);

      parameter.Update(
          request.Module,
          request.Group,
          request.Name,
          request.Title,
          request.Description,
          request.Type,
          request.Value,
          request.ValidationRegex,
          request.ValidationErrorCustomMessage,
          request.ListItems,
          request.ExternalListEndpoint,
          request.OverrideType,
          request.IsVisible);

      _unitOfWork.Parameters.Update(parameter);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(new SuccessInfo());
   }

   public async Task<ParameterDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
   {
      var parameterKey = new ParameterKey(key);
      return await _parameterQueryRepository.GetByModuleGroupAndKeyAsync(parameterKey.Module, parameterKey.Group, parameterKey.Name, cancellationToken);
   }

   public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var parameter = await _parameterRepository.GetByIdAsync(id, cancellationToken);

      if(parameter == null) return Result.Failure(new NotFoundError(SharedConst.Entity.Parameter));

      await _unitOfWork.Parameters.DeleteAsync(id, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(new SuccessInfo());
   }

   public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return _parameterRepository.ExistsAsync(id, cancellationToken);
   }

   public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
   {
      return _parameterRepository.ExistsAsync(key, cancellationToken);
   }

   public Task<bool> GetBoolAsync(string key, CancellationToken cancellationToken = default) => GetAndParseAsync<bool>(key, bool.TryParse, cancellationToken);

   public Task<short> GetShortIntAsync(string key, CancellationToken cancellationToken = default) => GetAndParseAsync<short>(key, short.TryParse, cancellationToken);
   public Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default) => GetAndParseAsync<int>(key, int.TryParse, cancellationToken);
   public Task<long> GetLongIntAsync(string key, CancellationToken cancellationToken = default) => GetAndParseAsync<long>(key, long.TryParse, cancellationToken);

   public Task<double> GetDoubleAsync(string key, CancellationToken cancellationToken = default)
   => GetAndParseAsync<double>(key, (string s, out double result) =>
       double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result), cancellationToken);

   public Task<decimal> GetDecimalAsync(string key, CancellationToken cancellationToken = default)
      => GetAndParseAsync<decimal>(key, (string s, out decimal result) =>
          decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result), cancellationToken);

   public Task<DateTime> GetDateTimeAsync(string key, CancellationToken cancellationToken = default)
    => GetAndParseAsync<DateTime>(key, (string s, out DateTime result) =>
        DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out result), cancellationToken);

   public async Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
   {
      return await GetResolvedValueAsync(key, cancellationToken) ?? string.Empty;
   }

   private delegate bool TryParseDelegate<T>(string s, out T result);

   private async Task<T> GetAndParseAsync<T>(string key, TryParseDelegate<T> parser, CancellationToken cancellationToken)
   {
      var value = await GetResolvedValueAsync(key, cancellationToken);

      if (value == null || !parser(value, out var result))
      {
         throw new InvalidDataException($"Value '{value ?? "null"}' is invalid for parameter '{key}' and expected type {typeof(T).Name}");
      }

      return result;
   }

   private async Task<string?> GetResolvedValueAsync(string key, CancellationToken cancellationToken)
   {
      var parameter = await _parameterQueryRepository.GetValueAsync(key, _userContext.UserOwnerId, _userContext.UserId, cancellationToken);

      if (parameter == null)
      {
         throw new ArgumentNullException(nameof(key));
      }

      return parameter.Value;
   }

   private Guid GetOwnerId(ParameterOverrideType overrideType) 
   {
      return overrideType switch
      {
         ParameterOverrideType.UserOwnerId => _userContext.UserOwnerId,
         ParameterOverrideType.UserId => _userContext.UserId,
         _ => throw new InvalidOperationException("Invalid override type")
      };
   }
   #endregion
}