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
import { Log } from '@microsoft/sp-core-library';
import {
  BaseListViewCommandSet,
  type Command,
  type IListViewCommandSetExecuteEventParameters,
  type ListViewStateChangedEventArgs
} from '@microsoft/sp-listview-extensibility';
// import { Dialog } from '@microsoft/sp-dialog';

import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { RelatedPanel } from './RelatedPanel';
import * as strings from 'RelatedRecordsCommandSetStrings';

/**
 * If your command set uses the ClientSideComponentProperties JSON input,
 * it will be deserialized into the BaseExtension.properties object.
 * You can define an interface to describe it.
 */
export interface IRelatedRecordsCommandSetProperties {
  // This is an example; replace with your own properties
  sampleTextOne: string;
  sampleTextTwo: string;
}

const LOG_SOURCE: string = 'RelatedRecordsCommandSet';

export default class RelatedRecordsCommandSet extends BaseListViewCommandSet<IRelatedRecordsCommandSetProperties> {

  public onInit(): Promise<void> {
    Log.info(LOG_SOURCE, 'Initialized RelatedRecordsCommandSet');

    const relatedCommand: Command = this.tryGetCommand('Opus_Related');
    relatedCommand.visible = false;

    this.context.listView.listViewStateChangedEvent.add(this, this._onListViewStateChanged);

    return Promise.resolve();
  }

  public onExecute(event: IListViewCommandSetExecuteEventParameters): void {
    switch (event.itemId) {
      case 'Opus_Related':
        const element = React.createElement(
          RelatedPanel,
          {
            event: event,
            spContext: this.context,
          }
        );
        ReactDOM.render(element, document.createElement("div"));
      break;
      default:
        throw new Error('Unknown command');
    }
  }

  private _onListViewStateChanged = (args: ListViewStateChangedEventArgs): void => {
    Log.info(LOG_SOURCE, 'List view state changed');
    let selRows = this.context.listView.selectedRows;
    let visibleCMDs = false;
    const relatedCommand: Command = this.tryGetCommand('Opus_Related');
    if (relatedCommand) {
      if (selRows) {
        let selectFold = selRows.some(row => row.getValueByName("FSObjType") == 1);
        const isRelatedColumnExist = this.context.listView.columns.some(col => col.field.internalName == "RecordsRelated");
        visibleCMDs = isRelatedColumnExist && !selectFold && selRows.length == 1;
      }
      relatedCommand.visible = visibleCMDs;
      relatedCommand.title = strings.Related_App_RelatedRecords;
    }
    this.raiseOnChange();
  }
}
