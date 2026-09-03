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
import {
  SPHttpClient,
} from "@microsoft/sp-http";
import packagedTime from "../config/PackagedInfo";
import { Logger } from "../common/Logger";
import { IClassCode, IClassCodeConfig, IRetentionPeriod, IRetentionSchedule } from "../model/IClassCodeConfig";
import { IAppConfigs, ICustomColumns } from '../model/IAppConfigs';
import { OBR_SITE_TYPE, allRetentionPeriodUnits } from '../common/Constants';
import { getDataByRestApi } from '../common/RestApiUtil';
import { equalIgnoreCase } from '../common/StringUtil';
import PnpUtil from '../common/PnpUtil';
import { ITermInfo } from '@pnp/sp/taxonomy';

// Opus Sync Term Job will replace configSiteUrl and Upload package to 'OpusAppConfig' library.
const _replaceConfigs = {
  configSiteUrl: "", //https://tenantname.sharepoint.com/sites/test1
  csdApiUrl: "CSD API URL",
  aosLoginAppId: "AOS Login Application ID"
};

const localWin: any = window;
localWin.__opus_app_package_info = {
  packagedTime: packagedTime,
  appConfigSite: _replaceConfigs.configSiteUrl
};

const _appConfigLibrary = "OpusAppConfig";
const appConfigFileName = "opus_customization_app_config.json";
let _appConfigs: any = null;
let _currentSiteType: string;
let _recordCodeMapping: Map<string, Map<string, IClassConfigCache>> = new Map();

export interface IClassConfigCache {
  classCode: IClassCode;
  countryCodes: string[];
  retentionTypes: Map<string, string[]>;  // key: countryCode, value: retentionType. (Event, Flat)
  retentionPeriods: Map<string, IRetentionPeriod>;  // key: countryCode + retentionType
}

function getConfigSiteUrl() {
  if (!_replaceConfigs.configSiteUrl) {
    throw new Error("configSiteUrl is null");
  }
  return _replaceConfigs.configSiteUrl.trim().replace(/\/+$/, '');
}

export function getCSDApiUrl() {
  if (!_replaceConfigs.csdApiUrl) {
    throw new Error("CSDApiUrl is null");
  }
  return _replaceConfigs.csdApiUrl.trim().replace(/\/+$/, '');
}

export function getAosLoginAppId() {
  if (!_replaceConfigs.aosLoginAppId) {
    throw new Error("AosLoginAppId is null");
  }
  return _replaceConfigs.aosLoginAppId.trim();
}

export function getCustomColumns(): ICustomColumns {
  return _appConfigs && _appConfigs.customColumns;
}

export function getAppVersion(): string {
  return _appConfigs && _appConfigs.appVersion;
}

export function getRecordRetentionLabel(): string {
  return _appConfigs && _appConfigs.recordRetentionLabel;
}

export function getAllRecordCodes(): string[] {
  let recordCodes: string[] = [];
  _recordCodeMapping.forEach((value, key) => recordCodes.push(key));
  return recordCodes;
}

export async function getAllClassCodes(pnpUtil: PnpUtil, termSetId: string, anchorId: string, recordCode: string): Promise<IClassCode[]> {
  let classCodes: IClassCode[] = [];
  let classCfgCache = _recordCodeMapping.get(recordCode);
  if (classCfgCache) {
    let scopedTerms = await pnpUtil.getTermsFromTermStore(termSetId, anchorId);
    classCfgCache.forEach((value) => {
      if (scopedTerms.has(value.classCode.termId.toLocaleLowerCase())) {
        classCodes.push(value.classCode)
      }
      else {
        Logger.warn(`${value.classCode.termId}|${value.classCode.termLabel} not found in the sp term store`);
      }
    });
  }

  return classCodes.sort((a, b) => a.termLabel.localeCompare(b.termLabel));
}

function getClassConfigCache(recordCode: string, classCodeId: string): IClassConfigCache | null {
  let classCfgCaches = _recordCodeMapping.get(recordCode);
  if (classCfgCaches) {
    return classCfgCaches.get(classCodeId) || null;
  }

  return null;
}

function getConfigFromClassCache(recordCode: string, classCodeId: string, configGetter: (classCfgCache: IClassConfigCache | null) => any): any {
  let classCfgCache = getClassConfigCache(recordCode, classCodeId);
  if (classCfgCache && configGetter) {
    return configGetter(classCfgCache);
  }

  return null;
}

export function getAllCountryCodes(recordCode: string, classCodeId: string): string[] {
  return getConfigFromClassCache(recordCode, classCodeId, (classCfgCache) => classCfgCache?.countryCodes?.sort()) || [];
}

export function getRetentionTypes(recordCode: string, classCodeId: string, countryCode: string): string[] {
  return getConfigFromClassCache(recordCode, classCodeId, (classCfgCache) => {
    if (classCfgCache?.retentionTypes) {
      return classCfgCache?.retentionTypes.get(countryCode);
    }
  }) || [];
}

export function getRetentionPeriod(recordCode: string, classCodeId: string, countryCode: string, retentionType: string): IRetentionPeriod | undefined | null {
  return getConfigFromClassCache(recordCode, classCodeId, (classCfgCache) => {
    if (classCfgCache?.retentionPeriods) {
      return classCfgCache.retentionPeriods.get(countryCode + retentionType);
    }
  });
}

export async function loadAppConfigs(context: ExtensionContext, filterSiteType: boolean): Promise<IAppConfigs> {
  if (_appConfigs) {
    return _appConfigs;
  }
  try {
    let configSiteUrl = getConfigSiteUrl();
    _appConfigs = await getDataByRestApi(context, `${configSiteUrl}/${_appConfigLibrary}/${appConfigFileName}`);
    _currentSiteType = await getSiteType(context);

    checkAppConfigs(_appConfigs);
    assemblyAppConfigs(_appConfigs, filterSiteType);

    return _appConfigs;
  } catch (error) {
    Logger.error(error, "load app config fails.");
    throw error;
  }
}

async function getSiteType(context: ExtensionContext): Promise<string> {
  if (!_appConfigs || !_appConfigs.siteTypePropertyName) {
    throw new Error("siteTypePropertyName is null");
  }
  let webUrl = context.pageContext.site.absoluteUrl;
  let data = await getDataByRestApi(context, `${webUrl}/_api/web/AllProperties?$Select=${_appConfigs.siteTypePropertyName}`);
  return data.SiteType;
}

function checkAppConfigs(appConfigs: IAppConfigs) {
  if (!appConfigs.customColumns) {
    throw Error("no customColumns configuration");
  }
  if (!appConfigs.customColumns.classCode
    || !appConfigs.customColumns.countryCode
    || !appConfigs.customColumns.recordStatus
    || !appConfigs.customColumns.retentionType
    || !appConfigs.customColumns.startDate
    || !appConfigs.customColumns.endDate
  ) {
    throw Error(`no all customColumns configuration: ${JSON.stringify(appConfigs)}`);
  }
}

function assemblyAppConfigs(appConfigs: IAppConfigs, filterSiteType: boolean) {
  let isOBRSite = equalIgnoreCase(OBR_SITE_TYPE, _currentSiteType);
  for (const classCfg of appConfigs.classCodeConfigs) {
    // OBR sites need to load all classes
    if (!classCfg || (!isOBRSite && filterSiteType && !equalIgnoreCase(classCfg.siteType, _currentSiteType))) {
      continue;
    }

    for (const schecule of classCfg.retentionSchedules) {
      convertToClassConfigCache(schecule, classCfg.classCode);
    }
  }

}

function convertToClassConfigCache(schedule: IRetentionSchedule, classCode: IClassCode) {
  let classCodeCfgs = _recordCodeMapping.get(schedule.recordStatus);
  if (!classCodeCfgs) {
    classCodeCfgs = new Map();
    _recordCodeMapping.set(schedule.recordStatus, classCodeCfgs);
  }

  let classCfgCache = classCodeCfgs?.get(classCode.termId);
  if (!classCfgCache) {
    classCfgCache = {
      classCode: classCode,
      countryCodes: [],
      retentionTypes: new Map<string, string[]>(),
      retentionPeriods: new Map<string, IRetentionPeriod>(),
    };
    classCodeCfgs.set(classCode.termId, classCfgCache);
  }
  const countryCodes = classCfgCache.countryCodes;
  const retentionTypes = classCfgCache.retentionTypes;
  const retentionPeriods = classCfgCache.retentionPeriods;

  if (schedule && schedule.countryCodes && schedule.retentionType && schedule.retentionPeriod
    && allRetentionPeriodUnits.indexOf(schedule.retentionPeriod.unit) >= 0 && schedule.retentionPeriod.value >= 0
  ) {
    for (const countryCode of schedule.countryCodes) {
      if (!countryCodes.some(c => c == countryCode)) {
        countryCodes.push(countryCode);
      }

      let tempTypes = retentionTypes.get(countryCode);
      if (!tempTypes) {
        tempTypes = [];
        retentionTypes.set(countryCode, tempTypes);
      }

      if (tempTypes.indexOf(schedule.retentionType) < 0) {
        tempTypes.push(schedule.retentionType);
      }

      let periodKey = countryCode + schedule.retentionType;
      let tempPeriod = retentionPeriods.get(periodKey);
      if (!tempPeriod) {
        retentionPeriods.set(periodKey, schedule.retentionPeriod);
      }
    }
  }
}
