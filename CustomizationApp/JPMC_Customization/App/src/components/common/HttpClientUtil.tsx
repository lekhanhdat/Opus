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
import {
  SPHttpClient,
  SPHttpClientResponse,
  IHttpClientOptions,
  AadHttpClient,
  HttpClientResponse
} from "@microsoft/sp-http";
import { ExtensionContext } from '@microsoft/sp-extension-base';
import { getAosLoginAppId, getCSDApiUrl } from "../../config/AppConfigs";

export function getTaxonomyFieldInfo(
  context: ExtensionContext,
  fieldName: string
): Promise<any> {
  let listId = context.pageContext.list?.id.toString();
  let webUrl = context.pageContext.web.absoluteUrl;
  return context.spHttpClient
    .get(
      `${webUrl}/_api/web/lists(guid'${listId}')/fields?$filter=InternalName eq '${fieldName}'&$select=DefaultValue,AllowMultipleValues,Required,TextField,TermSetId,AnchorId`,
      SPHttpClient.configurations.v1,
      {
        headers: [
          ["accept", "application/json;odata=nometadata"],
          ["odata-version", ""],
        ],
      }
    )
    .then((res: SPHttpClientResponse) => {
      return res.json();
    }).catch(error => {
      // throw new Error("An error occurred while retrieving data from the CSD class tree.");
      throw error;
    });
}

export function getTaxonomyTextFieldName(
  context: ExtensionContext,
  fieldId: string
): Promise<{ InternalName: string }> {
  let listId = context.pageContext.list?.id.toString();
  let webUrl = context.pageContext.web.absoluteUrl;
  return context.spHttpClient
    .get(
      `${webUrl}/_api/web/lists(guid'${listId}')/fields(guid'${fieldId}')?$select=InternalName`,
      SPHttpClient.configurations.v1,
      {
        headers: [
          ["accept", "application/json;odata=nometadata"],
          ["odata-version", ""],
        ],
      }
    )
    .then(
      (res: SPHttpClientResponse): Promise<{ InternalName: string }> => {
        return res.json();
      }
    );
}

export function getFieldValue(
  context: ExtensionContext,
  fieldName: string,
  itemId: string
): Promise<any> {
  let listId = context.pageContext.list?.id.toString();
  let webUrl = context.pageContext.web.absoluteUrl;
  return context.spHttpClient
    .get(
      `${webUrl}/_api/web/lists(guid'${listId}')/items(${itemId})/${fieldName}`,
      SPHttpClient.configurations.v1,
      {
        headers: [
          ["accept", "application/json;odata=nometadata"],
          ["odata-version", ""],
        ],
      }
    )
    .then((res: SPHttpClientResponse) => {
      return res.json();
    })
    .then((data): Promise<any> => {
      return data;
    });
}

export function callRecordsApi(context: ExtensionContext, relativeApiUrl:string, postData: any) {
  let requestHeaders: Headers = new Headers();
  requestHeaders.append("Content-type", "application/json");
  requestHeaders.append("Accept", "application/json");
  requestHeaders.append("Access-Control-Allow-Origin", "*");
  let httpClientOptions: IHttpClientOptions = {
    method: "POST",
    mode: "cors",
    credentials: "include",
    headers: requestHeaders,
    body: JSON.stringify(postData),
  };
  return context.aadHttpClientFactory
    .getClient(getAosLoginAppId())
    .then((client: AadHttpClient): Promise<HttpClientResponse | any> => {
      return client
        .post(`${getCSDApiUrl()}${relativeApiUrl}`, AadHttpClient.configurations.v1, httpClientOptions)
        .then((response: HttpClientResponse) => {
          if (response.ok) {
            return response.json();
          } else {
            throw new Error(response.statusText)
          }
        })
        .then((data): any => {
          // console.log(data);
          return data;
        }).catch(error => {
          console.log(error);
          throw error;
        });
    });
}
