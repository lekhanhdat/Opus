import "../../Less/CP/endUserRestoreSettings.less";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import { GroupTeamSitePermission, MessageType, SiteCollectionPermission, SiteCollectionPermissionType } from "./CPConstants";
import RouterUrls from "../../Constants/RouterUrls";
import { showToast } from "../../Utilities/CommonUtil";

export default class EndUserRestoreSetting extends R.Component {
    constructor(props) {
        super(props);
        this.permissionDefaultObj = {
            IsSearchGroupTeamSite: true,
            IsSearchSiteCollection: true,
            IsRestoreGroupTeamSite: true,
            IsRestoreSiteCollection: true,
            IsRestoreStubLink: true,
            IsExportGroupTeamSite: true,
            IsExportSiteCollection: true,
            IsExportStubLink: true,
            TeamsAndGroup: 0,
            SiteCollection: 0,
            SiteCollectionSpecialGroupNames: "",
        };
        this.state = {
            isEnableRestoreData: false,
            recenterColumns: this.getColumns(true),
            stubColumns: this.getColumns(),
            recenterDataList: [],
            stubDataList: [],
            permissionData: {},
            isEnableArchiveTier: false,
            isIncludeSharedLinks: false,
            isDisableStubOutOfPlace: false,
            isEnableStubOutOfPlace: true,
            isEnableSearchStub: false,
            isEnableManualInputDestinationStub: true,
            isEnableRestorePage: false,
            uploadImgStr: "",
            stubMessage: "",
            stubFooter: "",
        };
    }

    componentInit() {
        this.loadEndUserRestoreSetting();
    }

    getColumns(isRecenter) {
        let column = [
            {
                header: RMResx["StorageOptimization.Gui_9002510f-8cdb-49d1-9919-41e59c35de2a"],
                resizeable: true,
                width: 300,
            }, {
                header: RMResx["StorageOptimization.Gui_9bdcdf6f-7e76-4354-a224-fee226921a6c"],
                resizeable: true,
                width: 150,
            }, {
                headerTemplate: RMResx["StorageOptimization.Gui_a2935e6c-c285-4bb3-bd99-2ed19e980a75"],
                resizeable: true,
                width: 150,
            }, {
                header: RMResx["StorageOptimization.Gui_6f7288c0-fc0a-4dd4-ad8f-d2e1c7320be0"],
                resizeable: true,
                // width: 110,
            }
        ];

        if (isRecenter) {
            const newColumn = [...column];
            newColumn.splice(1, 0, {
                header: RMResx.RM_AR_CP_EURS_Search,
                resizeable: true,
                width: 150,
            });
            
            return newColumn;
        }

        return column;
    }

    loadEndUserRestoreSetting() {
        $$.loading(true);
        let option = {
            url: "/api/EndUserRestoreSetting/GetEndUserRestoreSetting",
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setState({
                isEnableRestoreData: res.IsAllowRestore,
                recenterDataList: [res.PermissionSetting, res.PermissionSetting],
                stubDataList: [res.PermissionSetting],
                permissionData: res.PermissionSetting,
                isEnableArchiveTier: res.IsRestoreArchivedTier,
                isIncludeSharedLinks: res.IsIncludeSharedLinks,
                isDisableStubOutOfPlace: res.PermissionSetting.IsRestoreStubLink ? false : true,
                isEnableStubOutOfPlace: res.PermissionSetting.StubOopRestoreSetting.IsEnableStubOopRestore,
                isEnableSearchStub: res.PermissionSetting.StubOopRestoreSetting.IsEnableSearchStubLocation,
                isEnableManualInputDestinationStub: res.PermissionSetting.StubOopRestoreSetting.IsEnableManualInputDesStubLocation,
                isEnableRestorePage: res.IsCustomizeStubRestorePage,
                uploadImgStr: res.Logo,
                stubMessage: res.Message,
                stubFooter: res.Footer,
            });

        }).catch((e) => {
            $$.loading(false);
        });
    }

    onRestoreDataSwitchChange = (args) => {
        if (args) {
            let copyPermissionData = RM.deepcopy(this.permissionDefaultObj);
            this.setState({
                permissionData: copyPermissionData,
                recenterDataList: [copyPermissionData, copyPermissionData],
                stubDataList: [copyPermissionData],
                isDisableStubOutOfPlace: false,
                isEnableStubOutOfPlace: true,
                isEnableSearchStub: true,
                isEnableManualInputDestinationStub: true,
            });
        }
        this.setState({ isEnableRestoreData: args });
    }

    onRowEvent = (args, data) => {
        let rowData = args.rowData;
        if (data) rowData.SiteCollectionSpecialGroupNames = data;
        if (args.type == 'setRowData') {
            if (!rowData.IsRestoreStubLink) {
                this.setState({
                    isEnableStubOutOfPlace: false,
                    isDisableStubOutOfPlace: true,
                });
            } else {
                this.setState({
                    isDisableStubOutOfPlace: false,
                });
            }
            this.setState({ permissionData: rowData });
        }
    }

    onArchiveTierChanged = (args) => {
        this.setState({ isEnableArchiveTier: args });
    }

    onSharedLinkChanged = (args) => {
        this.setState({ isIncludeSharedLinks: args });
    }

    onStubOutOfPlaceChanged = (args) => {
        this.setState({ isEnableStubOutOfPlace: args });
    }

    onSearchStubChanged = (args) => {
        this.setState({ isEnableSearchStub: args });
    }

    onManualInputDestChanged = (args) => {
        this.setState({ isEnableManualInputDestinationStub: args });
    }

    onRestorePageSwitchChange = (args) => {
        if (!args) {
            this.setState({
                uploadImgStr: "",
                stubMessage: RMResx["StorageOptimization.Gui_357cafb4-ed90-4141-b4e3-bd67a82624f6"],
                stubFooter: "",
            });
        }
        this.setState({ isEnableRestorePage: args });
    }

    onUploadClick = (args) => {
        this.chooseFileInput.value = "";
        this.chooseFileInput.click();
    }

    onResetClick = (args) => {
        this.setState({ uploadImgStr: "" });
    }

    onChooseFileChange = (e) => {
        let filePath = this.chooseFileInput.value;
        let fileInfo = this.chooseFileInput.files[0];
        if (!filePath) {
            return;
        }
        if (!filePath.endsWith(".jpg") && !filePath.endsWith(".png") && !filePath.endsWith(".bmp")) {
            showToast.error(RMResx.RM_AR_CP_EURS_ChangeLogoError);
            return;
        }
        if (fileInfo.size > 5 * 1024 * 1024) {
            showToast.error(RMResx.RM_AR_CP_EURS_ImgSizeError);
            return;
        }

        let reader = new FileReader();
        reader.readAsDataURL(fileInfo);
        let that = this;
        reader.onload = function (e) {
            let uploadImgBase64 = e.target.result;
            that.setState({ uploadImgStr: uploadImgBase64 });
        };
    }

    onStubMessageChange = (value) => {
        this.setState({ stubMessage: value });
    }

    onStubFooterChange = (value) => {
        this.setState({ stubFooter: value });
    }

    onCancelRestoreSetting = () => {
        this.props.history.push({
            pathname: RouterUrls.CP_Index
        });
    }

    onSaveRestoreSetting = () => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        let restoreSettingObj = {
            IsAllowRestore: this.state.isEnableRestoreData,
            IsRestoreArchivedTier: this.state.isEnableArchiveTier,
            IsIncludeSharedLinks: this.state.isIncludeSharedLinks,
            IsCustomizeStubRestorePage: false,    //this.state.isEnableRestorePage,
            Logo: this.state.uploadImgStr,
            Message: this.state.stubMessage,
            Footer: this.state.stubFooter,
            PermissionSetting: {
                ...this.state.permissionData,
                StubOopRestoreSetting: {
                    IsEnableManualInputDesStubLocation: this.state.isEnableManualInputDestinationStub,
                    IsEnableSearchStubLocation: this.state.isEnableSearchStub,
                    IsEnableStubOopRestore: this.state.isEnableStubOutOfPlace,
                }
            },
        };

        let permission = restoreSettingObj.PermissionSetting;
        if (permission && permission.SiteCollection == SiteCollectionPermissionType.SiteOwnerOrSpecialGroup && (permission.SiteCollectionSpecialGroupNames == "" || permission.SiteCollectionSpecialGroupNames == null)) {
            showToast.error(RMResx["StorageOptimization.Gui_90ae737b-9b79-486a-81ac-c1c13b84261b"]);
            return;
        }

        $$.loading(true);
        let option = {
            url: '/api/EndUserRestoreSetting/SaveEndUserRestoreSetting',
            method: "Post",
            data: restoreSettingObj
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == MessageType.Successful) {
                this.loadEndUserRestoreSetting();
                showToast.success(RMResx.RM_AR_CP_EURS_SaveSuccessful);
            } else {
                showToast.error(result.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getOutOfPlaceStubs = () => {
        return [
            {
                text: RMResx.RM_AR_CP_EURS_SearchStubCheckbox,
                value: RMResx.RM_AR_CP_EURS_SearchStubCheckbox,
                checked: this.state.isEnableSearchStub
            },
            {
                text: RMResx.RM_AR_CP_EURS_ManualInputDestinationStubCheckbox,
                value: RMResx.RM_AR_CP_EURS_ManualInputDestinationStubCheckbox,
                checked: this.state.isEnableManualInputDestinationStub
            },
        ]
    }

    onOutOfStubGroupChange = (args) => {
        this.setState({
            isEnableSearchStub: args.includes(RMResx.RM_AR_CP_EURS_SearchStubCheckbox),
            isEnableManualInputDestinationStub: args.includes(RMResx.RM_AR_CP_EURS_ManualInputDestinationStubCheckbox),
        })
    }

    renderRestoreSettings() {
        return <div className="ra-section">
            <div className="ra-section-head">
                <span tabIndex="0">{RMResx.RM_AR_CP_EURS_RestoreTitle}</span>
            </div>
            <div className="ra-section-switch">
                <R.Switch
                    id="raCpRestoreSwitch"
                    checked={this.state.isEnableRestoreData}
                    onChange={this.onRestoreDataSwitchChange}
                    aria="#ariaRestore"
                />
                <span id="ariaRestore" className="ra-switch-text">{RMResx.RM_AR_CP_EURS_RestoreSwitchTitle}</span>
                <$g.Popover>{RMResx["StorageOptimization.Gui_22bfe864-83b4-421b-8e83-d134f2f3bc70"]}</$g.Popover>
            </div>
            {this.state.isEnableRestoreData && (
                <>
                    <div className="ra-section-switch">
                        <div className="ra-section-switch">
                            <R.Table
                                id="raRecenterTable"
                                columns={this.state.recenterColumns}
                                rowTemplate={RecenterTemplate}
                                items={this.state.recenterDataList}
                                onRowEvent={this.onRowEvent}
                                />
                        </div>
                        <R.Table
                            id="raStubTable"
                            columns={this.state.stubColumns}
                            rowTemplate={StubTemplate}
                            items={this.state.stubDataList}
                            onRowEvent={this.onRowEvent}
                            />
                    </div>
                    <div className="flex flex-column gap-s">
                        <div>
                            <R.Checkbox
                                id="raSharedLinkChk"
                                text={RMResx["StorageOptimization.Gui_9DC59F76-D900-4F54-8ECD-5385AD1C7B8A"]}
                                title={RMResx["StorageOptimization.Gui_9DC59F76-D900-4F54-8ECD-5385AD1C7B8A"]}
                                checked={this.state.isIncludeSharedLinks}
                                onChange={this.onSharedLinkChanged}
                            />
                            <$g.Popover>{RMResx["StorageOptimization.Gui_6A8A562B-6C2E-4DF7-8102-0B52DF23A94B"]}</$g.Popover>
                        </div>
                        <div style={{ marginTop: -8 }}>
                            <R.Checkbox
                                id="raOutOfPlaceRestore"
                                text={RMResx.RM_AR_CP_EURS_OOPRestoreCheckbox}
                                title={RMResx.RM_AR_CP_EURS_OOPRestoreCheckbox}
                                disabled={this.state.isDisableStubOutOfPlace}
                                checked={this.state.isEnableStubOutOfPlace}
                                onChange={this.onStubOutOfPlaceChanged}
                            />
                        </div>
                        {this.state.isEnableStubOutOfPlace && (
                            <div className="margin-left-l">
                                <R.Validation element="Checkbox.Group" require={RMResx.RM_AR_CP_EURS_OOPRestoreCheckboxes_Valid_Msg} group="stubGroupValidation">
                                    <div className="flex flex-column gap-s">
                                        <R.Checkbox.Group
                                            block
                                            name="oopStubGroup"
                                            items={this.getOutOfPlaceStubs()}
                                            onChange={this.onOutOfStubGroupChange}
                                        />
                                    </div>
                                </R.Validation>
                            </div>
                        )}
                    </div>
                </>
            )}

            {/* temporarily hidden archive tier option */}
            {/* <div>
                <R.Checkbox
                    id="raArchiveTierChk"
                    text={RMResx["StorageOptimization.Gui_1F0922B3-9C88-4BF3-AE5D-1672A627AE91"]}
                    title={RMResx["StorageOptimization.Gui_1F0922B3-9C88-4BF3-AE5D-1672A627AE91"]}
                    checked={this.state.isEnableArchiveTier}
                    onChange={this.onArchiveTierChanged}
                />
                <$g.Popover>{RMResx.RM_AR_CP_EURS_ArchiveTierPop}</$g.Popover>
            </div> */}
        </div>;
    }

    renderStubSettings() {
        return <div className="ra-section">
            <div className="ra-section-head">
                <span tabIndex="0">{RMResx.RM_AR_CP_EURS_StubTitle}</span>
            </div>
            <div className="ra-section-switch">
                <R.Switch
                    id="raCpStubSwitch"
                    checked={this.state.isEnableRestorePage}
                    onChange={this.onRestorePageSwitchChange}
                    aria="#ariaStub"
                />
                <span id="ariaStub" className="ra-switch-text">{RMResx["StorageOptimization.Gui_4592b56a-7344-4fe1-8127-e2758e486d41"]}</span>
                <$g.Popover>{RMResx["StorageOptimization.Gui_8ad0439d-bdb3-43a2-a3ec-df42df890401"]}</$g.Popover>
            </div>
            {this.state.isEnableRestorePage && <div>
                <div className="ra-section-switch">
                    <div>
                        <span className="ra-stub-title">{RMResx.RM_AR_CP_EURS_ChangeLogo}</span>
                        <$g.Popover>{RMResx["StorageOptimization.Gui_f31b3fb3-c644-4f81-ac39-14d45a471306"]}</$g.Popover>
                    </div>
                    <div className="ra-logo">
                        <div className="ra-logo-background">
                            {!this.state.uploadImgStr && <div className="ra-logo-pic">
                                <img id="rclogo" src="/Images/Base/logo_rc.png" style={{ width: "200px", marginLeft: "9px" }} />
                            </div>}
                            {this.state.uploadImgStr && <div
                                className="ra-logo-pic"
                                style={{ background: `url("${this.state.uploadImgStr}")` }}
                            >
                            </div>}
                        </div>
                        <div className="ra-logo-uploadBtn">
                            <R.Button
                                id="raChangeLogoUploadBtn"
                                icon="fia-upload"
                                text={RMResx.RM_AR_CP_EURS_UploadBtn}
                                onClick={this.onUploadClick}
                            />
                        </div>
                        <div className="ra-logo-resetBtn">
                            <R.Button
                                id="raChangeLogoResetBtn"
                                text={RMResx.RM_AR_CP_EURS_ResetBtn}
                                onClick={this.onResetClick}
                            />
                        </div>
                    </div>
                </div>
                <div className="ra-section-switch">
                    <div className="ra-stub-title-bottom">
                        <span className="ra-stub-title">{RMResx.RM_AR_CP_EURS_StubMsg}</span>
                    </div>
                    <R.Input
                        id="raStubMessageIpt"
                        type="textarea"
                        className="resizable"
                        value={this.state.stubMessage}
                        onChange={this.onStubMessageChange}
                        aria={{ ariaLabel: RMResx.RM_AR_CP_EURS_StubMsg }}
                    />
                </div>
                <div className="ra-section-switch">
                    <div className="ra-stub-title-bottom">
                        <span className="ra-stub-title">{RMResx.RM_AR_CP_EURS_StubFooter}</span>
                    </div>
                    <R.Input
                        id="raStubFooterIpt"
                        type="textarea"
                        className="resizable"
                        value={this.state.stubFooter}
                        onChange={this.onStubFooterChange}
                        aria={{ ariaLabel: RMResx.RM_AR_CP_EURS_StubFooter }}
                    />
                </div>
            </div>}
        </div>;
    }

    render() {
        return <div id="raEndUserRestoreSettings">
            <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_EndUserRestore]} />
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-page-main">
                        {this.renderRestoreSettings()}
                        {/* {this.renderStubSettings()} */}
                    </div>
                    <div className="ra-foot-btns flex justify-end align-center gap-s">
                        <R.Button
                            text={RMResx.RM_JS_Common_Cancel}
                            onClick={this.onCancelRestoreSetting}
                        />
                        <R.Button
                            id="raCpRestoreSaveBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_JS_Common_Save}
                            onClick={this.onSaveRestoreSetting}
                        />
                    </div>
                </div>
            </R.Validation>
            <input type="file" id="choosefile" name="fileUp"
                style={{ display: "none" }} ref={r => this.chooseFileInput = r}
                onChange={this.onChooseFileChange} />
        </div>;
    }
}

export class RecenterTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {
            isSearchGroupTeamSite: this.props.rowData.IsSearchGroupTeamSite,
            isSearchSiteCollection: this.props.rowData.IsSearchSiteCollection,
            isRestoreGroupTeamSite: this.props.rowData.IsRestoreGroupTeamSite,
            isRestoreSiteCollection: this.props.rowData.IsRestoreSiteCollection,
            isExportGroupTeamSite: this.props.rowData.IsExportGroupTeamSite,
            isExportSiteCollection: this.props.rowData.IsExportSiteCollection,
            groupTeamSiteList: this.getGroupTeamSiteList(),
            siteCollectionList: this.getSiteCollectionList(),
            showSpecialGroupNameIpt: this.isShowSpecialGroupNameIpt(),
            specialGroupNameList: this.props.rowData?.SiteCollectionSpecialGroupNames?.split(";") || [],
            isShowErrorGroupNameList: false,
        };
    }

    getGroupTeamSiteList() {
        let rowData = this.props.rowData;
        let currentGroupTeamSiteList = [];
        RM.deepcopy(GroupTeamSitePermission).forEach(item => {
            item.checked = item.value == rowData.TeamsAndGroup;
            currentGroupTeamSiteList.push(item);
        });
        return currentGroupTeamSiteList;
    }

    getSiteCollectionList() {
        let rowData = this.props.rowData;
        let currentSiteCollectionList = [];
        RM.deepcopy(SiteCollectionPermission).forEach(item => {
            item.checked = item.value == rowData.SiteCollection;
            currentSiteCollectionList.push(item);
        });
        return currentSiteCollectionList;
    }

    isShowSpecialGroupNameIpt() {
        let rowData = this.props.rowData;
        return rowData.SiteCollection == SiteCollectionPermissionType.SiteOwnerOrSpecialGroup;
    }

    updateData = (stateKey, propKey, value) => {
        this.setState({ [stateKey]: value });
        this.props.rowData[propKey] = value;
    };

    onSearchChanged = (args1, args2) => {
        switch (args1) {
            case 'groupTeamSite':
                if (!args2) {
                    this.updateData("isRestoreGroupTeamSite", "IsRestoreGroupTeamSite", args2);
                    this.updateData("isExportGroupTeamSite", "IsExportGroupTeamSite", args2);
                }
                this.updateData("isSearchGroupTeamSite", "IsSearchGroupTeamSite", args2);
                break;
            case 'siteCollection':
                if (!args2) {
                    this.updateData("isRestoreSiteCollection", "IsRestoreSiteCollection", args2);
                    this.updateData("isExportSiteCollection", "IsExportSiteCollection", args2);
                }
                this.updateData("isSearchSiteCollection", "IsSearchSiteCollection", args2);
                break;
            default:
                break;
        }
        this.dispatch('setRowData');
    }

    onRestoreChanged = (args1, args2) => {
        switch (args1) {
            case 'groupTeamSite':
                if (args2) {
                    this.updateData("isSearchGroupTeamSite", "IsSearchGroupTeamSite", args2);
                }
                this.updateData("isRestoreGroupTeamSite", "IsRestoreGroupTeamSite", args2);
                break;
            case 'siteCollection':
                if (args2) {
                    this.updateData("isSearchSiteCollection", "IsSearchSiteCollection", args2);
                }
                this.updateData("isRestoreSiteCollection", "IsRestoreSiteCollection", args2);
                break;
            default:
                break;
        }
        this.dispatch('setRowData');
    }

    onExportChanged = (args1, args2) => {
        switch (args1) {
            case 'groupTeamSite':
                if (args2) {
                    this.updateData("isSearchGroupTeamSite", "IsSearchGroupTeamSite", args2);
                }
                this.updateData("isExportGroupTeamSite", "IsExportGroupTeamSite", args2);
                break;
            case 'siteCollection':
                if (args2) {
                    this.updateData("isSearchSiteCollection", "IsSearchSiteCollection", args2);
                }
                this.updateData("isExportSiteCollection", "IsExportSiteCollection", args2);
                break;
            default:
                break;
        }
        this.dispatch('setRowData');
    }

    onGroupTeamSitePermissionChanged = (args) => {
        this.props.rowData.TeamsAndGroup = args.newValue.value;
        this.dispatch('setRowData');
    }

    onSiteCollectionPermissionChanged = (args) => {
        const isRestoreSiteCollection = this.props.rowData.IsRestoreSiteCollection;
        const isExportSiteCollection = this.props.rowData.IsExportSiteCollection;
        const isSearchSiteCollection = this.props.rowData.IsSearchSiteCollection;

        if ((isSearchSiteCollection || isRestoreSiteCollection || isExportSiteCollection) && args.newValue.value == SiteCollectionPermissionType.SiteOwnerOrSiteMemberOrSiteVisitor) {
            this.siteVisitorMessageBox();
        }
        this.setState({ showSpecialGroupNameIpt: args.newValue.value == SiteCollectionPermissionType.SiteOwnerOrSpecialGroup });
        this.props.rowData.SiteCollection = args.newValue.value;
        this.dispatch('setRowData');
    }
    
    getListInItems(values) {
        let items = [];
        for (let value of values) {
            if(value){
                let item = {};
                item.name = value;
                item.checked = true;
                item.invalid = false;
                item.tooltip = value;
                items.push(item);
            }
        }
        return items;
    }

    doMatchArray = (args) => {
        return this.getListInItems(args.list);
    }

    onSpecialGroupChanged = (args) => {
        const listIn = args.newValue.map(item => item.name);
        if (listIn.length > 5) {
            this.setState({ isShowErrorGroupNameList: true });
            listIn.splice(5);
        } else if (listIn.length < 5) {
            this.setState({ isShowErrorGroupNameList: false });
        }
        this.setState({ specialGroupNameList: listIn }, 
            () => this.dispatch('setRowData', listIn.join(";"))
        );
    }

    siteVisitorMessageBox() {
        const args = {
            classify: "warn",
            width: "550px",
            title: RMResx.RM_CP_AM_Permission_WarningDialogTitle,
            content: RMResx.RM_AR_CP_EURS_Permission_SiteVisitor_Warning,
            buttons: [
                {
                    classify: "theme",
                    primary: true,
                    text: RMResx.RM_JS_Common_OK,
                    onClick: () => $$.messagedialog(false)
                },
            ]
        }
        $$.messagedialog(true, args);
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let rowIndex = this.props.index;
        let rowContent = null;
        if (rowIndex == 0) {
            rowContent = <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip tabIndex='0' aria-label={RMResx.RM_AR_CP_EURS_Service_TeamsOrGroups}>{RMResx.RM_AR_CP_EURS_Service_TeamsOrGroups}</div>
                </Cell>
                <Cell>
                    <R.Checkbox checked={this.state.isSearchGroupTeamSite} onChange={this.onSearchChanged.bind(this, "groupTeamSite")} />
                </Cell>
                <Cell>
                    <R.Checkbox checked={this.state.isRestoreGroupTeamSite} onChange={this.onRestoreChanged.bind(this, "groupTeamSite")} />
                </Cell>
                <Cell>
                    <R.Checkbox checked={this.state.isExportGroupTeamSite} onChange={this.onExportChanged.bind(this, "groupTeamSite")} />
                </Cell>
                <Cell>
                    <R.Combobox
                        id="raGroupTeamSiteCom"
                        tooltipField="name"
                        textField="name"
                        valueField="value"
                        checkedField="checked"
                        linkMode={false}
                        searchable={false}
                        items={this.state.groupTeamSiteList}
                        onChange={this.onGroupTeamSitePermissionChanged}
                    />
                </Cell>
            </Row>;
        }
        if (rowIndex == 1) {
            rowContent = <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip tabIndex='0' aria-label={RMResx.RM_AR_CP_EURS_Service_SPOnline}>{RMResx.RM_AR_CP_EURS_Service_SPOnline}</div>
                </Cell>
                <Cell>
                <R.Checkbox checked={this.state.isSearchSiteCollection} onChange={this.onSearchChanged.bind(this, "siteCollection")} />
                </Cell>
                <Cell>
                    <R.Checkbox checked={this.state.isRestoreSiteCollection} onChange={this.onRestoreChanged.bind(this, "siteCollection")} />
                </Cell>
                <Cell>
                    <R.Checkbox checked={this.state.isExportSiteCollection} onChange={this.onExportChanged.bind(this, "siteCollection")} />
                </Cell>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <R.Combobox
                            id="raSiteCollectionCom"
                            tooltipField="name"
                            textField="name"
                            valueField="value"
                            checkedField="checked"
                            linkMode={false}
                            searchable={false}
                            items={this.state.siteCollectionList}
                            onChange={this.onSiteCollectionPermissionChanged}
                        />
                        {this.state.showSpecialGroupNameIpt && <div className="ra-permission-input flex flex-column gap-s">
                            <R.RichCombobox
                                id="raSpecialGroupIpt"
                                searchPlaceholder={RMResx.RM_AR_CP_EURS_Permission_GroupName_Placeholder}
                                textField="name"
                                silence={true}
                                doMatch={this.doMatchArray}
                                items={this.state.specialGroupNameList.length > 0 ? this.getListInItems(this.state.specialGroupNameList) : []}
                                onChange={this.onSpecialGroupChanged}
                            />
                            <$g.ValidationMsg show={this.state.isShowErrorGroupNameList}>
                                {RMResx.RM_AR_CP_EURS_Permission_GroupName_MaxLimit}
                            </$g.ValidationMsg>
                        </div>}
                    </div>
                </Cell>
            </Row>;
        }
        return rowContent;
    }
}

export class StubTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onRestoreChanged = (args) => {
        this.props.rowData.IsRestoreStubLink = args;
        this.dispatch('setRowData');
    }

    onExportChanged = (args) => {
        this.props.rowData.IsExportStubLink = args;
        this.dispatch('setRowData');
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return <Row>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={RMResx.RM_AR_CP_EURS_Service_Stub} tabIndex="0">{RMResx.RM_AR_CP_EURS_Service_Stub}</div>
            </Cell>
            <Cell>
                <R.Checkbox checked={rowData.IsRestoreStubLink} onChange={this.onRestoreChanged.bind(this)} />
            </Cell>
            <Cell>
                <R.Checkbox checked={rowData.IsExportStubLink} onChange={this.onExportChanged.bind(this)} />
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={RMResx.RM_AR_CP_EURS_Service_StubViewer} tabIndex="0">{RMResx.RM_AR_CP_EURS_Service_StubViewer}</div>
            </Cell>
        </Row>;
    }
}