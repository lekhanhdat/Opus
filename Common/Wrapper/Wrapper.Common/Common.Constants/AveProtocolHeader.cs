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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveProtocolHeader
    {
        public char Type { get; set; }        
        public string WebRelativeUrl { get; set; }
        public string ListTitle { get; set; }
        public string FolderRelativeUrl { get; set; }
    }

    public class AveProtocolHeaderConstants
    {
        #region Header Type
        public const char SITE = 'S';
        public const char WEB = 'W';
        public const char LIST = 'L';
        public const char FOLDER = 'F';
        public const char DOCUMENT = 'D';            
        public const char LIST_ITEM = 'I';
        public const char ATTACHMENT = 'A';
        public const char END = '1';
        public const char REPORT = 'R';
        public const char RESET = '2';
        #endregion

        #region Header RelativeUrl
        public const string ROOT_WEB_NAME = ".";
        public const string URL_SEPERATOR = "/";
        #endregion

        public const string HEADER_ELEMENT_NAME = "Header";
        public const string HEADER_ELEMENT_ATTR_TYPE = "type";
        public const string HEADER_ELEMENT_ATTR_PATH = "path";
        public const string HEADER_ELEMENT_ATTR_WEB_RELATIVE_URL = "webRelativeUrl";
        public const string HEADER_ELEMENT_ATTR_FOLDER_RELATIVE_URL = "folderRelativeUrl";
    }
}
