using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class MainProgram
    {
        public static void Main()
        {
            try
            {
                MainEngine GameEngine = new MainEngine();
                GameEngine.Run();
            }
            catch(Exception e)
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
