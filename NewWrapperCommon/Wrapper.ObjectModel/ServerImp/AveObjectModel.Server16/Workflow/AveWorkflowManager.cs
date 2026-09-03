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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;

namespace AvePoint.ObjectModel.Server16
{
    class AveWorkflowManager : IAveWorkflowManager
    {
        private readonly string mWorkflowManager_Type = "Microsoft.SharePoint.Workflow.SPWorkflowManager";
        private readonly string workflowManagerGlobal_Type = "Microsoft.SharePoint.SPGlobal";
        private SPWorkflowManager mWorkflowManager
        {
            get { return AveAssemblyUtility.GetStaticPropertyValue(workflowManagerGlobal_Type, "WorkflowManager") as SPWorkflowManager; }
        }

        public AveWorkflowManager(SPWorkflowManager workflowManager)
        {
            //mWorkflowManager = workflowManager;
        }

        public AveWorkflowManager()
        { }

        #region IAveWorkflowManager Members

        public IAveWorkflowTemplateCollection GetWorkflowTemplatesByCategory(IAveWeb web, string strReqCategs)
        {
            return new AveWorkflowTemplateCollection(web,mWorkflowManager.GetWorkflowTemplatesByCategory((web as AveWeb).Web, strReqCategs));
        }

        public void EnableDeclarativeWorkflows(IAveSite site, bool fEnable)
        {
            AveAssemblyUtility.InvokeStaticMethod(mWorkflowManager_Type, "EnableDeclarativeWorkflows", new Type[] { typeof(SPSite), typeof(bool) }, new object[] { (site as AveSite).Site, fEnable });
        }

        public void Dispose()
        {
            //mWorkflowManager.Dispose();
        }

        //封装的静态方法，需要传一个IAveWeb进去，wrapper中暂时没有使用，先传null,稍后再考虑处理
        public IAveWorkflowTemplate WorkflowTemplateFromElement(IAveWorkflowElement wfDef)
        {
            return new AveWorkflowTemplate(null,AveAssemblyUtility.InvokeStaticMethod(mWorkflowManager_Type, "WorkflowTemplateFromElement", new object[] { (wfDef as AveWorkflowElement).WorkflowElement }) as SPWorkflowTemplate);
        }

        public void RemoveWorkflowFromListItem(IAveWorkflow instance)
        {
            mWorkflowManager.RemoveWorkflowFromListItem((instance as AveWorkflow).Workflow);
        }

        public List<IAveWorkflow> GetItemWorkflows(IAveListItem item, Guid id)
        {                                   
            IAveWorkflowCollection Wfs = new AveWorkflowCollection(item,mWorkflowManager.GetItemWorkflows((item as AveListItem).ListItem));
            List<IAveWorkflow> ItemWfInstance = new List<IAveWorkflow>();
            
            foreach (IAveWorkflow Wf in Wfs)
            {
                if (Wf.ParentAssociation.ID == id)
                {
                    ItemWfInstance.Add(Wf);
                }
            }
            
            return ItemWfInstance;
        }

        public List<IAveWorkflow> GetItemWorkflows(IAveListItem item)
        {
            IAveWorkflowCollection Wfs = new AveWorkflowCollection(item,mWorkflowManager.GetItemWorkflows((item as AveListItem).ListItem));
            List<IAveWorkflow> ItemWfInstance = new List<IAveWorkflow>();

            foreach (IAveWorkflow Wf in Wfs)
            {
                ItemWfInstance.Add(Wf);
            }

            return ItemWfInstance;
        }

        /// <summary>
        /// Used by SharePoint 2010
        /// </summary>
        /// <param name="parentItem"></param>
        /// <param name="association"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public IAveWorkflow StartWorkflow(object parentItem, IAveWorkflowAssociation association, string eventData, AveWorkflowRunOptions options)
        {
            object parentSPItem = null;
            if (parentItem is IAveWeb)
            {
                parentSPItem = (parentItem as AveWeb).Web;
            }
            else
            {
                parentSPItem = (parentItem as AveListItem).ListItem;
            }

            //SPWorkflowRunOptions is a public enum,but for compiled successfully,it must use reflect to get type
            Type typeofSPWorkflowRunOptions = typeof(SPListItem).Assembly.GetType("Microsoft.SharePoint.Workflow.SPWorkflowRunOptions");
            object valueofSPWorkflowRunOptions = Enum.ToObject(typeofSPWorkflowRunOptions, (int)options);
            return new AveWorkflow(association,(SPWorkflow)AveAssemblyUtility.InvokeMethod(mWorkflowManager, "StartWorkflow",
                new Type[] { typeof(object), typeof(SPWorkflowAssociation), typeof(string), typeofSPWorkflowRunOptions },
                new object[] { parentSPItem, (association as AveWorkflowAssociation).WorkflowAssociation, eventData, valueofSPWorkflowRunOptions }));
        }

        public int CountWorkflows(IAveWorkflowAssociation association)
        {
            return mWorkflowManager.CountWorkflows((association as AveWorkflowAssociation).WorkflowAssociation);
        }

        public IAveWorkflowCollection GetItemActiveWorkflows(IAveListItem item)
        {
            SPWorkflowCollection wfCollection = mWorkflowManager.GetItemActiveWorkflows((item as AveListItem).ListItem);
            return wfCollection == null ? null : new AveWorkflowCollection(item,wfCollection);
        }

        /// <summary>
        /// Used by SharePoint 2007
        /// </summary>
        /// <param name="parentItem"></param>
        /// <param name="association"></param>
        /// <param name="bAutoStart"></param>
        /// <param name="bCreateOnly"></param>
        /// <returns></returns>
        public IAveWorkflow StartWorkflow(IAveListItem parentItem, IAveWorkflowAssociation association, bool bAutoStart, bool bCreateOnly)
        {
            Type spWorkflowEventType = typeof(SPListItem).Assembly.GetType("Microsoft.SharePoint.Workflow.SPWorkflowEvent");
            object spWorkflowEventInstance = AveAssemblyUtility.CreateInstanceByType(spWorkflowEventType);

            AveAssemblyUtility.SetPropertyValue(spWorkflowEventInstance, "EventData", new object[] { "" });
            AveAssemblyUtility.SetPropertyValue(spWorkflowEventInstance, "isAutoStart", false);
            AveAssemblyUtility.SetPropertyValue(spWorkflowEventInstance, "RunAsUserId", parentItem.Web.CurrentUser.ID);

            //目前该方法标注为07使用，以后考虑去掉
            return new AveWorkflow(association,(SPWorkflow)AveAssemblyUtility.InvokeMethod(mWorkflowManager, "StartWorkflow",
                new Type[] { typeof(object), typeof(SPWorkflowAssociation), spWorkflowEventType, typeof(bool), typeof(bool) },
                new object[] { (parentItem as AveListItem).ListItem, (association as AveWorkflowAssociation).WorkflowAssociation, spWorkflowEventInstance, bAutoStart, true }));
        }

        public void CancelWorkflow(IAveWorkflow workflow) 
        {
            SPWorkflowManager.CancelWorkflow((workflow as AveWorkflow).Workflow);
        }
        #endregion






    }
}
