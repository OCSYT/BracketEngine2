﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    internal class MainProgram
    {
        public static void Main()
        {
            MainEngine GameEngine = new MainEngine();
            GameEngine.Run();
        }
    }
}