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






using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
namespace AvePoint.GCommon.Contract.Replicator.Object.Settings
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorConfiguration
    {
        [DataMember]
        public bool Include { get; set; }

        [DataMember]
        public bool ReceiveChangesFromDestination { get; set; }

        [DataMember]
        public ConfigurationSiteCollectionLevel SiteCollectionLevel { get; set; }

        [DataMember]
        public ConfigurationSiteLevel SiteLevel { get; set; }

        [DataMember]
        public ConfigurationListLevel ListLevel { get; set; }

        [DataMember]
        public ConflictAction ConflictAction { get; set; }

        [DataMember]
        public ConflictWinnerRuleValue ConflictSolution { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationSiteCollectionLevel
    {
        [DataMember]
        public bool FeaturesAndProperties { get; set; }

        [DataMember]
        public bool SearchScopesAndSearchKeywords { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationSiteLevel
    {
        [DataMember]
        public bool FeaturesAndProperties { get; set; }

        [DataMember]
        public bool ColumnAndContentType { get; set; }

        [DataMember]
        public bool NavigationAndQuickLaunch { get; set; }

        [DataMember]
        public bool SiteTemplateAndListTemplate { get; set; }

        [DataMember]
        public bool Others { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigurationListLevel
    {
        [DataMember]
        public bool ListSettings { get; set; }

        [DataMember]
        public bool ListAlerts { get; set; }

        [DataMember]
        public bool PersonalViews { get; set; }

        [DataMember]
        public bool PublicViews { get; set; }
    }


}
