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
    public class BoxFolderProxy : BoxItemProxy
    {
        public bool IsRootFolder => Id == BoxUtility.BoxRootFolderId;

        public BoxFolderProxy(BoxClientContext clientContext, string id) : base(clientContext, clientContext.GetFolder(id))
        {
        }

        public BoxFolderProxy(BoxClientContext clientContext, BoxFolder boxFolder) : base(clientContext, boxFolder)
        {
        }

        public BoxFolderProxy(BoxClientContext clientContext, BoxFolder boxFolder, BoxFolderProxy parentFolderProxy) : base(clientContext, boxFolder, parentFolderProxy)
        {
        }

        public List<BoxItemProxy> GetSubFolders()
        {
            var subFolders = _clientContext.GetSubFolders(Id);
            return subFolders.ConvertAll(subFolder => new BoxItemProxy(_clientContext, subFolder));
        }

        public List<BoxItemProxy> GetSubItems()
        {
            var subItems = _clientContext.GetSubItems(Id);

            var result = new List<BoxItemProxy>();

            foreach (var item in subItems)
            {
                if (item.Type == BoxType.file.ToString())
                {
                    result.Add(new BoxFileProxy(_clientContext, (BoxFile)item, this));
                    continue;
                }
                result.Add(new BoxFolderProxy(_clientContext, (BoxFolder)item, this));
            }

            return result;
        }

        public int GetSubItemsCount()
        {
            return _clientContext.GetFolderItems(Id, 1).TotalCount;
        }

    }
}
