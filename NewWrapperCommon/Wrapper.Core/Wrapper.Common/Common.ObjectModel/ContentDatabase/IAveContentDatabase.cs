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
using System.Data.SqlClient;
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    public interface IAveContentDatabase : IAveDatabase
    {
        string[,] CorrectTableClauseMap { get; }
        int CurrentSiteCount { get; }
        string Server { get; }
        IAveServiceInstance SearchServiceInstance { get; set; }
        IAveSiteCollection Sites { get; }
        IAveWebApplication WebApplication { get; }
        IAveRemoteBlobStorageSettings RemoteBlobStorageSettings { get; }
        Guid DatabaseId { get; }
        int MaximumSiteCount { get; set; }
        int WarningSiteCount { get; set; }
        IAveTimerServiceInstance PreferredTimerServiceInstance { get; set; }
        bool SupportsRbsShallowCopy { get; }//for sp2010 sp1

        string Repair(bool DeleteCorruption);
        IAveContentDatabase CreateUnattachedContentDatabase(SqlConnectionStringBuilder connection);
        ulong GetConnectorDataSize();
        List<AveUserDetail> GetUserDetailInDatabase(string userSearchInfo, AveAccountSearchFlag flag, string siteId, bool isExact);
        void Move(IAveContentDatabase destinationDb, List<IAveSite> sitesToMove, out Dictionary<IAveSite, string> failedSites);
        void Move(IAveContentDatabase destinationDb, List<IAveSite> sitesToMove, Dictionary<string, string> rbsProviderMap, out Dictionary<IAveSite, string> failedSites);
        void Upgrade(bool recursively);
        void RefreshSitesInConfigurationDatabase();
        Dictionary<Guid, StorageUsageInfo> GetSitesStorageInfo();

        //To operate Change Log
        IAveChangeCollection GetChanges();
        IAveChangeCollection GetChanges(IAveChangeQuery query);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd);
    }

    public class AveUserDetail
    {
        public string DisplayName { get; set; }

        public string LoginName { get; set; }

        public string Email { get; set; }

        public AveAccountType AccountType { get; set; }

        public AveAccountStatus AccountState { get; set; }

        public override int GetHashCode()
        {
            if (!string.IsNullOrEmpty(LoginName))
            {
                return LoginName.ToLower(CultureInfo.CurrentCulture).GetHashCode();
            }
            else
            {
                return 0;
            }
        }
    }

    [Flags]
    public enum AveAccountSearchFlag
    {
        None = 0,

        IncludeADUser = 1,

        IncludeADGroup = 2,

        IncludeSharePointUser = 4,
        IncludeSharePointGroup = 8,

        IncludeFormUser = 16,
        IncludeFormRole = 32,

        IncludeLocalUser = 64,
        IncludeLocalGroup = 128,

        IncludeAllUsers = 256,

        // get the parent of the user or group
        IncludeParentADGroup = 512,

        // include ad disabled users
        IncludeADDisabledUsers = 1024,

        // include form disabled users
        IncludeFormDisabledUsers = 2048,

        // include the special users in the sharepoint: nt authority\\local service;
        //nt authority\\authenticated users
        //sharepoint\\system
        //nt authority\\system
        IncludeSharePointSpecialUsers = 4096
    }

    public enum AveAccountType
    {
        None = 0,
        ADUser = 1,
        ADGroup = 2,
        SharePointUser = 4,
        SharePointGroup = 8,
        FormUser = 16,
        FormRole = 32,
        LocalUser = 64,
        LocalGroup = 128,
        AllUsers = 256
    }

    public enum AveAccountStatus
    {
        // is not verify, the user may be any of the status
        NoVerify = 0,

        Actived = 1,

        // the user is disable
        Deactived = 2,

        // the user is not exit or is  deleted.
        Deleted = 4
    }
}
