using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using ViberAPI.Models;
using Models;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Concurrent;

namespace ViberAPI
{
    public class DataProvider
    {
        public static DataProvider Current { get; private set; }

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly SqlConnection Connection100 = new(connectionSql100);

        private const string connectionSql100 = @"Server=192.168.4.100; Database=InetClient; uid=NovaPoshta; pwd=NovaPoshta;";
        private const string connectionSql4 = @"Server=192.168.4.4; Database=WebShop; uid=viberbot; pwd=kF9M7G6n9i;";

        const int _MaximumLength = 200;
        private readonly ConcurrentQueue<string> _Queue = new();
        private readonly Thread _threadSQL100;

        public DataProvider()
        {
            Logger.Info($"Start DataProvider...");
            if (Current == null)
                Current = this;

            _threadSQL100 = new Thread(Dequeuer100)
            {
                Name = "Run100SQL"
            };
            _threadSQL100.Start();
        }

        private void Dequeuer100()
        {
            while (true)
            {
                if (_Queue.Count != 0)
                {
                    _Queue.TryPeek(out string query);
                    Run100SQL(query);
                    _Queue.TryDequeue(out query);
                }
                else
                    Thread.Sleep(200);

                //query = _Queue.Dequeue();
                //Run100SQL(query);
            }
        }

        public void Enqueue100(string query)
        {
            if (_Queue.Count >= _MaximumLength)
            {
                Logger.Error($"Переповнення черги запитів SQL.");
                _Queue.Clear();
            }

            _Queue.Enqueue(query);
        }

        public List<User> GetOperatorsSQL()
        {
            var result = new List<User>();
            var query = $"SELECT * FROM [dbo].[ArseniumUsers]";
            using (var connection = new SqlConnection(connectionSql100))
            {
                var command = new SqlCommand(query, connection);
                SqlDataReader reader = null;
                try
                {
                    connection.Open();
                    reader = command.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        var oper = new UserArsenium
                        {
                            Name = Convert.ToString(reader["name"]),
                            Login = Convert.ToString(reader["login"]),
                            Password = Convert.ToString(reader["password"]),
                            UserType = UserTypes.Asterium,
                            Avatar = Convert.ToString(reader["icon"]),
                            Online = false,
                            Active = Convert.ToBoolean(reader["active"]),
                            Codep = reader["codep"] == System.DBNull.Value ? 0 : Convert.ToInt32(reader["codep"])
                        };
                        
                        var Id = Convert.ToString(reader["guid"]);
                        oper.Id = String.IsNullOrWhiteSpace(Id) ? Guid.Empty : new Guid(Id);

                        if (reader["permission"] != System.DBNull.Value)
                            oper.SetPermission(Convert.ToString(reader["permission"]));

                        result.Add(oper);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.GetOperatorsSQL(): {ex.Message}");
                }
                finally
                {
                    reader?.Close();
                }

                return result;
            }
        }

        public void ClientSubscribedSQL(ViberClient user, string phone)
        {
            var name = Regex.Replace(user.name, @"'", @"''");
            var query = $"EXECUTE [dbo].[us_Viber_ClientSubscribed] '{phone}', '{name}', '{user.id}'";
            Enqueue100(query);
        }

        public void ClientPoolSQL(ViberClient user, int pool)
        {
            var query = $"EXECUTE [dbo].[us_Viber_ClientPool] '{user.id}', {pool}, '{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}'";
            Enqueue100(query);
        }

        public void SaveOperatorSQL(UserArsenium oper)
        {
            var query = $"UPDATE [dbo].[ArseniumUsers] SET [guid] = '{oper.Id}', [password] = '{oper.Password}' WHERE login = '{oper.Login}'";
            Enqueue100(query);
        }

        public UserViber GetClientFormIdSQL(string viberId, out Guid operatorGuid)
        {
            var query = $"SELECT TOP 1 V.*, A.guid as operatorGuid FROM [dbo].[ArseniumViberClients] V LEFT JOIN [dbo].[ArseniumUsers] A ON operatorId = A.id WHERE viberId = '{viberId}'";
            return GetClientSQL(query, out operatorGuid);
        }

        public UserViber GetClientFormIdSQL(Guid viberGuid, out Guid operatorGuid)
        {
            var query = $"SELECT TOP 1 V.*, A.guid as operatorGuid FROM [dbo].[ArseniumViberClients] V LEFT JOIN [dbo].[ArseniumUsers] A ON V.operatorId = A.id WHERE V.guid = '{viberGuid}'";
            return GetClientSQL(query, out operatorGuid);
        }

        public UserViber GetClientFormPhoneSQL(string phone, out Guid operatorGuid)
        {
            var query = $"SELECT TOP 1 V.*, A.guid as operatorGuid FROM [dbo].[ArseniumViberClients] V LEFT JOIN [dbo].[ArseniumUsers] A ON operatorId = A.id WHERE phone = '{phone}'";
            return GetClientSQL(query, out operatorGuid);
        }

        public UserViber GetClientSQL(string query, out Guid operatorGuid)
        {
            UserViber result = null;
            operatorGuid = Guid.Empty;
            int idDB = 0;
            using (var connection = new SqlConnection(connectionSql100))
            {
                var command = new SqlCommand(query, connection);
                SqlDataReader reader = null;
                try
                {
                    connection.Open();
                    reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        var oper = Convert.ToString(reader["operatorGuid"]);
                        if (!String.IsNullOrWhiteSpace(oper))
                            operatorGuid = new Guid(oper);

                        idDB = Convert.ToInt32(reader["id"]);

                        result = new UserViber()
                        {
                            Id = new Guid(Convert.ToString(reader["guid"])),
                            Name = Convert.ToString(reader["viberName"]),
                            Avatar = Convert.ToString(reader["avatar"]),
                            UserType = UserTypes.Viber,
                            idViber = Convert.ToString(reader["viberId"]),
                            phone = Convert.ToString(reader["phone"]),
                            dateCreate = reader["dateCreate"] == System.DBNull.Value ? new DateTime(2000, 1, 1) : Convert.ToDateTime(reader["dateCreate"]),
                            inviteType = (InviteType)Convert.ToInt32(reader["inviteType"]),
                            subscribed = Convert.ToBoolean(reader["subscribed"]),
                            buhnetName = reader["namep"] == System.DBNull.Value ? null : Convert.ToString(reader["namep"]),
                            codep = reader["codep"] == System.DBNull.Value ? 0 : Convert.ToInt32(reader["codep"]),
                            language = reader["language"] == System.DBNull.Value ? null : Convert.ToString(reader["language"]),
                            country = reader["country"] == System.DBNull.Value ? null : Convert.ToString(reader["country"]),
                            primary_device_os = reader["os"] == System.DBNull.Value ? null : Convert.ToString(reader["os"]),
                            device_type = reader["device"] == System.DBNull.Value ? null : Convert.ToString(reader["device"])
                        };
                        if (string.IsNullOrWhiteSpace(result.phone))
                            result.phone = null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.GetClientSQL(): {ex.Message}");
                }
                finally
                {
                    reader?.Close();
                }
            }

            if (result != null)
                result.messageList = GetMessagesSQL(idDB, result);

            return result;
        }

        public List<ChatMessage> GetMessagesSQL(int idDB, UserViber user)
        {
            var result = new List<ChatMessage>();
            var query = $"SELECT M.*, A.guid as operatorGuid, A.name as operatorName FROM [dbo].[ArseniumMessages] M LEFT JOIN [dbo].[ArseniumUsers] A ON A.id = [ownerId] WHERE [ownerId] = {idDB} OR (([type] = 1 OR [type] = 11 OR [type] = 12 OR [type] = 14 OR [type] = 16) AND [receiverId] = {idDB})";
            using (var connection = new SqlConnection(connectionSql100))
            {
                var command = new SqlCommand(query, connection);
                SqlDataReader reader = null;
                try
                {
                    connection.Open();
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var message = new ChatMessage()
                        {
                            MessageId = Guid.NewGuid(),
                            Token = Convert.ToInt64(reader["token"]),
                            ChatMessageType = (ChatMessageTypes)Convert.ToInt32(reader["type"]),
                            Text = Convert.ToString(reader["text"]),
                            DateCreate = reader["dateCreate"] == System.DBNull.Value ? new DateTime(2000, 1, 1) : Convert.ToDateTime(reader["dateCreate"]),
                            DateDelivered = reader["dateDelivered"] == System.DBNull.Value ? new DateTime(2000, 1, 1) : Convert.ToDateTime(reader["dateDelivered"]),
                            DateSeen = reader["dateSeen"] == System.DBNull.Value ? new DateTime(2000, 1, 1) : Convert.ToDateTime(reader["dateSeen"])
                        };

                        //if (message.ChatMessageType == ChatMessageTypes.MessageToViber || message.ChatMessageType == ChatMessageTypes.LinkAsImageToViber || message.ChatMessageType == ChatMessageTypes.ImageToViber || message.ChatMessageType == ChatMessageTypes.VideoToViber || message.ChatMessageType == ChatMessageTypes.FileToViber)
                        if (message.ChatMessageType.MsgOperatorToViber())
                        {
                            if (Guid.TryParse(Convert.ToString(reader["operatorGuid"]), out Guid guid))
                            {
                                message.Owner = guid;
                                message.OwnerName = Convert.ToString(reader["operatorName"]);
                                message.Receiver = user.Id;
                            }
                            else
                            {
                                message.Owner = user.Id;
                            }
                        }
                        else 
                        {
                            message.Owner = user.Id;
                        }

                        result.Add(message);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.GetMessagesSQL(): {ex.Message}");
                }
                finally
                {
                    reader?.Close();
                }
            }
            return result;
        }

        public void SaveClientSQL(UserViber user)
        {
            var name = Regex.Replace(user.Name, @"'", @"''");
            var query = $"INSERT INTO [dbo].[ArseniumViberClients] ([guid], [phone], [dateCreate], [viberId], [viberName], [inviteType], [subscribed], [avatar], [language], [country], [os], [device]) VALUES ('{user.Id}', '{user.phone}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', '{user.idViber}', '{name}', {(int)user.inviteType}, {(user.subscribed ? 1 : 0)}, '{user.Avatar}', '{user.language ?? ""}', '{user.country ?? ""}', '{user.primary_device_os ?? ""}', '{user.device_type ?? ""}')";
            Enqueue100(query);
        }

        public void UpdateClientSQL(UserViber user)
        {
            var name = Regex.Replace(user.Name, @"'", @"''");
            var query = $"UPDATE [dbo].[ArseniumViberClients] SET [viberName] = '{name}', [avatar] = '{user.Avatar}', [language] = '{user.language ?? ""}', [country] = '{user.country ?? ""}', [os] = '{user.primary_device_os ?? ""}', [device] = '{user.device_type ?? ""}', [subscribed] = '{(user.subscribed ? 1 : 0)}' WHERE viberId = '{user.idViber}'";
            Enqueue100(query);
        }

        public void SetClientPhoneSQL(UserViber user)
        {
            var query = $"UPDATE [dbo].[ArseniumViberClients] SET [phone] = '{user.phone}' WHERE viberId = '{user.idViber}'";
            Enqueue100(query);
        }

        public void SetAttachOperatorSQL(UserViber user, UserArsenium oper)
        {
            var query = $"UPDATE [dbo].[ArseniumViberClients] SET [operatorId] = A.id FROM [dbo].[ArseniumUsers] A WHERE viberId = '{user.idViber}' AND A.guid = '{oper.Id}'";
            Enqueue100(query);
        }

        public void SetSubscribedSQL(string viberId, bool subscribed)
        {
            var query = $"UPDATE [dbo].[ArseniumViberClients] SET [subscribed] = '{(subscribed ? 1 : 0)}' WHERE viberId = '{viberId}'";
            Enqueue100(query);
        }

        public void SetInviteTypeSQL(string viberId, int inviteType)
        {
            var query = $"UPDATE [dbo].[ArseniumViberClients] SET [inviteType] = {inviteType} WHERE viberId = '{viberId}'";
            Enqueue100(query);
        }

        public void SaveViberMessagesSQL(ChatMessage message, UserViber userViber = null)
        {
            var text = Regex.Replace(message.Text, @"'", @"''");
            var query = $"INSERT INTO [dbo].[ArseniumMessages] ([token], [ownerId], [receiverId], [type], [text], [dateCreate], [dateDelivered], [dateSeen]) SELECT 0, V.id, NULL, {(int)message.ChatMessageType}, '{text}','{message.DateCreate:yyyy-MM-dd HH:mm:ss}', NULL, NULL FROM [dbo].[ArseniumViberClients] V WHERE V.guid = '{message.Owner}'";
            Enqueue100(query);
            switch (message.ChatMessageType)
            {
                case ChatMessageTypes.MarkOperator:
                    int markPoll = message.Text switch
                    {
                        "0" => 1,
                        "1" => 3,
                        "2" => 5,
                        _ => 5
                    };
                    query = $"INSERT INTO [dbo].[ViberPoll] ([codep], [namep], [phone], [codesk], [coden], [dateBuy], [sended], [dateSend], [viberId], [viberName], [nrating], [datePoll], [respond], [readAdmin], [commAdmin], [inviteId], [inviteStat], [namesk], [sumall], [nameop]) VALUES (0, NULL, '{userViber.phone ?? "000000000000"}', 5, 0, '{message.DateCreate:yyyy-MM-dd HH:mm:ss}', 4, '{message.DateCreate:yyyy-MM-dd HH:mm:ss}', '{userViber.idViber}', '{userViber.Name}', {markPoll}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', '', 0, '', NULL, NULL, 'Маркетплейс', 0, 'Viber')";
                    Enqueue100(query);
                    break;
                case ChatMessageTypes.Complaint:
                    if (userViber.messageList.Any(m => m.ChatMessageType.MsgPool() && (message.DateCreate - m.DateCreate).TotalMinutes < 24 * 60))
                        query = $"EXECUTE [dbo].[us_Viber_ClientText] '{userViber.idViber}', 'Скарга: {text}', '{message.DateCreate:yyyy-MM-dd HH:mm:ss}'";
                    else
                        query = $"INSERT INTO [dbo].[ViberPoll] ([codep], [namep], [phone], [codesk], [coden], [dateBuy], [sended], [dateSend], [viberId], [viberName], [nrating], [datePoll], [respond], [readAdmin], [commAdmin], [inviteId], [inviteStat], [namesk], [sumall], [nameop]) VALUES (0, NULL, '{userViber.phone ?? "000000000000"}', 5, 0, '{message.DateCreate:yyyy-MM-dd HH:mm:ss}', 4, '{message.DateCreate:yyyy-MM-dd HH:mm:ss}', '{userViber.idViber}', '{userViber.Name}', 1, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 'Скарга: {text}', 0, '', NULL, NULL, 'Маркетплейс', 0, 'Viber')";
                    Enqueue100(query);
                    break;
                case ChatMessageTypes.Offer:
                    if (userViber.messageList.Any(m => m.ChatMessageType.MsgPool() && (message.DateCreate - m.DateCreate).TotalMinutes < 24 * 60))
                        query = $"EXECUTE [dbo].[us_Viber_ClientText] '{userViber.idViber}', 'Пропозиція: {text}', '{message.DateCreate:yyyy-MM-dd HH:mm:ss}'";
                    else
                        query = $"INSERT INTO [dbo].[ViberPoll] ([codep], [namep], [phone], [codesk], [coden], [dateBuy], [sended], [dateSend], [viberId], [viberName], [nrating], [datePoll], [respond], [readAdmin], [commAdmin], [inviteId], [inviteStat], [namesk], [sumall], [nameop]) VALUES (0, NULL, '{userViber.phone ?? "000000000000"}', 5, 0, '{message.DateCreate:yyyy-MM-dd HH:mm:ss}', 4, '{message.DateCreate:yyyy-MM-dd HH:mm:ss}', '{userViber.idViber}', '{userViber.Name}', 5, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 'Пропозиція: {text}', 0, '', NULL, NULL, 'Маркетплейс', 0, 'Viber')";
                    Enqueue100(query);
                    break;
            }
        }

        public void SaveArseniumMessagesSQL(ChatMessage message)
        {
            var text = Regex.Replace(message.Text, @"'", @"''");
            var query = $"INSERT INTO [dbo].[ArseniumMessages] ([token], [ownerId], [receiverId], [type], [text], [dateCreate], [dateDelivered], [dateSeen]) SELECT {message.Token}, A.id, U.id, {(int)message.ChatMessageType}, '{text}', '{message.DateCreate:yyyy-MM-dd HH:mm:ss}', NULL, NULL FROM [dbo].[ArseniumUsers] A, [dbo].[ArseniumViberClients] U WHERE A.guid = '{message.Owner}' AND U.guid = '{message.Receiver}'";
            Enqueue100(query);
        }

        public void DeliveredArseniumMessagesSQL(long message, DateTime dateDelivered)
        {
            var query = $"UPDATE [dbo].[ArseniumMessages] SET [dateDelivered] = '{dateDelivered:yyyy-MM-dd HH:mm:ss}' WHERE token = {message}";
            Enqueue100(query);
        }

        public void SeenArseniumMessagesSQL(long message, DateTime dateSeen)
        {
            var query = $"UPDATE [dbo].[ArseniumMessages] SET [dateSeen] = '{dateSeen:yyyy-MM-dd HH:mm:ss}' WHERE token = {message}";
            Enqueue100(query);
        }

        public List<Guid> GetLastClientsSQL(Guid opratorGuid, int days, bool all)
        {
            var result = new List<Guid>();
            string query;
            if (all)
            {
                query = $"SELECT DISTINCT C.[guid] as client FROM [dbo].[ArseniumViberClients] C, [dbo].[ArseniumMessages] M WHERE C.id = M.[receiverId] AND ([type] = 1 OR [type] = 11 OR [type] = 12 OR [type] = 14 OR [type] = 16) AND M.[dateCreate] >= DATEADD(DAY, -{days}, GETDATE()) UNION SELECT C.[guid] FROM [dbo].[ArseniumViberClients] C, [dbo].[ArseniumMessages] M WHERE C.[id] = M.[ownerId] AND M.[dateCreate] >= DATEADD(DAY, -{days}, GETDATE())";
            }
            else
            {
                query = $"SELECT DISTINCT C.[guid] as client FROM [dbo].[ArseniumViberClients] C, [dbo].[ArseniumMessages] M, [dbo].[ArseniumUsers] O WHERE O.[guid] = '{opratorGuid}' AND O.id = M.[ownerId] AND C.id = M.[receiverId] AND ([type] = 1 OR [type] = 11 OR [type] = 12 OR [type] = 14 OR [type] = 16) AND M.[dateCreate] >= DATEADD(DAY, -{days}, GETDATE()) UNION SELECT C.[guid] FROM [dbo].[ArseniumViberClients] C, [dbo].[ArseniumMessages] M, [dbo].[ArseniumUsers] O WHERE O.[guid] = '{opratorGuid}' AND O.id = C.[operatorId] AND C.[id] = M.[ownerId] AND M.[dateCreate] >= DATEADD(DAY, -{days}, GETDATE())";
            }
            using (var connection = new SqlConnection(connectionSql100))
            {
                var command = new SqlCommand(query, connection);
                SqlDataReader reader = null;
                try
                {
                    connection.Open();
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var clientGuid = Convert.ToString(reader["client"]);
                        if (!String.IsNullOrWhiteSpace(clientGuid))
                            result.Add(new Guid(clientGuid));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.GetLastClientsSQL(): {ex.Message}");
                }
                finally
                {
                    reader?.Close();
                }

                return result;
            }
        }

        public List<Pool> GetPoolsSQL()
        {
            var result = new List<Pool>();
            var query = $"  SELECT C.guid, C.viberName, C.phone, C.avatar, O.name, M.[dateCreate], M.[type], M.[text] FROM [dbo].[ArseniumMessages] M INNER JOIN [dbo].[ArseniumViberClients] C ON C.id = M.ownerId LEFT JOIN [dbo].[ArseniumUsers] O ON O.id = C.operatorId WHERE [type] = 4 OR [type] = 8 OR [type] = 9 ORDER BY dateCreate DESC";
            using (var connection = new SqlConnection(connectionSql100))
            {
                var command = new SqlCommand(query, connection);
                SqlDataReader reader = null;
                try
                {
                    connection.Open();
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var pool = new Pool()
                        {
                            Id = new Guid(Convert.ToString(reader["guid"])),
                            ViberName = Convert.ToString(reader["viberName"]),
                            Phone = Convert.ToString(reader["phone"]),
                            Avatar = Convert.ToString(reader["avatar"]),
                            Operator = reader["name"] == System.DBNull.Value ? null : Convert.ToString(reader["name"]),
                            DateCreate = reader["dateCreate"] == System.DBNull.Value ? new DateTime(2000, 1, 1) : Convert.ToDateTime(reader["dateCreate"]),
                            Type = (ChatMessageTypes)Convert.ToInt32(reader["type"]),
                            Text = Convert.ToString(reader["text"])
                        };

                        if (pool.Type == ChatMessageTypes.MarkOperator)
                            switch (pool.Text.Trim())
                            {
                                case "0":
                                    pool.Text = "Погано";
                                    break;
                                case "1":
                                    pool.Text = "Нормально";
                                    break;
                                case "2":
                                    pool.Text = "Відмінно";
                                    break;
                            }
                        result.Add(pool);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.GetPoolsSQL(): {ex.Message}");
                }
                finally
                {
                    reader?.Close();
                }
            }
            return result;
        }

        public void Run100SQL(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
                return;
            var command = new SqlCommand(query, Connection100);
            try
            {
                if (Connection100.State != System.Data.ConnectionState.Open)
                {
                    Connection100.Close();
                    Connection100.Open();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"DataProvider.RunSQL100Open(): {ex.Message} Запит: {query}");
            }
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Error($"DataProvider.RunSQL100Execute(): {ex.Message} Запит: {query}");
            }
        }

        public void GetClientFromSQL(UserViber user)
        {
            var query = $"EXECUTE [dbo].[us_Asterium_GetClient] '{user.phone}'";
            using (var connection = new SqlConnection(connectionSql4))  //4 Сервер!!!!
            {
                var command = new SqlCommand(query, connection);
                SqlDataReader reader = null;
                try
                {
                    connection.Open();
                    reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        user.buhnetName = Convert.ToString(reader["name"]);
                        user.codep = Convert.ToInt32(reader["codep"]);
                    }
                    else //не наш клієнт
                    {
                        user.buhnetName = null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.GetClientFromSQL(): {ex.Message}");
                }
                finally
                {
                    reader?.Close();
                }
            }

            if (user.buhnetName != null)
            {
                query = $"UPDATE [dbo].[ArseniumViberClients] SET [codep] = '{user.codep}', [namep] = '{user.buhnetName}' WHERE viberId = '{user.idViber}'";
                Enqueue100(query);
            }
        }

        public List<string> MyOrders(string phone)
        {
            var result = new List<string>();
            var orderList = new List<ProformaOrder>();
            var query = $"EXECUTE [dbo].[us_Asterium_GetOrders] '{phone}'";
            using (var connection = new SqlConnection(connectionSql4))   //4 Сервер!!!!
            {
                var command = new SqlCommand(query, connection);
                SqlDataReader reader = null;
                try
                {
                    connection.Open();
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var stat = new ProformaOrder
                        {
                            Proforma = Convert.ToInt32(reader["proforma"]),
                            Date = Convert.ToDateTime(reader["daten"]),
                            NameTv = Convert.ToString(reader["nametv"]),
                            Count = Convert.ToDouble(reader["kol"]),
                            Cena = Convert.ToDouble(reader["cena_r"]),
                            Suma = Convert.ToDouble(reader["sumall"]),
                            Status = Convert.ToString(reader["status"])
                        };
                        orderList.Add(stat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.MyOrders(): {ex.Message}");
                }
                finally
                {
                    reader?.Close();
                }
            }
            if (orderList.Count != 0)
            {
                var proforms = orderList.GroupBy(order => order.Proforma);
                foreach (var proform in proforms)
                {
                    var orderString = $"Дата - {proform.Max(o => o.Date):dd.MM.yyyy}\nСума - {proform.Sum(o => o.Suma)}грн.\nСтатус - {proform.Max(o => o.Status)}\n";
                    foreach (var order in proform)
                    {
                        //🟢 - зелений кружок
                        orderString += $"✅{order.NameTv}, {order.Count}шт., {order.Suma}грн.\n";
                    }
                    result.Add(orderString);
                }

            }
            return result;
        }

        public List<MyStat1> GetStat1(int sklad)
        {
            var result = new List<MyStat1>();
            var query = $"DECLARE @count int; SELECT @count = COUNT(*) FROM[InetClient].[dbo].[ViberPoll] WHERE inviteStat IS NOT NULL AND sended = 1 AND inviteStat IN('CLICKED', 'VIBER_UNKNOWN_USER', 'VIBER_UNDELIVERED_STATE', 'DELIVERED', 'READ', 'PENDING', 'VIBER_EXPIRED_STATE') AND (codesk = {sklad} OR {sklad} = 0) ;SELECT MIN(namesk) as namesk, inviteStat as name, COUNT(*) as count, 100 * COUNT(*) / @count as rate FROM[InetClient].[dbo].[ViberPoll] WHERE inviteStat IS NOT NULL AND sended = 1 AND inviteStat IN('CLICKED', 'VIBER_UNKNOWN_USER', 'VIBER_UNDELIVERED_STATE', 'DELIVERED', 'READ', 'PENDING', 'VIBER_EXPIRED_STATE') AND (codesk = {sklad} OR {sklad} = 0) GROUP BY inviteStat";
            //var query = $"SELECT inviteStat as name, COUNT(*) as count FROM [InetClient].[dbo].[ViberPoll] WHERE inviteStat IS NOT NULL AND sended = 1 AND inviteStat IN ('CLICKED', 'VIBER_UNKNOWN_USER', 'VIBER_UNDELIVERED_STATE', 'DELIVERED', 'READ', 'PENDING', 'VIBER_EXPIRED_STATE') GROUP BY inviteStat";
            using (var connection = new SqlConnection(connectionSql100))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        var stat = new MyStat1 { Namesk = Convert.ToString(reader["namesk"]), Name = Convert.ToString(reader["name"]), Count = Convert.ToInt32(reader["count"]), Rate = Convert.ToInt32(reader["rate"]) };
                        result.Add(stat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.GetStat1(): {ex.Message}");
                }
                finally
                {
                    reader.Close();
                }
            }
            return result;
        }

        public List<MyStat2> GetStat2(int sklad)
        {
            var result = new List<MyStat2>();
            var query = $"SELECT nrating as nrating, COUNT(*) as count FROM [InetClient].[dbo].[ViberPoll] WHERE viberId IS NOT NULL AND nrating IS NOT NULL AND (codesk = {sklad} OR {sklad} = 0) GROUP BY nrating";
            using (var connection = new SqlConnection(connectionSql100))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        var stat = new MyStat2 { Nrating = Convert.ToInt32(reader["nrating"]), Count = Convert.ToInt32(reader["count"]) };
                        result.Add(stat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"DataProvider.GetStat2(): {ex.Message}");
                }
                finally
                {
                    reader.Close();
                }
            }
            return result;
        }
    }
}
