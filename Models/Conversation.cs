using Models;
using Models.Messages.Requests;
using Models.Messages.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberAPI.Models
{
    public class Conversation
    {
        private UserViber _user;
        private UserArsenium _oper;

        private static readonly List<Conversation> Conversations = new List<Conversation>();
        private static readonly object lockConversations = new object();

        public Conversation(UserViber user, UserArsenium oper)
        {
            _user = user;
            _oper = oper;
            lock (lockConversations)
            {
                Conversations.Add(this);
            }
        }

        public Conversation(UserViber user) : this(user, null) { }
        public Conversation(UserArsenium oper) : this(null, oper) { }

        private static Conversation FindConversation(UserViber user)
        {
            var conversation = Conversations.FirstOrDefault(c => c._user.Id == user.Id);
            return conversation;
        }

        private static void ConversationRemove(Conversation conversation)
        {
            lock (lockConversations)
                Conversations.Remove(conversation);
        }

        public static async Task ClientInit(UserViber user)
        {
            var conversation = new Conversation(user);
            await HandlerManager.Current.AddAndSendMessageAsync(user, "Клієнт. Поговорити з оператором.", ChatMessageTypes.Menu, false);
            if (UserManager.Current.IsOnlineOperator())
            {
                await HandlerManager.Current.SendFindOperatorAsync(user);
                await UserManager.Current.SendNewConversationToAdminsAsync(user);
                await HandlerManager.Current.SendMessageAsync(user.idViber, "АРС-бот", $"Йде пошук оператора....");
                await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageEndСonversation());
            }
            else
            {
                UserManager.Current.AddNightClient(user);
                await HandlerManager.Current.SendMessageAsync(user.idViber, "АРС-бот", $"Менеджери нашої компанії набираються сил перед новим робочим днем. Залиште своє повідомлення і наші оператори зроблять все можливе, щоб вирішити Ваше питання якнайшвидше!\nНаш графік роботи: пн - пт 9:00 - 18:00, сб - нд 9:00 - 17:00. Дякуємо за розуміння 💚");
                await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageMainMenu());
            }
        }

        public static async Task<bool> JoinedOperator(UserViber user, UserArsenium oper)
        {
            Conversation conversation;
            lock (lockConversations)
            {
                conversation = FindConversation(user);
                if (conversation == null || conversation._oper != null)
                    return false;
                else
                    conversation._oper = oper;
            }
            if (user.operatoId == Guid.Empty)
                await UserManager.Current.AttachOperator(user, oper);
            await UserManager.Current.SendToAllOperatorsAsync(new ClientBusyRequest(user));
            await HandlerManager.Current.AddAndSendMessageAsync(user, $"Під'єднався оператор {oper.Name}", ChatMessageTypes.OperatorConnect);
            await HandlerManager.Current.SendMessageAsync(user.idViber, "АРС-бот", $"Під'єднався оператор {oper.Name}");
            await HandlerManager.Current.SendMessageAsync(user.idViber, oper.Name, "Що бажаєте? Напишіть 👇🏻", oper.Avatar);
            await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
            await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageEndСonversation());
            return true;
        }

        public static async Task OperatorSendMessage(UserViber user, UserArsenium oper, ChatMessage message)
        {
            switch(message.ChatMessageType)
            {
                case ChatMessageTypes.MessageToViber:
                    await HandlerManager.Current.SendMessageAndSaveAsync(user.idViber, oper, message);
                    await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                    await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageEndСonversation());
                    break;
                case ChatMessageTypes.LinkAsImageToViber:
                case ChatMessageTypes.ImageToViber:
                case ChatMessageTypes.VideoToViber:
                case ChatMessageTypes.FileToViber:
                    await HandlerManager.Current.SendFileAndSaveAsync(user.idViber, oper, message);
                    await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                    await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageEndСonversation());
                    break;
            }
            var conversation = FindConversation(user);
            if (conversation == null)
            {
                conversation = new Conversation(user, oper);
                if (user.inviteType != InviteType.Pool)
                {
                    await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                    await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageEndСonversation());
                }
                await UserManager.Current.SendNewConversationToAdminsAsync(user);
                UserManager.Current.DeleteNightClientsOperator(user);
            }
            await UserManager.Current.SendToAllOperatorsWithoutIAsync(new ClientBusyRequest(user), oper);
        }

        public static async Task EndOperator(UserViber user, bool withMark)
        {
            var conversation = FindConversation(user);
            if (conversation != null)
            {
                ConversationRemove(conversation);
                if (withMark)
                {
                    await HandlerManager.Current.AddAndSendMessageAsync(user, "Fix. Закінчення розмови. Оцінка.", ChatMessageTypes.Fix, true);
                    await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                    await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageStartMain(user?.phone == null, $"Оцініть спілкування з оператором:", true));
                }
                else
                {
                    await HandlerManager.Current.AddAndSendMessageAsync(user, "Fix. Закінчення розмови. Без оцінки.", ChatMessageTypes.Fix, false);
                    await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                    await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageStartMain(user?.phone == null));
                }
            }
            else
            {
                await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageStartMain(user?.phone == null));
            }
        }

        public static async Task EndClient(UserViber user, bool withMark)
        {
            var conversation = FindConversation(user);
            if (conversation != null)
            {
                ConversationRemove(conversation);
                if (withMark)
                {
                    await HandlerManager.Current.AddAndSendMessageAsync(user, "Клієнт. Закінчення розмови. Оцінка.", ChatMessageTypes.Menu, true);
                    await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                    await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageStartMain(user?.phone == null, $"Оцініть спілкування з оператором:", true));
                }
                else
                {
                    await HandlerManager.Current.AddAndSendMessageAsync(user, "Клієнт. Закінчення розмови по таймінгу. Без оцінки.", ChatMessageTypes.Menu, false);
                    await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                    await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageStartMain(user?.phone == null));
                    //if (click.action == "MENU#SM1")
                    //    await HandlerManager.Current.SendKeyboardMessageAsync(click.id, MessageSend.MessageStartMain(withIdentify, $"Оцініть спілкування з оператором:", true));
                }
            }
            else
            {
                await HandlerManager.Current.SendClearKeyboardAsync(user.idViber);
                await HandlerManager.Current.SendKeyboardMessageAsync(user.idViber, MessageSend.MessageStartMain(user?.phone == null));
            }
        }
    }

    public enum ConversationState
    {
        Init = 1,
        Сontinues = 2,
    }
}
