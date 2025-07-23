using System;
using System.Diagnostics;
namespace Engine
{
    public class MainProgram
    {
        public static void Main()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            try
            {
                Engine.Game.Game GameEngine = new Engine.Game.Game();
                GameEngine.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
