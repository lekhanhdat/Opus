import { dispatchAction, RetentionConditionType, RetentionConditionUnit, RetentionDataTimeRadioValue, RetentionOperateType, ReviewType, TierTypes } from "./Constants";
import EnableManualApproval from "./EnableManualApproval";

const removeArchivedCondition = [
    { name: RMResx.RM_RDM_CreateRule_RemoveArchive_Time, value: "1", checked: true },
];

export default class RuleRetention extends R.Component {
    idAttr = true;
    componentCreate() {
        this.retentionDefaultObj = {
            IsEnableRetention: false,
            ColumnName: "Archived Time",
            Condition: RetentionConditionType.OlderThan,
            RetentionDataTimeType: RetentionDataTimeRadioValue.ArchivedTime,
            KeepDateNumber: "1",
            KeepDateUnite: RetentionConditionUnit.Year,
            Date: RM.TimeUtil.getCommonDateStr(null),
            OperateDataType: RetentionOperateType.DeleteData,
            RemoveOrphanedStub: true,
            IsSoftDelete: false,
            SoftKeepDateNumber: "",
            SoftKeepDateUnite: RetentionConditionUnit.Week,
            IsManualApproval: true,
            ReviewType: ReviewType.Workflow,
            WorkflowId: null,
            UserInfos: null,
            IsSendEamilToOwner: false,
            TierType: TierTypes.ColdTier,
        };
        this.removeArchMaParam = {};
        this.defaultDeviceId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        this.state = {
            manualApprovalComponentId: `${this.props.id}Ma`,
            removeArchivedCondition: RM.deepcopy(removeArchivedCondition),
            selectedStorageItem: null,
            ruleRetentions: [RM.deepcopy(this.retentionDefaultObj)],
        };
    }

    componentReceive(action, ruleData) {
        switch (action) {
            case dispatchAction.save: {
                this.dispatch(this.state.manualApprovalComponentId, dispatchAction.save);
                break;
            }
            case dispatchAction.setData: {
                this.echoData(ruleData);
                break;
            }
            case dispatchAction.selectedStorage: {
                this.updateStorage(ruleData);
                break;
            }
            case dispatchAction.resetRetentionInfo: {
                this.setState({ ruleRetentions: [{ ...this.retentionDefaultObj, IsEnableRetention: false }] })
            }
        }
    }

    getRemoveArchivedTimeCondition() {
        if (this.props.isShowManualApproval) {
            return [
                { name: RMResx.RM_JS_RDM_CreateRule_DateOption_Before, value: RetentionConditionType.Before, checked: false },
                { name: RMResx.RM_JS_RDM_CreateRule_DateOption_Older, value: RetentionConditionType.OlderThan, checked: true, }
            ];
        } else {
            return [
                { name: RMResx.RM_JS_RDM_CreateRule_DateOption_Older, value: RetentionConditionType.OlderThan, checked: true, }
            ];
        }
    }

    getDateOptions(index, state) {
        let options = [
            { text: RMResx.RM_AR_CP_GSS_Day, value: RetentionConditionUnit.Days },
            { text: RMResx.RM_AR_CP_GSS_Week, value: RetentionConditionUnit.Week },
            { text: RMResx.RM_AR_CP_GSS_Month, value: RetentionConditionUnit.Month },
            { text: RMResx.RM_JS_RDM_CreateRule_Unit_Years, value: RetentionConditionUnit.Year },
        ];
        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.ruleRetentions[index][state] == op.value;
            return op;
        });
    }

    getRemoveArchMaParam = (manualApprovalParam) => {
        this.removeArchMaParam = manualApprovalParam;
    };

    getRemoveArchMaIsValid = (isValid) => {
        this.manualApprovalIsValid = isValid;
    }

    getRemoveArchColIsValid() {
        let removeArchValid = true;
        let results = this.state.ruleRetentions.map((info, index) => {
            if (info.IsEnableRetention) {
                if (index === 0) {
                    let olderThanInValid = info.Condition == RetentionConditionType.OlderThan && this.customKeepDataVerify(info, info.KeepDateNumber, info.KeepDateUnite);
                    let softDeleteOlderThanInValid = info.Condition == RetentionConditionType.OlderThan && this.customKeepDataSoftDeleteVerify(info, info.SoftKeepDateNumber);
                    let beforeColInValid = info.Condition == RetentionConditionType.Before && this.customDateVerify(info, info.Date);
                    if (olderThanInValid || softDeleteOlderThanInValid || beforeColInValid) {
                        removeArchValid = false;
                    }
                } else {
                    if (this.customKeepDataVerify(info, info.KeepDateNumber, info.KeepDateUnite)) {
                        removeArchValid = false;
                    }

                    if (this.customKeepDataSoftDeleteVerify(info, info.SoftKeepDateNumber)) {
                        removeArchValid = false;
                    }
                }

                if (this.props.isShowManualApproval && info.OperateDataType == RetentionOperateType.DeleteData && info.IsManualApproval && !this.manualApprovalIsValid) {
                    removeArchValid = false;
                }
            }
            return removeArchValid;
        });
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
        return results.every(r => r);
    }

    echoData(data) {
        if (data.RetentionInfoList) {
            data.RetentionInfoList.forEach(item => {
                switch (item.Condition) {
                    case RetentionConditionType.Before:
                        item.Date = new Date(item.Date);
                        break;
                }
            });
        }
        this.setState({
            ruleRetentions: data.RetentionInfoList && data.RetentionInfoList.length > 0 ? data.RetentionInfoList : this.state.ruleRetentions,
        });
        this.dispatch(this.state.manualApprovalComponentId, dispatchAction.setData, data.RetentionInfoList);
    }

    updateStorage(storage) {
        this.setState({ selectedStorageItem: storage });
    }

    getRemoveArchParam() {
        let reviewTypeIsWorkflow = this.removeArchMaParam.manualReviewType == ReviewType.Workflow;
        let reviewTypeIsRecordOwner = this.removeArchMaParam.manualReviewType == ReviewType.RecordOwner;
        let retentionInfos = this.state.ruleRetentions;
        retentionInfos.forEach((info, index) => {
            info.KeepDateNumber = info.KeepDateNumber || 0;
            info.Date = RM.TimeUtil.getCommonDateStr(info.Date);
            if (this.props.isShowManualApproval && index === 0) {
                info.IsManualApproval = this.removeArchMaParam.isApproval;
                info.ReviewType = this.removeArchMaParam.manualReviewType;
                info.WorkflowId = reviewTypeIsWorkflow ? this.removeArchMaParam.workflowId : null;
                info.UserInfos = reviewTypeIsRecordOwner ? this.removeArchMaParam.users : null;
                info.IsSendEamilToOwner = this.removeArchMaParam.isSendEmail;
            } else {
                info.IsManualApproval = false;
                info.SoftKeepDateNumber = info.SoftKeepDateNumber || 0;
            }
        });
        return retentionInfos;
    }

    getNumberStr(index) {
        let numberStr = RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy;
        if (index > 0) {
            let number = index + 1;
            let remainder = number % 10;
            if (remainder == 1) {
                number = number + RMResx.RM_AR_CP_GSS_Retention_Number_ST;
            } else if (remainder == 2) {
                number = number + RMResx.RM_AR_CP_GSS_Retention_Number_ND;
            } else if (remainder == 3) {
                number = number + RMResx.RM_AR_CP_GSS_Retention_Number_RD;
            } else {
                number = number + RMResx.RM_AR_CP_GSS_Retention_Number_TH;
            }
            numberStr = <$g.I18NProvider msg={RMResx.RM_AR_CP_GSS_EnableRetention_Others}>{number}</$g.I18NProvider>;
        }
        return numberStr;
    }

    setDisabled(index) {
        if (this.state.ruleRetentions[index + 1] && this.state.ruleRetentions[index + 1].IsEnableRetention) {
            return true;
        } else if (this.state.ruleRetentions[index + 1] && !this.state.ruleRetentions[index + 1].IsEnableRetention) {
            if (!this.state.ruleRetentions[index].IsEnableRetention) {
                this.state.ruleRetentions.splice(index + 1, 1);
            }
            return false;
        }
    }

    onEnableRetentionChanged(index, args) {
        let indexRetention = this.state.ruleRetentions[index];
        indexRetention.IsEnableRetention = args;
        if (!args) {
            indexRetention.RetentionDataTimeType = RetentionDataTimeRadioValue.ArchivedTime;
            indexRetention.KeepDateNumber = "1";
            indexRetention.KeepDateUnite = RetentionConditionUnit.Year;
            indexRetention.OperateDataType = RetentionOperateType.DeleteData;
            indexRetention.RemoveOrphanedStub = false;
            indexRetention.IsSoftDelete = false;
            indexRetention.SoftKeepDateNumber = "";
            indexRetention.SoftKeepDateUnite = RetentionConditionUnit.Week;
            indexRetention.showKeepValueEmptyError = false;
            indexRetention.showKeepValueInvalidError = false;
            indexRetention.showKeepValueSoftDeleteEmptyError = false;
        } else {
            indexRetention.RemoveOrphanedStub = true;
        }
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    customKeepDataVerify(indexRetention, value, unit) {
        let selectedStorage = this.state.selectedStorageItem;
        let isDefaultStorage = selectedStorage && (selectedStorage.Id.toLowerCase() == this.defaultDeviceId || selectedStorage.IsSystemStorage);
        let isCheckedArchivedTime = indexRetention.RetentionDataTimeType === RetentionDataTimeRadioValue.ArchivedTime;
        if (!value) {
            indexRetention.showKeepValueEmptyError = true;
            indexRetention.showKeepValueInvalidError = false;
            return true;
        } else if (isDefaultStorage && !RM.gData.disableRetentionPeriodLimitation && isCheckedArchivedTime) {
            if ((unit == RetentionConditionUnit.Days && value < 91) ||
                (unit == RetentionConditionUnit.Week && value < 13) ||
                (unit == RetentionConditionUnit.Month && value < 4)
            ) {
                indexRetention.showKeepValueEmptyError = false;
                indexRetention.showKeepValueInvalidError = true;
                return true;
            } else {
                indexRetention.showKeepValueEmptyError = false;
                indexRetention.showKeepValueInvalidError = false;
                return false;
            }
        } else {
            indexRetention.showKeepValueEmptyError = false;
            indexRetention.showKeepValueInvalidError = false;
            return false;
        }
    }

    customKeepDataSoftDeleteVerify(indexRetention, value) {        
        if (!indexRetention.IsSoftDelete) {
            return false;
        }

        if (!value) {
            indexRetention.showKeepValueSoftDeleteEmptyError = true;
            return true;
        }

        indexRetention.showKeepValueSoftDeleteEmptyError = false;
        return false;
    }

    customDateVerify(indexRetention, date) {
        let selectedStorage = this.state.selectedStorageItem;
        let isDefaultStorage = selectedStorage && (selectedStorage.Id.toLowerCase() == this.defaultDeviceId || selectedStorage.IsSystemStorage);
        let ninetyOneDaysTicks = 91 * 24 * 60 * 60 * 1000;
        if (!value) {
            indexRetention.showKeepValueEmptyError = true;
            indexRetention.showKeepValueInvalidError = false;
            return true;
        } else if (isDefaultStorage && !RM.gData.disableRetentionPeriodLimitation) {
            if (new Date().getTime() - date.getTime() < ninetyOneDaysTicks) {
                indexRetention.showKeepValueEmptyError = false;
                indexRetention.showKeepValueInvalidError = true;
                return true;
            } else {
                indexRetention.showKeepValueEmptyError = false;
                indexRetention.showKeepValueInvalidError = false;
                return false;
            }
        } else {
            indexRetention.showKeepValueEmptyError = false;
            indexRetention.showKeepValueInvalidError = false;
            return false;
        }
    }

    onKeepValueChange(index, value) {
        let indexRetention = this.state.ruleRetentions[index];
        indexRetention.KeepDateNumber = value;
        this.customKeepDataVerify(indexRetention, value, indexRetention.KeepDateUnite);
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    onDateSelChange(index, args) {
        let indexRetention = this.state.ruleRetentions[index];
        indexRetention.KeepDateUnite = args.newValue.value;
        this.customKeepDataVerify(indexRetention, indexRetention.KeepDateNumber, args.newValue.value);
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    onDataRadioChanged(index, args) {
        let indexRetention = this.state.ruleRetentions[index];
        indexRetention.OperateDataType = args;
        if (args == RetentionOperateType.DeleteData) {
            indexRetention.RemoveOrphanedStub = true;
            indexRetention.IsManualApproval = true;

            if (this.state.ruleRetentions[index + 1] && !this.state.ruleRetentions[index + 1].IsEnableRetention) {
                this.state.ruleRetentions.splice(index + 1, 1);
            }
        } else {
            indexRetention.TierType = TierTypes.ColdTier;
            indexRetention.SoftKeepDateNumber = "";
            indexRetention.SoftKeepDateUnite = RetentionConditionUnit.Week;
            indexRetention.IsSoftDelete = false;

            let add = RM.deepcopy(this.retentionDefaultObj);
            this.state.ruleRetentions.push(add);
        }
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    onChangeIsDeleteStub = (index, isChecked) => {
        let indexRetention = this.state.ruleRetentions[index];
        indexRetention.RemoveOrphanedStub = isChecked;
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    };

    onSoftDeleteChanged(index, args) {
        const indexRetention = this.state.ruleRetentions[index];

        if ((args && indexRetention.SoftKeepDateNumber === 0) || !args) {
            indexRetention.SoftKeepDateNumber = "";
            indexRetention.SoftKeepDateUnite = RetentionConditionUnit.Week;
        }

        indexRetention.IsSoftDelete = args;
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    onKeepValueSoftDeleteChange(index, value) {
        const indexRetention = this.state.ruleRetentions[index];
        indexRetention.SoftKeepDateNumber = value;
        this.customKeepDataSoftDeleteVerify(indexRetention, value);
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    onDateSoftDeleteChange(index, args) {
        const indexRetention = this.state.ruleRetentions[index];
        indexRetention.SoftKeepDateUnite = args.newValue.value;
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    onTierRadioChanged = (index, args) => {
        let indexRetention = this.state.ruleRetentions[index];
        indexRetention.TierType = args;
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    onRetentionDataTimeRadioChanged = (index, args) => {
        let indexRetention = this.state.ruleRetentions[index];
        indexRetention.RetentionDataTimeType = args;
        if (args === RetentionDataTimeRadioValue.ModifiedTime) {
            indexRetention.OperateDataType = RetentionOperateType.DeleteData;

            if (this.state.ruleRetentions[index + 1] && !this.state.ruleRetentions[index + 1].IsEnableRetention) {
                this.state.ruleRetentions.splice(index + 1, 1);
            }
        }
        this.customKeepDataVerify(indexRetention, indexRetention.KeepDateNumber, indexRetention.KeepDateUnite);
        this.customKeepDataSoftDeleteVerify(indexRetention, indexRetention.SoftKeepDateNumber);
        this.setState({ ruleRetentions: RM.deepcopy(this.state.ruleRetentions) });
    }

    renderKeepData(retentionObj, index) {
        return <div className="cr-archive-remove-content">
            {this.renderRetentionDataTime(retentionObj, index)}
            <div className="retention-line-top">
                <div className="retention-label">{RMResx["Gui.Common_Keep the last"]}</div>
                <R.Input
                    id="raCPLastNumIpt"
                    type="number"
                    hasControl
                    width={100}
                    min={1}
                    value={retentionObj.KeepDateNumber}
                    onChange={this.onKeepValueChange.bind(this, index)}
                    aria={{ ariaLabel: RMResx["Gui.Common_Keep the last"] }}
                />
                <div className="inline-block margin-left-8">
                    <R.Combobox
                        id="raCPLastCom"
                        width={170}
                        searchable={false}
                        textField='text'
                        valueField='value'
                        checkedField='checked'
                        items={this.getDateOptions(index, "KeepDateUnite")}
                        onChange={this.onDateSelChange.bind(this, index)}
                    />
                </div> 
            </div>
            <$g.ValidationMsg show={retentionObj.showKeepValueEmptyError}>
                {RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={retentionObj.showKeepValueInvalidError}>
                {RMResx.RM_JS_RetentionRule_ValueError}
            </$g.ValidationMsg>
            {this.renderOperateDataRadio(retentionObj, index)}
        </div>
    }

    renderRetentionDataTime(retentionObj, index) {
        let selectedAzureStorage = this.state.selectedStorageItem && this.state.selectedStorageItem.mCurrentXRI.VIM === "azure_vim";
        if (RM.gData.enableFilelevelBackup && selectedAzureStorage && index === 0) {
            return <div className="retention-data-radio">
                <div>
                    <R.Radio
                        name={`${this.props.id}radioTime${index}`}
                        text={RMResx.RM_AR_CP_GSS_Retention_ArchivedTime}
                        value={RetentionDataTimeRadioValue.ArchivedTime}
                        checked={retentionObj.RetentionDataTimeType === RetentionDataTimeRadioValue.ArchivedTime}
                        onChange={this.onRetentionDataTimeRadioChanged.bind(this, index)}
                        disabled={this.setDisabled(index)}
                    />
                </div>
                <div className="margin-top-s">
                    <R.Radio
                        name={`${this.props.id}radioTime${index}`}
                        text={RMResx.RM_AR_CP_GSS_Retention_ModifiedTime}
                        value={RetentionDataTimeRadioValue.ModifiedTime}
                        checked={retentionObj.RetentionDataTimeType === RetentionDataTimeRadioValue.ModifiedTime}
                        onChange={this.onRetentionDataTimeRadioChanged.bind(this, index)}
                        disabled={this.setDisabled(index)}
                    />
                </div>
            </div>
        } else {
            return <div className="retention-title" tabIndex="0">{RMResx.RM_AR_CP_GSS_Retention_ArchivedTime}</div>
        }
    }

    renderOperateDataRadio(retentionObj, index) {
        let selectedStorage = this.state.selectedStorageItem || this.props.defaultStorage;
        let isBYOSStorage = selectedStorage && selectedStorage.mCurrentXRI.VIM === "azure_vim" && selectedStorage.Id.toLowerCase() != this.defaultDeviceId && !selectedStorage.IsSystemStorage;
        let supportOption = !RM.gData.enableFilelevelBackup || (RM.gData.enableFilelevelBackup && retentionObj.RetentionDataTimeType === RetentionDataTimeRadioValue.ArchivedTime);
        return <div>
            <div className="retention-data-radio">
                <div className="retention-title" tabIndex="0">{RMResx.RM_AR_CP_GSS_OperateDataTitle}</div>
                <div>
                    <R.Radio
                        name={`${this.props.id}radioDeleteData${index}`}
                        text={RMResx["Gui.Common_Delete the data"]}
                        value={RetentionOperateType.DeleteData}
                        checked={retentionObj.OperateDataType === RetentionOperateType.DeleteData}
                        onChange={this.onDataRadioChanged.bind(this, index)}
                        disabled={this.setDisabled(index)}
                    />
                </div>
                {retentionObj.OperateDataType === RetentionOperateType.DeleteData && <div>
                    {this.props.isShowDeleteStub && this.renderDeleteStub(retentionObj, index)}
                    {this.props.isShowManualApproval && this.renderManualApproval(retentionObj, index)}
                </div>}
            </div>
            {isBYOSStorage && supportOption && <div className="retention-data-radio">
                <R.Radio
                    name={`${this.props.id}radioMarkDataTier${index}`}
                    text={RMResx.RM_AR_CP_GSS_Retention_MarkDataTier}
                    value={RetentionOperateType.MarkDataTier}
                    checked={retentionObj.OperateDataType === RetentionOperateType.MarkDataTier}
                    onChange={this.onDataRadioChanged.bind(this, index)}
                    disabled={this.setDisabled(index)}
                />
                {retentionObj.OperateDataType === RetentionOperateType.MarkDataTier && this.renderTierRadio(retentionObj, index)}
            </div>}
        </div>;
    }

    renderDeleteStub(retentionObj, index) {
        const selectedStorage = this.state.selectedStorageItem;
        const isDefaultStorage = selectedStorage && (selectedStorage.Id.toLowerCase() == this.defaultDeviceId || selectedStorage.IsSystemStorage);

        return <div className="retention-tier">
            <R.Checkbox
                id="raCrDeleteStubChk"
                text={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub}
                checked={retentionObj.RemoveOrphanedStub}
                onChange={this.onChangeIsDeleteStub.bind(this, index)}
            />
            {RM.gData.enableSoftDelete && !isDefaultStorage && <>
                <div>
                    <R.Checkbox
                        id="raStorageSoftDelete"
                        text={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDelete}
                        title={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDelete}
                        checked={retentionObj.IsSoftDelete}
                        onChange={this.onSoftDeleteChanged.bind(this, index)}
                    />
                    <$g.Popover>{RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteDes}</$g.Popover>
                </div>
                {retentionObj.IsSoftDelete && (
                    <div>
                        <$g.I18NProvider msg={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteKeepDate}>
                            <span style={{ margin: "0 6px" }}>
                                <R.Input
                                    id="raCPLastNumIpt"
                                    type="number"
                                    hasControl
                                    width={100}
                                    min={1}
                                    value={retentionObj.SoftKeepDateNumber}
                                    onChange={this.onKeepValueSoftDeleteChange.bind(this, index)}
                                    aria={{ ariaLabel: RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteKeepDate }} />
                                <div className="inline-block margin-left-8">
                                    <R.Combobox
                                        id="raCPLastCom"
                                        width={170}
                                        searchable={false}
                                        textField='text'
                                        valueField='value'
                                        checkedField='checked'
                                        items={this.getDateOptions(index, "SoftKeepDateUnite")}
                                        onChange={this.onDateSoftDeleteChange.bind(this, index)}
                                    />
                                </div>
                            </span>
                        </$g.I18NProvider>
                        <$g.ValidationMsg show={retentionObj.showKeepValueSoftDeleteEmptyError}>
                            {RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]}
                        </$g.ValidationMsg>
                    </div>
                )}
            </>}
        </div>;
    }

    renderManualApproval() {
        return (
            <div className="retention-tier">
                <EnableManualApproval
                    id={this.state.manualApprovalComponentId}
                    workflowItems={this.props.workflowItems}
                    getIsVerificationPassed={this.getRemoveArchMaIsValid}
                    getApprovalData={this.getRemoveArchMaParam}
                    isShowTitle={false}
                    isSupportUserEmptyValidation={true}
                />
            </div>
        );
    }

    renderTierRadio(retentionObj, index) {
        return <div className="retention-tier">
            <div>
                <R.Radio
                    name={`${this.props.id}radioTier${index}`}
                    text={RMResx.RM_AR_CP_GSS_Retention_ColdTier}
                    value={TierTypes.ColdTier}
                    checked={retentionObj.TierType === TierTypes.ColdTier}
                    onChange={this.onTierRadioChanged.bind(this, index)}
                    disabled={this.setDisabled(index)}
                />
            </div>
            <div className="margin-top-s">
                <R.Radio
                    name={`${this.props.id}radioTier${index}`}
                    text={RMResx.RM_AR_CP_GSS_Retention_ArchiveTier}
                    value={TierTypes.ArchiveTier}
                    checked={retentionObj.TierType === TierTypes.ArchiveTier}
                    onChange={this.onTierRadioChanged.bind(this, index)}
                    disabled={this.setDisabled(index)}
                />
            </div>
        </div>;
    }

    renderRetentions(retention, index) {
        return <div>
            <div id="retentionPolicyCheckbox">
                <R.Checkbox
                    id="raCrRemoveArchivedChk"
                    text={this.getNumberStr(index)}
                    title={this.getNumberStr(index)}
                    disabled={this.setDisabled(index)}
                    checked={retention.IsEnableRetention}
                    onChange={this.onEnableRetentionChanged.bind(this, index)}
                />
                <$g.Popover>
                    <$g.I18NProvider msg={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicyTip}>
                        <a className="ra-link-a" href="/Root/CP/StorageSettings">{RMResx.RM_JS_CP_StorageSetting}</a>
                    </$g.I18NProvider>
                </$g.Popover>
            </div>
            {retention.IsEnableRetention && this.renderKeepData(retention, index)}
        </div>;
    }

    render() {
        return (
            <div>
                {this.state.ruleRetentions.map((retention, index) => {
                    return this.renderRetentions(retention, index);
                })}
            </div>
        );
    }
}