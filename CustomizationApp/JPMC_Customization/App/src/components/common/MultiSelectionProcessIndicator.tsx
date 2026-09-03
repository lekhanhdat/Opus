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
import { ProgressIndicator, ThemeProvider } from '@fluentui/react';
import { DetailsList, DetailsListLayoutMode, IColumn, SelectionMode, ConstrainMode } from '@fluentui/react/lib/DetailsList';
import { IGroup, IGroupRenderProps, IGroupHeaderProps, CollapseAllVisibility } from '@fluentui/react/lib/GroupedList';
import { IListViewCommandSetExecuteEventParameters, RowAccessor } from '@microsoft/sp-listview-extensibility';
import { Icon } from '@fluentui/react/lib/Icon';
import { TooltipHost, ITooltipHostStyles } from '@fluentui/react/lib/Tooltip';
import { initializeFileTypeIcons, getFileTypeIconProps, IFileTypeIconOptions, FileIconType } from '@fluentui/react-file-type-icons';
import * as strings from "OpusCustomizationStrings";
import styles from "../../scss/base.module.scss";
import { stringFormat } from '../../common/StringUtil';
import { Logger } from '../../common/Logger';

export enum ProcessState {
  waiting = 0,
  running = 1,
  success = 2,
  failed = 3
}

export interface IProcessItem {
  itemId: number;
  itemName: string;
  itemRow: RowAccessor;
  state: ProcessState;
  message?: string;
  isFolder?: boolean;
  iconOptions: IFileTypeIconOptions;
}

export function getProcessItems(event: IListViewCommandSetExecuteEventParameters) {
  return event.selectedRows.map((row): IProcessItem => {
    let fileName = row.getValueByName("FileLeafRef");
    let iconOptions: IFileTypeIconOptions;
    if (row.getValueByName("FSObjType") == 1) {
      var progId = row.getValueByName("ProgId");
      if (progId == "OneNote.Notebook") {
        iconOptions = { extension: "one", size: 16 } as IFileTypeIconOptions;
      } else if (progId == "Sharepoint.DocumentSet") {
        iconOptions = { type: FileIconType.docset, size: 16 } as IFileTypeIconOptions;
      } else {
        iconOptions = { type: FileIconType.folder, size: 16 } as IFileTypeIconOptions;
      }
    } else {
      iconOptions = { extension: getExtension(fileName), size: 16 } as IFileTypeIconOptions;
    }

    return {
      itemId: parseInt(row.getValueByName("ID")),
      itemName: fileName,
      itemRow: row,
      state: ProcessState.waiting,
      isFolder: row.getValueByName("FSObjType") == 1,
      message: '',
      iconOptions: iconOptions
    };
  });
}
function getExtension(fileName: string): string {
  if (fileName) {
    let strs = fileName.split(".");
    if (strs.length > 1) {
      return strs[strs.length - 1];
    }
  }
  return "";
};

export interface IProcessResults {
  success: boolean,
  message?: string,
}

export interface IMultiSelectionProcessIndicatorProps {
  start: boolean;
  processItems: Array<IProcessItem>;
  action: (item: IProcessItem) => Promise<IProcessResults>;
  isFolder?: boolean;
  maxProcessor?: number;
  processDescription?: React.ReactNode;
}

export interface IMultiSelectionProcessIndicatorState {
  showPanel: boolean;
  processItems: Array<IProcessItem>;
  groups: IGroup[],
  groupCollapsed: boolean[]
}

export default class MultiSelectionProcessIndicator extends React.Component<IMultiSelectionProcessIndicatorProps, IMultiSelectionProcessIndicatorState> {
  private maxProcessor: number = 2;
  private columns: IColumn[];
  private defaultGroupCollapsed: boolean[]

  constructor(props: IMultiSelectionProcessIndicatorProps) {
    super(props);
    initializeFileTypeIcons();
    this.defaultGroupCollapsed = [false, true, true, true];
    this.maxProcessor = props.maxProcessor || 2;
    this.state = {
      showPanel: true,
      groupCollapsed: this.defaultGroupCollapsed,
      processItems: this.sortItems(props.processItems),
      groups: this.groupedItems(props.processItems),
    };
    const calloutProps = { gapSpace: 0 };
    const hostStyles: Partial<ITooltipHostStyles> = { root: { display: 'inline-block' } };
    this.columns = [
      {
        key: 'column0', name: "File Type", isIconOnly: true, iconName: "Page", minWidth: 20, maxWidth: 20,
        isResizable: true, onRender: (item: IProcessItem) => {
          return item.iconOptions && <Icon {...getFileTypeIconProps(item.iconOptions)} />;
        }
      },
      {
        key: 'column1', name: strings.JPMC_App_Title, fieldName: 'itemName', minWidth: 100, maxWidth: 200,
        isResizable: true, onRender: (item: IProcessItem) => {
          let tooltipId = `${item.itemId}_itemName`
          return <TooltipHost
            content={item.itemName}
            id={tooltipId}
            calloutProps={calloutProps}
            styles={hostStyles}
          >
            <span aria-describedby={tooltipId} className={styles.DetailsMessageSpan}>{item.itemName}</span>
          </TooltipHost>;
        }
      },
      {
        key: 'column2', name: strings.JPMC_App_ProcessStatus, fieldName: 'state', minWidth: 75, maxWidth: 100,
        isResizable: true, onRender: (item: IProcessItem) => {
          let status = this.getProcessStateString(item.state);
          let tooltipId = `${item.itemId}_status`
          return <TooltipHost
            content={status}
            id={tooltipId}
            calloutProps={calloutProps}
            styles={hostStyles}
          >
            <span aria-describedby={tooltipId} className={styles.DetailsMessageSpan}>{status}</span>
          </TooltipHost>;
        }
      },
      {
        key: 'column3', name: strings.JPMC_App_ProcessComment, fieldName: 'message', minWidth: 200, maxWidth: 400,
        isResizable: true, onRender: (item: IProcessItem) => {
          let tooltipId = `${item.itemId}_message`
          return <TooltipHost
            content={item.message}
            id={tooltipId}
            calloutProps={calloutProps}
            styles={hostStyles}
          >
            <span aria-describedby={tooltipId} className={styles.DetailsMessageSpan}>{item.message}</span>
          </TooltipHost>;
        }
      },
    ];
  }

  public componentWillReceiveProps(nextProps: Readonly<IMultiSelectionProcessIndicatorProps>, nextContext: any) {
    if (nextProps.start && !this.props.start) {
      this.maxProcessor = nextProps.maxProcessor || 2;
      this.startProcess(this.maxProcessor);
    }

    const hasChange = nextProps.processItems.length > this.props.processItems.length &&
      nextProps.processItems.some(item => this.props.processItems.every(i => i.itemId !== item.itemId));

    if (hasChange) {
      this.setState({
        processItems: this.sortItems(nextProps.processItems),
        groups: this.groupedItems(nextProps.processItems),
      });
    }
  }

  private sortItems(processItems: any[]): Array<IProcessItem> {
    return processItems.sort((a, b) => {
      if (a.state != b.state) {
        return a.state - b.state;
      } else {
        return (a.isFolder ? -1 : 1) - (b.isFolder ? -1 : 1);
      }
    });
  }
  private groupBy = (array: any[], key: string) => {
    let results: any = {};
    return array.reduce((rs, x) => {
      let val: string = x[key];
      let tempArr: any[] = rs[val];
      if (!tempArr) {
        tempArr = [];
        rs[val] = tempArr;
      }
      tempArr.push(x);
      return rs;
    }, results);
  }

  private getProcessStateString(state: ProcessState) {
    switch (state) {
      case ProcessState.waiting:
        return strings.JPMC_App_ProcessStatus_Waiting;
      case ProcessState.running:
        return strings.JPMC_App_ProcessStatus_Running;
      case ProcessState.success:
        return strings.JPMC_App_ProcessStatus_Success;
      case ProcessState.failed:
        return strings.JPMC_App_ProcessStatus_Failed;
      default:
        return "";
    }
  }

  private groupedItems(processItems: any[]): Array<IGroup> {
    let groups = new Array<IGroup>();
    let startIndex = 0;
    let stateString = "";

    let processItemsGroup = this.groupBy(this.sortItems(processItems), 'state');
    let keysArr = [ProcessState.waiting, ProcessState.running, ProcessState.success, ProcessState.failed];
    keysArr.forEach(key => {
      let isExpand = groups.length == 0;
      let isCollapsed = (this.state ? this.state.groupCollapsed : this.defaultGroupCollapsed)[key];
      stateString = this.getProcessStateString(key);

      let itemCount = 0;
      if (processItemsGroup[key]) {
        itemCount = processItemsGroup[key].length;
        groups.push({
          startIndex: startIndex,
          count: itemCount,
          name: stateString,
          level: 0,
          //isCollapsed: !isExpand,
          isCollapsed: isCollapsed,
          data: { state: key }
        } as IGroup);
        startIndex = startIndex + itemCount;
      }
    });
    return groups;
  }

  private startProcess(maxProcessors: number) {
    if (!this.props.action) {
      Logger.error(new Error("the process action shouldn't be null."));
      return;
    }

    for (let index = 0; index < maxProcessors; index++) {
      this.processItem();
    }
  }

  private async processItem() {
    try {
      let item = this.popWaitingItem();
      if (!item) {
        Logger.info("no waiting item.");
        return;
      }

      let result = await this.props.action(item);
      if (!result) {
        item.state = ProcessState.failed;
        //item.message = strings.JPMC_App_Msg_Classify_Error;
      } else {
        item.state = result.success ? ProcessState.success : ProcessState.failed;
        item.message = result.message;
      }
      this.setState({
        processItems: this.sortItems(this.state.processItems),
        groups: this.groupedItems(this.state.processItems)
      });

      setTimeout(() => {
        this.processItem();
      }, 0);

    } catch (error) {
      Logger.error(error, "process fails.");
    }
  }

  private popWaitingItem() {
    for (let item of this.props.processItems) {
      if (item.state == ProcessState.waiting) {
        item.state = ProcessState.running;
        return item;
      }
    }
    return null;
  }

  private getPercentComplete() {
    let completeVal = 0;
    this.props.processItems.forEach(item => {
      switch (item.state) {
        case ProcessState.running:
          completeVal += 0.5;
          break;
        case ProcessState.success:
        case ProcessState.failed:
          completeVal += 1;
          break;
        case ProcessState.waiting:
          break;
      }
    });
    let percentComplete = completeVal / this.props.processItems.length;
    return percentComplete;
  }

  private toggleGroup = (group: IGroup) => {
    let tempStates = this.state.groupCollapsed.slice(0);
    tempStates[group.data.state] = !group.isCollapsed;
    this.setState({
      groupCollapsed: tempStates
    }, () => {
      //console.log(this.state.groupCollapsed);
    });
  }

  public render(): React.ReactElement<{}> {
    let allItems = this.props.processItems;
    let completeItems = allItems.filter((item) => {
      return item.state == ProcessState.success || item.state == ProcessState.failed;
    });
    let progressLabel = stringFormat(
      strings.JPMC_App_ProgressIndicatorLabel,
      [allItems.length.toString(), completeItems.length.toString()]);
    let isAllCompleted = allItems.length === completeItems.length;

    return (
      <div>
        {this.props.start && <div>
          <ProgressIndicator
            label={progressLabel}
            description={
              !isAllCompleted && <div className={styles.ProgressIndicatorDescription}>
                <div style={{ marginBottom: "10px" }}>{strings.JPMC_App_ProgressIndicatorDescription}</div>
              </div>
            }
            percentComplete={this.getPercentComplete()} />
          <div style={{ width: '90%' }}>
            <ThemeProvider>
              <DetailsList
                items={this.state.processItems}
                groups={this.state.groups}
                compact={false}
                columns={this.columns}
                selectionMode={SelectionMode.none}
                setKey="itemId"
                layoutMode={DetailsListLayoutMode.justified}
                constrainMode={ConstrainMode.unconstrained}
                selectionPreservedOnEmptyClick={true}
                groupProps={{
                  collapseAllVisibility: CollapseAllVisibility.hidden,
                  headerProps: { onToggleCollapse: this.toggleGroup } as IGroupHeaderProps
                } as IGroupRenderProps}
              />
            </ThemeProvider>
          </div>
        </div>}
      </div>
    );
  }
}
