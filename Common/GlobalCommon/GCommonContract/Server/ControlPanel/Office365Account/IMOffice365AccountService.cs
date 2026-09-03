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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMOffice365AccountService
    {
        [OperationContract]
        int CreateOffice365Account(Office365AccountInfo profile);
        [OperationContract]
        int EditOffice365Account(Office365AccountInfo profile);
        [OperationContract]
        int DeleteOffice365Account(string profileId);
        [OperationContract]
        int DeleteBatchOffice365Accounts(IEnumerable<string> profileIds);
        [OperationContract]
        List<Office365AccountInfo> GetAllOffice365Accounts();
        [OperationContract]
        Office365AccountInfo GetOffice365AccountById(string id);
        [OperationContract]
        bool IsNameExist(string name);
        [OperationContract]
        bool IsNameExistForUpdate(string name, string excludeId);
        [OperationContract]
        List<ServiceAccount> GetServiceAccounts(string customerId);
        [OperationContract]
        ServiceAccount GetServiceAccountById(string customerId, string serviceAccountId);
        [OperationContract]
        List<AppProfile> GetAppProfiles(string customerId);
    }
}
