/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */




using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
using AvePoint.GCommon.Contract.Server.Common.TimeZone;
using AvePoint.GCommon.Contract.Server.Login;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting
{
    /// <summary>
    /// SystemSettingService interface
    /// </summary>
    public interface ISystemOptionService
    {

        /// <summary>
        /// get SystemSettingContent
        /// </summary>
        /// <returns>SystemSettingContent</returns>
        SystemSettingContent GetSystemSettings();


        /// <summary>
        /// 用于获取用户保存的logo图像
        /// </summary>
        /// <returns>用户保存过则返回正确的值，没保存返回默认的logo</returns>
        byte[] GetLogoImage();

        /// <summary>
        /// 获取默认的logo图像
        /// </summary>
        /// <returns></returns>
        byte[] GetDefaultLogoImage();

        /// <summary>
        /// 初始化timeZone
        /// </summary>
        /// <returns></returns>
        TimeSettingDto InitTimeSetting();

        /// <summary>
        /// 获取可供使用的语言
        /// </summary>
        /// <returns></returns>
        List<LanguageDto> GetAllUsableLanguages();

        /// <summary>
        /// 获取语言环境信息
        /// </summary>
        /// <returns></returns>
        LanguageDto GetLanguageCulture();

        /// <summary>
        /// 获取用户设置的warning提示
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        UserConfirmDto LoadUserConfirmByUserName(string userName);

        /// <summary>
        /// 重置用户设置的warning提示
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [OperationContract]
        SystemSettingResult ResetWarningByUser(string userName);

        List<I18NMessageDto> GetAllI18NMessageDtoByModule(string module);

        void InitI18NMessage();

        /// <summary>
        /// 生成国际化语言包
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        void UpdateI18NResource(List<I18NMessageDto> messageDtos, string culture);
        [OperationContract]
        void GenerateI18NData(SystemSettingDto settingDto, string culture);

        string GetUserCultureSetting(SystemSettingContent content);

        string GetUseBrowserCulture();

        /// <summary>
        /// 添加SystemSetting数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        string Create(SystemSettingDto dto);

        /// <summary>
        /// 更新SystemSytting数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        string Update(SystemSettingDto dto);

        /// <summary>
        /// 根据Type获取SystemSetting数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        List<SystemSettingDto> GetByType<T>(SystemSettingType type);

        /// <summary>
        /// 设置是否开启schedule checker task
        /// 此方法包含update 或 create 数据库数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string SetScheduleCheckerTaskController(bool type);
    }
}