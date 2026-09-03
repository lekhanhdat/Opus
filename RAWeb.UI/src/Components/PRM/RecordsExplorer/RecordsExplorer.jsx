import { createRef } from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import {
    PhysicalDefaultColumnNames,
    PhysicalDefaultColumnHoldTypeNames,
    PhysicalObjectStatus,
    PhysicalDefaultColumnIDs,
    PhysicalObjectColumnType,
    EmptyGUID,
    TelemetryModule,
    TelemetryEventType,
    RoleType,
} from "../../../Constants/Constants";
import {
    PhysicalTableColumnInfo,
    YesOrNo,
    BoxAndFolderNumType,
    TemplateTreeNodeType
} from "../Constants";
import PhyColumnUtil from "../../../Utilities/PhyColumnUtil";
import { NodeType } from "../../../Constants/DAEnums";
import PhyRecordHoldForm from './ManageHold/PhyRecordHoldForm';
import PhyObjectInfo from './Components/PhyObjectInfo';
import PhyObjectForm from '../Common/PhyObjectForm';
import PhyObjectFilter from './Components/PhyObjectFilter';
import PhyObjectMove from './Components/PhyObjectMove';
import PhyObjectDetail from '../Common/PhyObjectDetail';
import PhysicalExplorerTree from '../../Common/Tree/Instances/Physical/PhysicalExplorerTree';
import PhyExplorerTermView from '../../Common/Tree/Instances/Physical/PhyExplorerTermView';
import PhysicalObjectStatusLegend from '../../Common/Tree/Instances/Physical/PhysicalObjectStatusLegend';
import PhyLoanRequest from './Components/PhyLoanRequest';
import PhyReclassify from './Components/PhyReclassify';
import Table from './Components/Table/Table';
import PhyRelatedRecords from './Components/PhyRelatedRecords';
import PhyObjectManagePermission from './Components/PhyObjectManagePermission';
import "../../../Less/PRM/RecordsExplorer.less";
import "../../../Less/PRM/Reclassify.less";
import "../../../Less/PRM/PhyMove.less";
import "../../../Less/PRM/ReclassifyRuleDetails.less";
import RouterUrls from "../../../Constants/RouterUrls";
import { getRequestVerificationToken, showToast } from '../../../Utilities/CommonUtil';
import { addTelemetryRecord } from '../../../Utilities/TelemetryUtil';
import { checkPermission } from '../../../Utilities/permissionManager';
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import PhysicalReport from "./PhysicalReport";
import StringUtil from "../../../Utilities/StringUtil";
import PhyObjBulkUpdateForm from "../Common/PhyObjBulkUpdateForm";
import { LicenseHelper } from "../../../Utilities/CommonUtil";
import ShowAuditInfoPanel from "./Components/PhyObjectAuditInfo"
import PhyMovementRequest from './Components/PhyMovementRequest';

export const PhyObjFormType = {
    CreatePhyObj: 1,
    EditPhyObj: 2,
    NewRequest: 3,
    EditRequest: 4,
    MovePhyObj: 5,
    HoldPhyObj: 6
};

const TemplateTypes = {
    CustomTemplate: 5,
    Box: 3,
    Folder: 2,
    Record: 1
};

//Items in Location or File in Box orRecord in File
export const PhyObjTableItemPosAndTypeDes = {
    9250: RMResx.RM_PRM_PRE_ItemsInContainer,
    9200: RMResx.RM_PRM_PRE_ItemsInLocation,
    9300: RMResx.RM_PRM_PRE_FileInBox,
    9400: RMResx.RM_PRM_PRE_RecordInFile,
};

const TableNavBarBtnInMore = {
    // edit: {
    //     id: 1,
    //     name: '.edit'
    // },
    delete: {
        id: 2,
        name: RMResx.RM_PRM_PRE_Delete
    },
    // reclassify: {
    //     id: 3,
    //     name: '.reclassify'
    // },
    // move: {
    //     id: 4,
    //     name: '.move'
    // },
    // related:{
    //     id: 5,
    //     name: '.related'
    // }
};
const ExportType = {
    None: 0,
    ExportToBrowser: 1,
    ExportToFS: 2,
};
const TreeViewMode = {
    Term: 0,
    Location: 1
};

const BarcodeType = {
    Code128: 0,
    Code39: 1
};

const DefaultBarcodeStandard = [
    {key: BarcodeType.Code128, value: RMResx.RM_PRM_PRE_BarcodeStandard_Code128, checked: true },
    {key: BarcodeType.Code39, value: RMResx.RM_PRM_PRE_BarcodeStandard_Code39, checked: false }
];

export default class RecordsExplorer extends R.Component {
    componentCreate() {
        this.bind(['cellOperate', 
            'onSelectedNodeChanged', 'onTermViewSelectedNodeChanged', 'onNewRequest',
            'onCreatePhyObj', 'onCreateBox', 'onCreateFile', 'onEditPhyObj', 'onEditTablePhyObj', 'onDeletePhyObj',
            'onDeleteTablePhyObj', 'onMovePhyObj', 'onMoveTablePhyObj', 'onHoldPhyObj',
            'onSavePhyObj', 'openExportBarcodesDia', 'closeExportBarcodesDialog', 'onExportBarcodes', 'onExportToBrowser', 'onExportToLocation',
            'onExportLocationChange', 'onSaveHold', 'onMove', 'onFilter', 'onClearFilter', "getTableCellDetail", "onCheckChange", 'onSaveLoanObj',
            'onCloseViewDetail', 'onLoanPhyObj', 'onShowFilter', 'pagerChange', 'onSearchTableList', "onLoanTablePhyObj", "onShowPhyAuditObj",
            'onStopSearch', 'phyObjHandleSwitchButtonChanged', 'validDelItemsHasChildren', 'hideMessageTip', 'hideBarcodesMessageTip',
            'onReclassifyPhyObj', 'onReclassifyTablePhyObj', 'onPermitTablePhyObj', 'onPermitPhyObj', 'hideManagePermissionPanel',
            'onSaveReclassify', "onSaveRelated", "onSavePermission", "onRemovePersonHoldConfirmPhyObj",
            "onRemovePersonHoldConfirmTablePhyObj", "onRemovePersonHold", "onCancelRemovePersonHold", "jumpToSetting",
            "onRelatedPhyObj", "onRelatedTablePhyObj", "onPhysicalMoveHoldConflict", "sendMoveRequestWithConflictResolution",
            "setPanelHeader", "onBeforeEditTablePhyObj", "onEditBulkUpdateTablePhyObj", "openBulkUpdateForm", "onSaveBulkUpdatePhyObj"
            , "onHide", "onShowTableAudit",
            "onMoveRequestPhyObj", "onMoveRequestTablePhyObj", "onSavePhyMoveRequest",
            "loadEffectiveLocationPermission", "parseEffectiveLocationPermission",
            "isLocationNodeType", "refreshHoldActionsByEffectiveLocationPermission"
            , "isDelegatedAdminRole", "isAdminRole", "isHoldOnlyModeByLocationPermission",
            "applyLocationPermissionForCurrentSelection", "loadEffectiveLocationPermissionForCurrentPage"
        ]);

        this.viewUniqueId = RM.Url.getParam(window.location.href, "uniqueId");    //for Global Search
        this.firstLoadByUniqueId = !!this.viewUniqueId;
        this.isRMAdmin = RM.gData.isPhysicalAdmin;
        this.isHoldManagerOnly = Number(RM.RoleType) == RoleType.ManageHoldUser;
        this.isRoleAdmin = Number(RM.RoleType) === RoleType.SupAdmin;
        this.isRoleDelegatedAdmin = Number(RM.RoleType) === RoleType.DelegateAdmin;
        this.hasManageHold = RM.gData.hasManageHold;
        this.effectiveLocationPermissionMap = {};
        this.effectiveLocationHoldPermissionMap = {};
        this.effectiveLocationPermissionPageKey = "";
        this.operateLimitCount = 15;
        this.selectedTreeItem = null;
        this.boxAndFolderNumType = 0;
        this.operateFormNodeType = null;
        this.editingNodeType = null;
        this.creatOrEditParam = [];

        this.peHoldId = "explorerPhyHoldForm";
        this.peFormId = "explorerPhyObjectForm";
        this.peBulkUpdateFormId = "explorerBulkUpdateForm";
        this.peLoanId = "explorerLoanRequest";
        this.peReclassifyId = "explorerReclassify";
        this.peReclassifyRuleId = "explorerReclassifyRule";
        this.peRelatedId = "explorerManageRelatedRecords";
        this.tableSelectItems = [];

        this.tableColumnLists = [];
        this.cachePageBrowserState = [];
        this.searchKey = "";
        this.filterData = {};
        this.currentPage = {
            pageIndex: 0,
            pageSize: 10,
        };
        // this.basicInfoColNamesNotInMateInfo = [RMResx.RM_PRM_PRE_Column_PersonHoldStatus, RMResx.RM_PRM_PRE_Column_LoanBy, RMResx.RM_PRM_PRE_Column_DisposalStatus, RMResx.RM_PRM_PRE_Column_HoldBy, RMResx.RM_PRM_PRE_Column_Creator,
        //     RMResx.RM_PRM_PRE_Column_CreatedTime, RMResx.RM_PRM_PRE_Column_Modifier, RMResx.RM_PRM_PRE_Column_ModifiedTime];
        // this.basicInfoColAttrsNotInMateInfo = ['PersonHold', 'PersonHoldBy', 'DisposalHold', 'HoldBy', 'CreatedBy', 'CreateTime', 'ModifiedBy', 'ModifiedTime'];
        this.locationBasicInfoColAttrsNotInMate = ['CreatedBy', 'CreateTime', 'ModifiedBy', 'ModifiedTime'];
        this.locationBasicInfoColNamesNotInMate = [
            RMResx.RM_PRM_PRE_Column_Creator,
            RMResx.RM_PRM_PRE_Column_CreatedTime,
            RMResx.RM_PRM_PRE_Column_Modifier,
            RMResx.RM_PRM_PRE_Column_ModifiedTime
        ];
        this.containerBasicInfoColAttrsNotInMate = ['ModifiedBy', 'ModifiedTime'];
        this.containerBasicInfoColNamesNotInMate = [
            RMResx.RM_PRM_PRE_Column_Modifier,
            RMResx.RM_PRM_PRE_Column_ModifiedTime
        ];
        this.boxBasicInfoColAttrsNotInMate = ['DisposalHold', 'HoldBy', 'HoldProfileTitle', 'HoldReleaseTime', 'ModifiedBy', 'ModifiedTime'];
        this.boxBasicInfoColNamesNotInMate = [
            RMResx.RM_PRM_PRE_Column_DisposalStatus,
            RMResx.RM_PRM_PRE_Column_HoldBy,
            RMResx.RM_PRM_PRE_Column_HoldType,
            RMResx.RM_PRM_PRE_Column_HoldUntil,
            RMResx.RM_PRM_PRE_Column_Modifier,
            RMResx.RM_PRM_PRE_Column_ModifiedTime
        ];
        this.fileBasicInfoColAttrsNotInMate = ['PersonHold', 'PersonHoldBy', 'DisposalHold', 'HoldBy', 'HoldProfileTitle', 'HoldReleaseTime', 'ModifiedBy', 'ModifiedTime'];
        this.fileBasicInfoColNamesNotInMate = [
            RMResx.RM_PRM_PRE_Column_PersonHoldStatus,
            RMResx.RM_PRM_PRE_Column_LoanBy,
            RMResx.RM_PRM_PRE_Column_DisposalStatus,
            RMResx.RM_PRM_PRE_Column_HoldBy,
            RMResx.RM_PRM_PRE_Column_HoldType,
            RMResx.RM_PRM_PRE_Column_HoldUntil,
            RMResx.RM_PRM_PRE_Column_Modifier,
            RMResx.RM_PRM_PRE_Column_ModifiedTime
        ];
        this.nodeTypeAndTemplateIdMapping = null;
        this.state = {
            barcodesShowTip: false,
            treeData: !this.viewUniqueId ? null : [],
            formPanelTitle: "",
            formTemplateName: "",
            showHoldPanel: { show: false },
            showFormPanel: { show: false },
            showLoanPanel: { show: false },
            showMovePanel: { show: false },
            showPhyMoveRequestPanel: { show: false },
            showRelatedPanel: { show: false },
            showFilterPanel: { show: false },
            showViewDetailPanel: { show: false },
            showReclassifyPanel: { show: false },
            showRelatedRecordsPanel: { show: false },
            removeHoldDialogShow: false,
            removeHoldSelectDialogShow: false,
            removePersonHoldDialogShow: false,
            boxHoldDialogShow: false,
            physicalMoveHoldConflictDialogShow: false,
            showManagePermissionPanel: false,
            isTopPermissionBtn: false,
            phyObjSwitchButtonChecked: true,
            currentPhyObj: null,
            currentPhyObjMetaData: [],
            currentLocationList: [],
            phyObjDetailParam: {},
            phyObjLoanParam: {},
            phyObjMoveParam: {},
            phyObjReclassifyParam: {},
            phyObjFilterEchoData: {},
            selectedPhyObjByTableOrTree: [],
            allowExportBarcode: false,
            allowNewChildren: false,
            tableNavBarBtnItemsInMore: [],
            mainNavbarBtnItemsInMore: [],
            moreTableNavBarBtnMenuShow: true,
            tableNavBarBtnAllow: {
                edit: false,
                delete: false,
                reclassify: false,
                move: false,
                phyMoveRequest: false,
                hold: false,
                loan: false,
                del_personal_hold: false,
                related: false,
                more: false,
                allowSetPermissions: false,
                allowShowAudit: false
            },
            currentPhyObjNavBarAllow: {
                edit: false,
                delete: false,
                reclassify: false,
                move: false,
                phyMoveRequest: false,
                hold: false,
                loan: false,
                del_personal_hold: false,
                related: false,
                allowSetPermissions: false,
                allowShowAudit: false
            },
            pager: {
                pageIndex: 0,
                pageSize: 10,
                shownCount: 10,
                hasNext: false,
            },
            tipMsgObj: this.showMessageTip,
            hasTermSettings: false,
            hasUniqueIdSettingsMapping: {},
            smallNodeType: NodeType.PhyBox,
            selectedMoveHoldConflict: "",
            //导出barcodes
            isExportToBrowser: true,
            noDownLoadToValue: false,
            exportBarcodesDiaShow: false,
            exportLocations: [],
            selectedExportLocation: {},
            isStandardUserHasPermission: true,
            viewModeLoaded: false,
            viewMode: TreeViewMode.Location,
            termUsageData: [],
            showImportPanel: { show: false },
            showDownloadTemplatePanel: { show: false },
            files: [],
            skipExistObjChecked: true,
            enableCustomTime: true,
            suiteList: [],
            suiteId: "",
            phyObjBulkUpdateData: {},
            showBulkUpdateFormPanel: { show: false },
            removeHoldProfileList: [],
            showBulkExportUpdatePanel: { show: false },
            showSettingDialog: { show: false },
            bulkUpdateFiles: [],
            showExportRecordsDialog: false,
            templateList: [],
            selectedTemplateIds: [],
            barcodeStandard:[...DefaultBarcodeStandard],
            showButtons: false,
            showBarcodeMessage: false,
            showAuditPanel : false,
            selectedTableItem : {},
            isPhysicalEndUser : false,
            barcodeTemplateList: [],
            selectedBarcodeTemplate: null,
            allowHoldActionsByEffectiveLocation: true,
            allowFullActionsByEffectiveLocation: true,
        };
        this.barcodeStandardRef = createRef();
        this.menuBtnItems = [];
        this.setPermissionButton = { id: "raPhyAccessControlBtnTop", name: RMResx.RM_PRM_PRE_ManagePermission, icon: "fia-manage-access-control", onClick: this.onPermitPhyObj };
        this.editPhyObjButton = { id: "raPhyEditBtnTop",name: RMResx.RM_PRM_PRE_Edit, icon: "fia-edit", onClick: this.onEditPhyObj };
        this.relatedPhyObjButton = { id: "raPhyRelatedBtnTop", name: RMResx.RM_PRM_PRE_Related, icon: "fia-related-records", onClick: this.onRelatedPhyObj };
        this.movePhyObjButton = { id: "raPhyMoveBtnTop", name: RMResx.RM_PRM_PRE_Move, icon: "fia-move", onClick: this.onMovePhyObj };
        this.removePersonHoldPhyObjButton = {id: "raPhyReturnBtnTop", name: RMResx.RM_PRM_PRE_Return, icon: "fia-return", onClick: this.onRemovePersonHoldConfirmPhyObj };
        this.reclassifyObjButton = {id: "raPhyReclassifyBtnTop", name: RMResx.RM_JS_BCM_Explorer_ChangeTerm, icon: "fia-reclassify", onClick: this.onReclassifyPhyObj };
        this.deleteObjButton = {id: "raPhyDeleteBtnTop",  name: RMResx.RM_PRM_PRE_Delete, icon: "fia-delete", onClick: this.onDeletePhyObj };
        this.loanObjButton = { id: "raPhyLoanBtnTop", name: RMResx.RM_PRM_PRE_NewLoanRequest, isStatic: true, onClick: this.onLoanPhyObj };
        this.moveRequestPhyObjButton = { id: "raPhyMoveRequestBtnTop", name: RMResx.RM_PRM_PRE_MovementRequest, icon: "fia-move", onClick: this.onMoveRequestPhyObj };
        this.showObjAuditButton = {id: "raPhyShowAuditBtnTop", name: RMResx.RM_PRM_PRE_ShowAudit, icon: "fia-select-all", onClick: this.onShowPhyAuditObj };
        this.menuBtnItemsInMore = [];
        this.menuBtnItems_table = [];
        this.menuBtnItemsInMore_table = [];
        this.uploaderRef = React.createRef();
        this.menuBtnItems_navbarRight = [];
        this.menuBtnItemsInMore_navbarRight = [];
        this.manageHoldBtn = { isStatic: true, id: "raPhyManangeHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_ManageHold, onClick: this.onManageHoldAction.bind(this, "manage") };
        this.importBtn = { id: "raPhyImportBtn", name: RMResx.RM_JS_TM_Import, icon: "fia-import", onClick: this.handleExploreImport.bind(this) };
        this.exportBarcodeBtn = { id: "raPhyExportBarcodeBtn", name: RMResx.RM_RDM_Explorer_ExportBarcodeBtn, icon: "fia-download", onClick: this.openExportBarcodesDia.bind(this) };
        this.bulkUpdateBtn = { id: "raPhyBulkExportUpdateBtn", name: RMResx.RM_PRM_PRE_BulkUpdate, icon: "fia-bulk-update", onClick: this.handleBulkExportUpdate.bind(this) };
        this.settingBtn = { id: "raPhySetting", name: RMResx.RM_PRM_PRE_Setting, icon: "fia-bulk-update", onClick: this.handleSettingClick.bind(this) };
    }

    componentInit() {
        if (this.viewUniqueId) {
            this.initSearchView();
            this.setState({
                viewModeLoaded: true,
                viewMode: TreeViewMode.Location
            });
        } else {
            this.initViewMode();
        }
        this.initTermUsageInfo();
        // this.validHasUniqueIdSettings();
        this.getTemplateIdAndNodeTypeMapping();
        this.initBarcodeSetting();
        addTelemetryRecord(TelemetryModule.PhysicalRecordsExplorer, TelemetryEventType.ContentPageLoaded);
        this.initIsEndUser();
    }

    componentUpdate(prevProps, prevState) {
    }

    initViewMode() {
        let option = {
            url: `/api/PhysicalRecordApi/GetViewMode`,
            method: "GET"
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            this.setState({
                viewModeLoaded: true,
                viewMode: res
            });
            $$.loading(false);
        });
    }

    initIsEndUser() {
        let option = {
            url: "/api/Dashboard/IsPhysicalEndUser",
            method: "POST"
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            this.setState({
                isPhysicalEndUser: res
            });
            $$.loading(false);
        });
    }


    initTermUsageInfo() {
        let url = "/api/PhysicalRecordApi/GetTermUsageInfo";
        let option = {
            url: url,
            method: "POST",
            data: {
                SourceFlag: 4
            }
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (data) {
                // data.forEach(element => {
                //     element.value = element.Content,
                //     element.name = element.Title
                // });
            }
            this.setState({
                termUsageData: data
            });
        });
    }

    initBarcodeSetting = async () => {
        let result = await fetchUtility({  url: "/api/PhysicalRecordApi/GetBarcodeStandard" });
        let barcodeInfo = RM.deepcopy(this.state.barcodeStandard);
        barcodeInfo.forEach((item) => {
            item.checked = result === item.key;
        });
        this.setState({
            barcodeStandard:barcodeInfo,
            showBarcodeMessage : result === 1
        });
        this.barcodeStandardRef = barcodeInfo;
    }

    resetTreeHeight(times) {
        setTimeout(() => {
            let rHeight = $(".raPhyObjInfoContainer").height();
            let lHeight = $("#raExplorerTreeContainer").height();
            if (rHeight != lHeight) {
                $("#raExplorerTreeContainer").css("height", rHeight + "px");
            } else if (times > 0) {
                this.resetTreeHeight(--times);
            }
        }, 200);
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    showMessageTip(type, msg) {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip() {
        this.setState({
            showTip: false
        });
    }

    initSearchView() {
        let option = {
            url: `/api/PhysicalRecordApi/SearchTree?uniqueId=${this.viewUniqueId}`
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            if (res.success) {
                this.selectedTreeItem = this.getSelectedItemFromTreeData(res.treeData[0]);
                this.loadEffectiveLocationPermission(this.selectedTreeItem);
                let phyObj = res.selectPhyObj;
                this.setState({
                    treeData: res.treeData,
                    currentPhyObj: phyObj,
                    currentPhyObjMetaData: this.getPhyObjMetadata(phyObj),
                    hasTermSettings: true,
                    allowNewChildren: this.isAllowNewChildren(phyObj),
                }, () => {
                    setTimeout(() => {
                        this.getCurrentPhyObjNavBarBtnShow();
                        if (res.showTableSearchKey) {
                            this.refTableSearchBox.setValue(this.viewUniqueId);
                        }
                        this.dispatch('phyObjTable', 'reset');
                        this.setTableData(res.tableData, NodeType.PhysicalBottomLocation, true);
                    }, 100);
                });
            } else {
                this.selectedTreeItem = res.treeData[0];
                this.loadEffectiveLocationPermission(this.selectedTreeItem);
                this.initCurrentPhysicalObjectInfo(this.selectedTreeItem);
                this.getPhysicalObjectList(this.selectedTreeItem, true);
                this.setState({ treeData: res.treeData });
            }

            $$.loading(false);
        });
    }

    getSelectedItemFromTreeData(treeItem) {
        if (treeItem.Children) {
            for (const child of treeItem.Children) {
                if (child.Checked) {
                    return child;
                }
                let selItem = this.getSelectedItemFromTreeData(child);
                if (selItem) {
                    return selItem;
                }
            }
        }
        return null;
    }

    jumpToSetting() {
        if (this.isRMAdmin) {
            this.routerTo(RouterUrls.PRM_LocationManagement);
        } else {
            this.routerTo(RouterUrls.BCM_ContentRepositoryManagement_Phy);
        }
    }

    validSettings(nodeItem, callback) {
        let msgContent = RMResx.RM_PRM_PRE_Msg_OnlyHasRootLocation;
        if (this.selectedTreeItem.NodeType == NodeType.PhysicalRootLocation) {
            if (this.selectedTreeItem.ChildrenCount == 0 && this.isRMAdmin) {
                this.showMessageTip('warn',
                    <$g.I18NProvider msg={msgContent}>
                        <a
                            className="ra-link-a ra-cursor-pointer"
                            onClick={this.jumpToSetting}>
                            {RMResx.RM_Nav_PR_LocationManager}
                        </a>
                    </$g.I18NProvider>
                );
            } else {
                this.hideMessageTip();
            }
            this.setState({ hasTermSettings: true });
            this.initCurrentPhysicalObjectInfo(nodeItem, false, callback);
        } else {
            $$.loading(true);
            let locationId = this.selectedTreeItem.LocationId;
            let option = {
                url: `/api/PhysicalRecordApi/HasTermSettingsForLocation?locationId=${locationId}`,
                method: "GET"
            };
            fetchUtility(option).then((result) => {
                $$.loading(false);
                if (result) {
                    this.hideMessageTip();
                    this.setState({
                        hasTermSettings: true,
                        allowExportBarcode: this.isAllowExportBarcode(this.selectedTreeItem, true)
                    }, () => {
                        this.initCurrentPhysicalObjectInfo(nodeItem, false, callback);
                    });
                } else {
                    this.showMessageTip(
                        'warn',
                        <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Msg_NoClassification}>
                            <a className="ra-link-a" href={RouterUrls.BCM_ContentRepositoryManagement_Phy} style={{ color: "#0072d0" }}>{`${RMResx.RM_JS_SPS_TabLabel_Physical} ${RMResx.RM_Nav_ContentRepository}`}</a>
                        </$g.I18NProvider>
                    );
                    this.setState({
                        hasTermSettings: false,
                        allowExportBarcode: this.isAllowExportBarcode(this.selectedTreeItem, false)
                    }, () => {
                        this.initCurrentPhysicalObjectInfo(nodeItem, false, callback);
                    });
                }
            });
        }
    }

    getAllBarcodeTemplateSuites = async () => {
        const requestOption = {
            url: "/api/TemplateManagementApi/GetAllBarcodeTemplateSuites",
            method: "GET",
        };
        $$.loading(true);
        const res = await fetchUtility(requestOption);
        $$.loading(false);
        if (res) {
            return res;
        }
        return [];
    };

    openExportBarcodesDia() {
        $$.loading(true);
        let currentPhyObj = this.state.currentPhyObj;
        //当前选择treeNode中含有folder和box的数量的type
        let url = "/api/PhysicalRecordApi/GetSelectNodeAllChildCount";
        let option = {
            url: url,
            method: "POST",
            data: {
                NodeId: currentPhyObj.Id,
                NodeType: currentPhyObj.NodeType,
            }
        };
        fetchUtility(option).then((res) => {
            if (res == BoxAndFolderNumType.BoxAndFildIsZero && currentPhyObj.NodeType < NodeType.PhyBox) {
                showToast.warn(RMResx.RM_RDM_ExportBarcode_Msg_HasNoBoxOrFolder);
            } else {
                this.getAllBarcodeTemplateSuites().then((result) => {
                    const barcodeTemplateList = result.map((item) => ({
                        name: item.Name,
                        value: item.SuiteId,
                        checked: false,
                    }))
                    this.boxAndFolderNumType = res;
                    this.setState({ exportBarcodesDiaShow: true, barcodeTemplateList, });
                    this.loadExportBarcordWithLocation();
                })
            }
            $$.loading(false);
        });
    }

    loadExportBarcordWithLocation() {
        $$.loading(true);
        let url = "/api/JMApi/GetJobExportSetting";
        let option = {
            url: url,
            method: "Get"
        };
        fetchUtility(option).then((res) => {
            this.setState({ exportLocations: JSON.parse(res).AllExportLocation });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleChangeBarcodeTemplate = (args) => {
        const newValue = args.newValue.value;
        let clonedBarcodeTemplateList = RM.deepcopy(this.state.barcodeTemplateList);
        clonedBarcodeTemplateList = clonedBarcodeTemplateList.map((item) => ({
            ...item,
            checked: item.value === newValue,
        }));
        this.setState({
            barcodeTemplateList: clonedBarcodeTemplateList,
            selectedBarcodeTemplate: newValue,
        });
    }

    onExportToBrowser() {
        this.setState({ isExportToBrowser: true });
    }

    onExportToLocation() {
        this.setState({
            isExportToBrowser: false,
            barcodesShowTip: false
        });
    }

    onExportLocationChange(args) {
        let exportLocation = args.newValue;
        this.setState({
            selectedExportLocation: exportLocation,
            noDownLoadToValue: false,
        });
    }

    onSubmitBarcodesForm(exportType) {
        let messageContent = '';
        let currentPhyObj = this.state.currentPhyObj;
        let fullPath = currentPhyObj.MetaInfo[PhysicalDefaultColumnIDs.Path] + '/' + currentPhyObj.MetaInfo[PhysicalDefaultColumnIDs.NameOrTitle];
        if (currentPhyObj.HomeLocationFullPath) {
            fullPath = currentPhyObj.HomeLocationFullPath + '/' + currentPhyObj.Name;
        }
        if (exportType == ExportType.ExportToFS) {
            $$.loading(true);
            let url = `/api/PhysicalRecordApi/ExportBarcodeToLD`;
            let selectedExportLocationId = null;
            let selectedExportLocationName = null;
            if (!this.state.isExportToBrowser) {
                selectedExportLocationId = this.state.selectedExportLocation.ID;
                selectedExportLocationName = this.state.selectedExportLocation.Name;
            }
            let param = {
                ExportType: exportType,
                NodeId: currentPhyObj.Id,
                NodeType: currentPhyObj.NodeType,
                FullPath: fullPath,
                ExportLocationId: selectedExportLocationId,
                ExportLocationName: selectedExportLocationName,
                SuiteId: this.state.selectedBarcodeTemplate || "",
            };
            let option = {
                url: url,
                method: "POST",
                data: param
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);
                if (res.MessageType == 0) {
                    messageContent = <$g.I18NProvider msg={RMResx.RM_RDM_Explorer_ExportBarcode_ExportToLocation_Message}>
                        <a className="ra-link-a" tabIndex="0" onClick={() => this.routerTo(RouterUrls.JM_Index)}>{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(messageContent);
                    this.setState({
                        exportBarcodesDiaShow: false
                    });
                } else if (res.ErrorMessage.indexOf(RMResx.RM_JS_CP_GSS_FTPExportLocationNotSupported) >= 0) {
                    showToast.error(RMResx.RM_JS_CP_GSS_FTPExportLocationNotSupported);
                }
            }).catch((e) => {
                $$.loading(false);
                showToast.error(messageContent);
            });
        } else {
            let downloadUrl = "/api/PhysicalRecordApi/ExportBarcode";
            let divElement = document.getElementById("downloadDiv");
            ReactDOM.render(
                <form action={downloadUrl} method='post'>
                    <input id='exportBarcodeType' type="hidden" name="ExportType" value={exportType} />
                    <input id='exportBarcodeNodeId' type="hidden" name="NodeId" value={currentPhyObj.Id} />
                    <input id='exportBarcodeNodeType' type="hidden" name="NodeType" value={currentPhyObj.NodeType} />
                    <input id='exportBarcodeNodeFullPath' type="hidden" name="FullPath" value={fullPath} />
                    <input id='exportBarcodeSelectedTemplate' type="hidden" name="SuiteId" value={this.state.selectedBarcodeTemplate || ""} />
                    {/* <input name='RequestVerificationToken' type='text' value={getRequestVerificationToken} readOnly /> */}
                </form>,
                divElement
            );
            divElement.querySelector("form").submit();
            ReactDOM.unmountComponentAtNode(divElement);

            let currentSelectedTreeName = this.selectedTreeItem.Name;
            messageContent = <$g.I18NProvider msg={RMResx.RM_RDM_Explorer_ExportBarcode_ExportToBrowser_Message}>
                {currentSelectedTreeName}
            </$g.I18NProvider>;
            showToast.success(messageContent);
            this.setState({
                exportBarcodesDiaShow: false
            });
        }
    }

    onExportBarcodes() {
        if (!$$.verify("export-barcodes-dialog")) return false;

        let exportType = this.state.isExportToBrowser ? ExportType.ExportToBrowser : ExportType.ExportToFS;
        if (!this.state.isExportToBrowser && (!this.state.selectedExportLocation.ID || this.state.selectedExportLocation.ID == EmptyGUID)) {
            this.setState({ noDownLoadToValue: true });
            return;
        }
        if (this.state.isExportToBrowser && this.boxAndFolderNumType == BoxAndFolderNumType.BoxAndFildIsZeroMoreThan300) {
            this.setState({
                barcodesShowTip: true
            });
            return;
        }
        this.onSubmitBarcodesForm(exportType);
    }

    hideBarcodesMessageTip() {
        this.setState({
            barcodesShowTip: false
        });
    }

    closeExportBarcodesDialog() {
        this.setState({
            exportBarcodesDiaShow: false,
            isExportToBrowser: true,
            noDownLoadToValue: false,
            barcodesShowTip: false,
            selectedExportLocation: {},
        });
    }

    cellOperate(args, tableSelectedOption) {
        switch (tableSelectedOption.index) {
            case 1:
                this.openForm(PhyObjFormType.EditPhyObj, args.NodeType, args.Id);
                break;
            case 2:
                this.validDelItemsHasChildren(true, [args]);
                break;
            case 3:
                this.openForm(PhyObjFormType.MovePhyObj, args.NodeType, args.Id);
                break;
            case 4:
                this.openForm(PhyObjFormType.HoldPhyObj, args.NodeType, args.Id);
                break;
        }
    }

    onCheckChange(data) {
        this.tableSelectItems = data;
        if(this.tableSelectItems.length == 1){
            this.setState({ selectedTableItem : this.tableSelectItems[0]})
        }

        if (this.isDelegatedAdminRole()) {
            this.applyLocationPermissionForCurrentSelection();
        }

        this.getTableNavBarBtnShow();
    }

    getTableNavBarBtnShow() {
        let selItemsCount = this.tableSelectItems.length;
        let tableNavBarBtnAllow = this.state.tableNavBarBtnAllow;
        for (let key in tableNavBarBtnAllow) {
            tableNavBarBtnAllow[key] = false;
        }
        if (selItemsCount != 0) {
            if (this.isAllowLoan(this.tableSelectItems)) {
                tableNavBarBtnAllow['loan'] = true;
            }
            if (this.isAllowMoveRequest(this.tableSelectItems, false)) {
                tableNavBarBtnAllow['phyMoveRequest'] = true;
            }
            if (this.isAllowEdit(this.tableSelectItems)) {
                tableNavBarBtnAllow['edit'] = true;
            }
            if (this.isAllowDelete(this.tableSelectItems)) {
                tableNavBarBtnAllow['delete'] = true;
            }
            if (this.isAllowMove(this.tableSelectItems)) {
                tableNavBarBtnAllow['move'] = true;
            }
            if (this.isAllowReclassify(this.tableSelectItems)) {
                tableNavBarBtnAllow['reclassify'] = true;
            }
            if (this.isAllowRemovePersonHold(this.tableSelectItems)) {
                tableNavBarBtnAllow['del_personal_hold'] = true;
            }
            if (this.isAllowRelated(this.tableSelectItems)) {
                tableNavBarBtnAllow['related'] = true;
            }
            if (this.isAllowSetPermissions(this.tableSelectItems)) {
                tableNavBarBtnAllow['allowSetPermissions'] = true;
            }
            if(this.isAllowShowAudit(this.tableSelectItems)){
                tableNavBarBtnAllow['allowShowAudit'] = true;
            }
            if (this.isAllowMore(tableNavBarBtnAllow)) {
                tableNavBarBtnAllow['more'] = true;
            }
        }
        this.setState({
            tableNavBarBtnAllow: Object.assign({}, tableNavBarBtnAllow),
            allowNewChildren: this.isAllowNewChildren(this.state.currentPhyObj)
        }, () => {
            this.updateTopButtonsForTable();
        });
    }

    getCurrentPhyObjNavBarBtnShow() {
        let phyObj = this.state.currentPhyObj;
        let currentPhyArr = [];
        currentPhyArr.push(phyObj);
        let currentPhyObjNavBarAllow = this.state.currentPhyObjNavBarAllow;
        for (let key in currentPhyObjNavBarAllow) {
            currentPhyObjNavBarAllow[key] = false;
        }
        if (this.isAllowLoan(currentPhyArr)) {
            currentPhyObjNavBarAllow['loan'] = true;
        }
        if (this.isAllowMoveRequest(currentPhyArr, true)) {
            currentPhyObjNavBarAllow['phyMoveRequest'] = true;
        }
        if (this.isAllowEdit(currentPhyArr)) {
            currentPhyObjNavBarAllow['edit'] = true;
        }
        if (this.isAllowDelete(currentPhyArr)) {
            currentPhyObjNavBarAllow['delete'] = true;
        }
        if (this.isAllowMove(currentPhyArr)) {
            currentPhyObjNavBarAllow['move'] = true;
        }
        if (this.isAllowReclassify(currentPhyArr)) {
            currentPhyObjNavBarAllow['reclassify'] = true;
        }
        if (this.isAllowRemovePersonHold(currentPhyArr)) {
            currentPhyObjNavBarAllow['del_personal_hold'] = true;
        }
        if (this.isAllowRelated(currentPhyArr)) {
            currentPhyObjNavBarAllow['related'] = true;
        }
        if (this.isAllowSetPermissions(currentPhyArr)) {
            currentPhyObjNavBarAllow['allowSetPermissions'] = true;
        }
        if(this.isAllowShowAudit(currentPhyArr)){
            currentPhyObjNavBarAllow['allowShowAudit'] = true;
        }
        this.setState({ currentPhyObjNavBarAllow: currentPhyObjNavBarAllow }, () => {
            this.updateTopButtons(phyObj);
            this.updateTopButtonsNavbarRight(phyObj);
        });
    }

    getPhysicalObjectList(nodeItem, isResetPagerIdx, withoutLoading, telemetryEvent) {
        if (!withoutLoading) {
            $$.loading(true);
        }
        let pagePager = {};
        let curNodeType = nodeItem.NodeType;
        this.filterData.SearchKey = this.searchKey;
        this.currentPage.pageIndex = isResetPagerIdx ? 0 : this.currentPage.pageIndex;
        pagePager.PageIndex = this.currentPage.pageIndex;
        pagePager.PageSize = this.currentPage.pageSize;
        if (curNodeType >= NodeType.PhysicalBottomLocation && pagePager.PageIndex != 0) {
            if (this.currentPage.pageIndex < this.state.pager.pageIndex) {
                pagePager.currentBrowserState = this.cachePageBrowserState[this.currentPage.pageIndex - 1];
            } else {
                pagePager.currentBrowserState = this.state.pager.currentBrowserState;
            }
        }
        let param = {
            NodeId: nodeItem.Id,
            CurrentNodeType: curNodeType,
            FilterOption: this.filterData,
            PagingInfo: pagePager
        };
        if (this.state.viewMode == TreeViewMode.Term) {
            param.FilterOption.TermTreeFilter = nodeItem.TermId;
        }
        let url = `/api/PhysicalRecordApi/GetPhysicalObjectList`;
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        let sendDate = new Date().getTime();
        fetchUtility(option).then((result) => {
            let receiveDate = new Date().getTime();
            let responseTime = receiveDate - sendDate;
            $$.loading(false);
            let res = JSON.parse(result);
            this.setTableData(res, curNodeType, isResetPagerIdx);
            if (telemetryEvent === TelemetryEventType.Search) {
                addTelemetryRecord(TelemetryModule.PhysicalRecordsExplorer, TelemetryEventType.Search, [res.PagingInfo.HasNextPage, responseTime]);
            }
            else if (telemetryEvent == TelemetryEventType.Filter) {
                addTelemetryRecord(TelemetryModule.PhysicalRecordsExplorer, TelemetryEventType.Filter, [this.filterData, responseTime]);
            }
            this.setState({ showFilterPanel: { show: false } });
        });
    }

    setTableData(res, curNodeType, isResetPagerIdx) {
        let hasNext = false;
        let currentPhysicalObjectList = res.Datas;
        let pagingInfo = res.PagingInfo;
        let tableColumnInfo = this.getTableColumns(curNodeType);
        let currentBrowserState = '';
        let tableData = {
            currentPhysicalObjectList: currentPhysicalObjectList,
            tableColumnInfo: tableColumnInfo,
            curNodeType: curNodeType,
            showCheckbox: (this.isRMAdmin && curNodeType >= NodeType.PhysicalBottomLocation)
                || (!this.isRMAdmin
                    && (curNodeType == NodeType.PhysicalBottomLocation
                        || curNodeType == NodeType.PhyCustom
                        || curNodeType == NodeType.PhyBox
                        || curNodeType == NodeType.PhyFile)),
            showActions: false,//this.isRMAdmin && curNodeType >= NodeType.PhysicalBottomLocation,
            pagerTotalCount: res.PagingInfo.Total
        };
        if (curNodeType < NodeType.PhysicalBottomLocation) {
            hasNext = pagingInfo.Total - (pagingInfo.PageIndex + 1) * pagingInfo.PageSize > 0;
        } else {
            currentBrowserState = pagingInfo.currentBrowserState;
            hasNext = pagingInfo.HasNextPage;
            if (pagingInfo.PageIndex >= this.state.pager.pageIndex || pagingInfo.PageIndex == 0) {
                if (pagingInfo.PageIndex == 0) {
                    this.cachePageBrowserState = [];
                }
                if (this.cachePageBrowserState.indexOf(currentBrowserState) == -1) {
                    if (currentBrowserState) {
                        this.cachePageBrowserState.push(currentBrowserState);
                    }
                }
            }
        }
        let pager = {
            pageIndex: pagingInfo.PageIndex,
            pageSize: pagingInfo.PageSize,
            shownCount: currentPhysicalObjectList.length,
            hasNext: hasNext,
            currentBrowserState: currentBrowserState
        };
        this.setState({
            pager: pager
        }, () => {
            this.dispatch('phyObjTable', 'setData', tableData);
            this.loadEffectiveLocationPermissionForCurrentPage(currentPhysicalObjectList, pagingInfo.PageIndex, pagingInfo.PageSize);
        });
        if (isResetPagerIdx) {
            this.dispatch('phyObjTable', 'reset');
        }
    }

    onSearchTableList(args) {
        if ((args || "").trim() === "") {
            this.onStopSearch();
            return;
        }
        this.filterData.NodeType = this.filterData.NodeType || -4;
        this.filterData.Status = this.filterData.Status || -1;
        this.searchKey = args;
        if (this.firstLoadByUniqueId) {
            this.firstLoadByUniqueId = false;
        } else {
            this.getPhysicalObjectList(this.selectedTreeItem, true, undefined, TelemetryEventType.Search);
        }
    }

    onStopSearch() {
        this.searchKey = '';
        if(this.filterData.NodeType == -4){
            delete this.filterData.NodeType;
        }
        if(this.filterData.Status == -1){
            delete this.filterData.Status;
        }
        this.getPhysicalObjectList(this.selectedTreeItem, true);
    }

    getTableColumns(nodeType) {
        if (this.tableColumnLists[nodeType]) {
            return this.tableColumnLists[nodeType];
        }

        let tableColumns = [];
        let baseColumns = null;
        if (nodeType < NodeType.PhysicalBottomLocation) {
            baseColumns = PhysicalTableColumnInfo.Location;
        } else {
            baseColumns = PhysicalTableColumnInfo[nodeType];
        }

        let headers = baseColumns.header;
        let columnsWidth = baseColumns.width;
        let columnIds = baseColumns.id;
        for (let key in headers) {
            if (headers.hasOwnProperty(key)) {
                let columnObj = {};
                if (headers[key] == RMResx.RM_PRM_PRE_Column_TotalSpace || headers[key] == RMResx.RM_PRM_PRE_Column_Capacity) {
                    columnObj.header = <span>
                        <span> {headers[key]}</span>
                        <span className='capacity-unit'> ({RMResx.RM_PRM_PRE_TotalSpace_Unit})</span>
                    </span>;
                } else {
                    columnObj.header = headers[key];
                }
                columnObj.width = [columnsWidth[key]];
                columnObj.id = columnIds[key];
                columnObj.resizeable = true;
                tableColumns.push(columnObj);
            }
        }

        this.tableColumnLists[nodeType] = tableColumns;
        return tableColumns;
    }

    validHasUniqueIdSettings(templateUniqueId) {
        let option = {
            method: "GET",
            url: "/api/PhysicalRecordApi/ValidHasUniqueIdSettings"
        };
        fetchUtility(option).then((res) => {
            this.setState({
                hasUniqueIdSettingsMapping: res
            });
        });
    }

    getTemplateIdAndNodeTypeMapping() {
        let option = {
            method: "GET",
            url: "/api/TemplateManagementApi/GetNodeTypeAndTemplateIdMapping"
        };
        fetchUtility(option).then((res) => {
            this.nodeTypeAndTemplateIdMapping = res;
        });
    }

    initCurrentPhysicalObjectInfo(nodeItem, withoutLoading, thenFunction) {
        if (!withoutLoading) {
            $$.loading(true);
        }
        let url = `/api/PhysicalRecordApi/GetPhysicalObjectById`;
        let option = {
            url: url,
            method: "POST",
            data: {
                Id: nodeItem.Id,
                NodeType: nodeItem.NodeType,
                TemplateIdPath: nodeItem.TemplateIdPath ?? ''
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let phyObj = JSON.parse(res);
            if (phyObj.NodeType == NodeType.PhysicalRootLocation) {
                this.setState({
                    allowExportBarcode: false
                });
            }
            this.setState({
                currentPhyObj: phyObj,
                currentPhyObjMetaData: this.getPhyObjMetadata(phyObj),
                allowNewChildren: this.isAllowNewChildren(phyObj),
            }, () => {
                this.getCurrentPhyObjNavBarBtnShow();
                this.dispatch('phyObjTable', 'reset');
                if (thenFunction) {
                    thenFunction();
                }
            });
        });
    }

    getPhyObjMetadata(phyObj) {
        let isLocation = phyObj.NodeType <= NodeType.PhysicalBottomLocation;
        if (isLocation) {
            return this.getLocationData(phyObj);
        } else {
            let columnTitleIdx = 0;
            let values = phyObj.MetaInfo;
            let templateCategoriesFromApi = JSON.parse(JSON.stringify(phyObj.Template.categories));
            let templateCategories = [];
            let categoryIndex = 0;
            for (let category of templateCategoriesFromApi) {
                if (categoryIndex == 0) {
                    let newCategory = [];
                    for (let column of category.columns) {
                        if (column.uniqueId == PhysicalDefaultColumnIDs.NameOrTitle
                            || column.uniqueId == PhysicalDefaultColumnIDs.Status
                            || column.uniqueId == PhysicalDefaultColumnIDs.Capability
                            || column.uniqueId == PhysicalDefaultColumnIDs.Classification
                        ) {
                            newCategory.push(column);
                        }
                    }
                    category.columns = newCategory;
                    categoryIndex++;
                }
                if (this.state.viewMode == TreeViewMode.Term && category.name == "RM_Template_Cagegory_Name_Statement") {
                    let tempCategoryColumns = [];
                    for (let column of category.columns) {
                        if (column.uniqueId != PhysicalDefaultColumnIDs.HomeLocation) {
                            tempCategoryColumns.push(column);
                        }
                    }
                    category.columns = tempCategoryColumns;
                }
                templateCategories.push(category);
            }
            for (const category of templateCategories) {
                for (const index in category.columns) {
                    if (category.columns.hasOwnProperty(index)) {
                        let currentColumn = category.columns[index];
                        let options = JSON.parse(currentColumn.optionsJSON);
                        currentColumn.columnValue = PhyColumnUtil.getDisplayValue(currentColumn, values);
                        if (currentColumn.typeId == PhysicalObjectColumnType.SingleChoice) {
                            let isDeleted = true;
                            for (let key in options) {
                                if (values[currentColumn.uniqueId]) {
                                    if (key == JSON.parse(values[currentColumn.uniqueId]).Value) {
                                        isDeleted = false;
                                        break;
                                    }
                                }
                            }
                            currentColumn.isDeleted = isDeleted;
                            if (!isDeleted) {
                                let oldColumnValue = JSON.parse(values[currentColumn.uniqueId]);
                                currentColumn.columnValue = options[oldColumnValue.Value];
                            }
                        }
                        if (currentColumn.typeId == PhysicalObjectColumnType.MultipleChoice) {
                            let isDeleted = false;
                            let mulChoiceOptValueArr = [];
                            let newMulChoiceArr = [];
                            for (let key in options) {
                                if (options.hasOwnProperty(key)) {
                                    mulChoiceOptValueArr.push(key);
                                }
                            }
                            if (currentColumn.columnValue) {
                                for (let opt of JSON.parse(values[currentColumn.uniqueId])) {
                                    if (mulChoiceOptValueArr.indexOf(opt.Value) == -1) {
                                        isDeleted = true;
                                        opt.showWavyLine = true;
                                    }
                                    newMulChoiceArr.push(opt);
                                }
                            }
                            currentColumn.isDeleted = isDeleted;
                            if (!isDeleted) {
                                if (values[currentColumn.uniqueId]) {
                                    let newMulColumnValue = [];
                                    let oldColumnValue = JSON.parse(values[currentColumn.uniqueId]);
                                    oldColumnValue.filter((item) => {
                                        newMulColumnValue.push(options[item.Value]);
                                    });
                                    currentColumn.columnValue = newMulColumnValue.join("; ");
                                }
                            } else {
                                newMulChoiceArr.forEach((item) => {
                                    for (let key in options) {
                                        if (key == item.Value) {
                                            item.Name = options[key];
                                        }
                                    }
                                });
                                currentColumn.columnValue = newMulChoiceArr;
                            }
                        }
                        if (category.columns[index].uniqueId == PhysicalDefaultColumnIDs.NameOrTitle) {
                            columnTitleIdx = index;
                        }
                    }
                }
            }
            //增加UniqueId column
            templateCategories[0].columns.splice(columnTitleIdx * 1 + 1, 0, {
                columnName: RMResx.RM_PRM_PRE_Column_ID,
                columnValue: phyObj.UniqueId
            });
            let basicInfoColAttrsNotInMateInfo = this.boxBasicInfoColAttrsNotInMate.slice(0);
            let basicInfoColNamesNotInMateInfo = this.boxBasicInfoColNamesNotInMate.slice(0);
            if (phyObj.NodeType == NodeType.PhyBox) {
                basicInfoColAttrsNotInMateInfo = this.fileBasicInfoColAttrsNotInMate.slice(0);
                basicInfoColNamesNotInMateInfo = this.fileBasicInfoColNamesNotInMate.slice(0);
            }
            if (phyObj.NodeType == NodeType.PhyFile) {
                basicInfoColAttrsNotInMateInfo = this.fileBasicInfoColAttrsNotInMate.slice(0);
                basicInfoColNamesNotInMateInfo = this.fileBasicInfoColNamesNotInMate.slice(0);
            }
            if (phyObj.NodeType == NodeType.PhyCustom) {
                basicInfoColAttrsNotInMateInfo = this.containerBasicInfoColAttrsNotInMate.slice(0);
                basicInfoColNamesNotInMateInfo = this.containerBasicInfoColNamesNotInMate.slice(0);
            }
            let categoryIndex1 = 0;
            for (const category of templateCategories) {
                if (categoryIndex1 == 0) {
                    if (!phyObj.DisposalHold) {
                        basicInfoColAttrsNotInMateInfo = basicInfoColAttrsNotInMateInfo.filter((value) => {
                            return value != 'HoldReleaseTime';
                        });
                        basicInfoColNamesNotInMateInfo = basicInfoColNamesNotInMateInfo.filter((value) => {
                            return value != RMResx.RM_PRM_PRE_Column_HoldUntil;
                        });
                    }
                    basicInfoColAttrsNotInMateInfo = basicInfoColAttrsNotInMateInfo.filter((value) => {
                        return value != undefined;
                    });
                    basicInfoColNamesNotInMateInfo = basicInfoColNamesNotInMateInfo.filter((value) => {
                        return value != undefined;
                    });
                    for (let key in basicInfoColAttrsNotInMateInfo) {
                        if (basicInfoColAttrsNotInMateInfo.hasOwnProperty(key)) {
                            let attr = basicInfoColAttrsNotInMateInfo[key];
                            let column = {};
                            switch (attr) {
                                case 'CreateTime':
                                case 'ModifiedTime':
                                case 'HoldReleaseTime':
                                    column.columnValue = phyObj[attr] > 0 ? phyObj[attr + "Str"] : '';
                                    break;
                                case 'PersonHold':
                                case 'DisposalHold':
                                    column.columnValue = phyObj[attr] ? YesOrNo[0] : YesOrNo[1];
                                    break;
                                case 'PersonHoldBy':
                                case 'HoldBy':
                                    column.columnValue = phyObj[attr] || RMResx.RM_JS_PRM_PRE_UserIsNull;
                                    break;
                                case 'HoldProfileTitle' :
                                    column.columnValue = phyObj[attr] || RMResx.RM_JS_PRM_PRE_UserIsNull;
                                    break;
                                default:
                                    column.columnValue = phyObj[attr];
                            }
                            column.columnName = basicInfoColNamesNotInMateInfo[key];
                            category.columns.push(column);
                        }
                    }
                    categoryIndex1++;
                }
            }
            return templateCategories;
        }
    }

    getLocationData(phyObj) {
        let metaInfo = phyObj.MetaInfo;
        let columns = [];
        let fullLocationObj = {};
        for (let key in metaInfo) {
            if (metaInfo.hasOwnProperty(key)) {
                let column = {};
                if (key != PhysicalDefaultColumnIDs.Path) {
                    column.columnName = key == PhysicalDefaultColumnIDs.Capability ? RMResx.RM_PRM_PRE_Column_TotalSpace : PhysicalDefaultColumnNames[key];
                    column.columnValue = metaInfo[key];
                    columns.push(column);
                } else {
                    fullLocationObj.columnName = PhysicalDefaultColumnNames[key];
                    fullLocationObj.columnValue = metaInfo[key];
                }
            }
        }

        let basicInfoColAttrsNotInMateInfo = this.locationBasicInfoColAttrsNotInMate;
        let basicInfoColNamesNotInMateInfo = this.locationBasicInfoColNamesNotInMate;
        for (let key in basicInfoColAttrsNotInMateInfo) {
            if (basicInfoColAttrsNotInMateInfo.hasOwnProperty(key)) {
                let attr = basicInfoColAttrsNotInMateInfo[key];
                let column = {};
                switch (attr) {
                    case 'CreateTime':
                    case 'ModifiedTime':
                        column.columnValue = phyObj[attr] > 0 ? phyObj[attr + "Str"] : '';
                        break;
                    case 'HoldType':
                        column.columnValue = PhysicalDefaultColumnHoldTypeNames[phyObj[attr]];
                        break;
                    default:
                        column.columnValue = phyObj[attr];
                }
                column.columnName = basicInfoColNamesNotInMateInfo[key];
                columns.push(column);
            }
        }
        columns.push(fullLocationObj);
        let categories = [{
            name: RMResx.RM_Template_Cagegory_Name_Basic,
            columns: columns
        }];
        return categories;
    }

    getFormPanelTitle(formType, nodeType) {
        let panelTitle = '';
        if (formType == PhyObjFormType.NewRequest) {
            switch (nodeType) {
                case NodeType.PhyBox:
                    panelTitle = RMResx.RM_PRM_PRE_NewBoxRequest; //"New Box Request";
                    break;
                case NodeType.PhyFile:
                    panelTitle = RMResx.RM_PRM_PRE_NewFolderRequest; //"New Folder Request";
                    break;
                case NodeType.PhyRecord:
                    panelTitle = RMResx.RM_PRM_PRE_NewRecordRequest; //"New Record Request";
                    break;
            }
            //panelTitle = RMResx.RM_PRM_PRE_NewRequest;
        } else {
            let isEdit = formType == PhyObjFormType.EditPhyObj;
            switch (nodeType) {
                case NodeType.PhyBox:
                    panelTitle = isEdit ? RMResx.RM_PRM_PRE_PanelTitle_EditBox : RMResx.RM_PRM_PRE_PanelTitle_NewBox;
                    break;
                case NodeType.PhyFile:
                    panelTitle = isEdit ? RMResx.RM_PRM_PRE_PanelTitle_EditFile : RMResx.RM_PRM_PRE_PanelTitle_NewFile;
                    break;
                case NodeType.PhyRecord:
                    panelTitle = isEdit ? RMResx.RM_PRM_PRE_PanelTitle_EditRecord : RMResx.RM_PRM_PRE_PanelTitle_NewRecord;
                    break;
                case NodeType.PhyCustom:
                    panelTitle = isEdit ? RMResx.RM_PRM_PRE_PanelTitle_EditContainer : RMResx.RM_PRM_PRE_PanelTitle_NewContainer;
                    break;
            }
        }
        return panelTitle;
    }

    isAllowLoan(items) {
        let allowLoan = true;
        for (const item of items) {
            if ((item.NodeType != NodeType.PhyFile && item.NodeType != NodeType.PhyBox)
                || item.Status == PhysicalObjectStatus.Destroyed
                || item.Status == PhysicalObjectStatus.Missing
                // || item.PersonHold
            ) {
                allowLoan = false;
                break;
            }
        }
        return allowLoan;
    }

    isAllowEdit(items) {
        let allow = true;
        if (items.length != 1) {
            items.forEach((e, index) => {
                let sameTemplateId = items.find(t => t.TemplateId != e.TemplateId);
                if (sameTemplateId || e.Status == PhysicalObjectStatus.Destroyed) {
                    allow = false;
                }
            });
        } else {
            if (items[0].Status == PhysicalObjectStatus.Destroyed) {
                allow = false;
            }
        }
        return allow;
    }

    isAllowDelete(items) {
        let allow = true;
        for (const item of items) {
            if (item.DisposalHold || item.PersonHold) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowMove(items) {
        let allow = true;
        for (const item of items) {
            if (item.Ancestors && item.Ancestors.length > 0) {
                if (item.NodeType == NodeType.PhyBox && item.ParentId != item.LocationId) {
                    //container下的box
                    allow = false;
                    break;
                }

                if (item.NodeType == NodeType.PhyFile) {
                    if (!(item.ParentId == item.LocationId || item.Ancestors[1] == item.BoxId)) {
                        //container下的folder
                        allow = false;
                        break;
                    }
                }

                if (item.NodeType == NodeType.PhyRecord) {
                    if (!(item.Ancestors[1] == item.BoxId || item.Ancestors[1] == item.FileId)) {
                        //container下的record
                        allow = false;
                        break;
                    }
                }
            }

            if ([NodeType.PhyRecord, NodeType.PhyCustom].includes(item.NodeType)) {
                if (!(LicenseHelper.EnableRecordsArchiver() && item.NodeType == NodeType.PhyRecord)) { 
                    allow = false;
                    break;
                }
            }
            if (item.Status == PhysicalObjectStatus.Destroyed
                || item.Status == PhysicalObjectStatus.Missing
                || item.PersonHold) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowMoveRequest(items, isFromTree) {
        let allow = items.length > 0;

        const role = Number(RM.RoleType);
        if (role !== RoleType.StandardUser && role !== RoleType.StandardReviewUser) {
            return false;
        }
        
        if (!checkPermission("PRM_MoveRequest", RM.UserResources)) {
            return false;
        }

        for (const item of items) {
            if (isFromTree) {
                if (item.NodeType !== NodeType.PhyFile && item.NodeType !== NodeType.PhyBox) {
                    allow = false;
                    break;
                }
            } else {
                if (item.NodeType !== NodeType.PhyFile && item.NodeType !== NodeType.PhyBox && item.NodeType !== NodeType.PhyRecord) {
                    allow = false;
                    break;
                }
            }
            
            if (item.Status === PhysicalObjectStatus.Destroyed || item.Status === PhysicalObjectStatus.Missing || item.PersonHold) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowReclassify(items) {
        let allow = true;
        for (const item of items) {
            if ([NodeType.PhyRecord, NodeType.PhyCustom].includes(item.NodeType)) {
                allow = false;
                break;
            }
            if (item.Status == PhysicalObjectStatus.Destroyed
                || item.Status == PhysicalObjectStatus.Missing
            ) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowRemovePersonHold(items) {
        if (!this.isRMAdmin && !checkPermission("PRM_FolderLoanReturn", RM.UserResources)) {
            return false;
        }
        let allow = items.length > 0;
        let phyFileItems = [];
        for (const item of items) {
            if ([NodeType.PhyRecord, NodeType.PhyCustom].includes(item.NodeType)) {
                allow = false;
            }
            if (item.NodeType == NodeType.PhyFile || item.NodeType == NodeType.PhyBox) {
                phyFileItems.push(item);
            }
        }

        if (phyFileItems.length == 0) {
            allow = false;
        } else {
            for (let phyFileItem of phyFileItems) {
                if (!phyFileItem.PersonHold) {
                    allow = false;
                }
            }
        }
        return allow;
    }

    isAllowRelated(items) {
        let allow = true;
        if (items.length != 1) {
            allow = false;
        }
        for (let item of items) {
            if ((item.NodeType != NodeType.PhyFile && item.NodeType != NodeType.PhyRecord) || item.Status == PhysicalObjectStatus.Destroyed
                || item.Status == PhysicalObjectStatus.Missing) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowSetPermissions(items) {
        let allow = true;
        //多选判断,如果ScopePermissionId不同，说明有item打破了继承，不显示按钮。
        if (items.length > 1) {
            let firstItem = items[0];
            let diffItem = items.filter((item, idx) => {
                return item.ScopePermissionId != firstItem.ScopePermissionId;
            });
            if (diffItem.length > 0) {
                allow = false;
            }
        }
        for (let item of items) {
            if (!(item.NodeType == NodeType.PhyBox ||
                item.NodeType == NodeType.PhyFile ||
                item.NodeType == NodeType.PhysicalBottomLocation ||
                item.NodeType == NodeType.PhysicalNormalLocation ||
                item.NodeType == NodeType.PhyCustom)) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowMore(items) {
        let allow = false;
        let tableNavBarBtnItemsInMore = [];
        //delete button 显示时一定在 More 中；
        if (items['delete']) {
            tableNavBarBtnItemsInMore.push(TableNavBarBtnInMore['delete']);
            allow = true;
        }
        if (tableNavBarBtnItemsInMore.length > 0) {
            this.setState({
                tableNavBarBtnItemsInMore: tableNavBarBtnItemsInMore,
            });
        }
        return allow;
    }

    isAllowShowAudit(items){
        let allow = true;
        if(this.state.isPhysicalEndUser){
            allow = false;
        }
        if(items.length != 1){
            allow = false;
        }
        for(let item of items){
            if(item.NodeType !== NodeType.PhyRecord && item.NodeType !== NodeType.PhyBox && item.NodeType !== NodeType.PhyFile){
                allow = false;
            }
        }
        return allow;
    } 

    isAllowNewChildren(phyObj) {
        let allowNew = true;
        if (this.tableSelectItems.length != 0) {
            return false;
        }
        if (!this.state.hasTermSettings) {
            allowNew = false;
        } else if (phyObj.NodeType > NodeType.PhysicalBottomLocation && phyObj.NodeType != NodeType.PhyCustom) {
            let status = JSON.parse(phyObj.MetaInfo[PhysicalDefaultColumnIDs.Status]).Value;
            if (status == PhysicalObjectStatus.Destroyed || status == PhysicalObjectStatus.Missing || phyObj.PersonHold) {
                allowNew = false;
            }
        }
        return allowNew;
    }

    isAllowExportBarcode(phyObj, isHasTermSettings) {
        let allowExportBarcode = false;
        if (phyObj.NodeType > NodeType.PhysicalRootLocation && phyObj.NodeType < NodeType.PhyFile && phyObj.NodeType != NodeType.PhyCustom && isHasTermSettings) {
            allowExportBarcode = true;
        }
        return allowExportBarcode;
    }

    openHoldForm(selectedRecords, operate) {
        let selTreeItem = this.selectedTreeItem;
        let formData = {
            records: selectedRecords,
            treeNode: selTreeItem,
            formType: operate,
            Id: selTreeItem.Id,
        };
        let title = RMResx.RM_JS_BCM_Explorer_Button_PutOnHold;
        switch (operate) {
            case "new":
                title = RMResx.RM_JS_BCM_Explorer_Button_PutOnHold;
                break;
            case "change":
                title = RMResx.RM_JS_BCM_Explorer_Button_ChangeHold;
                break;
            case "extend":
                title = RMResx.RM_JS_BCM_Explorer_Button_SuspendHold;
                break;
            case "append":
                title = RMResx.RM_JS_BCM_Explorer_Button_AppendHold;
                break;
        }
        this.setState({
            showHoldPanel: { show: true },
            formPanelTitle: title,
            phyObjFormData: formData
        });
    }

    // PhyObjFormType: formType
    openForm(formType, nodeType, nodeId, templateId) {
        if (formType == PhyObjFormType.CreatePhyObj) {
            // let templateId = this.nodeTypeAndTemplateIdMapping[nodeType];
        }
        this.operateFormNodeType = nodeType;
        let selTreeItem = this.selectedTreeItem;
        let selTreeItemType = selTreeItem.NodeType;
        let formHomeLocationFullPath = this.state.currentPhyObj.HomeLocationFullPath;
        if (formHomeLocationFullPath) {
            formHomeLocationFullPath += "/" + selTreeItem.Name;
        }
        let parentId = selTreeItem.Id;
        if([NodeType.PhysicalBottomLocation].includes(selTreeItem.NodeType))
        {
            parentId = selTreeItem.LocationId;
        }
        let formData = {
            formType: formType,
            NodeType: nodeType,
            ParentNodeType: selTreeItemType,
            ParentId: parentId,
            Id: nodeId ?? EmptyGUID,
            LocationId: selTreeItem.LocationId,
            LocationName: selTreeItem.LocationName,
            BoxId: (selTreeItemType == NodeType.PhyBox ? selTreeItem.Id : selTreeItem.BoxId) ?? EmptyGUID,
            FileId: (selTreeItemType == NodeType.PhyFile ? selTreeItem.Id : selTreeItem.FileId) ?? EmptyGUID,
            TemplateId: templateId,
            HomeLocationFullPath: formHomeLocationFullPath
        };

        this.setState({
            showFormPanel: { show: true },
            formPanelTitle: this.getFormPanelTitle(formType, nodeType),
            phyObjFormData: formData
        });
    }

    openBulkUpdateForm(formType, nodeType, selectItems) {
        let selectTemplateId = "";
        let selectNodeId = [];
        selectItems.forEach(item => {
            selectTemplateId = item.TemplateId;
            selectNodeId.push(item.Id);
        });
        let bulkUpdateFormData = {
            formType: formType,
            NodeType: nodeType,
            RecordIds: selectNodeId,
            TemplateId: selectTemplateId,
        }
        this.setState({
            showBulkUpdateFormPanel: { show: true },
            formPanelTitle: this.getFormPanelTitle(formType, nodeType),
            phyObjBulkUpdateData: bulkUpdateFormData
        });
    }

    setPanelHeader(templateName) {
        this.setState({
            formTemplateName: templateName,
        });
    }

    isLocationNodeType(nodeType) {
        return nodeType === NodeType.PhysicalRootLocation
            || nodeType === NodeType.PhysicalNormalLocation
            || nodeType === NodeType.PhysicalBottomLocation;
    }

    getEffectivePermissionLocationId(nodeItem) {
        if (!nodeItem) {
            return "";
        }
        if (nodeItem.LocationId && nodeItem.LocationId !== EmptyGUID) {
            return nodeItem.LocationId;
        }
        return nodeItem.Id || "";
    }

    isDelegatedAdminRole() {
        return this.isRoleDelegatedAdmin && this.hasManageHold;
    }

    isAdminRole() {
        return this.isRoleAdmin;
    }

    isHoldOnlyModeByLocationPermission() {
        return this.isDelegatedAdminRole() && this.state.allowFullActionsByEffectiveLocation === false;
    }

    parseEffectiveLocationPermission(result, locationId) {
        let data = result;

        if (typeof data === "string") {
            if (data.toLowerCase() === "true") {
                return { allowFull: true, allowHold: true };
            }
            if (data.toLowerCase() === "false") {
                return { allowFull: false, allowHold: false };
            }
            try {
                data = JSON.parse(data);
            } catch (error) {
                return { allowFull: false, allowHold: false };
            }
        }

        if (typeof data === "boolean") {
            return { allowFull: data, allowHold: data };
        }

        if (Array.isArray(data)) {
            this.effectiveLocationPermissionMap = {};
            this.effectiveLocationHoldPermissionMap = {};

            data.forEach((item) => {
                if (item && item.LocationId) {
                    const key = item.LocationId.toLowerCase();
                    const allowFull = item.IsPhysicalAdmin === true;
                    const allowHold = allowFull || item.IsHoldManager === true;

                    this.effectiveLocationPermissionMap[key] = allowFull;
                    this.effectiveLocationHoldPermissionMap[key] = allowHold;
                }
            });

            const currentLocationId = (locationId || "").toLowerCase();
            if (!currentLocationId) {
                return { allowFull: false, allowHold: false };
            }

            return {
                allowFull: this.effectiveLocationPermissionMap[currentLocationId] === true,
                allowHold: this.effectiveLocationHoldPermissionMap[currentLocationId] === true
            };
        }

        if (data && typeof data === "object") {
            const allowFull = data.IsPhysicalAdmin === true;
            const allowHold = allowFull || data.IsHoldManager === true;
            return { allowFull, allowHold };
        }

        return { allowFull: false, allowHold: false };
    }

    applyLocationPermissionForCurrentSelection() {
        if (!this.isDelegatedAdminRole()) {
            return;
        }

        let allowFull = true;
        let allowHold = true;

        if (this.tableSelectItems && this.tableSelectItems.length > 0) {
            for (let i = 0; i < this.tableSelectItems.length; i++) {
                const item = this.tableSelectItems[i];
                const id = this.getEffectivePermissionLocationId(item).toLowerCase();

                if (!this.effectiveLocationPermissionMap[id]) {
                    allowFull = false;
                }
                if (!this.effectiveLocationHoldPermissionMap[id]) {
                    allowHold = false;
                }
            }
        } else if (this.selectedTreeItem) {
            const nodeLocationId = this.getEffectivePermissionLocationId(this.selectedTreeItem).toLowerCase();
            allowFull = this.effectiveLocationPermissionMap[nodeLocationId] === true;
            allowHold = this.effectiveLocationHoldPermissionMap[nodeLocationId] === true;
        }

        this.setState({
            allowHoldActionsByEffectiveLocation: allowHold,
            allowFullActionsByEffectiveLocation: allowFull
        }, () => {
            this.refreshHoldActionsByEffectiveLocationPermission();
        });
    }

    loadEffectiveLocationPermissionForCurrentPage(pageItems, pageIndex, pageSize) {
        if (!this.isDelegatedAdminRole()) {
            return;
        }

        const nodeId = this.selectedTreeItem ? this.selectedTreeItem.Id : "";
        const key = `${nodeId}_${pageIndex}_${pageSize}`;
        if (this.effectiveLocationPermissionPageKey === key) {
            return;
        }
        this.effectiveLocationPermissionPageKey = key;

        this.loadEffectiveLocationPermission(this.selectedTreeItem);
    }

    refreshHoldActionsByEffectiveLocationPermission() {
        if (this.state.currentPhyObj) {
            this.updateTopButtons(this.state.currentPhyObj);
        }
        this.updateTopButtonsForTable();
    }

    loadEffectiveLocationPermission(nodeItem) {
        if (!nodeItem) {
            return;
        }

        // RoleType=1 Admin: skip API and keep original full-actions logic
        if (this.isAdminRole()) {
            this.setState({
                allowHoldActionsByEffectiveLocation: true,
                allowFullActionsByEffectiveLocation: true
            }, () => {
                this.refreshHoldActionsByEffectiveLocationPermission();
            });
            return;
        }

        // RoleType=5 Hold manager only: skip API and force hold-only view
        if (this.isHoldManagerOnly) {
            this.setState({
                allowHoldActionsByEffectiveLocation: true,
                allowFullActionsByEffectiveLocation: false
            }, () => {
                this.refreshHoldActionsByEffectiveLocationPermission();
            });
            return;
        }

        // Only RoleType=2 needs to call this API
        if (!this.isDelegatedAdminRole()) {
            this.setState({
                allowHoldActionsByEffectiveLocation: true,
                allowFullActionsByEffectiveLocation: true
            }, () => {
                this.refreshHoldActionsByEffectiveLocationPermission();
            });
            return;
        }

        const locationId = this.getEffectivePermissionLocationId(nodeItem);
        if (!locationId) {
            this.setState({
                allowHoldActionsByEffectiveLocation: true,
                allowFullActionsByEffectiveLocation: false
            }, () => {
                this.refreshHoldActionsByEffectiveLocationPermission();
            });
            return;
        }

        const option = {
            url: "/api/PhysicalRecordApi/GetEffectiveLocationPermissions",
            method: "GET"
        };

        fetchUtility(option).then((result) => {
            const permission = this.parseEffectiveLocationPermission(result, locationId);
            this.setState({
                allowHoldActionsByEffectiveLocation: permission.allowHold,
                allowFullActionsByEffectiveLocation: permission.allowFull
            }, () => {
                this.applyLocationPermissionForCurrentSelection();
            });
        }).catch(() => {
            this.setState({
                allowHoldActionsByEffectiveLocation: false,
                allowFullActionsByEffectiveLocation: false
            }, () => {
                this.refreshHoldActionsByEffectiveLocationPermission();
            });
        });
    }

    onSelectedNodeChanged(nodeItem, afterRefresh) {
        this.selectedTreeItem = nodeItem;
        this.effectiveLocationPermissionPageKey = "";
        this.loadEffectiveLocationPermission(nodeItem);
        if (this.refTableSearchBox) {
            this.refTableSearchBox.clear();
        }
        let callback = () => {
            this.resetFilter();
            this.getPhysicalObjectList(nodeItem, true);
        };
        if (!afterRefresh) {
            this.validSettings(nodeItem, callback);
        } else {
            this.initCurrentPhysicalObjectInfo(nodeItem, true, callback);
        }
        if (!this.isRMAdmin) {
            this.getIsStandardUserHasPermission(nodeItem);
        }
        this.dispatch('PhyObjectInfo', 'reset', this.selectedTreeItem);
    }

    onTermViewSelectedNodeChanged(nodeItem) {
        if (nodeItem.NodeType == NodeType.PhyBox || nodeItem.NodeType == NodeType.PhyFile) {
            this.selectedTreeItem = nodeItem;
            this.effectiveLocationPermissionPageKey = "";
            this.loadEffectiveLocationPermission(nodeItem);
            if (this.refTableSearchBox) {
                this.refTableSearchBox.clear();
            }
            let callback = () => {
                this.resetFilter();
                this.getPhysicalObjectList(nodeItem, true);
                this.setState({
                    hasTermSettings: true
                });
                if (!this.isRMAdmin) {
                    this.getIsStandardUserHasPermission(nodeItem);
                }
                this.dispatch('PhyObjectInfo', 'reset', this.selectedTreeItem);
            };
            this.initCurrentPhysicalObjectInfo(nodeItem, false, callback);

        } else {
            this.resetTermTreeView();
        }
    }

    resetTermTreeView() {
        this.setState({
            currentPhyObj: null,
            currentPhyObjMetaData: [],
            allowNewChildren: false,
        }, () => {
            this.dispatch('phyObjTable', 'reset');
        });
    }

    //获取ParentIdList，到BottomLocation
    getParentIdsByNode(pNode) {
        if (this.state.viewMode == TreeViewMode.Term) {
            return pNode.Ancestors;
        }
        if (this.state.viewMode == TreeViewMode.Location) {
            if (this.refExplorerTree) {
                let nodes = this.refExplorerTree.getTreeCacheNodes();
                let parentIds = [];
                this.getParentId(pNode, parentIds, nodes);
                parentIds.reverse();
                return parentIds;
            }
        }
    }

    getParentId(node, parentIds, nodes) {
        if (node && node.NodeType >= NodeType.PhysicalBottomLocation) {
            let parentId = node.Id;
            if (parentId && parentId != EmptyGUID) {
                parentIds.push(node.NodeType == NodeType.PhysicalBottomLocation ? node.LocationId : parentId);
                let parentNode = nodes.find(o => o.Id == node.ParentId);
                this.getParentId(parentNode, parentIds, nodes);
            }
        }
    }

    onSavePhyObj() {
        $$.loading(true);
        let parentIdList = this.getParentIdsByNode(this.selectedTreeItem);
        let callback = (success, data) => {
            if (success) {
                if (data.formType == PhyObjFormType.CreatePhyObj) {
                    showToast.success(RMResx.RM_PRM_PRE_Msg_NewItemSuccess);
                    if (this.state.viewMode == TreeViewMode.Term) {
                        // this.refTermTree.refreshSelectedNode();
                    } else if (this.state.viewMode == TreeViewMode.Location) {
                        this.refExplorerTree.refreshSelectedNode();
                    }
                } else if (data.formType == PhyObjFormType.EditPhyObj) {
                    showToast.success(RMResx.RM_PRM_PRE_Msg_EditItemSuccess);
                    if (data.Id == this.selectedTreeItem.Id) {
                        let updateProps = {
                            Name: data.Name,
                            BreakInheritance: this.selectedTreeItem.BreakInheritance,
                            OnLoan: data.PersonHold,
                        };
                        let statusInfo = data.MetaInfo[PhysicalDefaultColumnIDs.Status];
                        if (statusInfo) {
                            statusInfo = JSON.parse(statusInfo);
                            if (statusInfo) {
                                updateProps.RecordStatus = statusInfo.Value;
                            }
                        }
                        if (this.state.viewMode == TreeViewMode.Term) {
                            this.refTermTree.refreshSelectedNode(updateProps);
                        } else if (this.state.viewMode == TreeViewMode.Location) {
                            this.refExplorerTree.refreshSelectedNode(updateProps);
                        }
                    } else if (data.NodeType < NodeType.PhyRecord) {
                        if (this.state.viewMode == TreeViewMode.Term) {
                            this.refTermTree.refreshSelectedNode();
                        } else if (this.state.viewMode == TreeViewMode.Location) {
                            this.refExplorerTree.refreshSelectedNode();
                        }
                    }
                } else if (data.formType == PhyObjFormType.NewRequest) {
                    showToast.success(RMResx.RM_PRM_PRE_Msg_NewRequesSuccess);
                }

                if (data.Id == this.selectedTreeItem.Id) {
                    this.initCurrentPhysicalObjectInfo(this.selectedTreeItem);
                }
                if (data.formType != PhyObjFormType.NewRequest) {
                    this.getPhysicalObjectList(this.selectedTreeItem, true);
                }
                this.setState({ showFormPanel: { show: false } });
            }
            $$.loading(false);
        };
        this.dispatch(this.peFormId, 'onSave', callback, parentIdList);
        return false;
    }

    onSaveBulkUpdatePhyObj() {
        let parentIdList = this.getParentIdsByNode(this.selectedTreeItem);
        let callback = (success, data) => {
            $$.loading(true);
            if (success) {
                if (data.formType == PhyObjFormType.EditPhyObj) {
                    showToast.success(RMResx.RM_PRM_PRE_Msg_EditItemSuccess);
                    if (data.NodeType < NodeType.PhyRecord) {
                        if (this.state.viewMode == TreeViewMode.Term) {
                            this.refTermTree.refreshSelectedNode();
                        } else if (this.state.viewMode == TreeViewMode.Location) {
                            this.refExplorerTree.refreshSelectedNode();
                        }
                    }
                }

                if (data.formType != PhyObjFormType.NewRequest) {
                    this.getPhysicalObjectList(this.selectedTreeItem, true);
                }
                this.setState({ showBulkUpdateFormPanel: { show: false } });
            }
            $$.loading(false);
        };
        this.dispatch(this.peBulkUpdateFormId, 'onSave', callback, parentIdList);
        return false;
    }

    onSaveLoanObj() {
        let callback = (loanData, errorCallBack) => {
            let validateFailed = false;
            if (!validateFailed && (loanData.OnBehalf == null || loanData.OnBehalf.length == 0)) {
                validateFailed = true;
            }

            if (validateFailed) {
                return false;
            }
            $$.loading(true);
            let url = `/api/PhysicalRequestApi/LoanRequest`;
            let option = {
                url: url,
                method: "POST",
                data: loanData
            };
            fetchUtility(option).then((result) => {
                if (result.HasError) {
                    errorCallBack(result, () => this.setState({ showLoanPanel: { show: false } }));
                } else {
                    showToast.success(RMResx.RM_JS_RDM_Explorer_LoanRequestSuccessMsg);
                    this.setState({
                        showLoanPanel: { show: false }
                    });
                }
                $$.loading(false);
            }).catch((e) => {

            });

            addTelemetryRecord(TelemetryModule.PhysicalRecordsExplorer, TelemetryEventType.LoanRequest);
        };
        this.dispatch(this.peLoanId, 'onSave', callback);
        return false;
    }

    onSaveHold() {
        this.dispatch(this.peHoldId, 'onSavePhyHold', (success, data) => {
            $$.loading(false);
            if (success) {
                if (data.formType == "new") {
                    showToast.success(RMResx.RM_JS_RDM_Explorer_PutOnHoldSuccessMsg);
                } else if (data.formType == "extend") {
                    showToast.success(RMResx.RM_JS_RDM_Explorer_ExtendHoldSuccessMsg);
                } else if (data.formType == "change") {
                    showToast.success(RMResx.RM_JS_RDM_Explorer_ChangeHoldSuccessMsg);
                } else if (data.formType == "append") {
                    showToast.success(RMResx.RM_JS_RDM_Explorer_AppendHoldSuccessMsg);
                }

                if (data.Id == this.selectedTreeItem.Id) {
                    this.initCurrentPhysicalObjectInfo(this.selectedTreeItem);
                }
                this.getPhysicalObjectList(this.selectedTreeItem, true);
                this.setState({ showHoldPanel: { show: false } });
            }
        });
        return false;
    }

    onCloseViewDetail() {
        this.setState({
            showViewDetailPanel: { show: false }
        });
    }

    onCreatePhyObj() {
        let operateFormNodeType = null;
        switch (this.selectedTreeItem.NodeType) {
            case NodeType.PhyBox:
                operateFormNodeType = NodeType.PhyFile;
                break;
            case NodeType.PhyFile:
                operateFormNodeType = NodeType.PhyRecord;
                break;
            default:
                return;
        }
        this.openForm(PhyObjFormType.CreatePhyObj, operateFormNodeType);
    }

    onCreateSuite(item) {
        let operateFormNodeType = null;
        switch (item.Type) {
            case TemplateTypes.CustomTemplate:
                operateFormNodeType = NodeType.PhyCustom;
                break;
            case TemplateTypes.Box:
                operateFormNodeType = NodeType.PhyBox;
                break;
            case TemplateTypes.Folder:
                operateFormNodeType = NodeType.PhyFile;
                break;
            case TemplateTypes.Record:
                operateFormNodeType = NodeType.PhyRecord;
                break;
        }

        let option = {
            method: "GET",
            url: "/api/PhysicalRecordApi/ValidHasUniqueIdSettings/?templateId=" + item.UniqueId
        };
        fetchUtility(option).then((res) => {
            if (res == 0) {
                this.openForm(PhyObjFormType.CreatePhyObj, operateFormNodeType, null, item.UniqueId);
            } else if (res == 1) {
                let linkName = RMResx.RM_EditTemplate_BoxPageTitle;
                switch (operateFormNodeType) {
                    case NodeType.PhyCustom:
                        linkName = RMResx.RM_PRM_PRE_PanelTitle_EditContainer;
                        break;
                    case NodeType.PhyBox:
                        linkName = RMResx.RM_EditTemplate_BoxPageTitle;
                        break;
                    case NodeType.PhyFile:
                        linkName = RMResx.RM_EditTemplate_FilePageTitle;
                        break;
                    case NodeType.PhyRecord:
                        linkName = RMResx.RM_EditTemplate_RecordPageTitle;
                        break;
                    default:
                        break;
                }
                this.showMessageTip('warn',
                    <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Msg_NotifyConfigUniqueIdSetting}>
                        <a
                            className="ra-link-a ra-cursor-pointer"
                            style={{ color: "#0072d0" }}
                            onClick={() => this.routerTo(`${RouterUrls.PRM_TemplateManagement}`)}>
                            {linkName}
                        </a>
                    </$g.I18NProvider>
                );
                return;
            } else if (res == 2) {
                this.showMessageTip('warn',
                    <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Msg_NotifyConfigUniqueIdSetting}>
                        <a
                            className="ra-link-a ra-cursor-pointer"
                            style={{ color: "#0072d0" }}
                            onClick={() => this.routerTo(RouterUrls.PRM_TemplateManagement)}>
                            {RMResx.RM_EditTemplate_PhysicalUniqueIdSettingsTitle}
                        </a>
                    </$g.I18NProvider>
                );
            }
        });
    }

    onCreateBox() {
        this.openForm(PhyObjFormType.CreatePhyObj, NodeType.PhyBox);
    }

    onCreateFile() {
        this.openForm(PhyObjFormType.CreatePhyObj, NodeType.PhyFile);
    }

    onDeletePhyObj() {
        let currentPhyArr = [];
        currentPhyArr.push(this.state.currentPhyObj);
        this.validDelItemsHasChildren(true, currentPhyArr);
    }

    onEditPhyObj() {
        this.openForm(PhyObjFormType.EditPhyObj, this.selectedTreeItem.NodeType, this.selectedTreeItem.Id);
    }

    onBeforeEditTablePhyObj() {
        if (this.tableSelectItems.length > 15) {
            this.operateLimitDia();
            return;
        }
        
        if (this.tableSelectItems.length > 1) {
            this.onEditBulkUpdateTablePhyObj();
        } else {
            this.onEditTablePhyObj();
        }
    }
    
    onEditBulkUpdateTablePhyObj() {
        this.openBulkUpdateForm(PhyObjFormType.EditPhyObj, this.tableSelectItems[0].NodeType, this.tableSelectItems);
    }
    
    onEditTablePhyObj() {
        this.openForm(PhyObjFormType.EditPhyObj, this.tableSelectItems[0].NodeType, this.tableSelectItems[0].Id);
    }
    
    onReclassifyPhyObj() {
        let reclassifyData = {
            Source: [this.state.currentPhyObj],
            isTopButton: true
        };
        this.setState({
            showReclassifyPanel: { show: true },
            phyObjReclassifyParam: reclassifyData
        });
    }

    onReclassifyTablePhyObj() {
        if (this.tableSelectItems.length > 15) {
            this.operateLimitDia();
            return;
        }
        let reclassifyData = {
            Source: this.tableSelectItems,
            isTopButton: false
        };
        this.setState({
            showReclassifyPanel: { show: true },
            phyObjReclassifyParam: reclassifyData
        });
    }

    onPermitPhyObj() {
        this.setState({
            showManagePermissionPanel: true,
            isTopPermissionBtn: true,
            selectedPhyObjByTableOrTree: [this.state.currentPhyObj]
        });
    }

    getIsStandardUserHasPermission(nodeItem) {
        if (nodeItem.NodeType == NodeType.PhysicalBottomLocation) {
            let url = `/api/PhysicalRecordApi/CheckPerForScope?scopeId=${this.selectedTreeItem.Id}`;
            let option = {
                url: url,
                method: "GET"
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);
                this.setState({ isStandardUserHasPermission: res });
            });
        } else {
            this.setState({ isStandardUserHasPermission: true });
        }
    }

    onPermitTablePhyObj() {
        this.setState({
            showManagePermissionPanel: true,
            isTopPermissionBtn: false,
            selectedPhyObjByTableOrTree: this.tableSelectItems
        });
    }

    hideManagePermissionPanel() {
        this.setState({
            showManagePermissionPanel: false,
        });
    }

    onRelatedPhyObj() {
        this.setState({
            showRelatedRecordsPanel: { show: true },
            selectedPhyObjByTableOrTree: [this.state.currentPhyObj]
        });
    }

    onRelatedTablePhyObj() {
        this.setState({
            showRelatedRecordsPanel: { show: true },
            selectedPhyObjByTableOrTree: this.tableSelectItems
        });
    }

    onRemovePersonHoldConfirmPhyObj() {
        this.setState({
            removePersonHoldDialogShow: true,
            selectedPhyObjByTableOrTree: [this.state.currentPhyObj]
        });
    }

    onRemovePersonHoldConfirmTablePhyObj() {
        if (this.tableSelectItems.length > 15) {
            this.operateLimitDia();
            return;
        }
        this.setState({
            removePersonHoldDialogShow: true,
            selectedPhyObjByTableOrTree: this.tableSelectItems
        });
    }

    onRemovePersonHold() {
        $$.loading(true);
        let errorMsg = RMResx.RM_JS_RDM_PersonHold_CancelRecordError;
        let phyObjIDs = this.state.selectedPhyObjByTableOrTree.map((item) => item.Id);
        let option = {
            url: "/api/PhysicalRecordApi/RemovePersonalHold",
            method: "POST",
            data: phyObjIDs
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.success) {
                let updateProps = null;
                if (result.isStartJob && this.isRMAdmin) {
                    showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>);
                } else {
                    showToast.success(RMResx.RM_JS_RDM_Explorer_ReturnSuccessMsg);
                }
                if (phyObjIDs.length == 1 && phyObjIDs[0] == this.selectedTreeItem.Id) {
                    updateProps = {
                        Name: this.selectedTreeItem.Name,
                        BreakInheritance: this.selectedTreeItem.BreakInheritance,
                        OnLoan: result.isStartJob ? this.selectedTreeItem.OnLoan : this.selectedTreeItem.PersonHold,
                        RecordStatus: this.selectedTreeItem.RecordStatus
                    };
                    this.initCurrentPhysicalObjectInfo(this.selectedTreeItem);
                }
                if (this.state.viewMode == TreeViewMode.Term) {
                    this.refTermTree.refreshSelectedNode(updateProps);
                } else if (this.state.viewMode == TreeViewMode.Location) {
                    this.refExplorerTree.refreshSelectedNode(updateProps);
                }
                this.getPhysicalObjectList(this.selectedTreeItem, true);
            } else {
                showToast.error(result.message || errorMsg);
            }
            this.onCancelRemovePersonHold();
        }).catch((e) => {
            showToast.error(errorMsg);
            this.onCancelRemovePersonHold();
            $$.loading(false);
        });
    }

    onCancelRemovePersonHold() {
        this.setState({ removePersonHoldDialogShow: false });
    }

    updateNotificationTimer(jobId, type, isTopButton, args) {
        var timerCount = 0;
        var updateChangeTerm = setInterval(() => {
            ++timerCount;
            if (jobId) {
                let option = {
                    url: `/api/RecordsExplorerApi/GetRealTimeJobStatusInfo?jobId=${jobId}`,
                    method: "GET"
                };
                fetchUtility(option).then((result) => {
                    var msg = JSON.parse(result);
                    var stopTimer = false;
                    if (timerCount == (60 * 60) / 5) {
                        stopTimer = true;
                    }

                    //test
                    // if (timerCount == 5) {
                    //     stopTimer = true;
                    // }

                    if (msg.MessageType == 1 || msg.MessageType == 2) {// failed
                        stopTimer = true;
                        showToast.error(msg.ErrorMessage);
                    } else {
                        if (msg.Items) {
                            if (msg.Status == 4) {
                                stopTimer = true;
                                let message = '';
                                if (type == 'reclassify') {
                                    message = <$g.I18NProvider msg={RMResx.RM_PRM_Explorer_ReclassifySuccess}>
                                        {msg.Items.join(",")}
                                    </$g.I18NProvider>;
                                }
                                if (type == 'move') {
                                    message = <$g.I18NProvider msg={RMResx.RM_PRM_Explorer_MoveSuccess}>
                                        {msg.Items.join(",")}
                                    </$g.I18NProvider>;
                                }
                                showToast.success(message);
                            } else {
                                //self.showNotificationIcon(self.StatusEnum.InProgress, jobId);
                            }
                        }
                    }

                    //stop this timer
                    if (stopTimer) {
                        clearInterval(updateChangeTerm);

                        //refresh data, table & tree
                        if (type == 'reclassify') {
                            if (this.state.viewMode == TreeViewMode.Term) {
                                if (isTopButton) {
                                    this.refTermTree.refreshSelectedParentNode();
                                } else {
                                    this.getPhysicalObjectList(this.selectedTreeItem, true, true);
                                }
                            } else {
                                this.getPhysicalObjectList(this.selectedTreeItem, true, true);
                                this.initCurrentPhysicalObjectInfo(this.selectedTreeItem, true);
                            }
                        }

                        if (type == 'move') {
                            if (this.state.viewMode == TreeViewMode.Term) {
                                if (isTopButton) {
                                    this.initCurrentPhysicalObjectInfo(this.selectedTreeItem, true);
                                } else {
                                    this.refTermTree.refreshSelectedNode();
                                    this.getPhysicalObjectList(this.selectedTreeItem, true, true);
                                }
                            } else {
                                if (isTopButton) {
                                    this.refExplorerTree.refreshSelectedParentNode();
                                } else {
                                    this.refExplorerTree.refreshSelectedNode();
                                    this.getPhysicalObjectList(this.selectedTreeItem, true, true);
                                }
                                this.refExplorerTree.refreshMoveToNode(args);
                            }
                        }
                    }
                });
            }
        }, 1000);
    }


    tableNavBarBtnSelectInMore(item) {
        switch (item.id) {
            case TableNavBarBtnInMore.delete.id:
                this.onDeleteTablePhyObj();
                break;
            // case TableNavBarBtnInMore.edit.id:
            //     this.onEditTablePhyObj();
            //     break;
            // case TableNavBarBtnInMore.move.id:
            //     this.onMovePhyObj();
            //     break;
            // case TableNavBarBtnInMore.reclassify.id:
            //     this.onReclassifyPhyObj();
            //     break;
        }

    }

    phyObjHandleSwitchButtonChanged(e, args) {
        this.setState({
            phyObjSwitchButtonChecked: args.checked
        });
    }

    getTableCellDetail(args) {
        let selTreeItem = this.selectedTreeItem;
        let selTreeItemType = selTreeItem.NodeType;
        this.setState({
            phyObjDetailParam: {
                isRequest: false,
                id: args.Id,
                nodeType: args.NodeType,
                BoxId: selTreeItemType == NodeType.PhyBox ? selTreeItem.Id : selTreeItem.BoxId,
                FileId: selTreeItemType == NodeType.PhyFile ? selTreeItem.Id : selTreeItem.FileId,
            },
            showViewDetailPanel: { show: true }
        });
    }

    deletePhyObjMsg(isOnlySingle, currentPhyArr, hasChildrenItems) {
        let deleteMsgContent = '';
        if (hasChildrenItems.length == 0) {
            deleteMsgContent = RMResx.RM_PRM_PRE_Msg_ConfirmDeletePhyObj;
        } else {
            deleteMsgContent = <div>
                <div>{RMResx.RM_PRM_PRE_Msg_ConfirmDelHasChildrenPhyObj}</div>
                <div className='margin-top-10 strong'>
                    {
                        hasChildrenItems.map((item, index) => {
                            return (
                                <div key={index}>{item.Name}</div>
                            );
                        })
                    }
                </div>
            </div>;
        }
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div>{deleteMsgContent}</div>
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false, this.args);
                    }
                },
                {
                    id: "raPhyDeleteMsgSureBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.onDeletePhyObjMsgSureClick.bind(this, isOnlySingle, currentPhyArr)
                }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    validDelItemsHasChildren(isOnlySingle, currentPhyArr) {
        $$.loading(true);
        let param = isOnlySingle == true ? currentPhyArr : this.tableSelectItems;
        let url = `/api/PhysicalRecordApi/PreDeletePhysicalObject`;
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            this.deletePhyObjMsg(isOnlySingle, currentPhyArr, JSON.parse(result));
        });
    }

    onDeletePhyObjMsgSureClick(isSingle, currentPhyArr) {
        $$.messagedialog(false, this.args);
        $$.loading(true);
        let isDelCurrentObj = false;
        let param = [];
        if (isSingle) {
            isDelCurrentObj = currentPhyArr[0].Id == this.selectedTreeItem.Id;
            param = currentPhyArr;
        } else {
            param = this.tableSelectItems;
        }
        let url = `/api/PhysicalRecordApi/DeletePhysicalObject`;
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        fetchUtility(option).then((result) => {
            if (!result.HasError) {
                $$.loading(false);
                if (isDelCurrentObj) {
                    if (this.state.viewMode == TreeViewMode.Term) {
                        this.refTermTree.deleteSelectedNode();
                        return;
                    } else if (this.state.viewMode == TreeViewMode.Location) {
                        this.refExplorerTree.deleteSelectedNode();
                        return;
                    }
                }
                if (isSingle) {
                    this.dispatch('phyObjTable', 'reset', currentPhyArr);
                } else {
                    this.dispatch('phyObjTable', 'reset');
                }
                this.getPhysicalObjectList(this.selectedTreeItem, true);
                this.initCurrentPhysicalObjectInfo(this.selectedTreeItem);
                if (this.state.viewMode == TreeViewMode.Term) {
                    this.refTermTree.refreshSelectedNode();
                } else if (this.state.viewMode == TreeViewMode.Location) {
                    this.refExplorerTree.refreshSelectedNode();
                }
                showToast.success(RMResx.RM_PRM_PRE_Msg_DeleteItemSuccess);
            } else {
                showToast.error(RMResx.RM_PRM_PRE_Msg_DeleteItemError);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onDeleteTablePhyObj() {
        if (this.tableSelectItems.length > 15) {
            this.operateLimitDia();
            return;
        }
        this.validDelItemsHasChildren(false);
    }

    operateLimitDia() {
        this.args = {
            classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_Explorer_RecordsCheckLimit,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false, this.args);
                    }
                },
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onNewRequest(nodeType, templateId) {
        this.openForm(PhyObjFormType.NewRequest, nodeType, null, templateId);
    }

    onMove() {
        let callback = (moveData, errorCallBack) => {
            this.sendMoveRequest(moveData, errorCallBack);
        };

        this.dispatch('moveTree', 'onSave', callback);
        return false;
    }

    onPhysicalMoveHoldConflict(val) {
        this.setState({ selectedMoveHoldConflict: val });
    }

    sendMoveRequestWithConflictResolution() {
        this.moveData.HoldConflictOption = parseInt(this.state.selectedMoveHoldConflict, 10);
        this.setState({ selectedMoveHoldConflict: "" });
        this.sendMoveRequest(this.moveData);
    }

    sendMoveRequest(moveData, errorCallBack) {
        let boxId = "";
        if (moveData.Target.NodeType === NodeType.PhyBox) {
            boxId = moveData.Target.Id;
        } else if (moveData.Target.NodeType === NodeType.PhyFile) {
            boxId = moveData.Target.BoxId === EmptyGUID ? "" : moveData.Target.BoxId;
        }
        let moveSendData = {
            SourcePhyRecordIds: moveData.Source.map(function (t) {
                return t.Id;
            }),
            LocationId: moveData.Target.LocationId,
            BoxId: boxId,
            FolderId: moveData.Target.NodeType === NodeType.PhyFile ? moveData.Target.Id : "",
            NameConflictOption: moveData.ConflictOption,
            HoldConflictOption: moveData.HoldConflictOption
        };
        $$.loading(true);
        let url = `/api/RecordsExplorerApi/PhysicalMove`;
        let option = {
            url: url,
            method: "POST",
            data: moveSendData
        };
        fetchUtility(option).then((result) => {
            let resultData = JSON.parse(result);
            if (resultData.MessageType != 0) {
                if (resultData.FaildType == 12) {
                    this.moveData = moveData;
                    this.setState({ selectedMoveHoldConflict: "1" });
                    this.setState({ physicalMoveHoldConflictDialogShow: true });
                } else {
                    this.setState({ physicalMoveHoldConflictDialogShow: false });
                    errorCallBack(result.ErrorMessage);
                }
            } else {
                this.setState({
                    showMovePanel: { show: false },
                    physicalMoveHoldConflictDialogShow: false
                });
                showToast.success(RMResx.RM_RDM_Explorer_PhyMoveStarting);
                this.updateNotificationTimer(resultData.Extension, 'move', moveData.isTopButton, moveData.Target.Id);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onFilter() {
        let isClear = false;
        this.dispatch('filter', 'onSave', isClear);
        this.getPhysicalObjectList(this.selectedTreeItem, true, false, TelemetryEventType.Filter);
    }

    onClearFilter() {
        let isClear = true;
        this.dispatch('filter', 'onSave', isClear);
    }

    getFilterData(data) {
        this.filterData = RM.deepcopy(data);
        this.filterData.RecordsOwner = this.getPeopleIds(data.RecordsOwner, 'true');
        this.filterData.CreatedBy = this.getPeopleIds(data.CreatedBy);
        this.filterData.ModifiedBy = this.getPeopleIds(data.ModifiedBy);
        this.setState({ phyObjFilterEchoData: RM.deepcopy(data)});
    }

    resetFilter() {
        this.filterData = {};
        this.setState({ phyObjFilterEchoData: {} });
    }

    getPeopleIds(args, isRecordsOwner) {
        let params = [];
        if (args && args.length > 0) {
            for (let arg of args) {
                if (isRecordsOwner) {
                    params.push(arg.RMUserId);
                } else {
                    params.push(arg.DisplayName);
                }
            }
        }
        return params;
    }

    onMovePhyObj() {
        let selectedTreeItemArr = [];
        selectedTreeItemArr.push(this.selectedTreeItem);
        this.openMovePanel(selectedTreeItemArr, true);
    }

    onMoveTablePhyObj() {
        if (this.tableSelectItems.length > 15) {
            this.operateLimitDia();
            return;
        }
        this.openMovePanel(this.tableSelectItems, false);
    }

    onMoveRequestPhyObj() {
        let selectedTreeItemArr = [];
        selectedTreeItemArr.push(this.selectedTreeItem);
        this.openPhyMoveRequestPanel(selectedTreeItemArr);
    }

    onMoveRequestTablePhyObj() {
        if (this.tableSelectItems.length > 15) {
            this.operateLimitDia();
            return;
        }
        this.openPhyMoveRequestPanel(this.tableSelectItems);
    }

    openPhyMoveRequestPanel(items) {
        let moveData = {
            Source: items,
            isRequest: true
        };

        let smallNodeType = NodeType.PhysicalBottomLocation;
        for (let index = 0; index < items.length; index++) {
            const element = items[index];
            if (element.NodeType == NodeType.PhyRecord) smallNodeType = NodeType.PhyFile;
            if (element.NodeType == NodeType.PhyFile) smallNodeType = NodeType.PhyBox;
            if (element.NodeType == NodeType.PhyBox) {
                smallNodeType = NodeType.PhysicalBottomLocation;
                break;
            }
        }

        this.setState({
            showPhyMoveRequestPanel: { show: true },
            phyObjMoveParam: moveData,
            smallNodeType: smallNodeType,
        });
    }

    onSavePhyMoveRequest() {
        let callback = (moveData) => {
            let boxId = "";
            if (moveData.Target.NodeType === NodeType.PhyBox) {
                boxId = moveData.Target.Id;
            } else if (moveData.Target.NodeType === NodeType.PhyFile) {
                boxId = moveData.Target.BoxId === EmptyGUID ? "" : moveData.Target.BoxId;
            }

            let moveSendData = {
                Items: moveData.Source.map(item => ({
                    Id: item.Id,
                    Name: item.LeafName || item.Name,
                    UniqueId: item.RecordsId || item.UniqueId,
                    NodeType: item.NodeType
                })),
                MoveDto: {
                    SourcePhyRecordIds: moveData.Source.map(t => t.Id),
                    LocationId: moveData.Target.LocationId,
                    BoxId: boxId,
                    FolderId: moveData.Target.NodeType === NodeType.PhyFile ? moveData.Target.Id : "",
                    NameConflictOption: moveData.ConflictOption || "1",
                    HoldConflictOption: moveData.HoldConflictOption
                },
                Comment: moveData.Comment
            };

            $$.loading(true);
            let url = `/api/PhysicalRequestApi/MoveRequest`;
            let option = {
                url: url,
                method: "POST",
                data: moveSendData
            };

            fetchUtility(option, response => {
                this.handleError(response);
            }).then((result) => {
                $$.loading(false);
                if (result && result.HasError) {
                    $$.messagedialog(true, {
                        width: "500px",
                        title: RMResx.RM_PRM_PRE_Msg_MovementFailed_Title,
                        content: result.ErrorMsg || result.ErrorMessage,
                        buttons: [
                            {
                                text: RMResx.RM_JS_Common_OK,
                                primary: true,
                                classify: "theme",
                                onClick: () => { $$.messagedialog(false); }
                            }
                        ]
                    });
                } else {
                    showToast.success(RMResx.RM_PRM_PRE_Msg_MovementSuccess);
                    this.setState({ showPhyMoveRequestPanel: { show: false } });
                    this.getPhysicalObjectList(this.selectedTreeItem, true);
                }
            }).catch((e) => {
                $$.loading(false);
            });
        };

        this.dispatch('phyMoveRequestTree', 'onSave', callback);
        return false;
    }

    renderPhyMoveRequestPanel() {
        return <R.Panel
            id="moveRequestPanel"
            header={"Movement request"}
            size={600}
            status={this.state.showPhyMoveRequestPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <PhyMovementRequest
                    id="phyMoveRequestTree"
                    data={this.state.phyObjMoveParam}
                    smallNodeType={this.state.smallNodeType}
                ></PhyMovementRequest>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showPhyMoveRequestPanel: { show: false } });
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSavePhyMoveRequest} />
            </>
        </R.Panel>;
    }

    openMovePanel(items, isTopButton) {
        let moveData = {
            Source: items,
            isTopButton: isTopButton
        };

        //PhyBox: 9300,
        //PhyFile: 9400,
        let smallNodeType = NodeType.PhysicalBottomLocation;
        for (let index = 0; index < items.length; index++) {
            const element = items[index];
            if (element.NodeType == NodeType.PhyRecord) { 
                smallNodeType = NodeType.PhyFile;
            }
            if (element.NodeType == NodeType.PhyFile) {
                smallNodeType = NodeType.PhyBox;
            }
            if (element.NodeType == NodeType.PhyBox) {
                smallNodeType = NodeType.PhysicalBottomLocation;
                break;
            }
        }
        //this.dispatch('moveTree', 'init', moveData);
        this.setState({
            showMovePanel: { show: true },
            phyObjMoveParam: moveData,
            smallNodeType: smallNodeType,
        });
    }

    onCheckItemsOnHold(Ids, callback) {
        let option = {
            url: `/api/PhysicalRequestApi/CheckItemOnHold`,
            method: "POST",
            data: Ids
        };
        fetchUtility(option).then((res) => {
            if (res) {
                this.onLoanConfirming();
                return;
            }
            callback?.();
        });
    }

    onLoanTablePhyObj() {
        if (this.tableSelectItems.length > 15) {
            this.operateLimitDia();
            return;
        }
        let selectedItemIds = this.tableSelectItems.map((item) => item.Id);
        this.onCheckItemsOnHold(selectedItemIds, () => {
            let loanData = {
                Items: this.tableSelectItems.map((item) => {
                    return {
                        Id: item.Id,
                        Name: item.Name,
                        UniqueId: item.UniqueId,
                        NodeType: item.NodeType,
                    };
                })
            };
            // this.dispatch(this.peLoanId, 'init', loanData);
            this.setState({
                showLoanPanel: { show: true },
                phyObjLoanParam: loanData
            });
        })
    }

    onLoanPhyObj() {
        let selectedItemIds = [this.selectedTreeItem.Id];
        this.onCheckItemsOnHold(selectedItemIds, () => {
            let loanData = {
                Items: [{
                    Id: this.selectedTreeItem.Id,
                    Name: this.selectedTreeItem.Name,
                    NodeType: this.selectedTreeItem.NodeType,
                    UniqueId: this.state.currentPhyObj.UniqueId
                }]
            };
            this.setState({
                showLoanPanel: { show: true },
                phyObjLoanParam: loanData
            });
        })
    }

    onLoanConfirming = () => {
        $$.messagedialog(true, {
            width: "550px",
            title: RMResx.RM_LR_LoanRequest_Confirm,
            content: RMResx.RM_LR_Common_Refusal,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                }
            ]
        });
    }

    onShowTableAudit() {
        this.setState({
            showAuditPanel : true,
            selectedPhyObjByTableOrTree: this.tableSelectItems
        });
    }

    onShowPhyAuditObj(){
        this.setState({
            showAuditPanel : true,
            selectedPhyObjByTableOrTree:  [this.state.currentPhyObj]
        });
    }

    
    newHoldValidation(selectedRecords) {
        //let boxNodes = selectedRecords.filter(node => node.NodeType == NodeType.PhyBox);
        //if (boxNodes.length > 0) {
        //    $$.loading(true);
        //    let boxIds = boxNodes.map(node => node.Id);
        //    let url = `/api/PhysicalRecordApi/ValidateBoxHold`;
        //    let option = {
        //        url: url,
        //        method: "POST",
        //        data: {NodeId: boxIds, NodeType: NodeType.PhyBox}
        //    };
        //    fetchUtility(option).then((result) => {
        //        if (result.HasChildrenHold) {
        //            this.holdFilesForValidation = result.FolderNames;
        //            this.setState({boxHoldDialogShow: true});
        //        } else {
        //            this.openHoldForm(selectedRecords, "new");
        //        }
        //        $$.loading(false);
        //    }).catch((e) => {
        //        $$.loading(false);
        //    });
        //} else {
        //}
        this.openHoldForm(selectedRecords, "new");  //RECO-5058
    }

    onHoldPhyObj() {
        this.newHoldValidation([this.selectedTreeItem]);
    }

    onManageHoldAction(key, selectItemsByTree) {
        if (this.tableSelectItems.length > 15) {
            this.operateLimitDia();
            return;
        }
        let selectItems = selectItemsByTree ? [this.state.currentPhyObj] : this.tableSelectItems;
        if (key == "manage") {
            this.routerTo(RouterUrls.PRM_ManageHold, "phy");
            //this.openForm(PhyObjFormType.HoldPhyObj, this.tableSelectItems[0].NodeType, this.tableSelectItems[0].Id);
        } else if (key == "new") {
            //this.openHoldForm(this.tableSelectItems, "new");
            this.newHoldValidation(selectItems);
        } else if (key == "change") {
            this.openHoldForm(selectItems, "change");
        } else if (key == "remove") {
            this.removeHoldAction(selectItems);
        } else if (key == "extend") {
            this.openHoldForm(selectItems, "extend");
        } else if (key == "append") {
            this.openHoldForm(selectItems, "append");
        }
    }

    removeHoldAction(selectItems) {
        if (selectItems.length == 1) {
            let recordId = selectItems[0].Id;
            let url = "/api/RecordsExplorerApi/LoadHoldSettings?recordId=" + recordId;
            let option = {
                url: url,
                method: "Post"
            };
            fetchUtility(option).then((result) => {
                let removeList = [];
                let holdUntilTime = "";
                if (result != null) {
                    result.map(item => {
                        holdUntilTime = RMResx.RM_PRM_PRE_Dialog_WillReleaseTimeOn.format(item.Name, item.HoldUntilTime);
                        removeList.push({ name: holdUntilTime, value: item.Id, text: holdUntilTime, holdName: item.Name });
                    });
                    this.setState({
                        removeHoldProfileList: removeList,
                        removeHoldSelectDialogShow: true,
                        selectedPhyObjByTableOrTree: selectItems
                    });
                }
            }).catch((e) => {
                //console.log(e);
            });
        } else {
            this.setState({
                removeHoldDialogShow: true,
                selectedPhyObjByTableOrTree: selectItems
            });
        }
    }

    onShowFilter() {
        //当filter需要穿透的默认值
        this.state.phyObjFilterEchoData.NodeType = this.filterData.NodeType || -4;
        this.state.phyObjFilterEchoData.Status = this.filterData.Status || -1;
        this.setState({
            showFilterPanel: { show: true },
            phyObjFilterEchoData: this.state.phyObjFilterEchoData
        });
    }

    pagerChange(pageIndex, pageSize) {
        this.currentPage.pageIndex = pageIndex;
        this.currentPage.pageSize = pageSize;
        this.getPhysicalObjectList(this.selectedTreeItem, false);
    }

    onSaveReclassify() {
        let callback = (termData, errorCallBack) => {
            let validateFailed = false;
            //TODO xwwang reclassify
            if (termData.Type == 'Root' || termData.Type == 'TermGroup' || termData.Type == 'TermSet') {
                validateFailed = true;
                errorCallBack(RMResx.RM_JS_PRM_Msg_ReclassifyNoSelecteTermLevel);
            }
            if (validateFailed) {
                return false;
            }
            let reclassifyData = {
                PhyRecordIds: this.state.phyObjReclassifyParam.Source.map(function (t) {
                    return t.Id;
                }),
                TermInfo: {
                    Id: termData.Id,
                    Name: termData.Name,
                    UniqueId: termData.UniqueId
                },
                Comment: termData.Comment
            };
            $$.loading(true);
            let url = `/api/RecordsExplorerApi/ChangeTerm`;
            let option = {
                url: url,
                method: "POST",
                data: reclassifyData
            };
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((result) => {
                let resultData = JSON.parse(result);
                if (resultData.MessageType != 0) {
                    errorCallBack(resultData.ErrorMessage);
                } else {
                    this.setState({
                        showReclassifyPanel: { show: false }
                    });
                    showToast.success(RMResx.RM_RDM_Explorer_ChangeTermStarting);
                    this.updateNotificationTimer(resultData.Extension, 'reclassify', termData.isTopButton);
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        };
        this.dispatch(this.peReclassifyId, 'onSave', callback);
        return false;
    }

    handleError(response) {
        $$.loading(false);
        if (response.status == 403) {
            $$.messagedialog(true, {
                // classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_Common_NoPermissionLicense,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => { $$.messagedialog(false); }
                    }
                ]
            });
        }

    }

    onSaveRelated() {
        let callback = (data, errorCallBack) => {
            $$.loading(true);
            let url = `/api/RecordsExplorerApi/UpdateRelatedRecords`;
            let option = {
                url: url,
                method: "POST",
                data: data
            };
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((result) => {
                let resultData = JSON.parse(result);
                if (resultData.MessageType != 0) {
                    errorCallBack(resultData.ErrorMessage);
                } else {
                    showToast.success(RMResx.RM_JS_RDM_Explorer_RelatedRecordsOperationSuccessMsg);
                    this.setState({ showRelatedRecordsPanel: { show: false } });
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        };
        this.dispatch(this.peRelatedId, 'onSave', callback);
        this.dispatch('phyObjTable', 'reset');
        return false;
    }

    onSavePermission() {
        let callback = (data, success) => {
            if (success) {
                $$.loading(true);
                let nodeIds = [];
                let accounts = null;
                let userList = data.userList;
                let param = {};
                let url = `/api/PhysicalRecordApi/SavePhysicalPermission`;
                if (userList && userList.length > 0) {
                    accounts = [];
                    for (let item of userList) {
                        let userObj = {};
                        userObj.UserId = item.UserId;
                        userObj.UserName = item.UserName;
                        userObj.UserPrincipalName = item.UserPrincipalName;
                        userObj.Email = item.Email;
                        userObj.DisplayName = item.DisplayName;
                        userObj.InviteType = item.InviteType;
                        userObj.RMUserId = item.RMUserId;
                        userObj.Id = item.Id;
                        userObj.SurName = item.SurName;
                        userObj.GivenName = item.GivenName;
                        userObj.TenantId = item.TenantId;
                        accounts.push(userObj);
                    }
                }
                for (let item of this.state.selectedPhyObjByTableOrTree) {
                    nodeIds.push(item.Id);
                }
                param.ScopeIds = nodeIds;
                param.Accounts = accounts;
                param.IsInherit = data.IsInherit;
                let option = {
                    url: url,
                    method: "POST",
                    data: param
                };
                fetchUtility(option).then((result) => {
                    $$.loading(false);
                    this.setState({ showManagePermissionPanel: false });
                    let updateProps = {
                        BreakInheritance: !param.IsInherit,
                    };
                    if (this.selectedTreeItem.NodeType == NodeType.PhyFile || this.state.viewMode == TreeViewMode.Term) {
                        updateProps.Name = this.selectedTreeItem.Name;
                        updateProps.RecordStatus = this.selectedTreeItem.RecordStatus;
                        updateProps.OnLoan = this.selectedTreeItem.OnLoan;
                    }
                    if (result.MessageType == "0") {
                        if (result.Extension) {
                            if (this.isRMAdmin) {
                                showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                                </$g.I18NProvider>);
                            } else {
                                showToast.success(RMResx.RM_JS_MA_JobSucessMessage);
                            }
                        } else {
                            showToast.success(RMResx.RM_PRM_PRE_MsgSavePermissionSuccessful);
                        }
                    }
                    else {
                        showToast.error(result.ErrorMessage);
                        return;
                    }
                    let hasSelNodePermission = result.Extsion1 === false;
                    let isRefreshParentNode = hasSelNodePermission && this.state.isTopPermissionBtn;
                    if (nodeIds.length == 1 && (this.selectedTreeItem.Id == this.state.selectedPhyObjByTableOrTree[0].Id || this.selectedTreeItem.LocationId == this.state.selectedPhyObjByTableOrTree[0].Id)) {
                        if (this.state.viewMode == TreeViewMode.Term) {
                            this.refTermTree.refreshSelectedNode(updateProps);
                        } else if (this.state.viewMode == TreeViewMode.Location) {
                            if (!isRefreshParentNode) {
                                this.refExplorerTree.refreshSelectedNode(updateProps);
                            }
                        }
                    } else {
                        if (this.state.viewMode == TreeViewMode.Term) {
                            this.refTermTree.refreshSelectedNode();
                        } else if (this.state.viewMode == TreeViewMode.Location) {
                            if (!isRefreshParentNode) {
                                this.refExplorerTree.refreshSelectedNode();
                            }
                        }
                    }
                    if (isRefreshParentNode) {
                        this.refExplorerTree.refreshSelectedParentNode();
                    } else {
                        this.getPhysicalObjectList(this.selectedTreeItem, true, true);
                        this.dispatch('phyObjTable', 'reset');
                        this.dispatch('PhyObjectInfo', 'reset', this.selectedTreeItem);
                    }
                }).catch((e) => {
                    $$.loading(false);
                });
            }
        };
        this.dispatch('raPhyObjectManagePermission', 'onSave', callback);
        return false;
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    renderExportBarcodesDialog() {
        let content = LicenseHelper.EnableRecordsArchiver() ?
            <$g.I18NProvider msg={RMResx.RM_Common_ExportLocationTipForNewUserWithSpecialStorage}>
                <a className="ra-link-a" href="/Root/CP/StorageSettings">{RMResx.RM_JS_CP_StorageSetting}</a>
            </$g.I18NProvider> :
            RMResx.RM_Common_ExportLocationTipForOldUserWithSpecialStorage;
        return <R.Dialog
            id="exportBarcodesContainer"
            header={RMResx.RM_RDM_Explorer_ExportBarcode_DialogTitle}
            width={650}
            status={{ show: this.state.exportBarcodesDiaShow }}
            struct={{ foot: true }}
            onHide={this.closeExportBarcodesDialog}
            destroy={true}
        >
            <R.Validation>
                <div id="export-barcodes-dialog">
                    <R.Messagebar
                        message={RMResx.RM_RDM_Explorer_ExportBarcode_TooLargeValid_Message}
                        classify='warn'
                        status={{ show: this.state.barcodesShowTip }}
                        onClose={this.hideBarcodesMessageTip}
                    />
                    <div className="flex flex-column gap-xs">
                        <h4 className='radio-title require'>{RMResx.RM_RDM_Explorer_ExportBarcode_SelectTemplate}</h4>
                        <R.Validation element="Combobox" require>
                            <R.Combobox
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                tooltipField="name"
                                width="100%"
                                searchable={false}
                                items={this.state.barcodeTemplateList}
                                onChange={this.handleChangeBarcodeTemplate}
                            />
                        </R.Validation>
                    </div>
                    <div className='flex flex-column gap-xs margin-top-l'>
                        <h4 className='radio-title'>{RMResx.RM_RDM_Explorer_ExportBarcode_SelectExporType}</h4>
                        <div>
                            <div className='browser-radio-content'>
                                <R.Radio
                                    name='browser-radio'
                                    text={RMResx.RM_RDM_Explorer_ExportBarcode_ExportToBrowser}
                                    value='0'
                                    checked={this.state.isExportToBrowser}
                                    onChange={this.onExportToBrowser} />
                            </div>
                            <div className='browser-radio-content'>
                                <$g.Popover width='340px'>{RMResx.RM_RDM_Explorer_ExportBarcode_ExportToBrowser_Notice}</$g.Popover>
                            </div>
                        </div>
                        <div>
                            <div className='location-radio-label'>
                                <R.Radio
                                    name='location-radio'
                                    value='1'
                                    text={RMResx.RM_RDM_Explorer_ExportBarcode_ExportToLocation}
                                    checked={!this.state.isExportToBrowser}
                                    onChange={this.onExportToLocation} />
                            </div>
                            <div className='location-radio-combobox'>
                                <R.Combobox
                                    width='300'
                                    textField='Name'
                                    valueField='ID'
                                    checkedField='Checked'
                                    items={this.state.exportLocations}
                                    onChange={this.onExportLocationChange}
                                    disabled={this.state.isExportToBrowser}
                                    triggerBySource={true}
                                />
                            </div>
                            <div className='location-radio-popover'>
                                <$g.Popover width='340px'>{content}</$g.Popover>
                            </div>
                            <div className='no-select-location-valid'>
                                <$g.ValidationMsg show={this.state.noDownLoadToValue}>
                                    {RMResx.RM_JS_SPS_ExportSettting_ConfigureExportLocation}
                                </$g.ValidationMsg>
                            </div>
                        </div>
                    </div>
                </div>
            </R.Validation>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.closeExportBarcodesDialog} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_RDM_Explorer_ExportBarcode_DialogExportBtn} onClick={this.onExportBarcodes} />
            </>
        </R.Dialog>;
    }

    renderCreationRequestSuiteBtnGroup() {
        let items = this.state.currentPhyObj.ChildTemplates,
            boxButtons = [],
            folderButtons = [],
            recordButtons = [];
        items.map((item, index) => {
            switch (item.Type) {
                case TemplateTypes.Box:
                    if (checkPermission("PRM_BoxCreationRequest", RM.UserResources)) {
                        boxButtons.push(<R.Button
                            id={"raPrmPhyRequestNewBoxBtn_" + index}
                            key={index}
                            text={this.wrapperI18N(item.Name)}
                            onClick={this.onNewRequest.bind(this, NodeType.PhyBox, item.UniqueId)} />);
                    }
                    break;
                case TemplateTypes.Folder:
                    if (checkPermission("PRM_FolderCreationRequest", RM.UserResources)) {
                        folderButtons.push(<R.Button
                            id={"raPrmPhyRequestNewFolderBtn_" + index}
                            key={index}
                            text={this.wrapperI18N(item.Name)}
                            onClick={this.onNewRequest.bind(this, NodeType.PhyFile, item.UniqueId)} />);
                    }
                    break;
                case TemplateTypes.Record:
                    recordButtons.push(<R.Button
                        id={"raPrmPhyRequestNewRecordBtn_" + index}
                        key={index}
                        text={this.wrapperI18N(item.Name)}
                        onClick={this.onNewRequest.bind(this, NodeType.PhyRecord, item.UniqueId)} />);
                    break;
                default:
                    break;
            }
        });
        return <div className='new-button button-gap'>
            {boxButtons.length > 0 && <R.ButtonGroup
                id="raPrmPhyNewBoxRequestBtnGrp"
                type="button"
                classify="theme"
                height={200}
                text={RMResx.RM_PRM_PRE_NewBoxRequest}
            >
                {boxButtons}
            </R.ButtonGroup>}
            {folderButtons.length > 0 && <R.ButtonGroup
                id="raPrmPhyNewFolderRequestBtnGrp"
                type="button"
                classify="theme"
                height={200}
                text={RMResx.RM_PRM_PRE_NewFolderRequest}
            >
                {folderButtons}
            </R.ButtonGroup>}
            {recordButtons.length > 0 && <R.ButtonGroup
                id="raPrmPhyNewRecordRequestBtnGrp"
                type="button"
                classify="theme"
                height={200}
                text={RMResx.RM_PRM_PRE_NewRecordRequest}
            >
                {recordButtons}
            </R.ButtonGroup>}
        </div>;
    }

    genButtonGroupOptions(items) {
        let buttons = [];
        items.map((item, index) => {
            buttons.push(<R.Button
                id={this.getButtonGroupId(item) + index}
                key={index}
                text={this.wrapperI18N(item.Name)}
                onClick={this.onCreateSuite.bind(this, item)} />);
        });
        return buttons;
    }

    getButtonGroupId(item){
        if(item){
            switch(item.Type){
                case TemplateTypes.CustomTemplate:
                    return "raPrmPhyCreateContainer_";
                case TemplateTypes.Box:
                    return "raPrmPhyCreateBox_";
                case TemplateTypes.Folder:
                    return "raPrmPhyCreateFolder_";
                case TemplateTypes.Record:
                    return "raPrmPhyCreateRecord_";
            }
        }  
    }


    renderNewSuiteBtnGroup(nodeType) {
        let items = this.state.currentPhyObj.ChildTemplates,
            boxBtns = [],
            folderBtns = [],
            recordBtns = [],
            customBtns = [];

        switch (nodeType) {
            case NodeType.PhyCustom:
            case NodeType.PhysicalBottomLocation:
                customBtns = this.genButtonGroupOptions(items.filter(t => t.Type == TemplateTypes.CustomTemplate));
                boxBtns = this.genButtonGroupOptions(items.filter(t => t.Type == TemplateTypes.Box));
                folderBtns = this.genButtonGroupOptions(items.filter(t => t.Type == TemplateTypes.Folder));
                break;
            case NodeType.PhyBox:
                folderBtns = this.genButtonGroupOptions(items.filter(t => t.Type == TemplateTypes.Folder));
                break;
            case NodeType.PhyFile:
                recordBtns = this.genButtonGroupOptions(items.filter(t => t.Type == TemplateTypes.Record));
                break;
        }

        return <React.Fragment>
            {customBtns.length > 0 && <R.ButtonGroup
                id="raPrmPhyNewContainerBtnGrp"
                type="button"
                classify="theme"
                height={200}
                text={RMResx.RM_PRM_PRE_NewContainerWithTemplate}
            >{customBtns}</R.ButtonGroup>}

            {boxBtns.length > 0 && <R.ButtonGroup
                id="raPrmPhyNewBoxBtnGrp"
                type="button"
                classify="theme"
                height={200}
                text={RMResx.RM_PRM_PRE_NewBoxWithTemplate}
            >{boxBtns}</R.ButtonGroup>}

            {folderBtns.length > 0 && <R.ButtonGroup
                id="raPrmPhyNewFolderBtnGrp"
                type="button"
                classify="theme"
                height={200}
                text={RMResx.RM_PRM_PRE_NewFolderWithTemplate}
            >{folderBtns}</R.ButtonGroup>}

            {recordBtns.length > 0 && <R.ButtonGroup
                id="raPrmPhyNewRecordBtnGrp"
                type="button"
                classify="theme"
                height={200}
                text={RMResx.RM_PRM_PRE_NewRecordWithTemplate}
            >{recordBtns}</R.ButtonGroup>}

        </React.Fragment>;
    }

    renderNewButton() {
        let nodeType = this.selectedTreeItem.NodeType;
        let allowNew = this.state.allowNewChildren;
        let hasChildTemplates = this.state.currentPhyObj.ChildTemplates && this.state.currentPhyObj.ChildTemplates.length > 0;
        if (this.isRMAdmin) {
            if (allowNew) {
                return <div className='new-button button-gap'>
                    {nodeType >= NodeType.PhysicalBottomLocation && hasChildTemplates && this.renderNewSuiteBtnGroup(nodeType)}
                </div>;
            }
        } else {
            if (hasChildTemplates && allowNew && this.state.isStandardUserHasPermission) {
                { return this.renderCreationRequestSuiteBtnGroup(); }
            }
        }
    }

    renderChildrenTableNavBar(phyObj) {
        let showBtnCount = screen.availWidth && screen.availWidth <= 1366 ? 3 : 4 ;
        if (phyObj && phyObj.NodeType >= NodeType.PhysicalBottomLocation) {
            return <div>
                <div className='children-navbar'>
                    <div className='navbar-left'>
                        <R.Searchbox
                            id="raPhySearchRecordsIpt"
                            width={380}
                            ref={r => this.refTableSearchBox = r}
                            placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                            disabled={false}
                            onSearch={this.onSearchTableList}
                        />
                    </div>
                    <div className='navbar-right'>
                        <div className='searchbox'>
                            <div className="flex">
                                <R.Button
                                    id="raPhyFilterBtn"
                                    className="theme"
                                    text={RMResx.RM_Common_Filter}
                                    type="button"
                                    icon="fia-filter"
                                    tooltip={RMResx.RM_PRM_PRE_Filter}
                                    onClick={this.onShowFilter}
                                />
                            </div>
                        </div>
                    </div>
                </div>
                <div className="navbar-buttons">
                    {this.renderNewButton()}
                    {this.state.showButtons && <TopButtonsComponent
                        ref={r => this.refTableTopButtons = r}
                        data={{ menuBtnItems: [...this.menuBtnItems_table, ...this.menuBtnItemsInMore_table] }}
                        showCount={showBtnCount}
                    ></TopButtonsComponent>}
                </div>
            </div>;
        }
    }

    updateTopButtonsForTable() {
        let phyObj = this.state.currentPhyObj;
        let tableNavBarBtnAllow = this.state.tableNavBarBtnAllow;
        this.setPermissionButtonForTable = { id: "raPhyAccessControlBtn", name: RMResx.RM_PRM_PRE_ManagePermission, icon: "fia-manage-access-control", onClick: this.onPermitTablePhyObj };
        this.editPhyObjButtonForTable = { id: "raPhyEditBtn", name: RMResx.RM_PRM_PRE_Edit, icon: "fia-edit", onClick: this.onBeforeEditTablePhyObj };
        this.relatedPhyObjButtonForTable = { id: "raPhyRelatedBtn",name: RMResx.RM_PRM_PRE_Related, icon: "fia-related-records", onClick: this.onRelatedTablePhyObj };
        this.movePhyObjButtonForTable = { id: "raPhyMoveBtn", name: RMResx.RM_PRM_PRE_Move, icon: "fia-move", onClick: this.onMoveTablePhyObj };
        this.removePersonHoldPhyObjButtonForTable = { id: "raPhyReturnBtn", name: RMResx.RM_PRM_PRE_Return, icon: "fia-return", onClick: this.onRemovePersonHoldConfirmTablePhyObj };
        this.reclassifyObjButtonForTable = { id: "raPhyReclassifyBtn", name: RMResx.RM_JS_BCM_Explorer_ChangeTerm, icon: "fia-reclassify", onClick: this.onReclassifyTablePhyObj };
        this.deleteObjButtonForTable = { id: "raPhyDeleteBtn", name: RMResx.RM_PRM_PRE_Delete, icon: "fia-delete", onClick: this.onDeleteTablePhyObj };
        this.loanObjButtonForTable = { id: "raPhyLoanBtn", name: RMResx.RM_PRM_PRE_NewLoanRequest, isStatic: true, onClick: this.onLoanTablePhyObj };
        this.moveRequestPhyObjButtonForTable = { id: "raPhyMoveRequestBtn", name: RMResx.RM_PRM_PRE_MovementRequest, icon: "fia-move", onClick: this.onMoveRequestTablePhyObj };
        this.showAuditButtonForTable = { id: "raPhyShowAuditBtn", name: RMResx.RM_PRM_PRE_ShowAudit, icon: "fia-select-all", onClick: this.onShowTableAudit };
        let menuButtons = [...this.menuBtnItems_table];

        if (this.isHoldManagerOnly || this.isHoldOnlyModeByLocationPermission()) {
            this.initTableBarHoldButons(menuButtons);
            this.setState({ showButtons: menuButtons.length > 0 }, () => {
                this.refTableTopButtons && this.refTableTopButtons.updateButtons(menuButtons);
            });
            return;
        }

        if (this.isRMAdmin) {
            const isHoldAllowedNodeType = phyObj && (
                phyObj.NodeType == NodeType.PhyBox ||
                phyObj.NodeType == NodeType.PhysicalBottomLocation ||
                phyObj.NodeType == NodeType.PhyCustom
            );

            if (this.state.allowHoldActionsByEffectiveLocation && isHoldAllowedNodeType) {
                this.initTableBarHoldButons(menuButtons);
            }
            if (tableNavBarBtnAllow.allowSetPermissions) {
                menuButtons.push(this.setPermissionButtonForTable);
            }
            if (tableNavBarBtnAllow.edit) {
                menuButtons.push(this.editPhyObjButtonForTable);
            }
            if (tableNavBarBtnAllow.reclassify) {
                menuButtons.push(this.reclassifyObjButtonForTable);
            }
            if (tableNavBarBtnAllow.move) {
                menuButtons.push(this.movePhyObjButtonForTable);
            }
            if (tableNavBarBtnAllow.delete) {
                menuButtons.push(this.deleteObjButtonForTable);
            }
            if (tableNavBarBtnAllow.del_personal_hold) {
                menuButtons.push(this.removePersonHoldPhyObjButtonForTable);
            }
            if (tableNavBarBtnAllow.related) {
                menuButtons.push(this.relatedPhyObjButtonForTable);
            }
            if (tableNavBarBtnAllow.allowShowAudit){
                menuButtons.push(this.showAuditButtonForTable);
            }

        } else {
            if (tableNavBarBtnAllow.loan && checkPermission("PRM_FolderLoanRequest", RM.UserResources)) {
                menuButtons.push(this.loanObjButtonForTable);
            }
            if (tableNavBarBtnAllow.phyMoveRequest) {
                menuButtons.push(this.moveRequestPhyObjButtonForTable);
            }
            if (checkPermission("PRM_SetAccessControl", RM.UserResources) && tableNavBarBtnAllow.allowSetPermissions) {
                menuButtons.push(this.setPermissionButtonForTable);
            }
            if (tableNavBarBtnAllow.del_personal_hold) {
                menuButtons.push(this.removePersonHoldPhyObjButtonForTable);
            }
            if (tableNavBarBtnAllow.related) {
                menuButtons.push(this.relatedPhyObjButtonForTable);
            }
        }
        this.setState({ showButtons: menuButtons.length > 0 }, () => {
        this.refTableTopButtons && this.refTableTopButtons.updateButtons(menuButtons);
        });
    }

    getPhyObjHoldButons() {
        let selectedItems = this.state.currentPhyObj;
        let mixIcons = {
            "remove": "fas fa-ban",
            "change": "fas fa-pencil-alt",
            "extend": "far fa-clock"
        };
        //Holdstatus == 2 means inheriting hold from parent, nothing is available
        if (selectedItems.HoldStatus != 2) {
            if (selectedItems.HoldType == 2) {
                return <div className='hold-btns-group' style={{ display: "inline-block", marginRight: "8px" }}   >
                    <i className={"icon-mix " + mixIcons["remove"]} />
                    <R.ButtonGroup
                        // type="button"
                        classify="blank"
                        icon="fia-place-hold"
                        text={RMResx.RM_JS_BCM_Explorer_Button_CancelHold}
                        tooltip={RMResx.RM_JS_BCM_Explorer_Button_CancelHold}
                        onClick={this.onManageHoldAction.bind(this, "remove", true)}>
                        <R.Button
                            onClick={this.onManageHoldAction.bind(this, "remove", true)}
                            text={RMResx.RM_JS_BCM_Explorer_Button_CancelHold} />
                        <R.Button
                            onClick={this.onManageHoldAction.bind(this, "append", true)}
                            text={RMResx.RM_JS_BCM_Explorer_Button_AppendHold} />
                        <R.Button
                            onClick={this.onManageHoldAction.bind(this, "change", true)}
                            text={RMResx.RM_JS_BCM_Explorer_Button_ChangeHold} />
                        <R.Button
                            onClick={this.onManageHoldAction.bind(this, "extend", true)}
                            text={RMResx.RM_JS_BCM_Explorer_Button_SuspendHold} />
                    </R.ButtonGroup>
                </div>;
            } else {
                return <div style={{ display: "inline-block", marginRight: "8px" }}><R.Button
                    // type="icon"
                    icon="fia-place-hold"
                    text={RMResx.RM_JS_BCM_Explorer_Button_PutOnHold}
                    // className="nav-btn"
                    tooltip={RMResx.RM_JS_BCM_Explorer_Button_PutOnHold}
                    onClick={this.onHoldPhyObj} /></div>;
            }
        }
    }

    initTableBarHoldButons(menuButtons) {
        if (this.isHoldManagerOnly || this.selectedTreeItem.NodeType != NodeType.PhyFile) {
            let selectedItems = this.tableSelectItems;
            let selectOne = selectedItems.length == 1;
            let selectMany = selectedItems.length > 1;
            let tableSelectItemsHasRecord = false;
            for (let item of this.tableSelectItems) {
                if (item.NodeType == NodeType.PhyRecord || item.NodeType == NodeType.PhyCustom) {
                    tableSelectItemsHasRecord = true;
                    break;
                }
            }
            
            const shouldRenderForRecordSelection = tableSelectItemsHasRecord && this.state.allowHoldActionsByEffectiveLocation;
            const shouldRenderForNonRecordSelection = !tableSelectItemsHasRecord;

            if (shouldRenderForRecordSelection || shouldRenderForNonRecordSelection) {
                if (selectOne && selectedItems[0].HoldStatus != 2) {
                    let selectedItem = selectedItems[0];
                    if (selectedItem.HoldType == 2) {
                        this.holdButtonGroups = {
                            isGroup: true, id: "raPhyHoldBtnGroup", name: RMResx.RM_JS_BCM_Explorer_Button_HoldActions, icon: "fia-place-hold",
                            buttons: [
                                { id: "raPhyCancelHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_CancelHold, onClick: this.onManageHoldAction.bind(this, "remove", false) },
                                { id: "raPhyAppendHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_AppendHold, onClick: this.onManageHoldAction.bind(this, "append", false) },
                                { id: "raPhyChangeHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_ChangeHold, onClick: this.onManageHoldAction.bind(this, "change", false) },
                                { id: "raPhyExtendHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_SuspendHold, onClick: this.onManageHoldAction.bind(this, "extend", false) }
                            ]
                        };
                        menuButtons.push(this.holdButtonGroups);
                    } else {
                        this.holdButtonGroups = { id: "raPhyPlaceHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_PutOnHold, icon: "fia-place-hold", onClick: this.onManageHoldAction.bind(this, "new", false) };
                        menuButtons.push(this.holdButtonGroups);
                    }
                } else if (selectMany) {
                    let allHold = selectedItems.every((item, index) => (item.HoldType == 2 && item.HoldStatus != 2));
                    if (allHold) {
                        this.holdButtonGroups = {
                            isGroup: true, id: "raPhyHoldBtnGroup", name: RMResx.RM_JS_BCM_Explorer_Button_HoldActions, icon: "fia-place-hold",
                            buttons: [
                                { id: "raPhyCancelHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_CancelHold, onClick: this.onManageHoldAction.bind(this, "remove", false) },
                                { id: "raPhyAppendHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_AppendHold, onClick: this.onManageHoldAction.bind(this, "append", false) },
                                { id: "raPhyChangeHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_ChangeHold, onClick: this.onManageHoldAction.bind(this, "change", false) },
                                { id: "raPhyExtendHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_SuspendHold, onClick: this.onManageHoldAction.bind(this, "extend", false) }
                            ]
                        };
                        menuButtons.push(this.holdButtonGroups);
                    } else if (selectedItems.every((item, index) => (item.HoldType != 2 && item.HoldStatus != 2))) {
                        this.holdButtonGroups = { id: "raPhyPlaceHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_PutOnHold, icon: "fia-place-hold", onClick: this.onManageHoldAction.bind(this, "new", false) };
                        menuButtons.push(this.holdButtonGroups);
                    } else if (selectedItems.find((item) => (item.HoldType == 2 && item.HoldStatus != 2)) && selectedItems.find((item) => (item.HoldType != 2 && item.HoldStatus != 2))) {
                        this.holdButtonGroups = { id: "raPhyAppendHoldBtn", name: RMResx.RM_JS_BCM_Explorer_Button_AppendHold, icon: "fia-place-hold", onClick: this.onManageHoldAction.bind(this, "append", false) };
                        menuButtons.push(this.holdButtonGroups);
                    }
                }
            }

        }
    }

    renderChildrenContent(phyObj) {
        let pager = this.state.pager;
        let isShowClear = true;
        return <div className="children-content">
            {phyObj && <React.Fragment>
                <$g.Container show={phyObj.NodeType != NodeType.PhysicalRootLocation} className="ra-section-head">
                    {PhyObjTableItemPosAndTypeDes[phyObj.NodeType]}
                </$g.Container>

                {this.renderChildrenTableNavBar(phyObj)}

                <div className='table-content'>
                    <Table
                        id="phyObjTable"
                        cellOperate={this.cellOperate}
                        onCheckChange={this.onCheckChange}
                        cellClick={this.getTableCellDetail}
                        isShowClear={isShowClear}
                    ></Table>
                    <div className="table-pager">
                        <$g.SimplePager
                            pagerIndex={pager.pageIndex}
                            pagerSize={pager.pageSize}
                            shownCount={pager.shownCount}
                            hasNext={pager.hasNext}
                            onChange={this.pagerChange}
                        ></$g.SimplePager>
                    </div>
                </div>
            </React.Fragment>}
        </div>;
    }

    renderCurrentPhyObjNavBar() {
        return <div className='main-navbar'>
            <div className='navbar-left'>
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: [...this.menuBtnItems, ...this.menuBtnItemsInMore] }}
                    showCount={4}
                ></TopButtonsComponent>
            </div>
            <div className='navbar-right'>
                <TopButtonsComponent
                    ref={r => this.refNavbarRightTopButtons = r}
                    data={{ menuBtnItems: [...this.menuBtnItems_navbarRight, ...this.menuBtnItemsInMore_navbarRight] }}
                    showCount={2}
                ></TopButtonsComponent>
            </div>
        </div>;
    }

    updateTopButtons(phyObj) {
        let menuButtons = [...this.menuBtnItems];
        let currentPhyObjNavBarAllow = this.state.currentPhyObjNavBarAllow;

        if (this.isHoldManagerOnly || this.isHoldOnlyModeByLocationPermission()) {
            this.initHoldButtons(menuButtons, phyObj);
            this.refTopButtons && this.refTopButtons.updateButtons(menuButtons);
            return;
        }

        if (this.isRMAdmin) {
            let currentPhyObjNavBarBtnNotPermissionShow = phyObj && phyObj.NodeType >= NodeType.PhyCustom;
            let currentPhyObjPermissionBtnShow = phyObj && phyObj.NodeType >= NodeType.PhysicalNormalLocation;
            if (this.state.allowHoldActionsByEffectiveLocation && currentPhyObjNavBarBtnNotPermissionShow && phyObj.NodeType != NodeType.PhyCustom) {
                this.initHoldButtons(menuButtons, phyObj);
            }
            if (currentPhyObjPermissionBtnShow && currentPhyObjNavBarAllow.allowSetPermissions) {
                menuButtons.push(this.setPermissionButton);
            }
            if (currentPhyObjNavBarBtnNotPermissionShow && currentPhyObjNavBarAllow.edit) {
                menuButtons.push(this.editPhyObjButton);
            }
            if (currentPhyObjNavBarBtnNotPermissionShow && currentPhyObjNavBarAllow.reclassify) {
                menuButtons.push(this.reclassifyObjButton);
            }
            if (currentPhyObjNavBarBtnNotPermissionShow && currentPhyObjNavBarAllow.move) {
                menuButtons.push(this.movePhyObjButton);
            }
            if (currentPhyObjNavBarBtnNotPermissionShow && currentPhyObjNavBarAllow.delete) {
                menuButtons.push(this.deleteObjButton);
            }
            if (currentPhyObjNavBarBtnNotPermissionShow && currentPhyObjNavBarAllow.del_personal_hold) {
                menuButtons.push(this.removePersonHoldPhyObjButton);
            }
            if (currentPhyObjNavBarBtnNotPermissionShow && currentPhyObjNavBarAllow.related) {
                menuButtons.push(this.relatedPhyObjButton);
            }
            if (currentPhyObjNavBarBtnNotPermissionShow && currentPhyObjNavBarAllow.allowShowAudit) {
                menuButtons.push(this.showObjAuditButton);
            }

        } else {
            if (this.state.isStandardUserHasPermission) {
                if (checkPermission("PRM_FolderLoanRequest", RM.UserResources) && currentPhyObjNavBarAllow.loan) {
                    menuButtons.push(this.loanObjButton);
                }
                if (currentPhyObjNavBarAllow.phyMoveRequest) {
                    menuButtons.push(this.moveRequestPhyObjButton);
                }
                if (currentPhyObjNavBarAllow.related) {
                    menuButtons.push(this.relatedPhyObjButton);
                }
                if (checkPermission("PRM_SetAccessControl", RM.UserResources) && currentPhyObjNavBarAllow.allowSetPermissions) {
                    menuButtons.push(this.setPermissionButton);
                }
                if (checkPermission("PRM_FolderLoanReturn", RM.UserResources) && currentPhyObjNavBarAllow.del_personal_hold) {
                    menuButtons.push(this.removePersonHoldPhyObjButton);
                }
            }
        }
        this.refTopButtons && this.refTopButtons.updateButtons(menuButtons);
    }

    updateTopButtonsNavbarRight() {
        let menuButtonsNavbarRight = [...this.menuBtnItems_navbarRight];
        if (RM.RoleType == RoleType.SupAdmin || checkPermission(RouterUrls.BCM_ManageHold, RM.UserResources)) {
            menuButtonsNavbarRight.push(this.manageHoldBtn);
        }
        if (this.isRMAdmin) {
            if (this.state.allowExportBarcode) {
                menuButtonsNavbarRight.push(this.exportBarcodeBtn);
            }
            menuButtonsNavbarRight.push(this.importBtn);
            menuButtonsNavbarRight.push(this.bulkUpdateBtn);
            if(RM.RoleType == RoleType.SupAdmin){
                menuButtonsNavbarRight.push(this.settingBtn);
            }
        }
        this.refNavbarRightTopButtons && this.refNavbarRightTopButtons.updateButtons(menuButtonsNavbarRight);
    }

    initHoldButtons(menuButtons, phyObj) {
        let selectedItems = phyObj;
        
        if (!selectedItems) {
            return;
        }

        if (selectedItems.HoldStatus != 2) {
            if (selectedItems.HoldType == 2) {
                this.holdButtonGroups = {
                    isGroup: true, id: "raPhyHoldBtnGroupTop", name: RMResx.RM_JS_BCM_Explorer_Button_HoldActions, icon: "fia-place-hold",
                    buttons: [
                        { id: "raPhyCancelHoldBtnTop", name: RMResx.RM_JS_BCM_Explorer_Button_CancelHold, onClick: this.onManageHoldAction.bind(this, "remove", true) },
                        { id: "raPhyAppendHoldBtnTop", name: RMResx.RM_JS_BCM_Explorer_Button_AppendHold, onClick: this.onManageHoldAction.bind(this, "append", true) },
                        { id: "raPhyChangeHoldBtnTop", name: RMResx.RM_JS_BCM_Explorer_Button_ChangeHold, onClick: this.onManageHoldAction.bind(this, "change", true) },
                        { id: "raPhyExtendHoldBtnTop", name: RMResx.RM_JS_BCM_Explorer_Button_SuspendHold, onClick: this.onManageHoldAction.bind(this, "extend", true) }
                    ]
                };
                menuButtons.push(this.holdButtonGroups);
            } else {
                this.holdButtonGroups = { id: "raPhyPlaceHoldBtnTop", name: RMResx.RM_JS_BCM_Explorer_Button_PutOnHold, icon: "fia-place-hold", onClick: this.onHoldPhyObj };
                menuButtons.push(this.holdButtonGroups);
            }
        }
    }

    renderCurrenPhyObjInfo(phyObj) {
        if (phyObj) {
            var metaData = RM.deepcopy(this.state.currentPhyObjMetaData);
            if (phyObj.NodeType > NodeType.PhysicalBottomLocation) {
                this.initHomeLocationFullPath(metaData, phyObj.HomeLocationFullPath);
            }
            let isPhysicalBottomLocation = this.selectedTreeItem.NodeType == NodeType.PhysicalBottomLocation;
            let objectInfo = {
                metaData: metaData,
                barcode: {
                    barcodeBase64Str: phyObj.BarcodeBase64Str,
                    title: phyObj.Name,
                    uniqueId: phyObj.UniqueId
                },
                homeLocationFullPath: phyObj.HomeLocationFullPath,
                NodeType: phyObj.NodeType,
                Id: isPhysicalBottomLocation ? this.selectedTreeItem.LocationId : this.selectedTreeItem.Id,
                ColumnB: phyObj.ColumnB,
                ColumnC: phyObj.ColumnC,
                ColumnD: phyObj.ColumnD,
                ColumnE: phyObj.ColumnE,
                ColumnF: phyObj.ColumnF,
                ImageBase64Str: phyObj.ImageBase64Str
            };
            return <div className="main-content">
                {/* {this.renderCurrentPhyObjNavBar(phyObj)} */}
                {
                    phyObj.NodeType != NodeType.PhysicalRootLocation &&
                    <div>
                        <PhyObjectInfo id='PhyObjectInfo' data={objectInfo}></PhyObjectInfo>
                    </div>
                }
            </div>;
        } else {
            return <div className="main-content">
                {/* {this.renderCurrentPhyObjNavBar(phyObj)} */}
            </div>;
        }
    }

    initHomeLocationFullPath(metaData, path) {
        let isTermView = this.state.viewMode == TreeViewMode.Term;
        for (let cIndex in metaData) {
            if (metaData.hasOwnProperty(cIndex)) {
                var category = metaData[cIndex];
                if (isTermView) {
                    //category.id == PhyCategoryBaseInfoId.boxId || category.id == PhyCategoryBaseInfoId.fileId
                    if (cIndex == 0) {
                        category.columns.push({
                            uniqueId: "d3376585-d956-4ac1-8096-616834307e71",
                            columnName: "RM_Template_Column_Name_HomeLocation",
                            columnValue: path
                        });
                    }
                }
                for (let index in category.columns) {
                    if (category.columns.hasOwnProperty(index)) {
                        var column = category.columns[index];
                        if (column.uniqueId == PhysicalDefaultColumnIDs.HomeLocation) {
                            column.columnValue = path;
                            if (!isTermView) {
                                return false;
                            }
                        }
                    }
                }
            }
        }
    }

    renderCurrentPhyObjContent(phyObj) {
        return <div className='raPhyObjInfoContainer'>
            {this.renderCurrenPhyObjInfo(phyObj)}

            {this.renderChildrenContent(phyObj)}
        </div>;
    }

    onRemoveHoldConfirm() {
        //do remove CancelHoldByRecords
        let errorMsg = RMResx.RM_JS_RDM_Hold_CancelRecordError;
        let selRecIds = [];
        this.state.selectedPhyObjByTableOrTree.forEach((item, index) => {
            selRecIds.push(item.Id);
        });
        let postData = { recordsId: selRecIds, isPhysical: true };
        let option = {
            url: "/api/RecordsExplorerApi/CancelHoldByRecords",
            method: "POST",
            data: postData
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            if (result == "") {
                showToast.success(RMResx.RM_JS_RDM_Explorer_RemoveSuccessMsg);
                this.getPhysicalObjectList(this.selectedTreeItem, true);
                this.initCurrentPhysicalObjectInfo(this.selectedTreeItem);
            } else {
                showToast.error(result.message || errorMsg);
            }
            this.onCancelRemoveHold();
        }).catch((e) => {
            showToast.error(errorMsg);
            this.onCancelRemoveHold();
        });
    }

    onSingleRemoveHoldConfirm() {
        if (this.refSelectHoldValid && !$$.verify(this.refSelectHoldValid.ref.current)) {
            return false;
        }
        let errorMsg = RMResx.RM_JS_RDM_Hold_CancelRecordError;
        let selRecIds = [];
        this.state.selectedPhyObjByTableOrTree.forEach((item, index) => {
            selRecIds.push(item.Id);
        });
        let selectedHoldIds = this.state.removeHoldProfileList.filter(h => h.checked).map(t => t.value);
        let selectedHoldName = this.state.removeHoldProfileList.filter(h => h.checked).map(t => t.holdName);
        let postData = { recordsId: selRecIds, isPhysical: true, removeHoldIds: selectedHoldIds };
        let option = {
            url: "/api/RecordsExplorerApi/CancelSelectedHoldByRecords",
            method: "POST",
            data: postData
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            if (result == "") {
                let message = <$g.I18NProvider msg={RMResx.RM_PRM_Explorer_RemoveSuccess}>
                    {selectedHoldName.join(", ")}
                </$g.I18NProvider>;
                showToast.success(message);
                this.getPhysicalObjectList(this.selectedTreeItem, true);
                this.initCurrentPhysicalObjectInfo(this.selectedTreeItem);
            } else {
                let tipMsg = result.message || errorMsg;
                this.showMessageTip("error", tipMsg);
            }
            this.onCancelRemoveHold();
        }).catch((e) => {
            this.showMessageTip("error", errorMsg);
            this.onCancelRemoveHold();
        });
    }

    onCancelRemoveHold() {
        this.setState({ removeHoldDialogShow: false, removeHoldSelectDialogShow: false });
    }

    expanderShown() {
        this.setState({});
    }

    onViewModeTabChange(index) {
        if (this.refTableSearchBox) {
            this.refTableSearchBox.clear();
        }
        this.setState({ viewMode: index, allowExportBarcode: false });
        // if (args.selectedIndex == 0) {
        // }
        let option = {
            url: '/api/PhysicalRecordApi/SetViewMode',
            method: "post",
            data: index
        };
        fetchUtility(option).then((res) => {

        });
        // setTimeout(() => {
        // }, 200);
    }

    renderRemoveHoldDialog() {
        return <R.Dialog
            id="removeHoldConfirmDialog"
            header={RMResx.RM_JS_BCM_Explorer_Button_CancelHold}
            width={480}
            status={{ show: this.state.removeHoldDialogShow }}
            struct={{ foot: true }}
            onHide={this.onCancelRemoveHold.bind(this)}
            destroy={true}
        >
            <div id="removeHoldDialog_body" className="phyhold-expander">
                <div className="hold-dialog-removehold-tip">{RMResx.RM_PRM_PRE_Dialog_RemoveReminder}</div>
                <div>
                    <div className="hold-dialog-removehold-details">{RMResx.RM_PRM_PRE_Dialog_RemoveItemPrefix}</div>
                    <div className="hold-dialog-removehold-list" tabIndex="0">
                        {
                            this.state.selectedPhyObjByTableOrTree.map((item, index) => {
                                return <div
                                    key={"item" + index}
                                    className="hold-dialog-removehold-item">
                                    <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Dialog_WithReleaseTime}>
                                        <span>{item.Name + " (" + item.UniqueId + ")"}</span>
                                        <span>{item.HoldReleaseTimeStr}</span> 
                                    </$g.I18NProvider>
                                </div>;
                            })
                        }
                    </div>
                </div>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelRemoveHold.bind(this)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onRemoveHoldConfirm.bind(this)} />
            </>
        </R.Dialog>;
    }

    onCancelBoxHold() {
        this.setState({ boxHoldDialogShow: false });
    }

    renderBoxHoldDialog() {
        return <R.Dialog
            id="boxHoldConfirmDialog"
            header={RMResx.RM_JS_BCM_Explorer_Button_PutOnHold}
            width={650}
            height={400}
            status={{ show: this.state.boxHoldDialogShow }}
            struct={{ foot: true }}
            onHide={this.onCancelBoxHold.bind(this)}
            destroy={true}
        >
            <div id="boxHoldDialog_body" className="phyhold-expander">
                <R.Expander status={{ show: true }} onShow={this.expanderShown.bind(this)} title={RMResx.RM_PRM_PRE_Dialog_BoxHoldReminder}>
                    <div className="phyhold-expander-list">
                        {
                            this.holdFilesForValidation &&
                            this.holdFilesForValidation.map((item, index) => {
                                return <div key={"boxItem" + index} className="phyhold-expander-item">{item}</div>;
                            })
                        }
                    </div>
                </R.Expander>
            </div>
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_OK} onClick={this.onCancelBoxHold.bind(this)} />
        </R.Dialog>;
    }

    renderRemovePersonHoldDialog() {
        return <R.Dialog
            id="removePersonHoldConfirmDialog"
            header={RMResx.RM_PRM_PRE_Dialog_ReturnTitle}
            width={750}
            status={{ show: this.state.removePersonHoldDialogShow }}
            struct={{ foot: true }}
            onHide={this.onCancelRemovePersonHold}
            destroy={true}
        >
            <div id="removePersonHoldDialog_body" className="phyhold-expander">
                <div className="phyhold-dialog-removehold-label">{RMResx.RM_PRM_PRE_Dialog_ReturnReminder}</div>
                <R.Expander status={{ show: true }} onShow={this.expanderShown.bind(this)} title={RMResx.RM_PRM_PRE_Dialog_ReturnPrefix}>
                    <div className="phyhold-expander-list">
                        {
                            this.state.selectedPhyObjByTableOrTree.map((item, index) => {
                                return <div
                                    key={"item" + index}
                                    className="phyhold-expander-item">
                                    {item.PersonHoldReleaseTime > 0 &&
                                        <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Dialog_WithReturnTime}>
                                            <span>{item.Name + " (" + item.UniqueId + ")"}</span>
                                            <span>{item.PersonHoldReleaseTimeStr}</span>
                                        </$g.I18NProvider>}
                                    {((item.PersonHoldReleaseTime == 0) || (item.PersonHoldReleaseTime == undefined)) &&
                                        <span>{item.Name + " (" + item.UniqueId + ")"}</span>}
                                </div>;
                            })
                        }
                    </div>
                </R.Expander>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelRemovePersonHold} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onRemovePersonHold} />
            </>
        </R.Dialog>;
    }

    selectHoldValid = () => {
        let selectedHoldIds = this.state.removeHoldProfileList.filter(h => h.checked).map(t => t.value);
        return selectedHoldIds.length > 0 ? true : RMResx.RM_PRM_PRE_Dialog_SelectHoldErrorValid;
    }

    renderRemoveHoldSelectDialog() {
        return <R.Dialog
            id="removeHoldConfirmDialog"
            header={RMResx.RM_JS_BCM_Explorer_Button_CancelHold}
            width={480}
            status={{ show: this.state.removeHoldSelectDialogShow }}
            struct={{ foot: true }}
            onHide={this.onCancelRemoveHold.bind(this)}
            destroy={true}
        >
            <div id="removeHoldDialog_body" className="phyhold-expander">
                <div className="hold-dialog-remove-hold-list-label">
                    {this.state.removeHoldProfileList && this.state.removeHoldProfileList.length > 0 
                        ? RMResx.RM_PRM_PRE_Dialog_SelectHold 
                        : RMResx.RM_JS_RDM_Hold_InvalidPermission}
                </div>
                <div className="hold-remove-list">
                    <R.Checkbox.Group
                        id="raPhyHoldRemoveListChkGroup"
                        block={true}
                        name="checkboxgroup-removeList"
                        items={this.state.removeHoldProfileList}
                        onChange={this.handleRemoveHoldCheckboxChanged} />
                    <div className="margin-bottom-s"></div>
                    <R.ValidationFaker valid={this.selectHoldValid} ref={r => this.refSelectHoldValid = r} />
                </div>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelRemoveHold.bind(this)} />
                <R.Button slot="buttons" id="raPhyRemoveHoldSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSingleRemoveHoldConfirm.bind(this)} />
            </>
        </R.Dialog>;
    }


    renderHoldPanel() {
        return <R.Panel
            id="holdPanel"
            header={this.state.formPanelTitle}
            size={600}
            status={this.state.showHoldPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <PhyRecordHoldForm
                    id={this.peHoldId}
                    data={this.state.phyObjFormData}
                    type='phy'
                >
                </PhyRecordHoldForm>
            </div>
            <>
                <R.Button slot="buttons" id="raPhyHoldPanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showHoldPanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyHoldPanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveHold} />
            </>
        </R.Panel>;
    }

    renderFormPanel() {
        return <R.Panel
            id="formPanel"
            header={this.state.formPanelTitle}
            size={600}
            status={this.state.showFormPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="phy-head">{RMResx.RM_JS_BCM_Explorer_Template}</span>
                <span className="phy-head margin-xs">{this.state.formTemplateName}</span>
            </div>
            <div className="ra-panel-content">
                <PhyObjectForm
                    id={this.peFormId}
                    data={this.state.phyObjFormData}
                    parentNodeInfo={this.selectedTreeItem}
                    type='phy'
                    setPanelTitle={this.setPanelHeader}
                >
                </PhyObjectForm>
            </div>
            <>
                <R.Button slot="buttons" id="raPhyFormPanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showFormPanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyFormPanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSavePhyObj} />
            </>
        </R.Panel>;
    }

    renderBulkUpdateFormPanel() {
        return <R.Panel
            id="bulkUpdateFormPanel"
            header={this.state.formPanelTitle}
            size={600}
            status={this.state.showBulkUpdateFormPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="phy-head">{RMResx.RM_JS_BCM_Explorer_Template}</span>
                <span className="phy-head margin-xs">{this.state.formTemplateName}</span>
            </div>
            <div className="ra-panel-content">
                <PhyObjBulkUpdateForm
                    id={this.peBulkUpdateFormId}
                    data={this.state.phyObjBulkUpdateData}
                    parentNodeInfo={this.selectedTreeItem}
                    type='phy'
                    setPanelTitle={this.setPanelHeader}
                >
                </PhyObjBulkUpdateForm>
            </div>
            <>
                <R.Button slot="buttons" id="raPhyBulkUpdatePanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showBulkUpdateFormPanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyBulkUpdatePanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveBulkUpdatePhyObj} />
            </>
        </R.Panel>;
    }

    renderLoanPanel() {
        return <R.Panel
            id="loanPanel"
            header={RMResx.RM_PRM_PRE_NewLoanRequest}
            size={600}
            status={this.state.showLoanPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <div id="panel-content">
                    <PhyLoanRequest
                        id={this.peLoanId}
                        data={this.state.phyObjLoanParam}
                    >
                    </PhyLoanRequest>
                </div>
            </div>
            <>
                <R.Button slot="buttons" id="raPhyLoanPanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showLoanPanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyLoanPanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveLoanObj} />
            </>
        </R.Panel>;
    }

    renderReclassifyPanel() {
        return <R.Panel
            id="reclassifyPanel"
            header={RMResx.RM_JS_BCM_Explorer_ChangeTerm}
            size={664}
            status={this.state.showReclassifyPanel}
            destroy={true}
        >
            <div className="ra-panel-content reclassify-panel">
                <div id="reclassify-content">
                    <PhyReclassify
                        id={this.peReclassifyId}
                        data={this.state.phyObjReclassifyParam}
                        type='phy'
                    >
                    </PhyReclassify>
                </div>
            </div>
            <>
                <R.Button slot="buttons" id="raPhyReclassifyPanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showReclassifyPanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyReclassifyPanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveReclassify} />
            </>
        </R.Panel>;
    }

    renderMovePanel() {
        return <R.Panel
            id="movePanel"
            header={RMResx.RM_PRM_PRE_Move}
            size={600}
            status={this.state.showMovePanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <PhyObjectMove
                    id="moveTree"
                    data={this.state.phyObjMoveParam}
                    smallNodeType={this.state.smallNodeType}
                // onSave={(data) => {
                //     this.moveData = data;
                // }}
                ></PhyObjectMove>
                {this.renderPhyMoveHoldConflictDialog()}
            </div>
            <>
                <R.Button slot="buttons" id="raPhyMovePanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showMovePanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyMovePanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onMove} />
            </>
        </R.Panel>;
    }

    renderFilterPanel() {
        return <R.Panel
            id="filterPanel"
            header={RMResx.RM_PRM_PRE_Filter}
            size={400}
            status={this.state.showFilterPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <div className="ra-flex-justify-end">
                    <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClearFilter} tabIndex="0" role="button"> {RMResx.RM_Common_ClearFilter}</a>
                </div>
                <PhyObjectFilter
                    id='filter'
                    onSave={(data) => {
                        this.getFilterData(data);
                    }}
                    data={this.state.phyObjFilterEchoData}
                ></PhyObjectFilter>
            </div>
            <>
                <R.Button slot="buttons" id="raPhyFilterPanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showFilterPanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyFilterPanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onFilter} />
            </>
        </R.Panel>;
    }

    renderViewDetailPanel() {
        return <R.Panel
            id="viewDetailPanel"
            header={RMResx.RM_PRM_PRE_PanelTitle_ViewDetail}
            size={664}
            status={this.state.showViewDetailPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <PhyObjectDetail
                    data={this.state.phyObjDetailParam}
                ></PhyObjectDetail>
            </div>
            <R.Button slot="buttons" id="raPhyViewDetailPanelCloseBtn" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={this.onCloseViewDetail} />
        </R.Panel>;
    }

    renderRelatedRecordsPanel() {
        return <R.Panel
            id="relatedPanel"
            header={RMResx.RM_PRM_PRE_MRR_Title}
            size={1000}
            status={this.state.showRelatedRecordsPanel}
            destroy={true}
        >
            <div className="ra-panel-content reclassify-panel">
                <div id="reclassify-content">
                    <PhyRelatedRecords
                        id={this.peRelatedId}
                        data={this.state.selectedPhyObjByTableOrTree}
                    > </PhyRelatedRecords>
                </div>
            </div>
            <>
                <R.Button slot="buttons" id="raPhyRelatedPanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showRelatedRecordsPanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyReclassifyPanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveRelated} />
            </>
        </R.Panel>;
    }

    renderAuditPanel(){
        return (
            <ShowAuditInfoPanel
                item={this.state.selectedPhyObjByTableOrTree}
                show={this.state.showAuditPanel}
                onHide={this.onHide}
            />)
    }

    onHide(){
        this.setState({ showAuditPanel: false });
    }

    onCancelPhyMoveHoldConflict() {
        this.setState({ physicalMoveHoldConflictDialogShow: false });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    getRadioUniqueIdColumn() {
        return [
            { text: RMResx.RM_PRM_PRE_Import_SameUniqueId_SkipObj, value: true, checked: this.state.skipExistObjChecked },
            { text: RMResx.RM_PRM_PRE_Import_SameUniqueId_OverwriteObj, value: false, checked: !this.state.skipExistObjChecked },
        ];
    }

    getRadioTimeColumn() {
        return [
            { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.state.enableCustomTime },
            { text: RMResx.RM_JS_Common_No, value: false, checked: !this.state.enableCustomTime },
        ];
    }

    handleUpload(args) {
        const isSucceed = args.isSucceed;
        if (isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            this.files = args.files[0];
        }
    }

    handleDelete(args) {
        if (args.isSucceed) {
            this.files = null;
        }
    }

    handleBulkUpdateUpload(args) {
        const isSucceed = args.isSucceed;
        $$.log(isSucceed ? 'uploadSuccess:' : 'uploadError', args);
        if (isSucceed) {
            args.files.forEach(file => {
                if (!file.fileId) {
                    file.fileId = StringUtil.newGuid();
                }
            });
            this.bulkUpdateFiles = args.files;
        }
    }

    handleBulkUpdateDelete(args) {
        if (args.isSucceed) {
            this.bulkUpdateFiles = args.files;
        }
    }

    onBulkExportUpdateSaveClick = (e) => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        let totalSize = 0;
        let sizeLimit = 100 * 1024 * 1024;//100M
        const formData = new FormData();
        this.bulkUpdateFiles.forEach((file, index) => {
            formData.append(`recordsFileUp${index}`, file.file, file.fileName);
            totalSize += file.file.size;
        });
        if (totalSize > sizeLimit) {
            showToast.error(RMResx.RM_Phy_Import_UploadFileExceedLimit);
            $$.loading(false);
            return false;
        }
        fetch('/api/PhysicalRecordsBulkImportApi/ImportZipData', {
            method: 'POST',
            body: formData,
        })
            .then(async function (response) {
                return await response.text();
            })
            .then(function (result) {
                $$.loading(false);
                if (result === "ok") {
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                } else {
                    showToast.error(result);
                }
            }).catch((e) => {
                $$.loading(false);
            });
        this.setState({ showBulkExportUpdatePanel: { show: false } });
    }

    onShowExportRecordsDialogClick = () => {
        this.loadTemplate();
        this.setState({ showExportRecordsDialog: true });
    }

    onCancelExportRecordsDialog() {
        this.setState({ showExportRecordsDialog: false });
    }

    loadTemplate() {
        $$.loading(true);
        let option = {
            url: '/api/TemplateManagementApi/GetAllExistingTemplatesInfo',
            method: "Post"
        };
        fetchUtility(option).then((res) => {
            this.setState({ templateList: this.getMultiComboBoxWithGroupName(res.Templates) });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getMultiComboBoxWithGroupName(templates) {
        let mapping = {
            [TemplateTreeNodeType.Custom]: RMResx.RM_PRM_TM_ExistingCustomTemplate_GroupTitle,
            [TemplateTreeNodeType.Box]: RMResx.RM_PRM_TM_ExistingBoxTemplate_GroupTitle,
            [TemplateTreeNodeType.Folder]: RMResx.RM_PRM_TM_ExistingFolderTemplate_GroupTitle,
            [TemplateTreeNodeType.Records]: RMResx.RM_PRM_TM_ExistingRecordTemplate_GroupTitle
        };
        for(let item of templates){
            item.group = mapping[item.Type] || "";
        }
        return templates;
    }

    onTemplateChange = (args) => {
        let selTemplateIds = args.newValue.map((item) => {
            return item.Id;
        });
        this.setState({ selectedTemplateIds: selTemplateIds });
    }

    onExportRecords = () => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        let option = {
            data: this.state.selectedTemplateIds.join(","),
            url: "/api/PhysicalRecordsBulkImportApi/ExportZipData",
            method: "Post",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res === "ok") {
                showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                </$g.I18NProvider>);
            } else {
                showToast.error(res);
            }
            this.setState({ showExportRecordsDialog: false });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleExistSameUniqueId = (args) => {
        this.setState({ skipExistObjChecked: args });
    }

    handleEnableCustomTime = (args) => {
        this.setState({ enableCustomTime: args });
    }

    handleImportSaveClick = (e) => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        const formData = new FormData();
        formData.append('recordsFileUp', this.files.file, this.files.fileName);
        formData.append('conflictOption', this.state.skipExistObjChecked ? "0" : "1");
        formData.append('enableCustomTime', this.state.enableCustomTime ? "0" : "1");
        fetch('/api/PhysicalRecordsBulkImportApi/ImportData', {
            method: 'POST',
            body: formData,
        })
            .then(async function (response) {
                return await response.text();
            })
            .then(function (data) {
                $$.loading(false);
                if (data) {
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                } else {
                    showToast.error(RMResx.RM_SPS_Location_SaveImportFailed);
                }
                // return result;
            }).catch((e) => {
                $$.loading(false);
                showToast.error(RMResx.RM_SPS_Location_SaveImportFailed);
            });
        this.setState({ showImportPanel: { show: false } });
    }

    handleDownloadSaveClick = (e) => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }

        let suiteId = this.state.suiteId;
        this.handleDownloadLocal(suiteId);
        this.setState({ showDownloadTemplatePanel: { show: false } });
    }

    handleDownloadLocal = (value) => {
       
        let selectedSuiteId = value;
        var $suiteId = $("#suiteId");
        $suiteId.val(selectedSuiteId);

        $("#explore-form-download")
            .attr("action", "/api/PhysicalRecordsBulkImportApi/DownloadTemplate")
            .submit();
    }

    handleImportCancelClick = (e) => {
        this.setState({ showImportPanel: { show: false } });
    }

    handleDownloadCancelClick = (e) => {
        this.setState({ showDownloadTemplatePanel: { show: false } });
    }

    handleExploreImport() {
        this.setState({
            showImportPanel: { show: true },
            skipExistObjChecked: true
        });
    }

    handleBulkExportUpdate() {
        this.setState({
            showBulkExportUpdatePanel: { show: true },
        });
    }

    handleSettingClick() {
        this.setState({
            showSettingDialog: { show: true },
        });
    }

    handleCancelSettingClick() {
        this.setState({
            barcodeStandard: this.barcodeStandardRef,
            showBarcodeMessage: this.barcodeStandardRef.find((item) => item.checked)?.key === 1,
            showSettingDialog: { show: false },
        });
    }

    handleSettingChangedClick = (args) => {
        let currentSetting = RM.deepcopy(this.state.barcodeStandard);
        currentSetting.forEach((item) => {
            item.checked = item.key === args.newValue.key;
        });
        this.setState({
            barcodeStandard : currentSetting,
            showBarcodeMessage : args.newValue.key === 1
        });
    }

    handleSettingSaveClick = async() => {
        $$.loading(true);
        let currentSetting = RM.deepcopy(this.state.barcodeStandard);
        let checkedType = currentSetting.find((item) => item.checked);
        let selectedValue = checkedType ? checkedType.key : "0";
        let result = await fetchUtility({  url: "/api/PhysicalRecordApi/SaveBarcodeStandard", data: selectedValue });
        $$.loading(false);
        if(result){
            showToast.success(RMResx.RM_PRM_PRE_Setting_SaveSuccessful);
            this.initBarcodeSetting();
            this.setState({
                barcodeStandard: currentSetting,
                showBarcodeMessage: checkedType.key === 1,
                showSettingDialog: { show: false },
            });
            this.barcodeStandardRef = currentSetting,
            this.initCurrentPhysicalObjectInfo(this.selectedTreeItem);
            this.dispatch('PhyObjectInfo', 'reset', this.selectedTreeItem);
        }else{
            showToast.error(RMResx.RM_PRM_PRE_Setting_SaveFailed);
        }
    }

    openDownloadPanel = (e) => {
        this.setState({ showDownloadTemplatePanel: { show: true } });
        this.loadSuites();
    }

    onTemplateSelectedChange = (args) => {
        this.setState({ suiteId: args.newValue.UniqueId });
    }

    loadSuites() {
        $$.loading(true);
        let urlData = "/api/TemplateManagementApi/GetAllSimplifySuites";
        let option = {
            url: urlData,
            method: "Post"
        };
        fetchUtility(option).then((res) => {
            let data = res; //JSON.parse(res);
            data.forEach(match => {
                match.Name = RMResx[match.Name] ? RMResx[match.Name] : match.Name;
            });
            this.setState({ suiteList: data });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onBulkExportUpdateCancelClick = () => {
        this.setState({ showBulkExportUpdatePanel: { show: false } });
    }

    renderSettingDialog = () => {
        return <R.Dialog
            id="PhyMoveHoldConflictDialog"
            header={RMResx.RM_PRM_PRE_Setting}
            width={500}
            height={310}
            status={{ show: this.state.showSettingDialog.show }}
            struct={{ foot: true }}
            onHide={this.handleCancelSettingClick.bind(this)}
            destroy={true}
        >
            <div>
                <$g.FormRow label={RMResx.RM_PRM_PRE_BarcodeStandard} key="h1" id="ariaBarcodeStandard">
                    <R.Combobox
                        checkedField="checked"
                        textField="value"
                        valueField="key"
                        width={"100%"}
                        aria="#ariaBarcodeStandard"
                        hasFilter={false}
                        searchable={false}
                        items={this.state.barcodeStandard}
                        onChange={this.handleSettingChangedClick.bind(this)}
                    />
                    {this.state.showBarcodeMessage && <div style={{ marginTop : '8px' }}>{RMResx.RM_PRM_PRE_BarcodeMessage}</div>}
                </$g.FormRow>
            </div>
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleSettingSaveClick.bind(this)} />
        </R.Dialog>;
    }

    renderPhyMoveHoldConflictDialog() {
        return <R.Dialog
            id="PhyMoveHoldConflictDialog"
            header={RMResx.RM_PRM_Hold_Conflicted_FormTitle}
            width={500}
            status={{ show: this.state.physicalMoveHoldConflictDialogShow }}
            struct={{ foot: true }}
            onHide={this.onCancelPhyMoveHoldConflict.bind(this)}
            destroy={true}
        >
            <div>
                {/* TODO */}
                {/* <R.Radio.Group
                    name="radiogroup-type"
                    items={this.state.moveHoldConflictOptions}
                    onChange={this.onPhysicalMoveHoldConflict}
                /> */}

                <$g.FormRow label={RMResx.RM_PRM_Move_Hold_Conflicted_OptionHeader} key="h1">
                    <$g.RadioGroup
                        name="phy-move-conflict-resolution-type"
                        onChange={this.onPhysicalMoveHoldConflict.bind(this)}
                        value={this.state.selectedMoveHoldConflict}>
                        <$g.RadioOption value="1" text={RMResx.RM_PRM_Move_Hold_Conflicted_OverrideByDestination} />
                        <$g.RadioOption value="2" text={RMResx.RM_PRM_Move_Hold_Conflicted_Compare} />
                    </$g.RadioGroup>
                </$g.FormRow>
            </div>
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_OK} onClick={this.sendMoveRequestWithConflictResolution} />
        </R.Dialog>;
    }

    renderManagePermissionPanel() {
        return <R.Panel
            id="raPhyManagePermissionPanel"
            header={RMResx.RM_PRM_PRE_ManagePermission}
            size={600}
            status={{ show: this.state.showManagePermissionPanel }}
            onHide={this.hideManagePermissionPanel}
            destroy={true}
        >
            <div>
                <PhyObjectManagePermission
                    id='raPhyObjectManagePermission'
                    data={this.state.selectedPhyObjByTableOrTree}
                ></PhyObjectManagePermission>
            </div>
            <>
                <R.Button slot="buttons" id="raPhyManagePermissionPanelCancleBtn" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showManagePermissionPanel: false });
                }} />
                <R.Button slot="buttons" id="raPhyManagePermissionPanelSaveBtn" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSavePermission} />
            </>
        </R.Panel>;
    }

    renderTermPieContent() {
        if (this.state.viewMode == 0 && this.state.termUsageData.length > 0) {
            return <div className='raPhyObjInfoContainer'>
                <div id="recordsExplorer_pie_info">
                    <div >
                        <p>{RMResx.RM_PRM_PRE_TermViewSubTitleDesc} </p>
                    </div>
                    <R.Charts height={200}>
                        <R.Charts.Pie
                            type="donut"
                            items={this.state.termUsageData}
                            thickness="20"
                        />
                    </R.Charts>
                </div>
            </div>;
        } else {
            return "";
        }
    }

    renderImportPanel() {
        return <R.Panel
            header={RMResx.RM_TM_ImportDialogTitle}
            size={670}
            status={this.state.showImportPanel}
            destroy={true}
        >
            <div id="importSettingPanel">
                <R.Validation>
                    <div ref={r => this.allValidation = r}>
                        <div className="tm-import-download">
                            <span className="tm-import-download-span" onClick={this.openDownloadPanel} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_JS_TM_DownLoadTemplate}</span>
                        </div>
                        <div>
                            <div className="tm-import-title" tabIndex="0">
                                <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)} />
                            </div>
                            <div>
                                <R.Validation
                                    element="Uploader"
                                    require={RMResx.RM_SPS_Location_NoImportFile}>
                                    <R.Uploader
                                        ref={this.uploaderRef}
                                        files={this.state.files}
                                        fileTypes={["XLSX"]}
                                        onUpload={this.handleUpload.bind(this)}
                                        onDelete={this.handleDelete.bind(this)}
                                        multiple={false}
                                    />
                                </R.Validation>
                            </div>
                        </div>
                        <div className="ra-explorer-import">
                            <div className="require ra-explorer-import-uniqueid" tabIndex="0">{RMResx.RM_PRM_PRE_Import_SameUniqueIdTitle}</div>
                            <R.Validation
                                element="Radio.Group"
                                require>
                                <R.Radio.Group
                                    block={true}
                                    name="existSameUniqueId"
                                    items={this.getRadioUniqueIdColumn()}
                                    onChange={this.handleExistSameUniqueId}
                                />
                            </R.Validation>
                        </div>
                        <div className="ra-explorer-import">
                            <div className="require ra-explorer-import-uniqueid" tabIndex="0">{RMResx.RM_PRM_PRE_Import_OverrideTime}</div>
                            <R.Validation
                                element="Radio.Group"
                                require>
                                <R.Radio.Group
                                    block={true}
                                    name="enableCustomTimeId"
                                    items={this.getRadioTimeColumn()}
                                    onChange={this.handleEnableCustomTime}
                                />
                            </R.Validation>
                        </div>
                    </div>
                </R.Validation>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleImportCancelClick} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleImportSaveClick} />
            </>
        </R.Panel>;
    }

    renderDownloadTemplatePanel() {
        let requestVerificationToken = getRequestVerificationToken();
        return <R.Panel
            header={RMResx.RM_JS_TM_DownLoadTemplate}
            size={670}
            status={this.state.showDownloadTemplatePanel}
            actionType={"back"}
            destroy={true}
            onHide={this.handleDownloadCancelClick}
        >
            <div id="downloadTemplatePanel">
                <R.Validation>
                    <div ref={r => this.allValidation = r}>
                        <div className="ra-explorer-download">
                            <form id="explore-form-download" method="POST" action="">
                                <input type="hidden" id="suiteId" name="suiteId" value="" />
                                <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                            </form>
                            <span className="ra-explorer-download-span" tabIndex="0">{RMResx.RM_PRM_PRE_Download_Tips}</span>
                        </div>
                        <div>
                            <div className="require ra-explorer-templateTitle" tabIndex="0">{RMResx.RM_PRM_PRE_Download_TemplateTitle}</div>
                            <div>
                                <R.Validation
                                    element="Combobox"
                                    require={RMResx.RM_PRM_PRE_Download_NoSelectTemplate}
                                >
                                    <R.Combobox
                                        items={this.state.suiteList}
                                        tooltipField="Name"
                                        width='100%'
                                        textField="Name"
                                        valueField="UniqueId"
                                        checkedField="Checked"
                                        linkMode={false}
                                        searchable={false}
                                        onChange={this.onTemplateSelectedChange} />
                                </R.Validation>
                            </div>
                        </div>
                    </div>
                </R.Validation>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleDownloadCancelClick} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleDownloadSaveClick} />
            </>
        </R.Panel>;
    }

    renderBulkExportUpdatePanel() {
        return <R.Panel
            header={RMResx.RM_PRM_PRE_BulkUpdate}
            size={670}
            status={this.state.showBulkExportUpdatePanel}
            destroy={true}
        >
            <div id="bulkUpdatePanel">
                <R.Validation>
                    <div ref={r => this.allValidation = r}>
                        <div className="tm-bulkupdate-export">
                            <$g.I18NProvider msg={RMResx.RM_PRM_PRE_BulkUpdate_Msg}>
                                <span className="tm-bulkupdate-export-span" onClick={this.onShowExportRecordsDialogClick} onKeyDown={this.onKeyDown} tabIndex="0">{RMResx.RM_PRM_PRE_BulkUpdate_ExportRecords}</span>
                            </$g.I18NProvider>
                        </div>
                        <div>
                            <div className="tm-import-title" tabIndex="0">
                                <$g.I18NProvider msg={RMResx.RM_PRM_PRE_BulkUpdate_ImportFile} />
                            </div>
                            <div>
                                <R.Validation
                                    element="Uploader"
                                    require={RMResx.RM_PRM_PRE_BulkUpdate_NoImportFileError}>
                                    <R.Uploader
                                        ref={this.uploaderRef}
                                        files={this.state.bulkUpdateFiles}
                                        fileTypes={["CSV"]}
                                        onUpload={this.handleBulkUpdateUpload.bind(this)}
                                        onDelete={this.handleBulkUpdateDelete.bind(this)}
                                        multiple={true}
                                    />
                                </R.Validation>
                            </div>
                        </div>
                    </div>
                </R.Validation>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onBulkExportUpdateCancelClick} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onBulkExportUpdateSaveClick} />
            </>
        </R.Panel>;
    }

    renderExportRecordsDialog() {
        return <R.Dialog
            id="raExportRecordsDialog"
            header={RMResx.RM_PRM_PRE_BulkUpdate_ExportRecords_DialogTitle}
            width={480}
            status={{ show: this.state.showExportRecordsDialog }}
            struct={{ foot: true }}
            onHide={this.onCancelExportRecordsDialog.bind(this)}
            destroy={true}
        >
            <div id="raExportRecordsDialogContent">
                <R.Validation>
                    <div ref={r => this.allValidation = r}>
                        <div className="ra-export-msg" tabIndex="0">{RMResx.RM_PRM_PRE_BulkUpdate_ExportRecords_DialogMsg}</div>
                        <div>
                            <div className="ra-export-template-title require">{RMResx.RM_PRM_PRE_BulkUpdate_ExportRecords_TemplateTitle}</div>
                            <div>
                                <R.Validation
                                    element="Multicombobox"
                                    require={RMResx.RM_AR_CP_Common_SelEmpty}
                                >
                                    <R.Multicombobox
                                        id="raTemplate"
                                        width="100%"
                                        items={this.state.templateList}
                                        tooltipField="Name"
                                        textField="Name"
                                        valueField="UniqueId"
                                        checkedField="checked"
                                        groupField="group"
                                        onChange={this.onTemplateChange}
                                        aria={{ ariaLabel: RMResx.RM_PRM_PRE_BulkUpdate_ExportRecords_TemplateTitle }}
                                    />
                                </R.Validation>
                            </div>
                        </div>
                    </div>
                </R.Validation>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelExportRecordsDialog.bind(this)} />
                <R.Button slot="buttons" id="raPhyExportRecordsBtn" primary classify="theme" text={RMResx.RM_MA_Export} onClick={this.onExportRecords.bind(this)} />
            </>
        </R.Dialog>;
    }

    render() {
        let phyObj = this.state.currentPhyObj;
        return this.state.viewModeLoaded &&
            <div id='raRecordsExplorer' className="ra-explorer-main-container">
                <section>
                    <$g.SiteMap data={[SiteMapLinks.PRM_RecordsExplorer]} />
                    <R.Messagebar
                        message={this.state.tipMsg}
                        classify={this.state.tipType}
                        status={{ show: this.state.showTip }}
                        onClose={this.hideMessageTip}
                    />
                    <div className="container-menu margin-top-m">
                        {phyObj != null && this.renderCurrentPhyObjNavBar()}
                    </div>
                </section>
                <section className="rm-tm-content-main">
                    <div className={"rm-tm-content-left"} id="raExplorerTreeContainer">
                        <div className="view-mode-container">
                            <R.TabButton
                                active={this.state.viewMode}
                                // disabled={this.state.disabled}
                                items={[
                                    {
                                        text: RMResx.RM_PRM_PRE_TermView
                                    }, {
                                        text: RMResx.RM_PRM_PRE_LocationView
                                    }
                                ]}
                                onChange={this.onViewModeTabChange.bind(this)}
                            />
                            <PhysicalObjectStatusLegend />
                        </div>
                        {this.state.viewMode == TreeViewMode.Location &&
                            <div id='raExplorerTree' className="left-tree">
                                <PhysicalExplorerTree
                                    ref={r => this.refExplorerTree = r}
                                    data={this.state.treeData}
                                    rootSelectedDefault={true}
                                    onSelectedNodeChanged={this.onSelectedNodeChanged}>
                                </PhysicalExplorerTree>
                            </div>
                        }
                        {
                            this.state.viewMode == TreeViewMode.Term &&
                            <div id='raTermViewTree' className="left-tree">
                                <PhyExplorerTermView
                                    ref={r => this.refTermTree = r}
                                    id="explorerTermViewTree"
                                    onSelectedNodeChanged={this.onTermViewSelectedNodeChanged}
                                />
                            </div>
                        }
                    </div>
                    <div className={"rm-tm-content-right"}>
                        {
                            phyObj !== null && phyObj.NodeType !== 9000 ?
                                this.renderCurrentPhyObjContent(phyObj) :
                                <PhysicalReport isTermView={this.state.viewMode === TreeViewMode.Term} />
                        }
                    </div>
                </section>
                {this.renderExportBarcodesDialog()}
                {this.renderFormPanel()}
                {this.renderLoanPanel()}
                {this.renderMovePanel()}
                {this.renderFilterPanel()}
                {this.renderViewDetailPanel()}
                {this.renderReclassifyPanel()}
                {this.renderHoldPanel()}
                {this.renderRelatedRecordsPanel()}
                {this.renderRemoveHoldDialog()}
                {this.renderRemoveHoldSelectDialog()}
                {this.renderRemovePersonHoldDialog()}
                {this.renderBoxHoldDialog()}
                {this.renderManagePermissionPanel()}
                {this.renderImportPanel()}
                {this.renderDownloadTemplatePanel()}
                {this.renderBulkUpdateFormPanel()}
                {this.renderBulkExportUpdatePanel()}
                {this.renderExportRecordsDialog()}
                {this.renderSettingDialog()}
                {this.renderAuditPanel()}
                {this.renderPhyMoveRequestPanel()}
                <div id='downloadDiv' style={{ display: "none" }} />
            </div>;
    }
}
