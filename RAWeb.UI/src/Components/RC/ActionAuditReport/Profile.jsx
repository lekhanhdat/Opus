import { Component, createRef } from "react";
import { Prompt } from 'react-router';
import { JobType, SourceFlags, TelemetryEventType, TelemetryModule, TreeType } from "../../../Constants/Constants";
import RouterUrls from "../../../Constants/RouterUrls";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import "../../../Less/RC/commonReportProfile.less";
import { showToast } from "../../../Utilities/CommonUtil";
import StringUtil from "../../../Utilities/StringUtil";
import { ActionTypeCol, AuditEventType, AuditObjType, ObjTypeCol, RangeTypes, TreeScopeType, UserScopeType } from "../Constants";
import SPTree from "../../../Components/Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import { TabIndex } from "../../BCM/ContentRepositoryManagement/CRMForSPO";
import CommonExportDestination, { ExportDestinationEnums } from "../CommonExportDestination";
import { NewScheduleSetting } from "../../Common/NewScheduleSetting";
import { SCHEDULE_TYPES } from "../../Common/NewScheduleSetting/constants";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        this.addUserChanged = [];
        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.isEdit = !!this.profileId;
        this.timeInfo = RM.TimeUtil.getTodayStartEndTime();
        this.scheduledSettingsRef = createRef();
        let profile = {
            Type: RM.Url.getParam(window.location.href, "type") || JobType.SPOActionAuditReport,
            RangeType: RangeTypes["5D"]
        };
        this.NameMaxLength = 250;
        this.DespMaxLength = 250;
        this.spNodeItem = null;
        this.state = {
            showSourceTreeError: false,
            profile: profile,
            showDatePicker: false,
            sourceTreeData: null,
            settingsChanged: false,
            isRender: false,
            userList: [],
            extensionData: {
                UserScope: UserScopeType.All,
                TreeScope: TreeScopeType.Special,
                userInfos: [],
                ActionType: AuditEventType.All,
                ObjType: AuditObjType.All,
                FilterStr: "",
            },
            timeInfo: this.timeInfo,
            dateRanges: this.getDateRanges(),
            actionType: RM.deepcopy(ActionTypeCol),
            objType: RM.deepcopy(ObjTypeCol),
            exportDestination: ExportDestinationEnums.OpusDownloadCenter,
            destinationTreeData: null,
            scheduleData: {
                scheduleType: SCHEDULE_TYPES.NONE,
            },
            showDestinationTreeError: false,
        };
    }

    componentDidMount() {
        if (this.isEdit) {
            this.initProfileData();
        } else {
            this.setState({ isRender: true });
        }
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    getDateRanges() {
        return [
            {
                text: RMResx.RM_RC_Audit_Range_5D,
                title: RMResx.RM_RC_Audit_Range_5D,
                value: RangeTypes["5D"],
                checked: true
            },
            {
                text: RMResx.RM_RC_Audit_Range_1M,
                title: RMResx.RM_RC_Audit_Range_1M,
                value: RangeTypes["1M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_3M,
                title: RMResx.RM_RC_Audit_Range_3M,
                value: RangeTypes["3M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_6M,
                title: RMResx.RM_RC_Audit_Range_6M,
                value: RangeTypes["6M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_Custom,
                title: RMResx.RM_RC_Audit_Range_Custom,
                value: RangeTypes["Custom"],
                checked: false
            }
        ];
    }

    initProfileData() {
        let option = {
            url: "/api/ActionAuditReportApi/LoadProfileById",
            method: "Post",
            data: this.profileId
        };
        fetchUtility(option).then((data) => {
            const destinationData = data.Extension3 ? $.parseJSON(data.Extension3) : null;            
            if (!data.Extension1) {
                data.Extension1 = null;
            }

            var extension1Data = JSON.parse(data.Extension1);
            if (extension1Data.userInfos) {
                let newUsers = this.convertUsersToRichCombobox(extension1Data.userInfos);
                this.addUserChanged = newUsers;
                this.setState({
                    userList: newUsers,
                });
            }
            if (data.RangeType == RangeTypes["Custom"]) {
                this.setSelectDateRangTime(extension1Data);
            }
            this.setSelectDateRang(data.RangeType);
            this.isTreeDataLoaded = true;
            this.setState({
                profile: data,
                extensionData: extension1Data,
                sourceTreeData: $.parseJSON(data.Extension2),
                destinationTreeData: destinationData,
                scheduleData: data.scheduleInfo
                    ? {
                          ...data.scheduleInfo,
                          scheduleType: SCHEDULE_TYPES.CONFIGURE,
                      }
                    : {},
                exportDestination: data.Extension3 ? ExportDestinationEnums.SelectFromTree : ExportDestinationEnums.OpusDownloadCenter,
                isRender: true
            });

            let actionTypes = this.state.actionType;
            let objTypes = this.state.objType;
            if (extension1Data.ActionType != AuditEventType.All) {
                for (let index = 0; index < actionTypes.length; index++) {
                    const element = actionTypes[index];
                    element.checked = false;
                }

                for (const key in AuditEventType) {
                    if (Object.hasOwnProperty.call(AuditEventType, key)) {
                        const element = AuditEventType[key];
                        if ((extension1Data.ActionType & element) == element) {
                            let typeItem = actionTypes.find(v => v.value == element);
                            if (typeItem) {
                                typeItem.checked = true;
                            }
                        }
                    }
                }
                this.setState({ actionType: RM.deepcopy(actionTypes) });
            }
            if (extension1Data.ObjType != AuditObjType.All) {
                for (let index = 0; index < objTypes.length; index++) {
                    const element = objTypes[index];
                    element.checked = false;
                }

                for (const key in AuditObjType) {
                    if (Object.hasOwnProperty.call(AuditObjType, key)) {
                        const element = AuditObjType[key];
                        if ((extension1Data.ObjType & element) == element) {
                            let typeItem = objTypes.find(v => v.value == element);
                            if (typeItem) {
                                typeItem.checked = true;
                            }
                        }
                    }
                }
                this.setState({ objType: RM.deepcopy(objTypes) });
            }
        }).catch((e) => {

        });
    }

    convertUsersToRichCombobox(users) {
        let newUsers = [];
        users.forEach(user => {
            newUsers.push({
                name: user.DisplayName,
                // sub: user.DisplayName,
                value: user.UserId,
                disabled: false,
                tooltip: user.UserPrincipalName,
                readonly: false,
                invalid: false,
                conflict: false,
                data: user,
            });
        });
        return newUsers;
    }

    setSelectDateRang(type) {
        let items = RM.deepcopy(this.state.dateRanges);
        for (let item of items) {
            if (parseInt(item.value) === type) {
                item.checked = true;
            } else {
                item.checked = false;
            }
        }
        let showDatePicker = type == RangeTypes["Custom"] ? true : false;
        this.setState({
            dateRanges: items,
            showDatePicker: showDatePicker
        });
    }

    setSelectDateRangTime(item) {
        if (item) {
            this.timeInfo = { start: new Date(item.StartDateTime), end: new Date(item.EndDateTime) };
            this.setState({
                timeInfo: this.timeInfo
            });
        }
    }

    onDateRangChange = (value) => {
        let selValue = value;
        let profile = this.state.profile;
        let showDatePicker = selValue == RangeTypes["Custom"] ? true : false;
        profile.RangeType = selValue;
        this.setState({
            profile: profile,
            showDatePicker: showDatePicker,
            settingsChanged: true
        });
    }

    onNameChange = (value) => {

        let profile = this.state.profile;
        if (value.length == 0)
        {
            this.setState({ showRequireNameMsg: true });
        }
        else
        {
            this.setState({ showRequireNameMsg: false });
        }         
        if (value.length > this.NameMaxLength)
        {
            this.setState({ showRequireNameTooLongMsg: true });
        }
        else
        {          
            this.setState({ showRequireNameTooLongMsg: false });  
            profile.ProfileName = value.trim();
            this.setState({ profile: profile, settingsChanged: true });
        }
    }

    onDescriptionChange = (value) => {

        let profile = this.state.profile;
        if (value.length > this.DespMaxLength)
        {
            this.setState({ showDescriptionTooLongMsg: true });
        }
        else
        {
            this.setState({ showDescriptionTooLongMsg: false }); 
            profile.Description = value;
            this.setState({ profile: profile, settingsChanged: true });
        }

    }

    onSelectTime = (args) => {
        this.timeInfo = args.newValue;
        this.setState({ settingsChanged: true });
    }

    onUserScopeChanged = (args) => {
        let exData = this.state.extensionData;
        if (args == UserScopeType.All) {
            exData.userInfos = [];
        }
        exData.UserScope = args;
        this.setState({ extensionData: exData, settingsChanged: true });
    }

    onTreeScopeChanged = (args) => {
        let exData = this.state.extensionData;
        let profile = this.state.profile;
        exData.TreeScope = args;
        if (args == TreeScopeType.All) {
            profile.Extension2 = null;
            this.setState({ profile: profile, showSourceTreeError: false });
        }
        this.setState({ extensionData: exData, settingsChanged: true });
    }

    onSearchUser = (args) => {
        let searchValue = args.key;
        let urlData = `/api/BCMCommonSettingApi/SearchAADUsers?tenantId=&key=${searchValue}&onlyFromRecord=false&onlyIncludeAAdUser=true`;
        let option = {
            url: urlData,
            method: "get"
        };
        if (searchValue) {
            return fetchUtility(option).then((res) => {
                let users = RM.deepcopy(res.Users);
                return this.convertUsersToRichCombobox(users);
            }).catch((e) => {

            });
        }
    }

    onAddUserChanged = (args) => {
        let users = RM.deepcopy(args.newValue);
        this.addUserChanged = users;
        this.setState({ settingsChanged: true });
    }

    onActionTypeChanged = (actionType) => {
        let exData = this.state.extensionData;
        let actionTypeResult = 0;
        if (actionType.isSelectAll) {
            exData.ActionType = AuditEventType.All;
        } else {
            let actionTypeLists = actionType.newValue.map(v => v.value);
            for (let index = 0; index < actionTypeLists.length; index++) {
                const element = actionTypeLists[index];
                actionTypeResult = actionTypeResult | element;
            }
            exData.ActionType = actionTypeResult;
        }
        this.setState({ extensionData: exData, settingsChanged: true });
    }

    onObjTypeChanged = (objType) => {
        let exData = this.state.extensionData;
        let objTypeResult = 0;
        if (objType.isSelectAll) {
            exData.ObjType = AuditObjType.All;
        } else {
            let objTypeLists = objType.newValue.map(v => v.value);
            for (let index = 0; index < objTypeLists.length; index++) {
                const element = objTypeLists[index];
                objTypeResult = objTypeResult | element;
            }
            exData.ObjType = objTypeResult;
        }
        this.setState({ extensionData: exData, settingsChanged: true });
    }

    onScopeUrlChange = (scopeUrl) => {
        let exData = this.state.extensionData;
        exData.FilterStr = scopeUrl;
        this.setState({ extensionData: exData, settingsChanged: true });
    }

    onTriggerSourceTreeError(show) {
        this.setState({ showSourceTreeError: show });
    }

    onTreeChanged = () => {
        this.setState({ settingsChanged: true });
    }

    onNodeSelectedChange = () => {
        this.setState({ showSourceTreeError: false });
    }

    onSave = () => {
        let validSuccess = true;
        if (!$$.verify(this.allValidation)) {
            validSuccess = false;
        }
        let profile = this.state.profile;
        let extension1 = this.state.extensionData;
        let newState = {};
        let sourceTreeData = null;
        let isSelectDestination = this.state.exportDestination === ExportDestinationEnums.SelectFromTree
        let destinationTreeData = isSelectDestination && this.ruleMoveTree.getTreeData();
        const isNoSchedule = this.state.scheduleData?.scheduleType === SCHEDULE_TYPES.NONE
        if (this.state.showRequireNameTooLongMsg || this.state.showDescriptionTooLongMsg)
        {
            validSuccess = false;
        }

        if (this.refSourceTree) {
            sourceTreeData = this.refSourceTree.getTreeData();
            profile.Extension2 = JSON.stringify(sourceTreeData.items);
        }

        if (this.state.showDatePicker) {
            extension1.StartDateTime = RM.TimeUtil.getCommonDateStr(this.timeInfo.start);
            extension1.EndDateTime = RM.TimeUtil.getCommonDateStr(this.timeInfo.end);
            if (!this.timeInfo) {
                validSuccess = false;
            }
        }

        if (extension1.UserScope == UserScopeType.Special) {
            let newUserList = [];
            this.addUserChanged.forEach(user => {
                newUserList.push(user.data);
            });
            if (newUserList.length == 0) {
                validSuccess = false;
            } else {
                extension1.userInfos = newUserList;
            }
        }

        if ((sourceTreeData != null && sourceTreeData.selected) || (extension1.TreeScope == TreeScopeType.All)) {
            newState.showSourceTreeError = false;
        } else {
            validSuccess = false;
            newState.showSourceTreeError = true;
        }
        if (isSelectDestination) {
             if (destinationTreeData && this.spNodeItem) {
                 newState.showDestinationTreeError = false;
             } else {
                 validSuccess = false;
                 newState.showDestinationTreeError = true;
             }
        }
        if (validSuccess) {
            $$.loading(true);
            profile.Id = this.isEdit ? this.state.profile.Id : 0;
            profile.Modified = new Date();
            profile.Extension1 = JSON.stringify(extension1);
            profile.Extension3 = !isNoSchedule && this.state.exportDestination == ExportDestinationEnums.SelectFromTree ? JSON.stringify(this.ruleMoveTree.getTreeData()) : null; 
            profile.FullPath = !isNoSchedule && this.state.exportDestination == ExportDestinationEnums.SelectFromTree ? this.spNodeItem.FullPath : null;
            profile.scheduleInfo = this.getScheduleData() ? this.isEdit
                ? {
                      ...this.getScheduleData(),
                      Id: profile.scheduleInfo?.Id,
                  }
                : this.getScheduleData() : null;    
            let option = {
                url: this.isEdit ? "/api/ActionAuditReportApi/EditProfile" : "/api/ActionAuditReportApi/CreateProfile",
                data: profile
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);
                if (res == "") {
                    if (this.isEdit) {
                        RM.CommStatus.save(RM.CommStatus.EditSuccess);
                    } else {
                        RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                        addTelemetryRecord(TelemetryModule.ReportCenter, TelemetryEventType.ActionAuditProfile);
                    }
                    this.setState({ settingsChanged: false });
                    this.routerTo(RouterUrls.RC_ActionAuditReportManagement);
                } else {
                    let tipMsg = this.isEdit ? RMResx.RM_JS_RC_TUR_EditProfileFaild : RMResx.RM_JS_RC_TUR_CreateProfileFaild;
                    showToast.error(StringUtil.stringFormat(tipMsg, res));
                }
            }).catch((e) => {
            });
        } else {
            this.setState(newState);
        }
    }

    onCancel = () => {
        this.routerTo(RouterUrls.RC_ActionAuditReportManagement);
    }

    renderDateRangDatePiker() {
        if (this.state.showDatePicker) {
            let timeInfo = this.state.timeInfo;
            if (timeInfo && timeInfo.start != null && timeInfo.end != null) {
                return <R.Rangepicker
                    id="raRcActionCustomTime"
                    selectedDate={timeInfo}
                    data-part="vtWidget"
                    dateTimeFormat={RM.TimeSettingModel.DateFormat}
                    width={320}
                    onChange={this.onSelectTime}
                />;
            }
        }
    }

    renderSourceTree() {
        if ((!this.isEdit) ||(this.isEdit && this.isTreeDataLoaded)) {
            let SourceTree = null;
            let profileType = this.state.profile.Type;
            let sourceTreeFlags = SourceFlags.SP;
            if (profileType == JobType.SPOActionAuditReport) {
                SourceTree = SPTree;
            } else if (profileType == JobType.OneDriveActionAuditReport) {
                SourceTree = SPTree;
                sourceTreeFlags = SourceFlags.OneDrive;
            } else if (profileType == JobType.TeamsActionAuditReport) {
                SourceTree = ReportTeamsTree;
                sourceTreeFlags = SourceFlags.Teams;
            }
            if (SourceTree) {
                return <SourceTree
                    ref={r => this.refSourceTree = r}
                    searchKey={this.state.sourceSearchKey}
                    data={this.state.sourceTreeData}
                    onTreeChanged={this.onTreeChanged}
                    onNodeSelectedChange={this.onNodeSelectedChange}
                    treeSource={sourceTreeFlags}
                    treeType={TreeType.ActionReport}
                    mode={TabIndex.Archive}
                />;
            }
        }
    }

    getScheduleData() {
        return this.scheduledSettingsRef.current?.getScheduleData();
    }

    handleScheduleTypeChange = (scheduleType) => {
        const isNoSchedule = scheduleType === SCHEDULE_TYPES.NONE

        this.setState((previousState) => ({
            scheduleData: {
                ...previousState.scheduleData,
                scheduleType,
            },
            ...(isNoSchedule
                ? {
                    exportDestination: ExportDestinationEnums.OpusDownloadCenter,
                    destinationTreeData: null,
                    showDestinationTreeError: false,
                } : {}),
        }));

        if (isNoSchedule) {
            this.spNodeItem = null;
            this.ruleMoveTree = null;
        }
    }

    renderScheduleSetting() {
        return <div>
            <div className="reco-report-profile-tree-input-title" tabIndex="0">
                    ^Scheduled report generation{" "}
                    <$g.Popover>
                        {"^Automatically generate and export reports based on a recurring schedule."}
                    </$g.Popover>
            </div>
            <div className="reco-report-profile-tree-input-title" tabIndex="0">
                ^How often would you like to generate the report?
            </div>
            <NewScheduleSetting
                ref={this.scheduledSettingsRef}
                scheduleData={this.state.scheduleData}
                onScheduleTypeChange={this.handleScheduleTypeChange}
            />

        </div>
    }

    onTriggerDestinationTreeError = (show) => {
        this.setState({ showDestinationTreeError: show });
    }

    onDestTreeSelectedChanged = (nodeItem) => {
        this.spNodeItem = nodeItem;
        this.setState({
            showDestinationTreeError: false,
        });
    }

    render() {
        return <div>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="reco-report-profile-wrapper">
                        <section className="reco-report-profile-header">
                            <Prompt message={RMResx.RM_JS_RC_TUR_CancelMessage} when={this.state.settingsChanged} />
                            <$g.SiteMap
                                data={[SiteMapLinks.RC_ActionAuditReportManagement, { text: this.isEdit ? RMResx.RM_JS_Common_Edit : RMResx.RM_JS_Common_Create }]} />
                        </section>
                        <section className="reco-report-profile-card">
                            <div className="reco-report-profile-form">
                                <div className="reco-report-profile-form-item">
                                    <span className="reco-report-profile-input-title require">
                                        {RMResx.RM_JS_RC_DueDisposal_ProfileName}
                                    </span>
                                    <R.Validation
                                        element="Input"
                                        require={RMResx.RM_RC_DueDisposal_NoProfileName} >
                                        <R.Input
                                            id="raRcActionProfileNameIpt"
                                            type="text"
                                            value={this.state.profile.ProfileName}
                                            onChange={this.onNameChange}
                                            aria={{ ariaLabel: RMResx.RM_JS_RC_DueDisposal_ProfileName }} />
                                    </R.Validation>
                                    <$g.ValidationMsg show={this.state.showRequireNameTooLongMsg}>
                                        {RMResx.RM_RC_DueDisposal_ProfileNameTooLong}
                                    </$g.ValidationMsg>
                                </div>
                                <div className="reco-report-profile-form-item">
                                    <span className="reco-report-profile-input-title">
                                        {RMResx.RM_RC_Profile_Description}
                                    </span>
                                    <R.Input
                                        id="raRcActionDesIpt"
                                        type="textarea"
                                        value={this.state.profile.Description}
                                        onChange={this.onDescriptionChange}
                                        aria={{ ariaLabel: RMResx.RM_RC_Profile_Description }} />
                                    <span className="reco-report-profile-input-desc">
                                        {RMResx.RM_RC_Profile_Description_Tips}
                                    </span>
                                    <$g.ValidationMsg show={this.state.showDescriptionTooLongMsg}>
                                        {RMResx.RM_RC_DueDisposal_DescriptionTooLong}
                                    </$g.ValidationMsg>
                                </div>
                                <div className="reco-report-profile-form-item">
                                    <span id="ariaRangTime" className="reco-report-profile-input-title require">
                                        {RMResx.RM_RC_ActionAudit_TimeFrame}
                                    </span>
                                    <R.Radio.Group
                                        block
                                        name="radioGroupTimeType"
                                        items={this.state.dateRanges}
                                        onChange={this.onDateRangChange}
                                        aria="#ariaRangTime"
                                    />
                                    <div className="reco-report-profile-datarange-selector">
                                        {this.renderDateRangDatePiker()}
                                    </div>
                                </div>
                            </div>
                            <div className="reco-report-profile-tips">
                                <div className="reco-report-profile-tips-header">
                                    <span className="reco-report-profile-tips-icon fia-light">
                                    </span>
                                    <span className="reco-report-profile-tips-header-title" tabIndex="0">
                                        {RMResx.RM_Report_SectionTitle_Introduction}
                                    </span>
                                </div>
                                <div className="reco-report-profile-tips-content" tabIndex="0">
                                    {RMResx.RM_RC_ActionAudit_Desc}
                                </div>
                                <div className="reco-report-profile-tips-pic"></div>
                            </div>
                        </section>
                        <section className="reco-report-profile-scope-card">
                            <div className="reco-report-profile-scope">
                                <div className="reco-report-profile-scope-div">
                                    <span className="reco-report-profile-scope-title">
                                        {RMResx.RM_RC_ActionAudit_UserScope}
                                    </span>
                                    <div className="reco-report-profile-scope-btm">
                                        <div role="radiogroup" aria-label={RMResx.RM_RC_ActionAudit_UserScope}>
                                            <div className="reco-report-profile-scope-radio">
                                                <R.Radio
                                                    name="raRcActionUser"
                                                    text={RMResx.RM_RC_ActionAudit_AllUsers}
                                                    value={UserScopeType.All}
                                                    checked={this.state.extensionData.UserScope == UserScopeType.All}
                                                    onChange={this.onUserScopeChanged}
                                                />
                                            </div>
                                            <div className="reco-report-profile-scope-radio">
                                                <R.Radio
                                                    name="raRcActionUser"
                                                    text={RMResx.RM_RC_ActionAudit_SpecialUser}
                                                    value={UserScopeType.Special}
                                                    checked={this.state.extensionData.UserScope == UserScopeType.Special}
                                                    onChange={this.onUserScopeChanged}
                                                />
                                                {this.state.extensionData.UserScope == UserScopeType.Special && <div>
                                                    <div className="reco-report-profile-scope-radio">
                                                        <R.Validation
                                                            element="RichCombobox"
                                                            require={RMResx.RM_RC_ActionAudit_Error_SpecialUser} >
                                                            <R.RichCombobox
                                                                asyncSearch
                                                                width={400}
                                                                value={this.state.userList}
                                                                searchPlaceholder={RMResx.RM_Common_PeoplePicker_Watermark}
                                                                disabled={false}
                                                                textField="name"
                                                                valueField="value"
                                                                template="profile"
                                                                itemTemplate="profile"
                                                                checkedField="checked"
                                                                tooltipField="tooltip"
                                                                disabledField="disabled"
                                                                readonlyField="readonly"
                                                                invalidField="invalid"
                                                                groupField={null}
                                                                matchFields={{ 'name': false }}
                                                                searchable={true}
                                                                singleMode={false}
                                                                silence={false}
                                                                excludeChecked={true}
                                                                doLoad={this.onSearchUser}
                                                                onChange={this.onAddUserChanged}
                                                            />
                                                        </R.Validation>
                                                    </div>
                                                </div>}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div className="reco-report-profile-scope-div">
                                    <span className="reco-report-profile-scope-title">
                                        {RMResx.RM_RC_ActionAudit_ActionType}
                                    </span>
                                    <div className="reco-report-profile-scope-btm">
                                        <div className="reco-report-profile-scope-radio">
                                            <R.Multicombobox
                                                id="raRcActionTypeCbb"
                                                width={300}
                                                items={this.state.actionType}
                                                disabled={false}
                                                textField="name"
                                                valueField="value"
                                                checkedField="checked"
                                                tooltipField="tooltip"
                                                disabledField="disabled"
                                                required={true}
                                                linkMode={false}
                                                onChange={this.onActionTypeChanged}
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div className="reco-report-profile-scope-div">
                                    <span className="reco-report-profile-scope-title">
                                        {RMResx.RM_RC_ActionAudit_ObjType}
                                    </span>
                                    <div className="reco-report-profile-scope-btm">
                                        <div className="reco-report-profile-scope-radio">
                                            <R.Multicombobox
                                                id="raRcObjTypeCbb"
                                                width={300}
                                                items={this.state.objType}
                                                disabled={false}
                                                textField="name"
                                                valueField="value"
                                                checkedField="checked"
                                                tooltipField="tooltip"
                                                disabledField="disabled"
                                                required={true}
                                                linkMode={false}
                                                onChange={this.onObjTypeChanged}
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div className="reco-report-profile-scope-div">
                                    {this.renderScheduleSetting()}
                                    {this.state.scheduleData.scheduleType === SCHEDULE_TYPES.CONFIGURE && <div className="margin-top-l">
                                        <CommonExportDestination
                                            value={this.state.exportDestination}
                                            onChange={(value) =>
                                                this.setState({
                                                    exportDestination: value
                                                })
                                            }
                                            treeData={this.state.destinationTreeData}
                                            treeRef={(r) => (this.ruleMoveTree = r)}
                                            onSelectedNodeChanged={
                                                this.onDestTreeSelectedChanged
                                            }
                                        />
                                        <R.Messagebar
                                            message={"^Please specify a destination."}
                                            classify={"error"}
                                            onClose={this.onTriggerDestinationTreeError.bind(this, false)}
                                            status={{ show: this.state.exportDestination === ExportDestinationEnums.SelectFromTree && this.state.showDestinationTreeError }}
                                        />
                                    </div>}
                                    <span className="reco-report-profile-scope-title require margin-top-l">
                                        {RMResx.RM_RC_ActionAudit_TreeScope}
                                    </span>
                                    <div className="reco-report-profile-tree-search-message">
                                        <R.Messagebar
                                            message={RMResx.RM_JS_RC_DueDisposal_NoSelectedTree}
                                            classify={"error"}
                                            onClose={this.onTriggerSourceTreeError.bind(this, false)}
                                            status={{ show: this.state.showSourceTreeError }}
                                        />
                                    </div>
                                    <div className="reco-report-profile-scope-btm">
                                        <div role="radiogroup" aria-label={RMResx.RM_RC_ActionAudit_TreeScope}>
                                            <div className="reco-report-profile-scope-radio">
                                                <R.Radio
                                                    name="raRcActionReportScope"
                                                    text={RMResx.RM_RC_ActionAudit_AllTree}
                                                    value={TreeScopeType.All}
                                                    checked={this.state.extensionData.TreeScope == TreeScopeType.All}
                                                    onChange={this.onTreeScopeChanged}
                                                />
                                            </div>
                                            <div className="reco-report-profile-scope-radio">
                                                <R.Radio
                                                    name="raRcActionReportScope"
                                                    text={RMResx.RM_RC_ActionAudit_SpecialTree}
                                                    value={TreeScopeType.Special}
                                                    checked={this.state.extensionData.TreeScope == TreeScopeType.Special}
                                                    onChange={this.onTreeScopeChanged}
                                                />
                                                {this.state.extensionData.TreeScope == TreeScopeType.Special && <div className="reco-report-profile-tree">
                                                    {this.renderSourceTree()}
                                                </div>}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div>
                                    <span className="reco-report-profile-scope-title">
                                        {RMResx.RM_RC_ActionAudit_UrlScope}
                                    </span>
                                    <div className="reco-report-profile-scope-radio">
                                        <R.Input
                                            id="raRcActionProfileUrlIpt"
                                            type="text"
                                            width={400}
                                            value={this.state.extensionData.FilterStr}
                                            onChange={this.onScopeUrlChange}
                                            aria={{ ariaLabel: RMResx.RM_RC_ActionAudit_UrlScope }}
                                        />
                                    </div>
                                </div>
                            </div>
                        </section>
                        <section className="reco-report-profile-placeholder"></section>
                        <section className="reco-report-profile-actions">
                            <R.Button
                                id="raRcActionAuditProfileCancelBtn"
                                text={RMResx.RM_JS_Common_Cancel}
                                onClick={this.onCancel} />
                            <R.Button
                                id="raRcActionAuditProfileSaveBtn"
                                primary={true}
                                classify="theme"
                                text={RMResx.RM_JS_Common_Save}
                                onClick={this.onSave} />
                        </section>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}