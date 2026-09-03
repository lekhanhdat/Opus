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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{

    public class EnumObject
    {
        public static List<SourceFlag> GetAllDataSource()
        {
            List<SourceFlag> result = new List<SourceFlag>();
            var source = Enum.GetValues(typeof(SourceFlag));
            foreach (var item in Enum.GetValues(typeof(SourceFlag)))
            {
                result.Add((SourceFlag)item);
            }
            return result;
        }
    }
    /// <summary>
    /// Delete: deleted from source site by end user
    /// Archived: archived by DA
    /// </summary>
    public enum DestoryedAction
    {
        None = -1,        
        Delete = 0,
        Archived = 1
    }

    [DataContract]
    public enum SourceFlag
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        All = 0,
        [EnumMember]
        SharePoint = 1,
        [EnumMember]
        FileSystem = 2,
        [EnumMember]
        Exchange = 3,
        [EnumMember]
        Physical = 4,
        [EnumMember]
        SharePointOnPrem = 5,
        [EnumMember]
        OneDrive = 6,
        [EnumMember]
        AzureFileShare = 7,
        [EnumMember]
        Box = 8,
        [EnumMember]
        Google = 9,
        [EnumMember]
        SalesForce = 10,
        [EnumMember]
        Teams = 11,
        [EnumMember]
        Groups = 12,
        [EnumMember]
        LifecycleRetention = 99,
        [EnumMember]
        Connector = 999,
        [EnumMember]
        GGControl = 9999
    }
    public enum  DataSourceForOrphanBlob
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SharePoint = 1,
        [EnumMember]
        Teams = 2,
        [EnumMember]
        Mailbox = 3,
    }
    public enum FailureSourceType
    {
        None = -1,
        SharePointDataSync = 1,
        FileSystemDataSync = 2,
        ExchangeDataSync = 3,
        Physical = 4,
        SharePointOnPremDataSync = 5,
        OneDriveDataSync = 6,
        SharePointEnforceRetention = 7
    }


    public enum HoldType
    {
        None = 0,
        PersonalHold = 1,
        DisposalHold = 2,
    }

    /// <summary>

    /// 1:Active/Open, 2:Destroyed, 3: delete(RM 删除的文件，理论上不显示),  6:closed, 7: Missing.不使用4， 5 防止与其他值混淆 8.数据被archived(backup and delete)了置为8
    /// </summary>
    [DataContract]
    public enum RMRecordStatus
    {
        //All: For GUI Search, not for backend.
        [EnumMember]
        All = -1,
        [EnumMember]
        None = 0,
        [EnumMember]
        Active = 1,
        [EnumMember]
        Destroyed = 2,
        [EnumMember]
        RMDeleted = 3,
        [EnumMember]
        Moved = 4,
        [EnumMember]
        MoveOverwrite = 5,
        [EnumMember]
        Closed = 6,
        [EnumMember]
        Missing = 7,
        [EnumMember]
        Archived = 8,
        [EnumMember]
        ManualPreSync = 9,
        [EnumMember]
        Retention = 10,
        [EnumMember]
        TrainingManualSync = 11,
        [EnumMember]
        Hidden = 999,
    }

    [DataContract]
    public enum RetentionSourceFlag
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Rule = 1,
        [EnumMember]
        Storage = 2,
    }

    [DataContract]
    public enum EPhysicalRequestType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ErrorMessage = 1,
        [EnumMember]
        PopupMessage = 2,
    }
}
