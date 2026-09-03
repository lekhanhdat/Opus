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
using AvePoint.GCommon.Contract.AveLicense.Detail;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.AveLicense
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseOperationResult
    {
        public LicenseOperationResult(bool hasError, LicenseOperationError error)
        {
            HasError = hasError;
            Error = error;
        }

        public LicenseOperationResult()
            : this(false, LicenseOperationError.None)
        { }

        [DataMember]
        public LicenseDetail LicenseDetail { get; set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public bool NeedChange { get; set; }

        [DataMember]
        public LicenseOperationError Error { get; set; }

        [DataMember]
        public List<FarmDto> Farms { get; set; }

        [DataMember]
        public long SystemTime { get; set; }

        [DataMember]
        public LicenseNotificationSetting NotificationSetting { get; set; }

        [DataMember]
        public Nullable<int> NotCompliantDuration { get; set; }

        [DataMember]
        public bool IsNotifyAddGlobalAdmin { get; set; }

        [DataMember]
        public string[] CentralAdminUrls { get; set; }

        [DataMember]
        public RetrieveUserSetting RetrieveUserSetting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicenseOperationError
    {
        [EnumMember]
        None,
        [EnumMember]
        AlreadyRegistered,
        [EnumMember]
        InvalidLicenseFile,
        [EnumMember]
        FailedToGetLicenseDetail,
        [EnumMember]
        FailedToApplyLicense,
    }

    public class LicenseCheckResult
    {
        public LicenseCheckState State
        {
            get
            {
                if (TreeType == LicenseTreeType.MOSS)
                {
                    return MossLicenseState;
                }
                else
                {
                    bool hasAvailable = false;
                    bool hasUnavailable = false;
                    if (SiteCollectionResultList == null)
                    {
                        return LicenseCheckState.Unavailable;
                    }
                    foreach (RemoteSiteCollectionLicenseResult siteCollectionResult in SiteCollectionResultList)
                    {
                        if (siteCollectionResult.State == RemoteSiteCollectionLicenseState.Available)
                        {
                            hasAvailable = true;
                        }
                        else
                        {
                            hasUnavailable = true;
                        }
                    }
                    if (hasAvailable && hasUnavailable)
                    {
                        return LicenseCheckState.PartiallyAvailable;
                    }
                    else if (hasAvailable)
                    {
                        return LicenseCheckState.Available;
                    }
                    else if (hasUnavailable)
                    {
                        return LicenseCheckState.Unavailable;
                    }
                    else
                    {
                        return LicenseCheckState.Unavailable;
                    }
                }
            }
        }

        public IList<RemoteSiteCollectionLicenseResult> SiteCollectionResultList { get; set; }

        public LicenseCheckState MossLicenseState { get; set; }

        public LicenseTreeType TreeType { get; set; }

        public LicenseCheckResult()
        {
            SiteCollectionResultList = new List<RemoteSiteCollectionLicenseResult>();
        }
    }

    [DataContract]
    public enum LicenseTreeType
    {
        [EnumMember]
        MOSS,

        [EnumMember]
        BPOS
    }

    [DataContract]
    public enum LicenseCheckState
    {
        [EnumMember]
        Available,

        [EnumMember]
        Unavailable,

        [EnumMember]
        PartiallyAvailable
    }

    [DataContract]
    public class RemoteSiteCollectionLicenseResult
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public long LastModifyTime { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public RemoteSiteCollectionLicenseState State { get; set; }
    }

    [DataContract]
    public enum RemoteSiteCollectionLicenseState
    {
        [EnumMember]
        Available,

        [EnumMember]
        Unavailable,

        [EnumMember]
        CannotFindSiteCollection
    }
}
