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




namespace AvePoint.Media.Service
{
    #region using directives

    using System;
    using System.Reflection;
    using AvePoint.GCommon;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.Media.Service.DomainModel;

    #endregion using directives

    public abstract class BrowserServiceBase<TParameter, TResult>
        : ApplicationModelServiceBase
        , IBrowserService
        where TParameter : class, IBrowseInfo, new()
        where TResult : class, IBrowseResult, new()
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public IBrowseResult Browse(IBrowseInfo browserInfo)
        {
            var info = browserInfo as TParameter;
            return this.InternalBrowse(info);
        }

        private TResult InternalBrowse(TParameter browserInfo)
        {
            var result = default(TResult);
            try
            {
                logger.Debug(MediaServiceApplicationModelResource.BrowserServiceBaseInternalBrowseBegin, browserInfo.ToString());
                this.Open(browserInfo);
                result = this.Browse(browserInfo);
            }
            catch (Exception e)
            {
                this.ProcessException(e);
                throw;
            }
            finally { this.Close(); }
            return result;
        }

        public abstract void Open(TParameter browserInfo);

        public abstract TResult Browse(TParameter browserInfo);

        public abstract void ProcessException(Exception e);

        public virtual void Close()
        {
            this.Dispose();
        }

        public abstract void Dispose();
    }
}