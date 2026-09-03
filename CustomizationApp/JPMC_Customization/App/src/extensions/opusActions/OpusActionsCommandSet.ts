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
import { Logger } from '../../common/Logger';
import * as React from "react";
import * as ReactDom from "react-dom";
import {
  BaseListViewCommandSet,
  type Command,
  type IListViewCommandSetExecuteEventParameters,
  type ListViewStateChangedEventArgs
} from '@microsoft/sp-listview-extensibility';
import * as strings from 'OpusCustomizationStrings';
import SingleClassificationPanel from "../../components/classify/SingleClassificationPanel";
import { getAppVersion, getCustomColumns, loadAppConfigs } from '../../config/AppConfigs';
import MultiClassificationPanel from '../../components/classify/MultiClassificationPanel';
import PnpUtil from '../../common/PnpUtil';
import AppVersionDialog from '../../components/classify/AppVersionDialog';

/**
 * If your command set uses the ClientSideComponentProperties JSON input,
 * it will be deserialized into the BaseExtension.properties object.
 * You can define an interface to describe it.
 */
export interface IOpusActionsCommandSetProperties {}

const COMMAND_CLASSIFY: string = 'Classify';

const cache = new Map<string, any>();
export default class OpusActionsCommandSet extends BaseListViewCommandSet<IOpusActionsCommandSetProperties> {

  public async onInit(): Promise<void> {
    try {
      this.initCommand(COMMAND_CLASSIFY, false, strings.JPMC_App_Classify);

      await loadAppConfigs(this.context, true);

      let spUtil = new PnpUtil(this.context);
      const hasClassifierColumns = await spUtil.hasColumns(
        this.context.listView.list?.guid?.toString() ?? "",
        [
          getCustomColumns().classCode,
          getCustomColumns().countryCode,
          getCustomColumns().recordStatus,
          getCustomColumns().retentionType,
          getCustomColumns().startDate,
          getCustomColumns().endDate
        ]);
      if (hasClassifierColumns) {
        this.context.listView.listViewStateChangedEvent.add(this, this._onListViewStateChanged);
      } else {
        Logger.info("The list does not have the required columns for classification.");
      }
    } catch (error) {
      Logger.error(error, "init opus actions fails.");
    }
  }

  public onExecute(event: IListViewCommandSetExecuteEventParameters): void {
    switch (event.itemId) {
      case COMMAND_CLASSIFY:
        this.renderActionPanel(event);
        break;
      default:
        return;
    }
  }

  private _onListViewStateChanged = async (args: ListViewStateChangedEventArgs) => {
    let selRows = this.context.listView.selectedRows;
    let visibleCMDs = false;
    if (selRows) {
      visibleCMDs = selRows.length > 0;
      if (visibleCMDs) {
        let spUtil = new PnpUtil(this.context);

        for (let index = 0; index < selRows.length; index++) {
          const selRow = selRows[index];
          let fileRef = selRow.getValueByName("FileRef");
          let fileLeafRef = selRow.getValueByName("FileLeafRef")
          let folderPath = fileRef.substring(0, fileRef.lastIndexOf(fileLeafRef) - 1);
          let currentFolder = null;
          if (cache.has(folderPath)) {
            currentFolder = cache.get(folderPath);
            Logger.info(`Get folder ${folderPath} from cache.`);
          }
          else {
            currentFolder = await spUtil.getFolder(this.context.listView.list?.guid?.toString() ?? "", folderPath);
            if (cache.size > 5) {
              Logger.info("Opus--Clear cache");
              cache.clear();
            }
            cache.set(folderPath, currentFolder);
            Logger.info(`Get folder ${folderPath} from CSOM api.`);
          }
          if (currentFolder != null && currentFolder[getCustomColumns().classCode] && currentFolder[getCustomColumns().classCode].TermGuid) {
            visibleCMDs = false;
            break;
          }
        }
      }
    }

    this.initCommand(COMMAND_CLASSIFY, visibleCMDs);

    this.raiseOnChange();
  }

  private initCommand(cmdId: string, visible: boolean, cmdTitle?: string) {
    const command: Command = this.tryGetCommand(cmdId);
    if (command) {
      command.visible = visible;
      if (cmdTitle) {
        command.title = cmdTitle;
      }
    }
  }

  private renderActionPanel(event: IListViewCommandSetExecuteEventParameters) {
    switch (event.itemId) {
      case COMMAND_CLASSIFY:
        ReactDom.render(this.createClassifyPanelElement(event), document.createElement("div"));
        break;

      default:
        Logger.error(new Error("Unknown command"));
    }
  }

  private createClassifyPanelElement(event: IListViewCommandSetExecuteEventParameters) {
    let component = null;
    let params: any = {
      event: event,
      spContext: this.context,
    };
    if (this.manifest.version != getAppVersion()) {
      return React.createElement(
        AppVersionDialog
      );
    }
    if (event.selectedRows.length > 1) {
      component = MultiClassificationPanel;
    } else {
      component = SingleClassificationPanel;
    }

    return React.createElement(
      component,
      params
    );
  }
}

