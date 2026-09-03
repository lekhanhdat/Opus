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
using Box.V2.Models;

namespace RABox
{
    public class BoxFileProxy : BoxItemProxy
    {
        public string VersionId { get; internal set; }

        public string Sha1 { get; internal set; }

        public string UploaderDisplayName { get; internal set; }

        public long? DispositionAt { get; internal set; }

        public BoxFileProxy(BoxClientContext clientContext, string id) : base(clientContext, GetBoxFile(clientContext, id, out var boxFile))
        {
            InitProperties(boxFile);
        }

        public BoxFileProxy(BoxClientContext clientContext, BoxFile boxFile) : base(clientContext, boxFile)
        {
            InitProperties(boxFile);
        }

        public BoxFileProxy(BoxClientContext clientContext, BoxFile boxFile, BoxFolderProxy parentFolderProxy) : base(clientContext, boxFile, parentFolderProxy)
        {
            InitProperties(boxFile);
        }

        private BoxFileProxy InitProperties(BoxFile _boxFile)
        {
            VersionId = _boxFile.FileVersion.Id;
            Sha1 = _boxFile.Sha1;
            UploaderDisplayName = _boxFile.UploaderDisplayName;
            DispositionAt = _boxFile.DispositionAt?.UtcTicks;

            return this;
        }

        private static BoxFile GetBoxFile(BoxClientContext clientContext, string id, out BoxFile boxFile)
        {
            boxFile = clientContext.GetFile(id);
            return boxFile;
        }

        public bool DeleteFilePermanently()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return false;
            }

            var isRemove = TrashedAt.HasValue || _clientContext.DeleteFile(Id);

            if (isRemove)
            {
                try
                {
                    var isPurge = _clientContext.PurgeTrashedFile(Id);
                    return isRemove && isPurge;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("access_denied_insufficient_permissions"))
                    {
                        if (DispositionAt.HasValue && DispositionAt.Value > DateTime.UtcNow.Ticks)
                        {
                            throw new Exception("BoxItemNotReachRetentionExpiration");
                        }
                        throw new Exception("BoxItemUnderLegalHold");
                    }
                    throw;
                }
            }
            return false;
        }
    }
}
