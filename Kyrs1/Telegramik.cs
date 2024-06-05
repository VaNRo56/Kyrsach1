using Kyrs1.Models;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kyrs1
{
    public class TelegramBot
    {
        private static ITelegramBotClient botClient;
        private static database db = new database();

        public static async Task Main(string[] args)
        {
            botClient = new TelegramBotClient("7226916712:AAE6xcx68-H-5pOH2L4unT7vw2nnMo4XDMQ"); // Замените на ваш токен бота

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Hello! My name is {me.FirstName}");

            using var cts = new System.Threading.CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // Receive all update types
            };
            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
            cts.Cancel();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, System.Threading.CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
            {
                var messageText = update.Message.Text;
                var chatId = update.Message.Chat.Id;

                Console.WriteLine($"Received a message from {chatId}: {messageText}");

                string response = await ProcessMessage(messageText);
                var replyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "/createnote", "/updatenote" },
                    new KeyboardButton[] { "/deletenote", "/getallnotes" }
                })
                {
                    ResizeKeyboard = true
                };

                await botClient.SendTextMessageAsync(chatId, response, replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
            }
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, System.Threading.CancellationToken cancellationToken)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task<string> ProcessMessage(string message)
        {
            string[] parts = message.Split(' ');
            switch (parts[0].ToLower())
            {
                case "/createnote":
                    if (parts.Length < 3) return "Usage: /createnote title content";
                    await db.CreateNoteAsync(new NotesInputModel { Title = parts[1], Content = String.Join(" ", parts, 2, parts.Length - 2) });
                    return "Note created successfully.";
                case "/updatenote":
                    if (parts.Length < 4) return "Usage: /updatenote id title content";
                    await db.UpdateNoteAsync(new NotesInputModel { NoteNumber = int.Parse(parts[1]), Title = parts[2], Content = String.Join(" ", parts, 3, parts.Length - 3) });
                    return "Note updated successfully.";
                case "/deletenote":
                    if (parts.Length < 2) return "Usage: /deletenote id";
                    await db.DeleteNoteAsync(int.Parse(parts[1]));
                    return "Note deleted successfully.";
                case "/getallnotes":
                    var notes = await db.GetAllNotesAsync();
                    return notes.Count == 0 ? "No notes found." : String.Join("\n", notes.ConvertAll(note => $"ID: {note.NoteNumber}\n Title: {note.Title}\n Content: {note.Content}\n"));
                case "/getnews": // Новая команда для получения новостей
                    return await GetNewsAsync();
                default:
                    return "Unknown command. Available commands: /createnote, /updatenote, /deletenote, /getallnotes, /getnews";
            }
        }
        private static async Task<string> GetNewsAsync()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("https://gnews.io/api/v4/");
                    var response = await client.GetAsync($"search?q=example&lang=en&country=us&max=10&token={Constants.Api_Key}");

                    if (!response.IsSuccessStatusCode)
                    {
                        return "Failed to retrieve news. Status code: " + response.StatusCode;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var newsData = JsonConvert.DeserializeObject<ApiResponse>(content);

                    if (newsData.Articles == null || newsData.Articles.Count == 0)
                    {
                        return "No news found.";
                    }

                    // Формирование строки с новостями
                    var newsOutput = new StringBuilder();
                    foreach (var article in newsData.Articles)
                    {
                        newsOutput.AppendLine($"Title: {article.Title}\nURL: {article.Url}\n");
                        newsOutput.AppendLine("--------------------------------------------------");
                    }
                    return newsOutput.ToString();
                }
                catch (Exception ex)
                {
                    return $"Error retrieving news: {ex.Message}";
                }
            }
        }
    }
}
