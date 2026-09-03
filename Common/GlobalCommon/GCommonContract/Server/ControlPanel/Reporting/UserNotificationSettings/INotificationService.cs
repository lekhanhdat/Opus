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



using System.Collections.Generic;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface INotificationService
    {
        /// <summary>
        /// 保存NotificationSettingDto对象.
        /// </summary>
        /// <param name="dto">要保存的dto.</param>
        [OperationContract]
        NotificationSettingResult SaveNotificationSetting(NotificationSettingDto dto, bool forTest);

        /// <summary>
        /// 获取NotificationSettingDto
        /// </summary>
        /// <returns>dto</returns>
        [OperationContract]
        NotificationSettingDto GetNotificationSetting();

        /// <summary>
        /// 创建NotificationMessage
        /// </summary>
        /// <param name="notifyMessage">notificationMessage对象封装在ProfileDto的Content属性中</param>
        /// <exception cref="AveException">操作出现问题以异常形式返给前台</exception> 
        [OperationContract]
        List<ProfileDto> CreateNotificationMessage(ProfileDto notifyMessage);
        /// <summary>
        /// 编辑NotificationMessage
        /// </summary>
        /// <param name="notifyMessage">notificationMessage对象封装在ProfileDto的Content属性中</param>
        /// <exception cref="AveException">操作出现问题以异常形式返给前台</exception> 
        [OperationContract]
        List<ProfileDto> UpdateNotificationMessage(ProfileDto notifyMessage);
        /// <summary>
        /// 删除NotificationMessage
        /// </summary>
        /// <param name="notifyMessage">ProfileDto的id</param>
        /// <exception cref="AveException">操作出现问题以异常形式返给前台</exception> 
        [OperationContract]
        List<ProfileDto> DeleteNotificationMessage(List<string> ids);
        /// <summary>
        /// 获得所有NotificationMessage
        /// </summary>
        /// <param name="notifyMessage">notificationMessage对象封装在ProfileDto的Content属性中</param>
        [OperationContract]
        List<ProfileDto> GetAllNotificationMessages();
        /// <summary>
        /// 获得所有type为Service类型的NotificationMessage
        /// </summary>
        /// <param name="notifyMessage">notificationMessage对象封装在ProfileDto的Content属性中</param>
        [OperationContract]
        List<ProfileDto> GetAllServiceNotificationMessages();
        /// <summary>
        /// 获得所有type为Global类型的NotificationMessage
        /// </summary>
        /// <param name="notifyMessage">notificationMessage对象封装在ProfileDto的Content属性中</param>
        [OperationContract]
        List<ProfileDto> GetAllGlobalNotificationMessages();
        /// <summary>
        /// 获取是否有可用的设置
        /// </summary>
        /// <returns>dto</returns>
        [OperationContract]
        bool HasUseableSetting();
         [OperationContract]
        ProfileDto GetNotificationMessage(string id);

        /// <summary>
        /// 通过ProfileId获得NotificationMessage
        /// </summary>
        /// <param name="notifyMessage">notificationMessage对象封装在ProfileDto的Content属性中</param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [OperationContract]
        ProfileDto GetNotificationMessageByProfileId(string profileId);

    }
}
