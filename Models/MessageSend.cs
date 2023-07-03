using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace ViberAPI.Models
{
    public class MessageSend
    {
        public string receiver { get; set; }
        public int min_api_version { get; set; }
        public bool ShouldSerializemin_api_version() { return min_api_version != 0; }
        public Sender sender { get; set; }
        public string tracking_data { get; set; }
        public string type { get; set; }
        public string text { get; set; }
        public Keyboard keyboard { get; set; }

        public static Button ButtonMM = new Button()
        {
            Columns = 6,
            Rows = 1,
            Text = "<font color=\"#FFFFFF\"><font size=\"16\">Головне меню</font></font>",
            Image = "https://viber.ars.ua/mainmenu.png",
            TextOpacity = 0,
            ActionType = "reply",
            ActionBody = "MENU#MM",
            BgColor = "#3E3D3C",
            Silent = true
        };

        public static Button ButtonEndConv = new Button()
        {
            Columns = 6,
            Rows = 1,
            Text = "<font color=\"#FFFFFF\"><font size=\"16\">Головне меню</font></font>",
            Image = "https://viber.ars.ua/mainmenu.png",
            TextOpacity = 0,
            ActionType = "reply",
            ActionBody = "MENU#EC",
            BgColor = "#3E3D3C",
            Silent = true
        };

        public static MessageSend MessageActivateBot(string text = null)
        {
            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРС-бот",
                },
                type = (text == null ? null : "text"),
                text = text,
                keyboard = new Keyboard()
                {
                    //BgColor = "#FFFFFF",
                    InputFieldState = "regular",
                    Buttons = new List<Button>() {
                        new Button
                        {
                            Columns = 6,
                            Rows = 1,
                            Text = "<font color=\"#000000\"><font size=\"16\">Активувати бот</font></font>",
                            Image = "https://viber.ars.ua/activatebot.png",
                            TextOpacity = 0,
                            ActionType = "share-phone",
                            ActionBody = "phone",
                            BgColor = "#3E3D3C",
                            Silent = true
                        }
                    }
                }
            };
            return message;
        }

        public static MessageSend MessageStartMain(bool withIdentify = true, string text = null, bool pool = false)
        {
            var buttons = new List<Button>();
            if (pool)
            {
                buttons.AddRange(new List<Button>()
                {
                    new Button
                    {
                        Columns = 2,
                        Rows = 1,
                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"16\">0</font></font></b>",
                        Image = "https://viber.ars.ua/markoper1.png",
                        TextOpacity = 0,
                        ActionType = "reply",
                        ActionBody = "MENU#PT0",
                        BgColor = "#3E3D3C",
                        Silent = true
                    },
                    new Button
                    {
                        Columns = 2,
                        Rows = 1,
                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"16\">1</font></font></b>",
                        Image = "https://viber.ars.ua/markoper2.png",
                        TextOpacity = 0,
                        ActionType = "reply",
                        ActionBody = "MENU#PT1",
                        BgColor = "#3E3D3C",
                        Silent = true
                    },
                    new Button
                    {
                        Columns = 2,
                        Rows = 1,
                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"16\">2</font></font></b>",
                        Image = "https://viber.ars.ua/markoper3.png",
                        TextOpacity = 0,
                        ActionType = "reply",
                        ActionBody = "MENU#PT2",
                        BgColor = "#3E3D3C",
                        Silent = true
                    }
                });
            }
            buttons.Add(new Button
            {
                Columns = 2,
                Rows = 2,
                Text = "<font color=\"#000000\"><font size=\"16\">💬 Чат з оператором</font></font>",
                Image = "https://viber.ars.ua/chatoper.png",
                TextOpacity = 0,
                ActionType = "reply",
                ActionBody = "MENU#SM1",
                BgColor = "#3E3D3C",
                Silent = true
            });
            buttons.Add(new Button
            {
                Columns = 2,
                Rows = 2,
                Text = "<font color=\"#000000\"><font size=\"16\">📦 Мої замовлення</font></font>",
                Image = "https://viber.ars.ua/myorsers.png",
                TextOpacity = 0,
                ActionType = "reply",
                ActionBody = "MENU#SM2",
                BgColor = "#3E3D3C",
                Silent = true
            });
            buttons.Add(new Button
            {
                Columns = 2,
                Rows = 2,
                Text = "<font color=\"#000000\"><font size=\"16\">🫴🏼 Скарги/Пропозиції</font></font>",
                Image = "https://viber.ars.ua/complaintoffer.png",
                TextOpacity = 0,
                ActionType = "reply",
                ActionBody = "MENU#SM3",
                BgColor = "#3E3D3C",
                Silent = true
            });

            if (withIdentify)
            {
                var indenty = new Button
                {
                    Columns = 6,
                    Rows = 1,
                    Text = "<font color=\"#000000\"><font size=\"16\">Ідентифікувати себе</font></font>",
                    Image = "https://viber.ars.ua/indenty.png",
                    TextOpacity = 0,
                    ActionType = "share-phone",
                    ActionBody = "phone",
                    BgColor = "#3E3D3C",
                    Silent = true
                };
                buttons.Add(indenty);
            }

            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРС-бот",
                },
                type = (text == null ? null : "text"),
                text = text,
                keyboard = new Keyboard()
                {
                    //BgColor = "#FFFFFF",
                    InputFieldState = "hidden",
                    Buttons = buttons
                }
            };
            return message;
        }

        public static MessageSend MessageMainMenu()
        {
            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРС-бот",
                },
                keyboard = new Keyboard()
                {
                    InputFieldState = "regular",
                    Buttons = new List<Button>() { ButtonMM }
                }
            };
            return message;
        }

        public static MessageSend MessageEndСonversation()
        {
            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРС-бот",
                },
                keyboard = new Keyboard()
                {
                    InputFieldState = "regular",
                    Buttons = new List<Button>() { ButtonEndConv }
                }
            };
            return message;
        }

        public static MessageSend MessageComplaintOffer()
        {
            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРС-бот",
                },
                keyboard = new Keyboard()
                {
                    //BgColor = "#FFFFFF",
                    InputFieldState = "hidden",
                    Buttons = new List<Button>()
                                {
                                    new Button
                                    {
                                        Columns = 3,
                                        Rows = 2,
                                        Text = "<font color=\"#000000\"><font size=\"16\">👎 Скарга</font></font>",
                                        Image = "https://viber.ars.ua/complaint.png",
                                        TextOpacity = 0,
                                        ActionType = "reply",
                                        ActionBody = "MENU#CO1",
                                        BgColor = "#3E3D3C",
                                        Silent = true
                                    },
                                    new Button
                                    {
                                        Columns = 3,
                                        Rows = 2,
                                        Text = "<font color=\"#000000\"><font size=\"16\">👆 Пропозиція</font></font>",
                                        Image = "https://viber.ars.ua/offer.png",
                                        TextOpacity = 0,
                                        ActionType = "reply",
                                        ActionBody = "MENU#CO2",
                                        BgColor = "#3E3D3C",
                                        Silent = true
                                    },
                                    ButtonMM
                                }
                }
            };
            return message;
        }

        public static MessageSend MessagePoolStart(ViberImputMessage messageImput)
        {
            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРС-бот",
                },
                type = "text",
                text = "Оцініть якість обслуговування в супермаркеті АРС",
                keyboard = new Keyboard()
                {
                    Type = "keyboard",
                    //BgColor = "#FFFFFF",
                    InputFieldState = "hidden",
                    Buttons = new List<Button>()
                                {
                                    new Button
                                    {
                                        Columns = 6,
                                        Rows = 1,
                                        Text = "Відмінно",
                                        TextOpacity = 0,
                                        ActionType = "reply",
                                        ActionBody = "MENU#PS5T" + messageImput.context,
                                        BgColor = "#3E3D3C",
                                        Image = "https://viber.ars.ua/5_2.png"
                                    },
                                    new Button
                                    {
                                        Columns = 6,
                                        Rows = 1,
                                        Text = "Нормально",
                                        TextOpacity = 0,
                                        ActionType = "reply",
                                        ActionBody = "MENU#PS3T" + messageImput.context,
                                        BgColor = "#3E3D3C",
                                        Image = "https://viber.ars.ua/3_2.png"
                                    },
                                    new Button
                                    {
                                        Columns = 6,
                                        Rows = 1,
                                        Text = "Погано",
                                        TextOpacity = 0,
                                        ActionType = "reply",
                                        ActionBody = "MENU#PS1T" + messageImput.context,
                                        BgColor = "#3E3D3C",
                                        Image = "https://viber.ars.ua/1_2.png"
                                    }
                                }
                }
            };
            return message;
        }

        public static MessageSend MessageStartPrac()
        {
            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРСеній",
                },
                type = "text",
                text = "Вас недавно обслуговував працівник компютерного відділу АРС. Оцініть його, будь-ласка. Хто Вас обслуговував:",
                keyboard = new Keyboard()
                {
                    Type = "keyboard",
                    //BgColor = "#FFFFFF",
                    InputFieldState = "hidden",
                    Buttons = new List<Button>()
                                {
                                    new Button
                                    {
                                        Columns = 6,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">1. Печений Володимир</font></font></b>",
                                        ActionBody = "MENU#SP1",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 6,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">2. Герман Василь</font></font></b>",
                                        ActionBody = "MENU#SP2",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 6,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">3. Сахно Дмитро</font></font></b>",
                                        ActionBody = "MENU#SP3",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 6,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">4. Димашевський Віктор</font></font></b>",
                                        ActionBody = "MENU#SP4",
                                        BgColor = "#1E662D"
                                    }
                                }
                }
            };
            return message;
        }

        public static MessageSend MessagePracJakist()
        {
            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРСеній",
                },
                type = "text",
                text = "Оцініть якість обслуговування",
                keyboard = new Keyboard()
                {
                    Type = "keyboard",
                    //BgColor = "#FFFFFF",
                    InputFieldState = "hidden",
                    Buttons = new List<Button>()
                                {
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">0</font></font></b>",
                                        ActionBody = "MENU#PJ0",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">1</font></font></b>",
                                        ActionBody = "MENU#PJ1",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">2</font></font></b>",
                                        ActionBody = "MENU#PJ2",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">3</font></font></b>",
                                        ActionBody = "MENU#PJ3",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">4</font></font></b>",
                                        ActionBody = "MENU#PJ4",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">5</font></font></b>",
                                        ActionBody = "MENU#PJ5",
                                        BgColor = "#1E662D"
                                    }
                                }
                }
            };
            return message;
        }

        public static MessageSend MessagePracVdov()
        {
            var message = new MessageSend()
            {
                min_api_version = 6,
                sender = new Sender()
                {
                    name = "АРСеній",
                },
                type = "text",
                text = "Оцініть наскільки вас вдовільнив працівник компютерного відділу",
                keyboard = new Keyboard()
                {
                    Type = "keyboard",
                    //BgColor = "#FFFFFF",
                    InputFieldState = "hidden",
                    Buttons = new List<Button>()
                                {
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">0</font></font></b>",
                                        ActionBody = "MENU#PV0",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">1</font></font></b>",
                                        ActionBody = "MENU#PV1",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">2</font></font></b>",
                                        ActionBody = "MENU#PV2",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">3</font></font></b>",
                                        ActionBody = "MENU#PV3",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">4</font></font></b>",
                                        ActionBody = "MENU#PV4",
                                        BgColor = "#1E662D"
                                    },
                                    new Button
                                    {
                                        Columns = 1,
                                        Rows = 1,
                                        Text = $"<b><font color=\"#4DFAAC\"><font size=\"24\">5</font></font></b>",
                                        ActionBody = "MENU#PV5",
                                        BgColor = "#1E662D"
                                    }
                                }
                }
            };
            return message;
        }
    }
}
