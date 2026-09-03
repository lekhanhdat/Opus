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
    public class AveConstants
    {
        public const string UNKNOWN_STRING = "Unknown";
        public const string APVC_SERVICE_NAME = "DocAveAPVCService_V6";
        public const string APVC_SERVICE_DISPLAY_NAME = "DocAve (V6) Communication Service";
        public const string APVC_SERVICE_DESCRIPTION = "DocAve (V6) Manager and Agent Communication Interface";
        public const string AVE_SP14_REGISTER_PATH = @"software\Microsoft\shared Tools\web Server Extensions\14.0";
        public const string AVE_SP14_DLL_PATH = @"\Microsoft Shared\Web Server Extensions\14\ISAPI";

        public const int TRANSMISSION_ENCRYPTED = 0x00000001;
        public const int TRANSMISSION_COMPRESSED = 0x00000002;

        public const long SERVERSIDE_ENCRYPTED = 1 << 5;
        public const long SERVERSIDE_COMPRESSED = 1 << 4;

        public const int ENCRYPT_FLAG = 0x01;

        public const int BUFFER_SIZE = 64 * 1024 - 16;

        public const string FIELD_SEPARATOR = "#";
        public const string SYSTEM_FOLDER = "{System Folder}";
        public const string ROOT_FOLDER = "{Root Folder}";
        public const string MY_COLLEAGUES = "{My Colleagues}";
        public const string MY_DETAILS = "{My Details}";
        public const string MY_MEMBERSHIPS = "{My Memberships}";
        public const string MY_NOTES = "{My Notes}";
        public const string MY_TAGS = "{My Tags}";
        public const string MY_LINKS = "{My Links}";
        public const string WFPUB_LIST = "wfpub";
        public const string ROOT_WEB = ".";

        #region DocAve client sharepoint type
        public const char TYPE_FARM = 'R';
        public const char TYPE_WEBAPPLICATION = 'S';
        public const char TYPE_SITE = 'E';
        public const char TYPE_WEB = 'W';
        public const char TYPE_LIST = 'L';
        public const char TYPE_VIEW = 'H';
        public const char TYPE_LIBRARY_LIST = 'B';
        public const char TYPE_MYPROFILE_LIST = 'P';
        public const char TYPE_FOLDER = 'F';
        public const char TYPE_FOLDER_VERSION = 'N';
        public const char TYPE_DOCUMENT = 'D';
	    public const char TYPE_DOCUMENT_VERSION= 'K';
        public const char TYPE_LISTITEM = 'I';
        public const char TYPE_LISTITEMVERSION = 'U';
        public const char TYPE_MYPROFILE_ITEM = 'O';
        public const char TYPE_ATTACHMENTS = 'A';
        public const char TYPE_VERSION = 'V';
        public const char TYPE_AREA = 'C';
        public const char TYPE_INDEX = 'X';
        public const char TYPE_CATALOG = 'G';
        public const char TYPE_LASTCATALOG = 'M';
        public const char TYPE_DATA = 'T';
        public const char TYPE_APP = 'Y';

        public const int TYPE_VALUE_FARM = 1;
        public const int TYPE_VALUE_WEBAPPLICATION = 2;
        public const int TYPE_VALUE_CONTENT_DATABASE = 30;
        public const int TYPE_VALUE_SITE = 100;
        public const int TYPE_VALUE_WEB = 200;
        public const int TYPE_VALUE_APP = 280;
        public const int TYPE_VALUE_APPDEFINITION = 281;
        public const int TYPE_VALUE_GENERIC_LIST = 300;
        public const int TYPE_VALUE_DOCUMENT_LIBRARY_LIST = 301;
        public const int TYPE_VALUE_DISSCUSSION_FORUM_LIST = 303;
        public const int TYPE_VALUE_VOTE_OR_SURVEY_LIST = 304;
        public const int TYPE_VALUE_ISSUES_LIST = 305;

        public const int TYPE_VALUE_FOLDER = 400;
        public const int TYPE_VALUE_DOCUMENT = 500;
        public const int TYPE_VALUE_LISTITEM = 510;
        public const int TYPE_VALUE_ATTACHMENTS = 502;
        public const int TYPE_VALUE_VERSION = 503; // TODO Change to correct value

        #region Extend for ContentDesign
        public const int TYPE_VALUE_ROOTWEB = 201;
        public const int TYPE_VALUE_ROOTFOLDER = 402;
        public const int TYPE_VALUE_COLUMNGROUP = 451;
        public const int TYPE_VALUE_COLUMN = 551;
        public const int TYPE_VALUE_CONTENTTYPEGROUP = 452;
        public const int TYPE_VALUE_CONTENTTYPE = 552;
        #endregion
        #endregion

        #region SQL data type
        public const byte SQL_BOOL = 1;
        public const byte SQL_BYTE = 2;
        public const byte SQL_INT16 = 3;
        public const byte SQL_INT32 = 4;
        public const byte SQL_INT64 = 5;
        public const byte SQL_GUID = 6;
        public const byte SQL_DATETIME = 7;
        public const byte SQL_FLOAT = 8;
        public const byte SQL_DECIMAL = 9;
        public const byte SQL_DOUBLE = 10;
        public const byte SQL_STRING = 11;
        public const byte SQL_BINARY = 12;
        public const byte SQL_BIGBIN = 13;
        public const byte SQL_UNKNOWTYPE = 255;
        #endregion

        #region SharePoint Constant Id
        public const int SYSTEM_ACCOUNT_ID = 1073741823;
        public const int LIMIT_ACCESS_ROLE_ID = 1073741825;
        public const string SP_EXCEPTION_STRING = "Microsoft.SharePoint.SPException";
        #endregion

        #region FeatureId
        public static Guid OFFICEPUBLISHINGSITE = new Guid("F6924D36-2FA8-4f0b-B16D-06B7250180FA");
        public static Guid PUBLISHINGRESOURCES = new Guid("AEBC918D-B20F-4a11-A1DB-9ED84D79C87E");
        #endregion
    }
}

