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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365AppToken.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365AppToken
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMOffice365AppTokenService
    {
        [OperationContract]
        List<ProfileDto> GetAllAppProfiles(params AppProfileState[] status);
        [OperationContract]
        string Save(ProfileDto profile);
        [OperationContract]
        ProfileDto GetAppProfile(string id);
        [OperationContract]
        ProfileDto GetAppProfileByName(string name);
        [OperationContract]
        string UpdateAppProfile(ProfileDto profile);
        [OperationContract]
        int DeleteProfiles(List<string> profileIds);
        [OperationContract]
        int DeleteProfile(string profileId);
        [OperationContract]
        AppTokenInfo GetAppTokenInfo(string id);
        [OperationContract]
        ProfileDto GetAvailableDefaultApp(string tenantId);
        [OperationContract]
        CreateAppResult CreateOrUpdateDefaultApp(Office365AppProfileModel model);
        [OperationContract]
        ProfileDto GetAvailableCustomByTenantIdAndApplicationId(string tenantId, string applicationId);
        [OperationContract]
        ProfileDto GetAvailableDefaultByTenantId(string tenantId);
        [OperationContract]
        List<ProfileDto> GetAvailableAppProfilesByTenantId(string tenantId);
        [OperationContract]
        bool NeedUpdateRedirectURL(AppProfileType appType, string appId, string redirectURL);

        [OperationContract]
        bool IsNameExistForUpdate(string name, string excludeId);
        [OperationContract]
        bool IsNameExist(string name);
        [OperationContract]
        GetTenantIdResult GetTenantId(string username, string agentGroupId = null);
        [OperationContract]
        ResultEunmMessage ValidateOnlineAppProfile(ProfileDto profile, string agentGroupId = null);
        [OperationContract]
        bool ValidateForDelete(List<string> profileIds);
        
    }
}
