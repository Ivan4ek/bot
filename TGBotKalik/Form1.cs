using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TGBotKalik
{
    public partial class Form1 : Form
    {
        private ITelegramBotClient botClient;
        private Dictionary<long, UserOrder> userOrders = new Dictionary<long, UserOrder>();

        public enum ChatState
        {
            WaitingForContact,
            // Другие состояния, если необходимо
        }

        // Где-то в вашем коде, создайте словарь для отслеживания состояний чата
        private readonly Dictionary<long, ChatState> chatStates = new Dictionary<long, ChatState>();

        private string botToken = "1"; // Токен бота заказов
        private long channelId = -1; // ID канала администраторов

        private string MessageStart = "Вас приветствует кальянная `KalikSuper` [🎉]!" + Environment.NewLine + "[🤖] Этот бот поможет Вам заказать кальян ещё до посещения заведения." + Environment.NewLine + Environment.NewLine + "[👇] Для начала оформления заказа - нажмите на кнопку ниже.";
        private string ResponseMessagePeople = Environment.NewLine + $"[👥] Давайте выберем количество человек:";
        private string ResponseMessageTable = Environment.NewLine + $"[🪑] Давайте выберем столик:";
        private string ResponseMessageTabak = Environment.NewLine + "[🚬] Давайте выберем табак и крепость:";
        private string ResponseMessageTime = Environment.NewLine + "[⏰] Давайте выберем время:";
        private string ResponseMessageHelp = Environment.NewLine + "[📲] Нужно ли Вам перезвонить для уточнения заказа?";
        private string ResponseMessageNumber = Environment.NewLine + "[📲] Напиши номер";
        private string ResponseMessageExit = Environment.NewLine + "[📲] Давайте завершим Ваш заказ:";
        private string ResponseMessageGoodluck = "Будем ждать Вас!";

        public Form1()
        {
            InitializeComponent();

            botClient = new TelegramBotClient(botToken);
            botClient.OnMessage += BotOnMessageReceived;
            botClient.OnCallbackQuery += BotOnCallbackQueryReceived;
            botClient.StartReceiving();
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.Text == "/start")
            {
                var chatId = e.Message.Chat.Id;

                if (!userOrders.ContainsKey(chatId))
                {
                    userOrders[chatId] = new UserOrder();
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                    {
                        InlineKeyboardButton.WithCallbackData("[➕] Начать", "people_"),
                    }
                });

                var messageStart = await botClient.SendTextMessageAsync(chatId, MessageStart, replyMarkup: inlineKeyboard);
            }

            if (e.Message.Type == MessageType.Contact)
            {
                var contact = e.Message.Contact;
                var phoneNumber = contact?.PhoneNumber;

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    UserOrder uo = new UserOrder();
                    // Получите или создайте объект UserOrder для текущего пользователя
                    UserOrder userOrder = uo.GetUserOrderForUser(e.Message.Chat.Id);

                    // Запишите номер телефона в поле Phone
                    userOrder.Phone = phoneNumber;
                }

                var responseMessageExit1 = $"{ResponseMessageExit}";

                // Отправляем сообщение с ссылкой на пользователя
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[✅] Завершить заказ", "exit_"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "time_"),
                        }
                    });

                // Отправляем новое сообщение в чат с текстом "ыаыуы"
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, responseMessageExit1, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                var callbackData = e.CallbackQuery.Data;
                long chatId = e.CallbackQuery.Message.Chat.Id;

                if (!userOrders.ContainsKey(chatId))
                {
                    userOrders[chatId] = new UserOrder();
                }

                var userOrder = userOrders[chatId];

                #region Start

                if (callbackData == "start_order")
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    // Отправляем сообщение с начальным текстом и кнопкой "Начать"
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[➕] Начать", "people_"),
                        }
                    });

                    userOrder.Username = e.CallbackQuery.From.Username;

                    var messageStart = await botClient.SendTextMessageAsync(chatId, MessageStart, replyMarkup: inlineKeyboard);
                }

                #endregion

                #region People

                if (callbackData == "people_")
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    // Отправляем сообщение с ссылкой на пользователя
                    userOrder.Username = e.CallbackQuery.From.Username;
                    var responseMessagePeople = $"{userOrder.Username}, {ResponseMessagePeople}";

                    //InlineKeyboardMarkup
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("1 человек [1️⃣]", "people_1"),
                            InlineKeyboardButton.WithCallbackData("2 человек [2️⃣]", "people_2")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("3 человек [3️⃣]", "people_3"),
                            InlineKeyboardButton.WithCallbackData("4 человек [4️⃣]", "people_4")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("5 человек [5️⃣]", "people_5"),
                            InlineKeyboardButton.WithCallbackData("6 человек [6️⃣]", "people_6")
                        }
                        ,
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "start_order"),
                        }
                    });

                    await botClient.SendTextMessageAsync(chatId, responseMessagePeople, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                #endregion

                #region Table
                if (callbackData == "people_1" || callbackData == "people_2" || callbackData == "people_3" || callbackData == "people_4")
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var responseMessageTable = $"{userOrder.Username}, {ResponseMessageTable}";
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🪑] 1-й стол", "table_1"),
                            InlineKeyboardButton.WithCallbackData("[🪑] 2-й стол", "table_2")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🪑] 3-й стол", "table_3"),
                            InlineKeyboardButton.WithCallbackData("[🪑] 4-й стол", "table_4")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "people_"),
                        }
                    });

                    if (!callbackData.Equals("< < < Назад"))
                    {
                        // Сохраняем выбор объекте UserOrder для текущего пользователя
                        userOrder.PeopleNumber = callbackData.Replace("people_", "");
                    }
                    else
                    {
                        // Сбрасываем выбор, остальные данные остаются неизменными
                        userOrder.Username = e.CallbackQuery.From.Username;
                        userOrder.PeopleNumber = callbackData.Replace("people_", "");
                    }

                    await botClient.SendTextMessageAsync(chatId, responseMessageTable, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                if (callbackData == "people_5" || callbackData == "people_6")
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var responseMessageTable = $"{userOrder.Username}, {ResponseMessageTable}";
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🪑] 5-й стол", "table_5"),
                            InlineKeyboardButton.WithCallbackData("[🪑] 6-й стол", "table_6")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "people_"),
                        }
                    });

                    if (!callbackData.Equals("< < < Назад"))
                    {
                        // Сохраняем выбор объекте UserOrder для текущего пользователя
                        userOrder.PeopleNumber = callbackData.Replace("people_", "");
                    }
                    else
                    {
                        // Сбрасываем выбор, остальные данные остаются неизменными
                        userOrder.Username = e.CallbackQuery.From.Username;
                        userOrder.PeopleNumber = callbackData.Replace("people_", "");
                    }

                    await botClient.SendTextMessageAsync(chatId, responseMessageTable, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                #endregion

                #region Tabak

                if (callbackData.StartsWith("table_"))
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var responseMessageTabak = $"{userOrder.Username}, {ResponseMessageTabak}";

                    // Отправляем сообщение с ссылкой на пользователя
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🚬] [табак_1], [крепость_1]", "tabak_tabak1"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🚬] [табак_1], [крепость_2]", "tabak_tabak2")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🚬] [табак_2], [крепость_1]", "tabak_tabak3")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🚬] [табак_2], [крепость_2]", "tabak_tabak4")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🚬] [табак_3], [крепость_1]", "tabak_tabak5"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🚬] [табак_3], [крепость_2]", "tabak_tabak6")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "people_"),
                        }
                    });

                    userOrder.Tabak = callbackData.Replace("tabak_", "");

                    // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                    if (!callbackData.Equals("< < < Назад"))
                    {
                        // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                        userOrder.TableNumber = callbackData.Replace("table_", "");

                    }
                    else
                    {
                        // Сбрасываем выбор количества людей, остальные данные остаются неизменными
                        userOrder.Username = e.CallbackQuery.From.Username;
                        userOrder.PeopleNumber = userOrder.PeopleNumber;
                        userOrder.TableNumber = callbackData.Replace("table_", "");
                    }

                    // Отправляем новое сообщение в чат с текстом "ыаыуы"
                    await botClient.SendTextMessageAsync(chatId, responseMessageTabak, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                #endregion

                #region Time

                if (callbackData.StartsWith("tabak_"))
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var responseMessageTime = $"{userOrder.Username}, {ResponseMessageTime}";

                    // Отправляем сообщение с ссылкой на пользователя
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("1️⃣9️⃣:0️⃣0️⃣", "time_19:00"),
                            InlineKeyboardButton.WithCallbackData("2️⃣0️⃣:0️⃣0️⃣", "time_20:00")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("2️⃣1️⃣:0️⃣0️⃣", "time_21:00"),
                            InlineKeyboardButton.WithCallbackData("2️⃣2️⃣:0️⃣0️⃣", "time_22:00")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("2️⃣3️⃣:0️⃣0️⃣", "time_23:00"),
                            InlineKeyboardButton.WithCallbackData("0️⃣0️⃣:0️⃣0️⃣", "time_00:00")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "table_"),
                        }
                    });

                    // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                    if (!callbackData.Equals("< < < Назад"))
                    {
                        // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                        userOrder.Tabak = callbackData.Replace("tabak_", "");
                    }
                    else
                    {
                        // Сбрасываем выбор количества людей, остальные данные остаются неизменными
                        userOrder.Username = e.CallbackQuery.From.Username;
                        userOrder.PeopleNumber = userOrder.PeopleNumber;
                        userOrder.TableNumber = userOrder.TableNumber;
                        userOrder.Tabak = callbackData.Replace("tabak_", "");
                    }

                    // Отправляем новое сообщение в чат с текстом "ыаыуы"
                    await botClient.SendTextMessageAsync(chatId, responseMessageTime, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                #endregion

                #region Help

                if (callbackData.StartsWith("time_"))
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var responseMessageExit = $"{userOrder.Username}, {ResponseMessageHelp}";

                    // Отправляем сообщение с ссылкой на пользователя
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[✅] Да, мне нужна помощь", "help_Да"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[✅] Нет, мне не нужна помощь", "help_Нет"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "tabak_"),
                        }
                    });

                    // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                    if (!callbackData.Equals("< < < Назад"))
                    {
                        // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                        userOrder.Time = callbackData.Replace("time_", "");
                    }
                    else
                    {
                        // Сбрасываем выбор количества людей, остальные данные остаются неизменными
                        userOrder.Username = e.CallbackQuery.From.Username;
                        userOrder.PeopleNumber = userOrder.PeopleNumber;
                        userOrder.TableNumber = userOrder.TableNumber;
                        userOrder.Tabak = userOrder.Tabak;
                        userOrder.Time = callbackData.Replace("time_", "");
                    }

                    // Отправляем новое сообщение в чат с текстом "ыаыуы"
                    await botClient.SendTextMessageAsync(chatId, responseMessageExit, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                #endregion

                #region Exit

                if (callbackData.StartsWith("help_Нет"))
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var responseMessageExit = $"{userOrder.Username}, {ResponseMessageExit}";

                    // Отправляем сообщение с ссылкой на пользователя
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[✅] Завершить заказ", "exit_"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "time_"),
                        }
                    });

                    // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                    if (!callbackData.Equals("< < < Назад"))
                    {
                        // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                        userOrder.Help = callbackData.Replace("help_", "");
                    }
                    else
                    {
                        // Сбрасываем выбор количества людей, остальные данные остаются неизменными
                        userOrder.Username = e.CallbackQuery.From.Username;
                        userOrder.PeopleNumber = userOrder.PeopleNumber;
                        userOrder.TableNumber = userOrder.TableNumber;
                        userOrder.Tabak = userOrder.Tabak;
                        userOrder.Time = userOrder.Time;
                        userOrder.Help = callbackData.Replace("help_", "");
                    }

                    // Отправляем новое сообщение в чат с текстом "ыаыуы"
                    await botClient.SendTextMessageAsync(chatId, responseMessageExit, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                if (callbackData.StartsWith("help_Да"))
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var responseMessageExit = $"{userOrder.Username}, {ResponseMessageNumber}";

                    // Отправьте сообщение с запросом контакта
                    var inlineKeyboard1 = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton { Text = "Отправить контакт", RequestContact = true }
                        },
                    });

                    await botClient.SendTextMessageAsync(chatId, responseMessageExit, replyMarkup: inlineKeyboard1, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                    // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                    if (!callbackData.Equals("< < < Назад"))
                    {
                        // Сохраняем выбор количества людей в объекте UserOrder для текущего пользователя
                        userOrder.Time = callbackData.Replace("time_", "");
                        userOrder.Phone = userOrder.Phone;
                    }
                    else
                    {
                        // Сбрасываем выбор количества людей, остальные данные остаются неизменными
                        userOrder.Username = e.CallbackQuery.From.Username;
                        userOrder.PeopleNumber = userOrder.PeopleNumber;
                        userOrder.TableNumber = userOrder.TableNumber;
                        userOrder.Tabak = userOrder.Tabak;
                        userOrder.Phone = userOrder.Phone;
                        userOrder.Time = callbackData.Replace("time_", "");
                    }
                }
                #endregion

                #region Output

                if (callbackData.StartsWith("exit_"))
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🔺] Редактировать текущий заказ", "edit_1"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[🔸] Ещё один заказ", "edit_2"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[♦️] Отменить заказ", "edit_3"),
                        }
                    });

                    var responseMessageFinalClient = $"👤 {userOrder.Username}," + Environment.NewLine + $"[✅] Ваш заказ выглядит так:" + Environment.NewLine + Environment.NewLine + $"[🪑] Стол №{userOrder.TableNumber}" + Environment.NewLine + $"[🚬] Табак {userOrder.Tabak}" + Environment.NewLine + $"[👥] Компания из {userOrder.PeopleNumber} человек(а)" + Environment.NewLine + $"[⏰] Время: {userOrder.Time}.";

                    await botClient.SendTextMessageAsync(chatId, responseMessageFinalClient, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: inlineKeyboard);

                    var ChannelMessageFinalChannel = $"[⚠️] Дзынь-дзынь, новый заказ!" + Environment.NewLine + Environment.NewLine + $"👤 Пользователь [@{userOrder.Username}](https://t.me/{userOrder.Username}) сделал заказ!" + Environment.NewLine + Environment.NewLine + $"[🪑] Стол №{userOrder.TableNumber}." + Environment.NewLine + $"[🚬] Табак {userOrder.Tabak}." + Environment.NewLine + $"[👥] Будет компания из: {userOrder.PeopleNumber} человек(а)." + Environment.NewLine + $"[⏰] Время прибытия гостей: {userOrder.Time}." + Environment.NewLine + $"Нужна ли посетителю помощь? - {userOrder.Help} { userOrder.Phone}";

                    await botClient.SendTextMessageAsync(channelId, ChannelMessageFinalChannel, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                    // Отправьте сообщение с запросом контакта
                    var inlineKeyboard1 = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton { Text = "/start" }
                        },
                    });

                    var responseMessageExit = $"{userOrder.Username}, {ResponseMessageGoodluck}";
                    await botClient.SendTextMessageAsync(chatId, responseMessageExit, replyMarkup: inlineKeyboard1, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                #endregion

                #region Edits / (1, 2, 3)

                if (callbackData.StartsWith("edit_1"))
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var ChannelMessageFinalChannelEdit = $"[[🟥]] " + Environment.NewLine + Environment.NewLine + $"👤 Пользователь отредактировал заказ!" + Environment.NewLine + $"Заказ ниже от {userOrder.Username} является отредактированным, а значит заказ выше от этого пользователя выполнять не следует. Для уточнения пишите пользователю!";

                    await botClient.SendTextMessageAsync(channelId, ChannelMessageFinalChannelEdit, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                    // Отправляем сообщение с ссылкой на пользователя
                    userOrder.Username = e.CallbackQuery.From.Username;
                    var responseMessagePeople = $"{userOrder.Username}, {ResponseMessagePeople}";

                    //InlineKeyboardMarkup
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("1 человек [1️⃣]", "people_1"),
                            InlineKeyboardButton.WithCallbackData("2 человек [2️⃣]", "people_2")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("3 человек [3️⃣]", "people_3"),
                            InlineKeyboardButton.WithCallbackData("4 человек [4️⃣]", "people_4")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("5 человек [5️⃣]", "people_5"),
                            InlineKeyboardButton.WithCallbackData("6 человек [6️⃣]", "people_6")
                        }
                        ,
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("< < < Назад", "start_order"),
                        }
                    });

                    await botClient.SendTextMessageAsync(chatId, responseMessagePeople, replyMarkup: inlineKeyboard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                if (callbackData.StartsWith("edit_2"))
                {
                    // Удаление сообщения из телеграм канала.
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    if (!userOrders.ContainsKey(chatId))
                    {
                        userOrders[chatId] = new UserOrder();
                    }

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                    new[]
                        {
                            InlineKeyboardButton.WithCallbackData("[➕] Начать", "people_"),
                        }
                    });

                    var messageStart = await botClient.SendTextMessageAsync(chatId, MessageStart, replyMarkup: inlineKeyboard);
                }

                if (callbackData.StartsWith("edit_3"))
                {
                    // Удаляем сообщение с кнопками
                    await botClient.DeleteMessageAsync(chatId, e.CallbackQuery.Message.MessageId);

                    var ChannelMessageFinalChannelEdit = $"[[🔴]] " + Environment.NewLine + Environment.NewLine + $"👤 Пользователь отменил заказ!" + Environment.NewLine + $"Последний заказ от {userOrder.Username} является отмененным. Для уточнения пишите пользователю!";

                    await botClient.SendTextMessageAsync(channelId, ChannelMessageFinalChannelEdit, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }

                #endregion

            }
            catch (ApiRequestException ex)
            {
                Console.WriteLine($"Ошибка API Telegram: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }
    }

    public class UserOrder
    {
        // Где-то в вашем коде объявите словарь для хранения данных о заказах пользователей
        private Dictionary<long, UserOrder> userOrders = new Dictionary<long, UserOrder>();

        public UserOrder GetUserOrderForUser(long chatId)
        {

            if (!userOrders.ContainsKey(chatId))
            {
                userOrders[chatId] = new UserOrder();
            }

            return userOrders[chatId];
        }

        public string Username { get; set; }
        public string PeopleNumber { get; set; }
        public string TableNumber { get; set; }
        public string Tabak { get; set; }
        public string Time { get; set; }
        public string Help { get; set; }
        public string Phone { get; set; }
    }
}
