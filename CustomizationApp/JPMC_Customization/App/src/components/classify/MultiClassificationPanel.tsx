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
import * as React from "react";
import { IListViewCommandSetExecuteEventParameters } from "@microsoft/sp-listview-extensibility";
import { Dropdown, DatePicker, IDropdownStyles, IDropdownOption, Panel, PanelType, Label, ProgressIndicator, MessageBarType, MessageBar } from '@fluentui/react';
import {
  PrimaryButton,
  DefaultButton,
} from "@fluentui/react/lib/Button";
import { ExtensionContext } from '@microsoft/sp-extension-base';
import PnpUtil from "../../common/PnpUtil";
import { Logger } from "../../common/Logger";
import LoadingContainer from "../common/LoadingContainer";
import * as strings from "OpusCustomizationStrings";
import styles from "../../scss/base.module.scss";
import { getAllClassCodes, getAllCountryCodes, getAllRecordCodes, getCustomColumns, getRecordRetentionLabel, getRetentionTypes } from "../../config/AppConfigs";
import { ICustomColumns } from "../../model/IAppConfigs";
import { ClassifyActionStatus, allRetentionTypes, finalRecordType } from "../../common/Constants";
import { IClassCode } from "../../model/IClassCodeConfig";
import { dateToString, getDateOnlySPToday, localDateToSpDate, spDate2String, spDateToLocalDate } from "../../common/DateUtil";
import MultiSelectionProcessIndicator, { IProcessItem, IProcessResults, getProcessItems } from "../common/MultiSelectionProcessIndicator";
import { getSPColumnValue, isOfficeFile, itemIsRecord, notAllowClassify } from "../../common/ValidUtil";
import { DeclareUtil } from "../../common/DeclareUtil";
import { getTaxonomyFieldInfo, getTaxonomyHiddenFieldName, getTermWssId, getUserById, isUnauthorizedAccessRootWeb, setRecordLabel } from "../../common/RestApiUtil";
import * as HttpClientUtil from "../common/HttpClientUtil";


interface IMultiClassificationPanelProps {
  event: IListViewCommandSetExecuteEventParameters;
  spContext: ExtensionContext;
}

interface IMultiClassificationPanelState {
  showPanel: boolean,
  isSaving: boolean,
  isLoaded: boolean,
  message?: string,

  recordCodes?: IDropdownOption[] | null;
  classCodes?: IDropdownOption[] | null;
  countryCodes?: IDropdownOption[] | null;
  retentionTypes?: IDropdownOption[] | null;

  recordCode?: string | null,
  classCode?: IClassCode | null,
  countryCode?: string | null,
  retentionType?: string | null,
  startDate?: Date,
  percentComplete: number | undefined
}

export default class MultiClassificationPanel extends React.Component<
  IMultiClassificationPanelProps,
  IMultiClassificationPanelState
> {
  private reloadPage: boolean;
  private webId: string;
  private listId: string;
  private spUtil: PnpUtil;
  private customColumns: ICustomColumns;
  private processItems: IProcessItem[];
  private itemMap: Map<number, any>;
  private updatingClassCodeVal: any;
  private updatingFieldValues: any;
  private classFieldTermSetId: string;
  private classFieldAnchorId: string;
  private classHiddenFieldName: string;
  private classCodeWssId: number | undefined;
  private spToday: Date;
  private taskFailedCount: { [key: string]: number } = {}

  constructor(props: IMultiClassificationPanelProps) {
    super(props);
    this.spUtil = new PnpUtil(this.props.spContext);
    let pageContext = this.props.spContext.pageContext;
    this.webId = pageContext.web.id?.toString()!;
    this.listId = pageContext.list?.id?.toString()!;
    // this.webUrl = pageContext.web.absoluteUrl;
    this.customColumns = getCustomColumns();
    let recordCodes = getAllRecordCodes();
    this.processItems = getProcessItems(this.props.event);
    this.spToday = getDateOnlySPToday(this.props.spContext);

    this.state = {
      showPanel: true,
      isLoaded: false,
      isSaving: false,
      recordCodes: recordCodes.map(value => ({ key: value, text: value })),
      percentComplete: undefined
    };
  }

  componentDidMount(): void {
    this.initData();
  }

  private async initData() {
    if (isUnauthorizedAccessRootWeb()) {
      this.showErrorMessage(strings.JPMC_App_Msg_UnauthorizedAccessRootWeb);
      return;
    }
    let classFieldInfo = await getTaxonomyFieldInfo(this.props.spContext, this.customColumns.classCode);
    if (!classFieldInfo) {
      this.showErrorMessage(strings.JPMC_App_Msg_Classify_ClassFieldNotFound);
      return;
    }
    this.classFieldTermSetId = classFieldInfo.TermSetId;
    this.classFieldAnchorId = classFieldInfo.AnchorId;
    this.classHiddenFieldName = await getTaxonomyHiddenFieldName(this.props.spContext, classFieldInfo.TextField);

    await this.spUtil.getTermsFromTermStore(this.classFieldTermSetId, this.classFieldAnchorId);

    this.setState({
      isLoaded: true
    });
  }

  private showErrorMessage(message?: string) {
    this.setState({
      isLoaded: true,
      isSaving: false,
      message: message || strings.JPMC_App_Msg_Classify_Error,
    });
  }

  private getSelectItemIDs() {
    return this.props.event.selectedRows.map(r => parseInt(r.getValueByName("ID")));
  }

  private getSelectFolderIDs() {
    return this.props.event.selectedRows
      .filter(r => r.getValueByName("FSObjType") == 1)
      .map(r => parseInt(r.getValueByName("ID")));
  }

  private onRecordCodeChange = async (ev: React.FormEvent<HTMLDivElement>, option?: IDropdownOption) => {
    if (option) {
      let selectRecordCode = option.key + '';
      let classCodes = await getAllClassCodes(this.spUtil, this.classFieldTermSetId, this.classFieldAnchorId, selectRecordCode);

      this.setState({
        recordCode: selectRecordCode,
        classCode: null,
        classCodes: classCodes.map(value => {
          return { key: value.termId, text: value.termLabel }
        }),
        countryCode: null,
        countryCodes: [],
        retentionType: null,
        retentionTypes: [],
        startDate: undefined,
      });
    }
  }

  private onClassCodeChange = (ev: React.FormEvent<HTMLDivElement>, option?: IDropdownOption) => {
    if (option) {
      let classCode: IClassCode = { termId: option.key + '', termLabel: option.text };
      let countryCodes = getAllCountryCodes(this.state.recordCode!, classCode.termId);

      this.setState({
        classCode: classCode,
        countryCode: null,
        countryCodes: countryCodes.map(code => ({ key: code, text: code })),
        retentionType: null,
        retentionTypes: [],
        startDate: undefined,
      });
    }
  }

  private onCountryCodeChange = (ev: React.FormEvent<HTMLDivElement>, option?: IDropdownOption) => {
    if (option) {
      let countryCode = option.key + '';
      let retentionTypes = getRetentionTypes(this.state.recordCode!, this.state.classCode?.termId!, countryCode);

      this.setState({
        countryCode: countryCode,
        retentionType: null,
        retentionTypes: retentionTypes.map(rt => ({ key: rt, text: rt })),
        startDate: undefined,
      });
    }
  }

  private onRetentionTypeChange = (ev: React.FormEvent<HTMLDivElement>, option?: IDropdownOption) => {
    if (option) {
      let retentionType = option.key + '';

      this.setState({
        retentionType: retentionType,
        // startDate: getDateOnlySPToday(this.props.spContext),
      });
    }
  }

  private onStartDateChange = (date: Date | null | undefined): void => {
    this.setState({
      startDate: date!
    });
  }

  private hidePanel = (): void => {
    this.setState({ showPanel: false });
    if (this.reloadPage) {
      window.location.reload();
    }
  }

  private initUpdateFieldValues() {
    let startDate = this.state.retentionType === allRetentionTypes.event && !!this.state.startDate
      ? dateToString(this.state.startDate!)
      : '';

    this.updatingFieldValues = [
      { FieldName: this.customColumns.recordStatus!, FieldValue: this.state.recordCode! },
      { FieldName: this.customColumns.countryCode!, FieldValue: this.state.countryCode! },
      { FieldName: this.customColumns.retentionType!, FieldValue: this.state.retentionType! },
      { FieldName: this.customColumns.startDate!, FieldValue: startDate },
      { FieldName: this.customColumns.classCode, FieldValue: `${this.state.classCode!.termLabel}|${this.state.classCode!.termId}` },
    ];
  }
  private folderProcessedUpdaterList: Array<(value: IProcessResults | PromiseLike<IProcessResults>) => void> = [];
  private foldersProcessedResult: IProcessResults;
  private checkTaskStatus = async (taskId: string, timerInterval: number) => {
    if (!taskId) return;
    let postData = {
      CurrentUser: {
        LoginName: this.props.spContext.pageContext.user.loginName
      },
      ListItems: {
        WebUrl: this.props.spContext.pageContext.web.absoluteUrl,
        ListId: this.listId,
      },
      TaskId: taskId
    };
    try {
      const response = await HttpClientUtil.callRecordsApi(this.props.spContext, "/Api/OpusApp/GetReclassifyStatus", postData);
      if (response.TaskId && (response.ActionStatus == ClassifyActionStatus.Succeed || response.ActionStatus == ClassifyActionStatus.Failed)) {
        clearTimeout(timerInterval);

        let foldersProcessedMessage = '';
        if (this.state.showPanel) {
          if (response.ActionStatus == ClassifyActionStatus.Succeed) {
            this.reloadPage = true;
          }
          else if (response.ActionStatus == ClassifyActionStatus.Failed) {
            if (this.state.showPanel) {
              if (response.Message == "JPMC_App_Msg_Classify_Exception") {
                foldersProcessedMessage = strings.JPMC_App_Msg_Classify_Exception;
                this.reloadPage = true;
              } else if (response.Message == "JPMC_App_Msg_Classify_Skip") {
                foldersProcessedMessage = strings.JPMC_App_Msg_Classify_Skip;
                this.reloadPage = true;
              } else {
                foldersProcessedMessage = strings.JPMC_App_Msg_Classify_Error;
              }
            }
          }
        }

        this.foldersProcessedResult = {
          success: response.ActionStatus != ClassifyActionStatus.Failed,
          message: foldersProcessedMessage
        };
        this.batchUpdateFolderResults();
      }
    } catch (error) {
      this.taskFailedCount[taskId]++;
      if (this.taskFailedCount[taskId] > 60) {
        clearTimeout(timerInterval);
        this.foldersProcessedResult = {
          success: false,
          message: error.message
        };
        this.batchUpdateFolderResults();
      }
    }
  };

  private batchUpdateFolderResults = () => {
    for (const updater of this.folderProcessedUpdaterList) {
      updater(this.foldersProcessedResult);
    }
  }

  private checkContainsChannelFolder = (): boolean => {
    return this.props.event.selectedRows.some(r => this.spUtil.isChannelFolderForSPListItem(r))
  }

  private onSave = async () => {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    this.processFolders();

    this.initUpdateFieldValues();

    let spItems = await this.spUtil.getItems(
      this.listId,
      this.getSelectItemIDs(),
      []);
    this.itemMap = new Map();
    spItems.forEach(item => this.itemMap.set(item.ID, item));

    this.setState({
      isSaving: true,
    });
  }

  private processFolders = async (): Promise<void> => {
    const seletedFolderIDs = this.getSelectFolderIDs();
    if (seletedFolderIDs.length > 0) {
      let postData = {
        CurrentUser: {
          LoginName: this.props.spContext.pageContext.user.loginName
        },
        ListItems: {
          WebUrl: this.props.spContext.pageContext.web.absoluteUrl,
          ListId: this.listId,
          ItemIds: seletedFolderIDs,
        },
        ClassValue: [{
          TermId: this.state.classCode?.termId,
          Label: this.state.classCode?.termLabel
        }],
        RecordStatus: this.state.recordCode,
        CountryCode: this.state.countryCode,
        RetentionType: this.state.retentionType,
        StartDate: this.state.startDate ? spDateToLocalDate(this.props.spContext, this.state.startDate) : null,
        CustomColumns:this.customColumns
      };

      try {
        const res = await HttpClientUtil.callRecordsApi(this.props.spContext, "/Api/OpusApp/ReclassifyMulti", postData);
        const taskId = res.TaskId;
        this.taskFailedCount[taskId] = 0;

        let timerInterval = setInterval(() => {
          this.checkTaskStatus(taskId, timerInterval);
        }, 10 * 1000);

      } catch(error) {
        this.foldersProcessedResult = {
          success: false,
          message: error.message
        };
      }

    }
  }

  private getFolderProcessedResult = async (folder: IProcessItem): Promise<IProcessResults> => {
    return new Promise<IProcessResults>((resolve) => {
      if (!this.foldersProcessedResult) {
        this.folderProcessedUpdaterList.push(resolve);
      } else {
        resolve(this.foldersProcessedResult);
      }
    });
  }

  private saveItem = async (item: IProcessItem): Promise<IProcessResults> => {
    if (item.isFolder) {
      return this.getFolderProcessedResult(item);
    }

    let result: IProcessResults = {
      success: false
    };

    let declareUtil: DeclareUtil;
    try {
      let spItem = this.itemMap.get(item.itemId);
      let isFinalType = this.state.recordCode == finalRecordType;
      let notAllowMsg = await notAllowClassify(
        this.props.spContext,
        this.spUtil,
        this.listId,
        item.itemRow,
        spItem,
        isFinalType,
        this.state.classCode!.termId);

      if (notAllowMsg) {
        result.message = notAllowMsg;
      } else {
        let isRecord = itemIsRecord(spItem);
        if (isRecord || isFinalType) {
          declareUtil = new DeclareUtil({
            context: this.props.spContext,
            listId: this.listId,
            itemId: item.itemId
          });
          await declareUtil.init();
        }

        const recordLabel = getRecordRetentionLabel();
        if (isFinalType && !recordLabel && !declareUtil!.allowDeclareAsRecord()) {
          result.message = strings.JPMC_App_Msg_Action_NoPermission_Declared;
          return result;
        }

        if (isRecord) {
          let result = await declareUtil!.undeclaredRecord();
          if (!result.success) {
            return result;
          }
        } else if (getSPColumnValue(this.customColumns.recordStatus, spItem)) {
          if (!await this.clearRetentionLabel(item, result)) {
            return result;
          }
        }

        await this.doClassification(item);

        if (isFinalType) {
          if (!recordLabel) {
            const result = await declareUtil!.declareAsRecord();
            if (!result.success) {
              return result;
            }
          } else {
            if (!await setRecordLabel(this.props.spContext,  item.itemId, recordLabel)) {
              return result;
            }
          }
        }

        result.success = true;
      }
    } catch (error) {
      Logger.error(error, `classify item fails: ${item.itemId}`);
      result.message = strings.JPMC_App_Msg_Classify_Error;
    } finally {
      if (declareUtil!) {
        declareUtil.dispose();
      }
    }

    return result;
  }

  private doClassification = async (item: IProcessItem) => {
    const spItem = this.itemMap.get(item.itemId);
    const editor = await getUserById(this.props.spContext, spItem.EditorId);
    let modifiedDate = this.spUtil.getItemDateFieldValue(spItem, "Modified");
    modifiedDate = localDateToSpDate(this.props.spContext, modifiedDate!);
    await this.spUtil.updateItem(
      this.listId,
      item.itemId,
      [
        ...this.updatingFieldValues,
        { FieldName: "Editor", FieldValue: JSON.stringify([{ Key: editor.LoginName }]) },
        { FieldName: "Modified", FieldValue: dateToString(modifiedDate) }
      ]);
    this.reloadPage = true;
  }

  private clearRetentionLabel = async (item: IProcessItem, result: IProcessResults): Promise<boolean> => {
    let postData = {
      CurrentUser: {
        LoginName: this.props.spContext.pageContext.user.loginName
      },
      ItemInfo: {
        WebUrl: this.props.spContext.pageContext.web.absoluteUrl,
        ListId: this.listId,
        ItemId: item.itemId,
      }
    };
    try {
      const res = await HttpClientUtil.callRecordsApi(this.props.spContext, "/Api/OpusApp/ClearTag", postData);
      if (res.Success) {
        return true;
      } else {
        result.message = res.Message || strings.JPMC_App_Msg_Classify_Error;
      }
    } catch (error) {
      result.message = error.message;
    }
    return false;
  }

  private isSaveBtnDisabled(): boolean {
    return (
      this.state.isSaving ||
      !this.state.recordCode || !this.state.classCode ||
      !this.state.countryCode || !this.state.retentionType
      // || (this.state.retentionType == allRetentionTypes.event && !this.state.startDate)
    );
  }

  private disableInputs = () => {
    return this.state.isSaving;
  }

  private renderFooterContent = () => {
    return (
      <div style={{ marginTop: "20px" }}>
        {!this.state.isSaving && (
          <div className={styles.EventFootBtns}>
            <PrimaryButton
              text={strings.JPMC_App_Save}
              style={{ marginRight: "8px" }}
              disabled={this.isSaveBtnDisabled()}
              onClick={this.onSave}
            />
            <DefaultButton
              text={strings.JPMC_App_Cancel}
              disabled={this.state.isSaving}
              onClick={this.hidePanel}
            />
          </div>
        )}
        {(this.state.isSaving) && (
          <div className={styles.EventFootBtns}>
            <PrimaryButton text={strings.JPMC_App_Close} onClick={this.hidePanel} />
          </div>
        )}
      </div>
    );
  }

  public renderErrorPanel(): React.ReactElement<{}> {
    return (
      <div>
        <Label style={{ marginTop: "20px" }}>
          {this.state.message}
        </Label>
        <div className={styles.EventFootBtns}>
          <PrimaryButton text={strings.JPMC_App_Close} onClick={this.hidePanel} />
        </div>
      </div>
    )
  }

  public renderForm() {
    const dropdownStyles: Partial<IDropdownStyles> = {
      dropdown: { width: "auto" },
    };
    return <div style={{ marginTop: "20px" }}>
      {this.checkContainsChannelFolder() && <MessageBar
        messageBarType={MessageBarType.warning}
        isMultiline={true}
      >
        {strings.JPMC_App_Msg_Classify_ChannelFolderWaring}
      </MessageBar>}
      <Dropdown
        required
        label={strings.JPMC_App_RecordCode}
        selectedKey={this.state.recordCode}
        options={this.state.recordCodes || []}
        styles={dropdownStyles}
        disabled={this.disableInputs()}
        onChange={this.onRecordCodeChange}
      />
      <Dropdown
        required
        label={strings.JPMC_App_ClassCode}
        selectedKey={this.state.classCode && this.state.classCode.termId}
        options={this.state.classCodes || []}
        styles={dropdownStyles}
        disabled={this.disableInputs()}
        onChange={this.onClassCodeChange}
      />
      <Dropdown
        required
        label={strings.JPMC_App_CountryCode}
        selectedKey={this.state.countryCode}
        options={this.state.countryCodes || []}
        styles={dropdownStyles}
        disabled={this.disableInputs()}
        onChange={this.onCountryCodeChange}
      />
      <Dropdown
        required
        label={strings.JPMC_App_RetentionType}
        selectedKey={this.state.retentionType || null}
        options={this.state.retentionTypes || []}
        styles={dropdownStyles}
        disabled={this.disableInputs()}
        onChange={this.onRetentionTypeChange}
      />
      {this.state.retentionType == allRetentionTypes.event &&
        <DatePicker
          // isRequired
          label={strings.JPMC_App_StartDate}
          ariaLabel={strings.JPMC_App_StartDate}
          disabled={this.disableInputs()}
          onSelectDate={this.onStartDateChange}
          today={this.spToday}
          initialPickerDate={this.spToday}
          value={this.state.startDate}
          allowTextInput={true}
        />}
    </div>;
  }

  public renderPanel(): React.ReactElement<{}> {
    return (
      <div>
        <MultiSelectionProcessIndicator
          start={this.state.isSaving}
          processItems={this.processItems}
          maxProcessor={this.getSelectFolderIDs().length+2}
          action={this.saveItem}
        />

        {!this.state.isSaving && this.renderForm()}

        {this.renderFooterContent()}
      </div>
    );
  }

  public render(): React.ReactElement<{}> {
    return (
      <div>
        <Panel
          headerText={strings.JPMC_App_Classify}
          isOpen={this.state.showPanel}
          type={PanelType.medium}
          closeButtonAriaLabel={strings.JPMC_App_Close}
          onDismiss={this.hidePanel}
          allowTouchBodyScroll={true}
        >
          <LoadingContainer loaded={this.state.isLoaded}>
            {(this.state.message) ? this.renderErrorPanel() : this.renderPanel()}
          </LoadingContainer>
        </Panel>
      </div>
    );
  }
}
