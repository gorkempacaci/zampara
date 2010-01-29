using System;

namespace Zampara
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ZamparaGame game = new ZamparaGame())
            {
                game.Run();
            }
        }
    }
}

