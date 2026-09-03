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
using AvePoint.GCommon.Contract.Agent.ExchangeBrowser.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EmailAccountGroupDto
    {
        [DataMember]
        public String id { get; set; }
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public NodeLevel NodeLevel { get; set; }

        [IgnoreDataMember]
        public bool FromDAO { get; set; }

        [IgnoreDataMember]
        public string AosId { get; set; }

        public override string ToString()
        {
            return string.Format("EmailAccountGroupDto[Id {0}, Name {1}]", id, Name);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EmailAccountDto
    {
        [DataMember]
        public String Id { get; set; }
        [DataMember]
        public String Email { get; set; }
        [DataMember]
        public String ParentId { get; set; }
        [IgnoreDataMember]
        public String ParentName { get; set; }
        [DataMember]
        public String Username { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public EmailAccountState State { get; set; }
        [DataMember]
        public String FullPath { get; set; }
        /// <summary>
        /// 用来区分SP的版本，格式是int32.int32.int32.int32
        /// </summary>
        [DataMember]
        public String SPVersion { get; set; }
        [DataMember]
        public String ServiceUrl { get; set; }
        [DataMember]
        public NodeLevel NodeLevel { get; set; }
        [DataMember]
        public String TenantId { get; set; }
        [DataMember]
        public BposConnectionType ConnectionType { get; set; }
        [DataMember]
        public AppType AppType { get; set; }
        [DataMember]
        public List<ObjectPermissionDto> ObjectPermissions { get; set; }

        [DataMember]
        public String ServiceAccountId { get; set; }

        [DataMember]
        public MailboxType MailboxType { get; set; }

        [DataMember]
        public MailboxScanSource ScanSource { get; set; }

        [IgnoreDataMember]
        public bool FromDAO { get; set; }

        [IgnoreDataMember]
        public string ObjectId { get; set; }

        public override string ToString()
        {
            return string.Format("EmailAccountDto[Id {0}, Email {1}]", Id, Email);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MailboxScanSource
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        AOS,
        [EnumMember]
        ControlPanel,
        [EnumMember]
        AutoScan,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MailboxType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        PublicFolder = 1,
        [EnumMember]
        User = 2,
        [EnumMember]
        Group = 3,
        [EnumMember]
        Teams = 4,
        [EnumMember]
        PublicFolderMetadata = 5,
        [EnumMember]
        PersonalChat = 6,

        [EnumMember]
        PowerBIWorkspace = 7,

        [EnumMember]
        PowerAutomate = 8,

        [EnumMember]
        PowerApps = 9
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EmailAccountState
    {
        [EnumMember]
        AccessAll,
        [EnumMember]
        AccessNone,
        //[EnumMember]
        //AccountExpired,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EmailAccountTestResult
    {
        [DataMember]
        public EmailAccountState State { get; set; }
        [DataMember]
        public String RealEmail { get; set; }
        [DataMember]
        public ExchangeOnlineErrorInfo ErrorInfo { get; set; }
        [DataMember]
        public String SPVersion { get; set; }
        [DataMember]
        public String ServiceUrl { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScanEmailsResult
    {
        [DataMember]
        public ScanEmailsResultState ResultState { get; set; }
        [DataMember]
        public ExchangeOnlineErrorInfo ResultErrorInfo { get; set; }
        [DataMember]
        public String ServiceUrl { get; set; }
        [DataMember]
        public List<string> AccessEmails { get; set; }
        [DataMember]
        public List<string> UnAccessEmails { get; set; }
        [DataMember]
        public List<O365GroupScanResult> O365GroupScanResults { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public long TimeOut { get; set; }
    }

    [DataContract(Namespace=ContractConstants.Namespace)]
    public class O365GroupScanResult
    {
        [DataMember]
        public string Name{get;set;}
        [DataMember]
        public string GroupName{get;set;}
        [DataMember]
        public string SiteCollectionUrl{get;set;}
        [DataMember]
        public bool Avaiable{get;set;}
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScanEmailsInfo
    {
        [DataMember]
        public BposUserAccountInfo AccountInfo { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public string TenantUser { get; set; }
        [DataMember]
        public string TenantGroupId { get; set; }
        [DataMember]
        public string TenantGroupOwner { get; set; }
        [DataMember]
        public bool IncludeInPlaceArchiveMailbox { get; set; }
        [DataMember]
        public bool IncludeResourceMailbox { get; set; }

        [DataMember]
        public ActionType ActionType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReconnectEmailsResult
    {
        [DataMember]
        public bool IsAccessAll { get; set; }
        [DataMember]
        public List<EmailAccountDto> MailList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScanEmailsResultState
    {
        [EnumMember]
        Successful,
        [EnumMember]
        Error,
        [EnumMember]
        NoEmail,
        [EnumMember]
        NoAvailableAgent,
        [EnumMember]
        UnAuthorized,
        [EnumMember]
        PasswordExpired,
        [EnumMember]
        NotGlobalAdmin,
        [EnumMember]
        UnFinish,
        [EnumMember]
        TimeOut,
        [EnumMember]
        NoRegistrationProfile
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ImportEmailsResult
    {
        [DataMember]
        public ImportEmailsResultState ResultState { get; set; }
        [DataMember]
        public int UpAgentCount { get; set; }
        [DataMember]
        public int RegisteredCount { get; set; }
        [DataMember]
        public List<EmailAccountTestResult> Results { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ImportEmailsResultState
    {
        [EnumMember]
        None,
        [EnumMember]
        NoEmails,
        [EnumMember]
        NoAvailableAgent,
        [EnumMember]
        ReadFileError,
        [EnumMember]
        AllEmailsExist
    }
}
