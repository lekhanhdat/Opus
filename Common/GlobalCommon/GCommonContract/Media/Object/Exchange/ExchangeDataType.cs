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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System.Runtime.Serialization;
    #endregion

    [DataContract]
    public enum ExchangeDataType
    {
        [EnumMember]
        Mailbox = 0,
        [EnumMember]
        Folder = 1,
        [EnumMember]
        Item = 2,
        [EnumMember]
        Index = 3,
        [EnumMember]
        Plan = 4,
        [EnumMember]
        Task = 5,
        [EnumMember]
        Export = 6,
        [EnumMember]
        Attachment = 7,
        [EnumMember]
        Calendar = 8,
        [EnumMember]
        CalendarEvent = 9,
        [EnumMember]
        Post = 10,
        [EnumMember]
        SiteAttachmentItem = 11,
        [EnumMember]
        SiteDocumentItem = 12,
        [EnumMember]
        SiteVersionItem = 13,
        [EnumMember]
        None = 14,
        [EnumMember]
        SiteCollection = 15,
        [EnumMember]
        SiteList = 16,
        [EnumMember]
        SiteFolder = 17,
        Web = 18
    }
}
