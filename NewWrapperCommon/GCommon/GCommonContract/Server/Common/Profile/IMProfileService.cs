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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Profile
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMProfileService
    {
        [OperationContract]
        void Add(ProfileDto profile);
        [OperationContract]
        void AddBatch(IEnumerable<ProfileDto> profiles);
        [OperationContract]
        void Delete(string id);
        [OperationContract]
        void DeleteBatch(IEnumerable<string> ids);
        [OperationContract]
        void Update(ProfileDto profile);
        [OperationContract]
        ProfileDto Load(string id);
        [OperationContract]
        List<ProfileDto> GetByIdArray(IEnumerable<string> idArr);
        [OperationContract]
        List<ProfileDto> GetAll();
        [OperationContract]
        List<ProfileDto> GetByType(ProfileType type);
        [OperationContract]
        List<ProfileDto> GetByAgentGroupAndType(string agentGroupId, ProfileType type);
        [OperationContract]
        List<ProfileDto> GetByFarmAndType(string farmId, ProfileType type);
        [OperationContract]
        List<ProfileDto> GetByParentId(String parentId);
        [OperationContract]
        ProfileDto GetByProfileName(String profileName, ProfileType type);
        [OperationContract]
        bool IsNameExistByParentId(String parentId, String name);
        [OperationContract]
        bool IsNameExist(int type, string name);
        [OperationContract]
        bool IsNameExist(int type, string name, string excludeId);
    }
}

