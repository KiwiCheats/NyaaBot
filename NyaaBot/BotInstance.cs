using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.Logging;

using NyaaBot.Commands;
using NyaaBot.Models;

namespace NyaaBot {
    internal class BotInstance {
        internal BotInstance(Config config) {
            _config = config;
        }

        internal DiscordClient Client { get; set; }
        internal InteractivityExtension Interactivity { get; set; }
        internal CommandsNextExtension Commands { get; set; }

        internal async Task RunBotAsync() {
            var discordConfiguration = new DiscordConfiguration {
                Token = _config.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };
            
            this.Client = new DiscordClient(discordConfiguration);

            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;

            this.Client.UseInteractivity(new InteractivityConfiguration {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromMinutes(2)
            });

            var commandsConfiguration = new CommandsNextConfiguration {
                StringPrefixes = new[] {_config.Prefix},

                EnableDms = true,
                EnableMentionPrefix = true
            };

            this.Commands = this.Client.UseCommandsNext(commandsConfiguration);
            
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;

            this.Client.MessageCreated += (sender, args) => {
                return Task.CompletedTask;
            };
            
            this.Commands.RegisterCommands<Moderation>();

            await this.Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e) {
            sender.Logger.LogInformation("Bot is ready.");
            
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e) {
            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e) {
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e) {
            sender.Client.Logger.LogInformation($"Executed command {e.Command.Name}");
            
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e) {
            if (e.Exception is ChecksFailedException || e.Exception is UnauthorizedException) {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                var embed = new DiscordEmbedBuilder {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };

                await e.Context.RespondAsync(embed);
            }
        }

        private readonly Config _config;
    }
}