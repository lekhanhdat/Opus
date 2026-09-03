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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {
        private AveNintexAPIProcessor nintexAPIProcessor;

        [ReplaceByAPI]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">读取NinTexWorkflow文件的stream</param>
        /// <param name="publishName"></param>
        /// <param name="tenant"></param>
        /// <param name="siteServerRelativeUrl"></param>
        /// <param name="listName"></param>
        /// <param name="overWrite"></param>
        public override Guid PublishNintexWorkflow(System.IO.Stream stream, string publishName, string webUrl, string listName, Guid parentListId)
        {
            var workflowId = nintexAPIProcessor.PublishNintexWorkflow(stream, publishName, webUrl, listName, parentListId);
            return new Guid(workflowId);
            //return mRequestCommon.PublishNintexWorkflow(stream, publishName, tenant, siteServerRelativeUrl, listName, overWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webUrl"></param>
        /// <param name="workflowDefinitionId"></param>
        /// <returns></returns>
        [ReplaceByAPI]
        public override Guid PublishNintexWorkflow(string webUrl, Guid workflowDefinitionId)
        {
            var workflowId = nintexAPIProcessor.PublishNintexWorkflow(webUrl, workflowDefinitionId);
            return new Guid(workflowId);
        }

        [ReplaceByAPI]
        public override string ConvertNintexFormJsonObjectToXml(string webUrl, string formJsonData, string fileName)
        {
            return nintexAPIProcessor.ConvertNintexFormJsonObjectToXml(webUrl, formJsonData, fileName);
        }

        [ReplaceByAPI]
        public override string ImportNintexWorkflow(System.IO.Stream stream, string publishName, string webUrl, string listTitle, Guid parentListId, bool migrate)
        {
            return nintexAPIProcessor.ImportNintexWorkflow(stream, publishName, webUrl, listTitle, parentListId, migrate);
        }

        [ReplaceByAPI]
        public override void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId)
        {
            nintexAPIProcessor.SaveNintexForm(formXml, webUrl, listId, contentTypeId);
        }

        [ReplaceByAPI]
        public override void PublishNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            nintexAPIProcessor.PublishNintexForm(webUrl, listId, contentTypeId);
        }

        [ReplaceByAPI]
        public override Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            return nintexAPIProcessor.ExportNintexForm(webUrl, listId, contentTypeId);
        }

    }
}
