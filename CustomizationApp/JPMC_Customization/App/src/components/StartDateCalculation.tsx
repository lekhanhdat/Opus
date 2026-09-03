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
import * as React from 'react';
import { ListItemAccessor, FieldCustomizerContext } from '@microsoft/sp-listview-extensibility';
import { LOG_SOURCE_StartDateCustomizer, Logger } from '../common/Logger';
import { getCustomColumns } from '../config/AppConfigs';
import { ICustomColumns } from '../model/IAppConfigs';
import { allRetentionTypes } from '../common/Constants';
import { localDateToSpDate } from '../common/DateUtil';

export interface IStartDateCalculationProps {
  context: FieldCustomizerContext;
  spItem: ListItemAccessor;
}

interface IStartDateCalculationState {
  text: string
}

export default class StartDateCalculation extends React.Component<IStartDateCalculationProps, IStartDateCalculationState> {
  private customColumns: ICustomColumns;
  constructor(props: IStartDateCalculationProps) {
    super(props);

    let text = "";
    try {
      text = this.getText() || "";
    } catch (error) {
      Logger.error(error, "initData fails.", LOG_SOURCE_StartDateCustomizer);
    }

    this.state = {
      text: text
    };
  }

  private getText(): string | undefined {
    this.customColumns = getCustomColumns();
    const spItem = this.props.spItem;
    const classVal = spItem.getValueByName(this.customColumns.classCode);
    const classCodeId = classVal && classVal.TermID;
    if (!classCodeId) {
      Logger.warn("classCode is empty", LOG_SOURCE_StartDateCustomizer);
      return;
    }

    const recordCode = spItem.getValueByName(this.customColumns.recordStatus);
    const countryCode = spItem.getValueByName(this.customColumns.countryCode);
    const retentionType = spItem.getValueByName(this.customColumns.retentionType);
    if (!recordCode || !countryCode || !retentionType) {
      Logger.warn("one of recordCode, countryCode, retentionType is empty", LOG_SOURCE_StartDateCustomizer);
      return;
    }

    const startDateVal = spItem.getValueByName(this.customColumns.startDate + '.') || spItem.getValueByName(this.customColumns.startDate);
    let calcDate: Date;
    if (retentionType === allRetentionTypes.event) {
      if (!startDateVal) {
        Logger.warn("It's event retention. but createDate is empty.", LOG_SOURCE_StartDateCustomizer);
        return;
      }
      calcDate = localDateToSpDate(this.props.context, new Date(startDateVal));
    } else {
      calcDate = localDateToSpDate(this.props.context, new Date(spItem.getValueByName("Modified.") || spItem.getValueByName("Modified")));
    }

    return calcDate.toDateString();
  }

  public render(): React.ReactElement<{}> {
    return (
      <div>
        { this.getText() || "" }
      </div>
    );
  }
}
