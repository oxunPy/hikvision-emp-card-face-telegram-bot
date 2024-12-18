﻿using hikvision_emp_card_face_telegram_bot.bot.State;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Service.Impl;

namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface IEmployeeService
    {
        EmployeeDTO CreateBotUser(long chatId);

        EmployeeDTO? UpdateBotUserVisitDate(EmployeeDTO dto);

        EmployeeDTO FindByChatID(long chatId);

        EmployeeService.CodeResultRegistration? RegisterByChatID(long chatId);

        bool UpdateByChatID(long chatId, RegistrationStates state, EmployeeDTO dto);

        bool RemoveImgInCaseErrorRecognize(long chatID);

        void CreateNewHikiEmployee(long chatId);

        void SendFaceData(long chatId, string filePath);

        bool OrderedTodaysMenu(long chatId);
    }
}
