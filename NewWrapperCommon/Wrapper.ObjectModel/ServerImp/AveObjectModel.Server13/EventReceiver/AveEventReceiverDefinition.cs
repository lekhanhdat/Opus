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
    class AveEventReceiverDefinition : AveAutoSerializingObject, IAveEventReceiverDefinition
    {
        private SPEventReceiverDefinition mEventReceiverDefinition;

        public AveEventReceiverDefinition(SPEventReceiverDefinition eventReceiverDefinition)
            : base(eventReceiverDefinition)
        {
            mEventReceiverDefinition = eventReceiverDefinition;
        }

        #region IAveEventReceiverDefinition Members

        public string Assembly
        {
            get
            {
                return mEventReceiverDefinition.Assembly;
            }
            set
            {
                mEventReceiverDefinition.Assembly = value;
            }
        }

        public string Class
        {
            get
            {
                return mEventReceiverDefinition.Class;
            }
            set
            {
                mEventReceiverDefinition.Class = value;
            }
        }

        public string Name
        {
            get
            {
                return mEventReceiverDefinition.Name;
            }
            set
            {
                mEventReceiverDefinition.Name = value;
            }
        }

        public Guid ID
        {
            get { return mEventReceiverDefinition.Id; }
        }

        public AveEventReceiverType Type
        {
            get
            {
                return (AveEventReceiverType)mEventReceiverDefinition.Type;
            }
            set
            {
                mEventReceiverDefinition.Type = (SPEventReceiverType)value;
            }
        }

        public AveEventHostType HostType
        {
            get
            {
                return (AveEventHostType)mEventReceiverDefinition.HostType;
            }
            set
            {
                mEventReceiverDefinition.HostType = (SPEventHostType)value;
            }
        }

        public void Delete()
        {
            mEventReceiverDefinition.Delete();
        }

        public void Update()
        {
            mEventReceiverDefinition.Update();
        }

        public int Synchronization
        {
            get { return (int)mEventReceiverDefinition.Synchronization; }
            set
            {
                mEventReceiverDefinition.Synchronization = (SPEventReceiverSynchronization)Enum.Parse(typeof(SPEventReceiverSynchronization), value.ToString());
            }
        }

        public int SequenceNumber
        {
            get { return mEventReceiverDefinition.SequenceNumber; }
            set { mEventReceiverDefinition.SequenceNumber = value; }
        }
        #endregion
    }
}
