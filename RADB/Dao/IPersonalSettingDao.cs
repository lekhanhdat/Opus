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
using AvePoint.RA.Contract.PersonalSetting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IPersonalSettingDao
    {
        bool ExistSameNameEntity(RMPersonalSettingDto dto);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>entity id</returns>
        int CreateOrUpdate(RMPersonalSettingDto dto);
        List<RMPersonalSettingDto> GetByOwnerAndType(string owner, PersonalSettingType type, bool includeContent = false);
        List<RMPersonalSettingDto> GetByOwnerAndTypeForGoogleOne(string owner, PersonalSettingType type);

        /// <summary>
        /// get settings shared to the user represented by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns>settings without content</returns>
        List<RMPersonalSettingDto> GetSharedSettings(string userId, PersonalSettingType type);
        RMPersonalSettingDto GetById(int id, bool includeContent = true);
        Task<int> DeleteByIdsAsync(string owner, List<int> ids);

        /// <summary>
        /// set the setting as the default setting for the user
        /// </summary>
        /// <param name="settingId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool SetAsDefault(int settingId, string userId);
        /// <summary>
        /// upgrade the old IsDefault value
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="type"></param>
        void UpgradeDefaultSetting(string owner, PersonalSettingType type);
        /// <summary>
        /// if exist built-in setting in DB
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        bool ExistsBuiltIn(string owner, PersonalSettingType type);

        /// <summary>
        /// if exist default setting in DB
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        bool ExistsDefault(string owner, PersonalSettingType type);

        /// <summary>
        /// update date built-in setting as default one
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="type"></param>
        void SetBuiltInAsDefault(string owner, PersonalSettingType type);

        /// <summary>
        /// share personal setting with groups
        /// </summary>
        /// <param name="id">setting id</param>
        /// <param name="securityGroups">security group id list</param>
        void Share(int id, List<int> securityGroups);
        /// <summary>
        /// cancel sharing with others
        /// </summary>
        /// <param name="id">personal setting id</param>
        void CancelShare(int id);

        List<int> GetSharedGroups(int id);

        /// <summary>
        /// check if setting is shared to the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="settingId"></param>
        /// <returns></returns>
        bool IsSharedToUser(string userId, int settingId);
        Task<bool> SetAsDefaultForGoogleOne(int settingId, string userId);

    }
}
