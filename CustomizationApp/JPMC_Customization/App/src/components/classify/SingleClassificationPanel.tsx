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
import { IListViewCommandSetExecuteEventParameters, RowAccessor } from "@microsoft/sp-listview-extensibility";
import { Dropdown, DatePicker, IDropdownStyles, IDropdownOption, Panel, PanelType, Label, MessageBar, MessageBarType } from '@fluentui/react';
import { ProgressIndicator } from "@fluentui/react/lib/ProgressIndicator";
import {
  PrimaryButton,
  DefaultButton,
} from "@fluentui/react/lib/Button";
import { ExtensionContext } from '@microsoft/sp-extension-base';
import PnpUtil from "../../common/PnpUtil";
import { Logger } from "../../common/Logger";
import LoadingContainer from "../common/LoadingContainer";
import { FormRow } from "../common/FormRow";
import * as strings from "OpusCustomizationStrings";
import styles from "../../scss/base.module.scss";
import { getAllClassCodes, getAllCountryCodes, getAllRecordCodes, getCustomColumns, getRecordRetentionLabel, getRetentionTypes } from "../../config/AppConfigs";
import { ICustomColumns } from "../../model/IAppConfigs";
import { ClassifyActionStatus, allRetentionTypes, finalRecordType } from "../../common/Constants";
import { IClassCode } from "../../model/IClassCodeConfig";
import { dateToString, getDateOnlySPToday, localDateToSpDate, spDate2String, spDateToLocalDate } from "../../common/DateUtil";
import { getItemStatus, getSPColumnValue, isOfficeFile, itemIsRecord, notAllowClassify } from "../../common/ValidUtil";
import { DeclareUtil } from "../../common/DeclareUtil";
import { getTaxonomyFieldInfo, getTaxonomyHiddenFieldName, getTermWssId, getUserById, isUnauthorizedAccessRootWeb, setRecordLabel } from "../../common/RestApiUtil";
import * as HttpClientUtil from "../common/HttpClientUtil";
import { Field } from "@pnp/sp/fields/types";


interface IClassificationPanelProps {
  event: IListViewCommandSetExecuteEventParameters;
  spContext: ExtensionContext;
}

interface IClassificationPanelState {
  showPanel: boolean,
  isSaving: boolean,
  isComplete: boolean,
  isLoaded: boolean,
  message?: string,
  toggleStatus?: boolean,

  recordCodes?: IDropdownOption[] | null;
  classCodes?: IDropdownOption[] | null;
  countryCodes?: IDropdownOption[] | null;
  retentionTypes?: IDropdownOption[] | null;

  recordCode?: string | null,
  classCode?: IClassCode | null,
  countryCode?: string | null,
  retentionType?: string | null,
  startDate?: Date,
  progressBarMessage: string | null,
  percentComplete: number | undefined
}

export default class SingleClassificationPanel extends React.Component<
  IClassificationPanelProps,
  IClassificationPanelState
> {
  private reloadPage: boolean;
  private webId: string;
  private listId: string;
  private itemId: number;
  private FileLeafRef: string;
  private spUtil: PnpUtil;
  private customColumns: ICustomColumns;
  private rowItem: RowAccessor;
  private spItem: any;
  private classFieldTermSetId: string;
  private classFieldAnchorId: string;
  private classHiddenFieldName: string;
  private spToday: Date;
  private taskFailedCount: { [key: string]: number } = {}

  constructor(props: IClassificationPanelProps) {
    super(props);

    let notAllowMsg = isUnauthorizedAccessRootWeb() ? strings.JPMC_App_Msg_UnauthorizedAccessRootWeb : "";
    this.state = {
      showPanel: true,
      isLoaded: !!notAllowMsg,
      isSaving: false,
      isComplete: false,
      message: notAllowMsg,
      progressBarMessage: strings.JPMC_App_ProgressIndicatorDescription,
      percentComplete: undefined
    };
    if (notAllowMsg) {
      return;
    }

    this.rowItem = this.props.event.selectedRows[0];
    let pageContext = this.props.spContext.pageContext;
    this.webId = pageContext.web.id?.toString()!;
    this.listId = pageContext.list?.id?.toString()!;
    this.itemId = parseInt(this.rowItem.getValueByName("ID"));
    // this.webUrl = pageContext.web.absoluteUrl;
    this.FileLeafRef = this.rowItem.getValueByName("FileLeafRef");
    this.spToday = getDateOnlySPToday(props.spContext);

    this.initData().catch(error => {
      Logger.error(error, "init date fails.");
    });
  }

  private initData = async () => {
    this.spUtil = new PnpUtil(this.props.spContext);
    this.customColumns = getCustomColumns();
    this.spItem = await this.spUtil.getItem(this.listId, this.itemId);
    let notAllowMsg = await notAllowClassify(this.props.spContext, this.spUtil, this.listId, this.rowItem, this.spItem, false);
    if (notAllowMsg) {
      this.showErrorMessage(notAllowMsg);
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

    let recordCodes = getAllRecordCodes();
    let recordCode = this.spItem[this.customColumns.recordStatus];
    let classCode = this.spUtil.getItemTaxonomyFieldValue(this.spItem, this.customColumns.classCode);
    let countryCode = this.spItem[this.customColumns.countryCode];
    let retentionType = this.spItem[this.customColumns.retentionType];
    let startDate = this.spUtil.getItemDateFieldValue(this.spItem, this.customColumns.startDate);
    if (startDate) {
      startDate = localDateToSpDate(this.props.spContext, startDate);
    }

    const updateStates: any = {
      isLoaded: true,
      recordCodes: recordCodes.map(value => ({ key: value, text: value })),
    };

    if (recordCode && recordCodes.indexOf(recordCode) >= 0) {
      updateStates.recordCode = recordCode;

      let classCodes = await getAllClassCodes(this.spUtil, this.classFieldTermSetId, this.classFieldAnchorId, recordCode);
      let classCodeExists = false;
      updateStates.classCodes = classCodes.map(value => {
        if (classCode && value.termId === classCode.termId) {
          classCodeExists = true;
          updateStates.classCode = classCode;
        }
        return { key: value.termId, text: value.termLabel, title: value.description }
      });

      if (classCodeExists) {
        let countryCodes = getAllCountryCodes(recordCode, classCode!.termId);
        updateStates.countryCodes = countryCodes.map(code => ({ key: code, text: code }));

        if (countryCodes.indexOf(countryCode) >= 0) {
          updateStates.countryCode = countryCode;
        }

        const retentionTypes = getRetentionTypes(recordCode, classCode!.termId, countryCode);
        updateStates.retentionTypes = retentionTypes.map(rt => ({ key: rt, text: rt }));

        if (retentionTypes.indexOf(retentionType) >= 0) {
          updateStates.retentionType = retentionType;

          if (retentionType === allRetentionTypes.event) {
            updateStates.startDate = startDate;
          }
        }
      }
    }

    this.setState(updateStates);
  }

  private async updateItem() {
    let startDate = this.state.retentionType === allRetentionTypes.event && !!this.state.startDate
      ? dateToString(this.state.startDate!)
      : '';
    const editor = await getUserById(this.props.spContext, this.spItem.EditorId);
    let modifiedDate = this.spUtil.getItemDateFieldValue(this.spItem, "Modified");
    modifiedDate = localDateToSpDate(this.props.spContext, modifiedDate!);
    let updateValues = [
      { FieldName: this.customColumns.recordStatus!, FieldValue: this.state.recordCode! },
      { FieldName: this.customColumns.countryCode!, FieldValue: this.state.countryCode! },
      { FieldName: this.customColumns.retentionType!, FieldValue: this.state.retentionType! },
      { FieldName: this.customColumns.startDate!, FieldValue: startDate },
      { FieldName: this.customColumns.classCode!, FieldValue: `${this.state.classCode!.termLabel}|${this.state.classCode!.termId}` },
      { FieldName: "Editor", FieldValue: JSON.stringify([{ Key: editor.LoginName }]) },
      { FieldName: "Modified", FieldValue: dateToString(modifiedDate) },
    ];

    await this.spUtil.updateItem(
      this.listId,
      this.itemId,
      updateValues);

  }

  private onRecordCodeChange = async (ev: React.FormEvent<HTMLDivElement>, option?: IDropdownOption) => {
    if (option) {
      let selectRecordCode = option.key + '';
      let classCodes = await getAllClassCodes(this.spUtil, this.classFieldTermSetId, this.classFieldAnchorId, selectRecordCode);

      this.setState({
        recordCode: selectRecordCode,
        classCode: null,
        classCodes: classCodes.map(value => {
          return { key: value.termId, text: value.termLabel, title: value.description }
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
        //startDate: getDateOnlySPToday(this.props.spContext),
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

  private checkTaskStatus = async (taskId: string, timerInterval: number) => {
    if (!taskId) return;
    let postData = {
      CurrentUser: {
        LoginName: this.props.spContext.pageContext.user.loginName
      },
      ItemInfo: {
        WebUrl: this.props.spContext.pageContext.web.absoluteUrl,
        ListId: this.listId,
        ItemId: this.rowItem.getValueByName("ID"),
      },
      TaskId: taskId
    };
    try {
      const response = await HttpClientUtil.callRecordsApi(this.props.spContext, "/Api/OpusApp/GetReclassifyStatus", postData);
      if (response.TaskId && (response.ActionStatus == ClassifyActionStatus.Succeed || response.ActionStatus == ClassifyActionStatus.Failed)) {
        clearTimeout(timerInterval);
        if (response.ActionStatus == ClassifyActionStatus.Succeed) {
          if (this.state.showPanel) {
            this.setState({ percentComplete: 1, progressBarMessage: strings.JPMC_App_Msg_Classify_Successful });
            this.reloadPage = true;
          }
        }
        if (response.ActionStatus == ClassifyActionStatus.Failed) {
          if (this.state.showPanel) {
            if (response.Message == "JPMC_App_Msg_Classify_Exception") {
              this.showErrorMessage(strings.JPMC_App_Msg_Classify_Exception);
              this.reloadPage = true;
            } else if (response.Message == "JPMC_App_Msg_Classify_Skip") {
              this.showErrorMessage(strings.JPMC_App_Msg_Classify_Skip);
              this.reloadPage = true;
            } else {
              this.showErrorMessage(strings.JPMC_App_Msg_Classify_Error);
            }
          }
        }
      }
    } catch (error) {
      this.taskFailedCount[taskId]++;
      console.info(`Task: ${taskId} failed count is ${this.taskFailedCount[taskId]}`);
      if (this.taskFailedCount[taskId] > 6) {
        clearTimeout(timerInterval);
        this.showErrorMessage(error.message);
      }
    }
  };

  private clearRetentionLabel = async (): Promise<boolean> => {
    let postData = {
      CurrentUser: {
        LoginName: this.props.spContext.pageContext.user.loginName
      },
      ItemInfo: {
        WebUrl: this.props.spContext.pageContext.web.absoluteUrl,
        ListId: this.listId,
        ItemId: this.rowItem.getValueByName("ID"),
      }
    };
    try {
      const res = await HttpClientUtil.callRecordsApi(this.props.spContext, "/Api/OpusApp/ClearTag", postData);
      if (res.Success) {
        return true;
      } else {
        this.showErrorMessage(res.Message);
      }
    } catch (error) {
      this.showErrorMessage(error.message);
    }
    return false;
  }

  private onSave = async () => {
    if (this.rowItem.getValueByName("FSObjType") == 1) {
      this.setState({ isSaving: true, progressBarMessage: strings.JPMC_App_Msg_Classify_InProcess });
      let postData = {
        CurrentUser: {
          LoginName: this.props.spContext.pageContext.user.loginName
        },
        ItemInfo: {
          WebUrl: this.props.spContext.pageContext.web.absoluteUrl,
          ListId: this.listId,
          ItemId: this.rowItem.getValueByName("ID"),
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

      HttpClientUtil.callRecordsApi(this.props.spContext, "/Api/OpusApp/Reclassify", postData).then((res) => {
        const taskId = res.TaskId;
        this.taskFailedCount[taskId] = 0;
        let timerInterval = setInterval(() => {
          this.checkTaskStatus(taskId, timerInterval);
        }, 10 * 1000);

      }).catch(error => {
        this.showErrorMessage(error.message);
      });
      return;
    }

    let declareUtil: DeclareUtil;
    try {
      this.setState({ isSaving: true , progressBarMessage:strings.JPMC_App_ProgressIndicatorDescription});

      let isFinalType = this.state.recordCode == finalRecordType;
      let notAllowMsg = await notAllowClassify(this.props.spContext, this.spUtil, this.listId, this.rowItem, this.spItem, isFinalType, this.state.classCode!.termId);
      if (notAllowMsg) {
        this.showErrorMessage(notAllowMsg);
        return;
      }

      let isRecord = itemIsRecord(this.spItem);
      if (isRecord || isFinalType) {
        declareUtil = new DeclareUtil({
          context: this.props.spContext,
          listId: this.listId,
          itemId: this.itemId
        });
        await declareUtil.init();
      }

      const recordLabel = getRecordRetentionLabel();
      if (isFinalType && !recordLabel && !declareUtil!.allowDeclareAsRecord()) {
        this.showErrorMessage(strings.JPMC_App_Msg_Action_NoPermission_Declared);
        return;
      }

      if (isRecord) {
        let result = await declareUtil!.undeclaredRecord();
        if (!result.success) {
          this.showErrorMessage(result.message);
          return;
        }
      } else if (getSPColumnValue(this.customColumns.recordStatus, this.spItem)) {
        if (!await this.clearRetentionLabel()) {
          return;
        }
      }

      await this.updateItem();

      if (isFinalType) {
        if (!recordLabel) {
          const result = await declareUtil!.declareAsRecord();
          if (!result.success) {
            this.showErrorMessage(result.message);
            return;
          }
        } else {
          if (!await setRecordLabel(this.props.spContext,  this.itemId, recordLabel)) {
            this.showErrorMessage();
            return;
          }
        }
      }
      window.location.reload();
    } catch (error) {
      Logger.error(error, `classify fails: ${this.itemId}`);
      this.showErrorMessage();
    } finally {
      if (declareUtil!) {
        declareUtil.dispose();
      }
    }
  }

  private showErrorMessage(message?: string) {
    this.setState({
      isLoaded: true,
      isSaving: false,
      isComplete: true,
      message: message || strings.JPMC_App_Msg_Classify_Error,
    });
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
        {!this.state.isSaving && !this.state.isComplete && (
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
        {(this.state.isComplete || this.state.isSaving) && (
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
      {this.spUtil.isChannelFolderForSPListItem(this.rowItem) && <MessageBar
        messageBarType={MessageBarType.warning}
        isMultiline={true}
      >
        {strings.JPMC_App_Msg_Classify_ChannelFolderWaring}
      </MessageBar>}
      <FormRow
        labelName={strings.JPMC_App_Title}
        value={this.FileLeafRef}
      />
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
        title={!this.state.classCode ? "" : this.state.classCode.termLabel}
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
      {this.state.retentionType && this.state.retentionType == allRetentionTypes.event &&
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
        {this.state.isSaving && (
          <ProgressIndicator
            progressHidden={false}
            label=""
            description={this.state.progressBarMessage}
            percentComplete={this.state.percentComplete}
          />
        )}

        {!this.state.isSaving && this.renderForm()}
        {this.renderFooterContent()}
      </div>
    );
  }

  public render(): React.ReactElement<{}> {
    const dropdownStyles: Partial<IDropdownStyles> = {
      dropdown: { width: "auto" },
    };

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
