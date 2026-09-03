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
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;
using System.IO;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.UserMapping;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.DomainMapping;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common;
using UserAndDomainDto = AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object.UserAndDomainMapping;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping
{
    public interface IMUserMappingService
    {

        UserAndDomainDto GetUserAndDomainMapping(String userMappingId, String domainMappingId);

        #region  user mapping
        MappingValidateResult CreateUserMapping(UserMappingDataContract mapping);
        MappingValidateResult UpdateUserMapping(UserMappingDataContract mapping);
        List<ValidateUsedProfileResult> DeleteUserMappings(List<NameAndIdDto> profiles);
        List<UserMappingDataContract> GetAllUserMappings();
        List<NameAndIdDto> LoadUMNameAndIds();
        UserMappingDataContract GetUserMapping(String id);
        UserMappingDataContract UploadUserMappingToGui(string name, byte[] buffer);
        byte[] DownLoadUserMapping(UserMappingDataContract UserMapping);
        #endregion

        #region domain mapping
        MappingValidateResult CreateDomainMapping(DomainMappingDataContract mapping);
        MappingValidateResult UpdateDomainMapping(DomainMappingDataContract mapping);
        List<ValidateUsedProfileResult> DeleteDomainMappings(List<NameAndIdDto> profiles);
        List<NameAndIdDto> LoadDMNameAndIds();
        List<DomainMappingDataContract> GetAllDoaminMappings();
        DomainMappingDataContract GetDomainMapping(String id);
        DomainMappingDataContract UploadDomainMappingToGui(string name, byte[] buffer);
        byte[] DownLoadDomainMapping(DomainMappingDataContract domainMapping);
        #endregion
    }
}
