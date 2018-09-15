using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace SelfCareBot
{
    class Program
    {
        private static readonly ITelegramBotClient BotClient;
        private const string BotApiKey = "686437593:AAFt0ClaFzP4iQAIkARFg2H90c_mZ_a_DXI";
        private const string GoogleApiKey = "AIzaSyCbr_WfhlarDOxLPLtB8Jv9LGy8_EdWNOo";
        private const string GoogleApiUrl = "https://www.googleapis.com/customsearch/v1";
        private const string GoogleSearchEngineId = "013367013494666642248:ogkjoh-oih4";

        private const string VzhukhTemplate = @"　∧＿∧
（｡･ω･｡)つ━☆・*。{0}
⊂　　 ノ 　　　・゜+.
しーＪ　　　°。+ *´¨)";
        private static readonly Random Rng;

        static Program()
        {
            BotClient = new TelegramBotClient(BotApiKey);
            Rng = new Random(DateTime.Now.Millisecond);
        }

        static void Main(string[] args)
        {
            BotClient.OnMessage += OnBotMessageReceived;
            BotClient.StartReceiving();

            Console.WriteLine("Started listening. Press any key to stop listening");
            Console.ReadLine();

            BotClient.StopReceiving();
            Console.WriteLine("Listening stopped");
        }

        private static async void OnBotMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            var messageText = message?.Text;

            if (string.IsNullOrEmpty(messageText) || e.Message?.Type != MessageType.Text)
            {
                return;
            }

            string reply;

            var messageParts = messageText.Split(' ');
            switch (messageParts.FirstOrDefault()?.Replace("@selfcaremedbot", ""))
            {
                case "/talk":
                    reply =
                        $"Чятовец_ка {message.From.Username} попросил_а меня отправить следующее сообщение в чят: {string.Join(' ', messageParts.Skip(1))}";
                    await BotClient.SendTextMessageAsync(message.Chat.Id, reply, replyToMessageId: message.MessageId);
                    break;
                case "/cats":
                    var catUrl = GetRandomCatPicture();
                    await BotClient.SendTextMessageAsync(message.Chat.Id, $"Картинка с милыми котиками по вашему запросу: {catUrl}");
                    break;
                case "/vzhukh":
                    var name = messageParts.Skip(1).FirstOrDefault();
                    await BotClient.SendTextMessageAsync(message.Chat.Id,
                        $"Вам сделали вжух! \n" + string.Format(VzhukhTemplate, name));
                    break;
                default:
                    reply =
                        $"Привет, я Селф-кейр бот! Скоро я научусь делать что-то полезное. А пока что вы написали: {messageText}";
                    await BotClient.SendTextMessageAsync(message.Chat.Id, reply, replyToMessageId: message.MessageId);
                    break;
            }            

            Console.WriteLine($"Replied to message {message.MessageId}");
        }

        private static string GetRandomCatPicture()
        {
            var queryParams = new Dictionary<string, string>
            {
                ["key"] = GoogleApiKey,
                ["cx"] = GoogleSearchEngineId,
                ["q"] = "kittens",
                ["searchType"] = "image"
            };

            var service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = GoogleApiKey,
                ApplicationName = "SelfCareBot"
            });

            CseResource.ListRequest listRequest = service.Cse.List("kittens");
            listRequest.Cx = GoogleSearchEngineId;
            listRequest.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;

            var search = listRequest.Execute();

            var randomIndex = Rng.Next(0, search.Items.Count - 1);
            var randomItem = search.Items[randomIndex];

            return randomItem.Link;
        }
    }
}
