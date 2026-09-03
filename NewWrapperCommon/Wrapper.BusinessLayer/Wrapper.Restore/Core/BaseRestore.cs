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
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    /// <summary>
    /// Restore Controller
    /// </summary>
    /// <typeparam name="TRestoreOption"></typeparam>
    /// <typeparam name="TRestoreProfiler"></typeparam>
    /// <typeparam name="TRestoreReport"></typeparam>
    internal abstract class RestoreController<TRestoreOption, TRestoreProfiler, TRestoreReport> where TRestoreProfiler : ISPImportProfiler
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(TRestoreOption));

        /// <summary>
        /// Restore the metadata according to the restore option
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        public virtual TRestoreReport Restore(Common.IAveRestoreStream restoreStream, TRestoreOption restoreOption)
        {
            var profiler = CreateDefaultProfiler();

            Restore(restoreStream, restoreOption, profiler);

            var report = GenerateReport(profiler);

            return report;
        }

        public virtual void Restore(IAveRestoreStream restoreStream, TRestoreOption restoreOption, TRestoreProfiler profiler)
        {
            if (restoreStream == null)
            {
                throw new ArgumentNullException("restoreStream");
            }
            if (restoreOption == null)
            {
                throw new ArgumentNullException("restoreOption");
            }

            try
            {
                if (profiler != null) { profiler.BeginRestore(); }

                BeginRestore(restoreOption);

                while (true)
                {
                    var metadata = restoreStream.ReadMetadata();

                    if (metadata == null)
                    {
                        break;
                    }

                    var metadataType = metadata.MetadataType;

                    var action = GetMetadataRestoreAction(metadataType);

                    if (action != null)
                    {
                        try
                        {
                            if (profiler != null) { profiler.BeginRestoreMetadata(metadataType); }

                            action(restoreStream, metadata, restoreOption, profiler);
                        }
                        finally
                        {
                            if (profiler != null) { profiler.EndRestoreMetadata(metadataType); }
                        }
                    }
                    else
                    {
                        logger.Warn(
                            WrapperResource.GetString(WrapperResourceKey.Wrapper_NoAvailableActionAccordingToType,
                                                      metadataType));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(WrapperResource.GetString(WrapperResourceKey.Wrapper_RestoreFailed, ex));
                //profiler.Failed(ex.ToString());
                throw;
            }
            finally
            {
                EndRestore(restoreOption);
                if (profiler != null) { profiler.EndRestore(); }
            }
        }

        /// <summary>
        /// Get Metadata Restore Action accoding to the metadata type
        /// </summary>
        /// <param name="metadataType"></param>
        /// <returns></returns>
        protected abstract Action<Common.IAveRestoreStream , Common.AveMetadata, TRestoreOption, TRestoreProfiler> GetMetadataRestoreAction(
            AveMetadataType metadataType);

        /// <summary>
        /// Get default profiler
        /// </summary>
        /// <returns></returns>
        protected abstract TRestoreProfiler CreateDefaultProfiler();

        /// <summary>
        /// Generate Report
        /// </summary>
        /// <param name="profiler"></param>
        /// <returns></returns>
        protected abstract TRestoreReport GenerateReport(TRestoreProfiler profiler);

        /// <summary>
        /// Begin Restore
        /// </summary>
        /// <param name="restoreOption"></param>
        protected abstract void BeginRestore(TRestoreOption restoreOption);

        /// <summary>
        /// End Restore
        /// </summary>
        /// <param name="restoreOption"></param>
        protected abstract void EndRestore(TRestoreOption restoreOption);
    }
}
