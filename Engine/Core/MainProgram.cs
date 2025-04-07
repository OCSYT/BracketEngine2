using System;

namespace Engine
{
    public class MainProgram
    {
        public static void Main()
        {
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
            finally
            {
                Console.WriteLine("Press Enter to close the console.");
                Console.ReadLine();
            }
        }
    }
}
