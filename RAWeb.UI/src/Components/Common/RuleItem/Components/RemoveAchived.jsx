import { dispatchAction, RetentionConditionUnit, ReviewType } from "./Constants";
import { setCheckedStatusByValue } from "../../../../Utilities/CommonUtil";
import EnableManualApproval from "./EnableManualApproval";

const conditionType = {
    OlderThan: 1,
    Before: 3,
};

const conditionUnit = {
    Year: 1,
    Month: 2,
    Days: 4,
};

const removeArchivedCondition = [
    { name: RMResx.RM_RDM_CreateRule_RemoveArchive_Time, value: "1", checked: true },
];

const removeArchivedTimeUnit = [
    { name: RMResx.RM_JS_RDM_CreateRule_Unit_Days, value: conditionUnit.Days, checked: true },
    { name: RMResx.RM_JS_RDM_CreateRule_Unit_Months, value: conditionUnit.Month, checked: false },
    { name: RMResx.RM_JS_RDM_CreateRule_Unit_Years, value: conditionUnit.Year, checked: false },
];

const SoftDeleteTimeUnit = [
    { name: RMResx.RM_AR_CP_GSS_Day, value: RetentionConditionUnit.Days, checked: false, },
    { name: RMResx.RM_AR_CP_GSS_Week, value: RetentionConditionUnit.Week, checked: true, },
    { name: RMResx.RM_AR_CP_GSS_Month, value: RetentionConditionUnit.Month, checked: false, },
    { name: RMResx.RM_JS_RDM_CreateRule_Unit_Years, value: RetentionConditionUnit.Year, checked: false, },
]

export default class RemoveAchived extends R.Component {
    idAttr = true;
    componentCreate() {
        this.removeArchMaParam = {};
        this.defaultDeviceId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        this.state = {
            elementsEnable: false,
            isRemoveArchived: false,
            isDeleteStub: true,
            isSoftDelete: false,
            isGoogleSoftDelete: false,
            keepDateSoftDeleteNumber: "",
            selectedsoftDeleteUnit: RetentionConditionUnit.Week,
            keepDateSoftDeleteNumberEmpty: false,
            removeArchivedUnitNum: "",
            manualApprovalComponentId: `${this.props.id}Ma`,
            selectedRemoveArchivedTime: null,
            selectedRemoveArchivedTimeUnit: conditionUnit.Year,
            selectedRemoveArchTimeConditionVal: conditionType.OlderThan,
            removeArchivedTimeUnit: RM.deepcopy(removeArchivedTimeUnit),
            removeArchivedCondition: RM.deepcopy(removeArchivedCondition),
            removeArchConditionColIsInValid: false,
            removeArchConditionColNumValid: false,
            selectedStorageItem: null,
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
        }
    }

    getRemoveArchivedTimeCondition() {
        if (this.props.isShowManualApproval) {
            return [
                { name: RMResx.RM_JS_RDM_CreateRule_DateOption_Before, value: conditionType.Before, checked: false },
                { name: RMResx.RM_JS_RDM_CreateRule_DateOption_Older, value: conditionType.OlderThan, checked: true, }
            ];
        } else {
            return [
                { name: RMResx.RM_JS_RDM_CreateRule_DateOption_Older, value: conditionType.OlderThan, checked: true, }
            ];
        }
    }

    onChangeIsRemoveArchived = (isChecked) => {
        this.setState({
            isRemoveArchived: isChecked,
            removeArchConditionColIsInValid: false,
            removeArchConditionColNumValid: false,
            isDeleteStub: true,
            isGoogleSoftDelete: false,
            selectedsoftDeleteUnit: this.props.onlySoftDelete ? RetentionConditionUnit.Week : this.state.selectedsoftDeleteUnit,
            keepDateSoftDeleteNumber: this.props.onlySoftDelete ? "" : this.state.keepDateSoftDeleteNumber,
        });
    };

    onChangeIsDeleteStub = (isChecked) => {
        this.setState({ 
            isDeleteStub: isChecked,
        });
    };

    onChangeIsSoftDelete = (isChecked) => {
        this.setState({ 
            isSoftDelete: isChecked,
            isGoogleSoftDelete: isChecked,
            keepDateSoftDeleteNumber: "",
            selectedsoftDeleteUnit: RetentionConditionUnit.Week,
            keepDateSoftDeleteNumberEmpty: false,
        });
    };

    onKeepDateSoftDeleteChange = (value) => {
        this.setState({
            keepDateSoftDeleteNumber: value,
            keepDateSoftDeleteNumberEmpty: false,
        });
    }

    onSoftDeleteUnitChange = (args) => {
        this.setState({
            selectedsoftDeleteUnit: args.newValue.value,
        });
    }

    onChangeRemoveArchTimeCondition = (args) => {
        this.setState({ 
            selectedRemoveArchTimeConditionVal: args.newValue.value,
            removeArchConditionColIsInValid: false,
            removeArchConditionColNumValid: false,
        });
    };

    onChangeRemoveArchivedUnitNum = (value) => {
        if(value === "0"){ return false;}

        let selectedStorage = this.state.selectedStorageItem;
        let isDefaultStorage = selectedStorage && (selectedStorage.Id.toLowerCase() == this.defaultDeviceId || selectedStorage.IsSystemStorage);
        let timeUnit = this.state.selectedRemoveArchivedTimeUnit;
        if (!value) {
            this.setState({ 
                removeArchivedUnitNum: value,
                removeArchConditionColIsInValid: true,
                removeArchConditionColNumValid: false,
            });
        } else if (isDefaultStorage && !RM.gData.disableRetentionPeriodLimitation && ((timeUnit == conditionUnit.Days && value < 91) || (timeUnit == conditionUnit.Month && value < 4))) {
            this.setState({ 
                removeArchivedUnitNum: value,
                removeArchConditionColIsInValid: false,
                removeArchConditionColNumValid: true,
            });
        } else {
            this.setState({ 
                removeArchivedUnitNum: value,
                removeArchConditionColIsInValid: false,
                removeArchConditionColNumValid: false,
            });
        }
    };

    onChangeRemoveArchivedTimeUnit = (args) => {
        this.setState({ 
            selectedRemoveArchivedTimeUnit: args.newValue.value,
            removeArchConditionColIsInValid: false,
            removeArchConditionColNumValid: false,
        });
    };

    onChangeRemoveArchivedTime = (args) => {
        this.setState({ 
            selectedRemoveArchivedTime: args.newValue,
            removeArchConditionColIsInValid: false,
            removeArchConditionColNumValid: false,
        });
    };

    getRemoveArchMaParam = (manualApprovalParam) => {
        this.removeArchMaParam = manualApprovalParam;
    };

    getRemoveArchMaIsValid = (isValid) => {
        this.manualApprovalIsValid = isValid;
    }

    getRemoveArchived = () => {
        return this.state.isRemoveArchived;
    }

    getRemoveArchColIsValid(){
        let removeArchValid = true;
        let selectedStorage = this.state.selectedStorageItem;
        let isDefaultStorage = selectedStorage && (selectedStorage.Id.toLowerCase() == this.defaultDeviceId || selectedStorage.IsSystemStorage);
        let timeUnit = this.state.selectedRemoveArchivedTimeUnit;
        let ninetyOneDaysTicks = 91 * 24 * 60 * 60 * 1000;
        if(this.state.isRemoveArchived){
            let olderThanColIsInValid = this.state.selectedRemoveArchTimeConditionVal == 
                    conditionType.OlderThan && !this.state.removeArchivedUnitNum;
            let beforeColIsInValid = this.state.selectedRemoveArchTimeConditionVal == 
                    conditionType.Before && !this.state.selectedRemoveArchivedTime;
            let olderThanNumValid = this.state.selectedRemoveArchTimeConditionVal == 
                    conditionType.OlderThan && 
                    ((timeUnit == conditionUnit.Days && this.state.removeArchivedUnitNum < 91) || (timeUnit == conditionUnit.Month && this.state.removeArchivedUnitNum < 4));
            let beforeTimeValid = this.state.selectedRemoveArchTimeConditionVal == 
                    conditionType.Before && this.state.selectedRemoveArchivedTime && new Date().getTime() - this.state.selectedRemoveArchivedTime.getTime() < ninetyOneDaysTicks;
                
            if(olderThanColIsInValid || beforeColIsInValid){
                removeArchValid = false;
                this.setState({
                    removeArchConditionColIsInValid: true,
                    removeArchConditionColNumValid: false,
                });
            } else if (isDefaultStorage && !RM.gData.disableRetentionPeriodLimitation && (olderThanNumValid || beforeTimeValid)) {
                removeArchValid = false;
                this.setState({
                    removeArchConditionColIsInValid: false,
                    removeArchConditionColNumValid: true,
                });
            }
            if (this.props.isShowManualApproval && !this.manualApprovalIsValid) {
                removeArchValid = false;
            }
            if (this.state.isSoftDelete || this.state.isGoogleSoftDelete) {
                if (!this.state.keepDateSoftDeleteNumber) {
                    removeArchValid = false;
                    this.setState({
                        keepDateSoftDeleteNumberEmpty: true,
                    });
                }
            }
        }
        return removeArchValid;
    }

    echoData(data){
        if(data && data.IsEnableRetention){
            let removeAchivedData = {};
            let retentionInfo = data.RetentionInfo;
            removeAchivedData.isRemoveArchived = data.IsEnableRetention;
            removeAchivedData.isDeleteStub = retentionInfo.RemoveOrphanedStub;
            removeAchivedData.isSoftDelete = retentionInfo.IsSoftDelete;
            removeAchivedData.isGoogleSoftDelete = retentionInfo.IsSoftDelete;
            removeAchivedData.selectedRemoveArchTimeConditionVal = retentionInfo.Condition;
            switch (retentionInfo.Condition) {
                case conditionType.OlderThan:
                    removeAchivedData.selectedRemoveArchivedTimeUnit = retentionInfo.KeepDateUnite;
                    removeAchivedData.removeArchivedUnitNum = retentionInfo.KeepDateNumber;
                    removeAchivedData.selectedsoftDeleteUnit = retentionInfo.SoftKeepDateUnite;
                    removeAchivedData.keepDateSoftDeleteNumber = retentionInfo.SoftKeepDateNumber;
                    break;
                case conditionType.Before:
                    removeAchivedData.selectedRemoveArchivedTime = new Date(retentionInfo.Date);
                    break;
            }
            this.setState(removeAchivedData);
            this.echoManualApproval(retentionInfo);
        }
    }

    echoManualApproval(retentionInfo) {
        retentionInfo.EnableManualApproval = retentionInfo.IsManualApproval;
        retentionInfo.Users = retentionInfo.UserInfos;
        retentionInfo.ManualReviewType = retentionInfo.ReviewType;
        retentionInfo.IsSendEmailToOwner = retentionInfo.IsSendEamilToOwner;
        this.dispatch(this.state.manualApprovalComponentId, dispatchAction.setData, retentionInfo);
    }

    updateStorage(storage) {
        this.setState({ selectedStorageItem: storage });
        const isDefaultStorage = storage && (storage.Id.toLowerCase() == this.defaultDeviceId || storage.IsSystemStorage);
        if (!RM.gData.enableSoftDelete || !isDefaultStorage) {
            this.setState({ isSoftDelete: false });
        }
    }

    getRemoveArchParam() {
        let reviewTypeIsWorkflow = this.removeArchMaParam.manualReviewType == ReviewType.Workflow;
        let reviewTypeIsRecordOwner = this.removeArchMaParam.manualReviewType == ReviewType.RecordOwner;
        return {
            IsEnableRetention: this.state.isRemoveArchived,
            RetentionInfo: {
                ColumnName: "Archived Time",
                KeepDateUnite: this.state.selectedRemoveArchivedTimeUnit,
                Condition: this.state.selectedRemoveArchTimeConditionVal,
                KeepDateNumber: this.state.removeArchivedUnitNum || 0,
                Date: RM.TimeUtil.getCommonDateStr(this.state.selectedRemoveArchivedTime),
                IsManualApproval: this.removeArchMaParam.isApproval,
                ReviewType: this.removeArchMaParam.manualReviewType,
                WorkflowId: reviewTypeIsWorkflow ? this.removeArchMaParam.workflowId : null,
                UserInfos: reviewTypeIsRecordOwner ? this.removeArchMaParam.users : null,
                IsSendEamilToOwner: this.removeArchMaParam.isSendEmail,
                RemoveOrphanedStub: this.state.isDeleteStub,
                IsSoftDelete: this.props.onlySoftDelete ? this.state.isGoogleSoftDelete : this.state.isSoftDelete,
                SoftKeepDateNumber: this.state.keepDateSoftDeleteNumber || 0,
                SoftKeepDateUnite: this.state.selectedsoftDeleteUnit,
            },
        };
    }

    renderRemoveArchivedChk() {
        return <div id="retentionPolicyCheckbox">
            <R.Checkbox
                id="raCrRemoveArchivedChk"
                text={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy}
                disabled={this.state.elementsEnable}
                checked={this.state.isRemoveArchived}
                onChange={this.onChangeIsRemoveArchived}
            />
            <$g.Popover>
                <$g.I18NProvider msg={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicyTip}>
                    <a className="ra-link-a" href="/Root/CP/StorageSettings">{RMResx.RM_JS_CP_StorageSetting}</a>
                </$g.I18NProvider>
            </$g.Popover>
        </div>;
    }

    renderRemoveArchivedCondition() {
        let removeArchTimeCondition = setCheckedStatusByValue( 
            "value", 
            "checked", 
            this.getRemoveArchivedTimeCondition(),
            this.state.selectedRemoveArchTimeConditionVal
        );
        if (this.state.isRemoveArchived) {
            let olderThanCondition = this.state.selectedRemoveArchTimeConditionVal == conditionType.OlderThan;
            let removeArchivedConditionClass = olderThanCondition
                ? "archive-remove-time-part4"
                : "archive-remove-time-part3";
            return (
                <div className="cr-archive-remove-content">
                    <div className="cr-archive-remove-title" tabIndex="0">{RMResx.RM_RDM_CreateRule_RemoveArchive_Prefix}</div>
                    <div className={removeArchivedConditionClass}>
                        <R.Combobox
                            id="raCrRemoveArchivedCondition"
                            width={"auto"}
                            searchable={false}
                            disabled={this.state.elementsEnable}
                            textField="name"
                            items={this.state.removeArchivedCondition}
                        />
                        <R.Combobox
                            id="raCrRemoveArchivedTimeCondition"
                            width={"auto"}
                            searchable={false}
                            disabled={this.state.elementsEnable}
                            textField="name"
                            items={removeArchTimeCondition}
                            onChange={this.onChangeRemoveArchTimeCondition}
                        />
                        {this.renderRemoveArchivedOlderThanColumn()}
                        {this.renderRemoveArchivedTimeColumn()}
                    </div>
                    <$g.ValidationMsg show={this.state.removeArchConditionColIsInValid}>
                        {RMResx.RM_RDM_CreateRule_RemoveArchive_Tip}
                    </$g.ValidationMsg>
                    <$g.ValidationMsg show={this.state.removeArchConditionColNumValid}>
                        {RMResx.RM_JS_RetentionRule_ValueError}
                    </$g.ValidationMsg>
                    {this.props.isShowManualApproval && this.renderManualApproval()}
                    {this.props.isShowDeleteStub && this.renderDeleteStub()}
                </div>
            );
        }
    }

    renderRemoveArchivedOlderThanColumn() {
        if (this.state.selectedRemoveArchTimeConditionVal == conditionType.OlderThan) {
            let removeArchivedTimeUnit = setCheckedStatusByValue( 
                "value", 
                "checked", 
                this.state.removeArchivedTimeUnit, 
                this.state.selectedRemoveArchivedTimeUnit
            );
            return (
                <React.Fragment>
                    <R.Input
                        id="raCrRemoveArchivedTimeUnitIpt"
                        type="number" 
                        min={1}
                        onInput={this.onChangeRemoveArchivedUnitNum}
                        disabled={this.state.elementsEnable}
                        value={this.state.removeArchivedUnitNum}
                        onChange={this.onChangeRemoveArchivedUnitNum}
                    />
                    <R.Combobox
                        id="raCrRemoveArchivedTimeUnit"
                        width={"auto"}
                        searchable={false}
                        disabled={this.state.elementsEnable}
                        textField="name"
                        items={removeArchivedTimeUnit}
                        onChange={this.onChangeRemoveArchivedTimeUnit}
                    />
                </React.Fragment>
            );
        }
    }

    renderRemoveArchivedTimeColumn() {
        if (this.state.selectedRemoveArchTimeConditionVal == conditionType.Before) {
            return (
                <R.Datepicker
                    id="raCrRemoveArchivedDatepicker"
                    width={"auto"}
                    hasTimePicker={true}
                    dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                    selectedDate={this.state.selectedRemoveArchivedTime}
                    onChange={this.onChangeRemoveArchivedTime}
                />
            );
        }
    }

    renderManualApproval() {
        return (
            <div className="margin-top-s margin-bottom-s">
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

    renderDeleteStub() {
        const softDeleteTimeUnit = setCheckedStatusByValue( 
            "value", 
            "checked", 
            RM.deepcopy(SoftDeleteTimeUnit), 
            this.state.selectedsoftDeleteUnit
        );
        const selectedStorage = this.state.selectedStorageItem;
        const isDefaultStorage = selectedStorage && (selectedStorage.Id.toLowerCase() == this.defaultDeviceId || selectedStorage.IsSystemStorage);
        //#region Google archive setting
        if (this.props.onlySoftDelete && RM.gData.enableSoftDelete && !isDefaultStorage) {
            return (
                <div className="margin-top-s margin-bottom-s">
                    <div>
                        <R.Checkbox
                            id="raStorageSoftDelete"
                            text={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDelete}
                            disabled={this.state.elementsEnable}
                            checked={this.state.isGoogleSoftDelete}
                            onChange={this.onChangeIsSoftDelete}
                        />
                        <$g.Popover>{RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteDes}</$g.Popover>
                    </div>
                    {this.state.isGoogleSoftDelete && (
                        <div>
                            <$g.I18NProvider msg={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteKeepDate}>
                                <span ref={r => this.keepDataValidation = r} style={{ margin: "0 6px" }}>
                                    <R.Input
                                        id="raCPLastNumIpt"
                                        type="number"
                                        hasControl
                                        width={100}
                                        min={1}
                                        value={this.state.keepDateSoftDeleteNumber}
                                        onChange={this.onKeepDateSoftDeleteChange}
                                        aria={{ ariaLabel: RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteKeepDate }} />
                                    <div className="inline-block margin-left-8">
                                        <R.Combobox
                                            id="raCPLastCom"
                                            width={170}
                                            searchable={false}
                                            textField='name'
                                            valueField='value'
                                            checkedField='checked'
                                            items={softDeleteTimeUnit}
                                            onChange={this.onSoftDeleteUnitChange}
                                        />
                                    </div>
                                </span>
                            </$g.I18NProvider>
                            <$g.ValidationMsg show={this.state.keepDateSoftDeleteNumberEmpty}>
                                {RMResx.RM_RDM_CreateRule_RemoveArchive_Tip}
                            </$g.ValidationMsg>
                        </div>
                    )}
                </div>
            )
        }
        //#endregion

        return <div className="margin-top-s margin-bottom-s">
            {this.props.onlySoftDelete ? null : (
                <R.Checkbox
                    id="raCrDeleteStubChk"
                    text={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_RemoveStub}
                    disabled={this.state.elementsEnable}
                    checked={this.state.isDeleteStub}
                    onChange={this.onChangeIsDeleteStub}
                />
            )}
            {RM.gData.enableSoftDelete && !isDefaultStorage && <>
                <div>
                    <R.Checkbox
                        id="raStorageSoftDelete"
                        text={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDelete}
                        disabled={this.state.elementsEnable}
                        checked={this.state.isSoftDelete}
                        onChange={this.onChangeIsSoftDelete}
                    />
                    <$g.Popover>{RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteDes}</$g.Popover>
                </div>
                {this.state.isSoftDelete && (
                    <div>
                        <$g.I18NProvider msg={RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteKeepDate}>
                            <span ref={r => this.keepDataValidation = r} style={{ margin: "0 6px" }}>
                                <R.Input
                                    id="raCPLastNumIpt"
                                    type="number"
                                    hasControl
                                    width={100}
                                    min={1}
                                    value={this.state.keepDateSoftDeleteNumber}
                                    onChange={this.onKeepDateSoftDeleteChange}
                                    aria={{ ariaLabel: RMResx.RM_RDM_CreateRule_RuleLevelRetentionPolicy_SoftDeleteKeepDate }} />
                                <div className="inline-block margin-left-8">
                                    <R.Combobox
                                        id="raCPLastCom"
                                        width={170}
                                        searchable={false}
                                        textField='name'
                                        valueField='value'
                                        checkedField='checked'
                                        items={softDeleteTimeUnit}
                                        onChange={this.onSoftDeleteUnitChange}
                                    />
                                </div>
                            </span>
                        </$g.I18NProvider>
                        <$g.ValidationMsg show={this.state.keepDateSoftDeleteNumberEmpty}>
                            {RMResx.RM_RDM_CreateRule_RemoveArchive_Tip}
                        </$g.ValidationMsg>
                    </div>
                )}
            </>}
        </div>;
    }

    render() {
        return (
            <div>
                {this.renderRemoveArchivedChk()}
                {this.renderRemoveArchivedCondition()}
            </div>
        );
    }
}
