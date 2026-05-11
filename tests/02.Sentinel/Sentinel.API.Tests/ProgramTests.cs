namespace Sentinel.API.Tests;

public class ProgramTests
{
   [Fact]
   public void ProgramTypeExists()
   {
      Assert.NotNull(typeof(global::Program));
   }
}
