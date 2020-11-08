using System;
using IDS.Lexik.WebService.Sdk.WaitBehaviour.Abstract;

namespace IDS.Lexik.WebService.Sdk.WaitBehaviour
{
  public class WaitBehaviourWindows : AbstractWaitBehaviour
  {
    public override void Wait()
    {
      while (true)
      {
        var command = Console.ReadLine();
        if (command == "quit" || command == "exit")
          break;
      }
    }
  }
}