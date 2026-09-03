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
import { spfi, SPFI, SPFx } from "@pnp/sp"; 
import { LogLevel, PnPLogging } from "@pnp/logging";
import { ExtensionContext } from '@microsoft/sp-extension-base';
import "@pnp/sp/webs";
import "@pnp/sp/lists";
import "@pnp/sp/items";
import "@pnp/sp/taxonomy";
import "@pnp/sp/search";
import { PermissionKind } from "@pnp/sp/security";
import { ICustomSearchResult } from "../extensions/relatedRecords/common/ICustomSearchResult";


export default class PnpUtil {
  // private _context: ExtensionContext;
  private _sp: SPFI;
  private _context: ExtensionContext;

  public constructor(context: ExtensionContext) {
    this._context = context;
    this._sp = spfi().using(SPFx(context)).using(PnPLogging(LogLevel.Warning));
  }
  
  public async searchAllSites(queryText: string, pageIndex: number): Promise<{ datas: ICustomSearchResult[]; totalPages: number }> {
    const fullQueryText = `((IsDocument=True AND (FileName:${queryText} OR DlcDocId="${queryText}")) OR (ContentTypeId:0x0100* AND Title:${queryText}))`;
    const results = await this._sp.search({
      Querytext: fullQueryText,
      RowLimit: 10,
      StartRow: pageIndex * 10,
      TrimDuplicates: false,
      SelectProperties: ["UniqueId", "ListItemID", "AuthorOWSUSER", "EditorOWSUSER", "Filename", "Title", "Path", "FileExtension", "SPSiteUrl", "IsDocument"]
    });
    const totalRows = results.TotalRows;
    const totalPages = Math.ceil(totalRows / 10);
    const datas = results.PrimarySearchResults as ICustomSearchResult[];
    return { datas, totalPages };
  }

  public async checkItemHasEditPermission(siteURL: string, listId: string, itemId: number, retryCount = 1): Promise<boolean> {
    let newsp = spfi(siteURL).using(SPFx(this._context)).using(PnPLogging(LogLevel.Warning));
    try {
      const item = newsp.web.lists.getById(listId).items.getById(itemId);
      return await item.currentUserHasPermissions(PermissionKind.EditListItems);
    } catch (error) {
      console.error("Error fetching list item:", error);
      if (retryCount > 0) {
        return await this.checkItemHasEditPermission(siteURL, listId, itemId, retryCount - 1);
      } else {
        if (error?.['odata.error']?.message?.value.indexOf("Item does not exist") !== -1) {
          return true;
        }
        return false;
      }
    }
  }
}


