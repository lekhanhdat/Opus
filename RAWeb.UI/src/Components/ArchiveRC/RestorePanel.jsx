import "../../Less/ArchiveRC/archiveRestoreCenter.less";
import { showToast } from "../../Utilities/CommonUtil";
import { LoadingButton } from "../Common/Button";
import PeoplePicker from "../Common/PeoplePicker";
import { MessageType } from "../CP/CPConstants";
import { DataSourceType, LevelType, RestoreType, Priority, RestoreDocumentVersionsOption, ConflictItems, AppConflictItems, RestoreOption, RestoreLevel } from "./Constants";

const CONVERSATION_TYPE = {
    Skip: -1,
    Html: 0,
    Delegate: 1,
}

const unsupportedRestoreToStorageSources = new Set([DataSourceType.FS, DataSourceType.Google]);

export default class RestorePanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.state = {
            restoreType: this.props.defaultRestoreType,
            restoreVersionOption: RestoreDocumentVersionsOption.None,
            spoLibOrFolderPath: "",
            lastVersionNumber: 1,
            conflictList: ConflictItems,
            conflictType: RestoreOption.Skip,
            appsConflictList: AppConflictItems,
            appsConflictType: RestoreOption.Skip,
            isWorkflowChecked: false,
            isShareLinkChecked: false,
            IsSupportLockedSite:false,
            storageList: [],
            storageDto: {},
            userList: [],
            isSpecifyUserChecked: false,
			searchedUser: [],
			priorityValue: 0,
            conversationType: CONVERSATION_TYPE.Html,
            isCalculating: false,
            dataImpact: {},
            isCalculateDone: false,
        };
        this.idJobCalculator = "";
        this.isUnmounted = false;
        this.timeoutTimer = null;
        this.addUserChanged = [];
        this.avepointStorageId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        this.isUnsupportedRestoreToStorage = unsupportedRestoreToStorageSources.has(this.props.dataSourceType);
		this.bind(['priorityValueChange']);
        this.isSupportConflictResolution = [RestoreType.InPlace, RestoreType.SPOLibOrFolder];
	}

    componentInit() {
        this.getAllStorage();
        this.setRestoreVersionOption();
    }

    componentReceive(type, args) {
        // Change back to switch when has multiple type
        if (type === "onSave") {
            this.onSave(args);
        }
    }

    componentDestroy() {
        this.isUnmounted = true;
        clearTimeout(this.timeoutTimer);
    }

    setRestoreVersionOption = () => {
        this.setState({
            restoreVersionOption: this.props.isShowVersionOption ? RestoreDocumentVersionsOption.SpecifyVersions : RestoreDocumentVersionsOption.None
        });
    }

    getAllStorage = () => {
        $$.loading(true);
        let urlData = "/api/StorageDevice/GetAllActiveStorage";
        let option = {
            url: urlData,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            let customStorageList = res || [];
            customStorageList = customStorageList.filter(s => !(s.IsSystemStorage || s.Id.toLowerCase() === this.avepointStorageId));
            this.setState({
                storageList: customStorageList,
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getPayloadRestore = (calculatePayload = false) => {
        let restoreDataObj = {
            RestoreTypeSelect: this.state.restoreType,
            NodeObjects: this.props.itemsChecked,
			DataSource: this.props.dataSourceType,
			JobPriority: this.state.priorityValue,
            RestoreConversationType: this.state.conversationType,
            IsSupportLockedSite: this.state.IsSupportLockedSite
        };

        if (this.state.restoreType == RestoreType.SPOLibOrFolder) {
            restoreDataObj.SPOLibOrFolderPath = this.state.spoLibOrFolderPath;
        }

        if (this.isSupportConflictResolution.includes(this.state.restoreType)) {
            restoreDataObj.RestoreOption = this.state.conflictType;
            restoreDataObj.RestoreAPPOption = this.state.appsConflictType;
            restoreDataObj.IncludeWorkflowDefinition = this.state.isWorkflowChecked;
            restoreDataObj.IncludeSharingLink = this.state.isShareLinkChecked;
            restoreDataObj.IsSpecifyUser = this.state.isSpecifyUserChecked;
            restoreDataObj.SpecifyUserList = this.state.isSpecifyUserChecked ? this.state.searchedUser : [];
        } else if (!calculatePayload){
            let newUserList = [];
            this.addUserChanged.forEach(user => {
                newUserList.push(user.data);
            });
            if (newUserList.length == 0) {
                return false;
            } else {
                restoreDataObj.NotificationUsers = newUserList;
                restoreDataObj.StorageDeviceDto = this.state.storageDto;
            }
        }
        restoreDataObj.RestoreVersionOption = this.state.restoreVersionOption;
        if (this.state.restoreVersionOption === RestoreDocumentVersionsOption.AllVersions) {
            restoreDataObj.KeepVersionsNumber = 1;
        } else {
            restoreDataObj.KeepVersionsNumber = this.state.lastVersionNumber;
        }
        if (this.props.isSelectedAll) {
            restoreDataObj.SerchContract = this.props.searchContract;
        }
        restoreDataObj.RestoreObjectLevel = this.props.searchLevel;

        return restoreDataObj;
    }

	onSave(callback) {
        if (!$$.verify(this.allValidation)||!$$.verify('#restore-combobox-value')) {
            return false;
        }
        
        let restoreDataObj = this.getPayloadRestore();
        $$.loading(true);
        let url = '/api/ArchiverRestore/SaveRestoreSettingAndRun';
        if (this.props.searchLevel === LevelType.SiteCollection) {
            url = '/api/ArchiverRestore/SaveMultiSiteCollectionRestoreSettingAndRun';
            if(this.props.isSelectedAll){
                restoreDataObj.NodeObjects = this.props.searchAllDate;
            }else{
                restoreDataObj.NodeObjects = this.props.itemsChecked.map((item)=> item.Origin);
            }
        }
        let option = {
            url,
            method: "Post",
            data: restoreDataObj
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == MessageType.Successful) {
                callback(true);
                const msg = this.props.searchLevel === LevelType.SiteCollection ? RMResx.RM_AR_RC_Panel_RunJobsSuccess : RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage;
                let content = <$g.I18NProvider msg={msg}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
                this.props.onClear();
            } else if (result.MessageType == MessageType.Exception) {
                callback(true);
                const content = (
                    <$g.I18NProvider msg={RMResx.RM_AR_RC_Panel_RunJobException}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>
                );
                showToast.success(content);
                this.props.onClear();
            } else {
                showToast.error(result.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        }).finally(() => {
			$$.loading(false);
			this.setState({ priorityValue: 0 });
        });
    }

    onRestoreTypeChanged = (args) => {
        this.setState({ restoreType: args });
    }

    onRestoreToSpoLibOrFolderPathChanged = (newValue) => {
        this.setState({ spoLibOrFolderPath: newValue });
    }

    onRestoreVersionOptionChanged = (args) => {
        this.setState({ restoreVersionOption: args });
    }
    
    onVersionNumberChanged = (args) => {
        this.setState({ lastVersionNumber: args });
    }

    onConflictChanged = (args) => {
        this.setState({ conflictType: args.newValue.value });
    }

    onAppsConflictChanged = (args) => {
        this.setState({ appsConflictType: args.newValue.value });
    }

    onWorkflowChanged = (args) => {
        this.setState({ isWorkflowChecked: args });
    }

    onShareLinkChanged = (args) => {
        this.setState({ isShareLinkChecked: args });
    }

    onSpecifyUserChanged = (args) => {
        this.setState({ isSpecifyUserChecked: args });
    }

    onSearchUser = (args) => {
        this.setState({ searchedUser: args });
    }

    onStorageChanged = (args) => {
        this.setState({ storageDto: args.newValue });
    }

    onSupportLockedSiteChanged = (args) => {
        this.setState({ IsSupportLockedSite: args });
    }

    onSearch = (args) => {
        let searchValue = args.key;
        let urlData = `/api/BCMCommonSettingApi/SearchAADUsers?tenantId=&key=${searchValue}`;
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

    onAddUserSelectionChanged = (args) => {
        let selections = RM.deepcopy(args.newValue);
        this.addUserChanged = selections;
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
	
	priorityValueChange(value) {
		this.setState({ priorityValue: value?.newValue?.value });
    }

    renderInPlaceContent() {
        return <div>
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx["StorageOptimization.Gui_Conflict Resolution"]}</span>
                    <$g.Popover>{!this.isUnsupportedRestoreToStorage ? RMResx.RM_AR_RC_Panel_ConflictResolutionDes : RMResx.RM_AR_RC_Panel_FSConflictResolutionDes}</$g.Popover>
                </div>
                <R.Combobox
                    id="raConflictCom"
                    tooltipField="name"
                    textField="name"
                    valueField="value"
                    checkedField="checked"
                    width='100%'
                    linkMode={false}
                    searchable={false}
                    items={this.state.conflictList}
                    onChange={this.onConflictChanged}
                    aria={{ ariaLabel: RMResx["StorageOptimization.Gui_Conflict Resolution"] }}
                />
            </div>
            {!this.isUnsupportedRestoreToStorage && <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx["StorageOptimization.Gui_B2838B85-02A7-4D8D-8B89-2A138EE3589B"]}</span>
                    <$g.Popover>{RMResx.RM_AR_RC_Panel_AppsDes}</$g.Popover>
                </div>
                <R.Combobox
                    id="raAppsConflictCom"
                    tooltipField="name"
                    textField="name"
                    valueField="value"
                    checkedField="checked"
                    width='100%'
                    linkMode={false}
                    searchable={false}
                    items={this.state.appsConflictList}
                    onChange={this.onAppsConflictChanged}
                    aria={{ ariaLabel: RMResx["StorageOptimization.Gui_B2838B85-02A7-4D8D-8B89-2A138EE3589B"] }}
                />
            </div>}
            {!this.isUnsupportedRestoreToStorage && <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx.RM_AR_RC_Panel_Options}</span>
                </div>
                <div>
                    <R.Checkbox
                        id="raWorkflowChk"
                        text={RMResx["StorageOptimization.Gui_Include workflow definition"]}
                        checked={this.state.isWorkflowChecked}
                        onChange={this.onWorkflowChanged}
                    />
                    <$g.Popover>{RMResx["StorageOptimization.Gui_D5FD180A-A9BC-415A-9C28-94F19EA447E5"]}</$g.Popover>
                </div>
                <div>
                    <R.Checkbox
                        id="raShareLinkChk"
                        text={RMResx["StorageOptimization.Gui_9DC59F76-D900-4F54-8ECD-5385AD1C7B8A"]}
                        checked={this.state.isShareLinkChecked}
                        onChange={this.onShareLinkChanged}
                    />
                    <$g.Popover>{RMResx["StorageOptimization.Gui_6A8A562B-6C2E-4DF7-8102-0B52DF23A94B"]}</$g.Popover>
                </div>
                {this.props.isShowSpecifyUserOption && this.renderUserOption()}
                <div>
                    <R.Checkbox
                        id="raSupportLockedSiteChk"
                        text={RMResx.RM_AR_RC_Panel_RestoreToLocked}
                        checked={this.state.IsSupportLockedSite}
                        onChange={this.onSupportLockedSiteChanged}
                    />
                    <$g.Popover>{RMResx.RM_AR_RC_Panel_RestoreToLockedDes}</$g.Popover>
                </div>
            </div>}
        </div>;
    }

    renderUserOption() {
        const isTeamsGroupsLevel = this.props.searchLevel == LevelType.Teams;
        const checkboxLabel = isTeamsGroupsLevel ? RMResx.RM_RS_CheckTeamsGroupsAdminOrOwnerIfUserNotExist : RMResx.RM_RS_CheckSiteAdminOrOwnerIfUserNotExist;
        return <div className="rc-user-checkbox">
            <R.Checkbox
                id="raUserChk"
                text={checkboxLabel}
                checked={this.state.isSpecifyUserChecked}
                onChange={this.onSpecifyUserChanged}
            />
            {this.state.isSpecifyUserChecked && <div className="rc-user-people-picker">
                <R.Validation element="RichCombobox" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                    <PeoplePicker
                        id="raRCSpecifyUser"
                        width={600}
                        items={this.state.searchedUser}
                        selectionChanged={this.onSearchUser}
                    />
                </R.Validation>
            </div>}
        </div>
    }

    renderOutOfPlaceContent() {
        return <div>
            <div className="rc-panel-content">
                <div className="rc-panel-title margin-bottom-s require">
                    <span>{RMResx.RM_AR_RC_Panel_Storage}</span>
                </div>
                <R.Validation
                    element="Combobox"
                    require={RMResx.RM_AR_CP_Common_SelEmpty}
                >
                    <R.Combobox
                        id="raStorage"
                        tooltipField="Name"
                        textField="Name"
                        valueField="Id"
                        checkedField="checked"
                        width='100%'
                        linkMode={false}
                        searchable={false}
                        items={this.state.storageList}
                        onChange={this.onStorageChanged}
                        aria={{ ariaLabel: RMResx.RM_AR_RC_Panel_Storage }}
                    />
                </R.Validation>
            </div>
            <div className="rc-panel-content">
                <div className="rc-panel-title margin-bottom-s require">
                    <span>{RMResx.RM_AR_RC_Panel_Notification}</span>
                </div>
                <R.Validation
                    element="RichCombobox"
                    require={RMResx.RM_JS_CP_AM_Owner_Require} >
                    <R.RichCombobox
                        asyncSearch
                        id="raNotification"
                        width="100%"
                        height={80}
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
                        doLoad={this.onSearch}
                        onChange={this.onAddUserSelectionChanged}
                    />
                </R.Validation>
            </div>
        </div>;
	}
	
    renderPriorityContent() {
        const labelWithPopover = (
            <span>
                {RMResx.RM_JS_JM_Priority}
                <$g.Popover>{RMResx.RM_JS_JM_Priority_Tooltip}</$g.Popover>
            </span>
        );
		return <div>
			<$g.FormRow label={labelWithPopover}>
				<R.Validation id="restore-combobox-value">
					<R.Validation
						element="Combobox"
						require={RMResx.RM_JS_JM_Priority_ErrorMsg}
                    >
						<R.Combobox
							id="restorePriorityCombobox"
							textField='name'
							valueField='value'
							checkedField='checked'
							waterMark='Select a Location'
							items={Priority}
							width={"100%"}
							searchable={false}
							onChange={this.priorityValueChange}
							triggerBySource={true}
							aria="tooltip_demo_labelledby"
						/>
					</R.Validation>
				</R.Validation>
            </$g.FormRow>
        </div>;
	}

    renderRestoreVersion() {
        return <div className="rc-panel-content">
            <div className="rc-panel-title">
                <span>{RMResx.RM_RestoreCenter_RestoreVersionTitle}</span>
                <$g.Popover>{RMResx.RM_RestoreCenter_RestoreDocumentVersionDescription}</$g.Popover>
            </div>
            <div role="radiogroup" aria-label={RMResx.RM_RestoreCenter_RestoreVersionTitle}>
                <R.Radio
                    name="radioVersion"
                    text={RMResx.RM_RestoreCenter_RestoreKeepSpecialVersion}
                    value={RestoreDocumentVersionsOption.SpecifyVersions}
                    checked={this.state.restoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions}
                    onChange={this.onRestoreVersionOptionChanged}
                />
                {this.state.restoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions && <div className="margin-top-s margin-left-xl">
                    <R.Validation
                        element="Input"
                        require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]}
                    >
                        <R.Input
                            id="raLastVersionNumberIpt"
                            type="number"
                            min={1}
                            max={10000}
                            value={this.state.lastVersionNumber}
                            onChange={this.onVersionNumberChanged}
                        />
                    </R.Validation>
                </div>
                }
                <div className="margin-top-s">
                    <R.Radio
                        name="radioVersion"
                        text={RMResx.RM_RestoreCenter_RestoreKeepAllVersion}
                        value={RestoreDocumentVersionsOption.AllVersions}
                        checked={this.state.restoreVersionOption == RestoreDocumentVersionsOption.AllVersions}
                        // disabled={this.props.isDisabled}
                        onChange={this.onRestoreVersionOptionChanged}
                    />
                </div>
            </div>
        </div>;

    }

    onConversationTypeChanged = (args) => {
        this.setState({ conversationType: args });
    }

    renderConversationType() {
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title margin-bottom-s">
                    <span>{RMResx.RM_RestoreCenter_Conversation}</span>
                </div>
                <div role="radiogroup" aria-label={RMResx.RM_RestoreCenter_Conversation} className="margin-left-m flex flex-column">
                    <R.Radio
                        className="margin-bottom-s"
                        name="radioConversation"
                        text={RMResx.RM_RestoreCenter_Skip_RestoreConversation}
                        value={CONVERSATION_TYPE.Skip}
                        checked={this.state.conversationType == CONVERSATION_TYPE.Skip}
                        onChange={this.onConversationTypeChanged}
                    />
                    <R.Radio
                        name="radioConversation"
                        text={RMResx.RM_RestoreCenter_ConversationAsHtml}
                        value={CONVERSATION_TYPE.Html}
                        checked={this.state.conversationType == CONVERSATION_TYPE.Html}
                        onChange={this.onConversationTypeChanged}
                    />
                    <div>
                        <R.Radio
                            name="radioConversation"
                            text={RMResx.RM_RestoreCenter_ConversationInPlace_Delegate}
                            value={CONVERSATION_TYPE.Delegate}
                            checked={this.state.conversationType == CONVERSATION_TYPE.Delegate}
                            onChange={this.onConversationTypeChanged}
                        />
                        <$g.Popover>{RMResx.RM_RestoreCenter_ConversationInPlace_Delegate_Tooltip}</$g.Popover>
                    </div>
                </div>
            </div>
        )
    }

    getLevelCount = () => {
        return Object.entries(this.state.dataImpact.LevelCountMap)
            .filter(([key]) => key !== "0")
            .map(([key, value]) => {
            return {
                title: RestoreLevel[key],
                value: value,
            };
        });
    }

    renderDataImpact = () => {
        const listImpact = [{
                title: RMResx.RM_RestoreCenter_TotalSize_Impact,
                value: this.state.dataImpact.SizeStr,
        }]
        listImpact.push(...this.getLevelCount());
        return (
            <div className="impact-list">
                {listImpact.map((item, key) => (
                    <div key={key} className="impact-item col-md-12">
                        <div className="col-md-4 impact-label ra-ellipsis">{item.title}</div>
                        <div className="col-md-8 impact-value ra-ellipsis">{item.value}</div>
                    </div>
                ))}
            </div>
        )
    }

    handleCalculateImpact = () => {
        let payloadRestore = this.getPayloadRestore(true);

        this.setState({ isCalculating: true });
        let url = '/api/ArchiverRestore/PreviewRestore';
        if (this.props.searchLevel === LevelType.SiteCollection) {
            url = '/api/ArchiverRestore/PreviewMultiSiteCollectionRestore';
            if(this.props.isSelectedAll){
                payloadRestore.NodeObjects = this.props.searchAllDate;
            }else{
                payloadRestore.NodeObjects = this.props.itemsChecked.map((item)=> item.Origin);
            }
        }
        let option = {
            url,
            method: "Post",
            data: payloadRestore
        };
        fetchUtility(option).then((result) => {
            if (result.MessageType == MessageType.Successful) {
                this.idJobCalculator = result.Extension;
                this.handleGetImpactResult();
            } else {
                showToast.error(result.ErrorMessage);
                this.setState({ isCalculating: false }); 
            }
        }).catch((e) => {
            this.setState({ isCalculating: false });
        });
    }

    handleGetImpactResult = () => {
        clearTimeout(this.timeoutTimer);
        let url = `/api/ArchiverRestore/GetPreviewRestoreResult?messageId=${this.idJobCalculator}`;
        let option = {
            url,
            method: "GET",
        };
        fetchUtility(option).then((result) => {
            if(this.isUnmounted) return;
            if(result.MessageType == MessageType.Successful) {
                const impactData = result.Extsion1;
                if(impactData) {
                    if(impactData.IsCompleted) {
                        this.setState({ dataImpact: impactData, isCalculateDone: true, isCalculating: false });
                        return;
                    } else {
                        if(this.isOver5Minutes(impactData?.UpdateTime || impactData?.StartTime)) {
                            this.setState({ isCalculating: false });
                            return;
                        }
                    }
                }
                this.timeoutTimer = setTimeout(() => this.handleGetImpactResult(), 15000);
            } else {
                this.setState({ isCalculating: false });
                showToast.error(result.ErrorMessage);
            }
        }).catch((e) => {
            this.setState({ isCalculating: false });
        });
    }

    isOver5Minutes = (updateTime) => {
        const fiveMinutes = 5 * 60 * 1000;
        return Date.now() - new Date(updateTime).getTime() > fiveMinutes;
    }

    renderRestoreImpact() {
        const over10Items = this.props.itemsChecked.length > 10 || (this.props.isSelectedAll && this.props.totalItems > 10);
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <div className="rc-panel-title">
                        <span>{RMResx.RM_RestoreCenter_RestoreImpact}</span>
                        <$g.Popover>{RMResx.RM_RestoreCenter_Impact_Description}</$g.Popover>
                    </div>
                </div>
                {over10Items ? (
                    <div className="location-validate-msg">
                        <R.Messagebar
                            message={RMResx.RM_RestoreCenter_Impact_Limit}
                            status={{ show: true }}
                            classify={"error"}
                        />
                    </div>
                ) : (
                    <>
                        {this.state.isCalculating ?
                            <LoadingButton
                                isBusy={true}
                                text={RMResx.RM_RestoreCenter_Loading_Btn}
                                disabled={true}
                                classify="alt"
                                primary={true}
                            />
                        :
                            <R.Button
                                icon="fia-calculator"
                                text={this.state.isCalculateDone ? RMResx.RM_RestoreCenter_ReCalculateImpact_Btn : RMResx.RM_RestoreCenter_CalculateImpact_Btn}
                                onClick={this.handleCalculateImpact}
                            />
                        }
                        {this.state.isCalculateDone && this.renderDataImpact()}
                    </>
                )}
            </div>
        )
    }
    
    render() {
        const supportingRestoreToSpoLibOrFolderLevels = [LevelType.Document, LevelType.Folder, LevelType.DocumentVersion];
        const supportingRestoreToSpoLibOrFolderSources = [DataSourceType.M365];
        const isTeamsGroupsLevel = this.props.dataSourceType === DataSourceType.Teams && this.props.levelSelected === LevelType.Teams;
        const isShowCalculateImpact = this.props.dataSourceType === DataSourceType.M365 && this.props.isSearchTab;
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="rc-panel-content">
                        <div className="rc-panel-title">
                            <span>{RMResx["StorageOptimization.Gui_Restore Type"]}</span>
                            <$g.Popover>{!this.isUnsupportedRestoreToStorage ? RMResx.RM_AR_RC_Panel_RestoreTypeDes : RMResx.RM_AR_RC_Panel_FSRestoreTypeDes}</$g.Popover>
                        </div>
                        <div role="radiogroup" aria-label={RMResx["StorageOptimization.Gui_Restore Type"]}>
                            <div>
                                <R.Radio
                                    name="radioType"
                                    text={RMResx["StorageOptimization.Gui_In place restore"]}
                                    value={RestoreType.InPlace}
                                    checked={this.state.restoreType == RestoreType.InPlace}
                                    disabled={this.props.isDisabledInPlace}
                                    onChange={this.onRestoreTypeChanged}
                                />
                            </div>
                            {!this.isUnsupportedRestoreToStorage && <div>
                                <R.Radio
                                    name="radioType"
                                    text={RMResx.RM_JS_JM_JobType_ArchiverOutPlaceRestore}
                                    value={RestoreType.OutOfPlace}
                                    checked={this.state.restoreType == RestoreType.OutOfPlace}
                                    disabled={this.props.isDisabled}
                                    onChange={this.onRestoreTypeChanged}
                                />
                                <$g.Popover>{RMResx["StorageOptimization.Gui_717A2F18-C463-4A8E-9A4D-A026CF999F85"]}</$g.Popover>
                            </div>}
                            {supportingRestoreToSpoLibOrFolderSources.includes(this.props.dataSourceType) && supportingRestoreToSpoLibOrFolderLevels.includes(this.props.searchLevel) && (
                                <>
                                    <div>
                                        <R.Radio
                                            name="radioType"
                                            text={RMResx.RM_JS_JM_JobType_ArchiverToSpoRestore}
                                            value={RestoreType.SPOLibOrFolder}
                                            checked={this.state.restoreType == RestoreType.SPOLibOrFolder}
                                            disabled={this.props.isDisabledSPOLibOrFolder}
                                            onChange={this.onRestoreTypeChanged}
                                        />
                                        <$g.Popover style={{ marginTop: 0, marginBottom: 2 }}>{RMResx.RM_JS_JM_JobType_ArchiverToSpoRestore_Desc}</$g.Popover>
                                    </div>
                                    {this.state.restoreType == RestoreType.SPOLibOrFolder && (
                                        <div style={{ marginLeft: 26 }} className="margin-top-xs">
                                            <R.Validation element="Input" require>
                                                <R.Input
                                                    id="raSpoLibOrFolderPathIpt"
                                                    type="text"
                                                    placeholder="https:/domain.com/sites/sitename/library/folder"
                                                    value={this.state.spoLibOrFolderPath}
                                                    onChange={this.onRestoreToSpoLibOrFolderPathChanged}
                                                />
                                            </R.Validation>
                                        </div>
                                    )}
                                </>
                            )}
                        </div>
                    </div>
                    {this.props.isShowVersionOption
                    && [DataSourceType.M365, DataSourceType.Teams, DataSourceType.Google].includes(this.props.dataSourceType)
                    && this.renderRestoreVersion()}
                    {isTeamsGroupsLevel && this.state.restoreType !== RestoreType.OutOfPlace && this.renderConversationType()}
                    {isShowCalculateImpact && this.renderRestoreImpact()}
                    {this.isSupportConflictResolution.includes(this.state.restoreType) && this.renderInPlaceContent()}
                    {this.state.restoreType == RestoreType.OutOfPlace && this.renderOutOfPlaceContent()}
                    {this.renderPriorityContent()}
                </div>
            </R.Validation>
        </div>;
    }
}