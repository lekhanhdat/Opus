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
    public class AveContentTypeInfo
    {
        public string Name;
        public string Id;
        public bool ReadOnly;
        public string Description;
        public string FieldsSchemaXml;
        public string DocumentTemplate;
        public string DisplayFormTemplateName;
        
        public string DisplayFormUrl;
        public string DocumentTemplateUrl;
        public string EditFormTemplateName;
        public string EditFormUrl;
        public string Group;
        public bool Hidden;
        public string NewDocumentControl;
        public string NewFormTemplateName;
        public string NewFormUrl;
        public bool RequireClientRenderingOnNew = true;
        public string ResourceFolder;  // ? 
        public string SchemaXml;
        public string Scope;
        public bool Sealed;
        public int Version;
        public string SolutionId;

        public List<string> XmlDocuments = new List<string>();

        public string ParentName;
        public AveContentTypeInfo ParentContentTypeInfo;
        public List<AveContentTypeFileInfo> ResourceFolderFiles = new List<AveContentTypeFileInfo>();
        public bool IsPublished;
        public bool IsUnPublished;
        public string MappingName = string.Empty;

        /// <summary>
        /// add in 3.17.1 NFR 
        /// </summary>
        public Dictionary<string,string> NameResourceInfo { get; set; }
        /// <summary>
        /// add in 3.17.1 NFR 
        /// </summary>
        public Dictionary<string, string> DescriptionResourceInfo { get; set; }
        /// <summary>
        /// 用于外围存放扩展字段, Wrapper内部不使用
        /// </summary>
        public string ExtensionXml;

        /// <summary>
        /// 当前仅备份Publish version
        /// </summary>
        public string NintexFormXml { get; set; }
    }

    public class AveContentTypeFileInfo
    {
        public AveContentTypeFileInfo(string url,byte[] fileBinary)
        {
            Url = url;
            FileBinary = fileBinary;
        }

        public AveContentTypeFileInfo() { }

        public string Url;
        public byte[] FileBinary;
    }

    public class AveContentTypeCollectionInfo
    {
        public List<AveContentTypeInfo> ContentTypes = new List<AveContentTypeInfo>();
        public AveSiteInfo SourceSiteInfo;
    }

    public enum ContentTypeScope
    {
        Web,
        List
    }
}
