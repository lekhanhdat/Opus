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



namespace AvePoint.GCommon.Contract.CommonFilter
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    /// <summary>
    /// Policy对应的level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PolicyLevel
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        WebApplication = 1,
        [EnumMember]
        SiteCollection = 2,
        [EnumMember]
        Site = 4,
        [EnumMember]
        List = 8,
        [EnumMember]
        Folder = 16,
        [EnumMember]
        Item = 32,
        [EnumMember]
        Document = 64,
        [EnumMember]
        Attachment = 128,
        [EnumMember]
        DocumentVersion = 256,
        [EnumMember]
        ItemVersion = 512,
        //for auditor
        [EnumMember]
        User = 1024,
        //for auditor
        [EnumMember]
        ADProfile = 2048,
        //for auditor
        [EnumMember]
        Url = 4096,
        [EnumMember]
        Library = 8192,
        //For record
        [EnumMember]
        PhysicalBox = 10001,
        //For record
        [EnumMember]
        PhysicalFile = 10002,
        [EnumMember]
        View = 16384,
        [EnumMember]
        FileSysFile = 32768,
        [EnumMember]
        FileSysFolder = 65536,
        #region EO Item Sub Type for Archiver Filter, do not use them in rule
        [EnumMember]
        ExchangeOnlineItem_Message = 6553601,
        [EnumMember]
        ExchangeOnlineItem_Task = 6553602,
        [EnumMember]
        ExchangeOnlineItem_Post = 6553603,
        [EnumMember]
        ExchangeOnlineItem_Event = 6553604,
        [EnumMember]
        ExchangeOnlineItem_Journal = 6553605,
        [EnumMember]
        ExchangeOnlineItem_Note = 6553606,
        [EnumMember]
        ExchangeOnlineItem_Contact = 6553607,
        [EnumMember]
        ExchangeOnlineItem_Document = 6553608,
        #endregion
        [EnumMember]
        Newsfeed = 131072,
        [EnumMember]
        App = 262144,
        [EnumMember]
        AdvancedSearch = 524288,
       
    }
}
