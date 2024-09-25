using CardManagement;
using System.Runtime.InteropServices;
using static CardManagement.CHCNetSDKForCard;
using System.Text;
using Telegram.Bot;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class TerminalConfigurationService : ITerminalConfigurationService
    {
        public static int m_UserID = -1;
        private readonly ILogger<TerminalConfigurationService> _logger;
        private readonly TelegramBotClient _telegramBotClient;
        private readonly IDishService _dishService;

        public TerminalConfigurationService(ILogger<TerminalConfigurationService> logger, TelegramBotClient telegramBotClient, IDishService dishService) 
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _dishService = dishService;
        }


        public void AutoLogin(string username, string password)
        {
            if (m_UserID >= 0)
            {
                CHCNetSDKForCard.NET_DVR_Logout_V30(m_UserID);
            }

            CHCNetSDKForCard.NET_DVR_USER_LOGIN_INFO struLoginInfo = new CHCNetSDKForCard.NET_DVR_USER_LOGIN_INFO();
            CHCNetSDKForCard.NET_DVR_DEVICEINFO_V40 struDeviceInfoV40 = new CHCNetSDKForCard.NET_DVR_DEVICEINFO_V40();
            struDeviceInfoV40.struDeviceV30.sSerialNumber = new byte[CHCNetSDKForCard.SERIALNO_LEN];

            struLoginInfo.sDeviceAddress = "192.168.7.33";
            struLoginInfo.sUserName = username;
            struLoginInfo.sPassword = password;
            ushort.TryParse("8000", out struLoginInfo.wPort);

            int lUserID = CHCNetSDKForCard.NET_DVR_Login_V40(ref struLoginInfo, ref struDeviceInfoV40);

            if (lUserID >= 0)
            {
                m_UserID = lUserID;
                _logger.LogInformation("Login Successful");

                bool b = CHCNetSDKForCard.NET_DVR_SetDVRMessageCallBack_V31(SetupAlarmChanCallback, IntPtr.Zero);
                if (b)
                {
                    _logger.LogInformation("Set callback successfully");
                }

                //Arming parameters
                CHCNetSDKForCard.NET_DVR_SETUPALARM_PARAM m_strAlarmInfo = new CHCNetSDKForCard.NET_DVR_SETUPALARM_PARAM();
                m_strAlarmInfo.dwSize = (uint)Marshal.SizeOf(m_strAlarmInfo);
                m_strAlarmInfo.byLevel = 1; //布防优先级：0- 一等级（高），1- 二等级（中）
                m_strAlarmInfo.byAlarmInfoType = 1; //上传报警信息类型: 0- 老报警信息(NET_DVR_PLATE_RESULT), 1- 新报警信息(NET_ITS_PLATE_RESULT)
                m_strAlarmInfo.byDeployType = 1; //布防类型：0-客户端布防，1-实时布
                int lAlarmHandle = CHCNetSDKForCard.NET_DVR_SetupAlarmChan_V41(m_UserID, ref m_strAlarmInfo);
                //如果布防失败返回-1
                if (lAlarmHandle < 0)
                {
                    _logger.LogError("布防失败！ code：" + CHCNetSDKForCard.NET_DVR_GetLastError());
                    CHCNetSDKForCard.NET_DVR_Logout_V30(m_UserID);  //注销
                    CHCNetSDKForCard.NET_DVR_Cleanup(); //释放SDK资源
                }
            }
            else
            {
                uint nErr = CHCNetSDKForCard.NET_DVR_GetLastError();
                if (nErr == CHCNetSDKForCard.NET_DVR_PASSWORD_ERROR)
                {
                    _logger.LogError("user name or password error!");
                    if (1 == struDeviceInfoV40.bySupportLock)
                    {
                        string strTemp1 = string.Format("Left {0} try oppportunity", struDeviceInfoV40.byRetryLoginTime);
                        _logger.LogError(strTemp1);
                    }
                }

                else if (nErr == CHCNetSDKForCard.NET_DVR_USER_LOCKED)
                {
                    if (1 == struDeviceInfoV40.bySupportLock)
                    {
                        string strTemp1 = string.Format("user is locked, the remaining lock time is {0}", struDeviceInfoV40.dwSurplusLockTime);
                        _logger.LogError(strTemp1);
                    }
                }

                else
                {
                    _logger.LogError("net error or dvr is busy!");
                }
            }
        }

        private bool SetupAlarmChanCallback(int lCommand, ref NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            if (lCommand == CHCNetSDKForCard.COMM_ALARM_ACS)
            {
                // Allocate the structure and populate it using Marshal.PtrToStructure
                CHCNetSDKForCard.NET_DVR_ACS_ALARM_INFO strCardResult =
                    (CHCNetSDKForCard.NET_DVR_ACS_ALARM_INFO)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDKForCard.NET_DVR_ACS_ALARM_INFO));

                if (strCardResult.dwMinor == CHCNetSDKForCard.MINOR_FACE_VERIFY_PASS)
                {
                    // Convert the byte array (byCardNo) to a string (Assuming UTF-8 encoding)
                    string cardNo = Encoding.UTF8.GetString(strCardResult.struAcsEventInfo.byCardNo).TrimEnd('\0'); // Remove trailing null characters

                    // Log employee number and card number
                    _logger.LogInformation($"Employee No: {strCardResult.struAcsEventInfo.dwEmployeeNo}");
                    _logger.LogInformation($"Card No: {cardNo}");

                    _telegramBotClient.SendTextMessageAsync(
                        chatId: long.Parse(cardNo),
                        text: "Yuzingiz tanildi!"
                        );
                }
            }
            return true;
        }
    }
}
