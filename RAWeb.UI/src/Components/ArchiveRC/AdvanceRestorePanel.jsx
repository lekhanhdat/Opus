import SPDestinationTree from "../Common/Tree/Instances/SPTree/SPDestinationTree";
import { AdvanceLocationType, AdvanceRestoreScope, AdvanceRestoreType, ConflictItems, DataSourceType, Priority, RestoreDocumentVersionsOption, RestoreOption } from "./Constants";
import "../../Less/ArchiveRC/archiveRestoreCenter.less";
import TeamsDestinationTree from "../Common/Tree/Instances/TeamsTree/TeamsDestinationTree";
import { MessageType } from "../CP/CPConstants";
import { showToast } from "../../Utilities/CommonUtil";

export default class AdvanceRestorePanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            restoreType: AdvanceRestoreType.Microsoft365InPlace,
            locationType: AdvanceLocationType.Url,
            restoreScope: AdvanceRestoreScope.IncludeChildren,
            urlLocation: "",
            treeLocation: "",
            destinationActiveTab: 0,
            locationTreeData: "",
            locationTeamsTreeData: "",
            noSelectNode: false,
            priorityValue: Priority[1].value,
            restoreVersion: RestoreDocumentVersionsOption.SpecifyVersions,
            conflictList: ConflictItems,
            conflictType: RestoreOption.Skip,
            isWorkflow: false,
            isShareLink: false,
            isSupportLockedSite: false,
            listNode: [],
            noLocation: false,
            isLocationValidate: false,
            LocationValidate: "",
            LocationValidateType: "",
            dataSource: DataSourceType.M365,
            lastVersionNumber: 1,
        }
    }

    componentReceive(type, args) {
        if(type === 'onSave') {
            this.onSave(args);
        }
    }

    onSave = async (callback) => {
        if(this.state.locationType === AdvanceLocationType.Url) {
            const isValidated = await this.checkLocation();
            if (!isValidated) {
                return;
            }
        } else if(this.state.locationType === AdvanceLocationType.Tree) {
            if(this.state.locationTreeData === "" && this.state.locationTeamsTreeData === "") {
                this.setState({ noSelectNode: true });
                return;
            }
        }
        const fullPatch = this.state.dataSource === DataSourceType.M365 ? this.state.locationTreeData : this.state.locationTeamsTreeData;
        const restoreData = {
            RestoreTypeSelect: this.state.restoreType,
            DataSource: this.state.dataSource,
            JobPriority: this.state.priorityValue,
            IsSupportLockedSite: this.state.isSupportLockedSite,
            SPOLibOrFolderPath: this.state.locationType === AdvanceLocationType.Url ? this.state.urlLocation : fullPatch,
            RestoreOption: this.state.conflictType,
            IncludeWorkflowDefinition: this.state.isWorkflow,
            IncludeSharingLink: this.state.isShareLink,
            RestoreVersionOption: this.state.restoreVersion,
            KeepVersionsNumber: this.state.lastVersionNumber,
            RestoreScope: this.state.restoreScope,
            NodeObjects: []
        }
        const option = {
            url: '/api/ArchiverRestore/SaveAdvancedRestoreSettingAndRun',
            method: "Post",
            data: restoreData
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            if(res.MessageType === MessageType.Successful) {
                const content = (
                    <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>
                );
                showToast.success(content);
            }else{
                showToast.error(res.ErrorMessage);
            }
        })
        .finally(() => {
            callback();
            $$.loading(false);
        });
    }

    onChangeType = (name, value) => {
        this.setState({
            [name]: value
        });
    }

    checkLocation = async () => {
        const locationPath = this.state.urlLocation;
        if (locationPath === "") {
            this.setState({
                noLocation: true
            });
            return false;
        } else {
            this.setState({
                noLocation: false
            });
        }
        $$.loading(true);
        let urlData = "/api/RecordsExplorerApi/CheckSPLocation4Job";
        let option = {
            url: urlData,
            method: "POST",
            data: {
                LocationPath: locationPath,
                SPAccount: null
            }
        };

        try {
            const res = await fetchUtility(option);
            const isValid = res != "";
            this.setState({
                isLocationValidate: true,
                LocationValidate: isValid ? RMResx.RM_JS_CP_ES_SuccessToValidateDBSettings : RMResx.RM_JS_Rule_SPDestUrlError,
                LocationValidateType: isValid ? "success" : "error"
            });
            return isValid;
        } catch (e) {
            return false;
        } finally {
            $$.loading(false);
        }
    }

    locationPathChange = (value) => {
        this.setState({
            urlLocation: value.trim(),
            dataSource: DataSourceType.M365,
            isLocationValidate: false,
            LocationValidate: "",
            LocationValidateType: ""
        });
    }

    cancelLocationValidate = () => {
        this.setState({
            isLocationValidate: false,
            LocationValidate: "",
            LocationValidateType: ""
        });
    }

    onDestActiveTabChange = (value) => {
        this.setState({
            destinationActiveTab: value
        });
    }

    onSelectLocationTree = (node) => {
        this.setState({
            locationTreeData: node.FullPath,
            dataSource: DataSourceType.M365,
            noSelectNode: !node
        });
    }

    onSelectLocationTeamsTree = (node) => {
        this.setState({
            locationTeamsTreeData: node.FullPath,
            dataSource: DataSourceType.Teams,
            noSelectNode: !node
        });
    }
    
    priorityValueChange = (value) => {
		this.setState({ priorityValue: value?.newValue?.value });
    }

    onVersionNumberChanged = (value) => {
        if(this.state.restoreVersion === RestoreDocumentVersionsOption.SpecifyVersions) {
            this.setState({ lastVersionNumber: value });
        } else {
            this.setState({ lastVersionNumber: 1 });
        }
    }

    renderRestoreType = () => {
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx.RM_AdvancedRestore_RestoreType_Title}</span>
                    <$g.Popover>{RMResx.RM_AdvancedRestore_RestoreType_Description}</$g.Popover>
                </div>
                <div role="radiogroup" aria-label={RMResx.RM_AdvancedRestore_RestoreType_Title}>
                    <div className="margin-bottom-s">
                        <R.Radio
                            name="restoreType"
                            text={RMResx.RM_AdvancedRestore_RestoreType_InPlace}
                            value={AdvanceRestoreType.Microsoft365InPlace}
                            checked={this.state.restoreType == AdvanceRestoreType.Microsoft365InPlace}
                            onChange={() => this.onChangeType("restoreType", AdvanceRestoreType.Microsoft365InPlace)}
                        />
                    </div>
                    <div>
                        <R.Radio
                            name="restoreType"
                            text={RMResx.RM_AdvancedRestore_RestoreType_OutPlace}
                            value={AdvanceRestoreType.OpusArchivedStubs}
                            checked={this.state.restoreType == AdvanceRestoreType.OpusArchivedStubs}
                            onChange={() => this.onChangeType("restoreType", AdvanceRestoreType.OpusArchivedStubs)}
                        />
                    </div>
                </div>
            </div>
        )
    };

    renderLocationType = () => {
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx.RM_AdvancedRestore_Location_Title}</span>
                </div>
                <div className="margin-bottom-s margin-top-s">
                    <R.Radio
                        name="locationType"
                        text={RMResx.RM_AdvancedRestore_EnterLocation}
                        value={AdvanceLocationType.Url}
                        checked={this.state.locationType == AdvanceLocationType.Url}
                        onChange={() => this.onChangeType("locationType", AdvanceLocationType.Url)}
                    />
                    {this.state.locationType == AdvanceLocationType.Url && (
                        <div className="margin-top-s margin-left-l">
                            <div className="flex">
                                <R.Input
                                    id="raLocationInput"
                                    className="location-path"
                                    type="text"
                                    aria-label={RMResx.RM_AdvancedRestore_EnterLocation}
                                    placeholder={RMResx.RM_AdvancedRestore_EnterLocation_Placeholder}
                                    value={this.state.urlLocation || ""}
                                    onChange={this.locationPathChange}
                                />
                                <R.Button
                                    className="margin-left-s"
                                    text={RMResx.RM_AdvancedRestore_EnterLocation_ValidationTest}
                                    onClick={this.checkLocation}
                                />
                            </div>
                            <$g.ValidationMsg show={this.state.noLocation}>
                                {RMResx.RM_JS_RDM_CreateRule_Validation_NoInputLocaltion}
                            </$g.ValidationMsg>
                            <div className="location-validate-msg">
                                <R.Messagebar
                                    message={this.state.LocationValidate}
                                    status={{ show: this.state.isLocationValidate }}
                                    classify={this.state.LocationValidateType}
                                    onClose={this.cancelLocationValidate} />
                            </div>
                        </div>
                    )}
                </div>
                <div className="margin-bottom-s">
                    <div className="location-title">
                        <R.Radio
                            name="locationType"
                            text={RMResx.RM_AdvancedRestore_SelectLocationFromTree}
                            checked={this.state.locationType == AdvanceLocationType.Tree}
                            onChange={() => this.onChangeType("locationType", AdvanceLocationType.Tree)}
                        />
                        {this.state.locationType == AdvanceLocationType.Tree && (
                            <div className="rc-tree">
                                <div className="rc-tree-container">
                                    <div style={{ padding: "12px 20px" }}>
                                        <R.Tabcontrol active={this.state.destinationActiveTab} onChange={this.onDestActiveTabChange}>
                                            <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_SharePoint_Tab}>
                                                <div className="destination-tab">
                                                    
                                                    <SPDestinationTree
                                                        // treeData={this.state.locationTreeData}
                                                        onSelectedNodeChanged={this.onSelectLocationTree}
                                                        restoreTree={true}
                                                    />
                                                </div>
                                            </R.TabPanel>
                                            <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_Teams_Tab}>
                                                <div className="destination-tab">
                                                    <TeamsDestinationTree
                                                        // treeData={this.state.locationTeamsTreeData}   
                                                        onSelectedNodeChanged={this.onSelectLocationTeamsTree}
                                                        restoreTree={true}
                                                    />
                                                </div>
                                            </R.TabPanel>
                                        </R.Tabcontrol>
                                    </div>
                                </div>
                                <$g.ValidationMsg show={this.state.noSelectNode}>
                                    {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                                </$g.ValidationMsg>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        )
    };

    renderRestoreScope = () => {
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx.RM_AdvancedRestore_RestoreScope_Title}</span>
                    <$g.Popover>{RMResx.RM_AdvancedRestore_RestoreScope_Description}</$g.Popover>
                </div>
                <div role="radiogroup" aria-label={RMResx.RM_AdvancedRestore_RestoreScope_Title}>
                    <div className="margin-bottom-s">
                        <R.Radio
                            name="restoreScope"
                            text={RMResx.RM_AdvancedRestore_RestoreScope_IncludeChildren}
                            value={AdvanceRestoreScope.IncludeChildren}
                            checked={this.state.restoreScope == AdvanceRestoreScope.IncludeChildren}
                            onChange={() => this.onChangeType("restoreScope", AdvanceRestoreScope.IncludeChildren)}
                        />
                    </div>
                    <div>
                        <R.Radio
                            name="restoreScope"
                            text={RMResx.RM_AdvancedRestore_RestoreScope_SelectedLocationOnly}
                            value={AdvanceRestoreScope.SelectedLocationOnly}
                            checked={this.state.restoreScope == AdvanceRestoreScope.SelectedLocationOnly}
                            onChange={() => this.onChangeType("restoreScope", AdvanceRestoreScope.SelectedLocationOnly)}
                        />
                    </div>
                </div>
            </div>
        )
    };

    renderRestoreVersion = () => {
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx.RM_RestoreCenter_RestoreVersionTitle}</span>
                    <$g.Popover>{RMResx.RM_RestoreCenter_RestoreDocumentVersionDescription}</$g.Popover>
                </div>
                <div role="radiogroup" aria-label={RMResx.RM_RestoreCenter_RestoreVersionTitle}>
                    <R.Radio
                        name="radioVersion"
                        text={RMResx.RM_RestoreCenter_RestoreKeepSpecialVersion}
                        value={RestoreDocumentVersionsOption.SpecifyVersions}
                        checked={this.state.restoreVersion == RestoreDocumentVersionsOption.SpecifyVersions}
                        onChange={() => this.onChangeType("restoreVersion", RestoreDocumentVersionsOption.SpecifyVersions)}
                    />
                    {this.state.restoreVersion == RestoreDocumentVersionsOption.SpecifyVersions && <div className="margin-top-s margin-left-xl">
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
                            checked={this.state.restoreVersion == RestoreDocumentVersionsOption.AllVersions}
                            onChange={() => this.onChangeType("restoreVersion", RestoreDocumentVersionsOption.AllVersions)}
                        />
                    </div>
                </div>
            </div>
        )
    };

    renderConflictResolution = () => {
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx["StorageOptimization.Gui_Conflict Resolution"]}</span>
                    <$g.Popover>{RMResx.RM_AR_RC_Panel_ConflictResolutionDes}</$g.Popover>
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
                    onChange={(value) => this.onChangeType("conflictType", value.newValue.value)}
                    aria={{ ariaLabel: RMResx["StorageOptimization.Gui_Conflict Resolution"] }}
                />
            </div>
        )
    };

    renderMoreOptions = () => {
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx.RM_AR_RC_Panel_Options}</span>
                </div>
                <div>
                    <R.Checkbox
                        id="raWorkflowChk"
                        text={RMResx["StorageOptimization.Gui_Include workflow definition"]}
                        checked={this.state.isWorkflow}
                        onChange={() => this.onChangeType("isWorkflow", !this.state.isWorkflow)}
                    />
                    <$g.Popover>{RMResx["StorageOptimization.Gui_D5FD180A-A9BC-415A-9C28-94F19EA447E5"]}</$g.Popover>
                </div>
                <div>
                    <R.Checkbox
                        id="raShareLinkChk"
                        text={RMResx["StorageOptimization.Gui_9DC59F76-D900-4F54-8ECD-5385AD1C7B8A"]}
                        checked={this.state.isShareLink}
                        onChange={() => this.onChangeType("isShareLink", !this.state.isShareLink)}
                    />
                    <$g.Popover>{RMResx["StorageOptimization.Gui_6A8A562B-6C2E-4DF7-8102-0B52DF23A94B"]}</$g.Popover>
                </div>
                <div>
                    <R.Checkbox
                        id="raSupportLockedSiteChk"
                        text={RMResx.RM_AR_RC_Panel_RestoreToLocked}
                        checked={this.state.isSupportLockedSite}
                        onChange={() => this.onChangeType("isSupportLockedSite", !this.state.isSupportLockedSite)}
                    />
                    <$g.Popover>{RMResx.RM_AR_RC_Panel_RestoreToLockedDes}</$g.Popover>
                </div>
            </div>
        )
    };

    renderOpusStubSection = () => {
        return (
            <>
                {this.renderRestoreVersion()}
                {this.renderConflictResolution()}
                {this.renderMoreOptions()}
            </>
        )
    };

    renderPriorityContent = () => {
        return (
            <div className="rc-panel-content">
                <div className="rc-panel-title">
                    <span>{RMResx.RM_JS_JM_Priority}</span>
                    <$g.Popover>{RMResx.RM_JS_JM_Priority_Tooltip}</$g.Popover>
                </div>
                <R.Combobox
                    id="advanceRestorePriority"
                    textField='name'
                    valueField='value'
                    checkedField='checked'
                    items={Priority}
                    width={"100%"}
                    searchable={false}
                    onChange={this.priorityValueChange}
                    triggerBySource={true}
                    aria="tooltip_demo_labelledby"
                />
            </div>
        )
    };

    render() {
        return (
            <div id={this.props.id}>
                {this.renderRestoreType()}
                {this.renderLocationType()}
                {this.renderRestoreScope()}
                {this.state.restoreType == AdvanceRestoreType.OpusArchivedStubs && this.renderOpusStubSection()}
                {this.renderPriorityContent()}
            </div>
        );
    }
}