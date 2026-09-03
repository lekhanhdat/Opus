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
    /// The interface is defines the hold logic and data format of the hold data,
    /// When you use the Hold Service, it must be opened.
    ///
    /// <example>
    ///   <code>
    ///    var holdInfo = new HoldInfo();
    ///    var holdService = new HoldServices();
    ///    holdService.Open(HoldInfo);
    ///    holdService.Hold(stream1,metaData1);
    ///    holdService.Hold(stream2,metaData2);
    ///    holdService.Hold(stream3,metaData3);
    ///    holdService.Close();
    ///   </code>
    /// </example>
    /// <remarks>
    /// A hold service is associated with a hold name and job id,
    /// that means, The hold name and job id can be identify a hold
    /// process, in this case , you must open hold service only once.
    /// </remarks>
    /// </summary>
    public interface IHoldService
    {
        /// <summary>
        /// Open the Hold Service
        /// </summary>
        /// <param name="holdInfo">the open parameters of hold service</param>
        void Open(HoldServiceInfo holdServiceInfo);

        /// <summary>
        /// Hold one file by stream.
        /// </summary>
        /// <param name="dataStream">file stream or other stream which can be represent
        /// a single file</param>
        /// <param name="metaData">meta data info of the file</param>
        /// <returns>the hold result of the hold process</returns>
        HoldResult Hold(Stream dataStream, MetaData metaData);

        /// <summary>
        /// Hold one file by data data reader
        /// </summary>
        /// <param name="dataDataReader">the file data reader</param>
        /// <param name="metaData">meta data info of the file</param>
        /// <returns></returns>
        HoldResult Hold(IDataReader dataReader, MetaData metaData);

        /// <summary>
        /// Hold one file by DataReadAction delegate
        /// </summary>
        /// <param name="dataReadAction"></param>
        /// <param name="metaData"></param>
        /// <returns></returns>
        HoldResult Hold(DataReadAction read, MetaData metaData);

        /// <summary>
        /// Release one file data and metadata file
        /// </summary>
        /// <param name="dataFileName">data file name </param>
        /// <param name="metaDataFileName">meta data file name </param>
        /// <returns>Release result of the file</returns>
        ReleaseResult Release(HoldFileInfo fileInfo);

        /// <summary>
        /// Close the hold service
        /// </summary>
        void Close();
    }
}