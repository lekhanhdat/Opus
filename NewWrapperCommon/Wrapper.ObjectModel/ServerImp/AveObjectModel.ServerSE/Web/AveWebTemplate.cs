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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWebTemplate : AveServerObject, IAveWebTemplate
    {
        private SPWebTemplate mWebTemplate;
        private const string mWebTemplateBLOG = "BLOG";
        private const string mWebTemplateMWS = "MPS";
        private const string mWebTemplateSTS = "STS";
        private const string mWebTemplateWIKI = "WIKI";

        public AveWebTemplate(SPWebTemplate webTemplate)
        {
            mWebTemplate = webTemplate;
        }

        /// <summary>
        /// Construct for calling static member;
        /// </summary>
        public AveWebTemplate()
        { }

        internal SPWebTemplate WebTemplate
        {
            get
            {
                return mWebTemplate;
            }
        }

        #region IAveWebTemplate Members

        public string Description
        {
            get { return mWebTemplate.Description; }
        }

        public string DisplayCategory
        {
            get { return mWebTemplate.DisplayCategory; }
        }

        public int ID
        {
            get { return mWebTemplate.ID; }
        }

        public string ImageUrl
        {
            get { return mWebTemplate.ImageUrl; }
        }

        public bool IsHidden
        {
            get { return mWebTemplate.IsHidden; }
        }

        public bool IsRootWebOnly
        {
            get { return mWebTemplate.IsRootWebOnly; }
        }

        public bool IsSubWebOnly
        {
            get { return mWebTemplate.IsSubWebOnly; }
        }

        public uint Lcid
        {
            get { return mWebTemplate.Lcid; }
        }

        public string Name
        {
            get { return mWebTemplate.Name; }
        }

        public string Title
        {
            get { return mWebTemplate.Title; }
        }

        public string WebTemplateBLOG
        {
            get { return mWebTemplateBLOG; }
        }

        public string WebTemplateMWS
        {
            get { return mWebTemplateMWS; }
        }

        public string WebTemplateSTS
        {
            get { return mWebTemplateSTS; }
        }

        public string WebTemplateWIKI
        {
            get { return mWebTemplateWIKI; }
        }

        public Guid VisibilityFeatureDependencyId
        {
            get { return mWebTemplate.VisibilityFeatureDependencyId; }
        }

        public bool SupportsMultilingualUI
        {
            get { return mWebTemplate.SupportsMultilingualUI; }
        }

        #endregion

        #region add for SP2013
        public int CompatibilityLevel
        {
            get { return mWebTemplate.CompatibilityLevel; }
        }
        #endregion
    }
}
