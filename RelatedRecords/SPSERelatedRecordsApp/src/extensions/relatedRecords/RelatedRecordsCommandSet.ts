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
import { override } from '@microsoft/decorators';
import { Log } from '@microsoft/sp-core-library';
import {
  BaseListViewCommandSet,
  Command,
  IListViewCommandSetListViewUpdatedParameters,
  IListViewCommandSetExecuteEventParameters
} from '@microsoft/sp-listview-extensibility';

import RelatedPanel, { IRelatedPanelProps } from './RelatedPanel';
import { FarmSolutionService } from '../../services/FarmSolutionService';
import { ReplaceConfigs } from '../../config/AppConfigs';

export interface IRelatedRecordsCommandSetCommandSetProperties {
  panelTitle: string;
}

const LOG_SOURCE: string = 'RelatedRecordsCommandSetCommandSet';
let hasInitedRelatedConfig = false

export default class RelatedRecordsCommandSetCommandSet extends BaseListViewCommandSet<IRelatedRecordsCommandSetCommandSetProperties> {
  private _isPanelOpen: boolean = false;
  private panelPlaceholder: HTMLDivElement = null;

  @override
  public onInit(): Promise<void> {
    Log.info(LOG_SOURCE, 'Initialized RelatedRecordsCommandSetCommandSet');
    console.log('RelatedRecords CommandSet initialized - Command ID: RELATED_RECORDS');
    
    // Create panel placeholder
    this.panelPlaceholder = document.createElement('div');
    document.body.appendChild(this.panelPlaceholder);
    
    if(!hasInitedRelatedConfig && !(ReplaceConfigs.ClientId.indexOf("clientId") === 0)){
       new FarmSolutionService(this.context).InitRelatedConfig(ReplaceConfigs)
       hasInitedRelatedConfig = true
    }
   
    return Promise.resolve();
  }

  @override
  public onListViewUpdated(event: IListViewCommandSetListViewUpdatedParameters): void {
    const selectedRows = event.selectedRows;
    const relatedRecordsCommand: Command = this.tryGetCommand('RELATED_RECORDS');
    
    if (relatedRecordsCommand) {
      // Enable the command when a document or item is selected
      // relatedRecordsCommand.visible = event.selectedRows.length === 1;
      if (selectedRows.length > 0) {
        const selectedFolder = selectedRows.some(row => row.getValueByName("FSObjType") == 1);
        const isRelatedColumnNotExist = selectedRows[0].getValueByName("RecordsRelated") === undefined;
        // const isRelatedColumnExist = this.context.listView.columns.some(col => col.field.internalName == "RecordsRelated");
        relatedRecordsCommand.visible = !selectedFolder && !isRelatedColumnNotExist && selectedRows.length === 1
      }
      console.log(`RelatedRecords button visibility set to: ${relatedRecordsCommand.visible} with ${event.selectedRows.length} items selected`);
    } else {
      console.warn('RELATED_RECORDS command not found! Check if the command ID matches the manifest file.');
    }
  }

  @override
  public onExecute(event: IListViewCommandSetExecuteEventParameters): void {
    console.log("event: ", event);
    switch (event.itemId) {
      case 'RELATED_RECORDS':
        console.log('RelatedRecords button clicked!');
        this._showRelatedRecordsPanel(event);
        break;
      default:
        console.warn(`Unknown command executed: ${event.itemId}`);
        throw new Error('Unknown command');
    }
  }
  
  private _showRelatedRecordsPanel(event: IListViewCommandSetExecuteEventParameters): void {
    // Get selected item information
    if (this._isPanelOpen) return;
    this._isPanelOpen = true;
    const selectedRow = event.selectedRows[0];
    const itemId = parseInt(selectedRow.getValueByName('ID'));
    const itemTitle = selectedRow.getValueByName('Title') || selectedRow.getValueByName('FileLeafRef') || `Item #${itemId}`;
    const listId = this.context.pageContext.list.id.toString();
    const webUrl = this.context.pageContext.web.serverRelativeUrl;

    const endpoint = `${this.context.pageContext.web.absoluteUrl}/_api/web/lists(guid'${listId}')/items(${itemId})?$select=UniqueId`;
    const headers = {
      'content-type': 'application/json;charset=utf-8',
      'accept': 'application/json',
    };

    fetch(endpoint, {
      method: "GET",
      headers,
      credentials: "same-origin"
    })
      .then(res => res.json())
      .then(data => {
        const element: React.CElement<IRelatedPanelProps, RelatedPanel> = React.createElement(
          RelatedPanel,
          {
            // title: this.properties.panelTitle || 'Related Records',
            sourceItemId: itemId,
            // sourceItemTitle: itemTitle,
            sourceListId: listId,
            sourceWebUrl: webUrl,
            uniqueId: data.UniqueId || "",
            event,
            spContext: this.context,
            isOpen: this._isPanelOpen,
            onDismiss: () => {
              this._isPanelOpen = false;
              ReactDOM.unmountComponentAtNode(this.panelPlaceholder);
            }
          }
        );
        
        ReactDOM.render(element, this.panelPlaceholder);
      });
    
    console.log(`Showing panel for item: ${itemTitle} (ID: ${itemId})`);
  }
}
