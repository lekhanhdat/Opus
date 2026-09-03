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
using AvePoint.GCommon.Contract.CodeReview;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_12, CodeReviewConstants.CHECK_LIST_ID_CS_1 }, null, true)]
    class AveXsltListViewWebPart : AveWebPart, IAveXsltListViewWebPart
    {
        public AveXsltListViewWebPart(IAveRequest request, Dictionary<string, object> webpartProperties)
            : base(request, webpartProperties)
        {
        }
        private Guid mWebId;
        internal AveWebPartBaseInfo BaseInfo
        {
            get 
            {
                return base.BaseInfo;
            }
            set 
            {
                base.BaseInfo = value;
            }
        }
        #region IAveXsltListViewWebPart Members

        public string ListName
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

        public Guid WebId
        {
            get
            {
                return mWebId;
            }
            set
            {
                mWebId = value;
            }
        }

        public string ViewGuid
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

        #endregion

        #region IAveListWebPart Members

        public Guid ListId
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

        public AvePAGETYPE PageType
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

        public AveViewFlags ViewFlags
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

        public int ViewId
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

        #endregion

        public string DataSourcesString
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

        public string ParameterBindings
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
        /// <summary>
        /// 为webpartbaseinfo的必要属性赋值
        /// </summary>
        /// <param name="listTitle"></param>
        /// <param name="webId"></param>
        /// <param name="webRelaiveUrl"></param>
        public void Init(string listTitle, Guid webId, string webRelaiveUrl)
        {
            mWebId = webId;
            base.Init();
            base.GetPageTypeV3();
            base.GetWebPartIdPropertyV3();
            base.GetWebPartContentTypeIdV3();
            base.GetBaseViewIDV3();
            base.GetDisplayNameV3();
            base.GetLevelV3();
            base.BaseInfo.ListTitle = listTitle;
            base.CreateWebPartList(webRelaiveUrl, webId, null);
            base.GetViewV3();
            base.GetViewFlagV3();
            SetAllUserPropertiesAndPerUserProperties();
        }

        /// <summary>
        /// 模拟local数据库的AllUserPropertiesAndPerUserProperties
        /// </summary>
        private void SetAllUserPropertiesAndPerUserProperties()
        {
            SetAllUserPropertiesAndPerUserProperties(new List<string>() { "AllowClose", "ListName", 
                                                                    "XmlDefinition", "InitialAsyncDataFetch",
                                                                    "Title", "Description", "CatalogIconImageUrl",
                                                                    "TitleIconImageUrl", "Height", "Width",
                                                                    "HelpMode", "ViewFlags", "ViewFlag","ChromeType",
                                                                    "ClientRender","IsClientRender","GhostedXslLink" }, true);

            this.BaseInfo.DicAllUserPerUserPros.Add("WebId", mWebId);
            this.BaseInfo.DicAllUserPerUserPros.Add("ListId", BaseInfo.ListId);

        }
    }
}
