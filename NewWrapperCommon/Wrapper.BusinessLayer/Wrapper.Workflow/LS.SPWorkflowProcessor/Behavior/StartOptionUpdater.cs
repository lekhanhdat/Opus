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
//using AvePoint.Common;
//using AvePoint.GCommon;
///********************************************************************
// *
// *  PROPRIETARY and CONFIDENTIAL
// *
// *  This file is licensed from, and is a trade secret of:
// *
// *                   AvePoint, Inc.
// *                   Harborside Financial Center
// *                   9th Fl.   Plaza Ten
// *                   Jersey City, NJ 07311
// *                   United States of America
// *                   Telephone: +1-800-661-6588
// *                   WWW: www.avepoint.com
// *
// *  Refer to your License Agreement for restrictions on use,
// *  duplication, or disclosure.
// *
// *  RESTRICTED RIGHTS LEGEND
// *
// *  Use, duplication, or disclosure by the Government is
// *  subject to restrictions as set forth in subdivision
// *  (c)(1)(ii) of the Rights in Technical Data and Computer
// *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
// *  FAR 52.227-19 (C) (June 1987).
// *
// *  Copyright © 2001-2016 AvePoint® Inc. All Rights Reserved. 
// *
// *  Unpublished - All rights reserved under the copyright laws of the United States.
// *  $Revision:  $
// *  $Author:  $        
// *  $Date:  $
// */
//using AvePoint.Wrapper.Common;
//using System;
//using System.Collections.Generic;
//using WorkflowConfiguration = AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration;
//namespace LS.SPWorkflowProcessor
//{
//    interface IStartOptionUpdater
//    {
//        SPWFAssociationUnit AssociationUnit { get; set; }
//        void PreUpdate();
//        void PostUpdate();

//    }

//    class StartOptionBaseUpdater : IStartOptionUpdater
//    {
//        protected IAveLogger log = AveLogger.GetInstance(typeof(StartOptionBaseUpdater));
//        public SPWFAssociationUnit AssociationUnit { get; set; }

//        public static IStartOptionUpdater GetDefaultUpdater(SPWFAssociationUnit associationUnit,object association)
//        {
//            IStartOptionUpdater updater;
//            switch (associationUnit.WFInternalPlatform)
//            {
//                case SPWFInternalPlatform.WF2010PlatformType:
//                    updater = new StartOption10ModeDefaultUpdater(associationUnit, association as IAveWorkflowAssociation);
//                    break;
//                case SPWFInternalPlatform.WF2013PlatformType:
//                    updater = new StartOption13ModeDefaultUpdater(associationUnit, association as IAveWorkflowSubscription);
//                    break;
//                default:
//                    updater = new StartOptionBaseUpdater(associationUnit);
//                    break;
//            }
//            return updater;
//        }

//        public static IStartOptionUpdater GetDelayUpdater(SPWFAssociationUnit associationUnit,object association)
//        {
//            IStartOptionUpdater updater;
//            switch (associationUnit.WFInternalPlatform)
//            {
//                case SPWFInternalPlatform.WF2010PlatformType:
//                    updater = new StartOption10ModeDelayUpdater(associationUnit,association as IAveWorkflowAssociation);
//                    break;
//                case SPWFInternalPlatform.WF2013PlatformType:
//                    updater = new StartOption13ModeDelayUpdater(associationUnit,association as IAveWorkflowSubscription);
//                    break;
//                default:
//                    updater = new StartOptionBaseUpdater(associationUnit);
//                    break;
//            }
//            return updater;
//        }

//        public StartOptionBaseUpdater(SPWFAssociationUnit associationUnit)
//        {
//            AssociationUnit = associationUnit;
//        }

//        public virtual void PreUpdate()
//        {
           
//        }

//        public virtual void PostUpdate()
//        {
           
//        }
//    }

//    class StartOption10ModeDefaultUpdater : StartOptionBaseUpdater
//    {
//        protected IAveWorkflowAssociation Association { get; set; }
//        public StartOption10ModeDefaultUpdater(SPWFAssociationUnit associationUnit,IAveWorkflowAssociation association)
//            : base(associationUnit)
//        {
//            Association = association;
//        }

//        public override void PreUpdate()
//        {
//            Association.AutoStartCreate = CheckConfiguration(AssociationUnit.SerializableData.mConfiguration, WorkflowConfiguration.AutoStartAdd);
//            Association.AutoStartChange = CheckConfiguration(AssociationUnit.SerializableData.mConfiguration, WorkflowConfiguration.AutoStartChange);                                        
//        }

//        public override void PostUpdate()
//        {
//            base.PostUpdate();
//        }

//        protected bool CheckConfiguration(int configuration, AvePoint.Wrapper.Common.AveWorkflowAssociationCollection.Configuration check)
//        {
//            return (((WorkflowConfiguration)configuration & check) != WorkflowConfiguration.None);
//        }
       
//    }

//    class StartOption10ModeDelayUpdater : StartOption10ModeDefaultUpdater
//    {
//        public StartOption10ModeDelayUpdater(SPWFAssociationUnit associationUnit,IAveWorkflowAssociation association)
//            : base(associationUnit,association)
//        { }

//        public override void PreUpdate()
//        {
//            log.Debug("Set workflow start option to default value.");               
//            Association.AutoStartCreate = false;
//            Association.AutoStartChange = false;
//        }

//        public override void PostUpdate()
//        {
//            log.Debug("Begin to post update 10mode Workflow start option in post action. Name:{0},ParentType:{1}", AssociationUnit.SerializableData.mName, AssociationUnit.ParentObjectType);               
//            bool needUpdate = false;
//            AssociationUnit.ReloadSPAssociation();
//            Association = AssociationUnit.SPAssociation;
//            if (Association != null)
//            {
//                 bool autoStartChange = (((WorkflowConfiguration)AssociationUnit.SerializableData.mConfiguration & WorkflowConfiguration.AutoStartChange) != WorkflowConfiguration.None);
//                bool autoStartCreate = (((WorkflowConfiguration)AssociationUnit.SerializableData.mConfiguration & WorkflowConfiguration.AutoStartAdd) != WorkflowConfiguration.None);
//                if (Association.AutoStartChange != autoStartChange)
//                {
//                    Association.AutoStartChange = autoStartChange;
//                    needUpdate = true;
//                }
//                if (Association.AutoStartCreate != autoStartCreate)
//                {
//                    Association.AutoStartCreate = autoStartCreate;
//                    needUpdate = true;
//                }
//            }
//            if (needUpdate)
//            {
//                log.Debug("Update Workflow start option in post action. Name:{0},ID:{1},ParentType:{2}",Association.Name,Association.ID,AssociationUnit.ParentObjectType);
//                AssociationUnit.UpdateWorkflowAssociation(Association);
//            }
//        }
//    }

//    class StartOption13ModeDefaultUpdater : StartOptionBaseUpdater
//    {
//        protected IAveWorkflowSubscription Subscription { get; set; }
//        public StartOption13ModeDefaultUpdater(SPWFAssociationUnit associationUnit,IAveWorkflowSubscription subscription)
//            : base(associationUnit)
//        {
//            Subscription = subscription;
//        }
//    }

//    class StartOption13ModeDelayUpdater : StartOption13ModeDefaultUpdater
//    {
//        public StartOption13ModeDelayUpdater(SPWFAssociationUnit associationUnit,IAveWorkflowSubscription subscription)
//            : base(associationUnit,subscription)
//        { }

//        public override void PreUpdate()
//        {
//            if (Subscription == null)
//            {
//                log.Info("Workflow subscription is null, do not need to upddate the start option.");
//                return;
//            }
//            log.Debug("Remove auto start option from 13mode workflow subscription properties.");               
//            if (Subscription.EventTypes.Contains("ItemAdded"))
//            {
//                Subscription.EventTypes.Remove("ItemAdded");
//            }
//            if (Subscription.EventTypes.Contains("ItemUpdated"))
//            {
//                Subscription.EventTypes.Remove("ItemUpdated");
//            }
//        }

//        public override void PostUpdate()
//        {
//            log.Debug("Begin to post update 13mode Workflow start option in post action. Name:{0},ParentType:{1}", AssociationUnit.SerializableData.mName, AssociationUnit.ParentObjectType);               
            
//            var workflowServiceFactory=Singleton<WorkflowServiceFactory>.GetSingletonInstance(new object[] { AssociationUnit.ParentWeb });
//            if(workflowServiceFactory==null)
//            {
//                log.Info("workflowServiceFactory is null, do not need to upddate the start option.");
//                return;
//            }
//            workflowServiceFactory.UpdateWorkflowServiceManager(AssociationUnit.ParentWeb);
//            var workflowSubscriptionService=workflowServiceFactory.WFSubscriptionService;
//            if(workflowSubscriptionService==null)
//            {
//                log.Info("workflowSubscriptionService is null, do not need to upddate the start option.");
//                return;
//            }
//            bool needUpdate = false;
//            var workflowSubscription = workflowSubscriptionService.GetSubscription(AssociationUnit.WorkflowSubscription.Id);
//            //var newdd = this.WFSubscriptionService.PublishSubscription(newdefinitionSubscription);
//            object eventTypes=null;
//            var props13Mode = AssociationUnit.SerializableData.Properties.GetEx("Props.13Model") as Dictionary<string, object>;
//            if (props13Mode != null && props13Mode.TryGetValue("SharePointWorkflowContext.Subscription.EventType", out eventTypes) && eventTypes != null)
//            {
//                var events = eventTypes.ToString().Split(new string[] { "#;" }, StringSplitOptions.RemoveEmptyEntries);
//                foreach (string eventType in events)
//                {
//                    if ((string.Equals(eventType, "ItemAdded", StringComparison.Ordinal) || string.Equals(eventType, "ItemUpdated", StringComparison.Ordinal)) && !workflowSubscription.EventTypes.Contains(eventType))
//                    {
//                        needUpdate = true;
//                        workflowSubscription.EventTypes.Add(eventType);
//                    }
//                }
//            }
//            if (needUpdate)
//            {
//                Guid workflowSubscriptionId = Guid.Empty;
//                switch (AssociationUnit.ParentObjectType)
//                {
//                    case SPWFAssociationParentType.List:
//                        workflowSubscriptionId = workflowSubscriptionService.PublishSubscriptionForList(workflowSubscription, AssociationUnit.ParentList.ID);
//                        break;
//                    case SPWFAssociationParentType.Web:
//                        workflowSubscriptionId = workflowSubscriptionService.PublishSubscription(workflowSubscription);
//                        break;
//                    case SPWFAssociationParentType.ListContentType:
//                        workflowSubscriptionId = workflowSubscriptionService.PublishSubscription(workflowSubscription);
//                        break;
//                    default:
//                        log.Warn("Invalid 13mode workflow parent object type:{0}", AssociationUnit.ParentObjectType);
//                        break;
//                }
//                log.Debug("Update Workflow start option in post action. Name:{0},ID:{1},ParentType:{2}",workflowSubscription.Name, workflowSubscriptionId, AssociationUnit.ParentObjectType);
//            }
//        }
//    }

//}
 