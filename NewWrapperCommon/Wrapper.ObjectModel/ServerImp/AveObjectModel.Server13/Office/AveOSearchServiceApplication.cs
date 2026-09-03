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



using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using System;
using Microsoft.Office.Server.Search.Query;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOSearchServiceApplication : AveIisWebServiceApplication, IAveOSearchServiceApplication
    {
        private SearchServiceApplication mSearchServiceApplication;
        private AveOFASTAdminProxy mFASTAdminProxy;
        private AveOSearchServiceApplicationMonitoring mMonitoring;
        public AveOSearchServiceApplication(SearchServiceApplication searchServiceApplication)
            : base(searchServiceApplication)
        {
            mSearchServiceApplication = searchServiceApplication;
        }

        public AveOSearchServiceApplication()
            : this(new SearchServiceApplication())
        { }

        internal SearchServiceApplication SearchServiceApplication
        {
            get
            {
                return mSearchServiceApplication;
            }
        }

        #region IAveOSearchServiceApplication Members

        public bool AlertsEnabled
        {
            get
            {
                return mSearchServiceApplication.AlertsEnabled;
            }
            set
            {
                mSearchServiceApplication.AlertsEnabled = value;
            }
        }

        public override void Update()
        {
            mSearchServiceApplication.Update();
        }

        //public bool QueryLoggingEnabled
        //{
        //    get
        //    {
        //        return mSearchServiceApplication.QueryLoggingEnabled;
        //    }
        //    set
        //    {
        //        mSearchServiceApplication.QueryLoggingEnabled = value;
        //    }
        //}

        public void AddNewLocationConfiguration(IAveOLocationConfiguration configuration)
        {
            mSearchServiceApplication.AddNewLocationConfiguration((configuration as AveOLocationConfiguration).LocationConfiguration);
        }

        public IAveOSearchServiceApplication GetApplicationByName(string applicationName)
        {
            return GetApplicationByName(applicationName, false);
        }

        public IAveOSearchServiceApplication GetApplicationByName(string applicationName, bool cached)
        {
            object searchServiceApplication = AveAssemblyUtility.InvokeStaticMethod(typeof(SearchServiceApplication), "GetApplicationByName", new Type[] { typeof(string), typeof(bool) }, new object[] { applicationName, cached });
            if (searchServiceApplication != null)
            {
                return new AveOSearchServiceApplication(searchServiceApplication as SearchServiceApplication);
            }
            return null;
        }

        public IAveOLocationConfigurationCollection LocationConfigurations
        {
            get
            {
                LocationConfigurationCollection locationConfigurations = mSearchServiceApplication.LocationConfigurations;
                if (locationConfigurations == null)
                {
                    return null;
                }
                return new AveOLocationConfigurationCollection(locationConfigurations);
            }
        }

        //public AveSearchServiceApplicationType SearchApplicationType
        //{
        //    get
        //    {
        //        return (AveSearchServiceApplicationType)mSearchServiceApplication.SearchApplicationType;
        //    }
        //    set
        //    {
        //        AveAssemblyUtility.SetPropertyValue(mSearchServiceApplication, "SearchApplicationType", (SearchServiceApplicationType)value);
        //    }
        //}

        public AveSearchProvider DefaultSearchProvider
        {
            get
            {
                return (AveSearchProvider)mSearchServiceApplication.DefaultSearchProvider;
            }
            set
            {
                mSearchServiceApplication.DefaultSearchProvider = (SearchProvider)value;
            }
        }

        public IAveOFASTAdminProxy FASTAdminProxy
        {
            get 
            {
                if (mFASTAdminProxy == null)
                {
                    FASTAdminProxy fASTAdminProxy = mSearchServiceApplication.FASTAdminProxy;
                    if (fASTAdminProxy != null)
                    {
                        mFASTAdminProxy = new AveOFASTAdminProxy(fASTAdminProxy);
                    }
                }
                return mFASTAdminProxy;
            }
        }

        #endregion


        public bool QueryLoggingEnabled
        {
            //add by adrian
            get
            {
                return mSearchServiceApplication.QueryLogSettings.QLogEnabled;
            }
            set
            {
                mSearchServiceApplication.QueryLogSettings.QLogEnabled = value;
            }
        }

        public AveSearchServiceApplicationType SearchApplicationType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public object LockObject
        {
            //to be found add by adrian
            get { return AveAssemblyUtility.GetPropertyValue(mSearchServiceApplication, "LockObject"); }
        }

        public bool RestoreIsRunning
        {
            //to be found add by adrian
            get
            {
                return Convert.ToBoolean(AveAssemblyUtility.GetPropertyValue(mServiceApplication, "RestoreIsRunning"));
            }
        }

        public bool BackupIsRunning
        {
            //to be found add by adrian
            get
            {
                return Convert.ToBoolean(AveAssemblyUtility.GetPropertyValue(mServiceApplication, "BackupIsRunning"));
            }
        }

        public IAveOSearchProxyInfo GetProxyInfo()
        {
            SearchProxyInfo searchProxyInfo = mSearchServiceApplication.GetProxyInfo();
            if (searchProxyInfo != null)
            {
                return new AveOSearchProxyInfo(mSearchServiceApplication.GetProxyInfo());
            }
            return null;
        }

        public int IsPaused()
        {
            //to be found add by adrian
            return mSearchServiceApplication.IsPaused();
        }

        public IAveOScopesManagerInfo GetScopesManagerInfo()
        {
            //to be found add by adrian
            ScopesManagerInfo scopesManagerInfo = mSearchServiceApplication.GetScopesManagerInfo();
            return new AveOScopesManagerInfo(scopesManagerInfo);
        }


        public IAveOSearchServiceApplicationMonitoring Monitoring
        {
            get
            {
                if (mMonitoring == null)
                {
                    object searchServiceApplicationMonitoring = AveAssemblyUtility.GetPropertyValue(mSearchServiceApplication, "Monitoring");
                    if (searchServiceApplicationMonitoring != null)
                    {
                        mMonitoring = new AveOSearchServiceApplicationMonitoring(searchServiceApplicationMonitoring);
                    }
                }
                return mMonitoring;
            }
        }

        #region add for SP2013
        public string SearchCenterUrl
        {
            get
            {
                return mSearchServiceApplication.SearchCenterUrl;
            }
            set
            {
                mSearchServiceApplication.SearchCenterUrl = value;
            }
        }

        public IAveOSearchTopology ActiveTopology
        {
            get
            {
                return new AveOSearchTopology(mSearchServiceApplication.ActiveTopology);
            }
        }

        public List<IAveOQueryReportData> GetSearchReport(int reportType, Guid tenantId, Guid siteId, DateTime reportDate, bool bDaily, uint maxRows)
        {
            List<IAveOQueryReportData> reports = new List<IAveOQueryReportData>();
            var list = mSearchServiceApplication.GetSearchReport(reportType, tenantId, siteId, reportDate, bDaily, maxRows);
            if (list != null)
            {
                foreach (var reportdata in list)
                {
                    reports.Add(new AveOQueryReportData(reportdata));
                }
            }
            return reports;
        }


        public IEnumerable<IAveOSearchAnalyticsReportingDatabase> AnalyticsReportingDatabases
        {
            get
            {
                return this.AnalyticsReportingDatabaseList;
            }
        }

        private List<AveOSearchAnalyticsReportingDatabase> AnalyticsReportingDatabaseList
        {
            get
            {
                List<AveOSearchAnalyticsReportingDatabase> analyticsReportingDatabases = new List<AveOSearchAnalyticsReportingDatabase>();
                foreach (SearchAnalyticsReportingDatabase analyticsReportingDatabase in mSearchServiceApplication.AnalyticsReportingDatabases)
                {
                    analyticsReportingDatabases.Add(new AveOSearchAnalyticsReportingDatabase(analyticsReportingDatabase));
                }
                return analyticsReportingDatabases;
            }
        }


        #endregion
    }
}
