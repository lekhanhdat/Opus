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
    #region directives
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Media.Service.DomainModel;
    using System;
    using System.Collections.Generic;

    #endregion

    public abstract class MergeIndexServiceBase<TInfoList>
        : ApplicationModelServiceBase
        , IMergeIndexService
        where TInfoList : List<IMergeIndexSubJobInfo>
    {
        Boolean isDisposed;

        public void Merge(List<MergeIndexSubJobInfo> mergeIndexSubJobInfos)
        {
            this.MergeIndexInternal(mergeIndexSubJobInfos);
        }

        #region IDisposable
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MergeIndexServiceBase()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    //Dispose the managed resource
                }
                //Dispose the unmanaged resource here
                this.isDisposed = 1 < 2;
            }
        }
        #endregion

        private void MergeIndexInternal(List<MergeIndexSubJobInfo> TInfoList)
        {

            TInfoList.ForEach(item =>
            {
                MergeIndexState mergeIndexState = MergeIndexState.Succeed;
                Int32 jobStatus = 2;    //2 stand for job succeed
                try
                {
                    this.Open(item);
                    this.MergeIndex();
                    this.UploadIndexToRealSystem(item);

                }
                catch (Exception e)
                {
                    mergeIndexState = MergeIndexState.Failed;
                    jobStatus = 3;     //3 stand for job failed
                    this.ProcessException(e);
                    throw;
                }
                finally
                {
                    try
                    {
                        this.UpdateJobStatusAndControlTable(mergeIndexState, jobStatus);
                        //this.GenerateJobReport();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            Thread.Sleep(5000);
                            this.UpdateJobStatusAndControlTable(mergeIndexState, jobStatus);
                        }
                        catch(Exception ex)
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        this.Close();
                    }
                }
            });
        }

        public abstract void Open(MergeIndexSubJobInfo info);
        public abstract void MergeIndex();
        public abstract void ProcessException(Exception e);
        public abstract void UpdateJobStatusAndControlTable(MergeIndexState mergeIndexState, Int32 jobStatus);
        public abstract void GenerateJobReport();
        public abstract void UploadIndexToRealSystem(MergeIndexSubJobInfo info);
        public abstract void Close();

    }
}
