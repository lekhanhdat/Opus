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
  IHttpClientOptions,
  AadHttpClient,
  HttpClientResponse
} from "@microsoft/sp-http";
import { ExtensionContext } from '@microsoft/sp-extension-base';
import { getAosLoginAppId, getOpusApiUrl, getAoscustomerId } from "../config/AppConfigs";


export function callRecordsApi(context: ExtensionContext, relativeApiUrl:string, postData: any) {
  let requestHeaders: Headers = new Headers();
  requestHeaders.append("Content-type", "application/json");
  requestHeaders.append("Accept", "application/json");
  requestHeaders.append("Token-Source", "SpfxOAuth");
  requestHeaders.append("Customer-Id", getAoscustomerId());
  let httpClientOptions: IHttpClientOptions = {
    method: "POST",
    mode: "cors",
    headers: requestHeaders,
    body: JSON.stringify(postData),
  };
  return context.aadHttpClientFactory
    .getClient(getAosLoginAppId())
    .then((client: AadHttpClient): Promise<HttpClientResponse | any> => {
      return client
        .post(`${getOpusApiUrl()}${relativeApiUrl}`, AadHttpClient.configurations.v1, httpClientOptions)
        .then((response: HttpClientResponse) => {
          if (response.ok) {
            return response.json();
          } else {
            throw new Error(response.statusText)
          }
        })
        .then((data): any => {
          console.log(data);
          return data;
        }).catch(error => {
          console.log(error);
          throw error;
        });
    });
}
