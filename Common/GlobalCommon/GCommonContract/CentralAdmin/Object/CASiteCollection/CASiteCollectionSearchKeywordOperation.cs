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





using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASiteCollectionSearchKeywordOperation : CAOperation
    {
        [DataMember]
        public string SiteCollectionUrl { get; set; }
        [DataMember]
        public List<KeywordInfo> Keywords { get; set; }
        [DataMember]
        public SearchKeywordField SearchField { get; set; }
        [DataMember]
        public string SearchText { get; set; }
        [DataMember]
        public bool IsEditTerm { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class KeywordInfo
    {
        [DataMember]
        public string KeywordPhrase { get; set; }
        [DataMember]
        public string KeywordSynonyms { get; set; }
        [DataMember]
        public List<BestBetInfo> BestBets { get; set; }
        [DataMember]
        public string KeywordDefinition { get; set; }
        [DataMember]
        public UserDetail KeywordContact { get; set; }
        [DataMember]
        public string StartDate { get; set; }
        [DataMember]
        public string EndDate { get; set; }
        [DataMember]
        public string ReviewDate { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BestBetInfo
    {
        [DataMember]
        public string BestBetURL { get; set; }
        [DataMember]
        public string BestBetTitle { get; set; }
        [DataMember]
        public string BestBetDescription { get; set; }
    }

    public enum SearchKeywordField
    {
        Keyword = 0,
        Synonyms = 1, 
        BestBetTitle = 2,
        BestBetURL = 3,
        Contact = 4
    }
}
