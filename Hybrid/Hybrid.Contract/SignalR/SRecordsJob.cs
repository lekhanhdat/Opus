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
using CommonModel.MethodInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.Contract
{
    public class SRecordsJob : RemoteMessage<RecordsJobArgs>
    {

        public override RecordsJobArgs MethodArgs { get; set ; }

        public override string MethodName { get { return MethodMapping.MT[typeof(SRecordsJob)]; } }
    }


    public class RecordsJobArgs
    {
        public string JobId { set; get; }    
        public JobType JobType { set; get; }   
        public string TenantId { get; set; }
        public string FarmId { get; set; }
        public string AgentId { get; set; }
        public string TenantRegisterEmail { get; set; }
        public string Extensions { get; set; }
    }

    public class SFileSystemJobExecute : RemoteInvoke<RecordsJobArgs, FileSystemJobResult>
    {
        public override RecordsJobArgs MethodArgs { get; set; }
        public override FileSystemJobResult MethodResult { get; set; }

        public override string MethodName => MethodMapping.MT[typeof(SFileSystemJobExecute)];
    }
    [DataContract]
    public enum JobType
    {
        [EnumMember]
        Explore,
        [EnumMember]
        FSDataSync,
        [EnumMember]
        FSDisposal,
        [EnumMember]
        FSDisposalByClassCode,
        [EnumMember]
        SharePointOnPremApplySetting,
        [EnumMember]
        SharePointOnPremEnforceRuleAction,
        [EnumMember]
        SPOnPremTermSynchronization,
        [EnumMember]
        SharePointOnPremDataSync,
        [EnumMember]
        SPOnPremUniqueIDSetting,
        [EnumMember]
        SPOnPremGlobalSearch,
        [EnumMember]
        SPOnPremScanNode,
        [EnumMember]
        ImportFSSetting,
        [EnumMember]
        FSContentDueReport,
        [EnumMember]
        FSCreationAndDestructionReport,
        [EnumMember]
        FSArchiverRestore,
        [EnumMember]
        FSRetain,
        [EnumMember]
        FSRetainSimulate,
        [EnumMember]
        FSDiscovery,
    }

    public class FileSystemJobResult
    {
        public FileSystemResultEnum Result { set; get; }
        public string Message { set; get; }

    }

    public enum FileSystemResultEnum
    {
        Succeed,
        Failed
    }
    public class RecordsJobStopArgs
    {
        public string JobId { get; set; }
        public string TenantId { get; set; }
    }

    public class SRecordsJobStop : RemoteMessage<RecordsJobStopArgs>
    {
        public override RecordsJobStopArgs MethodArgs { get; set; }
        public override string MethodName => MethodMapping.MT[typeof(SRecordsJobStop)];
    }
}
