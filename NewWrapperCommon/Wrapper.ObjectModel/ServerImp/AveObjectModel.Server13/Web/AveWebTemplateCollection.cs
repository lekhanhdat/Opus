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

namespace AvePoint.ObjectModel.Server13
{
    class AveWebTemplateCollection : AveAbstractCommonCollection<IAveWebTemplate>, IAveWebTemplateCollection
    {
        private SPWebTemplateCollection mWebTemplates;
        private const string mWebTemplateCollection_Type = "Microsoft.SharePoint.SPWebTemplateCollection";

        public AveWebTemplateCollection(SPWebTemplateCollection webTemplates)
            : base(webTemplates)
        {
            mWebTemplates = webTemplates;
        }

        public AveWebTemplateCollection(string xmlWebTemplates, uint LCID)
            : this((SPWebTemplateCollection)AveAssemblyUtility.CreateInstance(mWebTemplateCollection_Type, new Type[] { typeof(string), typeof(uint) }, new object[] { xmlWebTemplates, LCID }))
        { }

        internal SPWebTemplateCollection WebTemplates
        {
            get
            {
                return mWebTemplates;
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveWebTemplate(t as SPWebTemplate);
        }

        #region IAveWebTemplateCollection Members

        public IAveWebTemplate this[string name]
        {
            get
            {
                return new AveWebTemplate(mWebTemplates[name]);
            }
        }

        public override IAveWebTemplate this[int index]
        {
            get
            {
                return new AveWebTemplate(mWebTemplates[index]);
            }
        }

        public override int Count
        {
            get { return mWebTemplates.Count; }
        }

        public IAveWebTemplate GetWebTemplateByIdConfiguration(int templateId, int config)
        {
            SPWebTemplate webTemplate = (SPWebTemplate)AveAssemblyUtility.InvokeMethod(mWebTemplates, "GetWebTemplateByIdConfiguration", new Type[] { typeof(int), typeof(int) }, new object[] { templateId, config });
            if (webTemplate != null)
            {
                return new AveWebTemplate(webTemplate);
            }
            return null;
        }

        #endregion
    }
}
