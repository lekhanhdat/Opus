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





namespace AvePoint.GCommon.Contract.Server.ControlPanel.SystemProfile
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMSystemProfileService
    {
        [OperationContract]
        string CreateSystemProfile(SystemProfileDto dto);
        [OperationContract]
        string UpdateSystemProfile(SystemProfileDto dto);
        [OperationContract]
        List<SystemProfileDto> GetAllSystemProfile();
        [OperationContract]
        string DeleteSystemProfile(string id);
        [OperationContract]
        string DeleteSystemProfiles(List<string> ids);
        [OperationContract]
        List<SystemProfileDto> GetAllSystemProfileByStatus();
        [OperationContract]
        List<string> TestSystemProfile(SystemProfileDto dto);
        [OperationContract]
        SystemProfileDto GetSystemProfileById(string id);
        [OperationContract]
        List<OntapItemInfo> LoadLuns(ServiceDto media);
        [OperationContract]
        List<OntapItemInfo> LoadCIFSShare(SystemProfileDto profile, string binPath);
    }
}
