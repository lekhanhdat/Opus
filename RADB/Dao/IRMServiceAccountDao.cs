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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMServiceAccountDao
    {
        void CreateServiceAccount(O365ServiceAccountDto account);
        void UpdateServiceAccount(O365ServiceAccountDto account);

        void CreateServiceAccount(List<O365ServiceAccountDto> accounts);
        void UpdateServiceAccount(List<O365ServiceAccountDto> accounts);
        void CreateOrUpdateServiceAccount(List<O365ServiceAccountDto> accounts);
        List<O365ServiceAccountDto> GetServiceAccounts(List<string> ids);
        O365ServiceAccountDto GetServiceAccountByUser(string userName);
        bool CheckServiceAccountExisted(string userName);
        void UpdateServiceAccountPass(string userName, string password);
        void UpdateServiceAccountPasswordById(string id, string password);

        List<string> GetAllUserNames();
        List<O365ServiceAccountDto> GetAllAccounts(bool withoutDecrypt = false);
        bool UpdateUserNameAndPasswordById(string id, string userName, string password);
    }
}
