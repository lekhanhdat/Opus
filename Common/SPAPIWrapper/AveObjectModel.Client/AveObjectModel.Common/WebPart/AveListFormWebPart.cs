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
    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_11, CodeReviewConstants.CHECK_LIST_ID_CS_1 }, null, true)]
    class AveListFormWebPart : AveWebPart, IAveListFormWebPart
    {
        private const string LISTFORMNAMESPACEV2 = "http://schemas.microsoft.com/WebPart/v2/ListForm";
        public AveListFormWebPart(IAveRequest request, AveWeb web, IDictionary<string, object> webpartProperties)
            : base(request, web, webpartProperties)
        {
            base.WebPartNsmgr.AddNamespace("SpecialNameSpaceForWebpartV2", LISTFORMNAMESPACEV2);
        }
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
                throw new NotImplementedException();;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public void Init(Guid webId, string webRelaiveUrl) 
        {
            base.Init();
            base.GetPageTypeV2();
            base.GetViewFlagV2();
            base.GetWebPartIdPropertyV2();
            base.GetWebPartContentTypeIdV2();
            base.BaseInfo.ListTitle = GetListNameV2();
            CreateWebPartList(webRelaiveUrl, webId, null);
        }
        
    }
}