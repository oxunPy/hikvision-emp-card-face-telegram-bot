using hikvision_emp_card_face_telegram_bot.Entity;
using Telegram.Bot.Types.ReplyMarkups;

namespace hikvision_emp_card_face_telegram_bot.Bot
{
    public class UserCommandMenus
    {
        public const string START = "/start";
        public const string REGISTER = "/register";
        public const string HELP = "/help";
               
        public const string I_DONT_EAT = "Bugun to'qman";
        public const string REPEAT_ORDER = "Qayta buyurtma berish";
        public const string DAILY_ORDER_REPORT = "Kundalik buyurtmalar hisoboti";
        public const string MONTHLY_ORDER_REPORT = "Oylik buyurtmalar hisoboti";
        public const string REFRESH_WEEKLY_MENU = "Menuni yangilash";

        public const string EMPLOYEE_POS_INL = "employee_";
        public const string CATERING_MANAGER_POS_INL = "catering_manager_";
        public const string MANAGER_POS_INL = "manager_";

        public const string YES = "yes_";
        public const string NO = "no_";

        public static ReplyKeyboardMarkup GetAccessibleCommands(Employee.Position? position)
        {
            switch (position)
            {
                case Employee.Position.EMPLOYEE:
                    return new ReplyKeyboardMarkup(new[] {
                                new KeyboardButton[] {I_DONT_EAT},
                                new KeyboardButton[] {REPEAT_ORDER}
                            });

                case Employee.Position.MANAGER:
                    return new ReplyKeyboardMarkup(new[] {
                                new KeyboardButton[] {I_DONT_EAT},
                                new KeyboardButton[] {REPEAT_ORDER},
                                new KeyboardButton[] {DAILY_ORDER_REPORT},
                                new KeyboardButton[] {MONTHLY_ORDER_REPORT}
                            });

                case Employee.Position.CATERING_MANAGER:
                    return new ReplyKeyboardMarkup(new[] {
                                new KeyboardButton[] {I_DONT_EAT},
                                new KeyboardButton[] {REPEAT_ORDER},
                                new KeyboardButton[] {REFRESH_WEEKLY_MENU}
                            });

                default:
                    return null;
            }
        }

        public static InlineKeyboardMarkup GetPositionMarkupInline(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] // Row 1
                        {
                            InlineKeyboardButton.WithCallbackData(Employee.Position.EMPLOYEE.ToString(), $"{EMPLOYEE_POS_INL}{chatId}"),
                        },
                        new [] // Row 2
                        {
                            InlineKeyboardButton.WithCallbackData(Employee.Position.CATERING_MANAGER.ToString(), $"{CATERING_MANAGER_POS_INL}{chatId}"),
                        },
                        new [] // Row 3
                        {
                            InlineKeyboardButton.WithCallbackData(Employee.Position.MANAGER.ToString(), $"{MANAGER_POS_INL}{chatId}"),
                        },
                    });

            return inlineKeyboard;
        }

        public static InlineKeyboardMarkup GetConfirmationMarkupInline(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] // Row 1
                        {
                            InlineKeyboardButton.WithCallbackData("XA", $"{YES}{chatId}"),
                            InlineKeyboardButton.WithCallbackData("YOQ", $"{NO}{chatId}"),
                        },
                    });

            return inlineKeyboard;
        }


        public static InlineKeyboardMarkup GetWeekDaysInlineMarkup()
        {
            // Create an inline keyboard with the days of the week
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                        new [] // Row 1
                        {
                            InlineKeyboardButton.WithCallbackData("Monday", "Monday"),
                            InlineKeyboardButton.WithCallbackData("Tuesday", "Tuesday"),
                        },
                        new [] // Row 2
                        {
                            InlineKeyboardButton.WithCallbackData("Wednesday", "Wednesday"),
                            InlineKeyboardButton.WithCallbackData("Thursday", "Thursday"),
                        },
                        new [] // Row 3
                        {
                            InlineKeyboardButton.WithCallbackData("Friday", "Friday"),
                            InlineKeyboardButton.WithCallbackData("Saturday", "Saturday"),
                        },
                        new [] // Row 4
                        {
                            InlineKeyboardButton.WithCallbackData("Sunday", "Sunday"),
                        },
                    });

            return inlineKeyboard;
        }
    }
}
