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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365AppToken.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365AppProfileModel
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string AppName { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string DisplayName
        {
            get
            {
                return string.Format("{0} ({1})", AppName, UserName);
            }
        }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public AzureRegions AzureRegion { get; set; }
        [DataMember]
        public AppProfileType AppType { get; set; }
        [DataMember]
        public string AppId { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string O365Password { get; set; }
        [DataMember]
        public byte[] Certificate { get; set; }
        [DataMember]
        public string CertificatePath { get; set; }
        [DataMember]
        public AppProfileState Status { get; set; }
        [DataMember]
        public bool ReAuthorizeDefault { get; set; }
        //[DataMember]
        //public bool NeedReAuthorize { get; set; } fly为兼容老数据的属性
        [DataMember]
        public AppErrorType AppErrorType { get; set; }
        [DataMember]
        public string RedirectURL { get; set; }
        [DataMember]
        public long ModifiedTime { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }
        [DataMember]
        public bool SaveWithoutAuth { get; set; }
    }

    public class AppProfileResultModel
    {
        public string Id { get; set; }
        public AzureRegions AzureRegion { get; set; }
        public string AppId { get; set; }
        public string TenantId { get; set; }
        public AppErrorType ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        //public bool NeedReAuthorize { get; set; }
    }

    public enum AppErrorType
    {
        None = 0,
        UserNameEmpty = 1,
        AppIdEmpty = 2,
        CertificateNull = 3,
        CertificateInvalid = 4,
        PasswordEmpty = 5,
        PasswordInvalid = 6,
        NoTenant = 7,
        AppNameEmpty = 8,
        AppNameExist = 9,
        O365PasswordEmpty = 10,
        CreateAppFailed = 11,
        CertificateSizeNot2048 = 12,
        AppCertificateError = 13,
        Nochange = 14,
        AppInUse = 15,
        BrowserResultFail=16
    }
}
