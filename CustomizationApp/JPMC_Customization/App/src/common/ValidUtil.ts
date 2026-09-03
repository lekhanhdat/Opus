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
import { RowAccessor } from '@microsoft/sp-listview-extensibility';
import { ExtensionContext } from '@microsoft/sp-extension-base';
import * as strings from "OpusCustomizationStrings";
import PnpUtil from './PnpUtil';
import { getCustomColumns } from '../config/AppConfigs';
import { getTaxonomyFieldInfo } from './RestApiUtil';


interface ItemStatus {
  IsRecord: boolean;
  IsHold: boolean;
  IsCheckout: boolean;
  CheckOutUserId: number;
}

export async function notAllowClassify(context: ExtensionContext, pnpUtil: PnpUtil, listId: string, itemRow: RowAccessor, spItem: any, setAsFinal: boolean, selClassCode?: string): Promise<string> {
  // if (itemRow.getValueByName("FSObjType") == 1) {
  //   return strings.JPMC_App_Msg_Classify_UnsupportedObj;
  // }

  if (!spItem) {
    return strings.JPMC_App_Msg_Classify_Error;
  }

  let hasPermission = await pnpUtil.hasEditPermissions(listId, parseInt(itemRow.getValueByName("ID")));
  if (!hasPermission) {
    return strings.JPMC_App_Msg_Action_NoPermission;
  }

  let itemStatus = getItemStatus(spItem);
  if (itemStatus.IsHold) {
    return strings.JPMC_App_Msg_Classify_Hold;
  } else if (itemStatus.IsCheckout && context.pageContext.legacyPageContext && context.pageContext.legacyPageContext.userId != itemStatus.CheckOutUserId) {
    return strings.JPMC_App_Msg_Classify_NotCheckoutUser;
  } else if (itemStatus.IsCheckout && setAsFinal) {
    return strings.JPMC_App_Msg_Classify_Checkout_Final;
  }

  return "";
}

// ...existing code...
const officeExtensions = new Set([
  ".doc", ".docx", ".dot", ".dotx",
  ".xls", ".xlsx", ".xlsm", ".xltx",
  ".ppt", ".pptx", ".ppsx",
  ".vsd", ".vsdx",
  ".pub",
]);
export function isOfficeFile(fileName: string): boolean {
  if (!fileName) {
    return false;
  }
  const ext = fileName.substring(fileName.lastIndexOf(".")).toLowerCase();
  return officeExtensions.has(ext);
}

export function itemIsRecord(spItem: any): boolean {
  let itemStatus = getItemStatus(spItem);
  return itemStatus.IsRecord;
}

export function getItemStatus(spItem: any): ItemStatus {
  let itemStatus: ItemStatus = {
    IsRecord: false,
    IsHold: false,
    IsCheckout: false,
    CheckOutUserId: spItem.CheckoutUserId || spItem.CheckedOutUserId || 0
  };
  let status = spItem.Properties.OData__x005f_vti_x005f_ItemHoldRecordStatus || spItem.Properties.Hold_x0020_and_x0020_Record_x0020_Status;
  if (status && status > 0) {
    if ((status & 0x1000) != 0) {
      itemStatus.IsHold = true;
    }
    if ((status & 0x10) != 0) {
      itemStatus.IsRecord = true;
    }
  }

  itemStatus.IsCheckout = !itemStatus.IsHold && !itemStatus.IsRecord && itemStatus.CheckOutUserId > 0;

  return itemStatus;
}

export function getSPColumnValue(columnName: string, spItem: any) {
  let colValue = spItem[columnName];
  if (!colValue && spItem.Properties) {
    colValue = spItem.Properties[columnName];
  }
  return colValue;
}
