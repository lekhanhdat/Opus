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
using System.Xml.Serialization;

namespace AvePoint.Item.Restore
{
    [XmlRoot("Request")]
    public class AveXmlRequest
    {
        [XmlAttribute("type")] public int Type;

        [XmlElement("Message")] public AveXmlMessage Message;

        [XmlElement("Backup")] public AveXmlBackup Backup;

        [XmlElement("Restore")] public AveXmlRestore Restore;

        [XmlElement("CrossRestore")] public AveXmlRestore CrossRestore;
    }

    [XmlRootAttribute("Message")]
    public class AveXmlMessage
    {
        [XmlArray("Items")] [XmlArrayItem("Item")] public List<AveXmlItem> Items;

        [XmlAttribute("childNodeType")] public int ChildNodeType;

        [XmlElement("AgentInfo")] public AveXmlAgentInfo AgentInfo;
    }

    [XmlRootAttribute("Item")]
    public class AveXmlItem
    {
        [XmlAttribute("id")] public string Id;

        [XmlAttribute("type")] public int Type;

        [XmlAttribute("hasSubFolders")] public bool HasSubFolders;

        [XmlAttribute("startFile")] public int StartFile;

        [XmlAttribute("title")] public string Title;

        [XmlAttribute("displayName")] public string DisplayName;
    }

    [XmlRootAttribute("AgentInfo")]
    public class AveXmlAgentInfo
    {
        [XmlAttribute("default")] public string Default;

        [XmlAttribute("domain")] public string Domain;

        [XmlAttribute("user")] public string User;

        [XmlAttribute("password")] public string Password;

        [XmlAttribute("agentAddress")] public string Address;

        [XmlAttribute("port")] public int Port;
    }

    [XmlRootAttribute("Backup")]
    public class AveXmlBackup
    {
        [XmlAttribute("testRun")] public string TestRun;
        [XmlAttribute("dataMode")] public int DataMode;
        [XmlAttribute("planId")] public string PlanId;
        [XmlAttribute("jobId")] public string JobId;

        [XmlElement("AgentInfo")] public AveXmlAgentInfo AgentInfo;
        [XmlElement("MediaServerInfo")] public AveXmlMediaServerInfo MediaServerInfo;
        [XmlElement("PlanExtraInfo")] public AveXmlPlanExtraInfo PlanExtraInfo;
    }

    [XmlRootAttribute("Restore")]
    public class AveXmlRestore
    {
        [XmlAttribute("jobId")] public string JobId;

        [XmlAttribute("contentDBId")] public Guid ContentDBId;

        [XmlAttribute("option")] public string Option;

        [XmlAttribute("restoreContentsToSub")] public bool RestoreContentsToSub;

        [XmlAttribute("includeItemsJobReport")] public bool IncludeItemsJobReport;

        [XmlAttribute("includingRecycleBinData")] public int IncludingRecycleBinData;

        [XmlAttribute("srcLanguage")] public string SrcLanguage;

        [XmlAttribute("desLanguage")] public string DestLanguage;

        [XmlAttribute("replaceType")] public string ReplaceType;

        [XmlElement("AgentInfo")] public AveXmlAgentInfo AgentInfo;
        [XmlElement("MediaServerInfo")] public AveXmlMediaServerInfo MediaServerInfo;

        [XmlElement("SrcItems")] public List<AveXmlItems> SrcItemsList;

        [XmlElement("DstItems")] public AveXmlItems DestItems;
    }

    [XmlRootAttribute("MediaServerInfo")]
    public class AveXmlMediaServerInfo
    {
        [XmlAttribute("host")] public string Host;
        [XmlAttribute("dataPort")] public int DataPort;
        [XmlAttribute("controlPort")] public int ControlPort;
    }

    [XmlRootAttribute("PlanExtraInfo")]
    public class AveXmlPlanExtraInfo
    {
        [XmlAttribute("workflow")] public int WorkFlow;

        [XmlAttribute("lockSite")] public int LockSite;
    }

    [XmlRootAttribute("Browse")]
    public class AveXmlBrowse
    {
        [XmlAttribute("siteId")] public string SiteId;

        [XmlAttribute("parentId")] public string ParentId;

        [XmlAttribute("parentType")] public int ParentType;
    }

    public class AveXmlItems
    {
        [XmlAttribute("isFolder")] public bool IsFolder;

        [XmlElement("Item")] public List<AveXmlItem> Items;
    }
}
