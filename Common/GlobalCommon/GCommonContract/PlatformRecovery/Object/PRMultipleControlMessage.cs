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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VerifyJobInfo
    {
        private int mVerifyQuantity;
        private bool mIsVerifyMirror;

        [DataMember]
        public bool IsVerifyMirror
        {
            get { return mIsVerifyMirror; }
            set { mIsVerifyMirror = value; }
        }
        [DataMember]
        public int VerifyQuantity
        {
            get { return mVerifyQuantity; }
            set { mVerifyQuantity = value; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VerifyInfo
    {
        public string JobId;
        public VerifyJobInfo VerificationJobInfo;
        public VerificationInfo VerificationServerInfo;
        public BackupPlanInfo BackupPlanInfo;
        public PRTreeNodeDto RootBackupNode;
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupPlanInfo
    {
        public string JobId;
        public string PlanId;
        public int PlanType;
        public string PlanName;
        public BackupLevels BackupLevel;
        public int ServerSideDataMode;
        public string DataVersion;
        public int copyOnly;   //by kyo  1--selected 0--not selected
        public int VerifyBackup; //1--selected，0--not selected
        public string RetentionOption; // <RetentionOption  timeLevel="0" number="1" />
        public int indexLevel; //1001--default 1002--site 1003--web
        //timeLevel = [0,1,2]
        //<option value="0">  Daily
        //<option value="1">  Weekly
        //<option value="2">  Monthly

        //number = [>0]
        public bool IsUpdateMirror;
        public bool IsUpdateSnapVault;
        public bool IsVerifyMirror;
        public bool IsVerifyJob;
        public bool IsUpdateMirrorForDevice;
        public string logicDriveId;
        public string MediaName; //Add for NET-3081
        //add for snapvault by long
        public bool IsVerifyArchiveBackup;
        public bool LogAfterFull = true;
        public string ArchivedBackupRetention { get; set; }
       // public string ArchivedBackupRetention
       // {
       //     get { return _archivedBackupRetention; }
       //     set
       //     {
       //         if (!string.IsNullOrEmpty(value) && SnapVaultManaGroup.Contains(value.Trim().ToLower()))
       //         {
       //             _archivedBackupRetention = SnapVaultManaGroup[value.Trim().ToLower()].ToString();
       //         }
       //         else
       //         {
       //             _archivedBackupRetention = SnapVaultManaGroup["daily"].ToString();
       //         }
       //     }
       // }

        public BackupPlanInfo()
        { }
       //// public static Hashtable SnapVaultManaGroup = null;
       // public static Hashtable SnapVaultManaGroup = null;

       // public Hashtable mmm;
        //public static BackupPlanInfo()
        //{
        //    SnapVaultManaGroup = new Hashtable();
        //    SnapVaultManaGroup.Add("daily", "Daily");
        //    SnapVaultManaGroup.Add("hourly", "Hourly");
        //    SnapVaultManaGroup.Add("weekly", "Weekly");
        //    SnapVaultManaGroup.Add("monthly", "Monthly");
        //    SnapVaultManaGroup.Add("unlimited", "Unlimited");
        //    SnapVaultManaGroup.Add("all", "All");//only for verify-backup
        //}

    }

    public enum BackupLevels
    {
        Full = 0,
        Incremental = 1,
        Differential
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DatabaseItem
    {
        [DataMember]
        public long StartTime { get; set; }
        [DataMember]
        public long EndTime { get; set; }
        [DataMember]
        public bool IsAlreadyBackup{ get; set; }
        [DataMember]
        public bool BackupSucceed{ get; set; }
        [DataMember]
        public bool VerifySucceed{ get; set; }
        [DataMember]
        public string InstanceName{ get; set; }
        [DataMember]
        public string ServerInstanceAliasName{ get; set; }
        [DataMember]
        public string DatabaseName{ get; set; }
        [DataMember]
        public PRTreeNodeDto BackupNode{ get; set; }
        [DataMember]
        public string VsUrl{ get; set; }
        [DataMember]
        public string BackupErrMsg{ get; set; }//if this message is not empty, indicate this db backup failed.can not restore
        [DataMember]
        public string BackupLogMsg{ get; set; } //this db backup succeed & can restore, but there is some error or exception
        [DataMember]
        public string BackupNodeXmlAfterBackup{ get; set; }
        [DataMember]
        public string BackupNodeXmlAfterRestore{ get; set; }
        [DataMember]
        public string CloneErrMsg{ get; set; }
        [DataMember]
        public string UpdateSnapVaultErrMsg{ get; set; }
        [DataMember]
        public string UpdateSnapMirrorErrMsg{ get; set; }
        [DataMember]
        public long DatabaseSize{ get; set; }

        [DataMember]
        public Guid ParentAppId{ get; set; }
        [DataMember]
        public string RestoreErrMsg{ get; set; }//if this message is not empty, indicate this db restore failed.
        [DataMember]
        public string RestoreLogMsg{ get; set; } //this db restore succeed , but there is some error or exception
        [DataMember]
        public string VerifyErrMsg{ get; set; }  // for verify job
        [DataMember]
        public string SnapShotName{ get; set; }
        [DataMember]
        public string OriginalLocation{ get; set; }
        [DataMember]
        public string TempDBName{ get; set; }//for manually restore tempdbName
        [DataMember]
        public bool IsIndexSuccessful;
        [DataMember]
        public string JobId { get; set; }
        public DatabaseItem(string instanceName, PRTreeNodeDto backupNode, string jobId)
        {
            this.InstanceName = instanceName;
            this.DatabaseName = backupNode.Name;
            this.BackupNode = backupNode;
            this.ParentAppId = backupNode.Parent.SPObjectId;
            RestoreErrMsg = "";
            RestoreLogMsg = "";
            ServerInstanceAliasName = backupNode.Server;
            IsAlreadyBackup = false;
            BackupSucceed = false;
            BackupErrMsg = "";
            BackupLogMsg = "";
            BackupNodeXmlAfterBackup = "";
            VerifyErrMsg = "";
            CloneErrMsg = "";
            DatabaseSize = 0;
            JobId = jobId;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DatabaseGroup
    {
        [DataMember]
        public bool IsVerify;
        [DataMember]
        public bool IsOffLine;
        [DataMember]
        public List<DatabaseItem> DatabaseItems = new List<DatabaseItem>();
        [DataMember]
        public string InstanceName { get; set; }
        [DataMember]
        public string AliasName{ get; set; }
        [DataMember]
        public string AgentName{ get; set; }
        public string UserName
        {
            get
            {
                if (DatabaseItems.Count != 0)
                {
                    return DatabaseItems[0].BackupNode.UserName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public string Password
        {
            get 
            {
                if (DatabaseItems.Count != 0)
                {
                    return DatabaseItems[0].BackupNode.Password;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
       
        private int pruningFailedCount = 0;
        [DataMember]
        public int PruningFailedCount
        {
            get { return pruningFailedCount; }
            set { pruningFailedCount = value; }
        }

        public DatabaseGroup() { }

        public DatabaseGroup(string instanceName)
        {
            this.InstanceName = instanceName;
        }
        public DatabaseGroup(string instanceName, string aliasName, string agentName)
        {
            this.InstanceName = instanceName;
            this.AliasName = aliasName;
            this.AgentName = agentName;
        }
        public void AddDatabaseItem(DatabaseItem dbItem)
        {
            if (string.IsNullOrEmpty(InstanceName))
            {
                InstanceName = dbItem.InstanceName;
            }
            DatabaseItems.Add(dbItem);
        }
        public bool IsContainDatabase(string instanceName, string databaseName, string jobId)
        {
            foreach (DatabaseItem item in DatabaseItems)
            {
                if (item.InstanceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase) 
                    && item.DatabaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase)
                    && item.JobId.Equals(jobId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        public bool IsContainDatabase(string instanceName, string databaseName)
        {
            foreach (DatabaseItem item in DatabaseItems)
            {
                if (item.InstanceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase)
                    && item.DatabaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        public DatabaseItem GetDatabaseItem(string instanceName, string databaseName, string jobId, bool throwException = true)
        {
            foreach (DatabaseItem item in DatabaseItems)
            {
                if (item.InstanceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase) 
                    && item.DatabaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase)
                    && item.JobId.Equals(jobId, StringComparison.OrdinalIgnoreCase))
                    return item;
            }
            if (throwException)
            {
                throw new Exception(string.Format("Database group of instance [{0}] does not contain database[{1}]", InstanceName, databaseName));
            }
            else
            {
                return null;
            }
        }

        public void RemoveDatabaseItem(string instanceName, string databaseName, string jobId)
        {
            if (IsContainDatabase(instanceName, databaseName, jobId))
            {
                DatabaseItem item = GetDatabaseItem(instanceName, databaseName, jobId);
                DatabaseItems.Remove(item);
            }
            else
            {
                throw new Exception(string.Format("Database group of instance [{0}] does not contain database[{1}], so we cannot remove it", InstanceName, databaseName));
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DatabaseGroups
    {        
        private List<DatabaseGroup> mGroups = new List<DatabaseGroup>();

        public DatabaseGroups() { }

        [DataMember]
        //NetApp 2.0
        public List<DatabaseGroup> Groups
        {
            get { return mGroups; }
            set { mGroups = value; }
        }
        //NetApp 2.0

        public void AddDatabaseGroup(DatabaseGroup groupIn)
        {
            mGroups.Add(groupIn);
        }

        public bool IsContainDatabaseGroup(string instanceName)
        {
            foreach (DatabaseGroup group in mGroups)
            {
                if (group.InstanceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public DatabaseGroup GetDatabaseGroup(string instanceName)
        {
            foreach (DatabaseGroup group in mGroups)
            {
                if (group.InstanceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase))
                {
                    return group;
                }
            }
            return null;
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSNDatabaseGroup
    {
        [DataMember]
        public string RealInstanceName{get;set;}
        [DataMember]
        public DatabaseGroup DbGroup = new DatabaseGroup();
        [DataMember]
        public BackupManagementGroupType ManagementGroup { get; set; }
        [DataMember]
        public int VerifyQuantity { get; set; }
        [DataMember]
        public List<string> JobIds { get; set; }
        [DataMember]
        public bool IsCluster { get; set; }
        public PRSNDatabaseGroup(string sqlInstance, BackupManagementGroupType managementGroup, bool isCluster)
        {
            this.RealInstanceName = sqlInstance;
            this.ManagementGroup = managementGroup;
            DbGroup = new DatabaseGroup(sqlInstance);
            this.VerifyQuantity = 1;
            this.IsCluster = isCluster;
            this.JobIds = new List<string>();
        }
        public PRSNDatabaseGroup() { }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MultipleDB
    {
        [DataMember]
        public string DbInstance { get; set; } //DbHostName  or  DbHostName\\DbInstanceName
        [DataMember]
        public string DbHostName { get; set; } 
        [DataMember]
        public string DbInstanceName { get; set; } 
        [DataMember]
        public string DbName { get; set; } 
        [DataMember]
        public string UserName { get; set; } 
        [DataMember]
        public string Passowrd { get; set; } 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VerificationInfo
    {
        private bool mIsVerify;
        private bool mIsVerifyMirror;
        private string mServerName;
        private string mUserName;
        private string mPassword;
        private string mAuthentication;
        private string mMountPoint;

        [DataMember]
        public bool IsVerify
        {
            get { return mIsVerify; }
            set { mIsVerify = value; }
        }
        [DataMember]
        public bool IsVerifyMirror
        {
            get { return mIsVerifyMirror; }
            set { mIsVerifyMirror = value; }
        }
        [DataMember]
        public string ServerName
        {
            get { return mServerName; }
            set { mServerName = value; }
        }
        [DataMember]
        public string UserName
        {
            get { return mUserName; }
            set { mUserName = value; }
        }
        [DataMember]
        public string Password
        {
            get { return mPassword; }
            set { mPassword = value; }
        }
        [DataMember]
        public string Authentication
        {
            get { return mAuthentication; }
            set { mAuthentication = value; }
        }
        [DataMember]
        public string MountPoint
        {
            get { return mMountPoint; }
            set { mMountPoint = value; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRMultipleControlMessage : PRMessage
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public ServiceDto ControlAgent { get; set; }

        [DataMember]
        public ServiceDto MediaInfo { get; set; }

        [DataMember]
        public PRBackupPlanDto Plan { get; set; }

        [DataMember]
        public MultipleDB MultipleDBInfo { get; set; }

        [DataMember]
        public PlatformBackupRequest ConfigForMedia { get; set; }

        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }

        [DataMember]
        public PRTreeNodeDto PRTreeNode { get; set; }

        [DataMember]
        public IList<ServiceDto> AgentList { get; set; }

        [DataMember]
        public PRBackupJobDto Job { get; set; }
    }
}
