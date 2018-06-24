﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using SQLite;
using Gensearch.Scrapers;

namespace Gensearch
{
    class GenSearch
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TimeSpan timeSpan = new TimeSpan();
            
            if (args.Contains("--items") || args.Contains("--all") || args.Length == 0) {
                var itemManager = new Items();
                itemManager.GetItemList().Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                Console.WriteLine("\n\nFinished with items! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Reset();
                stopwatch.Start();
            }
            
            if (args.Contains("--monsters") || args.Contains("--all") || args.Length == 0) {
                var monManager = new Monsters();
                Console.WriteLine("Starting monster data retrieval.");
                monManager.GetMonsters().Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                Console.WriteLine("Done with monsters! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Reset();
                stopwatch.Start();
            }
            
            if (args.Contains("--quests") || args.Contains("--all") || args.Length == 0) {
                var questManager = new Quests();
                Console.WriteLine("Starting quest data retrieval.");
                questManager.GetQuests("http://mhgen.kiranico.com/quest/village").Wait();
                questManager.GetQuests("http://mhgen.kiranico.com/quest/guild").Wait();
                questManager.GetQuests("http://mhgen.kiranico.com/quest/arena").Wait();
                questManager.GetQuests("http://mhgen.kiranico.com/quest/training").Wait();
                questManager.GetQuests("http://mhgen.kiranico.com/quest/special-permit").Wait();
                timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                Console.WriteLine("Done with all quests! Took " + timeSpan.ToString("c") + ".\n\n");
                stopwatch.Reset();
                stopwatch.Start();
            }

            if (args.Contains("--weapons")) {
                var weaponManager = new Weapons();
                weaponManager.GetWeapons("http://mhgen.kiranico.com/greatsword").Wait();
            }
            stopwatch.Stop();
        }
    }
}