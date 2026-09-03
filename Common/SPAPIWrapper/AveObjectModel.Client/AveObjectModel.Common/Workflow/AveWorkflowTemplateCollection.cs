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
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common.Workflow
{
    class AveWorkflowTemplateCollection : AveAbstractCommonCollection<IAveWorkflowTemplate>, IAveWorkflowTemplateCollection
    {
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private AveList mParentList;

        public AveWorkflowTemplateCollection(IAveRequest request, IAveWeb parentWeb, IAveList parentList, string contentTypeSource, IDictionary<string, object> workflowTemplatesPro)
        {
            mRequest = request;
            mParentWeb = parentWeb as AveWeb;
            mParentList = parentList as AveList;
          //  mContentTypeSource = contentTypeSource;
            base.DataCache.AddPropertyies(workflowTemplatesPro);
           InitWorkflowTemplates();
        }

        private void InitWorkflowTemplates()
        {
            mListData = new List<IAveWorkflowTemplate>();
            foreach (var properties in base.DataCache.GetChildren())
            {
                AveWorkflowTemplate template = new AveWorkflowTemplate(properties);
                this.mListData.Add(template);
            }
        }

        public IAveWorkflowTemplate this[int index] 
        {
            get
            {
                return mListData[index];
            }
        }

        public IAveWorkflowTemplate this[Guid guid] 
        {
            get
            {
                return mListData.Find(
                    delegate(IAveWorkflowTemplate workflowTemplate)
                    {
                        return workflowTemplate.ID.Equals(guid);
                    });
            }
        }

        public void Add(IAveWorkflowTemplate template) 
        {
            throw new NotImplementedException();
        }

        public IAveWorkflowTemplate GetTemplateByBaseID(Guid baseTemplateId)
        {
            return mListData.Find(
                delegate(IAveWorkflowTemplate workflowTemplate)
                {
                    return workflowTemplate.ID.Equals(baseTemplateId);
                });
        }
    }
}
