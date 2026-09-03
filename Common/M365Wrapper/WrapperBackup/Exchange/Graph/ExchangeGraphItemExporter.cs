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
#nullable enable
using AvePoint.RA.CommonUtil;
using Microsoft365.Graph.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangeBackupUtility.Graph;
//for large item
public class ExchangeGraphItemExporter(string mailboxId, GraphService service, string tempFolder) : IExchangeItemExporter
{
    public Dictionary<string, ExportItemResult> ExportItems(List<string> ids)
    {
        var id = ids.Single();
        var result = ExportItemAsync(id).ExecuteAsyncTask();
        return new Dictionary<string, ExportItemResult> { { id, result } };
    }

    public async Task<ExportItemResult> ExportItemAsync(string id)
    {
        using var stream = await service.Mails.ExportImport.ExportItemAsStreamAsync(mailboxId.ThrowIfNullOrEmpty(), id.ThrowIfNullOrEmpty().ToRestId());
        return await ToResult(id, stream);
    }
    private async Task<ExportItemResult> ToResult(string itemId, Stream stream)
    {
        var filePath = Path.Combine(tempFolder, Guid.NewGuid().ToString() + ".fts");
        using (var dest = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
        {
            await stream.CopyToAsync(dest);
        }
        return ExportItemResult.CreateSuccessfulResult(itemId, filePath);
    }
}