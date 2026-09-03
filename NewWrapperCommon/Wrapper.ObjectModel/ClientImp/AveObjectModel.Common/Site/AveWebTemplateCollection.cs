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


namespace AvePoint.ObjectModel.Common
{
    class AveWebTemplateCollection : AveAbstractCommonCollection<IAveWebTemplate>, IAveWebTemplateCollection
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private IAveRequest mRequest;

        public AveWebTemplateCollection(AveSite site, IAveRequest request, Dictionary<string, object> webTemplateColProperties)
        {
            mSite = site;
            mRequest = request;
            base.DataCache.AddPropertyies(webTemplateColProperties);
            InitWebTemplateCollection();
        }

        public AveWebTemplateCollection(AveWeb web, IAveRequest request, Dictionary<string, object> webTemplateColProperties)
        {
            mWeb = web;
            mRequest = request;
            base.DataCache.AddPropertyies(webTemplateColProperties);
            InitWebTemplateCollection();
        }
        internal void InitWebTemplateCollection()
        {
            List<Dictionary<string, object>> webTemplateList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveWebTemplate>();
            foreach (Dictionary<string, object> webTemplateProperties in webTemplateList)
            {
                AveWebTemplate webTemplate = new AveWebTemplate(webTemplateProperties);
                mListData.Add(webTemplate);
            }
        }

        public IAveWebTemplate this[string name]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveWebTemplate template)
                    {
                        return template.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                               template.Name.Equals(name + "#0", StringComparison.OrdinalIgnoreCase);
                    });
            }
        }

        public IAveWebTemplate GetWebTemplateByIdConfiguration(int templateId, int config)
        {
            //return null;
            return mListData.Find(
                   delegate(IAveWebTemplate template)
                   {
                       return template.ID.Equals(templateId) && 
                              template.Name.EndsWith("#" + config, StringComparison.OrdinalIgnoreCase);
                   });
        }
    }
}
