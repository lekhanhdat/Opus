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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AgentConfigFileDto
    {
        [DataMember]
        public List<string> AgentIds { get; set; }
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public List<string> ConfigFileNames { get; set; }
        [DataMember]
        public List<ConfigFile> ConfigFiles { get; set; }
        [DataMember]
        public FileConfilictOption ConfilictOption { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigFile
    {
        [DataMember]
        public String FileName { get; set; }

        [DataMember]
        public string fileContent { get; set; }
    }
    [DataContract]
    public enum FileConfilictOption
    {
        [EnumMember]
        Merge,
        [EnumMember]
        Replace
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConfigFileResult
    {
        [DataMember]
        public ConfigFileResultType resultType { get; set; }
        [DataMember]
        public List<string> message { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConfigFileResultType
    {
        [EnumMember]
        Successs = 0,
        [EnumMember]
        Error = 1,
        [EnumMember]
        DoesNotExistFiles = 2
    }
}
