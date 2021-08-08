using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace NyaaBot.Commands {
    internal class Moderation : BaseCommandModule {
        [Command("whois"), Description("Whois a selected user.")]
        public async Task Whois(CommandContext context, [Description("Member to whois.")] DiscordMember member) {
            var format = "F";
            
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{member.Username}#{member.Discriminator}")
                .WithDescription(
                    $"Account was created on {member.CreationTimestamp.ToString(format, DateTimeFormatInfo.CurrentInfo)}\r\n" +
                    $"Account joined on {member.JoinedAt.ToString(format, DateTimeFormatInfo.CurrentInfo)}")
                .Build();

            await context.RespondAsync(embed);
        }

        [Command("prune"), RequirePermissions(Permissions.ManageMessages | Permissions.ReadMessageHistory), Description("Prunes messages.")]
        public async Task Prune(CommandContext context, [Description("Messages to prune.")] int count, [Description("Reason for delete.")] string reason = null) {
            var messages = context.Channel.GetMessagesAsync(count + 1).Result
                .Where(message => (DateTimeOffset.UtcNow - message.Timestamp).TotalDays <= 14)
                .ToList();
            
            await context.Channel.DeleteMessagesAsync(messages, reason);
            
            await context.RespondAsync($"Pruned {messages.LongCount() - 1} messages.");
        }

        [Command("purge"), RequirePermissions(Permissions.ManageChannels), Description("Purges the active channel.")]
        public async Task Purge(CommandContext context) {
            var guild = context.Guild;
            
            var channel = context.Channel;

            var position = channel.Position;

            await channel.DeleteAsync();
            
            channel = await guild.CreateChannelAsync(channel.Name, channel.Type, topic: channel.Topic, parent: channel.Parent);

            await channel.ModifyPositionAsync(position, "Purged channel.");
        }
        
        [Command("kick"), RequirePermissions(Permissions.KickMembers), Description("Kick a selected user.")]
        public async Task Kick(CommandContext context, [Description("Member to ban.")] DiscordMember member, [Description("Reason for kick.")] string reason = null) {
            await member.RemoveAsync(reason);

            await context.RespondAsync($"I would have kicked <@{member.Id}> but I don't fucking know how!");
        }
        
        [Command("ban"), RequirePermissions(Permissions.BanMembers), Description("Bans a selected user.")]
        public async Task Ban(CommandContext context, [Description("User to ban.")] DiscordMember member, [Description("Reason for ban.")] string reason = null) {
            await member.BanAsync(0, reason);
            
            await context.RespondAsync($"Banned {member}.");
        }
    }
}