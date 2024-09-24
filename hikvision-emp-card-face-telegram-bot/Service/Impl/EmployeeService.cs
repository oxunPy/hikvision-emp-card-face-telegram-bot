using AutoMapper;
using CardManagement;
using FaceManagement;
using hikvision_emp_card_face_telegram_bot.bot.State;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using hikvision_emp_card_face_telegram_bot.Repository;
using System.Data;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Telegram.Bot.Types;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ITerminalConfigurationService _terminalConfigurationService;
        private readonly ILogger<EmployeeService> _logger;

        // card (set, get, del)
        public static Int32 m_lGetCardCfgHandle = -1;
        public static Int32 m_lSetCardCfgHandle = -1;
        public static Int32 m_lDelCardCfgHandle = -1;
        // face (set)
        public static Int32 m_lSetFaceCfgHandle = -1;

        public enum CodeResultRegistration
        {
            FIRST_NAME,
            LAST_NAME,
            EMPLOYEE_POSITION,
            FACE_UPLOAD,
            COMPLETE
        }

        public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper, ITerminalConfigurationService terminalConfigurationService, ILogger<EmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _terminalConfigurationService = terminalConfigurationService;
            _logger = logger;
        }

        public EmployeeDTO CreateBotUser(long chatId)
        {
            Employee newEmployee = new Employee();
            newEmployee.TelegramChatId = chatId.ToString();
            newEmployee.HikCardCode = chatId.ToString();

            bool result = _employeeRepository.CreateEmployee(newEmployee);
            if(result)
            {
                return _mapper.Map<EmployeeDTO>(newEmployee);
            }
            
            return null;
        }

        public CodeResultRegistration? FindByChatID(long chatId)
        {
            var employee = _employeeRepository.FindByTelegramChatId(chatId);
            
            if (employee == null)
            {
                return null;
            }
            else if (employee.FirstName == null)
            {
                return CodeResultRegistration.FIRST_NAME;
            }

            else if (employee.LastName == null)
            {
                return CodeResultRegistration.LAST_NAME;
            }

            else if (employee.PositionEmp == null)
            {
                return CodeResultRegistration.EMPLOYEE_POSITION;
            }
            else if (employee.FaceImagePath == null)
            {
                return CodeResultRegistration.FACE_UPLOAD;
            }

            return CodeResultRegistration.COMPLETE;
        }

        public bool UpdateByChatID(long chatId, RegistrationStates state, EmployeeDTO dto)
        {
            if(state == null || dto == null)
                return false;

            Employee employee = _employeeRepository.FindByTelegramChatId(chatId);
            if(employee == null) return false;

            switch(state)
            {
                case RegistrationStates.FIRST_NAME:
                    employee.FirstName = dto.FirstName;
                    break;
                case RegistrationStates.LAST_NAME:
                    employee.LastName = dto.LastName;
                    break;
                case RegistrationStates.EMPLOYEE_POSITION:
                    employee.PositionEmp = dto.PositionEmp;
                    break;
                case RegistrationStates.FACE_UPLOAD:
                    employee.FaceImagePath = dto.FaceImagePath;
                    break;
            }

            return _employeeRepository.UpdateEmployee(employee);
        }

        public void CreateNewHikiEmployee(long chatId)
        {
            _terminalConfigurationService.AutoLogin("admin", "JDev@2022");

            if (m_lSetCardCfgHandle != -1)
            {
                if (CHCNetSDKForCard.NET_DVR_StopRemoteConfig(m_lSetCardCfgHandle))
                {
                    m_lSetCardCfgHandle = -1;
                }
            }

            EmployeeDTO botUserEmployee = _mapper.Map<EmployeeDTO>(_employeeRepository.FindByTelegramChatId(chatId));
            if(botUserEmployee == null) return;

            string cardNo = chatId.ToString();
            string cardRightPlan = "1";
            string employeeNo = botUserEmployee.Id.ToString();
            string name = $"{botUserEmployee.FirstName} {botUserEmployee.LastName}";

            CHCNetSDKForCard.NET_DVR_CARD_COND struCond = new CHCNetSDKForCard.NET_DVR_CARD_COND();
            struCond.Init();
            struCond.dwSize = (uint)Marshal.SizeOf(struCond);
            struCond.dwCardNum = 1;
            IntPtr ptrStruCond = Marshal.AllocHGlobal((int)struCond.dwSize);
            Marshal.StructureToPtr(struCond, ptrStruCond, false);

            m_lSetCardCfgHandle = CHCNetSDKForCard.NET_DVR_StartRemoteConfig(TerminalConfigurationService.m_UserID, CHCNetSDKForCard.NET_DVR_SET_CARD, ptrStruCond, (int)struCond.dwSize, null, IntPtr.Zero);
            if (m_lSetCardCfgHandle < 0)
            {
                _logger.LogError("NET_DVR_SET_CARD error: " + CHCNetSDKForCard.NET_DVR_GetLastError());
                Marshal.FreeHGlobal(ptrStruCond);
                return;
            }
            else
            {
                SendCardData(cardNo, cardRightPlan, employeeNo, name);
                Marshal.FreeHGlobal(ptrStruCond);
            }
        }

        private void SendCardData(string cardNo, string cardRightPlan, string employeeNo, string name)
        {
            CHCNetSDKForCard.NET_DVR_CARD_RECORD struData = new CHCNetSDKForCard.NET_DVR_CARD_RECORD();
            struData.Init();
            struData.dwSize = (uint)Marshal.SizeOf(struData);
            struData.byCardType = 1;
            byte[] byTempCardNo = new byte[CHCNetSDKForCard.ACS_CARD_NO_LEN];
            byTempCardNo = System.Text.Encoding.UTF8.GetBytes(cardNo);
            for (int i = 0; i < byTempCardNo.Length; i++)
            {
                struData.byCardNo[i] = byTempCardNo[i];
            }

            ushort.TryParse(cardRightPlan, out struData.wCardRightPlan[0]);
            uint.TryParse(employeeNo, out struData.dwEmployeeNo);
            byte[] byTempName = new byte[CHCNetSDKForCard.NAME_LEN];
            byTempName = System.Text.Encoding.Default.GetBytes(name);
            for (int i = 0; i < byTempName.Length; i++)
            {
                struData.byName[i] = byTempName[i];
            }
            struData.struValid.byEnable = 1;
            struData.struValid.struBeginTime.wYear = 2000;
            struData.struValid.struBeginTime.byMonth = 1;
            struData.struValid.struBeginTime.byDay = 1;
            struData.struValid.struBeginTime.byHour = 11;
            struData.struValid.struBeginTime.byMinute = 11;
            struData.struValid.struBeginTime.bySecond = 11;
            struData.struValid.struEndTime.wYear = 2030;
            struData.struValid.struEndTime.byMonth = 1;
            struData.struValid.struEndTime.byDay = 1;
            struData.struValid.struEndTime.byHour = 11;
            struData.struValid.struEndTime.byMinute = 11;
            struData.struValid.struEndTime.bySecond = 11;
            struData.byDoorRight[0] = 1;
            struData.wCardRightPlan[0] = 1;
            IntPtr ptrStruData = Marshal.AllocHGlobal((int)struData.dwSize);
            Marshal.StructureToPtr(struData, ptrStruData, false);

            CHCNetSDKForCard.NET_DVR_CARD_STATUS struStatus = new CHCNetSDKForCard.NET_DVR_CARD_STATUS();
            struStatus.Init();
            struStatus.dwSize = (uint)Marshal.SizeOf(struStatus);
            IntPtr ptrdwState = Marshal.AllocHGlobal((int)struStatus.dwSize);
            Marshal.StructureToPtr(struStatus, ptrdwState, false);

            int dwState = (int)CHCNetSDKForCard.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS;
            uint dwReturned = 0;
            while (true)
            {
                dwState = CHCNetSDKForCard.NET_DVR_SendWithRecvRemoteConfig(m_lSetCardCfgHandle, ptrStruData, struData.dwSize, ptrdwState, struStatus.dwSize, ref dwReturned);
                struStatus = (CHCNetSDKForCard.NET_DVR_CARD_STATUS)Marshal.PtrToStructure(ptrdwState, typeof(CHCNetSDKForCard.NET_DVR_CARD_STATUS));
                if (dwState == (int)CHCNetSDKForCard.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_NEEDWAIT)
                {
                    Thread.Sleep(10);
                    continue;
                }
                else if (dwState == (int)CHCNetSDKForCard.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FAILED)
                {
                    _logger.LogError("NET_DVR_SET_CARD fail error: " + CHCNetSDKForCard.NET_DVR_GetLastError());
                }
                else if (dwState == (int)CHCNetSDKForCard.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS)
                {
                    if (struStatus.dwErrorCode != 0)
                    {
                        _logger.LogError("NET_DVR_SET_CARD success but errorCode:" + struStatus.dwErrorCode);
                    }
                    else
                    {
                        _logger.LogInformation("NET_DVR_SET_CARD success");
                    }
                }
                else if (dwState == (int)CHCNetSDKForCard.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FINISH)
                {
                    _logger.LogInformation("NET_DVR_SET_CARD finish");
                    break;
                }
                else if (dwState == (int)CHCNetSDKForCard.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_EXCEPTION)
                {
                    _logger.LogError("NET_DVR_SET_CARD exception error: " + CHCNetSDKForCard.NET_DVR_GetLastError());
                    break;
                }
                else
                {
                    _logger.LogError("unknown status error: " + CHCNetSDKForCard.NET_DVR_GetLastError());
                    break;
                }
            }
            CHCNetSDKForCard.NET_DVR_StopRemoteConfig(m_lSetCardCfgHandle);
            m_lSetCardCfgHandle = -1;
            Marshal.FreeHGlobal(ptrStruData);
            Marshal.FreeHGlobal(ptrdwState);
            return;
        }

        public void SendFaceData(long chatId, string filePath)
        {
            if (filePath == null || filePath.Length == 0)
            {
                _logger.LogError("Please choose human Face path");
                return;
            }

            EmployeeDTO botUserEmployee = _mapper.Map<EmployeeDTO>(_employeeRepository.FindByTelegramChatId(chatId));
            if (botUserEmployee == null) return;

            string cardReaderNo = "1";
            string cardNo = botUserEmployee.HikCardCode;

            CHCNetSDKForFace.NET_DVR_FACE_COND struCond = new CHCNetSDKForFace.NET_DVR_FACE_COND();
            struCond.init();
            struCond.dwSize = Marshal.SizeOf(struCond);
            struCond.dwFaceNum = 1;
            int.TryParse(cardReaderNo, out struCond.dwEnableReaderNo);
            byte[] byTemp = System.Text.Encoding.UTF8.GetBytes(cardNo);
            for (int i = 0; i < byTemp.Length; i++)
            {
                struCond.byCardNo[i] = byTemp[i];
            }

            int dwInBufferSize = struCond.dwSize;
            IntPtr ptrstruCond = Marshal.AllocHGlobal((int)dwInBufferSize);
            Marshal.StructureToPtr(struCond, ptrstruCond, false);
            m_lSetFaceCfgHandle = CHCNetSDKForFace.NET_DVR_StartRemoteConfig(TerminalConfigurationService.m_UserID, CHCNetSDKForCard.NET_DVR_SET_FACE, ptrstruCond, dwInBufferSize, null, IntPtr.Zero);
            if (-1 == m_lSetFaceCfgHandle)
            {
                Marshal.FreeHGlobal(ptrstruCond);
                _logger.LogInformation("NET_DVR_SET_FACE_FAIL, ERROR CODE" + CHCNetSDKForCard.NET_DVR_GetLastError().ToString());
                return;
            }

            CHCNetSDKForFace.NET_DVR_FACE_RECORD struRecord = new CHCNetSDKForFace.NET_DVR_FACE_RECORD();
            struRecord.init();
            struRecord.dwSize = Marshal.SizeOf(struRecord);

            byte[] byRecordNo = System.Text.Encoding.UTF8.GetBytes(cardNo);
            for (int i = 0; i < byRecordNo.Length; i++)
            {
                struRecord.byCardNo[i] = byRecordNo[i];
            }

            ReadFaceData(ref struRecord, filePath);
            int dwInBuffSize = Marshal.SizeOf(struRecord);
            int dwStatus = 0;


            CHCNetSDKForFace.NET_DVR_FACE_STATUS struStatus = new CHCNetSDKForFace.NET_DVR_FACE_STATUS();
            struStatus.init();
            struStatus.dwSize = Marshal.SizeOf(struStatus);
            int dwOutBuffSize = struStatus.dwSize;
            IntPtr ptrOutDataLen = Marshal.AllocHGlobal(sizeof(int));
            bool Flag = true;
            while (Flag)
            {
                dwStatus = CHCNetSDKForFace.NET_DVR_SendWithRecvRemoteConfig(m_lSetFaceCfgHandle, ref struRecord, dwInBuffSize, ref struStatus, dwOutBuffSize, ptrOutDataLen);

                switch (dwStatus)
                {
                    case CHCNetSDKForFace.NET_SDK_GET_NEXT_STATUS_SUCCESS:
                        ProcessSetFaceData(ref struStatus, ref Flag);
                        break;

                    case CHCNetSDKForFace.NET_SDK_GET_NEXT_STATUS_NEED_WAIT:
                        break;

                    case CHCNetSDKForFace.NET_SDK_GET_NEXT_STATUS_FAILED:
                        CHCNetSDKForFace.NET_DVR_StopRemoteConfig(m_lSetFaceCfgHandle);
                        _logger.LogError("NET_SDK_GET_NEXT_STATUS_FAILED" + CHCNetSDKForCard.NET_DVR_GetLastError().ToString());
                        Flag = false;
                        break;

                    case CHCNetSDKForFace.NET_SDK_GET_NEXT_STATUS_FINISH:
                        CHCNetSDKForFace.NET_DVR_StopRemoteConfig(m_lSetFaceCfgHandle);
                        Flag = false;
                        break;

                    default:
                        _logger.LogError("NET_SDK_GET_NEXT_STATUS_UNKOWN" + CHCNetSDKForCard.NET_DVR_GetLastError().ToString());
                        Flag = false;
                        CHCNetSDKForFace.NET_DVR_StopRemoteConfig(m_lSetFaceCfgHandle);
                        break;
                }
            }

            Marshal.FreeHGlobal(ptrstruCond);
            Marshal.FreeHGlobal(ptrOutDataLen);
        }

        private void ReadFaceData(ref CHCNetSDKForFace.NET_DVR_FACE_RECORD struRecord, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError("The face picture does not exist!");
                return;
            }

            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
            if (0 == fs.Length)
            {
                _logger.LogError("The face picture is 0k,please input another picture!");
                return;
            }
            if (200 * 1024 < fs.Length)
            {
                _logger.LogError("The face picture is larger than 200k,please input another picture!");
                return;
            }
            try
            {
                int.TryParse(fs.Length.ToString(), out struRecord.dwFaceLen);
                int iLen = (int)struRecord.dwFaceLen;
                byte[] by = new byte[iLen];
                struRecord.pFaceBuffer = Marshal.AllocHGlobal(iLen);
                fs.Read(by, 0, iLen);
                Marshal.Copy(by, 0, struRecord.pFaceBuffer, iLen);
                fs.Close();
            }
            catch
            {
                _logger.LogError("Read Face Data failed");
                fs.Close();
                return;
            }
        }

        private void ProcessSetFaceData(ref CHCNetSDKForFace.NET_DVR_FACE_STATUS struStatus, ref bool flag)
        {
            switch (struStatus.byRecvStatus)
            {
                case 1:
                    _logger.LogInformation("SetFaceDataSuccessful");
                    break;
                default:
                    flag = false;
                    _logger.LogError("NET_SDK_SET_Face_DATA_FAILED" + struStatus.byRecvStatus.ToString());
                    break;
            }

        }
    }
}
