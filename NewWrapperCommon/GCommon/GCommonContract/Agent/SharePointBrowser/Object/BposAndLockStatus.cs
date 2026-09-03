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
using System.Text;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Tree.Object;
    using SharePointBrowser;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BposInfo
    {
        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public BposUserAccountInfo UserAccountInfo { get; set; }

        [DataMember]
        public BPOSMode Mode { get; set; }

        /// <summary>
        /// For site collection node in security trimming mode, which stores its original farm id.
        /// </summary>
        [DataMember]
        public string OriginalFarmId { get; set; }

        [DataMember]
        public string RealId { get; set; }

        [DataMember]
        public AuthorizeType AuthorizeType { get; set; }

        [DataMember]
        public BposAppTokenInfo AppTokenInfo { get; set; }
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


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BposUserAccountInfo
    {
        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BposAppTokenInfo
    {
        [DataMember]
        public String ApplicationId { get; set; }

        [DataMember]
        public String TenantId { get; set; }

        [DataMember]
        public string AppTokenCertBase64String { get; set; }

        [DataMember]
        public string AppTokenCertPassword { get; set; }

        [DataMember]
        public AzureRegions AzureRegion { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReturnResult
    {
        Boolean _isOK = true;

        [DataMember]
        public Boolean IsOk { get { return _isOK; } set { _isOK = value; } }

        [Obsolete]
        [DataMember]
        public String ErrorMessage { get; set; }

        [DataMember]
        public CAStringFormatMessage ErrorMessageFormat { get; set; }
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
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LockStatus
    {
        [EnumMember]
        NotLocked,
        [EnumMember]
        AddContentPrevented,
        [EnumMember]
        ReadOnly,
        [EnumMember]
        NoAccess
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AppTokenInfo
    {
        [DataMember]
        public string ProfileId { get; set; }
        [DataMember]
        public AppTokenType Type { get; set; }
        [DataMember]
        public string ApplicationId { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public byte[] Certificate { get; set; }
        [DataMember]
        public String AppTokenCertBase64String { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public AzureRegions AzureRegion { get; set; }
        [DataMember]
        public string AuthorizeUser { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AppTokenType
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        DefaultAzure = 0,
        [EnumMember]
        CustomAzure = 1,
    }

}
