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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveListTemplate : IAveListTemplate
    {
        private SPListTemplate mListTemplate;

        public AveListTemplate(SPListTemplate spListTemplate)
        {
            mListTemplate = spListTemplate;
        }

        internal SPListTemplate ListTemplate
        {
            get
            {
                return mListTemplate;
            }
        }

        #region IAveListTemplate Members

        public AveBaseType BaseType
        {
            get
            {
                return (AveBaseType)mListTemplate.BaseType;
            }
        }

        public AveListTemplateType Type
        {
            get { return (AveListTemplateType)mListTemplate.Type; }
        }

        public bool AllowsFolderCreation
        {
            get { return mListTemplate.AllowsFolderCreation; }
        }

        public Guid FeatureId
        {
            get { return mListTemplate.FeatureId; }
        }

        public bool IsCustomTemplate
        {
            get { return mListTemplate.IsCustomTemplate; }
        }

        public string Name
        {
            get { return mListTemplate.Name; }
        }

        public string Description
        {
            get { return mListTemplate.Description; }
        }

        public string NewPage
        {
            get { return mListTemplate.NewPage; }
        }

        public AveListCategoryType CategoryType
        {
            get
            {
                return (AveListCategoryType)mListTemplate.CategoryType;
            }
        }

        public string InternalName
        {
            get
            {
                return mListTemplate.InternalName;
            }
        }

        public bool Hidden
        {
            get
            {
                return mListTemplate.Hidden;
            }
        }

        public bool Unique
        {
            get
            {
                return mListTemplate.Unique;
            }
        }

        public int Type_Client
        {
            get
            {
                return mListTemplate.Type_Client;
            }
        }

        #endregion
    }
}
