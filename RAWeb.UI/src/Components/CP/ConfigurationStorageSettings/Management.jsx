import { StorageTypeIndex } from "../../../Constants/Constants";
import Enviroments from "../../../Constants/Enviroments";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { getUserGuildTagPage, LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import { storageKeys } from "../../../Utilities/Constant";
import { checkPermission } from "../../../Utilities/permissionManager";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import { MessageType } from "../CPConstants";
import StoragePanel from "./StoragePanel";
import StorageTable from "./StorageTable";

export default class Management extends R.Component {
    constructor(props) {
        super(props);
        this.enableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
        this.isShowExportBtn = checkPermission("Archiver_Export_Index", RM.UserResources);
        this.defaultShowActions = {
            showCreateBtn: this.enableRecordsArchiver,
            showIndexBtn: false,
            showDeleteBtn: false,
        };
        this.defaultDeviceId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

        this.state = {
            storageChecked: [],
            storageCount: 0,
            storagePagerIndex: 0,
            storagePagerSize: 10,
            storagePanelTitle: '',
            showStorageSettingsPanel: { show: false },
            cellStorageId: null,
            showActions: this.defaultShowActions,
            allColumns: this.getColumns(),
            items: [],
            indexDeviceId: "",
            showTip: true,
            isShowExportIndexKeyDialog: false,
            isShowExportIndexKey: false,
            isShowCopyBtn: false,
            exportIndexKeyValue: "",
        };
        this.filterData = this.getDefaultPager();
    }

    componentInit() {
        this.initStorageSettingData(true);
    }

    initStorageSettingData = (isResetPagerIndex) => {
        $$.loading(true);
        if (isResetPagerIndex) {
            this.filterData.PageIndex = 1;
            this.setState({ storagePagerIndex: 0 });
        }
        let urlData = "/api/StorageDevice/GetAllActiveStorage";
        let option = {
            url: urlData,
            method: "POST",
            data: this.filterData,
        };
        fetchUtility(option).then((res) => {
            let data = res;
            this.setState({
                items: data.StorageDeviceUIDtosList,
                storageCount: data.TotalNumber,
                indexDeviceId: data.IndexDeviceId,
            });

            this.dispatch("raStorageTable", { columns: this.state.allColumns, items: data.StorageDeviceUIDtosList, isReset: isResetPagerIndex, indexDeviceId: data.IndexDeviceId });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getColumns() {
        let modifiedTime = [];
        if (this.enableRecordsArchiver) {
            modifiedTime.push({
                header: RMResx.RM_AR_CP_Common_ColName_ModifiedTime,
                width: 350,
                resizeable: true,
                valuePath: "ModifiedTime",
            });
        }
        return [
            {
                header: RMResx.RM_AR_CP_GSS_ColName_StorageName,
                width: 350,
                resizeable: true,
                valuePath: "StorageName",
            },
            ...modifiedTime,
            {
                header: RMResx.RM_AR_CP_Common_ColName_ArchivedTime,
                resizeable: true,
                width: 300,
                valuePath: "ArchivedTime",
            }
        ];
    }

    getDefaultPager() {
        let param = {
            PageIndex: 1,
            PageSize: 10,
            TotalNumber: 0,
            SearchValue: "",
        };
        return param;
    }

    getShowActions() {
        let { showCreateBtn, showIndexBtn, showDeleteBtn } = this.state.showActions;
        let buttonsInfo = [
            { isStatic: true, name: RMResx.RM_JS_Common_Create, onClick: this.onCreateStorage, isShow: showCreateBtn },
            { name: RMResx.RM_CP_StorageSetting_Export_Index_Title, icon: "fia-export-settings", onClick: this.exportIndexMessageBox, isShow: this.isShowExportBtn },
            { name: RMResx.RM_AR_CP_GSS_SetIndexBtn, icon: "fia-set-as-index-storage", onClick: this.onSetIndexStorage, isShow: showIndexBtn },
            { name: RMResx.RM_JS_Common_Delete, icon: "fia-delete", onClick: this.onDeleteStorage, isShow: showDeleteBtn },
        ];
        let showButtons = buttonsInfo.filter((item) => { return item.isShow; });
        return showButtons;
    }

    onRefresh = () => {
        this.setState({ showActions: this.defaultShowActions });
        this.initStorageSettingData(true);
    }

    onSelectChangeNewOpus = (items) => {
        let createBtn = true;
        let indexBtn = false;
        let deleteBtn = false;
        if (items.length == 1) {
            if (items[0].isIndex) {
                indexBtn = false;
                deleteBtn = false;
            } else if (items[0].Type == StorageTypeIndex.Box) {
                indexBtn = false;
                deleteBtn = true;
            } else if (!items[0].isIndex && items[0].Id.toLowerCase() == this.defaultDeviceId) {
                indexBtn = true;
                deleteBtn = false;
            } else {
                indexBtn = true;
                deleteBtn = true;
            }
        } else if (items.length > 1) {
            indexBtn = false;
            let cannotDel = items.find(r => r.isIndex === true || r.Id.toLowerCase() == this.defaultDeviceId);
            if (cannotDel) {
                deleteBtn = false;
            } else {
                deleteBtn = true;
            }
        }
        this.setState({
            showActions: {
                showCreateBtn: createBtn,
                showIndexBtn: indexBtn,
                showDeleteBtn: deleteBtn,
            },
            storageChecked: items
        }, () => {
            let showButtons = this.getShowActions();
            this.refTopButtons.updateButtons(showButtons);
        });
    }

    onSelectChange = (items) => {
        let indexBtn = false;
        if (items.length == 1 && !items[0].isIndex) {
            indexBtn = true;
        }
        this.setState({
            showActions: {
                showCreateBtn: false,
                showIndexBtn: indexBtn,
                showDeleteBtn: false,
            },
            storageChecked: items
        }, () => {
            let showButtons = this.getShowActions();
            this.refTopButtons.updateButtons(showButtons);
        });
    }

    onEditStorage = (rowData) => {
        this.setState({
            storagePanelTitle: RMResx.RM_AR_CP_GSS_PanelTitle_Edit,
            showStorageSettingsPanel: { show: true },
            cellStorageId: rowData.Id
        });
    }

    onCreateStorage = () => {
        this.setState({
            storagePanelTitle: RMResx.RM_AR_CP_GSS_PanelTitle_Create,
            showStorageSettingsPanel: { show: true },
            cellStorageId: null
        });
    }

    onShowExportIndexKey = () => {
        this.setState({
            isShowExportIndexKey: !this.state.isShowExportIndexKey,
            isShowCopyBtn: !this.state.isShowCopyBtn,
        });
    }

    exportIndexMessageBox = () => {
        const userGuideLink = getUserGuildTagPage(storageKeys.exportArchiveIndex);
        const args = {
            width: "500px",
            hideActions: false,
            title: RMResx.RM_CP_StorageSetting_Export_Index_Title,
            content: (
                <div className="flex flex-column">
                    <p style={{ marginTop: 0 }}>{RMResx.RM_CP_StorageSetting_Export_Index_Confirm}</p>
                    <$g.I18NProvider msg={RMResx.RM_CP_StorageSetting_Export_Index_Guide}>
                        <a className="ra-link-a" href={userGuideLink} target="_blank">
                            {RMResx.RM_CP_StorageSetting_Export_Index_GuideLink}
                        </a>
                    </$g.I18NProvider>
                </div>
            ),
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    id: "raCpStorageSettingExportIndexBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    onClick: this.onExportIndex,
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    exportIndexDialogMessage = () => {
        return (
            <R.Dialog
                id="ExportIndexKey"
                header={RMResx.RM_CP_StorageSetting_Export_Index_Title}
                width={500}
                height={330}
                status={{ show: this.state.isShowExportIndexKeyDialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={false}
            >
                <div className="export-key-wrapper">
                    <p>{RMResx.RM_CP_StorageSetting_Export_Index_CopyReminder}</p>
                    <R.Validation label={RMResx.RM_CP_StorageSetting_Export_KeyLabel} element="Input" require>
                        <div className="export-key-input-wrapper margin-top-8">
                            <R.Input
                                id="raCpStorageSettingExportIndexKeyInput"
                                type={this.state.isShowExportIndexKey ? "text" : "password"}
                                value={this.state.exportIndexKeyValue}
                                readonly={true}
                            />
                            <div className="export-key-eye-icon" onClick={this.onShowExportIndexKey}>
                                <span
                                    className={this.state.isShowExportIndexKey ? "fia-eye-slash" : "fia-eye"}
                                    tabIndex="0"
                                    role="button"
                                    aria-label={RMResx.RM_Common_ShowPassword}
                                ></span>
                            </div>
                        </div>
                    </R.Validation>
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCloseExportIndexDialogMessage} />
                    {this.state.isShowCopyBtn && (
                        <R.Button slot="buttons" id="raCpStorageSettingCopyExportIndexKeyBtn" primary classify="theme" text={RMResx.RM_JS_Common_Copy} onClick={() => {
                            this.onCopyExportIndexKey("#raCpStorageSettingExportIndexKeyInput input");
                            this.onCloseExportIndexDialogMessage();
                        }} />
                    )}
                </>
            </R.Dialog>
        )
    }

    onCloseExportIndexDialogMessage = () => {
        this.setState({
            isShowExportIndexKeyDialog: false,
            isShowExportIndexKey: false,
        });
    }

    onExportIndex = () => {
        $$.messagedialog(false);
        $$.loading(true);
        let urlData = "/api/StorageDevice/RunExportIndexJob";

        let option = {
            url: urlData,
            method: "POST",
            data: {},
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res.MessageType == MessageType.Successful) {
                this.setState({
                    isShowExportIndexKeyDialog: true,
                    exportIndexKeyValue: res.Extension,
                })
                const content = (
                    <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                    </$g.I18NProvider>
                );
                showToast.success(content);
            } else {
                showToast.error(res.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onCopyExportIndexKey = (selector) => {
        const exportIndexKey = document.querySelector(selector);
        exportIndexKey.select();
        navigator.clipboard.writeText(exportIndexKey.value);

        $$.loading(true);
        const urlData = "/api/StorageDevice/CopyIndexPassword";
        const option = {
            url: urlData,
            method: "POST",
            data: {},
        };
        fetchUtility(option).then(() => {}).finally(() => {
            $$.loading(false);
        });
    }

    onSetIndexStorage = () => {
        this.args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{this.enableRecordsArchiver ? RMResx.RM_AR_CP_GSS_NewOpusSetIndexMsg : RMResx.RM_AR_CP_GSS_SetIndexMsg}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.onSetIndexCancelClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onSetIndexSureClick }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onSetIndexSureClick = () => {
        $$.messagedialog(false);
        $$.loading(true);
        let urlData = "/api/StorageDevice/SetIndexDevice";
        let setIndexStorageId = this.state.storageChecked[0].Id;

        let option = {
            url: urlData,
            method: "POST",
            data: setIndexStorageId
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res.MessageType == MessageType.Successful) {
                this.onRefresh();
                let content = RMResx.RM_AR_CP_GSS_SetIndex_SaveSuccessful;
                if (this.enableRecordsArchiver && res.Extension != "" && res.Extension != null) {
                    content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                }
                showToast.success(content);
            } else {
                showToast.error(res.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onSetIndexCancelClick = () => {
        $$.messagedialog(false);
    }

    onDeleteStorage = () => {
        this.args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{RMResx.RM_AR_CP_GSS_DelStorageMsg}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.onDeleteCancelClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onDeleteSureClick }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onDeleteSureClick = () => {
        $$.messagedialog(false);
        $$.loading(true);
        let urlData = "/api/StorageDevice/DeleteStorageDevices";
        let storageIdList = [];
        for (let key of this.state.storageChecked) {
            storageIdList.push(key.Id);
        }
        let option = {
            url: urlData,
            method: "POST",
            data: storageIdList
        };
        fetchUtility(option).then((res) => {
            if (res.MessageType == MessageType.Successful) {
                this.onRefresh();
                showToast.success(RMResx.RM_AR_CP_GSS_DelStorageSuccessful);
            } else {
                showToast.error(res.ErrorMessage);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onDeleteCancelClick = () => {
        $$.messagedialog(false);
    }

    onSearchStart = (args) => {
        let searchValue = args;
        if (searchValue && searchValue != "") {
            this.filterData.SearchValue = searchValue;
            this.initStorageSettingData(true);
        } else {
            this.filterData.SearchValue = "";
            this.initStorageSettingData(false);
        }
    }

    onPagerChange = (pagerIndex, pagerSize, callback) => {
        this.filterData.PageIndex = pagerIndex + 1;
        this.filterData.PageSize = pagerSize;
        this.filterData.SearchValue = "";
        this.setState({
            storagePagerIndex: pagerIndex,
            storagePagerSize: pagerSize
        });
        this.initStorageSettingData(false);
        callback(true);
    };

    saveStorageSettings = (e) => {
        this.dispatch("storageSettingsPanel", 'onSave', (success, data) => {
            if (success) {
                this.setState({ showStorageSettingsPanel: { show: false } });
                this.onRefresh();
            }
        });
        return false;
    }

    cancelStorageSettings = () => {
        this.setState({ showStorageSettingsPanel: { show: false } });
    }

    hideMessageTip() {
        this.setState({
            showTip: false
        });
    }

    renderStorageHeader() {
        return <div className="ra-main-header">
            <R.Searchbox
                placeholder={RMResx.RM_AR_CP_Common_Search}
                disabled={false}
                onSearch={this.onSearchStart}
                width={380}
            />
        </div>;
    }

    renderStorageNavBar() {
        let selectStorageCount = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.storageChecked.length, this.state.storageCount);
        return < div className="ra-main-navbar">
            <div className="flex">
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: this.getShowActions() }}
                    showCount={4}
                ></TopButtonsComponent>
            </div>
            <div className="ra-main-selected-counter">{selectStorageCount}</div>
        </div >;
    }

    renderStorageTable() {
        return <div className="ra-main-table">
            <StorageTable
                id="raStorageTable"
                columns={this.state.allColumns}
                uniqueKey={"Id"}
                checkable={true}
                onChange={this.enableRecordsArchiver ? this.onSelectChangeNewOpus : this.onSelectChange}
                cellClick={this.onEditStorage}
            />
        </div>;
    }

    renderStorageFooter() {
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.storageCount}
                pagerIndex={this.state.storagePagerIndex}
                pagerSize={this.state.storagePagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onPagerChange} />
        </div>;
    }

    renderStorageSettingsPanel() {
        return <R.Panel
            id="raStorageSettingsPanel"
            header={this.state.storagePanelTitle}
            size={670}
            status={this.state.showStorageSettingsPanel}
            destroy={true}
        >
            <StoragePanel
                id="storageSettingsPanel"
                ref={r => this.refStorageSettingsPanel = r}
                cellStorageId={this.state.cellStorageId}
                indexDeviceId={this.state.indexDeviceId}
            ></StoragePanel>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.cancelStorageSettings}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.saveStorageSettings}
                />
            </>
        </R.Panel>;
    }

    render() {
        return <div id="raStorageSettingsManagement">
            <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_StorageSettings]} />
            {/* {this.enableRecordsArchiver && <div className="margin-bottom-l">
                <R.Messagebar
                    message={RMResx.RM_AR_CP_GSS_InfoMsg}
                    classify="info"
                    onClose={this.hideMessageTip}
                    status={{ show: this.state.showTip }} />
            </div>} */}
            <div className="ra-page-container">
                <section>
                    {this.renderStorageHeader()}
                    {this.renderStorageNavBar()}
                    {this.renderStorageTable()}
                    {this.renderStorageFooter()}
                </section>
            </div>
            {this.enableRecordsArchiver && this.renderStorageSettingsPanel()}
            {this.exportIndexDialogMessage()}
        </div>;
    }
}