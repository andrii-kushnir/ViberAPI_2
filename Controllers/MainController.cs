using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ViberAPI.Models;
using System.Net.Http;
using Newtonsoft.Json;
using WordsMatching;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Net;
using Models;
using System.Linq.Expressions;
using System.Threading;
using NLog;
using System.Configuration;

namespace ViberAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        //private static List<UserSQL> usersSQL = UserFromSQL.Do();

        static Logger Logger = LogManager.GetCurrentClassLogger();
        private const string setWebhook = "https://chatapi.viber.com/pa/set_webhook";

        private const string myId = "bKyXpm0suyQAc5TDr3/LcQ==";
        private const string tarasId = "rDuY+VPsZlpsbzbvNcnrHg==";
        private const string sachaId = "YbB+K3NrZ9mGSUVUnhvsqA==";
        private const string olegId = "Ew5DLPPlDtfXLRSmqllspg==";

        private const string myId_inMyBot = "A1uI5qBer7dkTSZX1h4aKg==";
        private const string olegId_inMyBot = "VtqX85PBCiexN3dnQQJDCA==";

        private const string null_number = "000000000000";


        public MainController()
        {
        }

        // GET main/viber.ars.ua
        [HttpGet("{url}")]
        //[HttpGet]
        public async Task<ActionResult<String>> Get(string url)
        {
            //ssh -R 80:localhost:13666 nokey@localhost.run
            //ngrok http 13666
            if (ConfigurationManager.AppSettings.Get("ipAddress") == "192.168.4.147")
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Viber-Auth-Token", Program.authToken);

                url = "https://" + url + "/main";
                var webhook = @"{""url"":""" + url + @""",""send_name"": true,""send_photo"": true}";
                var response = await httpClient.PostAsync(setWebhook, new StringContent(webhook));
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<WebhookResponse>(responseJson);
                if (result.Status != ErrorCode.Ok)
                {
                    return "НЕ ВСТАНОВЛЕНО веб-хук: \"" + url + "\"  бо " + result.Status.ToString();
                }
                return "Встановлено веб-хук: " + url;
            }
            else
                return "У вас немає прав встановити веб-хук";
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ViberImputMessage messageObj)
        {
            if (!ModelState.IsValid || messageObj.@event == "webhook")
                return Ok();

            var message = (ViberImputMessage)messageObj;
            string query;
            ViberClient user;
            UserViber userViber;
            bool notIdentify;
            MessageSend messageSend;

            switch (message.@event)
            {
                case "conversation_started":
                    user = message.user;
                    if (String.IsNullOrEmpty(message.context))
                    { // прийшов на бот по прямому посиланню
                        userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.DirectLink);
                        notIdentify = userViber?.phone == null;
                        Logger.Info($"New client - {user.name} ChatId - {user.id}");
                        messageSend = MessageSend.MessageStartMain(notIdentify, "Вітаємо в будівельному супермаркеті АРС-Кераміка!"); // SM[123]
                    }
                    else if (message.context.Length == 12)
                    { // прийшов по інвайту зі Спутника
                        userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Pool, message.context);
                        Logger.Info($"New client(from Pool) - {user.name} Tel - {message.context} ChatId - {user.id} ");
                        messageSend = MessageSend.MessagePoolStart(message); // PS[531]T + context
                        //if (message.context == "380123456789")
                        //{
                        //    if (СheckMooClicks(user?.id, "SP")) break;
                        //    SetMooClicks(user.id, "SP");
                        //    messageSend = MessageSend.MessageStartPrac(); // SP[1234]
                        //}
                    }
                    else if ((message.context.Length == 5 || message.context.Length == 17) && message.context.Substring(0, 5) == "ARSUA")
                    {
                        Logger.Info($"New client(from Site) - {user.name} Tel - {message.context} ChatId - {user.id} ");
                        if (message.context.Length == 5)
                        {
                            userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Site);
                            messageSend = MessageSend.MessageActivateBot($"Вітаємо! Щоб запустити чат-бот АРС, натисніть кнопку «Активувати бот», або введіть свій номер телефону у форматі 380ххххххххх. Гарного дня та вдалих покупок!(smiley)");
                        }
                        else //(message.context.Length == 17)
                        {
                            userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Site, message.context.Substring(5, 12));
                            messageSend = MessageSend.MessageStartMain(false, "Вітаємо в будівельному супермаркеті АРС-Кераміка!"); // SM[123]
                        }
                    }
                    else if ((message.context.Length == 5 || message.context.Length >= 17 ) && message.context.Substring(0, 5) == "BUHNT")
                    {
                        Logger.Info($"New client(from Buhnet) - {user.name} Tel - {message.context} ChatId - {user.id} ");
                        if (message.context.Length == 5)
                        {
                            userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Buhnet);
                            messageSend = MessageSend.MessageActivateBot($"Вітаємо! Щоб запустити чат-бот АРС, натисніть кнопку «Активувати бот», або введіть свій номер телефону у форматі 380ххххххххх. Гарного дня та вдалих покупок!(smiley)");
                        }
                        else //(message.context.Length >= 17)
                        {
                            userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Buhnet, message.context.Substring(5, 12));
                            messageSend = MessageSend.MessageStartMain(false, "Вітаємо в будівельному супермаркеті АРС-Кераміка!"); // SM[123]
                            if (message.context.Length > 17)
                            {
                                var operatorStr = message.context.Substring(17);
                                if (int.TryParse(operatorStr, out int operatorCode))
                                    await UserManager.Current.AttachOperator(userViber, operatorCode);
                            }
                        }
                    }
                    else if (message.context.Length == 17 && message.context.Substring(0, 5) == "WORKS")
                    {
                        Logger.Info($"New worker - {user.name} Tel - {message.context} ChatId - {user.id} ");
                        {
                            userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Worker, message.context.Substring(5, 12));
                            messageSend = MessageSend.MessageWorksInvite($"Вітаємо в будівельному супермаркеті АРС-Кераміка!\nСюди Вам будуть приходити повідомелння.\nНатисніть на кнопку нижче для підписки на Бота"); // WI
                            //messageSend = new MessageSend()
                            //{
                            //    sender = new Sender() {name = "АРС-бот"},
                            //    type = "text",
                            //    text = $"Вітаємо в будівельному супермаркеті АРС-Кераміка!\nСюди Вам будуть приходити повідомелння.\nНатисніть на кнопку нижче для підписки на Бота"
                            //};
                        }
                    }
                    else
                    {
                        return Ok();
                    }
                    var requestJson = JsonConvert.SerializeObject(messageSend, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return Ok(requestJson);
                case "subscribed":
                    UserManager.Current.SetResubscribed(message.user_id);
                    Logger.Info($"Resubscribed, ChatId - {message.user_id}");
                    break;
                case "message":
                    switch (message.message.type)
                    {
                        case "picture":
                        case "video":
                        case "file":
                            {
                                user = message.sender;
                                userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Unknown);
                                var file_name = message.message.file_name;
                                var file_url = message.message.media;
                                if (string.IsNullOrEmpty(file_name))
                                    file_name = Guid.NewGuid().ToString();
                                switch (message.message.type)
                                {
                                    case "picture":
                                        await HandlerManager.Current.AddAndSendFileAsync(userViber, file_url, file_name, ChatMessageTypes.ImageFromViber);
                                        break;
                                    case "video":
                                        await HandlerManager.Current.AddAndSendFileAsync(userViber, file_url, file_name, ChatMessageTypes.VideoFromViber);
                                        break;
                                    case "file":
                                        await HandlerManager.Current.AddAndSendFileAsync(userViber, file_url, file_name, ChatMessageTypes.FileFromViber);
                                        break;
                                }
                                break;
                            }
                        case "sticker":
                            break;
                        case "contact":
                            {
                                user = message.sender;
                                userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Unknown);
                                if (message.message?.contact?.phone_number != null)
                                {
                                    if (message.message.contact.phone_number.Length > 12)
                                        userViber.phone = message.message.contact.phone_number.Substring(message.message.contact.phone_number.Length - 12);
                                    else
                                        userViber.phone = message.message.contact.phone_number;
                                    await UserManager.Current.SetPhoneAsync(userViber);
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, "Клієнт. Надав номер телефону.", ChatMessageTypes.Menu);
                                    DataProvider.Current.GetClientFromSQL(userViber);
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    if (userViber.buhnetName == null)
                                        await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartMain(false, "Вітаємо в АРС! Поки що ви не є нашим клієнтом, приєднуйтесь до нас!")); // SM[123]
                                    else
                                        await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartMain(false, $"Вітаємо {userViber.buhnetName}! Тепер ви можете переглянути свої замовлення або отримати іншу інформацію як клієнт АРС.")); // SM[123]
                                    Logger.Info($"Надав телефон - {message.message.contact.phone_number}, ChatId - {user.id}");
                                }
                            }
                            break;
                        case "text":
                            if (String.IsNullOrEmpty(message.message.text))
                                break;
                            user = message.sender;
                            userViber = UserManager.Current.AddOrFindUserViber(user, InviteType.Unknown);
                            notIdentify = userViber?.phone == null;

                            if (Regex.IsMatch(message.message.text, @"MENU#PS\dT\d{12}"))
                            {
                                var pool = Convert.ToInt32(message.message.text.Substring(7, 1));
                                var phone_number = message.message.text.Substring(9, 12);
                                if (phone_number != null_number)
                                {
                                    if (notIdentify)
                                    {
                                        userViber.phone = phone_number;
                                        await UserManager.Current.SetPhoneAsync(userViber, false);
                                    }
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    Logger.Info($"Pool client - {user.name}, Phone - {phone_number}, ChatId - {user.id}");
                                    DataProvider.Current.ClientSubscribedSQL(user, phone_number);

                                    //var name = Regex.Replace(user.name, @"'", @"''");
                                    //query = $"EXECUTE [dbo].[us_Viber_ClientSubscribed] '{phone_number}', '{name}', '{user.id}'";
                                    //DataProvider.Current.Enqueue100(query);
                                }
                                else
                                {
                                    // Це повідомлення послане з SQL, бо я вже маю id цього клієнта
                                    // Я шлю нульовий номер, тому що клієнт вже підписаний і він є в базі і його номер відомий.
                                }
                                Logger.Info($"Poll is {pool}, ChatId - {user.id}");
                                DataProvider.Current.ClientPoolSQL(user, pool);
                                //query = $"EXECUTE [dbo].[us_Viber_ClientPool] '{user.id}', {pool}, '{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}'";
                                //DataProvider.Current.Enqueue100(query);
                                if (userViber.inviteType == InviteType.Pool)
                                {
                                    await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", $"Додайте коментар");
                                }
                                else
                                {
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartMain(false, $"Дякуємо за оцінку.")); // SM[123]
                                }
                                break;
                            }

                            if (userViber.inviteType == InviteType.Pool)
                            {
                                var text = Regex.Replace(message.message.text, @"'", @"''");
                                query = $"EXECUTE [dbo].[us_Viber_ClientText] '{user.id}', '{text}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}'";
                                DataProvider.Current.Enqueue100(query);
                                await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", $"Дякуємо за відгук.");
                                break;
                            }

//#warning Це тимчасово "для мене":
//                            if (message.message.text == "go" || message.message.text == "Go")
//                            {
//                                await HandlerManager.Current.SendClearKeyboardAsync(user.id);
//                                messageSend = MessageSend.MessageStartMain(notIdentify); // SM[123]
//                                await HandlerManager.Current.SendKeyboardMessageAsync(user.id, messageSend);
//                                break;
//                            }

                            //Статистика по інвайтам:
                            if (message.message.text.Length >= 4 && message.message.text.Substring(0, 4).ToLower() == "stat" && (user.id == myId_inMyBot || user.id == myId || user.id == tarasId || user.id == sachaId || user.id == olegId))
                            {
                                var sklad = message.message.text.ToLower() == "stat" ? 0 : Convert.ToInt32(message.message.text.Substring(4, 3));
                                await GetStatistic(user.id, sklad);
                                break;
                            }

                            //Надав номер телефону як текст:
                            if (notIdentify && userViber.dateCreate >= new DateTime(2023, 05, 09))
                            {
                                if ((message.message.text.Length == 10 || message.message.text.Length == 12) && Transliteration.IsDigitsOnly(message.message.text))
                                {
                                    if (message.message.text.Length == 10) message.message.text = "38" + message.message.text;
                                    userViber.phone = message.message.text;
                                    await UserManager.Current.SetPhoneAsync(userViber);
                                    DataProvider.Current.GetClientFromSQL(userViber);
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    if (userViber.buhnetName == null)
                                        await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartMain(false, "Вітаємо в АРС! Поки що ви не є нашим клієнтом, приєднуйтесь до нас!")); // SM[123]
                                    else
                                        await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartMain(false, $"Вітаємо {userViber.buhnetName}! Тепер ви можете переглянути свої замовлення або отримати іншу інформацію як клієнт АРС.")); // SM[123]
                                    Logger.Info($"Надав телефон - {message.message.text}, ChatId - {user.id}");
                                }
                                else
                                {
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageActivateBot($"Телефон вказано некоректно. Будь ласка, надішліть ваш номер у форматі 380ххххххххх(чи 0ххххххххх), або поділіться за допомогою кнопки:"));
                                }
                                break;
                            }

                            //Запрошення працівника АРС:
                            if (Regex.IsMatch(message.message.text, @"MENU#WI"))
                            {
                                await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                break;
                            }

                            //Попереднє меню:
                            string previousMenu = null;
                            if (message.message.text.Length >= 5 && message.message.text.Substring(0, 5) == "MENU#")
                            {
                                if (HandlerManager.Current.СheckMooClicks(user?.id, message.message.text)) break;
                                previousMenu = HandlerManager.Current.SetMenuClicks(user.id, message.message.text);
                            }
                            else
                            {
                                previousMenu = HandlerManager.Current.SetMenuClicks(user.id);
                            }

                            //Натиснута кнопка меню:
                            if (Regex.IsMatch(message.message.text, @"MENU#MM"))
                            {
                                if (message.message.text == "MENU#MM")
                                {
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, "Клієнт. Головне меню.", ChatMessageTypes.Menu, false);
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartMain(notIdentify));
                                    HandlerManager.Current.ClearMenuClicks(user.id);
                                }
                                break;
                            }
                            if (Regex.IsMatch(message.message.text, @"MENU#EC"))
                            {
                                if (message.message.text == "MENU#EC")
                                {
                                    await Conversation.EndClient(userViber, true);
                                    HandlerManager.Current.ClearMenuClicks(user.id);
                                }
                                break;
                            }
                            if (Regex.IsMatch(message.message.text, @"MENU#PT[0-9]"))
                            {
                                await HandlerManager.Current.AddAndSendMessageAsync(userViber, message.message.text.Substring(7, 1), ChatMessageTypes.MarkOperator, false);
                                await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartMain(notIdentify));
                                HandlerManager.Current.ClearMenuClicks(user.id);
                                break;
                            }
                            if (Regex.IsMatch(message.message.text, @"MENU#SM[0-9]"))
                            {
                                if (message.message.text == "MENU#SM1")
                                {
                                    await Conversation.ClientInit(userViber);
                                    //await HandlerManager.Current.AddAndSendMessageAsync(userViber, "Клієнт. Поговорити з оператором.", ChatMessageTypes.Menu, false);
                                    //if (UserManager.Current.IsOnlineOperator())
                                    //{ 
                                    //    await HandlerManager.Current.SendFindOperatorAsync(userViber);
                                    //    await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", $"Йде пошук оператора....");
                                    //    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    //    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageEndСonversation());
                                    //}
                                    //else
                                    //{
                                    //    UserManager.Current.AddNightClient(userViber);
                                    //    await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", $"Менеджери нашої компанії набираються сил перед новим робочим днем. Залиште своє повідомлення і наші оператори зроблять все можливе, щоб вирішити Ваше питання якнайшвидше!\nНаш графік роботи: пн - пт 9:00 - 18:00, сб - нд 9:00 - 17:00. Дякуємо за розуміння 💚");
                                    //    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    //    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageMainMenu());
                                    //}
                                }
                                if (message.message.text == "MENU#SM2")
                                {
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, "Клієнт. Подивився свої замовлення", ChatMessageTypes.Menu);
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    Logger.Info($"Orders, ChatId - {user.id}");

                                    if (notIdentify)
                                    {
                                        await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", "Для отримання інформації про замовлення потрібно ідентифікувати себе");
                                    }
                                    else
                                    {
                                        var orders = DataProvider.Current.MyOrders(userViber.phone);
                                        if (orders.Count == 0)
                                        {
                                            await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", "Нажаль у вас немає замовлень");
                                        }
                                        else
                                        {
                                            foreach (var order in orders)
                                            {
                                                await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", order);
                                            }
                                        }
                                    }
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartMain(notIdentify)); // SM[123]
                                }
                                if (message.message.text == "MENU#SM3")
                                {
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, "Клієнт. Скарга/Пропозиція", ChatMessageTypes.Menu);
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    Logger.Info($"Complaint/Offer, ChatId - {user.id}");
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageComplaintOffer()); // CO[12]
                                }
                                break;
                            }
                            if (Regex.IsMatch(message.message.text, @"MENU#CO[0-9]"))
                            {
                                if (message.message.text == "MENU#CO1")
                                {
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, "Клієнт. Скарга", ChatMessageTypes.Menu);
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    Logger.Info($"Complaint, ChatId - {user.id}");
                                    await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", $"Напишіть скаргу нижче 👇🏻");
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageMainMenu());
                                }
                                if (message.message.text == "MENU#CO2")
                                {
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, "Клієнт. Пропозиція", ChatMessageTypes.Menu);
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    Logger.Info($"Offer, ChatId - {user.id}");
                                    await HandlerManager.Current.SendMessageAsync(user.id, "АРС-бот", $"Напишіть пропозицію нижче 👇🏻");
                                    await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageMainMenu());
                                }
                                break;
                            }
                            if (Regex.IsMatch(message.message.text, @"MENU#SP[0-9]"))
                            {
                                await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                if (!(message.message.text == "MENU#SP3"))
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessageStartPrac()); // SP[1234]
                                else
                                    await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessagePracJakist()); // PJ[012345]

                                break;
                            }
                            if (Regex.IsMatch(message.message.text, @"MENU#PJ[0-9]"))
                            {
                                await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                await HandlerManager.Current.SendKeyboardMessageAsync(user.id, MessageSend.MessagePracVdov()); // PV[012345]
                                break;
                            }
                            if (Regex.IsMatch(message.message.text, @"MENU#PV[0-9]"))
                            {
                                await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                                await HandlerManager.Current.SendMessageAsync(user.id, "Сахно Дмитро", $"Дякую вам дуже за відгук.");
                                break;
                            }
                            //Насіслано текстове повідомлення:
                            await HandlerManager.Current.SendClearKeyboardAsync(user.id);
                            switch (previousMenu)
                            {
                                case "MENU#CO1":
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, message.message.text, ChatMessageTypes.Complaint, false);
                                    break;
                                case "MENU#CO2":
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, message.message.text, ChatMessageTypes.Offer, false);
                                    break;
                                default:
                                    await HandlerManager.Current.AddAndSendMessageAsync(userViber, message.message.text, ChatMessageTypes.MessageFromViber);
                                    break;
                            }
                            Logger.Info($"Text - {message.message.text}, ChatId - {user.id}");
                            break;
                    }
                    break;
                case "action":
                    break;
                case "seen":
                    await HandlerManager.Current.SendSeenMessageAsync(message.message_token, DateTime.Now);
                    break;
                case "delivered":
                    await HandlerManager.Current.SendDeliveredMessageAsync(message.message_token, DateTime.Now);
                    break;
                case "unsubscribed":
                    UserManager.Current.SetUnsubscribed(message.user_id);
                    Logger.Info($"Unsubscribed, ChatId - {message.user_id}");
                    break;
                case "webhook":
                    break;
                default:
                    break;
            }
            return Ok();
        }

        //private async Task OldSqlVersion(MessageImput message)
        //{
        //    string query;
        //    Keyboard keyboard_start;
        //    switch (message.message.text)
        //    {
        //        case "ST1":
        //            var usersSQLApproximate = GetApproximateUsers(message.sender.name);
        //            keyboard_start = new Keyboard()
        //            {
        //                Type = "keyboard",
        //                BgColor = "#FFFFFF",
        //                InputFieldState = "hidden",
        //                Buttons = new List<Button>()
        //            {
        //                new Button
        //                {
        //                    Columns = 6,
        //                    Rows = 1,
        //                    Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">1. {usersSQLApproximate[0].LastName} {usersSQLApproximate[0].FirstName} {usersSQLApproximate[0].Surname}</font></font></b>",
        //                    ActionBody = "ST1_NO1",
        //                    BgColor = "#1E662D"
        //                },
        //                new Button
        //                {
        //                    Columns = 6,
        //                    Rows = 1,
        //                    Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">2. {usersSQLApproximate[1].LastName} {usersSQLApproximate[1].FirstName} {usersSQLApproximate[1].Surname}</font></font></b>",
        //                    ActionBody = "ST1_NO2",
        //                    BgColor = "#1E662D"
        //                },
        //                new Button
        //                {
        //                    Columns = 6,
        //                    Rows = 1,
        //                    Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">3. {usersSQLApproximate[2].LastName} {usersSQLApproximate[2].FirstName} {usersSQLApproximate[2].Surname}</font></font></b>",
        //                    ActionBody = "ST1_NO3",
        //                    BgColor = "#1E662D"
        //                },
        //                new Button
        //                {
        //                    Columns = 6,
        //                    Rows = 1,
        //                    Text = "<b><font color=\"#4DFAAC\"><font size=\"24\">Тут мене немає</font></font></b>",
        //                    ActionBody = "ST1_NO4",
        //                    BgColor = "#1E662D"
        //                }
        //            }
        //            };
        //            await SendKeyboardMessageAsync(message.sender.id, "АРС-бот", keyboard_start, "Ідентифікуйте себе:");
        //            break;
        //    }
        //}

        //private bool СheckMooClicks(string id, string action)
        //{
        //    if (id == null) return false;
        //    var result = clicks.Exists(c => c.id == id && c.action == action && (DateTime.Now - c.dateTime).TotalMilliseconds < 1500);
        //    return result;
        //}

        //private string SetMenu(string id, string action = null)
        //{
        //    string result = null;
        //    if (id != null)
        //    {
        //        var click = clicks.FirstOrDefault(c => c.id == id);
        //        if (click != null)
        //        {
        //            result = click.action;
        //            click.action = action ?? "text";
        //            click.dateTime = DateTime.Now;
        //        }
        //        else
        //        {
        //            clicks.Add(new Click { id = id, action = action ?? "text", dateTime = DateTime.Now });
        //        }
        //    }
        //    return result;
        //}

        //public async void SendMainMenu(object obj)
        //{
        //    var noActivity = clicks.Where(c => (DateTime.Now - c.dateTime).TotalMilliseconds > intervalClear).ToList();
        //    foreach (var click in noActivity)
        //    {
        //        var userViber = UserManager.Current.FindUserViber(click.id);
        //        if (userViber != null)
        //            await Conversation.EndClient(userViber, true);
        //        click.action = "no activity";
        //    }
        //    clicks.RemoveAll(c => c.action == "no activity");
        //}

        #region Statistic
        public async Task GetStatistic(string userId, int sklad)
        {
            var msg = "";
            var statist = DataProvider.Current.GetStat1(sklad);
            msg += "Клікнули - " + statist.First(s => s.Name == "CLICKED").Count.ToString() + " (" + statist.First(s => s.Name == "CLICKED").Rate.ToString() + "%)" + "\n";
            msg += "Прочитали - " + statist.First(s => s.Name == "READ").Count.ToString() + " (" + statist.First(s => s.Name == "READ").Rate.ToString() + "%)" + "\n";
            msg += "Не прочитали - " + statist.First(s => s.Name == "DELIVERED").Count.ToString() + " (" + statist.First(s => s.Name == "DELIVERED").Rate.ToString() + "%)" + "\n";
            msg += "Доставка не підтверджена - " + statist.First(s => s.Name == "PENDING").Count.ToString() + " (" + statist.First(s => s.Name == "PENDING").Rate.ToString() + "%)" + "\n";
            msg += "Не доставлено - " + statist.First(s => s.Name == "VIBER_UNDELIVERED_STATE").Count.ToString() + " (" + statist.First(s => s.Name == "VIBER_UNDELIVERED_STATE").Rate.ToString() + "%)" + "\n";
            msg += "Час доставки вийшов - " + (statist.First(s => s.Name == "VIBER_EXPIRED_STATE")).Count.ToString() + " (" + statist.First(s => s.Name == "VIBER_EXPIRED_STATE").Rate.ToString() + "%)" + "\n";
            msg += "Немає вайберу - " + statist.First(s => s.Name == "VIBER_UNKNOWN_USER").Count.ToString() + " (" + statist.First(s => s.Name == "VIBER_UNKNOWN_USER").Rate.ToString() + "%)" + "\n";
            msg += "Всього - " + statist.Sum(s => s.Count);
            await HandlerManager.Current.SendMessageAsync(userId, sklad == 0 ? "Інвайти(усі):" : statist[0].Namesk + ":", msg);

            msg = "";
            var statist2 = DataProvider.Current.GetStat2(sklad);
            if (statist2.Any(s => s.Nrating == 5))
                msg += "Відмінно - " + statist2.First(s => s.Nrating == 5).Count.ToString() + "\n";
            if (statist2.Any(s => s.Nrating == 3))
                msg += "Номально - " + statist2.First(s => s.Nrating == 3).Count.ToString() + "\n";
            if (statist2.Any(s => s.Nrating == 1))
                msg += "Погано - " + statist2.First(s => s.Nrating == 1).Count.ToString() + "\n";
            msg += "Всього - " + statist2.Sum(s => s.Count);
            await HandlerManager.Current.SendMessageAsync(userId, "Рейтинг:", msg);
        }
        #endregion

        private static object locker = new object();
        static List<UserSQL> GetApproximateUsers(string user)
        {
            List<UserSQL> result = null;
            //lock (locker)
            //{
            //    result = usersSQL//.Where(u => u.Viber == 0)
            //                        .Select(u =>
            //                        {
            //                            var match = new MatchsMaker(Transliteration.Translit(u.LastName), Transliteration.Translit(user));
            //                            u.Vaga = match.GetScore();
            //                            return u;
            //                        })
            //                        .OrderByDescending(u => u.Vaga)
            //                        .Take(3)
            //                        .ToList();
            //}
            return result;
        }
    }
}
