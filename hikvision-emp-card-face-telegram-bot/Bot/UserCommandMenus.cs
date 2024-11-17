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
        public const string REPEAT_ORDER = "Buyurtma berish";
        public const string DAILY_ORDER_REPORT = "Kundalik buyurtmalar hisoboti";
        public const string MONTHLY_ORDER_REPORT = "Oylik buyurtmalar hisoboti";
        public const string REFRESH_WEEKLY_MENU = "Menuni yangilash";

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
    }
}
