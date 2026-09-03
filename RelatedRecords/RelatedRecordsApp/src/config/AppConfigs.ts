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

import packagedTime from "../config/PackagedInfo";
const _replaceConfigs = {
  opusApiUrl: "https://10.1.70.70:44310/",
  aosLoginAppId: "c4763714-72c1-4746-a68e-a17bcf7ad292",
  aosCustomerId: ""
};

const localWin: any = window;
localWin.__opus_app_package_info = {
  packagedTime: packagedTime,
};


export function getOpusApiUrl() {
  if (!_replaceConfigs.opusApiUrl) {
    throw new Error("opusApiUrl is null");
  }
  return _replaceConfigs.opusApiUrl.trim().replace(/\/+$/, '');
}

export function getAosLoginAppId() {
  if (!_replaceConfigs.aosLoginAppId) {
    throw new Error("AosLoginAppId is null");
  }
  return _replaceConfigs.aosLoginAppId.trim();
}

export function getAoscustomerId() {
  return _replaceConfigs.aosCustomerId.trim();
}

