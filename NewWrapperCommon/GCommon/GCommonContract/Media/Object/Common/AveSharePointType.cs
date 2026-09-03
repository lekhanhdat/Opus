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

    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml;
    using AvePoint.GCommon.Contract.Common;

    #endregion using directives

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveSharePointType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        TYPE_ATTACHMENTS = 'A',

        [EnumMember]
        TYPE_BLOB = 'B',

        [EnumMember]
        TYPE_AREA = 'C',

        [EnumMember]
        TYPE_DOCUMENT = 'D',

        [EnumMember]
        TYPE_SITE = 'E',

        [EnumMember]
        TYPE_FOLDER = 'F',

        [EnumMember]
        TYPE_CATALOG = 'G',

        [EnumMember]
        TYPE_LASTCONNECTION = 'H',

        [EnumMember]
        TYPE_LISTITEM = 'I',

        [EnumMember]
        TYPE_FAILEDREPORT = 'J',

        [EnumMember]
        TYPE_LIST = 'L',

        [EnumMember]
        TYPE_LASTCATALOG = 'M',

        [EnumMember]
        TYPE_FOLDER_VERSION = 'N',

        [EnumMember]
        TYPE_MYPROFILE_ITEM = 'O',

        [EnumMember]
        TYPE_MYPROFILE_LIST = 'P',

        [EnumMember]
        TYPE_FARM = 'R',

        [EnumMember]
        TYPE_SHAREDSEARCH = 'S',

        [EnumMember]
        TYPE_DATA = 'T',

        [EnumMember]
        TYPE_LISTITEMVERSION = 'U',

        [EnumMember]
        TYPE_VERSION = 'V',

        [EnumMember]
        TYPE_WEB = 'W',

        [EnumMember]
        TYPE_INDEX = 'X',

        [EnumMember]
        TYPE_APP = 'Y',

        [EnumMember]
        TYPE_VDBMAPPING = 'Z',
    }
}