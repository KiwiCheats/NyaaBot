using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NyaaBot.Models;

namespace NyaaBot {
    internal static class Program {
        internal static void Main(string[] args) {
            var configPath = Path.Join(AppContext.BaseDirectory, "config.json");

            if (!File.Exists(configPath)) {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new Config {
                    Prefix = String.Empty,
                    Token = String.Empty
                }, Formatting.Indented), Encoding.UTF8);
                
                Console.WriteLine("[*] created config.json");
                
                return;
            }

            Config config;

            try {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            }
            catch {
                Console.WriteLine("[-] failed to read config.json");
                
                return;
            }

            new BotInstance(config).RunBotAsync().GetAwaiter().GetResult();
        }
    }
}