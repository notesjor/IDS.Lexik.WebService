using System.Threading;
using IDS.Lexik.WebService.Sdk.WaitBehaviour.Abstract;

namespace IDS.Lexik.WebService.Sdk.WaitBehaviour
{
  public class WaitBehaviourLinux : AbstractWaitBehaviour
  {
    public override void Wait()
    {
      while (true)
        Thread.Sleep(25000);
    }
  }
}
