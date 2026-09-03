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
import { ExtensionContext } from "@microsoft/sp-extension-base";
import { callRecordsSolutionApi } from "../utils/HttpClientUtil";
import { IRelatedRecordSearchResult, ISaveRelatedRecord } from "../extensions/relatedRecords/types/relatedRecord";
import { SearchScopeKey } from "../extensions/relatedRecords/constants/nodeType";

export interface IRelatedRecord {
  ID: string
  UniqueId: string;
  IsDocument: boolean;
  FileName?: string;
  Title?: string;
  Path?: string;
  FileExtension: string;
  ListId?: string;
  WebId?: string;
  WebUrl?: string;
  SiteId?: string;
  ListItemID: string;
  SPSiteUrl: string;
  SourceFlag: number;
}

export class FarmSolutionService {
  private _context: ExtensionContext;

  public constructor(context: ExtensionContext) {
    this._context = context;
  }

  /**
   * Search for related records using the farm solution API
   */
  public async searchRelatedRecords(
    searchText: string,
    currentPage: number,
    sourceOptionKey: SearchScopeKey,
    tokenIndex: string
  ): Promise<IRelatedRecordSearchResult> {
    try {
      const requestData = {
        SiteUrl: this._context.pageContext.site.absoluteUrl,
        WebId: this._context.pageContext.web.id.toString(),
        SearchScope: sourceOptionKey,
        QueryText: searchText,
        PageIndex: currentPage,
        TokenIndex: tokenIndex
      };

      return await callRecordsSolutionApi(
        this._context,
        "/_layouts/15/RelatedRecords/RelatedRecordsHandler.ashx?RequestType=2",
        requestData
      );
    } catch (error) {
      console.error("Error searching related records:", error);
      throw error;
    }
  }

  /**
   * Save related records using the farm solution API
   */
  public async saveRelatedRecords(
    saveData: ISaveRelatedRecord
  ): Promise<any> {
    try {
      return await callRecordsSolutionApi(
        this._context,
        "/_layouts/15/RelatedRecords/RelatedRecordsHandler.ashx?RequestType=1",
        saveData
      );
    } catch (error) {
      console.error("Error saving related records:", error);
      throw error;
    }
  }

  public async checkItemHasEditPermission(
    siteURL: string,
    webId: string,
    listId: string,
    itemId: number
  ): Promise<boolean> {
    try {
      const postData = {
        SiteUrl: siteURL,
        WebId: webId,
        ListId: listId,
        ListItemId: itemId
      };

      return await callRecordsSolutionApi(
        this._context,
        "/_layouts/15/RelatedRecords/RelatedRecordsHandler.ashx?RequestType=3",
        postData
      ); 
    } catch (error) {
      console.error("Error fetching list item:", error);
      throw error;
    }
  }

  public async InitRelatedConfig(
    conifgInfo:{
      OpusIdentityUrl: string,
      OpusWebApiUrl: string,
      ClientId: string,
      ThumbPrint: string
    }
  ): Promise<boolean> {
    try {
      return await callRecordsSolutionApi(
        this._context,
        "/_layouts/15/RelatedRecords/RelatedRecordsHandler.ashx?RequestType=4",
        conifgInfo
      ); 
    } catch (error) {
      console.error("Error fetching list item:", error);
      throw error;
    }
  }

  public async checkIsRecordFromOpus(
    param: {
      SiteUrl: string,
      WebId: string,
      ListId: string,
      ListItemId: string
    }
  ): Promise<boolean> {
    try {
      return await callRecordsSolutionApi(
        this._context,
        "/_layouts/15/RelatedRecords/RelatedRecordsHandler.ashx?RequestType=5",
        param
      ); 
    } catch (error) {
      console.error("Error fetching list item:", error);
      throw error;
    }
  }
}