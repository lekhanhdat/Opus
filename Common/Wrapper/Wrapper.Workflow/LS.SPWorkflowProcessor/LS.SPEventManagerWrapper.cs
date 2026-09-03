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
using System.Reflection;
using AvePoint.Wrapper.Common;


namespace LS.SPWorkflowProcessor
{
    internal class SPEventManagerWrapper
    {

        private static IAveEventManager EvnetManager = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateEventManager();

        //private static readonly string mClassName = "Microsoft.SharePoint.SPEventManager";
        //private static readonly string mDisablePropertyName = "EventFiringDisabled";
        //private static Type mEventManagerType;



        //private static Type EventManagerType
        //{
        //    get
        //    {
        //        if (mEventManagerType == null)
        //            GetEventManagerType();
        //        return mEventManagerType;
        //    }
        //}

        //private static Type GetEventManagerType()
        //{
        //    try
        //    {
        //        mEventManagerType = typeof(IAveList).Assembly.GetType(mClassName, true);
        //        return mEventManagerType;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new SPWFProcessorException(SPWFProcessorErrorCode.CannotGetSPEventManagerType,e);
        //    }
        //}

        public static bool EventFiringDisabled
        {
            get 
            {
                try
                {
                    return EvnetManager.EventFiringDisabled;
                }
                catch (Exception e)
                {
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.CannotGetEventFiringDisabledStatus,e);
                }
            }
        }

        public static void EnableEventFiring()
        {
            try
            {
                EvnetManager.EnableEventFiring();
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.CannotSetEventFiringDisabledStatus, e, "Set property EventFiringDisabled to false exception");
            }
        }

        public static void DisableEventFiring()
        {
            try
            {
                EvnetManager.DisableEventFiring();
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.CannotSetEventFiringDisabledStatus, e, "Set property EventFiringDisabled to true exception");
            }
        }


    }
}
