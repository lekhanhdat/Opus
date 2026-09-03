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
    public class CASiteCollectionFastSearchKeywordsOperation: CAOperation
    {
        [DataMember]
        public bool IsFastProxyEnabled { get; set; }

        [DataMember]
        public List<FastSearchKeyword> Keywords { get; set; }

        [DataMember]
        public List<UserContextInfo> UserContexts { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FastSearchKeyword
    {
        [DataMember]
        public string KeywordName { get; set; }

        [DataMember]
        public string NewKeywordName { get; set; }

        [DataMember]
        public string TwoWaySynonyms { get; set; }

        [DataMember]
        public string OneWaySynonyms { get; set; }

        [DataMember]
        public string KeywordDefinition { get; set; }

        [DataMember]
        public ActionTypes Action { get; set; }        

        [DataMember]
        public List<KeywordCommonInfo> BestBets { get; set; }

        [DataMember]
        public List<KeywordCommonInfo> VisualBestBets { get; set; }

        [DataMember]
        public List<KeywordCommonInfo> DocumentPromotions { get; set; }

        [DataMember]
        public List<KeywordCommonInfo> DocumentDemotions { get; set; }

        [DataMember]
        public string ModifiedDate { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class KeywordCommonInfo
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string NewTitle { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string URL { get; set; }

        [DataMember]
        public List<UserContextInfo> UserContexts { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        public bool isChecked { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ActionTypes
    {
        //Keyword
        [EnumMember]
        Keyword,
        //BestBets
        [EnumMember]
        BestBets,
        //VisualBestBets
        [EnumMember]
        FeaturedContents,
        //DocumentPromotion
        [EnumMember]
        Boosts,
        //DocumentDemotion
        [EnumMember]
        Demotions
    }
}
