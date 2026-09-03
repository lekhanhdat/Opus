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
import { SPHttpClient, ISPHttpClientOptions } from "@microsoft/sp-http";
import "@pnp/sp/webs";
import "@pnp/sp/lists";
import "@pnp/sp/fields";
import "@pnp/sp/items";
import "@pnp/sp/taxonomy";
import { ITerm, ITermInfo } from "@pnp/sp/taxonomy";
import { ICamlQuery, IListItemFormUpdateValue, IRenderListDataParameters } from "@pnp/sp/lists";
import { IClassCode } from "../model/IClassCodeConfig";
import { PermissionKind } from "@pnp/sp/security";
import { Logger } from "./Logger";
import { EMPTY_GUID } from "./Constants";
import { equalIgnoreCase } from "./StringUtil";
import { ITaxonomyFieldValueInfo } from "../model/ITaxonomyFieldValueInfo";
import { dateToString } from "./DateUtil";

let _termsCache: Map<string, Map<string, ITermInfo>> = new Map(); // key: TermSetId|AnchorId
const getFoldersPagerSize = 200;


export default class PnpUtil {
  private _context: ExtensionContext;
  private _sp: SPFI;

  public constructor(context: ExtensionContext) {
    this._context = context;
    this._sp = spfi().using(SPFx(context)).using(PnPLogging(LogLevel.Warning));
  }

  public async updateItem(listId: string, itemId: number, formValues: IListItemFormUpdateValue[]): Promise<void> {
    const list = this._sp.web.lists.getById(listId);

    await list.items.getById(itemId).validateUpdateListItem(formValues, true);
  }

  public async updateItem1(listId: string, itemId: number, formValues: IListItemFormUpdateValue[], classCodeInfo: ITaxonomyFieldValueInfo): Promise<void> {
    let objectId = 11;
    const body = `<Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName=".NET Library" xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
  <Actions>
    <ObjectPath Id="2" ObjectPathId="1" />
    <ObjectPath Id="4" ObjectPathId="3" />
    <ObjectPath Id="6" ObjectPathId="5" />
    <ObjectPath Id="8" ObjectPathId="7" />
    <ObjectPath Id="10" ObjectPathId="9" />
    <Method Name="SetFieldValue" Id="${objectId++}" ObjectPathId="9">
      <Parameters>
        <Parameter Type="String">${classCodeInfo.HiddenColumnInternalName}</Parameter>
        <Parameter Type="String">${classCodeInfo.TermGuid}</Parameter>
      </Parameters>
    </Method>
    <Method Name="SetFieldValue" Id="${objectId++}" ObjectPathId="9">
      <Parameters>
        <Parameter Type="String">${classCodeInfo.ColumnInternalName}</Parameter>
        <Parameter Type="String">${classCodeInfo.WssId};#${classCodeInfo.TermLabel}|${classCodeInfo.TermGuid}</Parameter>
      </Parameters>
    </Method>
    ${formValues.map((d) => {
      const isEmptyVal = d.FieldValue === null || d.FieldValue === undefined || d.FieldValue === '';
      return `
    <Method Name="SetFieldValue" Id="${objectId++}" ObjectPathId="9">
      <Parameters>
        <Parameter Type="String">${d.FieldName}</Parameter>
        <Parameter ${isEmptyVal ? 'Type="Null" />' : (`Type="String" >${d.FieldValue}</Parameter>`)}
      </Parameters>
    </Method>`;
    }).join('')}
    <Method Name="SystemUpdate" Id="${objectId++}" ObjectPathId="9" />
  </Actions>
  <ObjectPaths>
    <StaticProperty Id="1" TypeId="{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}" Name="Current" />
    <Property Id="3" ParentId="1" Name="Web" />
    <Property Id="5" ParentId="3" Name="Lists" />
    <Method Id="7" ParentId="5" Name="GetById">
      <Parameters>
        <Parameter Type="Guid">{${listId}}</Parameter>
      </Parameters>
    </Method>
    <Method Id="9" ParentId="7" Name="GetItemById">
      <Parameters>
        <Parameter Type="Int32">${itemId}</Parameter>
      </Parameters>
    </Method>
  </ObjectPaths>
</Request>`;

    const endpoint = `${this._context.pageContext.web.absoluteUrl}/_vti_bin/client.svc/ProcessQuery`;

    return this._context.spHttpClient.post(
      endpoint,
      SPHttpClient.configurations.v1,
      {
        headers: {
          'Accept': '*/*',
          'Content-Type': 'text/xml;charset="UTF-8"',
          'X-Requested-With': 'XMLHttpRequest'
        },
        body
      })
      .then((r) => r.json())
      .then((r) => {
        if (r[0].ErrorInfo) {
          throw new Error(r[0].ErrorInfo.ErrorMessage);
        }
        return r;
      });
  }

  public async hasEditPermissions(listId: string, itemId: number) {
    let item = this._sp.web.lists.getById(listId).items.getById(itemId);
    return await item.currentUserHasPermissions(PermissionKind.EditListItems);
  }

  public async hasManageListsPermissions(listId: string, itemId: number) {
    let item = this._sp.web.lists.getById(listId).items.getById(itemId);
    return await item.currentUserHasPermissions(PermissionKind.ManageLists);
  }

  public async getItem(listId: string, itemId: number) {
    return await this._sp.web.lists.getById(listId).items.getById(itemId).expand("Properties")();
  }

  public async hasColumns(listId: string, columnInternalNames: string[]): Promise<boolean> {
    if (!columnInternalNames || columnInternalNames.length === 0) {
      return true;
    }

    const fields = await this._sp.web.lists.getById(listId).fields.select("InternalName")();
    const existingColumnInternalNames = new Set(fields.map((field) => field.InternalName.toLocaleLowerCase()));

    return columnInternalNames.every((columnInternalName) => existingColumnInternalNames.has(columnInternalName.toLocaleLowerCase()));
  }

  public async getItems(listId: string, itemIDs: number[], fields: string[]) {
    return await this.getItemsPaged(listId, itemIDs, fields, 50);
  }
  private async getItemsPaged(listId: string, itemIDs: number[], fields: string[], pageSize: number) {
    const allItems = [];
    for (let index = 0; index < itemIDs.length; index += pageSize) {
      let items = await this.queryItems(listId, itemIDs.slice(index, Math.min(index + pageSize, itemIDs.length)), fields);
      if (items) {
        for (const item of items) {
          allItems.push(item);
        }
      }
    }

    return allItems;
  }
  private async queryItems(listId: string, itemIDs: number[], fields: string[]) {
    let list = this._sp.web.lists.getById(listId);

    let tempFields = [];
    tempFields.push('Id');
    tempFields.push(...fields);

    const camlQuery: ICamlQuery = {
      DatesInUtc: true,
      ViewXml:
`<View Scope="RecursiveAll">
  <Query><Where><In><FieldRef Name="ID"/><Values>
    <Value Type="Integer">${itemIDs.join("</Value><Value Type=\"Integer\">")}</Value>
  </Values></In></Where></Query>
  <ViewFields>
    <FieldRef Name="${tempFields.join("\"></FieldRef><FieldRef Name=\"")}"></FieldRef>
  </ViewFields>
</View>`
    };

    return await list.getItemsByCAMLQuery(camlQuery, 'Properties');
  }

  public getItemDateFieldValue(spItem: any, fieldName: string) : Date | null {
    let value = spItem[fieldName];
    if (value) {
      return new Date(value);
    }
    return null;
  }

  public getItemTaxonomyFieldValue(spItem: any, fieldName: string) : IClassCode | undefined {
    let value = spItem[fieldName];
    if (value) {
      return { termId: value.TermGuid, termLabel: value.Label };
    }
    return undefined;
  }

  public async getTermsFromTermStore(termSetId: string, anchorId: string): Promise<Map<string, ITermInfo>> {
    let cacheKey = `${termSetId}|${anchorId}`;
    let terms = _termsCache.get(cacheKey);
    if (!terms) {
      let results = await this.getAllTerms(termSetId, anchorId);
      terms = new Map<string, ITermInfo>();
      for (const term of results) {
        terms.set(term.id.toLocaleLowerCase(), term);
      }
      _termsCache.set(cacheKey, terms);
    }

    return terms;
  }

  private async getAllTerms(termSetId: string, anchorId: string): Promise<ITermInfo[]> {
    try {
      let isTermScope = anchorId && anchorId != EMPTY_GUID;
      let termStore = this._sp.termStore;
      let termSet = termStore.sets.getById(termSetId);
      let terms = await termSet.getAllChildrenAsOrderedTree({ retrieveProperties: true });
      if (isTermScope) {
        let rootTerm = this.findChildTerm(terms, anchorId);
        if (!rootTerm) {
          return [];
        }

        return this.getAllTermChildren(termSetId, rootTerm.children);
      }
      else {
        return this.getAllTermChildren(termSetId, terms);
      }

    } catch (error) {
      Logger.error(error, `get all terms failed: ${termSetId}|${anchorId}`);
    }
    return [];
  }

  private getAllTermChildren(termSetId: string, children?: ITermInfo[] | null) : ITermInfo[] {
    let results: ITermInfo[] = [];
    if (children && children.length > 0) {
      for (const term of children) {
        if (!term.isDeprecated) {
          if (term.isAvailableForTagging && term.isAvailableForTagging.filter(t => equalIgnoreCase(t.setId, termSetId) && t.isAvailable).length > 0) {
            results.push(term);
          }
          results = results.concat(this.getAllTermChildren(termSetId, term.children));
        }
      }
    }
    return results;
  }

  private findChildTerm(children: ITermInfo[], termId: string) : ITermInfo | null {
    for (const term of children) {
      if (equalIgnoreCase(term.id, termId)) {
        return term;
      }
      if (term.children && children.length > 0) {
        let childTerm = this.findChildTerm(term.children, termId);
        if (childTerm) {
          return childTerm;
        }
      }
    }

    return null;
  }

  public async getFolder(listId: string, folderUrl: string) {
    let pagedItems = await this._sp.web.lists.getById(listId).items.filter(`FileRef eq '${folderUrl}'`).getPaged();
    if (pagedItems.results?.length > 0) {
      return pagedItems.results[0];
    }
    return null;
  }

  public async getParentFolder(listId: string, itemId: number) {
    let parentUrl = await this._sp.web.lists.getById(listId).items.getById(itemId).select("FileDirRef")();
    return parentUrl;
  }

  public isChannelFolderForSPListItem(item: any) {
    return item.getValueByName("FSObjType") == 1 && item.getValueByName("HTML_x0020_File_x0020_Type") == "Team.Channel";
  }

  public async getItemsInFolder(context: any, includeSubFolder: boolean, folderRelativeUrl: string, fields?: string[]): Promise<any[]> {
    const topFolderRelativeUrl = folderRelativeUrl;
    let queryResults = await this.renderListItemDataPaged(context, folderRelativeUrl, includeSubFolder, topFolderRelativeUrl, fields);
    let allItems = queryResults.items;
    if(!queryResults.recursiveList && includeSubFolder) {
      const folders = await this.getChildFolders(context, folderRelativeUrl);

      if (folders && folders.length > 0) {
        let folderIndex = 0;
        do {
          let folderItem = folders[folderIndex];
          allItems.push(folderItem);
          queryResults = await this.renderListItemDataPaged(context, folderItem.FileRef, true, topFolderRelativeUrl, fields);

          if(queryResults.recursiveList) {
            allItems = queryResults.items;
            break;
          }

          allItems = allItems.concat(queryResults.items);

          folderIndex++;

        } while (folderIndex < folders.length);
      }
    }

    return allItems;
  }

  private async getChildFolders(context: any, folderRelativeUrl: string) {
    let folderIDs = await this.getAllChildFolderIDs(context, folderRelativeUrl);
    let folderItems = await this.getListItems(context, folderRelativeUrl, folderIDs);
    return folderItems;
  }

  private async getAllChildFolderIDs(context: any, folderRelativeUrl: string): Promise<any[]> {
    let folderIDs = [];
    let childFolders = await this.getChildFoldersData(context, folderRelativeUrl);
    if (childFolders) {
      for (const item of childFolders) {
        folderIDs.push(item.ListItemAllFields.Id);
        if(item.ItemCount <= 0) {
          continue;
        }
        let subFolderUrl = item.ServerRelativeUrl;
        let grandFoldersIDs = await this.getAllChildFolderIDs(context, subFolderUrl);
        for (const folderItemId of grandFoldersIDs) {
          folderIDs.push(folderItemId);
        }
      }
    }

    return folderIDs;
  }

  //Recursive Files only
  public getListItems(context: any, folderRelativeUrl: string, ids: any[], fields?: string[]): Promise<any> {
    return this.getListItemsPaged(context, folderRelativeUrl, ids, 0, 50, [], fields);
  }

  private getViewFields(fields?: string[]): string[] {
    let tempFields = [];
    if(fields && fields.length > 0) {
      tempFields.push(...fields);
    }
    tempFields.push('FileDirRef');
    tempFields.push('FileLeafRef');
    tempFields.push('File_x0020_Type');
    tempFields.push('HTML_x0020_File_x0020_Type');
    tempFields.push('Editor');

    tempFields.push('_vti_ItemHoldRecordStatus');
    tempFields.push('_vti_ItemDeclaredRecord');
    tempFields.push('CheckedOutUserId');
    tempFields.push('FolderChildCount');
    tempFields.push('_ComplianceTag');

    return tempFields;
  }

  private async getListItemsPaged (context: any, folderRelativeUrl: string, ids: any[], start: number, pager: number, results: any[], fields?: string[]): Promise<any> {
    let end = Math.min(start + pager, ids.length);
    let tempFields = this.getViewFields(fields);

    const ViewXml =`<View Scope="RecursiveAll"><Query><Where><In><FieldRef Name="ID"/><Values>
      <Value Type="Integer">${ids.slice(start, end).join("</Value><Value Type=\"Integer\">")}</Value>
    </Values></In></Where></Query><ViewFields><FieldRef Name="${tempFields.join("\"></FieldRef><FieldRef Name=\"")}"></FieldRef></ViewFields></View>`

    // if(folderRelativeUrl && !/#|%/.test(folderRelativeUrl)) {
    //   camlQuery.FolderServerRelativeUrl = folderRelativeUrl;
    // }

    const renderListDataParams: IRenderListDataParameters = {
      ViewXml: ViewXml,
    };

    const queryResult = await this._sp.web.lists.getById(context.pageContext.list.id.toString()).renderListDataAsStream(renderListDataParams);

    const resultsList = [];

    for (const row of queryResult.Row) {
      resultsList.push(row);
      if (row.RevIMBCS) { row.RevIMBCS.TermGuid = row.RevIMBCS.TermID }
      row.Folder = { ProgID: row.ProgId };
      row.Properties = {
        OData__x005f_vti_x005f_ItemHoldRecordStatus: row._vti_ItemHoldRecordStatus,
        OData__x005f_vti_x005f_ItemDeclaredRecord: row["_vti_ItemDeclaredRecord."] };
      row.Created = row["Created."];
      row.RevIMDeletionDate = row["RevIMDeletionDate."];
      row.RevIMEventDate = row["RevIMEventDate."];
      row.FileSystemObjectType = row.FSObjType;
      row.HTML_x0020_File_x0020_Type = row.File_x0020_Type || row.ProgId;
    }

    return queryResult.Row;
  }


  private async getChildFoldersData(context: any, folderRelativeUrl: string, findedFolders?: any[]): Promise<any[]> {
    findedFolders = findedFolders || [];
    let skipCount = findedFolders.length;
    let webUrl = context.pageContext.web.absoluteUrl;
    let res = await context.spHttpClient
      .get(
        `${webUrl}/_api/web/GetFolderByServerRelativeUrl('${folderRelativeUrl}')/Folders?$skip=${skipCount}&$top=${getFoldersPagerSize}&$expand=ListItemAllFields`,
        SPHttpClient.configurations.v1,
        {
          headers: [
            ["accept", "application/json;odata=nometadata"],
            ["odata-version", ""],
          ],
        }
      );
    let data = await res.json();
    if (data.value) {
      for (const item of data.value) {
        findedFolders.push(item);
      }

      if(data.value.length >= getFoldersPagerSize) {
        return await this.getChildFoldersData(context, folderRelativeUrl, findedFolders);
      }
    }

    return findedFolders;
  }

  // if recursiveList == false => return all child files in the folder, exclude the files in the sub folders
  // if recursiveList == true && includeAllIfRecursiveList == false => return all child files in the folder, exclude the files in the sub folders
  // if recursiveList == true && includeAllIfRecursiveList == true => return all files and folders in the top folder
  // if topFolderRelativeUrl is not set, it will use the folderRelativeUrl as the top folder
  private async renderListItemDataPaged(
    context: any, folderRelativeUrl: string, includeAllIfRecursiveList: boolean, topFolderRelativeUrl?: string, fields?: string[]
  ): Promise<IListItemsResult> {
    if (!topFolderRelativeUrl) {
      topFolderRelativeUrl = folderRelativeUrl;
    }

    let recursiveList = /#|%/.test(folderRelativeUrl);
    let tempFields = this.getViewFields(fields);

    let queryScope = recursiveList ? "RecursiveAll" : "FilesOnly";
    const camlQueryXml = `
      <View Scope="${queryScope}">
        <ViewFields>
          <FieldRef Name="${tempFields.join("\"></FieldRef><FieldRef Name=\"")}"></FieldRef>
        </ViewFields>
        <RowLimit Paged="TRUE">1000</RowLimit>
      </View>`;

    const resultsList = [];
    let pagingToken: string | undefined = undefined;
    do {
      let renderListDataParams: IRenderListDataParameters = {
        ViewXml: camlQueryXml,
        Paging: pagingToken
      };

      if (!recursiveList) {
        renderListDataParams.FolderServerRelativeUrl = folderRelativeUrl;
      }

      const queryResult = await this._sp.web.lists.getById(context.pageContext.list.id.toString()).renderListDataAsStream(renderListDataParams);
      for (const row of queryResult.Row) {
        if(recursiveList && row.FileDirRef != topFolderRelativeUrl && (!includeAllIfRecursiveList || row.FileDirRef.indexOf(`${topFolderRelativeUrl}/`) != 0)) {
          continue;
        }

        if(!recursiveList && row.FSObjType == 1) {
          continue;
        }

        resultsList.push(row);
        if (row.RevIMBCS) { row.RevIMBCS.TermGuid = row.RevIMBCS.TermID }
        row.Folder = { ProgID: row.ProgId };
        row.Properties = {
          OData__x005f_vti_x005f_ItemHoldRecordStatus: row._vti_ItemHoldRecordStatus,
          OData__x005f_vti_x005f_ItemDeclaredRecord: row["_vti_ItemDeclaredRecord."] };
        row.Created = row["Created."];
        row.RevIMDeletionDate = row["RevIMDeletionDate."];
        row.RevIMEventDate = row["RevIMEventDate."];
        row.FileSystemObjectType = row.FSObjType;
        row.HTML_x0020_File_x0020_Type = row.File_x0020_Type || row.ProgId;
      }

      if (queryResult.NextHref) {
        pagingToken = queryResult.NextHref.split('?')[1];
      } else {
        pagingToken = undefined;
      }
    } while (pagingToken);

    return {
      items: resultsList,
      recursiveList: recursiveList
    };
  }
}


interface IListItemsResult {
  items: any[];
  recursiveList: boolean;
}


