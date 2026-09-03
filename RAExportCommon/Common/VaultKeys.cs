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
using System.Linq;
using System.Text;

namespace RAExportCommon
{
    internal class VaultKeys
    {
        public const string VAULT = "vault";
        public const string LASTVAULTTIME = "lastVaultTime";
        public const string ITEMID = "itemId";
        public const string VAULTFAILEDITEM = "vaultFailedItem";
        public const string MODIFIED = "Modified";
        public const string CREATED = "Created";
        public const string AUTHOR = "Author";
        public const string EDITOR = "Editor";
        public const string NULL = "null";
        public const string ROLEASSIGNMENT = "RoleAssignment";
        public const string WIKICONTENT = "Wiki Content";
        public const string ZERO = "0";
        public const string TITLE = "Title";
        public const string _UIVERSIONSTRING = "_UIVersionString";
        public const string AUTONOMYGROUP = "SP2013";
        public const string VAULTFIELDHEAD = "SP_";
        public const string DISCUSSIONTYPE = "0x012002";//"Discussion"
        public const string SYSTEMTYPE = "0x";//"System"
        public const string FILESIZE = "File_x0020_Size";

        public const string RETITLE = "TITLE";
        public const string RESMODIFIED = "MODIFIED";
        public const string RESMODIFIEDBY = "MODIFIEDBY";
        public const string RESCREATED = "CREATED";
        public const string RESCREATEDBY = "CREATEDBY";
        public const string RESEDITOR = "EDITOR";
        public const string RESAUTHOR = "AUTHOR";
        public const string RESSIZE = "Size";
        public const string RES_UIVERSIONSTRING = "_UIVERSIONSTRING";
    }
    internal class VaultLogFormat
    {
        public const string LOG = "VAULT:{0}";
        public const string LOGWITHPATH = "VAULT:{0} Path is:{1}";
        public const string LOGWITHEXCEPTION = "VAULT:{0}\r\n Exception is:{1}";
        public const string LOGWITHEXCEPTIONPATH = "VAULT:{0} Path is:{1}\r\n ERROR IS:{2}";//{0}error message,{1}path,{2}Exception.tostring()
    }

    public enum VaultExportFormat
    {
        VEO,
        NAA,
        NARA,
        EXOVEO,
        EXONAA,
        EXONARA,
    }

    public enum VaultMergeType
    {
        Base,
    }
}
