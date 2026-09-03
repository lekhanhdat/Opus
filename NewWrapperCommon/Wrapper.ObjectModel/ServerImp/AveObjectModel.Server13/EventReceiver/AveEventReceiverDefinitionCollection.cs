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

namespace AvePoint.ObjectModel.Server13
{
    class AveEventReceiverDefinitionCollection : AveAbstractCommonCollection<IAveEventReceiverDefinition>, IAveEventReceiverDefinitionCollection
    {
        private SPEventReceiverDefinitionCollection mEventReceiverDefinitionCollection;

        public AveEventReceiverDefinitionCollection(SPEventReceiverDefinitionCollection eventReceiverDefinitions)
            : base(eventReceiverDefinitions)
        {
            mEventReceiverDefinitionCollection = eventReceiverDefinitions;
        }

        #region IAveEventReceiverDefinitionCollection Members

        public IAveEventReceiverDefinition this[Guid eventReceiverId]
        {
            get
            {
                SPEventReceiverDefinition eventReceiveDefinition = mEventReceiverDefinitionCollection[eventReceiverId];
                if (eventReceiveDefinition == null)
                {
                    return null;
                }
                return new AveEventReceiverDefinition(eventReceiveDefinition);
            }
        }

        public void Add(AveEventReceiverType receiverType, string assembly, string className)
        {
            mEventReceiverDefinitionCollection.Add((SPEventReceiverType)receiverType, assembly, className);
        }

        public override IAveEventReceiverDefinition this[int index]
        {
            get
            {
                return new AveEventReceiverDefinition(mEventReceiverDefinitionCollection[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveEventReceiverDefinition(t as SPEventReceiverDefinition);
        }

        public override int Count
        {
            get { return mEventReceiverDefinitionCollection.Count; }
        }

        public void Add(AveEventReceiverType receiverType, string assembly, string className, string name)
        {
            SPEventReceiverDefinition eventReceiver = mEventReceiverDefinitionCollection.Add();
            eventReceiver.Name = name;
            eventReceiver.Type = (SPEventReceiverType)receiverType;
            eventReceiver.Assembly = assembly;
            eventReceiver.Class = className;
            eventReceiver.Update();
        }

        #endregion
    }
}
