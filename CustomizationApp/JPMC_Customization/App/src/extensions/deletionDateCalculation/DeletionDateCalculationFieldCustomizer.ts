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
import * as ReactDOM from 'react-dom';
import {
  BaseFieldCustomizer,
  type IFieldCustomizerCellEventParameters
} from '@microsoft/sp-listview-extensibility';
import DeletionDateCalculation from '../../components/DeletionDateCalculation';
import { LOG_SOURCE_EndDateCustomizer, Logger } from "../../common/Logger";
import { getCustomColumns, loadAppConfigs } from '../../config/AppConfigs';
import StartDateCalculation from '../../components/StartDateCalculation';

export interface IDeletionDateCalculationFieldCustomizerProperties {
}

export default class DeletionDateCalculationFieldCustomizer
  extends BaseFieldCustomizer<IDeletionDateCalculationFieldCustomizerProperties> {
  private initFailed = false;

  public async onInit(): Promise<void> {
    try {
      await loadAppConfigs(this.context, false);
    } catch (error) {
      this.initFailed = true;
      Logger.error(error, "init field customizer fails.", LOG_SOURCE_EndDateCustomizer);
    }
  }

  public onRenderCell(event: IFieldCustomizerCellEventParameters): void {
    const customColumns = getCustomColumns();
    const isStartField = this.context.field.internalName === customColumns.startDate;
    const isEndField = this.context.field.internalName === customColumns.endDate;
    const deletionDateCalculation: React.ReactElement<{}> =
      this.initFailed || (!isStartField && !isEndField) ?
        React.createElement(React.Fragment) :
        React.createElement((isEndField ? DeletionDateCalculation : StartDateCalculation), { context: this.context, spItem: event.listItem });

    ReactDOM.render(deletionDateCalculation, event.domElement);
  }

  public onDisposeCell(event: IFieldCustomizerCellEventParameters): void {
    ReactDOM.unmountComponentAtNode(event.domElement);
    super.onDisposeCell(event);
  }
}
