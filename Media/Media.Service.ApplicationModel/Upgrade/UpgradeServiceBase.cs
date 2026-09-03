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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon.Contract.CodeReview;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/2/22",
    "jbli@avepoint.com",
    "dwxue@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_12 },
    "ADO-26066",
    true)]
    #endregion
    public abstract class UpgradeServiceBase<TParameter>
        : ApplicationModelServiceBase
        , IUpgradeService
        where TParameter : class, IUpgradeInfo, new()
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public void Upgrade(IUpgradeInfo upgradeInfo)
        {
            ThreadPool.QueueUserWorkItem(upgradeRequest =>
            {
                InternalUpgrade(upgradeInfo as TParameter);
            }, upgradeInfo);
        }

        void InternalUpgrade(TParameter upgradeInfo)
        {
            try
            {
                this.Open(upgradeInfo);
                this.Upgrade(upgradeInfo);
            }
            catch (Exception e)
            {
                this.ProcessException(e);
            }
            finally
            {
                try
                {
                    this.GenerateJobReport();
                    this.Close();
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex.ToString());
                }
            }
        }

        public abstract void Open(TParameter upgradeInfo);
        public abstract void Upgrade(TParameter upgradeInfo);
        public abstract void ProcessException(Exception e);
        public virtual void GenerateJobReport() { }
        public virtual void Close()
        {
            this.Dispose();
        }
        public abstract void Dispose();
    }
}
