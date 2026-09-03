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

namespace AvePoint.ObjectModel.ServerSE.NonPublicAPI
{
    using Wrapper.Common;
    using Microsoft.SharePoint;
    using System;
    using System.IO;
    using System.Reflection;
    using GCommon;

    [NonPublicAPI("Microsoft.SharePoint.SPFile")]
    internal static class SPFileExtension
    {
        private static readonly Type TypeOfSPFile = typeof(SPFile);

        #region CancelUpload
        public static void CancelUpload(this SPFile file, Guid uploadId)
        {
            GetDelegate_CancelUpload()(file, uploadId);
        }
        private static Action<SPFile, Guid> GetDelegate_CancelUpload()
        {
            return TypeOfSPFile.GetMethod<Action<SPFile, Guid>>(nameof(CancelUpload), BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Guid) }, null);
        }
        #endregion

        #region ContinueUpload
        public static long ContinueUpload(this SPFile file, Guid uploadId, long fileOffset, Stream stream)
        {
            return GetDelegate_ContinueUpload()(file, uploadId, fileOffset, stream);
        }

        private static Func<SPFile, Guid, long, Stream, long> GetDelegate_ContinueUpload()
        {
            return TypeOfSPFile.GetMethod<Func<SPFile, Guid, long, Stream, long>>(nameof(ContinueUpload), BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Guid), typeof(long), typeof(Stream) }, null);
        }
        #endregion

        #region FinishUpload
        public static SPFile FinishUpload(this SPFile file, Guid uploadId, long fileOffset, Stream stream)
        {
            return GetDelegate_FinishUpload()(file, uploadId, fileOffset, stream);
        }

        private static Func<SPFile, Guid, long, Stream, SPFile> GetDelegate_FinishUpload()
        {
            return TypeOfSPFile.GetMethod<Func<SPFile, Guid, long, Stream, SPFile>>(nameof(FinishUpload), BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Guid), typeof(long), typeof(Stream) }, null);
        }
        #endregion

        #region StartUpload
        public static long StartUpload(this SPFile file, Guid uploadId, Stream stream)
        {
            return GetDelegate_StartUpload()(file, uploadId, stream);
        }

        private static Func<SPFile, Guid, Stream, long> GetDelegate_StartUpload()
        {
            return TypeOfSPFile.GetMethod<Func<SPFile, Guid, Stream, long>>(nameof(StartUpload), BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Guid), typeof(Stream) }, null);
        }
        #endregion

        public static void SaveBinaryExtension(this SPFile file, Stream fileStream)
        {
            if (fileStream.Length < 2047 * 1024 * 1024)
            {
                file.SaveBinary(fileStream);
            }
            else
            {
                FileUploader.SaveBinaryInternal(file, fileStream);
            }
        }
        public static void SaveBinaryExtension(this SPFile file, Stream fileStream, bool checkRequiredFields, bool createVersion, string etagMatch, string lockIdMatch, Stream fileFormatMetaInfo, out string etagNew)
        {
            if (fileStream.Length < 2047 * 1024 * 1024)
            {
                file.SaveBinary(fileStream, checkRequiredFields, createVersion, etagMatch, lockIdMatch, fileFormatMetaInfo, out etagNew);
            }
            else
            {
                using (MemoryStream emptyStream = new MemoryStream(new byte[] { }))
                {
                    file.SaveBinary(emptyStream, checkRequiredFields, createVersion, etagMatch, lockIdMatch, fileFormatMetaInfo, out etagNew);
                    FileUploader.SaveBinaryInternal(file, fileStream);
                }
            }
        }

    }
}
