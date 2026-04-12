using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Interfaces;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.Messages;

namespace IAM.Application.Services;

public class CustomerService(
    ICustomerQueryRepository customerQueryRepository,
    ICustomerRepository customerRepository,
    ICustomerValidator customerValidator,
    IIamUnitOfWork iamUnitOfWork,
    IUserContext userContext) : BaseService(userContext), ICustomerService
{
   private readonly ICustomerQueryRepository _customerQueryRepository = customerQueryRepository;
   private readonly ICustomerRepository _customerRepository = customerRepository;
   private readonly ICustomerValidator _customerValidator = customerValidator;   
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;

   public async Task<Result> ValidateCreateCustomerAsync(CustomerCreateRequest request, CancellationToken cancellationToken = default)
   {
      var codeExists = await _customerQueryRepository.ExistsByCodeAsync(request.Code, cancellationToken);

      var validation = _customerValidator.ValidateCreate(request, codeExists);
      if (validation.HasError)
      {
         return Result.Failure(validation.Messages);
      }

      return Result.Success();
   }

   public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await _customerQueryRepository.GetByIdAsync(id, cancellationToken);
   }

   public string GetRandomCode()
   {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      var random = new Random();
      return new string(Enumerable.Repeat(chars, IamConst.Customer.RandomCodeSize)
          .Select(s => s[random.Next(s.Length)]).ToArray());
   }

   public async Task<IEnumerable<CustomerDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
   {
      return await _customerQueryRepository.GetByNameAsync(name, cancellationToken);
   }

   public async Task<Result> UpdateAsync(Guid id, CustomerUpdateRequest request, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(id, async (ct) =>
      {
         var customer = await _customerRepository.GetByIdAsync(id, ct);
         var customerExists = customer is not null;

         var validation = _customerValidator.ValidateUpdate(request, customerExists);
         if (validation.HasError)
         {
            return Result.Failure(validation.Messages);
         }

         customer.Update(request.Name, request.Description, request.IsActive);

         return await CommitUpdateAsync(customer, ct);
      }, cancellationToken);
   }

   public async Task<Result> UpdateCodeAsync(Guid id, CustomerUpdateCodeRequest request, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(id, async (ct) =>
      {
         var customer = await _customerRepository.GetByCodeAsync(request.Code, ct);
         var newCodeExists = customer is not null;

         var validation = _customerValidator.ValidateUpdateCode(request, newCodeExists);
         if (validation.HasError)
         {
            return Result.Failure(validation.Messages);
         }
         
         customer = await _customerRepository.GetByIdAsync(id, ct);
         customer.Update(request.Code);

         return await CommitUpdateAsync(customer, ct);
      }, cancellationToken);
   }

   private async Task<Result> CommitUpdateAsync(Domain.Entities.Customer customer, CancellationToken cancellationToken)
   {
      _iamUnitOfWork.Customers.Update(customer);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(new SuccessInfo());
   }
}