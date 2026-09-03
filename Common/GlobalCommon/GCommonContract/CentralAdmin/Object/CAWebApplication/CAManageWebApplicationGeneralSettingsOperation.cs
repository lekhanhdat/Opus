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
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAManageWebApplicationGeneralSettingsOperation : CAOperation
    {
        [DataMember]
        public String  WebAppUrl { get; set; }

        [DataMember]
        public Int32 DefaultTimeZone { get; set; }

        [DataMember]
        public String DefaultQuotaTemplate { get; set; }

        [DataMember]
        public List<QuotaTemplate> QuotaTemplates { get; set; }

        [DataMember]
        public Boolean PresenceEnabled { get; set; }

        [DataMember]
        public Int32 MaximumFileSize { get; set; }

        [DataMember]
        public Boolean AlertsEnabled { get; set; }

        [DataMember]
        public Boolean AlertsLimited { get; set; }

        [DataMember]
        public Int32 AlertsMaximum { get; set; }

        [DataMember]
        public Boolean SyndicationEnabled { get; set; }

        [DataMember]
        public Boolean MetaWeblogEnabled { get; set; }

        [DataMember]
        public Boolean MetaWeblogAuthenticationEnabled { get; set; }

        [DataMember]
        public FormDigestSettings FormDigestSettings { get; set; }

        [DataMember]
        public BrowserFileHandling BrowserFileHandling { get; set; }

        [DataMember]
        public Boolean MasterPageReferenceEnabled { get; set; }

        [DataMember]
        public Boolean BrowserCEIPEnabled { get; set; }

        [DataMember]
        public Boolean FarmCEIPEnabled { get; set; }

        [DataMember]
        public Boolean SendLoginCredentialsByEmail { get; set; }

        [DataMember]
        public Boolean RecycleBinEnabled { get; set; }

        [DataMember]
        public Boolean RecycleBinCleanupEnabled { get; set; }

        [DataMember]
        public Int32 RecycleBinRetentionPeriod { get; set; }

        [DataMember]
        public Boolean RecycleBinAdd { get; set; }

        [DataMember]
        public Int32 SecondStageRecycleBinQuota { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FormDigestSettings
    {
        [DataMember]
        public Boolean Enabled { get; set; }

        [DataMember]
        public Boolean Expires { get; set; }

        [DataMember]
        public Double MinutesOfSeValidationExpire { get; set; }

    }

    public enum BrowserFileHandling
    {
        Permissive = 0,
        Strict = 1,
    }
}
