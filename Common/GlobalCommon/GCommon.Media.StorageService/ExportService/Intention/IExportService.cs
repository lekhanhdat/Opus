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



namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives
    using System.IO;
    #endregion

    /// <summary>
    /// The interface is used to export data to device
    /// <example>
    /// <code>
    ///   var exportServiceProvider = new ExportServiceProvider();
    ///   var exportService = exportServiceProvider.Create("Autonomy");
    ///   exportService.Open(serviceinfo);
    ///   exportService.Export(dataStream,exportInfo);
    ///   exportService.Export(metaData,exportInfo);
    ///   exportServcie.Close()
    /// </code>
    /// </example>
    /// </summary>
    public interface IExportService
        : IExportServiceEvents
    {
        /// <summary>
        /// Open the Hold Service
        /// </summary>
        /// <param name="holdInfo">the open parameters of hold service</param>
        void Open(ExportServiceInfo exportServiceInfo);

        /// <summary>
        /// Export the hold files which as a export file object
        /// </summary>
        /// <param name="exportFile">the exported files</param>
        /// <returns>the export result</returns>
        ExportResult Export(Stream dataStream, MetaData metaData, ExportInfo exportInfo = null);
        ExportResult Export(IDataReader dataReader, MetaData metaData, ExportInfo exportInfo = null);
        ExportResult Export(DataReadAction read, MetaData metaData, ExportInfo exportInfo = null);

        ExportDataFileResult Export(Stream dataStream, ExportInfo exportInfo);
        ExportDataFileResult Export(IDataReader dataReader, ExportInfo exportInfo);
        ExportDataFileResult Export(DataReadAction read, ExportInfo exportInfo);
        ExportMetaDataFileResult Export(MetaData metaData, ExportInfo exportInfo);

        Stream GetExportStream(ExportInfo exportInfo);

        /// <summary>
        /// Close the hold service
        /// </summary>
        void Close();
    }
}