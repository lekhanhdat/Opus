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


using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveWorkflowDefinitionCollection : AveAbstractCommonCollection<IAveWorkflowDefinition>, IAveWorkflowDefinitionCollection
    {
        private string mWorkfolwSource;
        private IAveWeb mWeb;
        public AveWorkflowDefinitionCollection(IAveWeb web, IAveList list, string workflowSource, Dictionary<string, object> prop)
        {
            mWeb = web;
            mWorkfolwSource = workflowSource;
            mListData = new List<IAveWorkflowDefinition>(prop.Count);
            base.DataCache.AddPropertyies(prop);
            InitWorkflowAssocCollection();
        }

        private void InitWorkflowAssocCollection()
        {
            foreach (var dic in base.DataCache.GetChildren())
            {
                AveWorkflowDefinition workflowDefinition = new AveWorkflowDefinition(mWeb, mWorkfolwSource, dic);
                mListData.Add(workflowDefinition);
            }
        }
    }
}
