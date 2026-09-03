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





using AvePoint.Common;
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveWrapperConstants
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveWrapperConstants));
        public const string ROOT_ELEMENT = "Data";
        public const string ROOT_VERSION = "version";
        public const string CURRENT_VERSION = "1.0";

        public const string COLUMN_ELEMENT = "Field";
        public const string COLUMN_NAME = "name";
        public const string COLUMN_TYPE = "type";
        public const string COLUMN_KEY_TYPE = "keyType";
        public const string AUTO_COMPRESSED_STRING_LABEL = "4bfb05b4-43f9-4a70-80c4-97928dfb95d7:";
        public const string AUTO_COMPRESSED_BYTES_LABEL = "070bf284-eb25-4534-8658-c4767a774942:";
        public const string AUTO_COMPRESSED_CHILDREN_LABEL = "4430f049-2b6a-443c-918a-72597f6ab476:";

        public const string VALUE_NULL = "null";
        public const string VALUE_COMPLEX = "1";

        public const string TYPE_STRING = "string";
        public const string TYPE_BOOL = "bool";
        public const string TYPE_BYTE = "byte";
        public const string TYPE_CHAR = "char";
        public const string TYPE_SHORT = "short";
        public const string TYPE_INT = "int";
        public const string TYPE_LONG = "long";
        public const string TYPE_FLOAT = "float";
        public const string TYPE_DOUBLE = "double";
        public const string TYPE_DECIMAL = "decimal";
        public const string TYPE_URI = "uri";
        public const string TYPE_GUID = "guid";
        public const string TYPE_DATETIME = "datetime";
        public const string TYPE_BINARY = "binary";
        public const string TYPE_SBYTE = "sbyte";
        public const string TYPE_USHORT = "ushort";
        public const string TYPE_UINT = "uint";
        public const string TYPE_ULONG = "ulong";

        public const int HEADER_SIZE = 16;
        public const int BUFFER_SIZE = 64 * 1024;
        public const byte METADATA_TYPE = (byte)'M';
        public const byte CONTENT_TYPE = (byte)'C';

        public const int MaxRows = 2000;

        public const int MEGABYTE = 1024 * 1024;

        #region only for replicator
        public const string REPLICATOR_CONFLICT_FOLDER_NAME = "__ReplicationConflicts__";
        #endregion

        #region only for Connector
        public const string AVEFSDLFEATRUEID = "{4B4D59EA-D376-4f4a-B7CF-130A5EB26F50}";//content library 的list template的feature id
        public const string AVEVDLFEATRUEID = "{E7A14D5A-37E3-42e4-B5F2-1D774E3D37CB}";//media library 的list template的feature id
        #endregion

        #region only for archiver
        public const string ARCHIVE_BY = "ArchiveBy";
        public const string ARCHIVE_TIME = "ArchiveTime";
        #endregion

        #region for AveWebTemplate
        public const string WebTemplateBLOG = "BLOG";
        public const string WebTemplateMWS = "MPS";
        public const string WebTemplateSTS = "STS";
        public const string WebTemplateWIKI = "WIKI";
        /// <summary>
        /// 仅用作Office 365 OneDrive的template使用，该值可能会随着Office 365的更新而修改。
        /// Local站点的Personal Site以及OneDrive请另行定义常量。
        /// </summary>
        public const string WebTemplate_OneDrive_Office365 = "SPSPERS#10";

        public const string WebTemplate_Office365_GroupSite_Prefix = "Group";
        public const string WebTemplate_Office365_CommunicationSite_Prefix = "SITEPAGEPUBLISHING";
        #endregion
        static string tempFolder = null;
        public static string WrapperTempFolder
        {
            get
            {
                if (string.IsNullOrEmpty(tempFolder))
                {
                    tempFolder = Path.Combine(AveEnv.AgentTempFolder, Process.GetCurrentProcess().ProcessName);
                    try
                    {
                        if (!Directory.Exists(tempFolder))
                        {
                            Directory.CreateDirectory(tempFolder);
                        }
                    }
                    catch (Exception e)//Keep same logic with AveEnv.AgentTempFolder. We consider the direcotry already exists if created failed.
                    {
                        mLog.Warn("Create Directory:{0} failed:{1}", tempFolder, e);
                    }
                }
                return tempFolder;
            }
        }
    }
}
