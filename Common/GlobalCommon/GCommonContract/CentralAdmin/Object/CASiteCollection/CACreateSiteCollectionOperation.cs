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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CACreateSiteCollectionOperation : CAOperation
    {
        [DataMember]
        [XmlAttribute("title")]
        public string Title { get; set; }

        [DataMember]
        [XmlAttribute("description")]
        public string Description { get; set; }

        [DataMember]
        [XmlAttribute("url")]
        public string Url { get; set; }

        [DataMember]
        [XmlAttribute("primary")]
        public UserDetail Primary { get; set; }

        [DataMember]
        [XmlAttribute("secondary")]
        public UserDetail Secondary { get; set; }

        [DataMember]
        [XmlAttribute("siteLanguage")]
        public string SiteLanguage { get; set; }

        [DataMember]
        public List<string> SiteLanguages { get; set; }

        [DataMember]
        [XmlAttribute("siteTemplate")]
        public string SiteTemplate { get; set; }

        [DataMember]
        public List<ContentDatabase> ContentDatabases { get; set; }

        [DataMember]
        [XmlElement("contentDatabase")]
        public ContentDatabase ContentDatabase { get; set; }

        [DataMember]
        [XmlElement("quotaTemplate")]
        public SiteCollectionQuota QuotaTemplate { get; set; }

        [DataMember]
        public List<SiteTemplate> WebTemplates { get; set; }

        [DataMember]
        public List<SiteCollectionQuota> Quotas { get; set; }

        [DataMember]
        public List<Template> WebAppCommonTemplateKinds { get; set; }

        [DataMember]
        public List<SiteCollectionQuota> WebAppCommonQuotas { get; set; }

        [DataMember]
        public List<string> WebAppCommonLanguages { get; set; }

        [DataMember]
        public List<ManagedPath> ManagedPaths { get; set; }

        [DataMember]
        [XmlElement]
        public ManagedPath ManagedPath { get; set; }

        [DataMember]
        [XmlAttribute("siteUrlName")]
        public string SiteUrlName { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteTemplate
    {
        [DataMember]
        public string Language { get; set; }

        [DataMember]
        public List<Template> Templates { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SubTemplate
    {
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Description { get; set; }
       
        public override bool Equals(object obj)
        {
            SubTemplate subTemplate = obj as SubTemplate;
            if (this.DisplayName.Equals(subTemplate.DisplayName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.DisplayName.GetHashCode();
        }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Template
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<SubTemplate> SubTemplates { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public string TemplateDescription { get; set; }

        /// <summary>
        /// 前台Merge数据时，需要重写自定义类的Equals方法，add by Zhang Hailong
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Template template = obj as Template;
            if (this.Name.Equals(template.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    public class WebAppInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<string> Languages { get; set; }
        [DataMember]
        public List<ManagedPath> ManagedPaths { get; set; }
        [DataMember]
        public List<Template> WebAppTemplateKinds { get; set; }
        [DataMember]
        public List<SiteCollectionQuota> Quotas { get; set; }
    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    [XmlRoot]
    public class ManagedPath
    {
        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }
        [DataMember]
        [XmlAttribute("prefixType")]
        public int PrefixType { get; set; }

        public override bool Equals(object obj)
        {
            ManagedPath path = obj as ManagedPath;
            if (path != null && this.Name.Equals(path.Name) && this.PrefixType == path.PrefixType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    public class PrefixType
    {
        public const int Explicit = 0;
        public const int ExplicitInclusion = 0;
        public const int Wildcard = 1;
        public const int WildcardInclusion = 1;
    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    [XmlRoot]
    public class SiteCollectionQuota
    {
        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }
        [DataMember]
        [XmlAttribute("storageLimit")]
        public long StorageLimit { get; set; }
        [DataMember]
        [XmlAttribute("userNumber")]
        public int UserNumber { get; set; }

        public override bool Equals(object obj)
        {
            SiteCollectionQuota Quota= obj as SiteCollectionQuota;
            if (Quota != null && this.Name.Equals(Quota.Name) && this.StorageLimit == Quota.StorageLimit && this.UserNumber == Quota.UserNumber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    [XmlRoot]
    public class ContentDatabase
    {
        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }
        [DataMember]
        [XmlAttribute("id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            ContentDatabase DataBase=obj as ContentDatabase;
            if (DataBase != null && this.Name.Equals(DataBase.Name) && this.Id.Equals(DataBase.Id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

}
