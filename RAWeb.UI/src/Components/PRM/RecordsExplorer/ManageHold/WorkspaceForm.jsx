import React from "react";
import { bindEvents } from "../../../../Utilities/CommonUtil";
import PeoplePicker from "../../../Common/PeoplePicker";

export default class WorkspaceHoldForm extends R.Component {
    idAttr = true;

    constructor(props) {
        super(props);

        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();

        this.state = {
            isEdit: false,
            isSaving: false,
            isSavingHoldNumber: false,
            isSavingHoldName: false,
            calenderTimeInvalid: false,
            holdNameIsExist: false,
            selectedSourceType: 1,
            selectedWorkspace: null,
            workspaceList: [],
            useExistingHold: true,
            useExistingRadioVal: "0",
            holdProfileList: [],
            holdType: 1,
            holdTypeItems: [],
            holdUnitItems: [],
            dataSourceList: [],
            holdProfile: {
                Name: "",
                Number: "",
                Unit: 0,
                Description: "",
                CalenderTime: null,
                TimeZoneId: null,
                IsDayLightSaving: false,
                HoldUserManagers: [],
                IsHoldManagerEmailNotificationEnabled: false,
            },
            submitAttempted: false,
        };

        bindEvents(
            this,
            "onSave",
            "onWorkspaceTypeChange",
            "onWorkspaceChange",
            "onHoldRadioChange",
            "onHoldProfileChange",
        );
    }

    componentInit() {
        this.initData(this.props.data);
        this.loadHoldProfiles();
        this.props.data.formType !== "edit" && this.loadWorkspaces(1);
    }

    componentReceive(type, callback) {
        switch (type) {
            case "onSave":
                this.saveWorkspaceHold(callback);
                break;
        }
    }

    getCurrentUserForHoldManager() {
        const gData = RM && RM.gData ? RM.gData : {};
        const userId = gData.userId || "";
        const userName = gData.userName || "";
        const userPrincipalName = gData.emailAddress || "";
        const displayName = userName || userPrincipalName;

        if (!userId && !displayName) {
            return null;
        }

        return {
            UserId: userId,
            UserName: userName || null,
            UserPrincipalName: userPrincipalName || null,
            Email: userPrincipalName || null,
            DisplayName: displayName,
            InviteType: 0,
        };
    }

    initData(data) {
        this.formData = data;
        this.intHoldMeta();
        if (data.formType === "new") {
            const currentUser = this.getCurrentUserForHoldManager();
            const HoldUserManagers = currentUser ? [currentUser] : [];
            const holdProfile = {
                ...this.state.holdProfile,
                HoldUserManagers,
                HoldManagers: currentUser ? currentUser.DisplayName : "",
            };
            this.setState({ holdProfile });
        }
        if (data.formType === "edit") {
            const item = data.holdItem;

            const dataSourceList = this.props.dataSourceList.map((source) => ({
                ...source,
                checked: source.value === item.SourceType,
            }));
            this.setState({
                isEdit: true,
                selectedSourceType: item.SourceType,
                dataSourceList,
                workspaceList: [
                    {
                        Id: item.WorkplaceId,
                        Url: item.WorkplaceUrl,
                        checked: true,
                    },
                ],
            });
        }
    }

    initHoldTypeCombo() {
        return [
            {
                value: 0,
                title: RMResx.RM_JS_RDM_Hold_Duration,
                checked: false,
            },
            {
                value: 1,
                title: RMResx.RM_JS_RDM_Hold_Canlender,
                checked: true,
            },
        ];
    }
    initHoldUnitCombo() {
        return [
            {
                value: 0,
                title: RMResx.RM_JS_ScheduleSetting_Days,
                checked: true,
            },
            {
                value: 1,
                title: RMResx.RM_JS_ScheduleSetting_Weeks,
                checked: false,
            },
            {
                value: 2,
                title: RMResx.RM_JS_RDM_Explorer_Months,
                checked: false,
            },
            {
                value: 3,
                title: RMResx.RM_JS_RDM_Explorer_Years,
                checked: false,
            },
        ];
    }
    initEditHoldTypeCombo(type) {
        if (type == 0) {
            return [
                {
                    value: 0,
                    title: RMResx.RM_JS_RDM_Hold_Duration,
                    checked: true,
                },
                {
                    value: 1,
                    title: RMResx.RM_JS_RDM_Hold_Canlender,
                    checked: false,
                },
            ];
        } else {
            return [
                {
                    value: 0,
                    title: RMResx.RM_JS_RDM_Hold_Duration,
                    checked: false,
                },
                {
                    value: 1,
                    title: RMResx.RM_JS_RDM_Hold_Canlender,
                    checked: true,
                },
            ];
        }
    }
    initEditHoldUnitCombo(Unit) {
        if (Unit == 0) {
            return [
                {
                    value: 0,
                    title: RMResx.RM_JS_ScheduleSetting_Days,
                    checked: true,
                },
                {
                    value: 1,
                    title: RMResx.RM_JS_ScheduleSetting_Weeks,
                    checked: false,
                },
                {
                    value: 2,
                    title: RMResx.RM_JS_RDM_Explorer_Months,
                    checked: false,
                },
                {
                    value: 3,
                    title: RMResx.RM_JS_RDM_Explorer_Years,
                    checked: false,
                },
            ];
        } else if (Unit == 1) {
            return [
                {
                    value: 0,
                    title: RMResx.RM_JS_ScheduleSetting_Days,
                    checked: false,
                },
                {
                    value: 1,
                    title: RMResx.RM_JS_ScheduleSetting_Weeks,
                    checked: true,
                },
                {
                    value: 2,
                    title: RMResx.RM_JS_RDM_Explorer_Months,
                    checked: false,
                },
                {
                    value: 3,
                    title: RMResx.RM_JS_RDM_Explorer_Years,
                    checked: false,
                },
            ];
        } else if (Unit == 2) {
            return [
                {
                    value: 0,
                    title: RMResx.RM_JS_ScheduleSetting_Days,
                    checked: false,
                },
                {
                    value: 1,
                    title: RMResx.RM_JS_ScheduleSetting_Weeks,
                    checked: false,
                },
                {
                    value: 2,
                    title: RMResx.RM_JS_RDM_Explorer_Months,
                    checked: true,
                },
                {
                    value: 3,
                    title: RMResx.RM_JS_RDM_Explorer_Years,
                    checked: false,
                },
            ];
        } else {
            return [
                {
                    value: 0,
                    title: RMResx.RM_JS_ScheduleSetting_Days,
                    checked: false,
                },
                {
                    value: 1,
                    title: RMResx.RM_JS_ScheduleSetting_Weeks,
                    checked: false,
                },
                {
                    value: 2,
                    title: RMResx.RM_JS_RDM_Explorer_Months,
                    checked: false,
                },
                {
                    value: 3,
                    title: RMResx.RM_JS_RDM_Explorer_Years,
                    checked: true,
                },
            ];
        }
    }

    intHoldMeta() {
        this.setState({
            holdTypeItems: this.initHoldTypeCombo(),
            holdUnitItems: this.initHoldUnitCombo(),
        });
    }

    intEidtHoldMeta(type, Unit) {
        this.setState({
            holdTypeItems: this.initEditHoldTypeCombo(type),
            holdUnitItems: this.initEditHoldUnitCombo(Unit),
        });
    }

    loadHoldProfiles() {
        fetchUtility({
            url: "/api/RecordsExplorerApi/GetSampleAllHolds",
            method: "GET",
        }).then((result) => {
            let holdProfileList = result || [];
            if (this.state.isEdit && this.formData?.holdItem?.HoldTitle) {
                const selectedHold = holdProfileList.find(
                    (item) => item.Name === this.formData.holdItem.HoldTitle,
                );

                holdProfileList = holdProfileList.map((item) => ({
                    ...item,
                    checked: item.Name === this.formData.holdItem.HoldTitle,
                }));

                this.reuseProfile = selectedHold;
            }

            this.setState({
                holdProfileList,
            });
        });
    }
    loadWorkspaces(sourceType = 1) {
        fetchUtility({
            url: "/api/RecordsExplorerApi/GetWorkspadeByNodeLevel",
            method: "POST",
            data: {
                sourceType,
            },
        }).then((result) => {
            this.setState({
                workspaceList: result || [],
            });
        });
    }

    onWorkspaceTypeChange(args) {
        const sourceType = args.newValue.value;

        this.setState({
            selectedSourceType: sourceType,
            selectedWorkspace: null,
        });

        this.loadWorkspaces(sourceType);
    }

    onWorkspaceChange(args) {
        this.setState({
            selectedWorkspace: args.newValue,
        });
    }

    onHoldRadioChange(val) {
        this.reuseProfile = null;

        this.setState({
            useExistingHold: val === "0",
            useExistingRadioVal: val,
        });
    }

    onHoldProfileChange(args) {
        this.reuseProfile = args.newValue;
    }

    buildHoldSetting() {
        if (this.state.useExistingHold) {
            return this.reuseProfile;
        }

        const profile = RM.deepcopy(this.state.holdProfile);

        profile.Type = this.state.holdType;

        profile.CalenderTime = profile.CalenderTime
            ? RM.TimeUtil.getCommonDateStr(new Date(profile.CalenderTime))
            : null;

        profile.Number = this.state.holdType === 1 ? 0 : profile.Number || 0;

        return profile;
    }

    saveWorkspaceHold(callback) {
        const isEdit = this.formData.formType === "edit";

        const workspaceValid = $$.verify("raWorkspaceUrl");

        let holdValid = true;

        if (this.state.useExistingHold) {
            holdValid = $$.verify("raPhyHoldProfiles");
        } else {
            holdValid =
                $.trim(this.state.holdProfile.Name) !== "" &&
                ((this.state.holdType === 1 &&
                    this.state.holdProfile.CalenderTime != null) ||
                    (this.state.holdType === 0 &&
                        this.isAvailableNumber(
                            this.state.holdProfile.Number,
                        ))) &&
                this.state.holdProfile.HoldUserManagers &&
                this.state.holdProfile.HoldUserManagers.length > 0;

            if (!holdValid) {
                this.setState({
                    isSaving: true,
                    isSavingHoldNumber: true,
                    isSavingHoldName: true,
                });
            }
        }

        if (!workspaceValid || !holdValid) {
            callback(false);
            return;
        }

        const payload = isEdit
            ? {
                  Id: this.formData.holdItem.Id,
                  HoldId: this.reuseProfile?.Id,
                  SourceType: this.formData.holdItem.SourceType,
                  WorkspaceHoldSettingDto: this.buildHoldSetting(),
              }
            : {
                  WorkplaceId: this.state.selectedWorkspace?.Id,
                  HoldId: this.reuseProfile?.Id,
                  SourceType: this.state.selectedSourceType,
                  WorkspaceHoldSettingDto: this.buildHoldSetting(),
              };

        const option = {
            url: isEdit
                ? "/api/RecordsExplorerApi/UpdateWorkspaceHold"
                : "/api/RecordsExplorerApi/CreateWorkspaceHold",
            method: "POST",
            data: payload,
        };
        fetchUtility(option)
            .then((result) => {
                if (result.MessageType === 0) {
                    callback(true, payload);
                } else {
                    callback(false, result);
                }
            })
            .catch(() => {
                callback(false);
            });
    }

    onHoldTileChange(column, value) {
        let profile = this.state.holdProfile;
        if (column == "title") {
            profile.Name = $.trim(value);
            this.setState({ holdNameIsExist: false, isSavingHoldName: true });
        } else if (column == "number") {
            profile.Number = value;
            this.setState({ isSavingHoldNumber: true });
        } else if (column == "comment") {
            profile.Description = value;
        }
        this.setState({ holdProfile: profile, isSaving: false });
        this.setState({ isSaving: true });
    }

    onHoldTypeChange(args) {
        let holdType = args.newValue.value;
        this.setState({ holdType: holdType, isSaving: false });
    }

    onHoldUnitChange(args) {
        let profile = this.state.holdProfile;
        let holdUnit = args.newValue.value;
        profile.Unit = holdUnit;
        this.setState({ holdProfile: profile });
    }

    onHoldDateChange(args) {
        let profile = this.state.holdProfile;
        var date = args.newValue;
        var zone = RM.TimeUtil.getGlobalTimezoneInfo();
        profile.CalenderTime = date;
        profile.CalendarDate = date;
        profile.TimeZoneId = zone.id;
        profile.IsDayLightSaving = zone.autoAdjustClock;
        this.setState({
            holdProfile: profile,
            isSaving: false,
        });
        this.setState({ isSaving: true });
        if (date && date.getTime() > new Date().getTime()) {
            this.setState({
                calenderTimeInvalid: false,
            });
        }
    }

    renderHoldDatePicker() {
        let selDate = null;
        if (this.state.holdProfile.CalenderTime != null) {
            selDate = new Date(this.state.holdProfile.CalenderTime);
        }
        return (
            <R.Datepicker
                id="raManageHoldDate"
                selectedDate={selDate}
                data-part="vtWidget"
                width={300}
                dateTimeFormat={this.defaultDateFormat}
                hasTimePicker={true}
                onChange={this.onHoldDateChange.bind(this)}
                triggerBySource={true}
                todayClick={this.todayClick}
            />
        );
    }

    isAvailableNumber(inputText) {
        if (inputText == "" || inputText * 1 == 0) {
            return false;
        }
        let patt = new RegExp("^[0-9]*$", "g");
        let result = patt.test(inputText);
        if (inputText > 2147483647 && result == true) {
            result = false;
        }
        return result;
    }

    onHoldUsersChange = (items) => {
        let profile = this.state.holdProfile;
        const uniqueRecipients = Array.from(
            new Map(items.map((item) => [item.UserId, item])).values(),
        );
        profile.HoldUserManagers = uniqueRecipients;
        this.setState({ holdProfile: profile, isSaving: false });
        this.setState({ isSaving: true });
    };

    onNotifyHoldManagerChange = (e, checked) => {
        let profile = this.state.holdProfile;
        let isChecked =
            typeof e === "boolean"
                ? e
                : typeof checked === "boolean"
                  ? checked
                  : e && e.target
                    ? e.target.checked
                    : !profile.IsHoldManagerEmailNotificationEnabled;
        profile.IsHoldManagerEmailNotificationEnabled = isChecked;
        this.setState({ holdProfile: profile });
    };

    render() {
        return (
            <>
                <div className="margin-bottom-l">
                    <div className="reco-optimize-title">
                        {RMResx.RM_JS_RDM_Workspace_Type}
                    </div>
                    <R.Combobox
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        items={
                            this.state.isEdit
                                ? this.state.dataSourceList
                                : this.props.dataSourceList
                        }
                        width="100%"
                        searchable={false}
                        disabled={this.state.isEdit}
                        onChange={this.onWorkspaceTypeChange}
                    />
                </div>
                <div className="margin-bottom-l">
                    <div className="reco-optimize-title require">
                        {RMResx.RM_JS_RDM_Workspace}
                    </div>
                    <R.Validation
                        element={"Combobox"}
                        require={RMResx.RM_JS_RDM_Workspace_Select}
                        id="raWorkspaceUrl"
                    >
                        <R.Combobox
                            textField="Url"
                            valueField="Id"
                            checkedField="checked"
                            items={this.state.workspaceList}
                            width="100%"
                            searchable={false}
                            disabled={this.state.isEdit}
                            onChange={this.onWorkspaceChange}
                        />
                    </R.Validation>
                </div>
                <$g.FormRow
                    label={RMResx.RM_JS_RDM_Hold_HoldTypeTitle.replace(":", "")}
                    id="ariaHoldType"
                >
                    <$g.RadioGroup
                        name="manage-hold-new-type"
                        onChange={this.onHoldRadioChange}
                        value={this.state.useExistingRadioVal}
                    >
                        <$g.RadioOption
                            value="0"
                            text={RMResx.RM_JS_RDM_Hold_UseExist}
                        />

                        <$g.RadioOption
                            value="1"
                            text={RMResx.RM_JS_RDM_Hold_Create}
                        />
                    </$g.RadioGroup>
                </$g.FormRow>

                {this.state.useExistingHold && (
                    <$g.FormRow
                        label={RMResx.RM_JS_PRM_Hold_RecordForm_SelectExistingHold.replace(
                            ":",
                            "",
                        )}
                        id="ariaSelectHold"
                        require={true}
                    >
                        <R.Validation
                            element={"Combobox"}
                            require={RMResx.RM_JS_RDM_Hold_NeedSelectHold}
                            id="raPhyHoldProfiles"
                        >
                            <R.Combobox
                                checkedField="checked"
                                textField="Name"
                                valueField="Id"
                                width={300}
                                items={this.state.holdProfileList}
                                onChange={this.onHoldProfileChange}
                            />
                        </R.Validation>
                    </$g.FormRow>
                )}

                {!this.state.useExistingHold && (
                    <React.Fragment>
                        <$g.FormRow
                            label={RMResx.RM_JS_RDM_Hold_HoldName.replace(
                                ":",
                                "",
                            )}
                            require={true}
                            id="ariaHoldName"
                            key="h3"
                        >
                            <R.Validation>
                                <R.Input
                                    id="raManageHoldNameIpt"
                                    type="text"
                                    width={300}
                                    value={this.state.holdProfile.Name}
                                    onChange={this.onHoldTileChange.bind(
                                        this,
                                        "title",
                                    )}
                                    aria={{
                                        "aria-labelledby": "ariaHoldName",
                                        "aria-required": true,
                                    }}
                                />
                                <R.ValidationFaker
                                    valid={
                                        !(
                                            this.state.isSaving &&
                                            this.state.isSavingHoldName &&
                                            this.state.holdProfile.Name == ""
                                        )
                                    }
                                    of="#raManageHoldNameIpt"
                                    message={RMResx.RM_JS_RDM_Hold_NoName}
                                />
                                <R.ValidationFaker
                                    valid={
                                        !(
                                            this.state.isSaving &&
                                            this.state.holdNameIsExist &&
                                            this.state.holdProfile.Name
                                        )
                                    }
                                    of="#raManageHoldNameIpt"
                                    message={
                                        RMResx.RM_JS_RDM_Hold_HoldNameExist
                                    }
                                />
                            </R.Validation>
                        </$g.FormRow>
                        <$g.FormRow
                            label={RMResx.RM_JS_RDM_Hold_Until.replace(":", "")}
                            require={true}
                            id="ariaHoldUntil"
                            key="h4"
                        >
                            <R.Validation>
                                <R.Combobox
                                    id="raManageHoldType"
                                    checkedField="checked"
                                    textField="title"
                                    valueField="value"
                                    searchable={false}
                                    width={300}
                                    disabled={false}
                                    items={this.state.holdTypeItems}
                                    onChange={this.onHoldTypeChange.bind(this)}
                                    aria={{
                                        ariaLabelledby: "ariaHoldUntil",
                                        ariaRequired: true,
                                    }}
                                />
                                {this.state.holdType == 0 && (
                                    <div className="ra-inline-middle margin-top-s margin-bottom-s">
                                        <R.Input
                                            id="raManageHoldUnitNumIpt"
                                            type="text"
                                            width={148}
                                            value={
                                                this.state.holdProfile.Number
                                            }
                                            onChange={this.onHoldTileChange.bind(
                                                this,
                                                "number",
                                            )}
                                            aria={{
                                                "aria-labelledby":
                                                    "ariaHoldUntil",
                                                "aria-required": true,
                                            }}
                                        />
                                        <span
                                            style={{
                                                width: "4px",
                                                display: "inline-block",
                                            }}
                                        />
                                        <R.Combobox
                                            id="raManageHoldUnit"
                                            checkedField="checked"
                                            textField="title"
                                            valueField="value"
                                            searchable={false}
                                            width={148}
                                            disabled={false}
                                            items={this.state.holdUnitItems}
                                            onChange={this.onHoldUnitChange.bind(
                                                this,
                                            )}
                                        />
                                    </div>
                                )}
                                {this.state.holdType == 1 && (
                                    <div
                                        id="hold-datapicker"
                                        className="margin-top-s margin-bottom-s"
                                    >
                                        <R.Validation>
                                            {this.renderHoldDatePicker()}
                                            <R.ValidationFaker
                                                valid={
                                                    !(
                                                        this.state.holdType ==
                                                            1 &&
                                                        this.state
                                                            .calenderTimeInvalid
                                                    )
                                                }
                                                of="#hold-datapicker"
                                                message={
                                                    RMResx.RM_JS_RDM_CreateRule_Validation_ConditionErrorDateTime
                                                }
                                            />
                                            <R.ValidationFaker
                                                valid={
                                                    !(
                                                        this.state.isSaving ==
                                                            true &&
                                                        this.state.holdType ==
                                                            1 &&
                                                        this.state.holdProfile
                                                            .CalenderTime ==
                                                            null
                                                    )
                                                }
                                                of="#hold-datapicker"
                                                message={
                                                    RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime
                                                }
                                            />
                                        </R.Validation>
                                    </div>
                                )}
                                <R.ValidationFaker
                                    valid={
                                        !(
                                            this.state.isSaving &&
                                            this.state.isSavingHoldNumber &&
                                            this.state.holdType == 0 &&
                                            !this.isAvailableNumber(
                                                this.state.holdProfile.Number,
                                            )
                                        )
                                    }
                                    of="#raManageHoldUnitNumIpt"
                                    message={RMResx.RM_JS_RDM_NotNumber}
                                />
                            </R.Validation>
                        </$g.FormRow>
                        <$g.FormRow
                            label={RMResx.RM_JS_JM_Comment}
                            require={false}
                            key="h5"
                        >
                            <R.Input
                                type="textarea"
                                value={this.state.holdProfile.Description}
                                width={300}
                                onChange={this.onHoldTileChange.bind(
                                    this,
                                    "comment",
                                )}
                                aria={{ ariaLabel: RMResx.RM_JS_JM_Comment }}
                            />
                        </$g.FormRow>
                        <$g.FormRow
                            label={RMResx.RM_JS_JM_HoldManager}
                            require={true}
                            id="ariaHoldConfigure"
                            key="h6"
                        >
                            <div style={{ fontSize: "13px" }}>
                                <div>{RMResx.RM_JS_HoldManager_Configure}</div>
                                <div className="margin-top-xs font-semibold">
                                    {
                                        RMResx.RM_JS_HoldManager_UserOrGroupName_Title
                                    }
                                </div>
                                <div className="margin-top-s">
                                    <R.Validation>
                                        <div id="holdManagerPickerWrapper">
                                            <PeoplePicker
                                                id="raHoldManagersPicker"
                                                height={78}
                                                width={300}
                                                items={
                                                    this.state.holdProfile
                                                        .HoldUserManagers || []
                                                }
                                                selectionChanged={
                                                    this.onHoldUsersChange
                                                }
                                                searchUsersByPermissionScope
                                            />
                                        </div>
                                        <R.ValidationFaker
                                            valid={
                                                !(
                                                    this.state
                                                        .submitAttempted &&
                                                    (!this.state.holdProfile
                                                        .HoldUserManagers ||
                                                        this.state.holdProfile
                                                            .HoldUserManagers
                                                            .length === 0)
                                                )
                                            }
                                            of="#holdManagerPickerWrapper"
                                            message={
                                                RMResx.RM_JS_JM_HoldManager
                                            }
                                        />
                                    </R.Validation>
                                </div>
                                <div className="margin-top-s">
                                    <R.Checkbox
                                        id="notifyHoldManagerChk"
                                        text={
                                            RMResx.RM_JS_HoldManager_Email_Notification
                                        }
                                        checked={
                                            this.state.holdProfile
                                                .IsHoldManagerEmailNotificationEnabled
                                        }
                                        onChange={
                                            this.onNotifyHoldManagerChange
                                        }
                                    />
                                </div>
                            </div>
                        </$g.FormRow>
                    </React.Fragment>
                )}
            </>
        );
    }
}
