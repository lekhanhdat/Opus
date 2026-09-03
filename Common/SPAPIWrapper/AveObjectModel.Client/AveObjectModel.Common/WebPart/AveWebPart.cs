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
using AvePoint.Wrapper.Common;
using System.Xml;
using AvePoint.GCommon.Contract.CodeReview;


namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_11, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]
    class AveWebPart : AveClientObject, IAveWebPart
    {
        private IAveRequest mRequest;
        private AveWebPartBaseInfo mBaseInfo;
        internal XmlDocument XDocForDefinitionXml;
        internal string WebPartNameSpace = string.Empty;
        internal XmlNamespaceManager WebPartNsmgr;
        internal string ViewNodeName = string.Empty;
        private AveWeb mWeb;

        public AveWebPart(IAveRequest request, AveWeb web, IDictionary<string, object> webpartProperties)
        {
            mRequest = request;
            mWeb = web;
            base.DataCache.AddPropertyies(webpartProperties);
            InitBaseInfo();
        }

        internal virtual AveWebPartBaseInfo BaseInfo
        {
            get
            {
                if (mBaseInfo == null)
                {
                    InitBaseInfo();
                }
                return mBaseInfo;
            }
            set
            {
                mBaseInfo = value;
            }
        }
        internal void InitBaseInfo()
        {
            mBaseInfo = new AveWebPartBaseInfo();            
            mBaseInfo.ID = new Guid(this.Id);
            mBaseInfo.ZoneID = this.ZoneID;
            mBaseInfo.IsIncluded = this.IsIncluded;
            mBaseInfo.DefinitionXml = base.DataCache.GetProperty<string>("DefinitionXml");
            mBaseInfo.WebPartIdProperty = base.DataCache.GetProperty<string>("WebPartIdProperty");
            //取ListID和PartOrder，还原时会用到，取不到时会返回default。
            mBaseInfo.ListId = base.DataCache.GetProperty<Guid>("ListId");
            mBaseInfo.PartOrder = base.DataCache.GetProperty<int>("PartOrder");
            XDocForDefinitionXml = new XmlDocument();
            XDocForDefinitionXml.LoadXml(mBaseInfo.DefinitionXml);
            if (XDocForDefinitionXml.OuterXml.Contains("http://schemas.microsoft.com/WebPart/v2"))
            {
                WebPartNameSpace = "http://schemas.microsoft.com/WebPart/v2";
                ViewNodeName = "ListViewXml";
            }
            else if (XDocForDefinitionXml.OuterXml.Contains("http://schemas.microsoft.com/WebPart/v3"))
            {
                WebPartNameSpace = "http://schemas.microsoft.com/WebPart/v3";
                ViewNodeName = "XmlDefinition";
            }
            IWebPartPropertyExtractor extractor = WebPartExtractorFactory.Create(mBaseInfo.DefinitionXml);
            if (extractor != null)
            {
                mBaseInfo.SolutionId = extractor.SolutionId;
            }
            WebPartNsmgr = new XmlNamespaceManager(XDocForDefinitionXml.NameTable);
            WebPartNsmgr.AddNamespace("WebPart", WebPartNameSpace);

        }
        public void Init()
        {
            GetWebPartTypeId();
        }
        #region IAveWebPart Members

        public string Id
        {
            get
            {
                return base.DataCache.GetProperty<string>("ID");
            }
        }

        public string Height
        {
            get
            {
                return base.DataCache.GetProperty<string>("Height");
            }
            set
            {
                base.DataCache.AddChangedProperty("Height", value);
            }
        }

        public string Width
        {
            get
            {
                return base.DataCache.GetProperty<string>("Width");
            }
            set
            {
                base.DataCache.AddChangedProperty("Width", value);
            }
        }

        public string TitleUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("TitleUrl");
            }
            set
            {
                base.DataCache.AddChangedProperty("TitleUrl", value);
            }
        }

        public bool Hidden
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Hidden");
            }
            set
            {
                base.DataCache.AddChangedProperty("Hidden", value);
            }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
            set
            {
                base.DataCache.AddChangedProperty("Title", value);
            }
        }

        public string ZoneID
        {
            get
            {
                return base.DataCache.GetProperty<string>("ZoneID");
            }
            set
            {
                base.DataCache.AddChangedProperty("ZoneID", value);
            }
        }

        public bool IsIncluded
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsIncluded");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsIncluded", value);
            }
        }

        public string ID
        {
            get
            {
                return base.DataCache.GetProperty<string>("ID");
            }
            set
            {
                base.DataCache.AddChangedProperty("ID", value);
            }
        }

        public string CatalogIconImageUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Subtitle
        {
            get { throw new NotImplementedException(); }
        }

        public string TitleIconImageUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string AuthorizationFilter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetWebPartProperty(string propertyName, object value)
        {
            throw new NotImplementedException();
        }

        public string WebPartTypeID
        {
            get { throw new NotImplementedException(); }
        }

        public int ZoneIndex
        {
            get { return base.DataCache.GetProperty<int>("ZoneIndex"); }
        }

        public string RealWebPartType
        {
            get { return base.DataCache.GetProperty<string>("RealWebPartType"); }
        }

        /// <summary>
        /// 获取V2版本webpart xml中的pagetype
        /// </summary>
        internal void GetPageTypeV2()
        {
            string pageType = string.Empty;
            pageType = GetPropertyValueStringForV2WebPart("PageType", "SpecialNameSpaceForWebpartV2", string.Empty);
            if (!pageType.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                mBaseInfo.Type = Convert.ToByte((Enum.Parse(typeof(AvePAGETYPE), pageType)));
            }
            else
            {
                mBaseInfo.Type = null;
            }
        }
        /// <summary>
        /// 获取V3版本webpart xml中的pagetype
        /// </summary>
        internal void GetPageTypeV3()
        {
            string pageType = string.Empty;
            pageType = GetPropertyValueStringForV3WebPart("PageType", string.Empty);
            if (!pageType.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                mBaseInfo.Type = Convert.ToByte((Enum.Parse(typeof(AvePAGETYPE), pageType)));
            }
            else
            {
                mBaseInfo.Type = null;
            }
        }
        /// <summary>
        /// 获取V2版本webpart xml中的View xml
        /// </summary>
        internal void GetViewV2()
        {
            string viewXml = GetPropertyValueStringForV2WebPart(ViewNodeName, "specialNameSpaceForWebpartV2", string.Empty);
            XmlDocument xViewDoc = new XmlDocument();
            xViewDoc.LoadXml(viewXml);
            string viewValue = xViewDoc.DocumentElement.InnerXml;
            BaseInfo.View = AveCompressedUtility.GetTCompressedBytes(viewValue);
        }
        /// <summary>
        /// 获取V3版本webpart xml中的View xml
        /// </summary>
        internal void GetViewV3()
        {
            string viewXml = GetPropertyValueStringForV3WebPart(ViewNodeName, string.Empty);
            XmlDocument xViewDoc = new XmlDocument();
            xViewDoc.LoadXml(viewXml);
            string viewValue = xViewDoc.DocumentElement.InnerXml;
            BaseInfo.View = AveCompressedUtility.GetTCompressedBytes(viewValue);
        }
        /// <summary>
        /// 获取V3版本webpart xml中的WebPartTitle
        /// </summary>
        /// <returns></returns>
        internal string GetWebPartTitleV3()
        {
            return GetPropertyValueStringForV3WebPart("Title", string.Empty);
        }
        /// <summary>
        /// 获取V2版本webpart xml中的WebPartContentTypeId
        /// </summary>
        internal void GetWebPartContentTypeIdV2()
        {
            string contentTypeId = string.Empty;
            contentTypeId = GetViewAttributeValueV2("ContentTypeID");
            if (!contentTypeId.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                mBaseInfo.ContentTypeId = ConvertIdStringToBytes(contentTypeId);
            }
        }
        /// <summary>
        /// 获取V3版本webpart xml中的WebPartContentTypeId
        /// </summary>
        internal void GetWebPartContentTypeIdV3()
        {
            string contentTypeId = string.Empty;
            contentTypeId = GetViewAttributeValueV3("ContentTypeID");
            if (!contentTypeId.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                mBaseInfo.ContentTypeId = ConvertIdStringToBytes(contentTypeId);
            }
        }
        /// <summary>
        /// 获取V2版本webpart xml中的ViewFlag
        /// </summary>
        internal void GetViewFlagV2()
        {
            string viewFlag = string.Empty;
            viewFlag = GetPropertyValueStringForV2WebPart("ViewFlag", "SpecialNameSpaceForWebpartV2", string.Empty);
            if (!viewFlag.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                mBaseInfo.Flags = Convert.ToInt32(viewFlag);
            }
        }
        /// <summary>
        /// 获取V3版本webpart xml中的ViewFlag 
        /// </summary>
        internal void GetViewFlagV3()
        {
            string viewFlag = string.Empty;
            viewFlag = GetPropertyValueStringForV3WebPart("ViewFlag", string.Empty);
            if (!viewFlag.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                mBaseInfo.Flags = Convert.ToInt32(viewFlag);
            }
            else
            {
                viewFlag = GetPropertyValueStringForV3WebPart("ViewFlags", string.Empty);
                if (!string.IsNullOrEmpty(viewFlag))
                {
                    string[] flags = viewFlag.Split(',');
                    int resultFlag = 0;
                    foreach (string flag in flags)
                    {
                        if (!string.IsNullOrEmpty(flag))
                        {
                            int temp = Convert.ToInt32(Enum.Parse(typeof(AveViewFlags), flag));
                            resultFlag |= temp;
                        }
                    }
                    mBaseInfo.Flags = resultFlag;
                }
            }
        }
        /// <summary>
        /// 获取V2webpart的BaseViewID
        /// </summary>
        internal void GetBaseViewIDV2()
        {
            mBaseInfo.BaseViewID = Convert.ToByte(GetViewAttributeValueV2("BaseViewID"));
        }
        /// <summary>
        /// 获取V3webpart的BaseViewID
        /// </summary>
        internal void GetBaseViewIDV3()
        {
            mBaseInfo.BaseViewID = Convert.ToByte(GetViewAttributeValueV3("BaseViewID"));
        }
        /// <summary>
        /// 获取V2webpart的DisplayName
        /// </summary>
        internal void GetDisplayNameV2()
        {
            mBaseInfo.DisplayName = string.Empty;
            mBaseInfo.DisplayName = GetViewAttributeValueV2("DisplayName");
        }
        /// <summary>
        /// 获取V3webpart的DisplayName
        /// </summary>
        internal void GetDisplayNameV3()
        {
            mBaseInfo.DisplayName = string.Empty;
            mBaseInfo.DisplayName = GetViewAttributeValueV3("DisplayName");
        }
        /// <summary>
        /// 获取V2webpart的level
        /// </summary>
        internal void GetLevelV2()
        {
            byte level = 0;
            level = Convert.ToByte(GetViewAttributeValueV2("Level"));
            mBaseInfo.Level = level;
        }
        /// <summary>
        /// 获取V3webpart的level
        /// </summary>
        internal void GetLevelV3()
        {
            byte level = 0;
            level = Convert.ToByte(GetViewAttributeValueV3("Level"));
            mBaseInfo.Level = level;
        }
        /// <summary>
        /// 获取V2webpart的WebPartIdProperty
        /// </summary>
        internal void GetWebPartIdPropertyV2()
        {
            XmlNode xNode = XDocForDefinitionXml.SelectSingleNode("WebPart:WebPart/WebPart:ID", WebPartNsmgr);
            if (xNode != null)
            {
                mBaseInfo.WebPartIdProperty = xNode.InnerText.StartsWith("g_", StringComparison.OrdinalIgnoreCase) ? xNode.InnerText : "g_" + xNode.InnerText.ToLower().Replace("-", "_");
            }
        }
        /// <summary>
        /// 获取V3webpart的WebPartIdProperty
        /// </summary>
        internal void GetWebPartIdPropertyV3()
        {
            XmlNode xNode = XDocForDefinitionXml.SelectSingleNode("//WebPartIdProperty", WebPartNsmgr);
            if (xNode != null)
            {
                mBaseInfo.WebPartIdProperty = xNode.InnerText.StartsWith("g_", StringComparison.OrdinalIgnoreCase) ? xNode.InnerText : "g_" + xNode.InnerText.ToLower().Replace("-", "_");
            }
        }
        /// <summary>
        /// 获取V2webpart的CatalogIconImageUrl
        /// </summary>
        /// <returns></returns>
        internal string GetCatalogIconImageUrlV2()
        {
            string catalogIconImageUrl = string.Empty;
            catalogIconImageUrl = GetPropertyValueStringForV2WebPart("CatalogIconImageUrl", string.Empty, string.Empty);
            return catalogIconImageUrl;
        }
        /// <summary>
        /// 获取V3webpart的CatalogIconImageUrl
        /// </summary>
        /// <returns></returns>
        internal string GetCatalogIconImageUrlV3()
        {
            string catalogIconImageUrl = string.Empty;
            catalogIconImageUrl = GetPropertyValueStringForV3WebPart("CatalogIconImageUrl", string.Empty);
            return catalogIconImageUrl;
        }
        /// <summary>
        /// 获取V2webpart的listname
        /// </summary>
        /// <returns></returns>
        internal string GetListNameV2()
        {
            string listName = string.Empty;
            listName = GetPropertyValueStringForV2WebPart("ListName", "specialNameSpaceForWebpartV2", string.Empty);
            return listName;
        }
        /// <summary>
        /// 获取V3webpart的listname
        /// </summary>
        /// <returns></returns>
        internal string GetListNameV3()
        {
            string listName = string.Empty;
            listName = GetPropertyValueStringForV3WebPart("ListName", string.Empty);
            return listName;
        }

        internal string GetListIdV2()
        {
            string listName = string.Empty;
            listName = GetPropertyValueStringForV2WebPart("ListName", string.Empty, string.Empty);
            if (IsValidGuid(listName))
            {
                return listName;
            }
            listName = GetPropertyValueStringForV2WebPart("ListId", string.Empty, string.Empty);
            if (IsValidGuid(listName))
            {
                return listName;
            }
            listName = GetPropertyValueStringForV2WebPart("ListGuid", string.Empty, string.Empty);
            if (IsValidGuid(listName))
            {
                return listName;
            }
            return listName;
        }

        internal string GetListIdV3()
        {
            string listName = string.Empty;
            listName = GetPropertyValueStringForV3WebPart("ListName", string.Empty);
            if (IsValidGuid(listName))
            {
                return listName;
            }
            listName = GetPropertyValueStringForV3WebPart("ListId", string.Empty);
            if (IsValidGuid(listName))
            {
                return listName;
            }
            listName = GetPropertyValueStringForV3WebPart("ListGuid", string.Empty);
            if (IsValidGuid(listName))
            {
                return listName;
            }
            return listName;            
        }

        private bool IsValidGuid(string listid)
        {
            return AveTypeHelper.IsGuid(listid) && new Guid(listid) != Guid.Empty;
        }

        /// <summary>
        /// 获取V3webpart的XmlDefinition
        /// </summary>
        /// <returns></returns>
        internal string GetXmlDefinitionV3()
        {
            string xmlDefinition = string.Empty;
            xmlDefinition = string.Format("<View BaseViewID='{0}'/>", Convert.ToString(mBaseInfo.BaseViewID));
            return xmlDefinition;
        }
        /// <summary>
        /// 获取V2webpart的InitialAsyncDataFetch
        /// </summary>
        /// <returns></returns>
        internal bool GetInitialAsyncDataFetchV2()
        {
            bool initialAsyncDataFetch = false;
            initialAsyncDataFetch = bool.Parse(GetPropertyValueStringForV2WebPart("InitialAsyncDataFetch", string.Empty, string.Empty));
            return false;
        }
        /// <summary>
        /// 获取V3webpart的InitialAsyncDataFetch
        /// </summary>
        /// <returns></returns>
        internal bool GetInitialAsyncDataFetchV3()
        {
            bool initialAsyncDataFetch = false;
            initialAsyncDataFetch = bool.Parse(GetPropertyValueStringForV3WebPart("InitialAsyncDataFetch", string.Empty));
            return false;
        }
        /// <summary>
        /// 模拟local的webparttypeid
        /// </summary>
        internal void GetWebPartTypeId()
        {
            //string webPartAssemblyAndType = string.Empty;
            string listId = null;
            if (WebPartNameSpace.Equals("http://schemas.microsoft.com/WebPart/v2", StringComparison.OrdinalIgnoreCase))
            {
                //webPartAssemblyAndType = GetPropertyValueStringForV2WebPart("Assembly", string.Empty, string.Empty) + "|" + GetPropertyValueStringForV2WebPart("TypeName", string.Empty, string.Empty);
                listId = GetListIdV2();
            }
            else if (WebPartNameSpace.Equals("http://schemas.microsoft.com/WebPart/v3", StringComparison.OrdinalIgnoreCase))
            {
                listId = GetListIdV3();
            }
            if (string.IsNullOrEmpty(mBaseInfo.ListTitle) && AveTypeHelper.IsGuid(listId))
            {
                IAveList list = this.mWeb.Lists.GetListById(new Guid(listId), false);
                if (list != null)
                {
                    mBaseInfo.ListTitle = list.Title;
                }
            }
            mBaseInfo.WebPartTypeId = Guid.NewGuid();
        }
        /// <summary>
        /// 模拟local的tp_view
        /// </summary>
        /// <param name="webPartContenttypeId"></param>
        /// <returns></returns>
        private byte[] ConvertIdStringToBytes(string webPartContenttypeId)
        {
            byte[] idBytes = null;
            if (webPartContenttypeId.Length > 2 && webPartContenttypeId.Length % 2 == 0 && webPartContenttypeId.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                idBytes = new byte[(webPartContenttypeId.Length - 2) / 2];
                for (int i = 0; i < idBytes.Length; i++)
                {
                    string idByte = webPartContenttypeId.Substring(2 + 2 * i, 2);
                    idBytes[i] = Convert.ToByte(idByte, 16);
                }
            }
            else
            {
                idBytes = new byte[0];
            }
            return idBytes;
        }
        /// <summary>
        /// 模拟local的webparttypeid，获取计算后的guid
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        /// <summary>
        /// 创建需要local的webpartlist中的数据
        /// </summary>
        /// <param name="webUrl"></param>
        /// <param name="webId"></param>
        /// <param name="userId"></param>
        internal void CreateWebPartList(string webUrl, Guid webId, Nullable<int> userId)
        {
            if (mBaseInfo.WebPartList == null)
            {
                mBaseInfo.WebPartList = new List<AveWebPartListInfo>();
            }
            AveWebPartListInfo webPartListInfo = new AveWebPartListInfo();
            webPartListInfo.FullUrl = webUrl;
            webPartListInfo.Level = mBaseInfo.Level;
            webPartListInfo.UserID = userId;
            webPartListInfo.WebId = webId;
            mBaseInfo.WebPartList.Add(webPartListInfo);
        }


        /// <summary>
        /// 获取v3版本viewxml里的属性值
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        private string GetViewAttributeValueV3(string attributeName)
        {
            string propertyValue = string.Empty;
            XmlDocument xDocForView = new XmlDocument();
            string viewXml = GetPropertyValueStringForV3WebPart(ViewNodeName, string.Empty);
            if (!viewXml.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                xDocForView.LoadXml(viewXml);
                propertyValue = xDocForView.DocumentElement.Attributes[attributeName].Value;
            }
            return propertyValue;
        }
        /// <summary>
        /// 获取v2版本viewxml里的属性值
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        private string GetViewAttributeValueV2(string attributeName)
        {
            string propertyValue = string.Empty;
            XmlDocument xDocForView = new XmlDocument();
            string viewXml = GetPropertyValueStringForV2WebPart(ViewNodeName, "SpecialNameSpaceForWebpartV2", string.Empty);
            if (!viewXml.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                xDocForView.LoadXml(viewXml);
                propertyValue = xDocForView.DocumentElement.Attributes[attributeName].Value;
            }
            return propertyValue;
        }
        /// <summary>
        ///  获取v3版本的webpartxml中node的innertext或者特定属性的值,node name为property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        internal string GetPropertyValueStringForV3WebPart(string propertyName, string attributeName)
        {
            string propertyValue = string.Empty;
            XmlNode xNode = XDocForDefinitionXml.SelectSingleNode(string.Format("//WebPart:property[@name='{0}']", propertyName), WebPartNsmgr);
            if (xNode != null)
            {
                if (attributeName.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    propertyValue = xNode.InnerText;
                }
                else
                {
                    propertyValue = xNode.Attributes[attributeName].Value;
                }
            }
            return propertyValue;
        }
        /// <summary>
        /// 添加构造alluserproperty，preuserproperty的hashtable
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="webPartProperties"></param>
        internal void AddNotEmptyStringPropertyV3(string propertyName, Dictionary<string, object> webPartProperties)
        {
            string propertyValue = string.Empty;
            propertyValue = GetPropertyValueStringForV3WebPart(propertyName, string.Empty);
            if (!propertyValue.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                webPartProperties.Add(propertyName, propertyValue);
            }
        }
        /// <summary>
        /// 获取v3版本的webpartxml中node的innertext或者特定属性的值,node name不为property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="specialNameSpaceForWebpartV2"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        internal string GetSpecialValueStringForV3WebPart(string specialPropertyName, string attributeName)
        {
            string propertyValue = string.Empty;
            XmlNode xNode = XDocForDefinitionXml.SelectSingleNode(string.Format("//WebPart:{0}", specialPropertyName), WebPartNsmgr);
            if (xNode != null)
            {
                if (attributeName.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    propertyValue = xNode.InnerText;
                }
                else
                {
                    propertyValue = xNode.Attributes[attributeName].Value;
                }
            }
            return propertyValue;
        }
        /// <summary>
        /// 获取v2版本的webpartxml中node的innertext或者特定属性的值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="specialNameSpaceForWebpartV2"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        internal string GetPropertyValueStringForV2WebPart(string propertyName, string specialNameSpaceForWebpartV2, string attributeName)
        {
            string propertyValue = string.Empty;
            string path = string.Empty;
            if (!specialNameSpaceForWebpartV2.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                path = string.Format("WebPart:WebPart/SpecialNameSpaceForWebpartV2:{0}", propertyName);
            }
            else
            {
                path = string.Format("WebPart:WebPart/WebPart:{0}", propertyName);
            }
            XmlNode xNode = XDocForDefinitionXml.SelectSingleNode(path, WebPartNsmgr);
            if (xNode != null)
            {
                if (attributeName.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    propertyValue = xNode.InnerText;
                }
                else
                {
                    propertyValue = xNode.Attributes[attributeName].Value;
                }
            }
            return propertyValue;
        }
        internal virtual void SetAllUserPropertiesAndPerUserProperties()
        {

        }
        #endregion

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public string WebPartIdProperty
        {
            get
            {
                return base.DataCache.GetProperty<string>("WebPartIdProperty");
            }
        }
    }
}
