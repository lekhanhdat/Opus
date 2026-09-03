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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOSearchServiceApplication : IAveIisWebServiceApplication
    {
        bool AlertsEnabled { get; set; }
        bool QueryLoggingEnabled { get; set; }
        IAveOLocationConfigurationCollection LocationConfigurations { get; }
        IAveOFASTAdminProxy FASTAdminProxy { get; }
        object LockObject { get; }
        AveSearchServiceApplicationType SearchApplicationType { get; set; }
        AveSearchProvider DefaultSearchProvider { get; set; }
        bool RestoreIsRunning { get; }
        bool BackupIsRunning { get; }
        IAveOSearchServiceApplicationMonitoring Monitoring { get; }

        void AddNewLocationConfiguration(IAveOLocationConfiguration configuration);
        IAveOSearchServiceApplication GetApplicationByName(string applicationName);
        IAveOSearchServiceApplication GetApplicationByName(string applicationName, bool cached);
        IAveOSearchProxyInfo GetProxyInfo();
        int IsPaused();
        IAveOScopesManagerInfo GetScopesManagerInfo();

        #region add for SP2013
        string SearchCenterUrl { get; set; }
        IAveOSearchTopology ActiveTopology { get; }
        List<IAveOQueryReportData> GetSearchReport(int reportType, Guid tenantId, Guid siteId, DateTime reportDate, bool bDaily, uint maxRows);
        IEnumerable<IAveOSearchAnalyticsReportingDatabase> AnalyticsReportingDatabases { get; }
        #endregion
    }
}
