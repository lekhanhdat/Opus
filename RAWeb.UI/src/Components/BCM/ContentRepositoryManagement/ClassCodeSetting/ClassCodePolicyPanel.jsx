import { showToast } from "../../../../Utilities/CommonUtil";
import { RAMessageType } from "../Common/CRMCommonUtil";

const RetentionTypes = {
    None: 0,
    Event: 1,
    Flat: 2
};

const DEFAULT_RETENTION_OPTIONS = [
    { name: RMResx.RM_FS_ClassCodePolicy_RetentionEventType, value: RetentionTypes.Event, checked: false },
    { name: RMResx.RM_FS_ClassCodePolicy_RetentionFlatType, value: RetentionTypes.Flat, checked: false },
];

const buildRetentionOptions = (selectedValue) => {
    if (Number(selectedValue) === RetentionTypes.None) { 
        selectedValue = RetentionTypes.Flat;
    }
    return DEFAULT_RETENTION_OPTIONS.map((option) => ({
        ...option,
        checked: Number(option.value) === Number(selectedValue),
    }));
};

const buildCountryOptions = (countries) => {
    return countries.map((country) => ({
        name: country,
        value: country
    }));
};

const buildClassCodeOptions = (data, termUniqueId) => {
    let countryCodeOptions = [];

    const classCodeOptions = data.map((item) => {
        const mappedCountryOptions = buildCountryOptions(item.CountryCode);
        if (item.TermUniqueId === termUniqueId) {
            countryCodeOptions = mappedCountryOptions;
        }
        return {
            name: item.ClassCode,
            value: item.ClassCode,
            termUniqueId: item.TermUniqueId,
            countryCodeOptions: mappedCountryOptions,
        };
    });

    return { classCodeOptions, countryCodeOptions };
};

const TICKS = 621355968000000000;

const convertToTicks = (date) => {
    const epochTicks = date.getTime() * 10000;
    return epochTicks + TICKS;
}

const ScopeType = {
    SelectedNode: 1,
    AllNodes: 2,
};

export default class FSClassCode extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.state = {
            classCodeOptions: [],
            countryCodeOptions: [],
            retentionTypeOptions: buildRetentionOptions(RetentionTypes.Flat),
            selectedClassCode: '',
            selectedTermUniqueId: '',
            selectedCountryCode: '',
            selectedRetentionType: RetentionTypes.Flat,
            selectedDate: null,
            applyExistDocument: false,
            classCodeValidateFailed: false,
            countryCodeValidateFailed: false,
            startDateValidateFailed: false,
            showTip: false,
            tipMsg: '',
            showWarningTip: false,
            warningMsgForRunningJob: '',
            dateTimeFormat: RM.TimeUtil.getGlobalAuiFormat(),
            scopeOptions: [
                { text: RMResx.RM_FS_ClassCodePolicy_ApplySelectedNode, value: ScopeType.SelectedNode, checked: !this.props.data.ApplyExistDocument },
                { text: RMResx.RM_FS_ClassCodePolicy_ApplyAllNodes, value: ScopeType.AllNodes, checked: this.props.data.ApplyExistDocument },
            ]
        };
    }

    componentInit() {
        this.getClassCodeItems();
        this.checkApplyClassCodeJobRunning(this.props.data);
    }

    componentReceive(type, callback, isRunJobNow) {
        switch (type) {
            case "onSave":
                this.onSave(callback, isRunJobNow);
                break;
            case "onValidate":
                const isValid = this.onValidate();
                if (!isValid) {
                    return;
                }
                callback?.();
                break;
        }
    }

    getClassCodeItems = () => {
        const { TermSetId, ConnGroupId, Level, Id, ClassCode, ApplyExistDocument } = this.props.data;
        const option = {
            url: "/API/FSSettingApi/GetClassCodeCascadeData",
            method: "POST",
            data: {
                TermSetId,
                ConnGroupId,
                Level,
                CurrentNodeId: Id
            }
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            if (res && res.length > 0) {
                const selections = {};
                selections.selectedClassCode = ClassCode?.ClassCodeId ?? '';
                selections.selectedCountryCode = ClassCode?.CountryCode ?? '';
                selections.selectedRetentionType = ClassCode?.RetentionType ?? RetentionTypes.Flat;
                selections.selectedDate = ClassCode?.RetentionDate ? new Date(ClassCode.RetentionDate) : null;
                selections.selectedTermUniqueId = ClassCode?.TermUniqueId ?? '';
                selections.applyExistDocument = ApplyExistDocument ?? false;

                const retentionTypeOptions = buildRetentionOptions(selections.selectedRetentionType);
                const { classCodeOptions, countryCodeOptions } = buildClassCodeOptions(res, selections.selectedTermUniqueId);

                this.setState({
                    classCodeOptions,
                    countryCodeOptions,
                    retentionTypeOptions,
                    ...selections
                });
            }
        }).catch((error) => {
            console.error("Failed to fetch class code data:", error);
        }).finally(() => {
            $$.loading(false);
        });
    };

    checkApplyClassCodeJobRunning = (node) => {
        const option = {
            url: "/API/FSSettingApi/CheckApplyClassJobRunning",
            method: "POST",
            data: node
        };
        fetchUtility(option).then((res) => {
            this.setState({
                showWarningTip: res,
                warningMsgForRunningJob: res ? RMResx.RM_FS_ClassCodePolicy_ApplyJobRunningWarning : ''
            });
        }).catch((error) => {
            console.error("Failed to check class code job running:", error);
        });
    }
    
    updateOptionsSelection = (optionsKey, newValue) => {
        const clonedOptions = [...this.state[optionsKey]];
        const updatedOptions = clonedOptions.map(option => ({
            ...option,
            checked: option.value.toString() === newValue.value.toString()
        }));
        const additionalState = {};
        if (optionsKey === "classCodeOptions") {
            additionalState.countryCodeOptions = newValue.countryCodeOptions || [];
            additionalState.selectedClassCode = newValue.value;
            additionalState.selectedTermUniqueId = newValue.termUniqueId;
            additionalState.selectedCountryCode = "";
            additionalState.classCodeValidateFailed = false;
        }
        if (optionsKey === "countryCodeOptions") {
            additionalState.selectedCountryCode = newValue.value;
            additionalState.countryCodeValidateFailed = false;
        }
        if (optionsKey === "retentionTypeOptions") {
            additionalState.selectedRetentionType = Number(newValue.value);
        }
        this.setState({
            [optionsKey]: updatedOptions,
            ...additionalState
        });
    };

    fieldChangeHandlers = (args, stateKey) => {
        this.updateOptionsSelection(stateKey, args.newValue);
    }

    onStartDateChange = (args) => {
        this.setState({ selectedDate: args.newValue });
    };

    onApplyToAllChange = (args) => {
        this.setState({ applyExistDocument: Number(args) === ScopeType.AllNodes });
    }

    onValidate = () => {
        let isValid = true;
        const validateState = {
            classCodeValidateFailed: false,
            countryCodeValidateFailed: false,
        };
        if (!this.state.selectedClassCode) {
            validateState.classCodeValidateFailed = true;
            isValid = false;
        }
        if (!this.state.selectedCountryCode) {
            validateState.countryCodeValidateFailed = true;
            isValid = false;
        }
        this.setState({ ...validateState });
        return isValid;
    }

    runApplyJob = (payload) => {
        $$.loading(true);
        let option = {
            url: "/api/FSSettingApi/RunFSApplyClassCodeJob",
            method: "Post",
            data: payload
        };
        fetchUtility(option).then((resultData) => {
            $$.loading(false);
            if (resultData.MessageType == RAMessageType.Successful) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunCollectionJobSuccess}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else if (resultData.MessageType == RAMessageType.Failed) {
                if (resultData.ErrorMessage != "") {
                    showToast.error(resultData.ErrorMessage);
                }
            }
        }).catch((e) => {
            console.error("Failed to run apply job:", e);
            $$.loading(false);
        });
    }

    onSave = (callback, isRunJobNow) => { 
        const { TermSetId, ConnGroupId, Id } = this.props.data;
        const data = {
            TermSetId,
            ConnGroupId,
            CurrentNodeId: Id,
            ClassCode: this.state.selectedClassCode,
            TermUniqueId: this.state.selectedTermUniqueId,
            CountryCode: this.state.selectedCountryCode,
            RetentionScheduleType: this.state.selectedRetentionType,
            StartDate: this.state.selectedDate ?? '0001-01-01T00:00:00',
            ApplyExistDocument: this.state.applyExistDocument,
            FSTreeNode: this.props.data
        };
        let option = {
            url: "/API/FSSettingApi/SaveClassCodePolicy",
            method: "POST",
            data
        };
        $$.loading(true);
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == 1) {
                this.setState({ tipMsg: result.ErrorMessage, showTip: true });
            } else {
                this.props.closePanel();
                callback?.();
                const payload = {
                    TermId: this.state.selectedTermUniqueId,
                    ClassCode: this.state.selectedClassCode,
                    CountryCode: this.state.selectedCountryCode,
                    RetentionType: this.state.selectedRetentionType,
                    StartDate: this.state.selectedDate ? convertToTicks(new Date(this.state.selectedDate)) : 0,
                    ApplyToExistingDoc: this.state.applyExistDocument,
                    FSTreeNode: [this.props.data]
                };
                if(isRunJobNow) this.runApplyJob(payload);
            }
        }).catch((e) => {
            console.error("Failed to save class code policy:", e);
            $$.loading(false);
        });
    }

    hideMessageTip = () => {
        this.setState({ showTip: false });
    }

    hideWarningMessageTip = () => {
        this.setState({ showWarningTip: false });
    }

    renderCombobox = (id, ariaLabelId, optionsKey, selectedValue) => {
        const supportSearch = new Set(["classCodeOptions", "countryCodeOptions"])
        return (
            <R.Combobox
                id={id}
                textField="name"
                valueField="value"
                checkedField="checked"
                width="100%"
                linkMode={false}
                value={selectedValue}
                searchable={supportSearch.has(optionsKey)}
                items={this.state[optionsKey]}
                onChange={(args) => this.fieldChangeHandlers(args, optionsKey)}
                aria={`#${ariaLabelId}`}
            />
        )
    }

    render() {
        return (
            <div id={this.props.id}>
                <R.Messagebar
                    message={this.state.tipMsg}
                    status={{ show: this.state.showTip }}
                    classify={"error"}
                    onClose={this.hideMessageTip}
                />
                <R.Messagebar
                    message={this.state.warningMsgForRunningJob}
                    status={{ show: this.state.showWarningTip }}
                    classify={"warn"}
                    onClose={this.hideWarningMessageTip}
                />
                <div className="margin-top-s">
                    <div className="margin-bottom-l">
                        <div id="ariaClassCode" className="margin-bottom-s strong require" tabIndex={0}>
                            {RMResx.RM_FS_ClassCodePolicy_ClassCode}
                        </div>
                        {this.renderCombobox("raClassCode", "ariaClassCode", "classCodeOptions", this.state.selectedClassCode)}
                        <$g.ValidationMsg show={this.state.classCodeValidateFailed}>
                            {RMResx.RM_FS_ClassCodePolicy_ValidationClassCodeMessage}
                        </$g.ValidationMsg>
                    </div>
                    <div className="margin-bottom-l">
                        <div id="ariaCountryCode" className="margin-bottom-s strong require" tabIndex={0}>
                            {RMResx.RM_FS_ClassCodePolicy_CountryCode}
                        </div>
                        {this.renderCombobox("raCountryCode", "ariaCountryCode", "countryCodeOptions", this.state.selectedCountryCode)}
                        <$g.ValidationMsg show={this.state.countryCodeValidateFailed}>
                            {RMResx.RM_FS_ClassCodePolicy_ValidationCountryCodeMessage}
                        </$g.ValidationMsg>
                    </div>
                    <div className="margin-bottom-l">
                        <div id="ariaRetentionType" className="margin-bottom-s strong" tabIndex={0}>
                            {RMResx.RM_FS_ClassCodePolicy_RetentionType}
                        </div>
                        {this.renderCombobox("raRetentionType", "ariaRetentionType", "retentionTypeOptions", this.state.selectedRetentionType)}
                    </div>
                    
                    {this.state.selectedRetentionType === RetentionTypes.Event && (
                        <div className="margin-bottom-l">
                            <div id="ariaStartDate" className="margin-bottom-s strong" tabIndex={0}>
                                {RMResx.RM_FS_ClassCodePolicy_StartDate}
                            </div>
                            <R.Datepicker
                                id="raStartDate"
                                width="100%"
                                dateTimeFormat={this.state.dateTimeFormat}
                                selectedDate={this.state.selectedDate}
                                disabled={false}
                                hasTimePicker={true}
                                onChange={this.onStartDateChange}
                                aria="#ariaStartDate"
                            />
                            <R.ValidationFaker valid={!this.state.startDateValidateFailed} of="#raStartDate" message={RMResx.RM_JS_Common_AUI_Datepicker_Earlier} />
                        </div>
                    )}

                    <div className="margin-bottom-l">
                        <div id="ariaEffectScope" className="margin-bottom-s strong require" tabIndex={0}>
                            {RMResx.RM_FS_ClassCodePolicy_EffectScope}
                        </div>
                        <R.Radio.Group
                            aria={{ "aria-labelledby" : "ariaEffectScope" }}
                            name="ApplyToExistingRadio"
                            block={true}
                            items={this.state.scopeOptions}
                            onChange={this.onApplyToAllChange}
                        />
                    </div>
                </div>
            </div>
        )
    }
}