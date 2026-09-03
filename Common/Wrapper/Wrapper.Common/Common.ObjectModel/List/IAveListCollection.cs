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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public interface IAveListCollection : ICollection, IEnumerable<IAveList>, IEnumerable
    {
        Guid Add(string title, string description, IAveListTemplate template, string featureId = null);
        Guid Add(string title, string description, IAveListTemplate template, IAveDocTemplate documentTemplate);
        Guid Add(string title, string description, string url, IAveListDataSource dataSource);
        Guid Add(string title, string description, string url, string dataSource);
        Guid Add(string title, string description, string url, string featureId, int templateType, string docTemplateType);
        void Delete(Guid uniqueID);
        IAveList Add(AveListCreationInformation listCreationInfo);
        IAveList GetById(Guid uniqueId);
        IAveList GetByTitle(string strListName);
        IAveList GetList(Guid uniqueId, bool fetchMetadata);
        IAveList this[Guid id] { get; }
        IAveList this[string name] { get; }
        IAveList this[int index] { get; }
        IAveWeb Web { get; }

        Guid Add(string strTitle, string strDescription, AveListTemplateType templateType, string featureId = null);
        Guid Add(string title, string description, string url, string featureId, int templateType, string docTemplateType, AveQuickLaunchOptions quickLaunchOptions);
        XmlNode GetList(string listName);
        XmlNode GetListCollection();
        IAveList GetListByName(string strListName, bool bThrowException);
        IAveList GetListById(Guid uniqueId, bool bThrowException);
        IAveList TryGetList(string listTitle);
    }



    public class AveListCreationInformation
    {
        private string mcustomSchemaXml;
        private IDictionary<string, string> mdataSourceProperties = new Dictionary<string, string>();
        private string mdescription;
        private int mdocumentTemplateType;
        private AveQuickLaunchOptions mquickLaunchOption;
        private Guid mtemplateFeatureId;
        private int mtemplateType;
        private string mtitle;
        private string murl;

        public string CustomSchemaXml
        {
            get
            {
                return this.mcustomSchemaXml;
            }
            set
            {
                this.mcustomSchemaXml = value;
            }
        }

        public IDictionary<string, string> DataSourceProperties
        {
            get
            {
                return this.mdataSourceProperties;
            }
            set
            {
                this.mdataSourceProperties = value;
            }
        }

        public string Description
        {
            get
            {
                return this.mdescription;
            }
            set
            {
                this.mdescription = value;
            }
        }

        public int DocumentTemplateType
        {
            get
            {
                return this.mdocumentTemplateType;
            }
            set
            {
                this.mdocumentTemplateType = value;
            }
        }

        public AveQuickLaunchOptions QuickLaunchOption
        {
            get
            {
                return this.mquickLaunchOption;
            }
            set
            {
                this.mquickLaunchOption = value;
            }
        }

        public Guid TemplateFeatureId
        {
            get
            {
                return this.mtemplateFeatureId;
            }
            set
            {
                this.mtemplateFeatureId = value;
            }
        }

        public int TemplateType
        {
            get
            {
                return this.mtemplateType;
            }
            set
            {
                this.mtemplateType = value;
            }
        }

        public string Title
        {
            get
            {
                return this.mtitle;
            }
            set
            {
                this.mtitle = value;
            }
        }

        public string Url
        {
            get
            {
                return this.murl;
            }
            set
            {
                this.murl = value;
            }
        }
    }

    public enum AveQuickLaunchOptions
    {
        Off,
        On,
        DefaultValue
    }
}
