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

namespace AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SyncConfigFileMessage
    {
        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public ConfilictOption ConfilictOption { get; set; }

        [DataMember]
        public List<string> AllSyncFileNames { get; set; }

        [DataMember]
        public List<string> DoesNotExistFiles { get; set; }

        /// <summary>
        /// 所有的需要同步的config文件的信息
        /// </summary>
        [DataMember]
        public List<ConfigFileInfo> AllSyncFiles { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigFileInfo
    {
        [DataMember]
        public String FileName { get; set; }

        [DataMember]
        public string fileContent { get; set; }
    }

    [DataContract]
    public enum ConfilictOption
    {
        [EnumMember]
        Merge,

        [EnumMember]
        Replace
    }
}
