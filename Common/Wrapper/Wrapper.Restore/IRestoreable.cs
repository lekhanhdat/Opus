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






using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AvePoint.Wrapper.Restore
{
    public interface IRestoreable
    {
        void SetRestoreOption(AveRestoreOption restoreOption);
        AveRestoreOption RestoreOption { get; }
    }

    public class RestoreableObject : IRestoreable
    {
        protected AveRestoreOption mRestoreOption = new AveRestoreOption();
        protected AveSecurityMapping mSecurityMapping;
        protected AveCommonRestoreConfiguraion mRestoreConfig;
        protected bool mIsRestored;

        public RestoreableObject()
        {
        }

        public RestoreableObject(AveSecurityMapping securityMapping, AveCommonRestoreConfiguraion restoreConfig)
        {
            mSecurityMapping = securityMapping;
            mRestoreConfig = restoreConfig;
        }

        public AveRestoreOption RestoreOption
        {
            get { return mRestoreOption; }
        }       

        public void SetRestoreOption(AveRestoreOption restoreOption)
        {
            mRestoreOption = restoreOption;
        }

        public bool CheckRestoreOption(AveRestoreMode ro)
        {
            return CheckRestoreOption(false, ro);
        }

        public bool CheckRestoreOption(bool isNewCreated, AveRestoreMode ro)
        {
            switch (ro)
            {
                case AveRestoreMode.OverWrite:
                    return isNewCreated || mRestoreOption.CheckRestoreOption(AveRestoreMode.OverWrite);
                case AveRestoreMode.RestoreProperty:
                    return (isNewCreated || mRestoreOption.CheckRestoreOption(AveRestoreMode.OverWrite)) && mRestoreOption.CheckRestoreOption(ro);
                case AveRestoreMode.RestoreSecurity:
                    return (isNewCreated || mRestoreOption.CheckRestoreOption(AveRestoreMode.OverWrite)) && mRestoreOption.CheckRestoreOption(ro);
                default:
                    return mRestoreOption.CheckRestoreOption(ro);
            }
        }

        public AveCommonRestoreConfiguraion RestoreConfiguraion
        {
            get
            {
                return mRestoreConfig;
            }
        }

        public bool IsRestored
        {
            get
            {
                return mIsRestored;
            }
            internal set
            {
                mIsRestored = value;
            }
        }
    }

    public class RestoreableObject<T> : RestoreableObject
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected List<IPostAction<T>> postActions;

        public RestoreableObject() : base()
        {
            postActions = new List<IPostAction<T>>();
        }

        public RestoreableObject(AveSecurityMapping securityMapping, AveCommonRestoreConfiguraion restoreConfig) : base(securityMapping, restoreConfig)
        {
            postActions = new List<IPostAction<T>>();
        }

        public void Add(IPostAction<T> item)
        {
            lock (postActions)
            {
                postActions.Add(item);
            }
        }

        public void ResolvePostActions(T item)
        {
            try
            {
                foreach (var action in postActions)
                {
                    try
                    {
                        if (!action.Resolve(item))
                        {
                            log.Warn("Cannot post action for:{0}", action);
                        }
                        else
                        {
                            log.Info("Do post action for {0} successfully.", action);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Post action for:{0} failed:{1}", action, ex);
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Do post action failed:{0}", e);
            }
            finally
            {
                postActions.Clear();
            }
        }
    }
}
