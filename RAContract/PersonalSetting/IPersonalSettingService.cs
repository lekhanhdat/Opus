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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.PersonalSetting
{
    public interface IPersonalSettingService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>entity id</returns>
        int Save(RMPersonalSettingDto dto);
        RMPersonalSettingDto GetByOwnerAndId(string userId, int id);

        /// <summary>
        /// will not return setting value
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        List<RMPersonalSettingDto> GetByOwnerAndType(string userId, PersonalSettingType type);
        List<RMPersonalSettingDto> GetByOwnerAndTypeForGoogleOne(string userId, PersonalSettingType type);


        RMPersonalSettingDto GetById(int id, bool includeContent = true);
        /// <summary>
        /// get the settings which are shared to the user represented by userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        List<RMPersonalSettingDto> GetSharedSettings(string userId, PersonalSettingType type);


        Task<bool> DeleteAsync(RMPersonalSettingDto dto);

        bool SetAsDefault(RMPersonalSettingDto param);

        /// <summary>
        /// if there is a builtin setting in DB
        /// </summary>
        /// <returns></returns>
        bool ExistsBuiltIn(RMPersonalSettingDto param);

        /// <summary>
        /// share with other security groups. note that, only the owner can share the setting.
        /// </summary>
        /// <param name="dto"></param>
        void Share(RMPersonalSettingSecurityGroupMappingDto dto);

        /// <summary>
        /// cancel sharing with others. note that, only the owner can cancel sharing.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id">setting id</param>
        void CancelShare(string userId, int id);

        /// <summary>
        /// get the shared groups of a setting
        /// </summary>
        /// <param name="id">setting id</param>
        /// <returns></returns>
        RMGlobalSearchSharedSettingDto GetSharedInfo(int id);

        /// <summary>
        /// check if the setting is shared to user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id">setting id</param>
        /// <returns></returns>
        bool IsSharedToUser(string userId, int id);

        string RunSearchOffline(int id);
        Task<string> RealRunSearchOfflineAsync(RMWeb.JobRunBy jobRunBy, string jobRunByUser, int settingId, string userId);

        /// <summary>
        /// for old data, default setting value is saved as a column, however, in order to supporting 'Share' function, a new table is used to save the default setting.
        /// therefore, it need to upgrade the default settings.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="type"></param>
        void UpgradeDefaultSetting(string owner, PersonalSettingType type);
        Task<bool> SetAsDefaultForGoogleOne(RMPersonalSettingDto param);


    }
}
