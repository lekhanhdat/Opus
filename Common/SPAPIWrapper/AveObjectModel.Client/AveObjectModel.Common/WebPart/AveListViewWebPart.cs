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
    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_6, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]
    class AveListViewWebPart : AveWebPart, IAveListViewWebPart
    {
        private const string LISTVIEWNAMESPACEV2 = "http://schemas.microsoft.com/WebPart/v2/ListView";
        public AveListViewWebPart(IAveRequest request, AveWeb web, IDictionary<string, object> webpartProperties)
            : base(request, web, webpartProperties)
        {
            base.WebPartNsmgr.AddNamespace("SpecialNameSpaceForWebpartV2", LISTVIEWNAMESPACEV2);
        }
        private Guid mWebId;
        internal virtual AveWebPartBaseInfo BaseInfo
        {
            get
            {
                AveWebPartBaseInfo baseInfo = base.BaseInfo;
                baseInfo.ListId = this.ListId;
                baseInfo.ListTitle = this.ListName;
                
                return baseInfo;
            }
        }

        #region IAveListViewWebPart Members

        public Guid ListId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("ListId");
            }
            set
            {
            }
        }

        public string ListName
        {
            get
            {
                return base.DataCache.GetProperty<string>("ListName");
            }
            set
            {
                base.DataCache.AddChangedProperty("ListName", value);
            }
        }

        public string ViewGuid
        {
            get
            {
                return base.DataCache.GetProperty<string>("ViewGuid");
            }
            set
            {
                base.DataCache.AddChangedProperty("ViewGuid", value);
            }
        }        

        public Guid WebId
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
        public void Init(string listTitle, Guid webId, string webRelaiveUrl)
        {
            mWebId = webId;
            base.Init();
            base.GetPageTypeV2();
            base.GetViewFlagV2();
            base.GetWebPartIdPropertyV2();
            base.GetWebPartContentTypeIdV2();
            base.GetBaseViewIDV2();
            base.GetDisplayNameV2();
            base.GetLevelV2();
            base.BaseInfo.ListTitle = listTitle;
            base.CreateWebPartList(webRelaiveUrl, webId, null);
            base.GetViewV2();
            SetAllUserPropertiesAndPerUserProperties();
        }
        private void SetAllUserPropertiesAndPerUserProperties()
        {
            
        }
        #endregion
    }
}
