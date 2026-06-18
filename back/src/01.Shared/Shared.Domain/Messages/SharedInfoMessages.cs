using Myce.Response.Messages;

namespace Shared.Domain.Messages;

public class SuccessInfo : InformationMessage
{
   public SuccessInfo() : base("SUCCESS", "Operation completed successfully.") { }
}
