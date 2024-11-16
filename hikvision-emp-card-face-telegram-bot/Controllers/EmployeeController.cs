
using CardManagement;
using FaceManagement;
using hikvision_emp_card_face_telegram_bot.Dto;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using hikvision_emp_card_face_telegram_bot.Repository;
using AutoMapper;
using hikvision_emp_card_face_telegram_bot.Interfaces;

namespace hikvision_emp_card_face_telegram_bot.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class EmployeeController
    {
        public static Int32 m_lGetCardCfgHandle = -1;
        public static Int32 m_lSetCardCfgHandle = -1;
        public static Int32 m_lDelCardCfgHandle = -1;

        public static Int32 m_lSetFaceCfgHandle = -1;

        private readonly ILogger<EmployeeController> _logger;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _logger = logger;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        [HttpGet("/page-employees")]
        public ICollection<EmployeeDTO> getAllEmployeesByPage([FromQuery] int page = 0, 
                                                              [FromQuery] int size = 10)
        {
            return new List<EmployeeDTO>();
        }

        [HttpPost("/add-employee")]
        public void CreateNewEmployee([FromQuery(Name = "card_no")] String cardNo,
                                             [FromQuery(Name = "card_right_plan")] String cardRightPlan,
                                             [FromQuery(Name = "employee_no") ] String employeeNo,
                                             [FromQuery(Name = "name")] String name)
        {
            if(m_lSetCardCfgHandle != -1)
            {
                if(CHCNetSDKForCard.NET_DVR_StopRemoteConfig(m_lSetCardCfgHandle))
                {
                    m_lSetCardCfgHandle = -1;
                }
            }

            CHCNetSDKForCard.NET_DVR_CARD_COND struCond = new CHCNetSDKForCard.NET_DVR_CARD_COND();
            struCond.Init();
            struCond.dwSize = (uint) Marshal.SizeOf(struCond);
            struCond.dwCardNum = 1;
            IntPtr ptrStruCond = Marshal.AllocHGlobal((int) struCond.dwSize);
            Marshal.StructureToPtr(struCond, ptrStruCond, false);

            m_lSetCardCfgHandle = CHCNetSDKForCard.NET_DVR_StartRemoteConfig(TerminalConfigurationController.m_UserID, CHCNetSDKForCard.NET_DVR_SET_CARD, ptrStruCond, (int)struCond.dwSize, null, IntPtr.Zero);
            if(m_lSetCardCfgHandle < 0)
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

        [HttpPost("/upload-face")]
        public async void UploadFaceEmployee(IFormFile file)
        {
            if (file == null || file.Length == 0) 
            {
                _logger.LogError("No file uploaded.");
            }

            try
            {
                // Check if the file is an image by examining the content type
                if(!file.ContentType.StartsWith("image/"))
                {
                    _logger.LogError("Uploaded file is not an image.");
                }

                // Define the directory to save the image
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "faces");

                // Ensure that the directory exists
                if(!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                // Generate a unique filename to avoid overwriting existing files
                var filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(savePath, filename);

                // Save the file to the server
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                SendFaceData(filePath);

                // Return success and the file path
                //  Ok(new { FilePath = $"/uploads/{filename}" });
                _logger.LogInformation($"/uploads/{filename}");

            }
            catch (Exception ex)
            {
                return;
            }
        }

        [HttpGet("/get-by-chatId")]
        public EmployeeDTO GetEmployeeByChatID([FromQuery(Name = "chat_id")] long chatID)
        {
            return _mapper.Map<EmployeeDTO>(_employeeRepository.FindByTelegramChatId(chatID));
        }


        [HttpGet("/exists-menu")]
        public bool CheckExistsSelectedMenu([FromQuery(Name = "chat_id")] long chatID)
        {
            return _employeeRepository.ExistsEmployeeSelectedMenu(chatID);
        }


        private void SendCardData(String cardNo, String cardRightPlan, String employeeNo, String name)
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

        private void SendFaceData(string filePath)
        {
            if (filePath == null || filePath.Length == 0)
            {
                _logger.LogError("Please choose human Face path");
                return;
            }

            string cardReaderNo = "1";
            string cardNo = "9771";

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
            IntPtr ptrstruCond = Marshal.AllocHGlobal((int) dwInBufferSize);
            Marshal.StructureToPtr(struCond, ptrstruCond, false);
            m_lSetFaceCfgHandle = CHCNetSDKForFace.NET_DVR_StartRemoteConfig(TerminalConfigurationController.m_UserID, CHCNetSDKForCard.NET_DVR_SET_FACE, ptrstruCond, dwInBufferSize, null, IntPtr.Zero);
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
            while( Flag )
            {
                dwStatus = CHCNetSDKForFace.NET_DVR_SendWithRecvRemoteConfig(m_lSetFaceCfgHandle, ref struRecord, dwInBuffSize, ref struStatus, dwOutBuffSize, ptrOutDataLen);

                switch(dwStatus)
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
            if (!File.Exists(filePath))
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
                int iLen = (int) struRecord.dwFaceLen;
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
