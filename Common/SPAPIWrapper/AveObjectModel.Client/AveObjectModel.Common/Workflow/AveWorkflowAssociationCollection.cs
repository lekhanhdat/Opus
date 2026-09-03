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
    class AveWorkflowAssociationCollection : AveAbstractCommonCollection<IAveWorkflowAssociation>, IAveWorkflowAssociationCollection
    {
        private string mWorkfolwSource;
        private IAveWeb mWeb = null;
        private IAveList mList = null;
        private IAveRequest mRequest;

        public AveWorkflowAssociationCollection(IAveWeb web, IAveList list, string workflowSource, Dictionary<string, object> prop)
        {
            mWeb = web;
            mList = list;
            mWorkfolwSource = workflowSource;
            mRequest = ((AveSite)(mWeb.Site)).Request;
            mListData = new List<IAveWorkflowAssociation>(prop.Count);
            base.DataCache.AddPropertyies(prop);
            InitWorkflowAssocCol();
        }

        private void InitWorkflowAssocCol()
        {
            foreach (var dic in base.DataCache.GetChildren())
            {
                AveWorkflowAssociation workflow = new AveWorkflowAssociation(mWeb, mList, this.mWorkfolwSource, dic);
                mListData.Add(workflow);
            }
        }

        public void Add(AveWorkflowAssociation w)
        {
            mListData.Add(w);
        }

        #region IAveWorkflowAssociationCollection Members

        public IAveWorkflowAssociation this[Guid workflowAssociationId]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveWorkflowAssociation w)
                    {
                        return w.ID.Equals(workflowAssociationId);
                    });
            }
        }

        public void Remove(IAveWorkflowAssociation association)
        {
            mRequest.DeleteWorkflowAssociation(association, mWorkfolwSource);
            mListData.Remove(association);
            //update?
        }

        public void Update(IAveWorkflowAssociation workflowAssociation)
        {
            throw new NotImplementedException();
        }

        public IAveWorkflowAssociation GetAssociationByBaseID(Guid baseTemplateId)
        {
            throw new NotImplementedException();
        }

        public IAveWorkflowAssociation GetAssociationByBaseID(Guid baseTemplateId, bool ignoreStartSettings)
        {
            throw new NotImplementedException();
        }


        public IAveWorkflowAssociation GetAssociationByName(string name, System.Globalization.CultureInfo cultureInfo)
        {
            return mListData.Find(
                delegate(IAveWorkflowAssociation wfAsso)
                {
                    return name.Equals(wfAsso.Name);
                });
            //throw new NotImplementedException();
        }

        #endregion




        public IAveWorkflowAssociation Add(IAveWorkflowAssociation workflowAssociation)
        {
            //throw new NotImplementedException();
            Dictionary<string, object> props = mRequest.CreateWebAssociation(mWeb.ServerRelativeUrl, mWeb.ID, "web.workflowTemplates", workflowAssociation);
            return new AveWorkflowAssociation(mWeb, null, string.Empty, props);
        }

    }
}