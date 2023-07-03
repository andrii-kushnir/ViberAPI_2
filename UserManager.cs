using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Models.Messages;
using Models.Messages.Requests;
using Models.Network;
using NLog;
using ViberAPI.Models;

namespace ViberAPI
{
    public class UserManager
    {
        public static UserManager Current { get; private set; }

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<User> UserList;
        private readonly List<UserViber> UserOffline;
        public static object lockUserOffline = new object();

        public UserManager()
        {
            Logger.Info($"Start UserManager...");
            if (Current == null)
                Current = this;

            UserList = DataProvider.Current.GetOperatorsSQL();
            UserOffline = new List<UserViber>();

            var newZeroUser = new UserViber(Guid.NewGuid(), "ZeroUser", "", UserTypes.Viber) 
            { 
                idViber = "", 
                subscribed = true, 
                messageList = new List<ChatMessage>()
            };
            UserList.Add(newZeroUser);
        }

        public UserViber AddOrFindUserViber(ViberClient viberClient, InviteType inviteType, string phone = null)
        {
            var userViber = FindUserViber(viberClient.id);
            if (userViber == null)
            {
                userViber = DataProvider.Current.GetClientFormIdSQL(viberClient.id, out Guid operGuid);
                if (userViber == null)
                {
                    userViber = new UserViber(Guid.NewGuid(), viberClient.name, viberClient.avatar, UserTypes.Viber)
                    {
                        idViber = viberClient.id,
                        language = viberClient.language,
                        country = viberClient.country,
                        primary_device_os = viberClient.primary_device_os,
                        device_type = viberClient.device_type,
                        phone = phone,
                        dateCreate = DateTime.Now,
                        messageList = new List<ChatMessage>(),
                        subscribed = true,
                        inviteType = inviteType
                    };
                    DataProvider.Current.SaveClientSQL(userViber);
                    DataProvider.Current.GetClientFromSQL(userViber);
                }
                else
                {
                    userViber.messageList = userViber.messageList.Where(msg => msg.ChatMessageType.MsgSendToOperator()).ToList();
                    if (operGuid != Guid.Empty)
                    {
                        userViber.operatoId = operGuid;
                        userViber.operatoName = FindUserArsenium(operGuid).Name;
                    }
                }
                UserList.Add(userViber);
            }
            return userViber;
        }

        public UserViber FindAndBdUserViber(Guid userGuid)
        {
            var user = FindUserViber(userGuid);
            if (user == null)
            {
                user = DataProvider.Current.GetClientFormIdSQL(userGuid, out Guid operGuid);
                if (user == null) return null; //На всякий випадок))
                user.messageList = user.messageList.Where(msg => msg.ChatMessageType.MsgSendToOperator()).ToList();
                if (operGuid != Guid.Empty)
                {
                    user.operatoId = operGuid;
                    user.operatoName = FindUserArsenium(operGuid).Name;
                }
                UserList.Add(user);
            }
            return user;
        }

        public UserViber FindUserViber(string userId)
        {
            var user = UserList.Where(us => us.UserType == UserTypes.Viber).Select(us => us as UserViber).FirstOrDefault(us => us.idViber == userId);
            return user;
        }

        public UserViber FindUserViber(Guid guid)
        {
            var user = UserList.FirstOrDefault(us => us.UserType == UserTypes.Viber && us.Id == guid) as UserViber;
            return user;
        }

        public UserViber FindUserViberFromPhone(string phone)
        {
            var user = UserList.Where(us => us.UserType == UserTypes.Viber).Select(us => us as UserViber).FirstOrDefault(us => us.phone == phone);
            if (user == null)
            {
                user = DataProvider.Current.GetClientFormPhoneSQL(phone, out Guid operGuid);

                if (user != null)
                {
                    user.messageList = user.messageList.Where(msg => msg.ChatMessageType.MsgSendToOperator()).ToList();
                    UserList.Add(user);
                    if (operGuid != Guid.Empty)
                    {
                        user.operatoId = operGuid;
                        user.operatoName = FindUserArsenium(operGuid).Name;
                    }
                }
            }
            return user;
        }

        public UserViber FindMessage(long token)
        {
            var user = UserList.Where(us => us.UserType == UserTypes.Viber).FirstOrDefault(us => (us as UserViber).messageList.Exists(msg => msg.Token == token));
            return user as UserViber;
        }

        public async Task SetPhoneAsync(UserViber user, bool sendOperator = true)
        {
            DataProvider.Current.SetClientPhoneSQL(user);
            if (sendOperator)
                await UserManager.Current.SendToOperatorsAsync(user, new MessageViberPhone(user));
        }

        public UserArsenium FindUserArsenium(Guid guid)
        {
            var user = UserList.FirstOrDefault(us => us.UserType == UserTypes.Asterium && us.Id == guid);
            return user as UserArsenium;
        }

        public UserArsenium FindUserArsenium(string login)
        {
            var user = UserList.Where(us => us.UserType == UserTypes.Asterium).Select(us => us as UserArsenium).FirstOrDefault(us => us.Login == login);
            return user;
        }

        public List<UserArsenium> GetOnlineOperator()
        {
            return UserList.Where(us => us.UserType == UserTypes.Asterium).Select(us => us as UserArsenium).Where(us => us.Online).ToList();
        }

        public bool IsOnlineOperator()
        {
            return UserList.Where(us => us.UserType == UserTypes.Asterium).Any(us => (us as UserArsenium)?.Online ?? false);
        }

        public UserArsenium GetAttachedOperator(UserViber userViber)
        {
            if (userViber.operatoId == Guid.Empty)
                return null;
            return UserList.FirstOrDefault(op => (op.UserType == UserTypes.Asterium && op.Id == userViber.operatoId)) as UserArsenium;
        }

        public async Task AttachOperator(UserViber userViber, UserArsenium oper)
        {
            if (userViber.operatoId == oper.Id)
                return;

            DeleteNightClientsOperator(userViber);
            await SendToAllOperatorsWithoutIAsync(new ClientBusyRequest(userViber), oper);

            userViber.operatoId = oper.Id;
            userViber.operatoName = oper.Name;

            await UserManager.Current.SendToAllOperatorsAsync(new AttachOperatorRequest(userViber));
            DataProvider.Current.SetAttachOperatorSQL(userViber, oper);
        }

        public void SetUnsubscribed(string userId)
        {
            var user = FindUserViber(userId);
            if (user != null)
                user.subscribed = false;
            DataProvider.Current.SetSubscribedSQL(userId, false);
        }

        public void SetResubscribed(string userId)
        {
            var user = FindUserViber(userId);
            if (user != null)
                user.subscribed = true;
            DataProvider.Current.SetSubscribedSQL(userId, true);
        }

        public void AddNightClient(UserViber user)
        {
            lock (lockUserOffline)
                if (!UserOffline.Any(u => u.Id == user.Id))
                    UserOffline.Add(user);
        }

        public List<UserViber> GetNightClients()
        {
            return UserOffline.ToList();
        }

        public void DeleteNightClientsOperator(UserViber user)
        {
            lock (lockUserOffline)
                if (UserOffline.Any(u => u.Id == user.Id))
                    UserOffline.RemoveAll(u => u.Id == user.Id);
        }

        public async Task DeleteAllNightClientsOperator(UserArsenium oper)
        {
            foreach (var user in UserOffline.Where(u => u.operatoId == oper.Id).ToList())
            {
                await SendToAllOperatorsWithoutIAsync(new ClientBusyRequest(user), oper);
            }
            lock (lockUserOffline)
                UserOffline.RemoveAll(u => u.operatoId == oper.Id);
        }

        public async Task SendNewConversationToAdminsAsync(UserViber user)
        {
            var admins = UserList.Where(us => us.UserType == UserTypes.Asterium).Select(us => us as UserArsenium).Where(oper => oper.Online && oper.Permission.IsRole(Permissions.PermissionRole.p_SeeAllUsers)).ToList();
            var message = new NewConversationRequest(user);
            foreach (var oper in admins)
            {
                var session = SessionManager.Current.FindSession(oper.Id);
                if (session != null)
                {
                    await session.SendAsync(message);
                }
            }
        }

        public async Task SendToOperatorsAsync(UserViber user, Message message)
        {
            var oper = GetAttachedOperator(user);
            if (oper == null || !oper.Online)
            {
                await SendToAllOperatorsAsync(message);
            }
            else
            {
                var session = SessionManager.Current.FindSession(oper.Id);
                if (session != null)
                {
                    await session.SendAsync(message);
                }
            }
        }

        public async Task SendToAllOperatorsAsync(Message message)
        {
            foreach (var oper in GetOnlineOperator())
            {
                var session = SessionManager.Current.FindSession(oper.Id);
                if (session != null)
                {
                    await session.SendAsync(message);
                }
            }
        }

        public async Task SendToAllOperatorsWithoutIAsync(Message message, UserArsenium operWithout)
        {
            foreach (var oper in GetOnlineOperator())
            {
                var session = SessionManager.Current.FindSession(oper.Id);
                if (session != null && operWithout.Id != oper.Id)
                    await session.SendAsync(message);
            }
        }

        public List<UserViber> GetLastClients(UserArsenium oper, int days, bool all = false)
        {
            if (all) Logger.Info($"GetLastClients do");
            var guids = DataProvider.Current.GetLastClientsSQL(oper.Id, days, all);
            var result = guids.Select(g => FindAndBdUserViber(g)).ToList();
            return result;
        }
    }
}
