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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListRssSettingOperation : CAOperation
    {
        // Summary:
        //  default rss settings
        [DataMember]
        public bool IsDefault { get; set; }

        [DataMember]
        public bool IsLibrary { get; set; }

        [DataMember]
        public BaseType BaseType { get; set; }

        // Summary:
        //  List RSS
        [DataMember]
        public bool AllowRss { get; set; }

        // Summary:
        //  List's Parent Web RSS
        [DataMember]
        public bool AllowRssOnSite { get; set; }

        // Summary:
        //  RSS Channel Information :  Truncate multi-line text fields to 256 characters?  
        [DataMember]
        public bool LimitDescriptionLength { get; set; }

        // Summary:
        //  RSS Channel Information : Title
        [DataMember]
        public string ChannelTitle { get; set; }

        // Summary:
        //  RSS Channel Information : Description
        [DataMember]
        public string ChannelDescription { get; set; }

        // Summary:
        //  RSS Channel Information : Image URL
        [DataMember]
        public string ChannelImageUrl { get; set; }

        // Summary:
        //  Item Limit 
        [DataMember]
        public string ItemLimit { get; set; }
        [DataMember]
        public string DayLimit { get; set; }

        // Summary:
        //  Document Options : Include file enclosures for items in the feed? 
        [DataMember]
        public bool DocumentAsEncloseure { get; set; }

        // Summary:
        //  Document Options : Link RSS items directly to their files? 
        [DataMember]
        public bool DocumentAsLink { get; set; }

        [DataMember]
        public List<CAListRssColumnOperation> RssColumns { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListRssColumnOperation
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public bool Include { get; set; }

        [DataMember]
        public int Order { get; set; }
    }
}
