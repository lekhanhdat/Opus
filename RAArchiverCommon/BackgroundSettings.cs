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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Common;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using RecordsHotfixMaintenanceService;

namespace RAArchiverCommon
{
    public class BackgroundSettings
    {
        private RALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object padlock = new object();
        private static BackgroundSettings instance;

        private const string TEMPFOLDERNAME = "Archiver";
        private const string CACHEFOLDERNAME = "ArchiverCache";
        #region MultiThread
        private readonly int DefaultTotalMultiBackupThreadNumber = 4;
        private readonly int DefaultTotalMultiDeleteThreadNumber = 4;
        private readonly int DefaultTotalTransferQueueNumber = 20;
        private readonly int DefaultItemDependencyOption = 3;

        private int mTotalMultiBackupThreadNumber = 0;

        public int TotalMultiBackupThreadNumber
        {
            get
            {
                if (mTotalMultiBackupThreadNumber == 0)
                {
                    try
                    {
                        var backupThreadNumber = SettingProfilesDao.LoadByType(SettingProfilesType.TotalMultiBackupThreadNumber);
                        mTotalMultiBackupThreadNumber = backupThreadNumber == null ? DefaultTotalMultiBackupThreadNumber : Convert.ToInt32(backupThreadNumber.Settings);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"An error occurred while getting BackupThreadNumber. error is {e.ToString()}");
                        mTotalMultiBackupThreadNumber = DefaultTotalMultiBackupThreadNumber;
                    }
                }
                mLog.Info($"[BackgroundSettings.TotalMultiBackupThreadNumber] : {mTotalMultiBackupThreadNumber}");
                return mTotalMultiBackupThreadNumber;
            }
        }

        private int mTotalMultiDeleteThreadNumber = 0;

        public int TotalMultiDeleteThreadNumber
        {
            get
            {
                if (mTotalMultiDeleteThreadNumber == 0)
                {
                    try
                    {
                        var deleteThreadNumber = SettingProfilesDao.LoadByType(SettingProfilesType.TotalMultiDeleteThreadNumber);
                        mTotalMultiDeleteThreadNumber = deleteThreadNumber == null ? DefaultTotalMultiDeleteThreadNumber : Convert.ToInt32(deleteThreadNumber.Settings);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"An error occurred while getting DeleteThreadNumber. error is {e.ToString()}");
                        mTotalMultiDeleteThreadNumber = DefaultTotalMultiDeleteThreadNumber;
                    }
                }
                mLog.Info($"[BackgroundSettings.TotalMultiDeleteThreadNumber] : {mTotalMultiDeleteThreadNumber}");
                return mTotalMultiDeleteThreadNumber;
            }
        }

        private int mTotalTransferQueueNumber = 0;

        public int TotalTransferQueueNumber
        {
            get
            {
                if (mTotalTransferQueueNumber == 0)
                {
                    try
                    {
                        var transferQueueNumber = SettingProfilesDao.LoadByType(SettingProfilesType.TotalTransferQueueNumber);
                        mTotalTransferQueueNumber = transferQueueNumber == null ? DefaultTotalTransferQueueNumber : Convert.ToInt32(transferQueueNumber.Settings);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"An error occurred while getting TransferQueueNumber. error is {e.ToString()}");
                        mTotalTransferQueueNumber = DefaultTotalTransferQueueNumber;
                    }
                }
                mLog.Info($"[BackgroundSettings.TotalTransferQueueNumber] : {mTotalTransferQueueNumber}");
                return mTotalTransferQueueNumber;
            }
        }

        public bool SOSkipDeletionForTest
        {
            get
            {
                try
                {
                    var mSOSkipDeletionForTest = SettingProfilesDao.LoadByType(SettingProfilesType.SOSkipDeletionForTest);
                    if (mSOSkipDeletionForTest != null)
                    {
                        mLog.Info($"[BackgroundSettings.SOSkipDeletionForTest] : {mSOSkipDeletionForTest}");
                        return Convert.ToBoolean(mSOSkipDeletionForTest.Settings);
                    }
                    else
                    {
                        mLog.Info($"[BackgroundSettings.SOSkipDeletionForTest] : {false}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn($"An error occurred while getting TransferQueueNumber. error is {e.ToString()}");
                    return false;
                }
            }
        }

        public int ItemDependencyOption
        {
            get
            {
                try
                {
                    var mItemDependencyOption = SettingProfilesDao.LoadByType(SettingProfilesType.ItemDependencyOption);

                    if (mItemDependencyOption != null)
                    {
                        mLog.Info($"[BackgroundSettings.ItemDependencyOption] : {mItemDependencyOption.Settings}");
                        return Convert.ToInt32(mItemDependencyOption.Settings);
                    }
                    else
                    {
                        mLog.Info($"[BackgroundSettings.ItemDependencyOption] : {DefaultItemDependencyOption}");
                        return DefaultItemDependencyOption;
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn($"An error occurred while getting ItemDependencyOption. error is {e.ToString()}");
                    return DefaultItemDependencyOption;
                }
            }
        }

        public bool EnableMultiDelete = true;
        public bool EnableMultiBackup = true;
        #endregion

        #region backup OutputStreamLevel FileLevel = 0,DataBlockLevel = 4096,
        private OutputStreamLevel mArchiverOutputStreamLevel = OutputStreamLevel.None;

        public OutputStreamLevel ArchiverOutputStreamLevel
        {
            get
            {
                if (mArchiverOutputStreamLevel == OutputStreamLevel.None)
                {
                    //Archiver 默认用DataBlockLevel
                    mArchiverOutputStreamLevel = OutputStreamLevel.DataBlockLevel;
                    try
                    {
                        if (int.TryParse(KeyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                        {
                            switch (outputStreamLevel)
                            {
                                case 0:
                                    mArchiverOutputStreamLevel = OutputStreamLevel.FileLevel;
                                    break;
                                case 4096:
                                default:
                                    mArchiverOutputStreamLevel = OutputStreamLevel.DataBlockLevel;
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"An error occurred while getting ArchiverOutputStreamLevel. error is {e.ToString()}");
                    }
                }
                mLog.Info($"BackgroundSettings.ArchiverOutputStreamLevel : {mArchiverOutputStreamLevel}");
                return mArchiverOutputStreamLevel;
            }
        }

        private OutputStreamLevel mRecordsOutputStreamLevel = OutputStreamLevel.None;

        public OutputStreamLevel RecordsOutputStreamLevel
        {
            get
            {
                if (mRecordsOutputStreamLevel == OutputStreamLevel.None)
                {
                    //records 默认用FileLevel
                    mRecordsOutputStreamLevel = OutputStreamLevel.FileLevel;
                    try
                    {
                        if (int.TryParse(KeyValueDao.GetValueByKey(RMKeyValuesConstants.RecordsBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                        {
                            switch (outputStreamLevel)
                            {
                                case 4096:
                                    mRecordsOutputStreamLevel = OutputStreamLevel.DataBlockLevel;
                                    break;
                                case 0:
                                default:
                                    mRecordsOutputStreamLevel = OutputStreamLevel.FileLevel;
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"An error occurred while getting RecordsOutputStreamLevel. error is {e.ToString()}");
                    }
                }
                mLog.Info($"BackgroundSettings.RecordsOutputStreamLevel : {mRecordsOutputStreamLevel}");
                return mRecordsOutputStreamLevel;
            }
        }
        private OutputStreamLevel mGoogleOutputStreamLevel = OutputStreamLevel.None;
        public OutputStreamLevel GoogleOutputStreamLevel
        {
            get
            {
                if (mGoogleOutputStreamLevel == OutputStreamLevel.None)
                {
                    //google 默认用DataBlockLevel
                    mGoogleOutputStreamLevel = OutputStreamLevel.DataBlockLevel;
                    try
                    {
                        if (int.TryParse(KeyValueDao.GetValueByKey(RMKeyValuesConstants.RecordsBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                        {
                            switch (outputStreamLevel)
                            {
                                case 4096:
                                    mGoogleOutputStreamLevel = OutputStreamLevel.DataBlockLevel;
                                    break;
                                case 0:
                                default:
                                    mGoogleOutputStreamLevel = OutputStreamLevel.FileLevel;
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"An error occurred while getting RecordsOutputStreamLevel. error is {e.ToString()}");
                    }
                }
                mLog.Info($"BackgroundSettings.RecordsOutputStreamLevel : {mGoogleOutputStreamLevel}");
                return mGoogleOutputStreamLevel;
            }
        }
        #endregion

        public bool IsOutputVerboseLog = true;
        public bool IsDeleteRecord = false;
        public readonly int MaxDeletionCacheSize = 100000;
        public string ArchiveTemp { get; private set; }
        public string ArchiveCache { get; private set; }
        public List<int> ListTemplateTable = new List<int>() { 100, 101, 103, 104, 106, 107, 108, 115, 119, 433, 700, 851, 1302 };
        public List<string> SkipExtentionName = new List<string>() { ".aspx", ".js", ".css", ".md", ".copilot" };
        public List<string> RADisplayColumns= new List<string>() { "Content Type", "Created", "Author", "Modified", "Editor", SPColumnConstants.DocumentId, RcordsBuiltInColumn.UNIQUEID_NAME };//add for RevIM report
        public bool UseHighSpeedCreateStub = true;
        public string VEOType = "VEO";
        public string VEOV3Type = "VEOV3";
        public int ManifestXmlSize = 1;

        private ISettingProfilesDao mSettingProfilesDao;
        protected ISettingProfilesDao SettingProfilesDao
        {
            get
            {
                if (mSettingProfilesDao == null)
                {
                    mSettingProfilesDao = (ISettingProfilesDao)PlatformWindsorManager.GetService(typeof(ISettingProfilesDao));
                }
                return mSettingProfilesDao;
            }
        }

        protected IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private ITenantService mTenantService;
        protected ITenantService TenantService
        {
            get 
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
            }
        }

        private BackgroundSettings()
        {
            Init();
        }
        public static BackgroundSettings GetInstance()
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new BackgroundSettings();
                    }
                }
            }
            return instance;
        }

        private void Init()
        {
            ArchiveTemp = Path.Combine(RecordsEnv.AppDomainRootFolder, TEMPFOLDERNAME);
            ArchiveCache = Path.Combine(RecordsEnv.AppDomainRootFolder, CACHEFOLDERNAME);
            if (TenantService.IsCSDTenant() && DataCenterUtil.Is21V())
            {
                SkipExtentionName = new List<string>();
                mLog.Info("Do not skip js, css, aspx for CSD Tenant.");
            }
        }
    }
}
