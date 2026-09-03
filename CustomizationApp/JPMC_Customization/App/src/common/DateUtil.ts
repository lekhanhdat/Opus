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
import { Logger } from './Logger';


interface DataInfo {
  Day: number;
  DayOfWeek: number;
  Hour: number;
  Milliseconds: number;
  Minute: number;
  Month: number;
  Second: number;
  Year: number;
}

function getTimeZoneInfo(context: ExtensionContext) {
  let timeZoneInfo = null;
  let pageContext = context.pageContext.legacyPageContext;
  if (pageContext) {
    if (pageContext.preferUserTimeZone) {
      timeZoneInfo = pageContext.userTimeZoneData;
    }
    if (!timeZoneInfo) {
      timeZoneInfo = pageContext.webTimeZoneData;
    }
  }
  return timeZoneInfo;
}
function convertDateByInfo(dateInfo: DataInfo, today: Date): Date {
  if (dateInfo) {
    let date = new Date(
      today.getFullYear(), dateInfo.Month - 1, 1,
      dateInfo.Hour, dateInfo.Minute, dateInfo.Second, dateInfo.Milliseconds
    );
    let day = date.getDay();
    date.setDate(day == 0 ? 8 : 15 - day);
    return date;
  } else {
    return new Date(today.getTime());
  }
}
function isDaylightDate(webTimeZoneInfo: any, spDate: Date): boolean {
  let daylightStart = convertDateByInfo(webTimeZoneInfo.DaylightDate, spDate);
  let daylightEnd = convertDateByInfo(webTimeZoneInfo.StandardDate, spDate);
  return daylightStart <= spDate && spDate < daylightEnd;
}
function getOffsetMinutes(spTimeZoneInfo: any, date: Date): number {
  let offsetMinutes = date.getTimezoneOffset() - spTimeZoneInfo.Bias;
  if (isDaylightDate(spTimeZoneInfo, date)) {
    offsetMinutes -= spTimeZoneInfo.DaylightBias;
  }
  return offsetMinutes;
}

export function getSPToday(context: ExtensionContext) {
  let today = new Date();
  return localDateToSpDate(context, today);
}

export function getDateOnlySPToday(context: ExtensionContext) {
  let today = getSPToday(context);
  return new Date(today.getFullYear(), today.getMonth(), today.getDate());
}

export function localDateToSpDate(context: ExtensionContext, localDate: Date): Date {
  try {
    let spDate: Date = new Date(localDate.getTime());
    let webTimeZoneInfo = getTimeZoneInfo(context);
    if (webTimeZoneInfo) {
      let offsetMinutes = getOffsetMinutes(webTimeZoneInfo, spDate);
      spDate.setMinutes(spDate.getMinutes() + offsetMinutes);
    }
    return spDate;
  } catch (error) {
    Logger.error(error, "localDateToSpDate fails.");
  }

  return localDate;
}

export function spDateToUtcDate(context: ExtensionContext, spDate: Date) {
  try {
    let localDate = new Date(spDate.getTime());
    let webTimeZoneInfo = getTimeZoneInfo(context);
    if (webTimeZoneInfo) {
      let offsetMinutes = webTimeZoneInfo.Bias;
      if (isDaylightDate(webTimeZoneInfo, localDate)) {
        offsetMinutes += webTimeZoneInfo.DaylightBias;
      }
      localDate.setMinutes(localDate.getMinutes() + offsetMinutes);
    }
    return localDate;
  } catch (error) {
    Logger.error(error, "spDateToLocalDate fails.");
  }

  return spDate;
}

export function spDateToLocalDate(context: ExtensionContext, spDate: Date) {
  try {
    let localDate = new Date(spDate.getTime());
    let webTimeZoneInfo = getTimeZoneInfo(context);
    if (webTimeZoneInfo) {
      let offsetMinutes = getOffsetMinutes(webTimeZoneInfo, localDate);
      localDate.setMinutes(localDate.getMinutes() - offsetMinutes);
    }
    return localDate;
  } catch (error) {
    Logger.error(error, "spDateToLocalDate fails.");
  }

  return spDate;
}

export function spDate2String(pageContext: ExtensionContext, date: Date) {
  if (date) {
    try {
      date = spDateToLocalDate(pageContext, date);
    } catch (error) { }
  }
  return date.toISOString();
}

export function dateToString(date: Date) {
  let monthPart = paddingLeft(date.getMonth() + 1, '00');
  let datePart = paddingLeft(date.getDate(), '00');
  let hoursPart = paddingLeft(date.getHours(), '00');
  let minPart = paddingLeft(date.getMinutes(), '00');
  let secPart = paddingLeft(date.getSeconds(), '00');
  return `${date.getFullYear()}-${monthPart}-${datePart} ${hoursPart}:${minPart}:${secPart}`;
}

function paddingLeft(num: number, padString: string) {
  return (padString + num.toString()).slice(-padString.length);
};
