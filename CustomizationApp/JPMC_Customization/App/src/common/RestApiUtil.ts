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
import { ExtensionContext } from '@microsoft/sp-extension-base';
import { spfi, SPFx } from "@pnp/sp";
import "@pnp/sp/webs";
import "@pnp/sp/lists";
import "@pnp/sp/items";
import { ICamlQuery } from "@pnp/sp/lists";
import {
  SPHttpClient,
} from "@microsoft/sp-http";
import { Logger } from "../common/Logger";

let unauthorizedAccessRootWeb = false;

export async function getDataByRestApi(context: ExtensionContext, restApiUrl: string): Promise<any> {
  try {
    var response = await context.spHttpClient.get(
      restApiUrl,
      SPHttpClient.configurations.v1,
      {
        headers: [
          ["accept", "application/json;odata=nometadata"],
          ["odata-version", ""],
        ],
      }
    );

    if (response.status == 403) {
      unauthorizedAccessRootWeb = true;
      return "Unauthorized_42B6BB40-BD6B-4B38-9AEE-0B72EB887D3D";
    } else {
      return await response.json();
    }
  } catch (error) {
    Logger.warn(`get data failed by: ${restApiUrl}`);
    throw error;
  }
}

export function isUnauthorizedAccessRootWeb() {
  return unauthorizedAccessRootWeb;
}

const _cacheUsers: any = {};
export async function getUserById(context: ExtensionContext, userId: number) {
  if(!_cacheUsers[userId]) {
    const webUrl = context.pageContext.web.absoluteUrl;
    const endpoint = `${webUrl}/_api/web/getUserById(${userId})`;
    const user = await getDataByRestApi(context, endpoint);
    _cacheUsers[userId] = user;
  }

  return _cacheUsers[userId];
}

const taxonomyFieldInfoes: any = {};
export async function getTaxonomyFieldInfo(
  context: ExtensionContext,
  fieldName: string
): Promise<any> {
  let fieldInfo = taxonomyFieldInfoes[fieldName];
  if (fieldInfo) {
    return fieldInfo;
  }

  try {
    let listId = context.pageContext.list?.id.toString();
    let webUrl = context.pageContext.web.absoluteUrl;
    let res = await context.spHttpClient.get(
      `${webUrl}/_api/web/lists(guid'${listId}')/fields?$filter=InternalName eq '${fieldName}'&$select=TermSetId,AnchorId,TextField`,
      SPHttpClient.configurations.v1,
      {
        headers: [
          ["accept", "application/json;odata=nometadata"],
          ["odata-version", ""],
        ],
      }
    );
    let data = await res.json();
    taxonomyFieldInfoes[fieldName] = data.value && data.value[0];
    return taxonomyFieldInfoes[fieldName];
  } catch (error) {
    Logger.error(error, `get metadata column failed: ${fieldName}`);
  }
  return null;
}

export async function getTaxonomyHiddenFieldName(
  context: ExtensionContext,
  fieldId: string
): Promise<string> {
  try {
    let listId = context.pageContext.list?.id.toString();
    let webUrl = context.pageContext.web.absoluteUrl;
    let res = await context.spHttpClient.get(
      `${webUrl}/_api/web/lists(guid'${listId}')/fields(guid'${fieldId}')?$select=InternalName`,
      SPHttpClient.configurations.v1,
      {
        headers: [
          ["accept", "application/json;odata=nometadata"],
          ["odata-version", ""],
        ],
      }
    );
    let data = await res.json();
    return data && data.InternalName;
  } catch (error) {
    Logger.error(error, `get metadata column failed: ${fieldId}`);
  }
  return '';
}

const termWssIdCache = new Map<string, number>();
const TAXONOMY_HIDDEN_LIST = "TaxonomyHiddenList";
const TAXONOMY_QUERY_BATCH = 2000;
export async function getTermWssId(
  context: ExtensionContext,
  termSetId: string,
  termGuid: string
): Promise<number | undefined> {
  const cacheKey = `${termSetId.toLowerCase()}|${termGuid.toLowerCase()}`;
  if (termWssIdCache.has(cacheKey)) {
    return termWssIdCache.get(cacheKey);
  }

  try {
    const maxId = await getListMaxItemId(context);
    if (maxId < 1) {
      return undefined;
    }

    for (let startId = 1; startId <= maxId; startId += TAXONOMY_QUERY_BATCH) {
      const endId = Math.min(startId + TAXONOMY_QUERY_BATCH - 1, maxId);
      const match = await queryTaxonomyHiddenListRange(context, termSetId, termGuid, startId, endId);
      if (match) {
        termWssIdCache.set(cacheKey, match.ID);
        return match.ID;
      }
    }
  } catch (error) {
    Logger.error(error, `get term WSS ID failed: ${termSetId}, ${termGuid}`);
  }

  return undefined;
}

async function getListMaxItemId(context: ExtensionContext): Promise<number> {
  const webUrl = context.pageContext.web.absoluteUrl;
  const endpoint = `${webUrl}/_api/web/lists/getByTitle('${TAXONOMY_HIDDEN_LIST}')/items?$select=ID&$orderby=ID desc&$top=1`;
  const response = await context.spHttpClient.get(
    endpoint,
    SPHttpClient.configurations.v1,
    {
      headers: [
        ["accept", "application/json;odata=nometadata"],
        ["odata-version", ""],
      ],
    }
  );
  if (!response.ok) {
    throw new Error(`Failed to resolve max ID from TaxonomyHiddenList: ${response.statusText}`);
  }
  const payload = await response.json();
  return payload?.value?.[0]?.ID ?? 0;
}

interface ITaxonomyHiddenListItem {
  ID: number;
  IdForTerm?: string;
  IdForTermSet?: string;
}

async function queryTaxonomyHiddenListRange(
  context: ExtensionContext,
  termSetId: string,
  termGuid: string,
  startId: number,
  endId: number
): Promise<ITaxonomyHiddenListItem | undefined> {
  const caml: ICamlQuery = {
    ViewXml: `
<View>
  <Query>
    <Where>
      <And>
        <Geq>
          <FieldRef Name="ID" />
          <Value Type="Number">${startId}</Value>
        </Geq>
        <And>
          <Leq>
            <FieldRef Name="ID" />
            <Value Type="Number">${endId}</Value>
          </Leq>
          <And>
            <Eq>
              <FieldRef Name="IdForTermSet" />
              <Value Type="Text">${termSetId}</Value>
            </Eq>
            <Eq>
              <FieldRef Name="IdForTerm" />
              <Value Type="Text">${termGuid}</Value>
            </Eq>
          </And>
        </And>
      </And>
    </Where>
  </Query>
  <ViewFields>
    <FieldRef Name="ID" />
    <FieldRef Name="IdForTerm" />
    <FieldRef Name="IdForTermSet" />
  </ViewFields>
  <RowLimit>${TAXONOMY_QUERY_BATCH}</RowLimit>
</View>`
  };

  const sp = spfi().using(SPFx(context));
  const list = sp.web.lists.getByTitle(TAXONOMY_HIDDEN_LIST);
  const items = await list.getItemsByCAMLQuery(caml);
  return items.length > 0 ? (items[0] as ITaxonomyHiddenListItem) : undefined;
}

export async function setRecordLabel(
  context: ExtensionContext,
  itemId: string | number,
  complianceTag: string
): Promise<boolean> {
  if (!complianceTag) {
    return false;
  }
  try {
    let listId = context.pageContext.list?.id.toString();
    let webUrl = context.pageContext.web.absoluteUrl;
    let res = await context.spHttpClient.post(
      `${webUrl}/_api/web/lists(guid'${listId}')/items(${itemId})/SetComplianceTag()`,
      SPHttpClient.configurations.v1,
      {
        headers: [
          ["accept", "application/json;odata=verbose"],
          ["content-type", "application/json;odata=verbose"],
        ],
        body: JSON.stringify({
          complianceTag: complianceTag,
          isTagPolicyHold: true,
          isTagPolicyRecord: true,
          isEventBasedTag: false,
          isTagSuperLock: false,
          isUnlockedAsDefault: false
        }),
      }
    );

    const success = res.status === 200 || res.status === 204;
    //await res.json();
    return success;
  } catch (error) {
    Logger.error(error, `set record label for item failed: ${itemId}`);
  }
  return false;
}
