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
using System.Xml.Serialization;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    /*
     
-<Request type="100"> //lock : archiver job
     * <WebPartRequest type="5"> //5:end user archiver backup job
         * <VaultClient domainUser="SHAREPOINT\system" agentType="8589938688" agentPort="10103" agentName="ENF43CAX64" agentAddress="10.1.20.184"/>

     *     <Metadata value=""/>
         * <WebPartTreeNode type="2" name="http://enf43cax64:9871/">
             * <WebPartTreeNode type="100" name="http://enf43cax64:9871/sites/archiver-1">
                 * <WebPartTreeNode type="200" name=".">
                     * <WebPartTreeNode type="301" name="Shared Documents"/>
    *             </WebPartTreeNode>
    *         </WebPartTreeNode>
    *     </WebPartTreeNode>
         *<CurrentLevel type="D/I" guid="e3929344-60ca-4066-9700-05ff0f2cdffc"/>
     **   or: <CurrentLevel type="F" guid="e3929344-60ca-4066-9700-05ff0f2cdffc"/>
     *</WebPartRequest>
 </Request>
     */
    [XmlRoot("SOEndUserBackupRequest")]
    public class SORelativeDataArchiveBackupRequest
    {
        [XmlElement("webAppId")]
        public string WebAppId = string.Empty;
        [XmlElement("webAppUrl")]
        public string WebAppUrl = string.Empty;
        [XmlElement("siteCollectionId")]
        public string SiteCollectionId = string.Empty;
        [XmlElement("siteCollectionUrl")]
        public string SiteCollectionUrl = string.Empty;
        [XmlElement("webId")]
        public string WebId = string.Empty;
        [XmlElement("listId")]
        public string ListId = string.Empty;
        [XmlElement("folderId")]
        public string FolderId = string.Empty;
        [XmlElement("itemId")]
        public string ItemId = string.Empty;
        [XmlElement("path")]
        public string Path = string.Empty;
        [XmlElement("docLibRowId")]
        public int DocLibRowId = int.MinValue;
        [XmlElement("leafName")]
        public string LeafName = string.Empty;
        [XmlElement("currentLevel")]
        public string CurrentLevel = string.Empty;
        [XmlElement("parentFolderIsRootFolder")]
        public bool ParentFolderIsRootFolder = false;
        [XmlElement("tagInfo")]
        public List<TagInfoCollection> TagInfo = new List<TagInfoCollection>();
        [XmlElement("includeIds")]
        public List<int> IncludeIds = new List<int>();
        [XmlElement("itemLastModifiedTime")]
        public DateTime ItemLastModifiedTime = DateTime.MinValue;
        //Add for Delete Related Document Feature
        [XmlElement("webServerRelatedUrl")]
        public string WebServerRelatedUrl = string.Empty;
        [XmlElement("listUrl")]
        public string ListUrl = string.Empty;
        [XmlElement("folderUrl")]
        public string FolderUrl = string.Empty;
    }
}
