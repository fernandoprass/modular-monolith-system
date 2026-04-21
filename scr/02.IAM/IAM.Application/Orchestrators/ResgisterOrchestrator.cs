using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Enums;
using IAM.Domain.Interfaces;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using Isopoh.Cryptography.Argon2;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.Messages;

namespace IAM.Application.Orchestrators;

public class ResgisterOrchestrator(
   ICustomerService customerService,
   ICustomerQueryRepository customerQueryRepository,
   IUserContext userContext,
   IUserRepository userRepository,
   IUserService userService,
   IIamUnitOfWork iamUnitOfWork) : BaseService(userContext), IRegisterOrchestrator
{
   private readonly ICustomerService _customerService = customerService;
   private readonly ICustomerQueryRepository _customerQueryRepository = customerQueryRepository;
   private readonly IUserRepository _userRepository = userRepository;
   private readonly IUserService _userService = userService;
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;

   public async Task<Result<UserDto>> RegisterUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
   {
      var customerDto = await _customerQueryRepository.GetByIdAsync(request.CustomerId, cancellationToken);

      var customerExists = customerDto is not null;

      var result = await _userService.CreateUserAsync(request, customerExists, cancellationToken);

      if (result.IsSuccess)
      {
         result.Data.CustomerName = customerDto.Name;
      }

      return result;
   }
   public async Task<Result<CustomerDto>> RegisterCustomerAsync(CustomerCreateRequest customerCreate, CancellationToken cancellationToken = default)
   {
      var customerValidateResult = await _customerService.ValidateCreateCustomerAsync(customerCreate, cancellationToken);
      var userValidateResult = await _userService.ValidateUserForNewCustomerAsync(customerCreate.User, cancellationToken);

      var result = Result.Merge(customerValidateResult, userValidateResult);

      var customer = Customer.Create(
         customerCreate.Type,
         customerCreate.Type.Equals(CustomerType.Company) ? customerCreate.Code : _customerService.GetRandomCode(),
         customerCreate.Type.Equals(CustomerType.Company) ? customerCreate.Name : customerCreate.User.Name,
         customerCreate.Description
      );

      if (result.HasError)
      {
         return Result<CustomerDto>.Failure(result.Messages);
      }

      var user = User.Create(
       customerCreate.User.Name,
       customerCreate.User.Email,
       Argon2.Hash(customerCreate.User.Password),
       DateTime.UtcNow.AddDays(30),
       customer.Id
      );

      customer.CreatedBy = user.Id;

      await _iamUnitOfWork.Customers.AddAsync(customer, cancellationToken);
      await _iamUnitOfWork.Users.AddAsync(user, cancellationToken);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      return Result<CustomerDto>.Success(customer.ToCustomerDto());
   }

   public async Task<Result> DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(id, async (ct) =>
      {
         var customer = await _iamUnitOfWork.Customers.GetByIdAsync(id, ct);
         if (customer == null)
         {
            return Result.Failure(new NotFoundError(IamConst.Entity.Customer));
         }

         await _iamUnitOfWork.Customers.DeleteAsync(id, ct);

         var users = await _userRepository.GetByCustomerIdAsync(id, ct);
         foreach (var u in users)
         {
            await _iamUnitOfWork.Users.DeleteAsync(u.Id, ct);
         }

         await _iamUnitOfWork.SaveChangesAsync(ct);

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }
}
