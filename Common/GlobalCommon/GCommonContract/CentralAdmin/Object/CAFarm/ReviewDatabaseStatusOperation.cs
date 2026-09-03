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

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReviewDatabaseStatusOperation : CAOperation
    {
        /// <summary>
        /// Search Server List
        /// </summary>
        [DataMember]
        public List<NameAndIdDto> SearchServerList { get; set; }
        /// <summary>
        /// Server List for Timer Job
        /// </summary>
        [DataMember]
        public List<NameAndIdDto> TimerServerList { get; set; }
        /// <summary>
        /// Database List
        /// </summary>
        [DataMember]
        public List<DatabaseObject> DatabaseList { get; set; }
        /// <summary>
        /// Remove Current Database if its value is true.
        /// </summary>
        [DataMember]
        public bool RemoveDB { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DatabaseObject : IComparable<DatabaseObject>
    {
        /// <summary>
        /// Database ID
        /// </summary>
        [DataMember]
        public string DBId { get; set; }
        /// <summary>
        /// Database Server 
        /// </summary>
        [DataMember]
        public string DBServer { get; set; }
        /// <summary>
        /// Database Name
        /// </summary>
        [DataMember]
        public string DBName { get; set; }
        /// <summary>
        /// Database Type
        /// </summary>
        [DataMember]
        public string TypeName { get; set; }
        /// <summary>
        /// Database Upgrade Status. This is for Main Page
        /// </summary>
        [DataMember]
        public string DBUpgradeStatus { get; set; }    
        /// <summary>
        /// Database Status, Ready or Offline. For Manage Database Settings 
        /// </summary>
        [DataMember]
        public StatusObject DBStatus { get; set; }     
        /// <summary>
        /// Database Read-Only
        /// </summary>
        [DataMember]
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// Database Authentication, Windows Authentication or Sql Authentication 
        /// </summary>
        [DataMember]
        public bool IsWinAuth { get; set; } 
        /// <summary>
        /// if use sql authentication
        /// </summary>
        [DataMember]
        public string UserName { get; set; }
        /// <summary>
        /// if use sql authentication
        /// </summary>
        [DataMember]
        public string Password { get; set; }
        /// <summary>
        /// Database Schema Versions
        /// </summary>
        [DataMember]
        public List<string> DBSchemaVersions { get; set; }
        //下面是改进的DBSchemaVersions结构
        [DataMember]
        public List<DBSchemaVersion> DBSchemaVersionsInfo { get; set; }
        /// <summary>
        /// Failover Database Server
        /// </summary>
        [DataMember]
        public string FailoverDBServer { get; set; }
        /// <summary>
        ///  Current number of sites 
        /// </summary>
        [DataMember]
        public int CurrentSiteCount { get; set; }
        /// <summary>
        ///  Number of sites before a warning event is generated  
        /// </summary>
        [DataMember]
        public int WaringSiteCount { get; set; }
        /// <summary>
        /// Maximum number of sites that can be created in this database
        /// </summary>
        [DataMember]
        public int MaximumSiteCount { get; set; }
        /// <summary>
        /// Search server's id of the database.
        /// </summary>
        [DataMember]
        public string SearchServerId { get; set; }
        /// <summary>
        /// Prefered server's id for timer job.
        /// </summary>
        [DataMember]
        public string TimerServerId { get; set; }
        /// <summary>
        /// If exists site collection in the database.
        /// </summary>
        [DataMember]
        public bool HasSiteCollection { get; set; }

        #region IComparable<DatabaseObject> Members

        public int CompareTo(DatabaseObject other)
        {           
            if (other == null) return 1;           
            if (this.TypeName.Equals("Content Database"))
            {
                if (other.TypeName.Equals("Content Database"))
                {
                    return string.Compare(this.DBName, other.DBName, StringComparison.Ordinal);
                }
                return -1;
            }
            else
            {
                if (other.TypeName.Equals("Content Database"))
                {
                    return 1;
                }
                return string.Compare(this.DBName, other.DBName, StringComparison.Ordinal);
            }
        }

        #endregion
    }

    /// <summary>
    /// Database Status, this enum parameter can be changed to store more status of database.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StatusObject 
    {
        [EnumMember]
        Offline,
        [EnumMember]
        Ready
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NameAndIdDto 
    {
        [DataMember]
        public string Id { get; set;}
        [DataMember]
        public string Name { get; set;}

        [DataMember]
        public NameAndIdExtendDto NameAndIdExtendDto { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NameAndIdExtendDto
    {
        //标记user mapping是否选择了 PlaceHolder
        [DataMember]
        public Boolean UsePlaceHolder { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DBSchemaVersion 
    {
        [DataMember]
        public string DatabaseSequenceName { get; set; }
        [DataMember]
        public string CurrentSchemaVersion { get; set; }
        [DataMember]
        public string MaximumSchemaVersion { get; set; }
    }
}
