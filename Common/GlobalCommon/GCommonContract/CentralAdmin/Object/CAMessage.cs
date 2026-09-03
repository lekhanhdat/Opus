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






namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    using System.Text;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAMessage : AveMessage
    {
        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }

        [DataMember]
        public CAAction Action { get; set; }

        [DataMember]
        public Boolean IsUseMutliThreadHandleMessage { get; set; }

        [DataMember]
        public List<CAOperation> Operations { get; set; }

        [DataMember]
        public String Extension { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BposInfo
    {
        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public BposUserAccountInfo UserAccountInfo { get; set; }

        [DataMember]
        public BPOSMode Mode { get; set; }

        [DataMember]
        public BposConnectionType ConnectionType { get; set; }

        [DataMember]
        public AppType AppType { get; set; }

        [DataMember]
        public MailboxType MailboxType { get; set; }
        [DataMember]
        public string TenantGroupId { get; set; }
        
        

        public string CustomerId { get; set; } //new

        [DataMember]
        public List<BposUserAccountInfo> ExternalBposInfos { get; set; }//new

        [DataMember]
        public AADEnvironment? AADEnvType { get; set; }//new

        [DataMember]
        public string DelegateAppUserName { get; set; }//new

        [DataMember]
        public bool EnableHideFolder { get; set; }//new

        [DataMember]
        public string SecurityGroupName { get; set; }//new

        [DataMember]
        public string ServiceAccountUsername { get; set; }//new
    }

    [DataContract]
    public enum BposConnectionType
    {
        [EnumMember]
        ServiceAccount = 0,

        [EnumMember]
        AppToken = 1,

        [EnumMember]
        Modern = 2

    }

    [DataContract]
    public enum AppType
    {
        [EnumMember]
        Office365 = 0,

        [EnumMember]
        SharePoint = 1,

        [EnumMember]
        Exchange = 2,

        [EnumMember]
        CustomAzureApp = 3,

        [EnumMember]
        CustomDelegateApp = 4,

        [EnumMember]
        CloudRecords = 5,

        [EnumMember]
        Google = 6,//new

        [EnumMember]
        MicrosoftDelegate = 7,//new
        [EnumMember]
        YammerApp = 8,//new
        [EnumMember]
        CBForM365 = 6,//new
        [EnumMember]
        CBForSharePointApp = 9,//new
        [EnumMember]
        CBForExchangeApp = 10,//new


        AOSPTokenApp = 11,//new
        [EnumMember]
        AospCustomDelegateApp = 12
    }

    [DataContract]
    public enum BPOSMode
    {
        [EnumMember]
        Undetermined,

        [EnumMember]
        SecurityTrimming,

        [EnumMember]
        Office365
    }

    [Flags]
    [DataContract]
    public enum DelegateAppCloudBackupModuleType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Channel = 1,
        [EnumMember]
        Planner = 2,
        [EnumMember]
        PowerBI = 4,
        [EnumMember]
        PowerAutomate = 8
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AADEnvironment
    {
        [EnumMember]
        AzureCloud = 0,
        [EnumMember]
        AzureChinaCloud = 1,
        [EnumMember]
        USGovernment = 2,
        [EnumMember]
        AzureGermanyCloud = 3,
        [EnumMember]
        USGovernment_DoD = 4,
        [EnumMember]
        AzurePPE = 99,
        [EnumMember]
        None = 255
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BposUserAccountInfo
    {
        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }

        [DataMember]
        public string SecondaryUsername { get; set; }

        [DataMember]
        public string SecondaryPassword { get; set; }
        [DataMember]
        public string SecurityGroup { get; set; }
        [DataMember]
        public string AppClientId { get; set; }
        [DataMember]
        public string AppCertSecret { get; set; }
        //[DataMember]
        //public string AppCertContent { get; set; }
        [DataMember]
        public string AppCertSecretContent { get; set; }
        [DataMember]
        public AADEnvironment AADEnvironment { get; set; }
        [DataMember]
        public string AppId { get; set; }

        [DataMember]
        public bool SecondarySAIsMFA { get; set; }//new

        [DataMember]
        public string SecondarySAUsername { get; set; }//new

        [DataMember]
        public string ServiceAccountUsername { get; set; }//new

        [DataMember]
        public string MFAUsername { get; set; }//new


        [DataMember]
        public bool ServiceAccountIsMFA { get; set; }//new

        [DataMember]
        public List<string> ExchangeUserNames { get; set; }

        /// <summary>
        /// 节点上请勿使用这个apptype，应使用外层的apptype
        /// </summary>
        [DataMember]
        public AppType AppType { get; set; }//new

        [DataMember]
        public string CustomerAppId { get; set; }//new
        [DataMember]
        public string AppUserName { get; set; }//new

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccountPoolInfo
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string SecurityGroup { get; set; }

        [DataMember]
        public string ServiceAccountId { get; set; }

        [DataMember]
        public string AdminUrl { get; set; }

        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public BposConnectionType ConnectionType { get; set; }

        [DataMember]
        public AppType IdentityProviderType { get; set; }

        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<TenantUser> Users { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TenantUser
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string AdminUrl { get; set; }

        [DataMember]
        public string TenantName { get; set; }

        [DataMember]
        public bool IsEnableMFA { get; set; }

        [DataMember]
        public bool IsServiceAccountUser { get; set; }

        [DataMember]
        public int UserStatus { get; set; }

        [DataMember]
        public Office365UserRole Role { get; set; }

        public int Weight { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum Office365UserRole
    {
        User = 0,
        GlobalAdministrator = 1,
        ExchangeAdministrator = 2,
        SharePointAdministrator = 4,
        DynamicAdministrator = 8
    }

    public class ReturnResult
    {
        Boolean _isOK = true;

        [DataMember]
        public Boolean IsOk { get { return _isOK; } set { _isOK = value; } }

        [DataMember]
        public String ErrorMessage { get; set; }

        [DataMember]
        public CAStringFormatMessage ErrorMessageFormat { get; set; }

        [DataMember]
        public List<PropertyItem> I18NStrList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CAAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Set,
        [EnumMember]
        Get,
        [EnumMember]
        Add,
        [EnumMember]
        Extended,
        [EnumMember]
        Update,
        [EnumMember]
        Block,
        [EnumMember]
        Remove,
        [EnumMember]
        Submit,
        [EnumMember]
        Delete,
        [EnumMember]
        Load,
        [EnumMember]
        Test,
        [EnumMember]
        Verify,

    }

    /// <summary>
    /// The enum derived from AveContextKind in Wrapper.Common.dll.
    /// Plesase make sure the values are consistent.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ApiObjectModelType
    {
        //[EnumMember]
        //ServerObjectModel = 2,
        //[EnumMember]
        //ClientObjectModel = 1,

        [EnumMember]
        Auto = 0,

        [EnumMember]
        ClientObjectModel = 1,

        [EnumMember]
        ServerObjectModel = 2,

        [EnumMember]
        Server07ObjectModel = 3
    }

    /// <summary>
    /// 方便GUI国际化使用
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAStringFormatMessage
    {
        [DataMember]
        public string FormatString { get; set; }

        [DataMember]
        public List<string> Parameters { get; set; }

        public void Format(string formatString, params string[] parameters)
        {
            FormatString = formatString;
            Parameters = new List<string>();
            foreach (string parameter in parameters)
            {
                Parameters.Add(parameter);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (FormatString == null && Parameters == null)
            {
                return string.Empty;
            }
            if (FormatString != null)
            {
                if (Parameters != null)
                {
                    builder.Append(string.Format(FormatString, Parameters.ToArray()));
                }                
            }
            return builder.ToString();
        }
    }

   
}
