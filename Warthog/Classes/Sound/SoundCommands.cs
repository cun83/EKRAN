using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Discord.Audio;
using System.Diagnostics;
using Warthog.Classes.Sound;

namespace Warthog.Classes
{
    public class SoundCommands : ModuleBase
    {
        private CommandService _service;

        public SoundCommands(CommandService service)           /* Create a constructor for the commandservice dependency */
        {
            _service = service;
        }

        private Process CreateStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {path} -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(ffmpeg);
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            var ffmpeg = CreateStream(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = client.CreatePCMStream(AudioApplication.Mixed);
            await output.CopyToAsync(discord);
            await discord.FlushAsync();
        }

        private async Task SendAsyncYT(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            var ffmpeg = CreateLinkStream(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = client.CreatePCMStream(AudioApplication.Mixed);
            await output.CopyToAsync(discord);
            await discord.FlushAsync();
        }

        public Process CreateLinkStream(string url)
        {
            Process currentsong = new Process();

            //clean up the links... youtube-dl barfs if any get parameters except the v=[videoid] parameter are present

            var cleanUrl = YoutubeVideoLinkScrubber.GetCleanWatchUrlFromFromUrl(url);

            currentsong.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe -o - {cleanUrl} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            currentsong.Start();
            return currentsong;
        }


        //public async Task LeaveAudio(IGuild guild)
        //{
        //    IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
        //    IAudioClient client;
        //    if (ConnectedChannels.TryRemove(guild.Id, out client))
        //    {
        //        await client.StopAsync();
        //        //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
        //    }
        //}
        /// 
        /// Global helper functions (exist in every command module until I know how to seperate them)
        /// 
        public async Task DeleteCommandMessage()
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have enough permissions to manage messages - Please fix that.`");
                return;
            }
            await Context.Message.DeleteAsync();
        }

        ///
        /// Sound commands
        /// 
        /// <summary>
        /// Nod to upstream humor :)
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        [Command("wdyltf", RunMode = RunMode.Async)]
        [Alias("WhereDidYouLearnToFly")]
        public async Task WhereDidYouLearnToFly(IVoiceChannel channel = null)
        {
            await DeleteCommandMessage();
            Console.WriteLine($"{DateTime.Now} [Audit] Sound: {Context.User.Username} ran a sound command.");

            // Get the audio channel
            channel = (Context.User as IVoiceState).VoiceChannel;
            if (channel == null) { await Context.User.SendMessageAsync("User must be in a voice channel."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();
            await SendAsync(audioClient, @"Soundeffects\WhereDidYouLearnToFly.wav");
            await audioClient.StopAsync();
        }


        [Command("PlayYT", RunMode = RunMode.Async)]
        public async Task Yttest(string VideoURL, IVoiceChannel channel = null)
        {
            Console.WriteLine($"{DateTime.Now} [Audit] Sound: {Context.User.Username} ran the YouTube command.");

            await DeleteCommandMessage();
            // Get the audio channel
            channel = (Context.User as IVoiceState).VoiceChannel;
            if (channel == null) { await Context.User.SendMessageAsync("User must be in a voice channel."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();
            await SendAsyncYT(audioClient, VideoURL);
            await audioClient.StopAsync();
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task Stop(IVoiceChannel channel = null)
        {
            Console.WriteLine($"{DateTime.Now} [Audit] Sound: {Context.User.Username} ran the sound stop command.");

            await DeleteCommandMessage();
            //// Get the audio channel
            channel = (Context.User as IVoiceState).VoiceChannel;
            if (channel == null) { await Context.User.SendMessageAsync("User must be in a voice channel."); return; }

            var audioClient = await channel.ConnectAsync();

            await audioClient.StopAsync();
        }

        [Command("wep")]
        public async Task Wep()
        {

            Console.WriteLine($"{DateTime.Now} [Audit] Pictures: {Context.User.Username} ran a picture command.");
            await DeleteCommandMessage();
            /*
                        if (119019602574442497 != Context.User.Id)
                        {
                            // Secrurity check
                            //await ReplyAsync("This is a debug command, you cannot use it!");
                            return;
                        }
            */

            await ReplyAsync("https://cdn.discordapp.com/attachments/182225129680404480/409384412602433546/unknown.png");

        }
    }
}
