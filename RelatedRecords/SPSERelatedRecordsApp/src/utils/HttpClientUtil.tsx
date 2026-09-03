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

export async function callRecordsSolutionApi(
  context: ExtensionContext,
  relativeApiUrl: string,
  postData: any
) {
  var digest = await getDigest(context);
  const webUrl = context.pageContext.web.absoluteUrl;
  const headers = {
    'content-type': 'application/json;charset=utf-8',
    'accept': 'application/json',
	  'X-RequestDigest': digest
  };

  var response = await fetch(`${webUrl.replace(/\/$/, "")}${relativeApiUrl}`, {
    method: 'POST',
    headers,
    body: JSON.stringify(postData)
  });

  if (response.ok) {
    const data = await response.json();
    return data;
  } else {
    throw new Error(response.statusText);
  }
}

const digestCache = { value: null, expiresAt: 0, webUrl: null };
async function getDigest(context: ExtensionContext) {
  const domEle = document.getElementById("__REQUESTDIGEST");
  const domVal = domEle ? domEle["value"] : null;
  if (domVal) return domVal;

  const webUrl = context.pageContext.web.absoluteUrl;

  if (digestCache.value && digestCache.webUrl === webUrl && Date.now() < digestCache.expiresAt) {
    return digestCache.value;
  }

  const resp = await fetch(webUrl.replace(/\/$/, "") + "/_api/contextinfo", {
    method: "POST",
    headers: { "Accept": "application/json;odata=nometadata" },
    credentials: "same-origin"
  });
  if (!resp.ok) throw new Error(await resp.text());
  const j = await resp.json();

  const d = j.FormDigestValue ? j : (j.d && j.d.GetContextWebInformation) ? j.d.GetContextWebInformation : null;
  if (!d) throw new Error("Unexpected contextinfo response.");

  const value = d.FormDigestValue;
  const timeoutSec = d.FormDigestTimeoutSeconds || 1500;
  digestCache.webUrl = webUrl;
  digestCache.value = value;
  digestCache.expiresAt = Date.now() + (timeoutSec * 1000) - 5000;
  return value;
}

