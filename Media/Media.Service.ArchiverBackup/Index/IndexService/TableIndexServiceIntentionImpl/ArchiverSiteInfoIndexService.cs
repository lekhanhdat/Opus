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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public class ArchiverSiteInfoIndexService : ArchiverTableIndexServiceBase, IArchiverSiteInfoIndexService
    {
        public void UpdateSiteGuid(string guid)
        {
            string sql = "delete from " + IndexConstants.TableNameArchiveSiteInfo;
            IndexProcessor.Execute(sql, default(Dictionary<String, Object>));
            sql = "insert into " + IndexConstants.TableNameArchiveSiteInfo + " (COL_GUID) values (@COL_GUID)";
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_GUID"] = guid;
            IndexProcessor.Execute(sql, parameterDictionary);
        }
    }
}