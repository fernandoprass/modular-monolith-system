using Myce.Response.Messages;

namespace Shared.Domain.Messages;

public class FailedToRecordDataError : ErrorMessage
{
   public FailedToRecordDataError() : base("FailedToRecordDataError", "Failed to record data.") { }
}

public class NotFoundError : ErrorMessage
{
   public NotFoundError() : base("NotFoundError", "The requested resource was not found.") { }

   public NotFoundError(string entity) : base("NotFoundDetailedError", "{entity} not found.")
   {
      AddVariable("entity", entity);
   }
}

public class UnauthorizedAccessError : ErrorMessage
{
   public UnauthorizedAccessError() : base("UnauthorizedAccessError", "You do not have permission to access this resource.") { }
}