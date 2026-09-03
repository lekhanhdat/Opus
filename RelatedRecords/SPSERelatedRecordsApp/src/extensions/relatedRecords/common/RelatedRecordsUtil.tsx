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
import { ENodeType } from "../constants/nodeType";
import { IRelatedInfo, IRelatedRecordItem } from "../types/relatedRecord";

export function getRelatedRecordsFromXmlData(xmlString: any): any[] {
  const relatedRecordsList: any[] = [];
  const xmlParser = new DOMParser();
  const xmlDoc = xmlParser.parseFromString(xmlString, "application/xml");
  const elements = xmlDoc.getElementsByTagName("a");
  for (let index = 0; index < elements.length; index++) {
    const element = elements[index];
    const relatedObjString = element.getAttribute("rel") || "";
    const relatedObj = JSON.parse(relatedObjString);
    relatedRecordsList.push(relatedObj);
  }
  return relatedRecordsList;
}

export function removeCurlyBraces(value: string): string {
  if (value) {
    return value.replace(/[{}]/g, "");
  }
  return "";
}

export function checkRelatedRecordsEqual(
  relatedInfos: IRelatedInfo[],
  existedRelatedRecords: IRelatedRecordItem[]
): boolean {
  if (relatedInfos.length !== existedRelatedRecords.length) {
    return false;
  }

  for (let i = 0; i < relatedInfos.length; i++) {
    const relatedInfo = relatedInfos[i];
    const existedRelatedRecord = existedRelatedRecords[i];

    if (
      relatedInfo.SiteId !== existedRelatedRecord.siteId ||
      relatedInfo.UniqueId !== existedRelatedRecord.uniqueId ||
      relatedInfo.NeedDelete !== existedRelatedRecord.needDelete
    ) {
      return false;
    }
  }

  return true;
}

export function getFileIcon(docType?: string) {
  let fileType = "folder";
  if (docType) {
    if (docType.indexOf(".") !== -1) {
      
      fileType = docType.split(".")[1];
    } else {
      if (docType === "aspx") {
        fileType = "spo";
      } else {
        fileType = docType;
      }
    }
  }

  return {
    docType,
    url: `/_layouts/15/next/odspnext/fluentui-resources/item-types/20/${fileType}.png`,
  };
}

export function getPhysicalIcons(nodeType: ENodeType) {
   let iconName = "folder";
   if(nodeType == ENodeType.PhyRecord){
      iconName = "genericFile"
   }

   return `/_layouts/15/next/odspnext/fluentui-resources/item-types/20/${iconName}.png`
}
