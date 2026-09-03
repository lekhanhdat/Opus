import { bindEvents, getRequestVerificationToken, showToast, GetExportResultCountLimit, LicenseHelper } from '../../../Utilities/CommonUtil';
import RouterUrls from "../../../Constants/RouterUrls";
import { NodeLevel, NodeType } from "../../../Constants/DAEnums";
import { ElecStatusEnum } from '../../BCM/Constants';
import { checkPermission } from '../../../Utilities/permissionManager';
import ElecMoveForm from "../../BCM/ElecRecordsExplorer/ElecMoveForm";
import ElecDetailForm from "../../BCM/ElecRecordsExplorer/ElecDetailForm";
import ElecReclassifyForm from "../../PRM/RecordsExplorer/Components/PhyReclassify";
import ElecRelatedForm from "../../PRM/RecordsExplorer/Components/PhyRelatedRecords";
import PhyObjectManagePermission from '../../PRM/RecordsExplorer/Components/PhyObjectManagePermission';
import PhyObjectMove from "../../PRM/RecordsExplorer/Components/PhyObjectMove";
import PhyMovementRequest from "../../PRM/RecordsExplorer/Components/PhyMovementRequest";
import PhyObjectForm from "../../PRM/Common/PhyObjectForm";
import PhyLoanRequest from "../../PRM/RecordsExplorer/Components/PhyLoanRequest";
import HoldForm from "../../PRM/RecordsExplorer/ManageHold/PhyRecordHoldForm";
import PhyObjectDetail from '../../PRM/Common/PhyObjectDetail';
import { PhyObjFormType } from "../../PRM/RecordsExplorer/RecordsExplorer";
import { RoleType, SourceFlags, DefaultExportLimit, EmptyGUID } from '../../../Constants/Constants';
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import StringUtil from "../../../Utilities/StringUtil";
import "../../../Less/PRM/hybridSearch.less";
import PhyObjBulkUpdateForm from '../../PRM/Common/PhyObjBulkUpdateForm';
import ShowAuditInfoPanel from "../../PRM/RecordsExplorer/Components/PhyObjectAuditInfo";

import {
    PhysicalObjectStatus,
    PhysicalDefaultColumnIDs,
} from "../../../Constants/Constants";
import Enviroments from '../../../Constants/Enviroments';
import GoogleReclassify from '../../PRM/RecordsExplorer/Components/GoogleReclassify';
import { ChangeTermOrigin } from '../../RDM/ManualApproval/Constants/ManualReviewActions';
import { AuditModule } from './Constants';

const TableNavBarBtnInMore = {
    phyDelete: {
        id: 2,
        name: RMResx.RM_PRM_PRE_Delete
    },
    allowShowAudit: {
        id: 3,
        name: RMResx.RM_PRM_PRE_ShowAudit
    }
};
let limitCount = 10;
const GlobalSearchAction = {
    None: 0,
    Reclassify: 1,
    MoveTo: 2,
    DeclareRecords: 3,
    UnDeclareRecords: 4,
    AccessControl: 5,
    PhysicalBulkUpdate : 6
};

const SearchExportStatus = {
    None: 0,
    InProgress: 1,
    Finished: 2
};
export default class HSActions extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            selectedItems: [],
            showReclassifyPanel: { show: false },
            showRelatedPanel: { show: false },
            showPermissionPanel: { show: false },
            showAuditPanel : false,
            showMovePanel: { show: false },
            showPhyMovePanel: { show: false },
            showPhyMoveRequestPanel: { show: false },
            showFormPanel: { show: false },
            showLoanPanel: { show: false },
            showHoldPanel: { show: false },
            showRemoveHoldDialog: { show: false },
            removeHoldSelectDialogShow: { show: false },
            showViewDetailPanel: { show: false },
            showPhyViewDetailPanel: { show: false },
            removePersonHoldDialogShow: false,
            tableNavBarBtnAllow: this.initTableNavBarBtnAllow(),
            holdType: "",
            holdFormParam: {},
            phyObjMoveParam: {},
            phyObjLoanParam: {},
            viewDetailParam: {},
            phyObjDetailParam: {},
            smallNodeType: NodeType.PhyBox,
            selectedMoveHoldConflict: "",
            phyObjFormData: {},
            releaseTimesForRemoveHold: [],
            isOverWriteSubFiles: false,
            isReclassifySubFiles: false,
            isCurrentViewChecked: true,
            isExportToBrowser: true,
            exportDialogShow: { show: false },
            removeHoldProfileList: [],
            showBulkUpdateFormPanel: { show: false },
            phyObjBulkUpdateData: {},
            formTemplateName: "",
            connectorDetail: [],
            showCountButton: window.devicePixelRatio >= 1.5 ? 1 : 2,
            canShowHoldByRecordsPermission: true,
        };
        bindEvents(this, "showMessageTip", "onSaveReclassify", "showFSFolderHoldOption", "onSaveRelated", "onSavePhyMove", "onDeletePhyObj", "onSaveMove", "onSavePhyObj", "routerToExplorer");
        this.peFormId = "explorerPhyObjectForm";
        this.peLoanId = "explorerLoanRequest";
        this.peBulkUpdateFormId = "explorerBulkUpdateForm";
        this.isPhysicalAdmin = RM.gData.isPhysicalAdmin;
        this.isPhysicalEndUser = RM.RoleType == RoleType.StandardUser;
        this.isStandardReviewUser = RM.RoleType == RoleType.StandardReviewUser;
        this.isDelegateAdmin = RM.RoleType == RoleType.DelegateAdmin;
        this.isSupAdmin = RM.RoleType == RoleType.SupAdmin;
        this.isHoldManagerOnly = RM.RoleType == RoleType.ManageHoldUser;
        this.roleTypeValue = Number(RM.RoleType);
        this.isRoleAdmin = this.roleTypeValue === 1 || this.isPhysicalAdmin;
        this.isRoleDelegateAdmin = this.roleTypeValue === 2;
        this.isRoleHoldManager = this.roleTypeValue === 5 || this.isHoldManagerOnly;
        this.hasManageHold = RM.gData.hasManageHold;
        this.canUseRecordsPermissionApi = this.isRoleDelegateAdmin && this.hasManageHold;

        this.holdPermissionQueryKey = "";
        this.holdPermissionCache = {};
        this.holdPermissionPageKey = "";
        this.connectorRecordMinSourceFlag = 1000;
        this.exportTypes = {
            Browser: "0",
            Location: "1"
        };
        this.exportLimitCount = GetExportResultCountLimit();
        this.exportUniqueId = null;
        this.isNewLogicAccount = LicenseHelper.EnableRecordsArchiver();
        this.is21VEnv = LicenseHelper.Is21VEnv();
        this.isLockableSource = false;
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "onCheckChanged":
                this.dataChanged(...args);
                break;
            case "showDetails":
                this.onViewDetail(...args);
                break;
            case "SET_LIMIT_COUNT":
                limitCount = args[0];
                break;
            case "onPageDataLoaded":
                this.onPageDataLoaded(...args);
                break;
        }
        window.matchMedia(`(resolution: ${window.devicePixelRatio}dppx)`)
        .addEventListener('change', () => {
            this.state.showCountButton = window.devicePixelRatio >= 1.5 ? 1 : 2;
        });
    }

    componentDestroy() {
        this.dispatch('raNotificationMenu', 'close');
        this.dispatch('raNotification', []);
        this.dispatch('rmSuiteBar', true);
        window.removeEventListener('resize', () => {
            this.state.showCountButton = window.devicePixelRatio >= 1.5 ? 1 : 2;
        });
    } 

    getRadioExportColumn() {
        return [
            { text: RMResx.RM_HS_Export_CurrentView, value: true, checked: this.state.isCurrentViewChecked },
            { text: RMResx.RM_HS_Export_AllColumn, value: false, checked: !this.state.isCurrentViewChecked },
        ];
    }

    getViewDetailPanelBtns() {
        if (this.state.phyObjDetailParam && this.state.phyObjDetailParam.nodeType == NodeLevel.PhyBox) {
            return <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Close}
                    onClick={() => {
                        this.setState({
                            showViewDetailPanel: { show: false },
                            showPhyViewDetailPanel: { show: false },
                        });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_PRM_Button_ViewSubFolders}
                    primary={true}
                    classify="theme"
                    onClick={this.routerToExplorer}
                />
            </>
        } else {
            return <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Close}
                primary={true}
                classify="theme"
                onClick={() => {
                    this.setState({ showPhyViewDetailPanel: { show: false } });
                }}
            />
        }
    }

    getViewDetailDownloadBtn() {
        //8 Archived
        if (this.recordData && this.recordData.RecordStatus == 8) {
            return (
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Close}
                        onClick={() => {
                            this.setState({ showViewDetailPanel: { show: false } });
                        }}
                    />
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_DC_DownloadBtn}
                        primary={true}
                        classify="theme"
                        onClick={() => {
                            this.onDownloadFile(this.detailRecordData);
                        }}
                    />
                </>
            )
        } else {
            return <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Close}
                primary={true}
                classify="theme"
                onClick={() => {
                    this.setState({ showViewDetailPanel: { show: false } });
                }}
            />
        }
    }

    reclassifyPanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showReclassifyPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSaveReclassify}
                />
            </>
        );
    }

    relatedPanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showRelatedPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSaveRelated}
                />
            </>
        );
    }

    managePermissionPanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showPermissionPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSavePermission.bind(this)}
                />
            </>
        );
    }

    movePanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showMovePanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSaveMove}
                />
            </>
        );
    }

    phyMovePanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showPhyMovePanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSavePhyMove}
                />
            </>
        );
    }

    formPanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showFormPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSavePhyObj}
                />
            </>
        );
    }

    bulkUpdatePanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showBulkUpdateFormPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSaveBulkUpdatePhyObj.bind(this)}
                />
            </>
        );
    }

    loanPanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showLoanPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSaveLoanObj.bind(this)}
                />
            </>
        );
    }

    holdPanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showHoldPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.showFSFolderHoldOption}
                />
            </>
        );
    }

    getHoldPermissionQueryKey(searchOption) {
        try {
            return JSON.stringify(searchOption || []);
        } catch (error) {
            return "";
        }
    }

    resetHoldPermissionCacheIfNeeded(searchOption) {
        const key = this.getHoldPermissionQueryKey(searchOption);
        if (this.holdPermissionQueryKey !== key) {
            this.holdPermissionQueryKey = key;
            this.holdPermissionCache = {};
        }
    }

    getPermissionCacheKey(recordId, sourceFlag) {
        return `${recordId}#${sourceFlag}`;
    }

    getSelectedRecordIds(selectedTableItems) {
        return (selectedTableItems || [])
            .filter(item => !!item && !!item.Id && item.SourceFlag !== undefined && item.SourceFlag !== null)
            .map(item => this.getPermissionCacheKey(item.Id, item.SourceFlag));
    }

    buildPermissionRequestItems(pageItems) {
        return (pageItems || [])
            .filter(item => !!item && !!item.Id && item.SourceFlag !== undefined && item.SourceFlag !== null)
            .map(item => ({
                recordId: item.Id,
                ContentSource: item.SourceFlag,
                permissionKey: this.getPermissionCacheKey(item.Id, item.SourceFlag)
            }));
    }

    parseHoldPermissionResult(result, permissionKeys) {
        let data = result;
        const permissionMap = {};

        if (typeof data === "string") {
            try {
                data = JSON.parse(data);
            } catch (error) {
                return permissionMap;
            }
        }

        if (Array.isArray(data)) {
            data.forEach((item, index) => {
                const fallbackKey = permissionKeys[index];
                const recordId = item && (item.RecordId || item.recordId || item.Id);
                const contentSource = item && (item.ContentSource ?? item.contentSource ?? item.SourceFlag ?? item.sourceFlag);
                const recordKey = (recordId && contentSource !== undefined && contentSource !== null)
                    ? this.getPermissionCacheKey(recordId, contentSource)
                    : fallbackKey;
                const allow = !!(item && item.HasDelegatedAdmin === true);
                if (recordKey) {
                    permissionMap[recordKey] = allow;
                }
            });
            return permissionMap;
        }

        if (typeof data === "boolean") {
            permissionKeys.forEach(key => { permissionMap[key] = data; });
            return permissionMap;
        }

        if (data && typeof data === "object") {
            const sharedAllow = data.HasDelegatedAdmin === true;
            permissionKeys.forEach(key => { permissionMap[key] = sharedAllow; });
        }

        return permissionMap;
    }

    applyDelegatePermissionToSelected(selectedIds) {
        if (selectedIds.length === 0) {
            this.setState({ canShowHoldByRecordsPermission: true }, () => {
                this.refTopButtons && this.refTopButtons.updateButtons(this.getShowActions());
            });
            return;
        }

        const allAllowed = selectedIds.every(id => this.holdPermissionCache[id] === true);
        this.setState({ canShowHoldByRecordsPermission: allAllowed }, () => {
            this.refTopButtons && this.refTopButtons.updateButtons(this.getShowActions());
        });
    }

    getPagePermissionKey(searchOption, pageIndex) {
        const queryKey = this.getHoldPermissionQueryKey(searchOption);
        return `${queryKey}|${pageIndex}`;
    }

    onPageDataLoaded(pageItems, searchOption, pageIndex) {
        if (this.isRoleAdmin || this.isRoleHoldManager || !this.canUseRecordsPermissionApi) {
            return;
        }

        const pageKey = this.getPagePermissionKey(searchOption, pageIndex);
        if (this.holdPermissionPageKey === pageKey) {
            return;
        }
        this.holdPermissionPageKey = pageKey;

        const permissionItems = this.buildPermissionRequestItems(pageItems);
        const permissionKeys = permissionItems.map(item => item.permissionKey);

        if (permissionItems.length === 0) {
            this.holdPermissionCache = {};
            this.applyDelegatePermissionToSelected(this.getSelectedRecordIds(this.props.getSelectedItems()));
            return;
        }

        const option = {
            url: "/api/RecordsExplorerApi/GetRecordsPermission",
            method: "POST",
            data: permissionItems.map(item => ({
                recordId: item.recordId,
                ContentSource: item.ContentSource
            }))
        };

        fetchUtility(option).then((result) => {
            const resultMap = this.parseHoldPermissionResult(result, permissionKeys);
            const newCache = {};
            permissionKeys.forEach(key => {
                newCache[key] = resultMap[key] === true;
            });
            this.holdPermissionCache = newCache;
            this.applyDelegatePermissionToSelected(this.getSelectedRecordIds(this.props.getSelectedItems()));
        }).catch(() => {
            const newCache = {};
            permissionKeys.forEach(key => {
                newCache[key] = false;
            });
            this.holdPermissionCache = newCache;
            this.applyDelegatePermissionToSelected(this.getSelectedRecordIds(this.props.getSelectedItems()));
        });
    }

    updateHoldPermissionBySelection(selectedTableItems, searchOption) {
        if (this.isRoleAdmin || this.isRoleHoldManager) {
            this.setState({ canShowHoldByRecordsPermission: true });
            return;
        }

        if (!this.canUseRecordsPermissionApi) {
            this.setState({ canShowHoldByRecordsPermission: true });
            return;
        }

        this.resetHoldPermissionCacheIfNeeded(searchOption);

        const selectedIds = this.getSelectedRecordIds(selectedTableItems);
        this.applyDelegatePermissionToSelected(selectedIds);
    }

    dataChanged(isSelectResult, selectedTableItems, searchOption, isSelectedOneSourceFilter, canDoPhysicalBulkUpdate) {
        this.isSelectResult = !!isSelectResult;
        this.searchOption = searchOption || [];
        this.isSelectedOneSource = isSelectedOneSourceFilter;
        this.canDoPhysicalBulkUpdate = canDoPhysicalBulkUpdate;
        this.updateHoldPermissionBySelection(selectedTableItems, this.searchOption);

        if (this.isHoldManagerOnly) {
            let tableNavBarBtnAllow = this.state.tableNavBarBtnAllow;
            for (let key in tableNavBarBtnAllow) {
                tableNavBarBtnAllow[key] = false;
            }
            tableNavBarBtnAllow.holdManagement = false;

            this.setState({
                tableNavBarBtnAllow: Object.assign({}, tableNavBarBtnAllow),
            }, () => {
                this.refTopButtons.updateButtons([]);
            });
            return;
        }

        let isCrossDataSource = false;
        let oneItemSource;
        if (selectedTableItems && selectedTableItems.length > 0) {
            oneItemSource = selectedTableItems[0].SourceFlag;
            for (let index = 0; index < selectedTableItems.length; index++) {
                const element = selectedTableItems[index];
                if (element.SourceFlag != oneItemSource) {
                    isCrossDataSource = true;
                }
            }
        }
        this.currentSelectedItemDataSource = oneItemSource;
        let tableNavBarBtnAllow = this.state.tableNavBarBtnAllow;
        for (let key in tableNavBarBtnAllow) {
            tableNavBarBtnAllow[key] = false;
        }
        let archivedRecordSelected = selectedTableItems.some((item) => { return item.RecordStatus == 8; }); //8 Archived
        //let connectorRecordSelected = selectedTableItems.some((item) => { return item.SourceFlag >= this.connectorRecordMinSourceFlag; }); // more than 1000 is connector
        if (isCrossDataSource || (this.isSelectResult && !isSelectedOneSourceFilter) || archivedRecordSelected) {
            tableNavBarBtnAllow["holdManagement"] = true;
            this.setState({
                tableNavBarBtnAllow: Object.assign({}, tableNavBarBtnAllow),
            }, () => {
                let showButtons = this.getShowActions();
                this.refTopButtons.updateButtons(showButtons);
            });
            return;
        }
        if (oneItemSource == 4) {
            let selItemsCount = selectedTableItems.length;
            if (selItemsCount != 0) {
                if (this.isAllowLoan(selectedTableItems)) {
                    tableNavBarBtnAllow['phyLoan'] = true;
                }
                if (this.isAllowMoveRequest(selectedTableItems)) {
                    tableNavBarBtnAllow['phyMoveRequest'] = true;
                }
                if (this.isAllowEdit(selectedTableItems)) {
                    tableNavBarBtnAllow['phyEdit'] = true;
                }
                if (this.isAllowDelete(selectedTableItems)) {
                    tableNavBarBtnAllow['phyDelete'] = true;
                }
                if (this.isAllowMove(selectedTableItems)) {
                    tableNavBarBtnAllow['move'] = true;
                }
                if (this.isAllowReclassify(selectedTableItems)) {
                    tableNavBarBtnAllow['reclassify'] = true;
                }
                if (this.isAllowRemovePersonHold(selectedTableItems)) {
                    tableNavBarBtnAllow['del_personal_hold'] = true;
                }
                if (this.isAllowRelated(selectedTableItems)) {
                    tableNavBarBtnAllow['related'] = true;
                }
                if (this.isAllowSetPermissions(selectedTableItems)) {
                    tableNavBarBtnAllow['allowSetPermissions'] = true;
                }
                if(this.isAllowShowAudit(selectedTableItems)){
                    tableNavBarBtnAllow['allowShowAudit'] = true;
                }
                if (this.isAllowMore(tableNavBarBtnAllow)) {
                    tableNavBarBtnAllow['more'] = true;
                }
            } else {
                tableNavBarBtnAllow["holdManagement"] = false;
            }
        } else {
            let selItemsCount = selectedTableItems.length;
            let exoItemsCount = 0;
            let spItemsCount = 0;
            let spFoldersCount = 0;
            let fsItemsCount = 0;
            let fsFoldersCount = 0;
            let splItemsCount = 0;
            let splFoldersCount = 0;
            let onedriveItemsCount = 0;
            let onedriveFoldersCount = 0;
            let declareAsRecordCount = 0;
            let unDeclareAsRecordCount = 0;
            let lockWithRecordsLabelCount = 0;
            let unlockWithRecordsLabelCount = 0;
            let azureFileShareDirectoryCount = 0;
            let azureFileShareFileCount = 0;
            let boxDirectoryCount = 0;
            let boxFileCount = 0;
            let googleFileCount = 0;
            let googleFolderCount = 0;
            let connectorItemsCount = 0;
            let containerSPListItem = false;
            let teamsFileCount = 0;
            let teamsFolderCount = 0;
            for (let item of selectedTableItems) {
                if (item.NodeType == 7002) {
                    azureFileShareDirectoryCount++;
                }
                if (item.NodeType == 7003) {
                    azureFileShareFileCount++;
                }
                if (item.NodeType == 400 && item.SourceFlag == SourceFlags.OneDrive) {
                    onedriveFoldersCount++;
                }
                if (item.NodeType == 400 && item.SourceFlag == SourceFlags.SP) {
                    spFoldersCount++;
                }
                if (item.NodeType == 500 && item.SourceFlag == SourceFlags.SP) {
                    spItemsCount++;
                }
                if (item.NodeType == 5110) {
                    exoItemsCount++;
                }
                if (item.NodeType == 2200) {
                    fsItemsCount++;
                }
                if (item.NodeType == 2100) {
                    fsFoldersCount++;
                }
                if (item.DeclareAsRecord) {
                    declareAsRecordCount++;
                } else {
                    unDeclareAsRecordCount++;
                }
                if (item.LockedByRecordLabel) {
                    lockWithRecordsLabelCount++;
                } else {
                    unlockWithRecordsLabelCount++;
                }
                if (item.ExtensionForFile == RMResx.RM_RDM_RecordDetails_DataType_SPItem) {
                    containerSPListItem = true;
                }
                if (item.NodeType == 500 && item.SourceFlag == SourceFlags.SPLocal) {
                    splItemsCount++;
                }
                if (item.NodeType == 400 && item.SourceFlag == SourceFlags.SPLocal) {
                    splFoldersCount++;
                }
                if (item.NodeType == 500 && item.SourceFlag == SourceFlags.OneDrive) {
                    onedriveItemsCount++;
                }
                if (item.NodeType == 7103) {
                    boxDirectoryCount++;
                }
                if (item.NodeType == 7104) {
                    boxFileCount++;
                }
                if (item.NodeType == 7202){
                    googleFolderCount++;
                }
                if (item.NodeType == 7203){
                    googleFileCount++;
                }
                if (item.SourceFlag >= this.connectorRecordMinSourceFlag) {
                    connectorItemsCount++;
                }
                if (item.NodeType == 400 && item.SourceFlag == SourceFlags.Teams) {
                    teamsFolderCount++;
                }
                if (item.NodeType == 500 && item.SourceFlag == SourceFlags.Teams) {
                    teamsFileCount++;
                }
            }
            for (let key in tableNavBarBtnAllow) {
                tableNavBarBtnAllow[key] = false;
            }
            if (splFoldersCount == 0 && onedriveItemsCount == 0 && onedriveFoldersCount == 0 && azureFileShareFileCount == 0 && azureFileShareDirectoryCount == 0 && boxDirectoryCount == 0 && boxFileCount == 0 && connectorItemsCount == 0 && googleFileCount ==0 && googleFolderCount ==0) {
                if (fsFoldersCount == 0 && spFoldersCount == 0 && teamsFolderCount == 0 && splFoldersCount == 0) {
                    if (selItemsCount != 0) {
                        if ((spItemsCount == 1 || teamsFileCount == 1 || splItemsCount == 1) && declareAsRecordCount != 1 && RM.gData.enviromentName != Enviroments.ChinaNorth) {
                            tableNavBarBtnAllow['related'] = true;
                        }
                        if ((selItemsCount == spItemsCount || selItemsCount == teamsFileCount) && !containerSPListItem) {
                            tableNavBarBtnAllow['move'] = true;
                        }
                        if (selItemsCount == exoItemsCount + spItemsCount + fsItemsCount + teamsFileCount + splItemsCount) {
                            tableNavBarBtnAllow['reclassify'] = true;
                        }
                        this.isLockableSource = selItemsCount == spItemsCount || selItemsCount == onedriveItemsCount || selItemsCount == teamsFileCount;
                        if (exoItemsCount == 0 && fsItemsCount == 0) {
                            if (!this.is21VEnv && this.isNewLogicAccount && this.isLockableSource) {
                                if (selItemsCount == unlockWithRecordsLabelCount) {
                                    tableNavBarBtnAllow['lockWithRecordsLabel'] = true;
                                }
                            } else {
                                if (selItemsCount == unDeclareAsRecordCount) {
                                    tableNavBarBtnAllow['declareAsRecord'] = true;
                                }
                            }
                        }
                        if (exoItemsCount == 0 && fsItemsCount == 0) {
                            if (!this.is21VEnv && this.isNewLogicAccount && this.isLockableSource) {
                                if (selItemsCount == lockWithRecordsLabelCount) {
                                    tableNavBarBtnAllow['unlockWithRecordsLabel'] = true;
                                }
                            } else {
                                if (selItemsCount == declareAsRecordCount) {
                                    tableNavBarBtnAllow['removeDeclareAsRecord'] = true;
                                }
                            }
                        }
                        tableNavBarBtnAllow["holdManagement"] = false;
                    } else {
                        tableNavBarBtnAllow["holdManagement"] = true;
                    }
                } else {
                    if (fsFoldersCount == selItemsCount || spFoldersCount == selItemsCount || teamsFolderCount == selItemsCount) {
                        tableNavBarBtnAllow['reclassify'] = true;
                        tableNavBarBtnAllow["holdManagement"] = (spFoldersCount == selItemsCount) || (teamsFolderCount == selItemsCount);
                    } else {
                        tableNavBarBtnAllow["holdManagement"] = true;
                    }
                }
            } else {
                if (splItemsCount + splFoldersCount == selItemsCount) {
                    tableNavBarBtnAllow["reclassify"] = splFoldersCount == 0;
                    tableNavBarBtnAllow['declareAsRecord'] = splFoldersCount == 0 && selItemsCount == unDeclareAsRecordCount;
                    tableNavBarBtnAllow['removeDeclareAsRecord'] = splFoldersCount == 0 && selItemsCount == declareAsRecordCount;
                    tableNavBarBtnAllow["holdManagement"] = !(splFoldersCount == 0);
                }
                if (onedriveItemsCount + onedriveFoldersCount == selItemsCount) {
                    if(onedriveItemsCount == selItemsCount){
                        if (!this.isNewLogicAccount) {
                            tableNavBarBtnAllow['declareAsRecord'] = selItemsCount == unDeclareAsRecordCount;
                            tableNavBarBtnAllow['removeDeclareAsRecord'] = selItemsCount == declareAsRecordCount;
                        } else if (!this.is21VEnv) {
                            tableNavBarBtnAllow['lockWithRecordsLabel'] = selItemsCount == unlockWithRecordsLabelCount;
                            tableNavBarBtnAllow['unlockWithRecordsLabel'] = selItemsCount == lockWithRecordsLabelCount;
                        }

                        if (!containerSPListItem) {
                            tableNavBarBtnAllow['move'] = true;
                        }
                    }
                    tableNavBarBtnAllow["reclassify"] = onedriveItemsCount == selItemsCount || onedriveFoldersCount == selItemsCount;
                    tableNavBarBtnAllow["holdManagement"] = onedriveItemsCount != selItemsCount;
                }

                if (azureFileShareFileCount + azureFileShareDirectoryCount == selItemsCount) {
                    if(isSelectResult) {
                        tableNavBarBtnAllow["reclassify"] = true;
                        tableNavBarBtnAllow["holdManagement"] = !(azureFileShareFileCount == selItemsCount && azureFileShareFileCount > 0);
                    }
                    else {
                        tableNavBarBtnAllow["reclassify"] = azureFileShareFileCount == selItemsCount && azureFileShareFileCount > 0;
                        tableNavBarBtnAllow["holdManagement"] = !(azureFileShareFileCount == selItemsCount && azureFileShareFileCount > 0);
                    }
                }

                if (boxFileCount + boxDirectoryCount == selItemsCount) {
                    if (isSelectResult) {
                        tableNavBarBtnAllow["reclassify"] = true;
                    } else {
                        tableNavBarBtnAllow["reclassify"] = boxFileCount == selItemsCount && boxFileCount > 0;
                    }
                    tableNavBarBtnAllow["holdManagement"] = !(boxFileCount == selItemsCount && boxFileCount > 0);
                }
                if (googleFileCount + googleFolderCount  == selItemsCount){
                    if ((googleFileCount && googleFileCount == selItemsCount) || (googleFolderCount && googleFolderCount == selItemsCount)) {
                        tableNavBarBtnAllow["reclassify"] = true;
                    }
                    tableNavBarBtnAllow["holdManagement"] = true;
                }
                if (selItemsCount == connectorItemsCount && connectorItemsCount > 0) {
                    tableNavBarBtnAllow["reclassify"] = true;
                    // tableNavBarBtnAllow["holdManagement"] = this.isSelectResult ;
                }
            }
        }
        this.setState({
            tableNavBarBtnAllow: Object.assign({}, tableNavBarBtnAllow),
        }, () => {
            let showButtons = this.getShowActions();
            this.refTopButtons.updateButtons(showButtons);
        });
    }

    loadData() {
        this.props.loadData(false, true);
        this.setState({ tableNavBarBtnAllow: this.initTableNavBarBtnAllow() });
    }

    checkOperateLimitSuccess(selectedItems, unsupportedBulkAction) {
        if (this.isSelectResult) {
            if (unsupportedBulkAction) {
                if (selectedItems.length <= 5000) {
                    return true;
                } else {
                    this.showLimitMessageBox();
                    $$.messagedialog(true, this.args);
                    return false;
                }
            } else {
                return true;
            }
        }
        if (selectedItems.length <= 5000) {
            return true;
        } else {
            this.showLimitMessageBox();
            $$.messagedialog(true, this.args);
            return false;
        }
    }

    showLimitMessageBox() {
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_Common_Msg_CheckMoreThanActionLimitCount,
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
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    routerToExplorer() {
        if (this.phyObjectDetailRef.recordsStatus == 3) {
            $$.messagedialog(true, {
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_PRM_PhyRecordsDeletedTip,
                buttons: [
                    { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => { $$.messagedialog(false); } }
                ],
            });
        } else {
            window.open(`${RouterUrls.PRM_RecordsExplorer}/?uniqueId=${this.state.phyObjDetailParam.RecordsId}`);
            return false;
        }
    }

    getTableBarHoldButtons() {
        let selectedItems = this.props.getSelectedItems();
        let selectOne = selectedItems.length == 1;
        let selectMany = selectedItems.length > 1;
        let tableSelectItemsHasRecord = false;
        for (let item of selectedItems) {
            if (item.NodeType == NodeType.PhyRecord || item.NodeType == NodeType.PhyCustom) {
                tableSelectItemsHasRecord = true;
                break;
            }
        }

        const isDelegateHoldOnlyMode = this.isRoleDelegateAdmin && !this.state.canShowHoldByRecordsPermission;
        const canRenderHoldButtons = this.isHoldManagerOnly || isDelegateHoldOnlyMode || !tableSelectItemsHasRecord || this.state.canShowHoldByRecordsPermission;

        if (canRenderHoldButtons) {
            if (selectOne) {
                let selectedItem = selectedItems[0];
                if (selectedItem.HoldStatus && selectedItem.HoldId) {
                    return <div id='holdBtnGroup' style={{ marginLeft: "8px" }}>
                        <R.ButtonGroup
                            // classify="theme"
                            text={RMResx.RM_JS_BCM_Explorer_Button_HoldActions}
                        // onClick={this.onManageHoldAction.bind(this, "remove", false)}
                        >
                            <R.Button
                                id="raHsRemoveHoldBtn"
                                onClick={this.onManageHoldAction.bind(this, "remove", false)}
                                text={RMResx.RM_JS_BCM_Explorer_Button_CancelHold} />
                            <R.Button
                                onClick={this.onManageHoldAction.bind(this, "append", true)}
                                text={RMResx.RM_JS_BCM_Explorer_Button_AppendHold} />
                            <R.Button
                                onClick={this.onManageHoldAction.bind(this, "change", false)}
                                text={RMResx.RM_JS_BCM_Explorer_Button_ChangeHold} />
                            <R.Button
                                id="raHsExtendHoldBtn"
                                onClick={this.onManageHoldAction.bind(this, "extend", false)}
                                text={RMResx.RM_JS_BCM_Explorer_Button_SuspendHold} />
                        </R.ButtonGroup>
                    </div>;
                } else if (!selectedItem.HoldStatus) {
                    return <R.Button
                        id="raHsCreateHoldBtn"
                        icon="fia-place-hold"
                        onClick={this.onManageHoldAction.bind(this, "new", false)}
                        text={RMResx.RM_JS_BCM_Explorer_Button_PutOnHold} />;
                }
            }
            else if (selectMany) {
                let allHold = selectedItems.every((item, index) => (item.HoldStatus && item.HoldId));
                if (allHold) {
                    return <div id='holdBtnGroup' style={{ marginLeft: "8px" }}>
                        <R.ButtonGroup
                            // classify="theme"
                            text={RMResx.RM_JS_BCM_Explorer_Button_HoldActions}
                        // onClick={this.onManageHoldAction.bind(this, "remove", false)}
                        >
                            <R.Button
                                onClick={this.onManageHoldAction.bind(this, "remove", false)}
                                text={RMResx.RM_JS_BCM_Explorer_Button_CancelHold} />
                            <R.Button
                                onClick={this.onManageHoldAction.bind(this, "append", true)}
                                text={RMResx.RM_JS_BCM_Explorer_Button_AppendHold} />
                            <R.Button
                                onClick={this.onManageHoldAction.bind(this, "change", false)}
                                text={RMResx.RM_JS_BCM_Explorer_Button_ChangeHold} />
                            <R.Button
                                onClick={this.onManageHoldAction.bind(this, "extend", false)}
                                text={RMResx.RM_JS_BCM_Explorer_Button_SuspendHold} />
                        </R.ButtonGroup>
                    </div>;
                } else if (selectedItems.every((item, index) => (!item.HoldStatus))) {
                    return <R.Button
                        icon="fia-place-hold"
                        onClick={this.onManageHoldAction.bind(this, "new", false)}
                        text={RMResx.RM_JS_BCM_Explorer_Button_PutOnHold} />;
                } else if (selectedItems.find((item) => (item.HoldStatus)) && selectedItems.find((item) => (!item.HoldStatus))) {
                    return <R.Button
                        icon="fia-place-hold"
                        onClick={this.onManageHoldAction.bind(this, "append", false)}
                        text={RMResx.RM_JS_BCM_Explorer_Button_AppendHold} />;
                }
            }
        }
    }

    isAllowLoan(items) {
        if (this.isPhysicalAdmin) {
            return false;
        }
        if (!checkPermission("PRM_FolderLoanRequest", RM.UserResources)) {
            return false;
        }
        let allowLoan = true;
        for (const item of items) {
            if ((item.NodeType != NodeType.PhyFile && item.NodeType != NodeType.PhyBox)
                || item.RecordStatus == PhysicalObjectStatus.Destroyed
                || item.RecordStatus == PhysicalObjectStatus.Missing
                // || item.PersonHold
            ) {
                allowLoan = false;
                break;
            }
        }
        return allowLoan;
    }

    isAllowEdit(items) {
        if (!this.isPhysicalAdmin) {
            return false;
        }
        if (this.isSelectResult && this.isSelectedOneSource && !this.canDoPhysicalBulkUpdate) {
            return false;
        }
        let allow = true;
        if (items.length != 1) {
            items.forEach((e, index) => {
                let sameTemplateId = items.find(t => t.TemplateId != e.TemplateId);
                if (sameTemplateId || e.RecordStatus == PhysicalObjectStatus.Destroyed) {
                    allow = false;
                }
            });
        } else {
            if (items[0].RecordStatus == PhysicalObjectStatus.Destroyed) {
                allow = false;
            }
        }
        return allow;
    }

    isAllowDelete(items) {
        if (!this.isPhysicalAdmin) {
            return false;
        }
        if (this.isSelectResult) {
            return false;
        }
        let allow = true;
        for (const item of items) {
            if (item.HoldStatus || item.PersonHold) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowMove(items) {
        if (this.isSelectResult) {
            return false;
        }
        if (!this.isPhysicalAdmin) {
            return false;
        }
        let allow = true;
        const isOnlyPhysicalRecordData = items.every(item => item.NodeType == NodeType.PhyRecord);
        const isAllowMovePhyRecord = this.isNewLogicAccount && isOnlyPhysicalRecordData;
        for (const item of items) {
            if ([NodeType.PhyRecord, NodeType.PhyCustom].includes(item.NodeType)) {
                if (!isAllowMovePhyRecord) { 
                    allow = false;
                    break;
                }
            }
            if (item.RecordStatus == PhysicalObjectStatus.Destroyed
                || item.RecordStatus == PhysicalObjectStatus.Missing
                || item.PersonHold) {
                allow = false;
                break;
            }
            if (item.Ancestors && item.Ancestors.length > 0) {
                if (item.NodeType == NodeType.PhyBox && item.ParentId != item.LocationId) {
                    //container下的box
                    allow = false;
                    break;
                }

                if (item.NodeType == NodeType.PhyFile) {
                    if (item.ParentId == item.LocationId || item.Ancestors[1] == item.BoxId) {
                        //location下folder和location下的box下的folder
                        continue;
                    }
                    else {
                        //container下的folder
                        allow = false;
                        break;
                    }
                }

                if (item.NodeType == NodeType.PhyRecord) {
                    //only allow move record if it's under location >> box >> folder or location >> folder
                    if (!(item.Ancestors[1] == item.BoxId || item.Ancestors[1] == item.FileId)) {
                        //container下的record
                        allow = false;
                        break;
                    }
                }
            }
        }
        return allow;
    }

    isAllowMoveRequest(items) {
        if (this.isSelectResult) {
            return false;
        }

        const role = Number(RM.RoleType);
        if (role !== RoleType.StandardUser && role !== RoleType.StandardReviewUser) {
            return false;
        }
        
        if (!checkPermission("PRM_MoveRequest", RM.UserResources)) {
            return false;
        }

        let allow = items.length > 0;

        const hasRecords = items.some(item => item.NodeType === NodeType.PhyRecord);
        const hasFoldersOrBoxes = items.some(item => item.NodeType === NodeType.PhyFile || item.NodeType === NodeType.PhyBox);
        if (hasRecords && hasFoldersOrBoxes) {
            return false;
        }
        
        for (const item of items) {
            if (item.SourceFlag != 4) {
                allow = false;
                break;
            }
            if (item.RecordStatus == PhysicalObjectStatus.Destroyed
                || item.RecordStatus == PhysicalObjectStatus.Missing
                || item.PersonHold) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowReclassify(items) {
        if (!this.isPhysicalAdmin) {
            return false;
        }
        let allow = true;
        for (const item of items) {
            if ([NodeType.PhyRecord, NodeType.PhyCustom].includes(item.NodeType)) {
                allow = false;
                break;
            }
            if (item.RecordStatus == PhysicalObjectStatus.Destroyed
                || item.RecordStatus == PhysicalObjectStatus.Missing
            ) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowRemovePersonHold(items) {
        if (!this.isPhysicalAdmin && !checkPermission("PRM_FolderLoanReturn", RM.UserResources)) {
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
            if ((item.NodeType != NodeType.PhyFile && item.NodeType != NodeType.PhyRecord && RM.gData.enviromentName == Enviroments.ChinaNorth)
                || item.NodeType == NodeType.PhyBox
                || item.NodeType == NodeType.PhyCustom
                || item.RecordStatus == PhysicalObjectStatus.Destroyed
                || item.RecordStatus == PhysicalObjectStatus.Missing) {
                allow = false;
                break;
            }
        }
        return allow;
    }

    isAllowSetPermissions(items) {
        if (!checkPermission("PRM_SetAccessControl", RM.UserResources)) {
            return false;
        }
        let allow = true;
        for (let item of items) {
            if (!(item.NodeType == NodeType.PhyBox || item.NodeType == NodeType.PhyFile || item.NodeType == NodeType.PhyCustom)) {
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
        if (items['phyDelete']) {
            tableNavBarBtnItemsInMore.push(TableNavBarBtnInMore['phyDelete']);
            allow = true;
        }
        if(items['allowShowAudit'] && RM.RoleType != RoleType.StandardUser){
            tableNavBarBtnItemsInMore.push(TableNavBarBtnInMore['allowShowAudit']);
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
        if(this.props.isPhysicalEndUser){
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

    isAllowNewChildren(selectedTableItems, phyObj) {
        let allowNew = true;
        if (selectedTableItems.length != 0) {
            return false;
        }
        if (!this.state.hasTermSettings) {
            allowNew = false;
        } else if (phyObj.NodeType > NodeType.PhysicalBottomLocation) {
            let status = JSON.parse(phyObj.MetaInfo[PhysicalDefaultColumnIDs.Status]).Value;
            if (status == PhysicalObjectStatus.Destroyed || status == PhysicalObjectStatus.Missing || phyObj.PersonHold) {
                allowNew = false;
            }
        }
        return allowNew;
    }

    showMessageTip(type, msg) {
        this.props.showMessageTip(type, msg);
    }

    getIsIncludeFolderForSp(selectedTableItem){
        let sourcesIncludeFolderForSp = [1, 6, 11];
        return sourcesIncludeFolderForSp.includes(selectedTableItem.SourceFlag) && selectedTableItem.NodeType == 400;
    } 

    onSaveReclassify() {
        let isSelectedOneSourceFilter = this.isSelectedOneSource;
        let needStartJob = isSelectedOneSourceFilter && this.isSelectResult;
        let callback = (termData, errorCallBack) => {
            let validateFailed = false;
            if (termData.Type == 'Root' || termData.Type == 'TermGroup' || termData.Type == 'TermSet') {
                validateFailed = true;
                errorCallBack(RMResx.RM_JS_PRM_Msg_ReclassifyNoSelecteTermLevel);
            }
            if (validateFailed) {
                return false;
            }
            let reclassifyParam = {
                RecordIds: [],
                EXORecordIds: [],
                FSRecordIds: [],
                PhyRecordIds: [],
                SPOnPremRecordIds: [],
                OneDriveRecordIds: [],
                AzureFileShareRecordIds: [],
                BoxRecordIds: [],
                CustomizeConnectorRecordIds: [],
                TeamsRecordIds: [],
                TermInfo: {
                    Id: termData.Id,
                    Name: termData.Name,
                    UniqueId: termData.UniqueId
                },
                Comment: termData.Comment
            };

            let googleReclassifyParam ={
                GoogleDriveRecordIds:[],
                TermInfo: {
                    Id: termData.Id,
                    Name: termData.Name,
                    UniqueId: termData.UniqueId
                },
                Comment: termData.Comment
            }

            let selectedTableItems = this.state.selectedItems;
    
            let containsFolder = selectedTableItems.find((item) => { 
                return this.getIsIncludeFolderForSp(item) || item.NodeType == 2100; 
            });
            for (let item of selectedTableItems) {
                if (item.SourceFlag == 3) {
                    reclassifyParam.EXORecordIds.push(item.Id);
                } else if (item.SourceFlag == 2) {
                    reclassifyParam.FSRecordIds.push(item.Id);
                } else if (item.SourceFlag == 4) {
                    reclassifyParam.PhyRecordIds.push(item.Id);
                } else if (item.SourceFlag == 5) {
                    reclassifyParam.SPOnPremRecordIds.push(item.Id);
                } else if (item.SourceFlag == 6) {
                    reclassifyParam.OneDriveRecordIds.push(item.Id);
                } else if (item.SourceFlag == 7) {
                    reclassifyParam.AzureFileShareRecordIds.push(item.Id);
                } else if (item.SourceFlag == 8) {
                    reclassifyParam.BoxRecordIds.push(item.Id);
                } else if (item.SourceFlag == 9) {
                    googleReclassifyParam.GoogleDriveRecordIds.push(item.Id);
                } else if (item.SourceFlag == 11) {
                    reclassifyParam.TeamsRecordIds.push(item.Id);
                } else if(item.SourceFlag >= 1000) {
                    reclassifyParam.CustomizeConnectorRecordIds.push(item.Id);
                }
                else {
                    reclassifyParam.RecordIds.push(item.Id);
                }
            }

            const isGoogleReclassify = this.state.selectedItems?.length > 0 && this.state.selectedItems?.every(item=>item.NodeType == 7202 || item.NodeType == 7203);
            if (isGoogleReclassify){
                let nodeId = new Set();
                selectedTableItems.forEach(item => {
                    if (item.ScopeId && !nodeId.has(item.ScopeId)) {
                        nodeId.add(item.ScopeId)
                    }
                });
                googleReclassifyParam.NodeId = JSON.stringify(Array.from(nodeId));
                
                let containsGoogleFolder = selectedTableItems.find((item) => { 
                    return item.NodeType == 7202; 
                });
                if (containsGoogleFolder){
                    this.folderReclassifyOption = {
                        selectedTableItems: selectedTableItems,
                        reclassifyParam: googleReclassifyParam,
                        forceDiscoverAll:  this.isSelectResult && isSelectedOneSourceFilter,
                        errorCallBack: errorCallBack
                    };
                    this.setState({ 
                        isOverWriteSubFiles: false,
                        isReclassifySubFiles: false
                    });
                    this.showGoogleFolderReclassifyOption();
                } else if (needStartJob) {
                    this.sendRunJobReclassifyRequest(googleReclassifyParam, selectedTableItems, needStartJob, errorCallBack);
                } else {
                    this.sendGoogleReclassifyRequest(selectedTableItems, googleReclassifyParam, errorCallBack);
                }
                return;

            }

            if (containsFolder) {
                this.folderReclassifyOption = {
                    selectedTableItems: selectedTableItems,
                    reclassifyParam: reclassifyParam,
                    forceDiscoverAll:  this.isSelectResult && isSelectedOneSourceFilter,
                    errorCallBack: errorCallBack
                };
                this.setState({ 
                    isOverWriteSubFiles: false,
                    isReclassifySubFiles: false
                });
                this.showFSFolderReclassifyOption();
            } else if (needStartJob) {
                this.sendRunJobReclassifyRequest(reclassifyParam, selectedTableItems, this.isSelectResult && isSelectedOneSourceFilter, errorCallBack);
            } else {
                this.sendReclassifyRequest(selectedTableItems, reclassifyParam, errorCallBack);
            }
        };
        this.dispatch("elecReclassifyForm", 'onSave', callback);
        return false;
    }

    onCheckChangeOverWrite = (checked) => {
        this.setState({ isOverWriteSubFiles: checked });
    }

    onCheckChangeReclassifySubFiles = (checked) =>{
        this.setState({ 
            isReclassifySubFiles: checked,
            isOverWriteSubFiles: false
        },()=>{
            this.showFSFolderReclassifyOption();
        });
    }

    getReclassifyOverWriteSubFilesCheckbox(){
        return <R.Checkbox
            name="checkbox-fs-folder-opt"
            text={RMResx.RM_JS_BCM_IncludeAllFileUnderFolder_Message}
            title={RMResx.RM_JS_BCM_IncludeAllFileUnderFolder_Message}
            checked={this.state.isOverWriteSubFiles}
            onChange={this.onCheckChangeOverWrite}
        />;
    }

    getReclassifySetOptions(selectedTableItems){
        let reclassifySubFilesCheckbox = "";
        let reclassifyOverWriteSubFiles = "";
        let isIncludeFolderForSp = this.getIsIncludeFolderForSp(selectedTableItems[0]);
        if(isIncludeFolderForSp){
            reclassifySubFilesCheckbox = <div>
                <R.Checkbox
                    name="checkbox-sp-folder-opt"
                    text={RMResx.RM_HS_Msg_ReclassifyWithFolder}
                    title={RMResx.RM_HS_Msg_ReclassifyWithFolder}
                    checked={this.state.isReclassifySubFiles}
                    onChange={this.onCheckChangeReclassifySubFiles}
                />
            </div>;
            if(this.state.isReclassifySubFiles){
                reclassifyOverWriteSubFiles = <div className="margin-top-s margin-left-l">
                    {this.getReclassifyOverWriteSubFilesCheckbox()}
                </div>;
            }
        }else{
            reclassifyOverWriteSubFiles = this.getReclassifyOverWriteSubFilesCheckbox();
        }
        return <div>
            <div className="margin-bottom-l">{RMResx.RM_JS_BCM_ChangeTermForFolder_Message}</div>
            {reclassifySubFilesCheckbox}
            {reclassifyOverWriteSubFiles}
        </div>;
    }

    showFSFolderReclassifyOption = () => {
        let {selectedTableItems, reclassifyParam, forceDiscoverAll, errorCallBack} = this.folderReclassifyOption;
        this.args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_BCM_Explorer_ChangeTerm,   
            content: <div>
                <div>{this.getReclassifySetOptions(selectedTableItems)}</div>
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false, this.args);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.sendRunJobReclassifyRequest.bind(this, reclassifyParam, selectedTableItems, forceDiscoverAll, errorCallBack)
                },

            ]
        };
        $$.messagedialog(true, this.args);
    }

    showGoogleFolderReclassifyOption = () => {
        let {selectedTableItems, reclassifyParam, forceDiscoverAll, errorCallBack} = this.folderReclassifyOption;
        this.args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_BCM_Explorer_ChangeTerm,   
            content: <div>
                <div>{this.getGoogleReclassifySetOptions(selectedTableItems)}</div>
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false, this.args);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.sendRunJobReclassifyRequest.bind(this, reclassifyParam, selectedTableItems, forceDiscoverAll, errorCallBack)
                },

            ]
        };
        $$.messagedialog(true, this.args);
    }

    getGoogleReclassifySetOptions(){
        return (
            <div>
                <div className="margin-bottom-l">
                    {RMResx.RM_JS_BCM_ChangeTermForFolder_Message}
                </div>
                <R.Checkbox
                    name="checkbox-fs-folder-opt"
                    text={
                        RMResx.RM_JS_BCM_Label_IncludeAllFileUnderFolder_Message
                    }
                    title={
                        RMResx.RM_JS_BCM_Label_IncludeAllFileUnderFolder_Message
                    }
                    checked={this.state.isOverWriteSubFiles}
                    onChange={this.onCheckChangeOverWrite}
                />
            </div>
        );
    }


    sendRunJobReclassifyRequest(reclassifyParam, selectedTableItems, forceDiscoverAll, errorCallBack) {
        $$.messagedialog(false);
        reclassifyParam.OverWriteSubFiles = this.state.isOverWriteSubFiles;
        reclassifyParam.ReclassifySubFiles = this.state.isReclassifySubFiles;
        let sourceFlag = 0;
        if (forceDiscoverAll) {
            sourceFlag = selectedTableItems[0].SourceFlag;
        } else {
            if (reclassifyParam.RecordIds?.length > 0) {
                sourceFlag = SourceFlags.SP;
            } else if (reclassifyParam.EXORecordIds?.length > 0) {
                sourceFlag = SourceFlags.Exo;
            } else if (reclassifyParam.FSRecordIds?.length > 0) {
                sourceFlag = SourceFlags.FS;
            } else if (reclassifyParam.PhyRecordIds?.length > 0) {
                sourceFlag = SourceFlags.Phy;
            } else if (reclassifyParam.OneDriveRecordIds?.length > 0) {
                sourceFlag = SourceFlags.OneDrive;
            } else if (reclassifyParam.GoogleDriveRecordIds?.length > 0) {
                sourceFlag = SourceFlags.Google;
            } else if (reclassifyParam.TeamsRecordIds?.length > 0) {
                sourceFlag = SourceFlags.Teams;
            } else if (reclassifyParam.CustomizeConnectorRecordIds?.length > 0) {
                sourceFlag = selectedTableItems[0].SourceFlag;
            }
        }

        let param = {
            IsRealTimeAction: false,
            FilterInfo: { QueryOption: { Values: this.searchOption } },
            Action: GlobalSearchAction.Reclassify,
            ActionExtension: reclassifyParam,
            SourceFlag: sourceFlag,
            ForceDiscoverAll: forceDiscoverAll,
            RecordIds: selectedTableItems.map(i => i.Id),
        };
        let url = `/api/GlobalSearchApi/DoAction`;
        param.ChangeTermOrigin = ChangeTermOrigin.Search;
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        $$.loading(true);
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((resultJson) => {
            $$.loading(false);
            if (resultJson.MessageType == "0") {
                if (resultJson.Extension) {
                    this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>);
                }
            }
            else {
                this.showMessageTip("error", resultJson.ErrorMessage);
            }
            this.setState({
                showReclassifyPanel: { show: false }
            });
        });
    }

    sendReclassifyRequest(selectedTableItems, reclassifyParam, errorCallBack) {
        $$.loading(true);
        let url = `/api/RecordsExplorerApi/ChangeTerm`;
        let option = {
            url: url,
            method: "POST",
            data: reclassifyParam
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            $$.loading(false);
            let resultData = JSON.parse(result);
            if (resultData.MessageType == 1) {
                errorCallBack(resultData.ErrorMessage);
            } else {
                this.updateNotificationTimer(resultData.Extension, 'reclassify', false, selectedTableItems);
                this.setState({
                    showReclassifyPanel: { show: false }
                });
            }
        });
    }

    sendGoogleReclassifyRequest(selectedTableItems, reclassifyParam, errorCallBack) {
        $$.loading(true);
        let url = `/api/RecordsExplorerApi/ChangeLabel`;
        let option = {
            url: url,
            method: "POST",
            data: reclassifyParam
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            $$.loading(false);
            let resultData = JSON.parse(result);
            if (resultData.MessageType == 1) {
                errorCallBack(resultData.ErrorMessage);
            } else {
                this.updateGoogleNotificationTimer(resultData.Extension, selectedTableItems);
                this.setState({
                    showReclassifyPanel: { show: false }
                });
            }
        });
    }

    sendFSFolderReclassifyRequest = (selectedTableItems, reclassifyParam, errorCallBack) => {
        $$.messagedialog(false);
        $$.loading(true);
        reclassifyParam.OverWriteSubFiles = this.state.isOverWriteSubFiles;
        let url = `/api/RecordsExplorerApi/ChangeTerm`;
        let option = {
            url: url,
            method: "POST",
            data: reclassifyParam
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            $$.loading(false);
            let resultJson = JSON.parse(result);
            if (resultJson.MessageType == "0") {
                if (resultJson.Extension) {
                    this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>);
                }
            }
            else {
                this.showMessageTip("error", resultJson.ErrorMessage);
            }
            this.setState({
                showReclassifyPanel: { show: false }
            });
        });
    }

    handleError(response) {
        $$.loading(false);
        if (response.status == 403) {
            response.text().then((errorMessage) => {
                let messageDialogContent = RMResx.RM_JS_Common_NoPermissionLicense;
                if(errorMessage && errorMessage.includes("User have no sp access")){
                    messageDialogContent = RMResx.RM_JS_Common_NoSharepointPermissionLicense;
                }
                $$.messagedialog(true, {
                classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: messageDialogContent,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => { $$.messagedialog(false); }
                    }
                ]
                });
            });
        }
    }

    updateNotificationTimer(jobId, type, isDeclare, selectedElecItems) {
        //判断notification panel中节点是否被Dismiss all,被Dismiss all，清空notificationCacheData;
        if ($(".rm-notification-content").children().length == 0) {
            this.notificationCacheData = [];
        }
        let startTime = new Date();
        let timerCount = 0;
        let notificationMsg = this.getNotificationTitleMsg(jobId, selectedElecItems, isDeclare, ElecStatusEnum.InProgress);
        let notificationMenuMsg = this.getNotificationTitleMsg(jobId, selectedElecItems, isDeclare, ElecStatusEnum.InProgress);
        let notificationItem = {
            msg: notificationMsg,
            selectedElecItems: selectedElecItems,
            status: RMResx.RM_JS_Notifi_Status_Running,
            jobId: jobId,
            showTime: RM.TimeUtil.dateToStringSimplifyTimeZone(startTime, RM.TimeUtil.getGlobalTimezoneInfo())
        };
        this.notificationCacheData.push(notificationItem); //每次调用listener增加一条；
        let notificationHtml = this.getNotificationHtml();
        let notificationMenuHtml = this.getNotificationMenuMsgHtml(notificationMenuMsg, selectedElecItems);
        this.dispatch('raNotification', notificationHtml);
        this.dispatch('rmSuiteBar');
        this.dispatch('raNotificationMenu', notificationMenuHtml, ElecStatusEnum.InProgress);
        let updateChangeTerm = setInterval(() => {
            ++timerCount;
            if (jobId) {
                let completenotificationCache = [];
                let filednotificationCache = [];
                let exceptionNotificationCache = [];
                let notificationCache = [];
                let option = {
                    url: `/api/RecordsExplorerApi/GetRealTimeJobStatusInfo?jobId=${jobId}`,
                    method: "GET"
                };
                fetchUtility(option).then((result) => {
                    let msg = JSON.parse(result);
                    let stopTimer = false;
                    if (timerCount == 60 * 10) {// 10min
                        stopTimer = true;
                    }
                    if (msg.MessageType == 1) {// failed
                        stopTimer = true;
                        let endTime = new Date();
                        $$.loading(false);
                        for (let item of this.notificationCacheData) {
                            if (item.jobId == jobId) {
                                item.msg = this.getNotificationTitleMsg(jobId, selectedElecItems, isDeclare, ElecStatusEnum.Failed);
                                item.status = RMResx.RM_JS_Notifi_Status_Failed;
                                item.showTime = RM.TimeUtil.dateToStringSimplifyTimeZone(endTime, RM.TimeUtil.getGlobalTimezoneInfo());
                            }
                            if (item.status == RMResx.RM_JS_Notifi_Status_Competed) {
                                completenotificationCache.push(item);
                            } else if (item.status == RMResx.RM_JS_Notifi_Status_Failed) {
                                filednotificationCache.push(item);
                            } else if (item.status == RMResx.RM_JS_Notifi_Status_Exception) {
                                exceptionNotificationCache.push(item)
                            } else {
                                notificationCache.push(item);
                            }
                        }
                        this.notificationCacheData = [];
                        this.notificationCacheData.push(...notificationCache, ...filednotificationCache, ...completenotificationCache, ...exceptionNotificationCache);
                        notificationMenuMsg = this.getNotificationTitleMsg(jobId, selectedElecItems, isDeclare, ElecStatusEnum.Failed);
                        notificationMenuHtml = this.getNotificationMenuMsgHtml(notificationMenuMsg, selectedElecItems);
                        notificationHtml = this.getNotificationHtml();
                        this.dispatch('raNotification', notificationHtml);
                        this.dispatch('raNotificationMenu', notificationMenuHtml, ElecStatusEnum.Failed);
                    } else if (msg.MessageType == 2) { //exception
                        stopTimer = true;
                        let endTime = new Date();
                        $$.loading(false);
                        for (let item of this.notificationCacheData) {
                            if (item.jobId == jobId) {
                                item.msg = this.getNotificationTitleMsg(jobId, selectedElecItems, isDeclare, ElecStatusEnum.Exception);
                                item.status = RMResx.RM_JS_Notifi_Status_Exception;
                                item.showTime = RM.TimeUtil.dateToStringSimplifyTimeZone(endTime, RM.TimeUtil.getGlobalTimezoneInfo());
                            }
                            if (item.status == RMResx.RM_JS_Notifi_Status_Competed) {
                                completenotificationCache.push(item);
                            } else if (item.status == RMResx.RM_JS_Notifi_Status_Failed) {
                                filednotificationCache.push(item);
                            } else if (item.status == RMResx.RM_JS_Notifi_Status_Exception) {
                                exceptionNotificationCache.push(item)
                            } else {
                                notificationCache.push(item);
                            }
                        }
                        this.notificationCacheData = [];
                        this.notificationCacheData.push(...notificationCache, ...filednotificationCache, ...completenotificationCache, ...exceptionNotificationCache);
                        notificationMenuMsg = this.getNotificationTitleMsg(jobId, selectedElecItems, isDeclare, ElecStatusEnum.Failed);
                        notificationMenuHtml = this.getNotificationMenuMsgHtml(notificationMenuMsg, selectedElecItems);
                        notificationHtml = this.getNotificationHtml();
                        this.dispatch('raNotification', notificationHtml);
                        this.dispatch('raNotificationMenu', notificationMenuHtml, ElecStatusEnum.Failed);

                    } else {
                        if (msg.Items) {
                            if (msg.Status == 4) {
                                stopTimer = true;
                                let endTime = new Date();
                                for (let item of this.notificationCacheData) {
                                    if (item.jobId == jobId) {
                                        item.msg = this.getNotificationTitleMsg(jobId, selectedElecItems, isDeclare, ElecStatusEnum.Completed);
                                        item.status = RMResx.RM_JS_Notifi_Status_Competed;
                                        item.showTime = RM.TimeUtil.dateToStringSimplifyTimeZone(endTime, RM.TimeUtil.getGlobalTimezoneInfo());
                                    }
                                    if (item.status == RMResx.RM_JS_Notifi_Status_Competed) {
                                        completenotificationCache.push(item);
                                    } else if (item.status == RMResx.RM_JS_Notifi_Status_Failed) {
                                        filednotificationCache.push(item);
                                    } else if (item.status == RMResx.RM_JS_Notifi_Status_Exception) {
                                        exceptionNotificationCache.push(item)
                                    } else {
                                        notificationCache.push(item);
                                    }
                                }
                                this.notificationCacheData = [];
                                this.notificationCacheData.push(...notificationCache, ...filednotificationCache, ...completenotificationCache, ...exceptionNotificationCache);
                                notificationMenuMsg = this.getNotificationTitleMsg(jobId, selectedElecItems, isDeclare, ElecStatusEnum.Completed);
                                notificationMenuHtml = this.getNotificationMenuMsgHtml(notificationMenuMsg, selectedElecItems);
                                notificationHtml = this.getNotificationHtml();
                                this.dispatch('raNotification', notificationHtml);
                                this.dispatch('raNotificationMenu', notificationMenuHtml, ElecStatusEnum.Completed);
                            }
                        }
                    }
                    //stop this timer
                    if (stopTimer) {
                        clearInterval(updateChangeTerm);
                        this.loadData();
                    }
                });
            }
        }, 1000);
    }

    updateGoogleNotificationTimer(jobId, selectedElecItems) {
        if ($(".rm-notification-content").children().length == 0) {
            this.notificationCacheData = [];
        }
        let startTime = new Date();
        let notificationMsg = this.getNotificationTitleMsg(jobId, selectedElecItems, false, ElecStatusEnum.InProgress);
        let notificationMenuMsg = this.getNotificationTitleMsg(jobId, selectedElecItems, false, ElecStatusEnum.InProgress);
        let notificationItem = {
            msg: notificationMsg,
            selectedElecItems: selectedElecItems,
            status: RMResx.RM_JS_Notifi_Status_Running,
            jobId: jobId,
            startTime: startTime.toLocaleTimeString(),
            showTime: RM.TimeUtil.dateToStringSimplifyTimeZone(startTime, RM.TimeUtil.getGlobalTimezoneInfo())
        };
        this.notificationCacheData.push(notificationItem);
        let notificationHtml = this.getNotificationHtml();
        let notificationMenuHtml = this.getNotificationMenuMsgHtml(notificationMenuMsg, selectedElecItems);
        this.dispatch('raNotification', notificationHtml);
        this.dispatch('rmSuiteBar');
        this.dispatch('raNotificationMenu', notificationMenuHtml, ElecStatusEnum.InProgress);

        // Update change label
        const updateNotificationStatus = (elecStatus, iconsStatus) => {
            const endTime = new Date();
            let completenotificationCache = [];
            let filednotificationCache = [];
            let exceptionNotificationCache = [];
            let notificationCache = [];

            for (let item of this.notificationCacheData) {
                if (item.jobId == jobId) {
                    item.msg = this.getNotificationTitleMsg(jobId, selectedElecItems, false, elecStatus);
                    item.status = iconsStatus;
                    item.startTime = endTime.toLocaleTimeString();
                    item.showTime = RM.TimeUtil.dateToStringSimplifyTimeZone(endTime, RM.TimeUtil.getGlobalTimezoneInfo())
                }
                if (item.status == RMResx.RM_JS_Notifi_Status_Competed) {
                    completenotificationCache.push(item);
                } else if (item.status == RMResx.RM_JS_Notifi_Status_Failed) {
                    filednotificationCache.push(item);
                } else if (item.status == RMResx.RM_JS_Notifi_Status_Exception) {
                    exceptionNotificationCache.push(item)
                } else {
                    notificationCache.push(item);
                }
            }
            this.notificationCacheData = [];
            this.notificationCacheData.push(...notificationCache, ...filednotificationCache, ...completenotificationCache, ...exceptionNotificationCache);
            notificationMenuMsg = this.getNotificationTitleMsg(jobId, selectedElecItems, false, elecStatus);
            notificationMenuHtml = this.getNotificationMenuMsgHtml(notificationMenuMsg, selectedElecItems);
            notificationHtml = this.getNotificationHtml();
            this.dispatch('raNotification', notificationHtml);
            this.dispatch('raNotificationMenu', notificationMenuHtml, elecStatus);
        }

        const startReclassifyTime = Date.now();
        const _this = this;
        (function updateChangeTerm() {
            if (jobId) {
                let option = {
                    url: `/api/RecordsExplorerApi/GetRealTimeJobStatusInfo?jobId=${jobId}`,
                    method: "GET"
                };

                fetchUtility(option).then((result) => {
                    let data = JSON.parse(result);
                    let stopTimer = false;
                    if (data.MessageType == 1) {
                        // reclassify failed
                        stopTimer = true;
                        updateNotificationStatus(ElecStatusEnum.Failed, RMResx.RM_JS_Notifi_Status_Failed);
                    } else if (data.MessageType == 2) {
                        // exception
                        stopTimer = true;
                        updateNotificationStatus(ElecStatusEnum.Exception, RMResx.RM_JS_Notifi_Status_Exception);
                    } else if (data.Items && data.Status == 4) {
                        // reclassify completed
                        stopTimer = true;
                        updateNotificationStatus(ElecStatusEnum.Completed, RMResx.RM_JS_Notifi_Status_Competed);
                    }

                    if (stopTimer || (Date.now() - startReclassifyTime) >= 10 * 60000) { // 60000ms = 1 minute
                        // stop reclassification if it fails, succeeds, or runs for over 10 minutes.
                        _this.loadData();
                    } else {
                        // function will be called after 1 second
                        setTimeout(updateChangeTerm, 1000);
                    }
                });
            }
        })()
    }

    getNotificationTitleMsg(jobId, data, isDeclare, status) {
        let actionString = '';
        let statusString = '';
        let showMsgFile = data.length == 1 ? RMResx.RM_JS_Notifi_Message_File : RMResx.RM_JS_Notifi_Message_Files;
        if (jobId.startsWith("UT") || jobId.startsWith("UL")) {
            actionString = RMResx.RM_JS_Notifi_Action_Reclassification;
        } else if (jobId.startsWith("PM")) {
            actionString = RMResx.RM_JS_Notifi_Action_Move;
        } else {
            const actionStringMap = {
                declare: this.isLockableSource && !this.is21VEnv && this.isNewLogicAccount ? RMResx.RM_JS_Notifi_Action_LockByRecordsLabel : RMResx.RM_JS_Notifi_Action_Declare,
                undeclare: this.isLockableSource && !this.is21VEnv && this.isNewLogicAccount ? RMResx.RM_JS_Notifi_Action_UnLockByRecordsLabel : RMResx.RM_JS_Notifi_Action_Undeclare,
            };
            actionString = isDeclare ? actionStringMap.declare : actionStringMap.undeclare;
        }
        if (status == 0) {
            statusString = RMResx.RM_JS_Notifi_Status_Running;
        } else if (status == 1) {
            statusString = RMResx.RM_JS_Notifi_Status_Failed;
        } else if (status == 2) {
            statusString = RMResx.RM_JS_Notifi_Status_Competed;
        } else if (status == 3) {
            statusString = RMResx.RM_JS_Notifi_Status_Exception;
        }
        let showMsg = `${actionString} ${statusString}`;// ${data.length} ${showMsgFile} 
        return showMsg;
    }

    getNotificationHtml() {
        let notificationItems = RM.deepcopy(this.notificationCacheData);
        return <div>
            {
                notificationItems.map((item, index) => {
                    return <div key={index}>
                        <div className="notification-space"></div>
                        <div className='ra-elec-notification-conetnt'>
                            <div className='ra-elec-notification-msg'>{item.msg}</div>
                            <div className='flex'>
                                <div className='fia-searchbox-close notify-icon' onClick={this.onDeleteNotification.bind(this, index)}></div>
                            </div>
                        </div>
                        <div className='ra-elec-notification-items'>
                            {
                                item.selectedElecItems.slice(0, 3).map((item, index) => {
                                    return <div key={index} className='ra-elec-notification-item'>
                                        <div>{item.LeafName}</div>
                                    </div>;
                                })
                            }
                            {
                                item.selectedElecItems.length > 3 &&
                                <div key={index} className='ra-elec-notification-item'>
                                    <div>{`...(${item.selectedElecItems.length - 3})`}</div>
                                </div>
                            }
                        </div>
                        <div className='ra-elec-notification-time'>
                            {item.status == RMResx.RM_JS_Notifi_Status_Running && <div className='fia-in-progress'></div>}
                            {item.status == RMResx.RM_JS_Notifi_Status_Competed && <div className='fia-checkbox-device completed'></div>}
                            {item.status == RMResx.RM_JS_Notifi_Status_Failed && <div className='fia-status-error'></div>}
                            {item.status == RMResx.RM_JS_Notifi_Status_Exception && <div className='fia-status-error'></div>}
                            <span className='ra-elec-notification-showTime'>{item.showTime}</span>
                        </div>
                    </div>;
                })
            }
        </div>;
    }

    getNotificationMenuMsgHtml(notificationMsg, selectedElecItems) {
        let notificationMenuItems = RM.deepcopy(selectedElecItems);
        if (selectedElecItems.length > 3) {
            let ellipsisStr = `...(${notificationMenuItems.length - 3})`;
            notificationMenuItems = notificationMenuItems.slice(0, 3);
            notificationMenuItems.push({ LeafName: ellipsisStr });
        }
        return <div className="right">
            <div className="nTitle" tabIndex="0">
                <div>{notificationMsg}</div>
            </div>
            <div className="nBody">
                {
                    notificationMenuItems.map((item, key) => {
                        return <div className="nDescription" tabIndex="0" key={key}>{item.LeafName}</div>;
                    })
                }
            </div>
        </div>;
    }

    onDeleteNotification(index) {
        this.notificationCacheData = this.notificationCacheData.filter((item, idx) => {
            return index != idx;
        });
        let notificationHtml = this.getNotificationHtml();
        this.dispatch('raNotification', notificationHtml, this.notificationCacheData);
    }

    initTableNavBarBtnAllow() {
        return {
            holdManagement: true,
            reclassify: false,
            move: false,
            hold: false,
            related: false,
            declareAsRecord: false,
            removeDeclareAsRecord: false,
            lockWithRecordsLabel: false,
            unlockWithRecordsLabel: false,
            allowSetPermissions: false,
            phyEdit: false,
            phyDelete: false,
            phyLoan: false,
            phyMoveRequest: false,
            allowShowAudit: false,
        };
    }

    onManageHoldAction(key) {
        let selectItems = this.props.getSelectedItems();
        if (!this.checkOperateLimitSuccess(selectItems)) {
            return;
        }
        if (key == "new") {
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
        let fsFolder = selectItems.find(r => r.NodeType == 2100);
        if (selectItems.length == 1 && fsFolder == undefined) {
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
                        removeHoldSelectDialogShow: { show: true },
                        selectedPhyObjByTableOrTree: selectItems
                    });
                }
            }).catch((e) => {
                //console.log(e);
            });
        } else {
            this.setState({
                showRemoveHoldDialog: { show: true },
                selectedItems: selectItems,
            });
        }
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

    onLoanPhyObj() {
        let selectedTableItems = this.props.getSelectedItems();
        let unsupportedBulkAction = true;
        if (!this.checkOperateLimitSuccess(selectedTableItems, unsupportedBulkAction)) {
            return;
        }
        let selectedItemIds = selectedTableItems.map((item) => item.Id);
        this.onCheckItemsOnHold(selectedItemIds, () => {
            let loanData = {
                Items: selectedTableItems.map((item) => {
                    return {
                        Id: item.Id,
                        Name: item.LeafName,
                        UniqueId: item.RecordsId,
                        NodeType: item.NodeType,
                    };
                })
            };
            this.setState({
                showLoanPanel: { show: true },
                phyObjLoanParam: loanData
            });
        });
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
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((result) => {
                if (result.HasError) {
                    errorCallBack(result, () => this.setState({ showLoanPanel: { show: false } }));
                } else {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_LoanRequestSuccessMsg);
                    this.setState({
                        showLoanPanel: { show: false }
                    });
                }
                $$.loading(false);
            }).catch((e) => {

            });
        };
        this.dispatch(this.peLoanId, 'onSave', callback);
        return false;
    }
    newHoldValidation(selectedRecords) {
        this.openHoldForm(selectedRecords, "new");  //RECO-5058
    }

    onRemoveHoldConfirm() {
        let errorMsg = RMResx.RM_JS_RDM_Hold_CancelRecordError;
        let selRecIds = [];
        this.state.selectedItems.forEach((item, index) => {
            selRecIds.push(item.Id);
        });
        let selectItems = this.props.getSelectedItems();
        let containsFolder = selectItems.find((item) => (item.NodeType == 2100));
        let firstRecord = selectItems[0];
        let isPhysical = false;
        if (firstRecord) {
            isPhysical = firstRecord.SourceFlag == 4;
        }
        let postData = { recordsId: selRecIds, isPhysical: isPhysical };
        let option = {
            url: "/api/RecordsExplorerApi/CancelHoldByRecords",
            method: "POST",
            data: postData
        };
        $$.loading(true);
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            if (result == "") {
                if (containsFolder) {
                    this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>);
                }
                else {
                    this.showMessageTip("success", RMResx.RM_JS_RDM_Explorer_RemoveSuccessMsg);
                }
                this.loadData();
                $$.loading(false);
            } else {
                let tipMsg = result.message || errorMsg;
                this.showMessageTip("error", tipMsg);
                $$.loading(false);
            }
            this.onCancelRemoveHold();
        }).catch((e) => {
            this.showMessageTip("error", errorMsg);
            this.onCancelRemoveHold();
            $$.loading(false);
        });
    }

    onSingleRemoveHoldConfirm() {
        if (this.refSelectHoldValid && !$$.verify(this.refSelectHoldValid.ref.current)) {
            return false;
        }
        let errorMsg = RMResx.RM_JS_RDM_Hold_CancelRecordError;
        let selRecIds = [];
        let selectItems = this.props.getSelectedItems();
        selectItems.map((item, index) => {
            selRecIds.push(item.Id);
        });
        let containsFolder = selectItems.find((item) => (item.NodeType == 2100));
        let firstRecord = selectItems[0];
        let isPhysical = false;
        if (firstRecord) {
            isPhysical = firstRecord.SourceFlag == 4;
        }
        let selectedHoldIds = this.state.removeHoldProfileList.filter(h => h.checked).map(t => t.value);
        let selectedHoldName = this.state.removeHoldProfileList.filter(h => h.checked).map(t => t.holdName);
        let postData = { recordsId: selRecIds, isPhysical: isPhysical, removeHoldIds: selectedHoldIds };
        let option = {
            url: "/api/RecordsExplorerApi/CancelSelectedHoldByRecords",
            method: "POST",
            data: postData
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            if (result == "") {
                if (containsFolder) {
                    this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>);
                }
                else {
                    let message = <$g.I18NProvider msg={RMResx.RM_PRM_Explorer_RemoveSuccess}>
                        {selectedHoldName.join(", ")}
                    </$g.I18NProvider>;
                    showToast.success(message);
                }
                this.loadData();
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
        this.setState({ showRemoveHoldDialog: { show: false }, removeHoldSelectDialogShow: { show: false } });
    }

    expanderShown() {
        this.setState({});
    }

    openHoldForm(selectedRecords, operate) {
        let formData = {
            formType: operate,
            records: selectedRecords,
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
        let holdType = "";
        if (selectedRecords && selectedRecords.length > 0 && selectedRecords[0].SourceFlag == 4) {
            holdType = "phy";
        }
        this.setState({
            showHoldPanel: { show: true },
            formPanelTitle: title,
            holdFormParam: formData,
            holdType: holdType
        });
    }

    onSaveHoldFile() {
        $$.loading(true);
        $$.messagedialog(false);
        let isOverWrite = this.state.isOverWriteSubFiles;
        this.dispatch('sHoldForm', 'onSaveElectronicHold', (success, data) => {
            $$.loading(false);
            if (success) {
                if (data.formType == "new") {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_PutOnHoldSuccessMsg);
                } else if (data.formType == "extend") {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_ExtendHoldSuccessMsg);
                } else if (data.formType == "change") {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_ChangeHoldSuccessMsg);
                } else if (data.formType == "append") {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_AppendHoldSuccessMsg);
                }
                this.loadData();
                this.setState({ showHoldPanel: { show: false } });
            }
        }, isOverWrite);
        return false;
    }

    onSaveHoldPhy() {
        $$.loading(true);
        this.dispatch('sHoldForm', 'onSavePhyHold', (success, data) => {
            $$.loading(false);
            if (success) {
                if (data.formType == "new") {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_PutOnHoldSuccessMsg);
                } else if (data.formType == "extend") {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_ExtendHoldSuccessMsg);
                } else if (data.formType == "change") {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_ChangeHoldSuccessMsg);
                } else if (data.formType == "append") {
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_AppendHoldSuccessMsg);
                }
                this.loadData();
                this.setState({ showHoldPanel: { show: false } });
            }
        });
        return false;
    }

    onSaveHoldFolder() {
        $$.loading(true);
        $$.messagedialog(false);
        let isOverWrite = this.state.isOverWriteSubFiles;
        this.dispatch('sHoldForm', 'onSaveElectronicHold', (success, data) => {
            $$.loading(false);
            if (success) {
                if (data.formType == "new") {
                    if (data.records[0].NodeType == 2100) {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                    else {
                        this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_PutOnHoldSuccessMsg);
                    }
                } else if (data.formType == "extend") {
                    if (data.records[0].NodeType == 2100) {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                    else {
                        this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_ExtendHoldSuccessMsg);
                    }
                } else if (data.formType == "change") {
                    if (data.records[0].NodeType == 2100) {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                    else {
                        this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_ChangeHoldSuccessMsg);
                    }
                } else if (data.formType == "append") {
                    if (data.records[0].NodeType == 2100) {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                    else {
                        this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_AppendHoldSuccessMsg);
                    }
                }
                this.setState({ showHoldPanel: { show: false } });
            }
        }, isOverWrite);
        return false;
    }

    showFSFolderHoldOption() {
        let selectItems = this.props.getSelectedItems();
        let conatinsFolder = selectItems.find((item) => (item.NodeType == 2100));
        if (conatinsFolder) {
            if (this.state.holdFormParam.formType == "change" || this.state.holdFormParam.formType == "extend") {
                this.showWarningMessageBox();
            } else if (this.state.holdFormParam.formType == "append") {
                this.onSaveHoldFolder();
            } else {
                this.showOverWriteMessageBox();
            }
        }
        else {
            if (selectItems[0].SourceFlag == 4) {
                this.onSaveHoldPhy();
            } else {
                this.onSaveHoldFile();
            }
        }
        return false;
    }

    showWarningMessageBox() {
        this.setState({ isOverWriteSubFiles: false });
        let optionMsgContent = (
            <React.Fragment>
                <div>{RMResx.RM_JS_BCM_ChangeHoldFileForFolder_Message}</div>
            </React.Fragment>
        );
        let title = RMResx.RM_JS_BCM_Explorer_Button_ChangeHold;
        if (this.state.holdFormParam.formType == "extend") {
            optionMsgContent = (
                <React.Fragment>
                    <div>{RMResx.RM_JS_BCM_ExtendHoldFileForFolder_Message}</div>
                </React.Fragment>
            );
            title = RMResx.RM_JS_BCM_Explorer_Button_SuspendHold;
        }
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: title,
            content: (
                <div>
                    <div>{optionMsgContent}</div>
                </div>
            ),
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false, this.args);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.onSaveHoldFolder.bind(this),
                },

            ],
        };
        $$.messagedialog(true, this.args);
    }

    showOverWriteMessageBox() {
        this.setState({ isOverWriteSubFiles: false });
        let optionMsgContent = (
            <React.Fragment>
                <div>{RMResx.RM_JS_BCM_HoldFileForFolder_Message}</div>
                <div className="margin-top-15"></div>
                <R.Checkbox
                    name="checkbox-fs-folder-opt"
                    text={RMResx.RM_JS_BCM_HoldFileUnderFolder_Message}
                    title={RMResx.RM_JS_BCM_HoldFileUnderFolder_Message}
                    checked={this.state.isOverWriteSubFiles}
                    onChange={this.onCheckChangeOverWrite}
                />
            </React.Fragment>
        );
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_BCM_Explorer_Hold,
            content: (
                <div>
                    <div>{optionMsgContent}</div>
                </div>
            ),
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false, this.args);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.onSaveHoldFolder.bind(this),
                },

            ],
        };
        $$.messagedialog(true, this.args);
    }

    onCheckTermGroup() {
        let selectedItems = this.props.getSelectedItems();
        let firstContainerId = "";
        let sourceFlag = SourceFlags.None;
        for (let index = 0; index < selectedItems.length; index++) {
            const element = selectedItems[index];
            if (index == 0) {
                sourceFlag = element.SourceFlag;
            }
            if (element.ContainerId) {
                firstContainerId = element.ContainerId;
                break;
            }
        }
        let containerSourceFlags = [SourceFlags.SP, SourceFlags.Exo, SourceFlags.OneDrive];
        if(containerSourceFlags.find(s => s == sourceFlag) && (!firstContainerId)){
            this.showReclassifyMessageBox(RMResx.RM_JS_BCM_Reclassify_MissingContainerIdErrorMessage);
            return;
        }
        
        let itemsId = [];
        selectedItems.forEach(item => {
            itemsId.push(item.Id);
        });
        let option = {
            url: '/api/RecordsExplorerApi/CheckItemsInTheSameSecurityGroup',
            method: "POST",
            data: itemsId
        };
        
        fetchUtility(option).then((res) => {
            if (res) {
                this.openReclassifyPanel();
            } else {
                this.showReclassifyMessageBox(RMResx.RM_JS_BCM_Reclassify_Message);
            }
        }).catch((e) => {

        });
    }

    showReclassifyMessageBox = (message) => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: message,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                        $$.messagedialog(false);
                    }
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    openReclassifyPanel = () => {
        let selectedItems = this.props.getSelectedItems();
        if (!this.checkOperateLimitSuccess(selectedItems)) {
            return;
        }
        this.setState({
            showReclassifyPanel: { show: true },
            selectedItems: selectedItems,
        });
    }

    openRelatedPanel() {
        let selectedItems = this.props.getSelectedItems();
        this.setState({
            showRelatedPanel: { show: true },
            selectedItems: selectedItems
        });
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
                    this.showMessageTip('success', RMResx.RM_JS_RDM_Explorer_RelatedRecordsOperationSuccessMsg);
                    this.setState({ showRelatedPanel: { show: false } });
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        };
        this.dispatch("elecRelatedForm", 'onSave', callback);
        return false;
    }

    openMovePanel() {
        let selectedItems = this.props.getSelectedItems();
        if (selectedItems != null && selectedItems[0] != null) {
            if (selectedItems[0].SourceFlag != 4) {
                if (!this.checkOperateLimitSuccess(selectedItems)) {
                    return;
                }
                this.setState({
                    showMovePanel: { show: true },
                    selectedItems: selectedItems
                });
            } else {
                if (!this.checkOperateLimitSuccess(selectedItems, true)) {
                    return;
                }
                let moveData = {
                    Source: selectedItems,
                    isGlobalSearch: true,
                };

                //PhyBox: 9300,
                //PhyFile: 9400,
                let smallNodeType = NodeType.PhysicalBottomLocation;
                for (let index = 0; index < selectedItems.length; index++) {
                    const element = selectedItems[index];
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
                this.setState({
                    showPhyMovePanel: { show: true },
                    phyObjMoveParam: moveData,
                    smallNodeType: smallNodeType,
                });
            }
        }
    }

    openPhyMoveRequestPanel() {
        let selectedItems = this.props.getSelectedItems();
        if (!this.checkOperateLimitSuccess(selectedItems, true)) {
            return;
        }

        let moveData = {
            Source: selectedItems,
            isGlobalSearch: true,
        };

        let smallNodeType = NodeType.PhysicalBottomLocation;
        for (let index = 0; index < selectedItems.length; index++) {
            const element = selectedItems[index];
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
                    Name: item.LeafName,
                    UniqueId: item.RecordsId,
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
                    this.showMessageTip("success", RMResx.RM_PRM_PRE_Msg_MovementSuccess);
                    this.setState({ showPhyMoveRequestPanel: { show: false } });

                    this.loadData();
                }
            }).catch((e) => {
                $$.loading(false);
            });
        };

        this.dispatch('phyMoveRequestTree', 'onSave', callback);
        return false;
    }

    phyMoveRequestPanelBtns() {
        return (
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showPhyMoveRequestPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.onSavePhyMoveRequest.bind(this)}
                />
            </>
        );
    }

    onDeclareAsRecord(isDeclare) {
        let selectItems = this.props.getSelectedItems();
        if (!this.checkOperateLimitSuccess(selectItems)) {
            return;
        }
        let message = isDeclare ? <div>
            <div>{RMResx.RM_BCM_Explorer_DeclareMsgTip}</div>
            <div style={{ color: "#cc0000", marginTop: "8px" }}>{RMResx.RM_BCM_Explorer_DeclareMsgTip_WarnForOD}</div>
        </div> : RMResx.RM_JS_BCM_Explorer_UndeclareMsgTip;
        this.args = {
            classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: message,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick() {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.onSureDeclare.bind(this, isDeclare)
                }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onSureDeclare(isDeclare) {
        let selectedTableItems = this.props.getSelectedItems();
        let isSelectedOneSourceFilter = this.isSelectedOneSource;
        // let needStartJob = isSelectedOneSourceFilter && (this.isSelectAll || selectedTableItems.length > limitCount);
        let needStartJob = isSelectedOneSourceFilter && this.isSelectResult;
        if (needStartJob) {
            let param = {
                IsRealTimeAction: false,
                FilterInfo: { QueryOption: { Values: this.searchOption } },
                Action: isDeclare ? GlobalSearchAction.DeclareRecords : GlobalSearchAction.UnDeclareRecords,
                SourceFlag: selectedTableItems[0].SourceFlag,
                ForceDiscoverAll: this.isSelectResult && isSelectedOneSourceFilter,
                RecordIds: selectedTableItems.map(i => i.Id),
            };
            let url = `/api/GlobalSearchApi/DoAction`;
            let option = {
                url: url,
                method: "POST",
                data: param
            };
            $$.loading(true);
            fetchUtility(option, response => {
                $$.loading(false);
                this.handleError(response);
            }).then((resultJson) => {
                $$.loading(false);
                $$.messagedialog(false);
                if (resultJson.MessageType == "0") {
                    if (resultJson.Extension) {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                }
                else {
                    this.showMessageTip("error", resultJson.ErrorMessage);
                }
            });
        } else {
            $$.loading(true);
            let url = '/api/RecordsExplorerApi/DeclareRecords';
            if (!isDeclare) {
                url = '/api/RecordsExplorerApi/UndeclareRecords';
            }
            let isCloseMsgbox = true;
            let ids = [];
            let selectedItems = this.props.getSelectedItems();
            for (let item of selectedItems) {
                ids.push(item.Id);
            }
            let option = {
                url: url,
                method: "POST",
                data: ids
            };
            fetchUtility(option, response => {
                $$.loading(false);
                this.handleError(response);
                isCloseMsgbox = false;
            }).then((result) => {
                $$.loading(false);
                let res = JSON.parse(result);
                if (res.MessageType == 0) {
                    this.updateNotificationTimer(res.Extension, 'declare', isDeclare, selectedItems);
                } else {
                    this.showMessageTip("error", res.ErrorMessage);
                }
                $$.messagedialog(false, this.args);
            }).catch((e) => {
                $$.loading(false);
                if (isCloseMsgbox) {
                    $$.messagedialog(false, this.args);
                }
            });
        }
    }

    onLockByRecordsLabel = (isLock) => {
        const selectItems = this.props.getSelectedItems();
        if (!this.checkOperateLimitSuccess(selectItems)) {
            return;
        }
        const message = isLock ? RMResx.RM_BCM_Explorer_LockMsgTips : RMResx.RM_JS_BCM_Explorer_UnLockMsgTips
        this.args = {
            classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: message,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => this.onSureLockByRecordsLabel(isLock)
                }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onSureLockByRecordsLabel = (isLock) => {
        let selectedTableItems = this.props.getSelectedItems();
        let isSelectedOneSourceFilter = this.isSelectedOneSource;
        // let needStartJob = isSelectedOneSourceFilter && (this.isSelectAll || selectedTableItems.length > limitCount);
        let needStartJob = isSelectedOneSourceFilter && this.isSelectResult;
        if (needStartJob) {
            let param = {
                IsRealTimeAction: false,
                FilterInfo: { QueryOption: { Values: this.searchOption } },
                Action: isLock ? GlobalSearchAction.DeclareRecords : GlobalSearchAction.UnDeclareRecords,
                SourceFlag: selectedTableItems[0].SourceFlag,
                ForceDiscoverAll: this.isSelectResult && isSelectedOneSourceFilter,
                RecordIds: selectedTableItems.map(i => i.Id),
            };
            let url = `/api/GlobalSearchApi/DoAction`;
            let option = {
                url: url,
                method: "POST",
                data: param
            };
            $$.loading(true);
            fetchUtility(option, response => {
                $$.loading(false);
                this.handleError(response);
            }).then((resultJson) => {
                $$.loading(false);
                $$.messagedialog(false);
                if (resultJson.MessageType == "0") {
                    if (resultJson.Extension) {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                }
                else {
                    this.showMessageTip("error", resultJson.ErrorMessage);
                }
            });
        } else {
            $$.loading(true);
            let url = '/api/RecordsExplorerApi/DeclareRecords';
            if (!isLock) {
                url = '/api/RecordsExplorerApi/UndeclareRecords';
            }
            let isCloseMsgbox = true;
            let ids = [];
            let selectedItems = this.props.getSelectedItems();
            for (let item of selectedItems) {
                ids.push(item.Id);
            }
            let option = {
                url: url,
                method: "POST",
                data: ids
            };
            fetchUtility(option, response => {
                $$.loading(false);
                this.handleError(response);
                isCloseMsgbox = false;
            }).then((result) => {
                $$.loading(false);
                let res = JSON.parse(result);
                if (res.MessageType == 0) {
                    this.updateNotificationTimer(res.Extension, isLock ? 'lock' : 'remove', isLock, selectedItems);
                } else {
                    this.showMessageTip("error", res.ErrorMessage);
                }
                $$.messagedialog(false, this.args);
            }).catch((e) => {
                $$.loading(false);
                if (isCloseMsgbox) {
                    $$.messagedialog(false, this.args);
                }
            });
        }
    }

    onSavePhyObj() {
        $$.loading(true);
        this.dispatch(this.peFormId, 'onSave', (success, data) => {
            if (success) {
                this.setState({ showFormPanel: { show: false } });
                if (data.formType == PhyObjFormType.CreatePhyObj) {
                    this.showMessageTip('success', RMResx.RM_PRM_PRE_Msg_NewItemSuccess);

                } else if (data.formType == PhyObjFormType.EditPhyObj) {
                    this.showMessageTip('success', RMResx.RM_PRM_PRE_Msg_EditItemSuccess);
                } else if (data.formType == PhyObjFormType.NewRequest) {
                    this.showMessageTip('success', RMResx.RM_PRM_PRE_Msg_NewRequesSuccess);
                }
            }
            this.loadData();
            $$.loading(false);
        });
        return false;
    }

    onSaveBulkUpdatePhyObj() {
        let isSelectedOneSourceFilter = this.isSelectedOneSource;
        let needStartJob = isSelectedOneSourceFilter && this.isSelectResult;
        if (needStartJob) {
            this.onBulkUpdateEditSaveStartJob(needStartJob);
        } else {
            this.onBulkUpdateEditSave();
        }
        return false;
    }

    onBulkUpdateEditSaveStartJob = (forceDiscoverAll) => {
        let selectedTableItems = this.props.getSelectedItems();
        let callback = (success, data) => {
            let param = {
                IsRealTimeAction: false,
                FilterInfo: { QueryOption: { Values: this.searchOption } },
                Action: GlobalSearchAction.PhysicalBulkUpdate,
                ActionExtension: data,
                SourceFlag: selectedTableItems[0].SourceFlag,
                ForceDiscoverAll: forceDiscoverAll,
                RecordIds: selectedTableItems.map(i => i.Id),
            };
            let url = `/api/GlobalSearchApi/DoAction`;
            let option = {
                url: url,
                method: "POST",
                data: param
            };
            $$.loading(true);
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((resultJson) => {
                $$.loading(false);
                if (resultJson.MessageType == "0") {
                    if (resultJson.Extension) {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                } else {
                    this.showMessageTip("error", resultJson.ErrorMessage);
                }
                this.setState({
                    showBulkUpdateFormPanel: { show: false }
                });
            });
        };
        this.dispatch(this.peBulkUpdateFormId, 'getMetaInfoData', callback);
    }

    onBulkUpdateEditSave = () => {
        let callback = (success, data) => {
            $$.loading(true);
            if (success) {
                if (data.formType == PhyObjFormType.EditPhyObj) {
                    this.showMessageTip('success', RMResx.RM_PRM_PRE_Msg_EditItemSuccess);
                    this.setState({
                        showBulkUpdateFormPanel: { show: false }
                    });
                }
            }
            this.loadData();
            $$.loading(false);
        };
        this.dispatch(this.peBulkUpdateFormId, 'onSave', callback);
    }

    onBeforeEditPhyObj() {
        let selectedItems = this.props.getSelectedItems();
        if (!this.checkOperateLimitSuccess(selectedItems)) {
            return;
        }
        if (selectedItems.length > 1) {
            this.onEditBulkUpdatePhyObj(selectedItems);
        } else {
            this.onEditPhyObj(selectedItems);
        }
    }

    onEditBulkUpdatePhyObj(selectedItems) {
        this.openBulkUpdateForm(PhyObjFormType.EditPhyObj, selectedItems[0].NodeType, selectedItems);
    }

    onEditPhyObj(selectedItems) {
        this.openForm(PhyObjFormType.EditPhyObj, selectedItems[0]);
    }

    // PhyObjFormType: formType
    openForm(formType, item) {
        let nodeType = item.NodeType;
        this.operateFormNodeType = nodeType;
        let locationName = item.CustomColumnDic && item.CustomColumnDic[PhysicalDefaultColumnIDs.HomeLocation] ? item.CustomColumnDic[PhysicalDefaultColumnIDs.HomeLocation].Name : "";
        let formData = {
            Id: item.Id,
            formType: formType,
            NodeType: nodeType,
            BoxId: item.BoxId,
            FileId: item.FileId,
            LocationName : locationName
        };

        this.setState({
            showFormPanel: { show: true },
            formPanelTitle: this.getFormPanelTitle(formType, nodeType),
            phyObjFormData: formData
        });
    }

    openBulkUpdateForm = (formType, nodeType, selectItems) => {
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
        };
        this.setState({
            showBulkUpdateFormPanel: { show: true },
            formPanelTitle: this.getFormPanelTitle(formType, nodeType),
            phyObjBulkUpdateData: bulkUpdateFormData
        });
    }

    setPanelHeader = (templateName) => {
        this.setState({
            formTemplateName: templateName,
        });
    }

    tableNavBarBtnSelectInMore(item) {
        switch (item.id) {
            case TableNavBarBtnInMore.phyDelete.id:
                this.onDeletePhyObj();
                break;
        }
    }

    onDeletePhyObj() {
        let currentPhyArr = this.props.getSelectedItems();
        this.validDelItemsHasChildren(true, currentPhyArr);
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
            classify: "warn",
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
        let selectItems = isOnlySingle == true ? currentPhyArr : this.tableSelectItems;
        let param = RM.deepcopy(selectItems).map((item)=>{
            if(!item.PersonHoldReleaseTime){
                delete item.PersonHoldReleaseTime;
            }
            return item;
        });

        let url = `/api/PhysicalRecordApi/PreDeletePhysicalObject`;
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            $$.loading(false);
            this.deletePhyObjMsg(isOnlySingle, currentPhyArr, JSON.parse(result));
        });
    }

    onDeletePhyObjMsgSureClick(isSingle, currentPhyArr) {
        $$.messagedialog(false, this.args);
        $$.loading(true);
        let param = RM.deepcopy(currentPhyArr).map((item)=>{
            if(!item.PersonHoldReleaseTime){
                delete item.PersonHoldReleaseTime;
            }
            return item;
        });
        let url = `/api/PhysicalRecordApi/DeletePhysicalObject`;
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            if (!result.HasError) {
                $$.loading(false);
                this.loadData();
                this.showMessageTip('success', RMResx.RM_PRM_PRE_Msg_DeleteItemSuccess);
            } else {
                this.showMessageTip('error', RMResx.RM_PRM_PRE_Msg_DeleteItemError);
            }
        }).catch((e) => {
            $$.loading(false);
        });
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

    showManagePermissionPanel() {
        let selectedItems = this.props.getSelectedItems();
        // if (!this.checkOperateLimitSuccess(selectedItems)) {
        //     return;
        // }
        this.setState({
            showPermissionPanel: { show: true },
            selectedItems: selectedItems,
        });
    }

    onShowAudit(){
        this.setState({
            showAuditPanel : true
        });
    }

    getShowActions() {
        if (this.isHoldManagerOnly) {
            return [];
        }

        if (this.isRoleDelegateAdmin && !this.state.canShowHoldByRecordsPermission) {
            return [];
        }

        let { related, move, reclassify, declareAsRecord, removeDeclareAsRecord, lockWithRecordsLabel, unlockWithRecordsLabel, allowSetPermissions, allowShowAudit, 
            del_personal_hold, phyEdit, phyLoan, phyMoveRequest, more } = this.state.tableNavBarBtnAllow;
        const isHideReclassifyBtnByApiSetting = this.props.isHideReclassifyBtnByApiSetting;
        let buttonsInfo = [
            { name: RMResx.RM_JS_BCM_Explorer_ChangeTerm, icon: "fia-term", onClick: this.onCheckTermGroup.bind(this, false), isShow: !isHideReclassifyBtnByApiSetting && reclassify },
            { name: RMResx.RM_PRM_PRE_Related, icon: "fia-related-records", onClick: this.openRelatedPanel.bind(this, false), isShow: related },
            { name: RMResx.RM_PRM_PRE_Move, icon: "fia-move", onClick: this.openMovePanel.bind(this), isShow: move },
            { name: RMResx.RM_JS_BCM_Explorer_Button_DeclareAsSharePointRecord, icon: "fia-declare-as-record", onClick: this.onDeclareAsRecord.bind(this, true), isShow: declareAsRecord },
            { name: RMResx.RM_JS_BCM_Explorer_Button_LockByRecordsLabel, icon: "fia-lock", onClick: this.onLockByRecordsLabel.bind(this, true), isShow: lockWithRecordsLabel },
            { name: RMResx.RM_JS_BCM_Explorer_Button_UndeclareAsSharePointRecord, icon: "fia-remove-record-declaration", onClick: this.onDeclareAsRecord.bind(this, false), isShow: removeDeclareAsRecord },
            { name: RMResx.RM_JS_BCM_Explorer_Button_RemoveRecordsLabel, icon: "fia-lock-open", onClick: this.onLockByRecordsLabel.bind(this, false), isShow: unlockWithRecordsLabel },
            { name: RMResx.RM_PRM_GS_Permission_Btn_Default, icon: "fia-manage-access-control", onClick: this.showManagePermissionPanel.bind(this), isShow: allowSetPermissions },
            { name: RMResx.RM_PRM_PRE_Return, icon: "fia-return", onClick: this.onRemovePersonHoldConfirmTablePhyObj.bind(this), isShow: del_personal_hold },
            { name: RMResx.RM_PRM_PRE_Edit, icon: "fia-edit", onClick: this.onBeforeEditPhyObj.bind(this), isShow: phyEdit },
            { name: RMResx.RM_PRM_PRE_NewLoanRequest, icon: "fia-loan", onClick: this.onLoanPhyObj.bind(this), isShow: phyLoan },
            { name: RMResx.RM_PRM_PRE_MovementRequest, icon: "fia-move", onClick: this.openPhyMoveRequestPanel.bind(this), isShow: phyMoveRequest },
            { name: RMResx.RM_PRM_PRE_Delete, icon: "fia-delete", onClick: this.onDeletePhyObj.bind(this), isShow: more },
            { name: RMResx.RM_PRM_PRE_ShowAudit, icon: "fia-select-all", onClick: this.onShowAudit.bind(this), isShow: allowShowAudit },
        ];
        let showButtons = buttonsInfo.filter((item) => { return item.isShow; });
        return showButtons;
    }

    exportButton = () => {
        this.setState({
            isCurrentViewChecked: true,
            isExportToBrowser: true,
            exportDialogShow: { show: true },
        });
    }

    onExportColumnChanged = (args) => {
        this.setState({ isCurrentViewChecked: args });
    }

    exportDialogContent = () => {
        return <React.Fragment>
            <div id="exportDialog">
                <div className="ra-exportdialog-contenttitle require">{RMResx.RM_HS_ExportColumn}</div>
                <R.Radio.Group
                    block={true}
                    name="radioExportColumn"
                    items={this.getRadioExportColumn()}
                    onChange={this.onExportColumnChanged}
                />
            </div>
        </React.Fragment>;
    }

    renderExplorerTableNavBar() {
        let canManagementHold = this.isHoldManagerOnly
            || this.isSupAdmin
            || (this.currentSelectedItemDataSource == SourceFlags.Phy ? this.isPhysicalAdmin : this.isDelegateAdmin);

        const isDelegateHoldOnlyMode = this.isRoleDelegateAdmin && !this.state.canShowHoldByRecordsPermission;
        canManagementHold = canManagementHold && (isDelegateHoldOnlyMode ? true : this.state.canShowHoldByRecordsPermission);

        return <div className='ra-nav-bar'>
            <div className="nav-bar-left flex">
                <div className='nav-bar-icon flex'>
                    {(this.props.getExportInfo().totalCount != 0) && <R.Button
                        primary={true}
                        classify="theme"
                        title={RMResx.RM_HS_Export}
                        text={RMResx.RM_HS_Export}
                        onClick={this.exportButton} />}
                    {(!this.state.tableNavBarBtnAllow.holdManagement && canManagementHold) && this.getTableBarHoldButtons()}
                    <TopButtonsComponent
                        ref={r => this.refTopButtons = r}
                        data={{ menuBtnItems: this.getShowActions() }}
                        showCount={this.state.showCountButton}
                    ></TopButtonsComponent>
                </div>
            </div>
        </div>;
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
                    <ElecReclassifyForm
                        id='elecReclassifyForm'
                        data={this.state.selectedItems}>
                    </ElecReclassifyForm>
                </div>
            </div>
            {this.reclassifyPanelBtns()}
        </R.Panel>;
    }

    renderRelatedRecordsPanel() {
        return <R.Panel
            id="relatedPanel"
            header={RMResx.RM_PRM_PRE_MRR_Title}
            size={1000}
            status={this.state.showRelatedPanel}
            destroy={true}
        >
            <div className="ra-panel-content reclassify-panel">
                <div id="reclassify-content">
                    <ElecRelatedForm
                        id='elecRelatedForm'
                        data={this.state.selectedItems}
                    > </ElecRelatedForm>
                </div>
            </div>
            {this.relatedPanelBtns()}
        </R.Panel>;
    }

    renderManagePermissionPanel() {
        let panelTitle = RMResx.RM_PRM_GS_Permission_Btn_Default;
        return <R.Panel
            id="raPhyManagePermissionPanel"
            header={panelTitle}
            size={600}
            status={this.state.showPermissionPanel}
            onHide={this.hideManagePermissionPanel}
            destroy={true}
        >
            <div>
                <PhyObjectManagePermission
                    id='raPhyObjectManagePermission'
                    data={this.state.selectedItems || []}
                    globalSearch={true}
                ></PhyObjectManagePermission>
            </div>
            {this.managePermissionPanelBtns()}
        </R.Panel>;
    }

    renderMovePanel() {
        let selectedItems = this.props.getSelectedItems();
        let sourceType = selectedItems.length > 0 ? selectedItems[0].SourceFlag : null;
        return <R.Panel
            id="movePanel"
            header={RMResx.RM_JS_BCM_Explorer_Button_MoveRecords}
            size={630}
            status={this.state.showMovePanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <ElecMoveForm
                    id="moveTree"
                    sourceType={sourceType}>
                </ElecMoveForm>
            </div>
            {this.movePanelBtns()}
        </R.Panel>;
    }

    renderPhyMovePanel() {
        return <R.Panel
            id="movePanel"
            header={RMResx.RM_PRM_PRE_Move}
            size={600}
            status={this.state.showPhyMovePanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <PhyObjectMove
                    id="phyMoveTree"
                    data={this.state.phyObjMoveParam}
                    smallNodeType={this.state.smallNodeType}
                ></PhyObjectMove>
                {this.renderPhyMoveHoldConflictDialog()}
            </div>
            {this.phyMovePanelBtns()}
        </R.Panel>;
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
            {this.phyMoveRequestPanelBtns()}
        </R.Panel>;
    }

    onCancelPhyMoveHoldConflict() {
        this.setState({ physicalMoveHoldConflictDialogShow: false });
    }

    renderPhyMoveHoldConflictDialog() {
        return <R.Dialog
            id="PhyMoveHoldConflictDialog"
            header={RMResx.RM_PRM_Hold_Conflicted_FormTitle}
            width={500}
            height={400}
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
                        onChange={this.onPhysicalMoveHoldConflict.bind(this)}
                        value={this.state.selectedMoveHoldConflict}>
                        <$g.RadioOption value="1" text={RMResx.RM_PRM_Move_Hold_Conflicted_OverrideByDestination} />
                        <$g.RadioOption value="2" text={RMResx.RM_PRM_Move_Hold_Conflicted_Compare} />
                    </$g.RadioGroup>
                </$g.FormRow>
            </div>
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_OK} onClick={this.sendMoveRequestWithConflictResolution.bind(this)} />
        </R.Dialog>;
    }

    onPhysicalMoveHoldConflict(val) {
        this.setState({ selectedMoveHoldConflict: val });
    }

    onSaveMove() {
        let callback = (moveData, errorCallBack) => {
            let selectedTableItems = this.state.selectedItems;
            moveData.SourceRecords = selectedTableItems;
            let isSelectedOneSourceFilter = this.isSelectedOneSource;
            let needRunDiscover = this.isSelectResult && isSelectedOneSourceFilter;
            let param = {
                IsRealTimeAction: false,
                FilterInfo: { QueryOption: { Values: this.searchOption } },
                Action: GlobalSearchAction.MoveTo,
                ActionExtension: moveData,
                SourceFlag: selectedTableItems[0].SourceFlag,
                ForceDiscoverAll: needRunDiscover,
                RecordIds: selectedTableItems.map(i => i.Id),
            };
            let url = `/api/GlobalSearchApi/DoAction`;
            let option = {
                url: url,
                method: "POST",
                data: param
            };
            $$.loading(true);
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((resultJson) => {
                $$.loading(false);
                if (resultJson.MessageType == "0") {
                    if (resultJson.Extension) {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                }
                else {
                    this.showMessageTip("error", resultJson.ErrorMessage);
                }
                this.setState({
                    showMovePanel: { show: false }
                });
            });
        };
        this.dispatch('moveTree', 'onSave', callback);
        return false;
    }

    onSavePhyMove() {
        let callback = (moveData, errorCallBack) => {
            if (moveData && moveData.Source[0].NodeType === NodeType.PhyRecord) {
                this.confirmMovePhysicalRecords(moveData, errorCallBack);
                return;
            }
            this.sendMoveRequest(moveData, errorCallBack);
        };

        this.dispatch('phyMoveTree', 'onSave', callback);
        return false;
    }

    managePermission = (returnData) => {
        let reqParam = {
            QueryDto: null,
            QueryV3Dto: {
                QueryOption: {
                    OrderColumn: null,
                    Values: this.searchOption,
                }
            },
            NodeIds: null,
            Accounts: returnData.userList,
            UserConflictOption: returnData.selConflictType
        };

        let selectedItems = this.props.getSelectedItems();
        if (selectedItems && selectedItems.length > 0) {
            reqParam.NodeIds = selectedItems.map(o => { return o.Id; });
        }

        let isSelectedOneSourceFilter = this.isSelectedOneSource;
        let needRunDiscover = this.isSelectResult && isSelectedOneSourceFilter;

        let param = {
            IsRealTimeAction: false,
            FilterInfo: { QueryOption: { Values: this.searchOption } },
            Action: GlobalSearchAction.AccessControl,
            ActionExtension: reqParam,
            SourceFlag: selectedItems[0].SourceFlag,
            ForceDiscoverAll: needRunDiscover,
            RecordIds: selectedItems.map(i => i.Id),
        };
        let url = `/api/GlobalSearchApi/DoAction`;
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        $$.loading(true);
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == "0") {
                if (result.Extension) {
                    if (this.isPhysicalEndUser || this.isStandardReviewUser) {
                        this.showMessageTip("success", RMResx.RM_JS_MA_JobSucessMessage);
                    } else {
                        this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    }
                }
                this.hideManagePermissionPanel();
            }
            else {
                this.showMessageTip("error", result.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    hideManagePermissionPanel = () => {
        this.setState({
            showPermissionPanel: { show: false },
        });
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
            HoldConflictOption: moveData.HoldConflictOption,
            FromModule: AuditModule.PhysicalRecordsGlobalSearch,
        };
        $$.loading(true);
        let url = `/api/RecordsExplorerApi/PhysicalMove`;
        let option = {
            url: url,
            method: "POST",
            data: moveSendData
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
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
                    showPhyMovePanel: { show: false },
                    physicalMoveHoldConflictDialogShow: false
                });
                this.updateNotificationTimer(resultData.Extension, 'move', null, moveData.Source, moveData.Target.Id);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onViewDetail(args) {
        this.sourceFlag = args.SourceFlag;
        this.recordData = args;
        if(this.sourceFlag >= this.connectorRecordMinSourceFlag){
            this.setConnectorRecordsDetail();
        }else{
            switch(this.sourceFlag){
                case 4:
                    this.setState({
                        phyObjDetailParam: {
                            isRequest: false,
                            id: args.Id,
                            nodeType: args.NodeType,
                            BoxId: args.BoxId,
                            FileId: args.FileId,
                            RecordsId: args.RecordsId
                        },
                        showPhyViewDetailPanel: { show: true }
                    });
                    break;
                default:
                    this.setState({
                        viewDetailParam: { id: args.Id, isArchived: false },
                        showViewDetailPanel: { show: true }
                    });

            }
        }
    }

    setConnectorRecordsDetail(){
        let option = {
            url: "/api/Connector/ViewItemDetailForExplorerSearch",
            method: "POST",
            data: this.recordData.Id
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            this.setState({
                connectorDetail: result,
                showViewDetailPanel: { show: true }
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onSavePermission() {
        let callback = (data, success) => {
            this.managePermission(data);
        };
        this.dispatch('raPhyObjectManagePermission', 'onSave', callback);
        return false;
    }

    handleRemoveHoldCheckboxChanged = (value, oldValue) => {
        // console.log('list: ', value);
        // console.log(this.state.removeHoldProfileList);
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
                <span className="phy-head">{RMResx.RM_JS_BCM_Explorer_Template} </span>
                <span className="phy-head margin-xs">{this.state.formTemplateName}</span>
            </div>
            <div className="ra-panel-content">
                <PhyObjectForm
                    id={this.peFormId}
                    data={this.state.phyObjFormData}
                    type='phy'
                    setPanelTitle={this.setPanelHeader}
                    showMsgBar={true}
                >
                </PhyObjectForm>
            </div>
            {this.formPanelBtns()}
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
                    type='phy'
                    setPanelTitle={this.setPanelHeader}
                    showMsgBar={true}
                >
                </PhyObjBulkUpdateForm>
            </div>
            {this.bulkUpdatePanelBtns()}
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
            {this.loanPanelBtns()}
        </R.Panel>;
    }

    renderRemoveHoldDialog() {
        let selectedItems = this.props.getSelectedItems();
        let sourceType = selectedItems.length > 0 ? selectedItems[0].SourceFlag : null;
        let isPhysical = sourceType == 4;
        let containsFolder = selectedItems.find((item) => (item.NodeType == 2100));
        return <R.Dialog
            id="removeHoldConfirmDialog"
            header={RMResx.RM_JS_BCM_Explorer_Button_CancelHold}
            width={480}
            status={this.state.showRemoveHoldDialog}
            struct={{ foot: true }}
            onHide={this.onCancelRemoveHold.bind(this)}
            destroy={true}
        >
            <div id="removeHoldDialog_body" className="phyhold-expander">
                <div className="hold-dialog-removehold-tip">
                    {isPhysical ? RMResx.RM_PRM_PRE_Dialog_RemoveReminder : containsFolder ? RMResx.RM_PRM_PRE_Dialog_FSRemoveHold : RMResx.RM_JS_RDM_Hold_CancelHoldDes}
                </div>
                <div>
                    <div className="hold-dialog-removehold-details">{RMResx.RM_PRM_PRE_Dialog_RemoveItemPrefix}</div>
                    <div className="hold-dialog-removehold-list" tabIndex="0">
                        {
                            this.state.selectedItems.map((item, index) => {
                                return <div
                                    key={"item" + index}
                                    className="hold-dialog-removehold-item" data-tooltip="diffneed">
                                    <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Dialog_WithReleaseTime}>
                                        <span>{item.LeafName + " (" + item.RecordsId + ")"}</span>
                                        <span>{item.ReleaseTime}</span>
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

    selectHoldValid = () => {
        let selectedHoldIds = this.state.removeHoldProfileList.filter(h => h.checked).map(t => t.value);
        return selectedHoldIds.length > 0 ? true : RMResx.RM_PRM_PRE_Dialog_SelectHoldErrorValid;
    }

    renderRemoveHoldSelectDialog() {
        return <R.Dialog
            id="removeHoldConfirmDialog"
            header={RMResx.RM_JS_BCM_Explorer_Button_CancelHold}
            width={480}
            status={this.state.removeHoldSelectDialogShow}
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
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSingleRemoveHoldConfirm.bind(this)} />
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

    onRemovePersonHoldConfirmTablePhyObj() {
        let selectedItems = this.props.getSelectedItems();
        if (!this.checkOperateLimitSuccess(selectedItems)) {
            return;
        }
        this.setState({
            removePersonHoldDialogShow: true,
            selectedItems: selectedItems
        });
    }

    onRemovePersonHold() {
        $$.loading(true);
        let errorMsg = RMResx.RM_JS_RDM_PersonHold_CancelRecordError;
        let phyObjIDs = this.props.getSelectedItems().map((item) => item.Id);
        let hasPermission = true;
        let option = {
            url: "/api/PhysicalRecordApi/RemovePersonalHold",
            method: "POST",
            data: phyObjIDs
        };
        fetchUtility(option, response => {
            this.handleError(response);
            hasPermission = false;
        }).then((result) => {
            $$.loading(false);
            if (result.success) {
                if (result.isStartJob && this.isPhysicalAdmin) {
                    showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>);
                } else {
                    showToast.success(RMResx.RM_JS_RDM_Explorer_ReturnSuccessMsg);
                    this.loadData();
                }
            } else {
                this.showMessageTip("error", result.message || errorMsg);
            }
            this.onCancelRemovePersonHold();
        }).catch((e) => {
            if (hasPermission) { this.showMessageTip("error", errorMsg); }
            this.onCancelRemovePersonHold();
            $$.loading(false);
        });
    }

    onCancelRemovePersonHold = () => {
        this.setState({ removePersonHoldDialogShow: false });
    }

    onDownloadFile = (recordData) => {
        $$.loading(true);
        //2 download job已经finish
        if (recordData.Record.ContentDownloadStatus == 2) {
            let urlData = "/api/RecordsExplorerApi/GetDownloadSasById";
            let option = {
                url: urlData,
                method: "Post",
                data:recordData.Record.Id,
            };
            $$.loading(false);
            fetchUtility(option).then((res) => {
                //DB里已经存在Uri
                if (res != null && res != "") {
                    window.open(res, "_blank");
                } else {
                    let downloadFile = recordData.Record.Id;
                    var $downloadStatusKey = $("#downloadFlag");
                    $downloadStatusKey.val(downloadFile);

                    $("#downloadFile")
                        .attr(
                            "action",
                            "/api/RecordsExplorerApi/DownloadArchivedContent"
                        )
                        .submit();
                }
            });
        } else if (recordData.Record.ContentDownloadStatus == 1) {  //1 有job再跑还没完成
            if (!this.isNewLogicAccount) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_DC_DownloadSuccessMessage}>
                    <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
                $$.loading(false);
            }
            else {
                showToast.success(
                    <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                        <a className="ra-link-a" href="/Root/JM/Index">
                            {RMResx.RM_JS_JM_Title}
                        </a>
                        <a className="ra-link-a" href="/Root/DC/Download">
                            {RMResx.RM_JS_DC_Title}
                        </a>
                    </$g.I18NProvider>
                );
                $$.loading(false);
            }
            
        } else {
            let urlData = "/api/RecordsExplorerApi/StartRestoreArchivedContent";
            let option = {
                url: urlData,
                method: "POST",
                data: [recordData.Record.Id]
            };
            fetchUtility(option).then((res) => {
                let resultData = JSON.parse(res);
                if (resultData.MessageType == 0) {  //0 Successful
                    if (!this.isNewLogicAccount) {
                        let content = <$g.I18NProvider msg={RMResx.RM_JS_DC_DownloadSuccessMessage}>
                            <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                        </$g.I18NProvider>;
                        showToast.success(content);
                        $$.loading(false);
                    }
                    else {
                        showToast.success(
                            <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                                <a className="ra-link-a" href="/Root/JM/Index">
                                    {RMResx.RM_JS_JM_Title}
                                </a>
                                <a className="ra-link-a" href="/Root/DC/Download">
                                    {RMResx.RM_JS_DC_Title}
                                </a>
                            </$g.I18NProvider>
                        );
                    }
                } else {
                    showToast.error(RMResx.RM_JS_DC_DownloadFailedMessage);
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }
        this.setState({ showViewDetailPanel: { show: false } });
    }

    getRecordData = (data) => {
        this.detailRecordData = data;
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
                            this.state.selectedItems.map((item, index) => {
                                return <div
                                    key={"item" + index}
                                    className="phyhold-expander-item">
                                    {item.PersonHoldReleaseTime &&
                                        <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Dialog_WithReturnTime}>
                                            <span>{item.LeafName + " (" + item.RecordsId + ")"}</span>
                                            <span>{item.PersonHoldReleaseTime}</span>
                                        </$g.I18NProvider>}
                                    {!item.PersonHoldReleaseTime && <span>{item.LeafName + " (" + item.RecordsId + ")"}</span>}
                                </div>;
                            })
                        }
                    </div>
                </R.Expander>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelRemovePersonHold} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onRemovePersonHold.bind(this)} />
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
                <HoldForm
                    id="sHoldForm"
                    data={this.state.holdFormParam}
                    type={this.state.holdType}
                    extend={"search"}
                >
                </HoldForm>
            </div>
            {this.holdPanelBtns()}
        </R.Panel>;
    }

    renderElecDetailContent(){
        return <ElecDetailForm
            data={this.state.viewDetailParam}
            sourceFlag={this.sourceFlag}
            detailRecordData={this.getRecordData}
        ></ElecDetailForm>;
    }

    renderConnectorDetailContent(){
        return <React.Fragment>
            <div className='ra-section-head'>
                <span tabIndex="0">{RMResx.RM_Report_SectionTitle_BaseInfo}</span>
            </div>
            <$g.DetailList>
                {this.state.connectorDetail.map((item, index) => {
                    return <$g.DetailRow key={index}>
                        <$g.DetailCell label={item.Name}>
                            <span className="ra-pre-wrap">{item.Value}</span>
                        </$g.DetailCell>
                    </$g.DetailRow>;
                })}
            </$g.DetailList>
        </React.Fragment>;
    }

    renderViewDetailPanel() {
        let detailContent = this.sourceFlag >= this.connectorRecordMinSourceFlag ? this.renderConnectorDetailContent() : this.renderElecDetailContent();
        return <R.Panel
            id="recExpViewDetail"
            header={RMResx.RM_PRM_PRE_PanelTitle_ViewDetail}
            size={664}
            status={this.state.showViewDetailPanel}
            destroy={true}
        >
            <div>
                {detailContent}
            </div>
            {this.getViewDetailDownloadBtn()}
        </R.Panel>;
    }

    renderPhyViewDetailPanel() {
        return <R.Panel
            id="viewDetailPanel"
            header={RMResx.RM_PRM_PRE_PanelTitle_ViewDetail}
            size={664}
            status={this.state.showPhyViewDetailPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <PhyObjectDetail
                    ref={r => { this.phyObjectDetailRef = r; }}
                    data={this.state.phyObjDetailParam}
                ></PhyObjectDetail>
            </div>
            {this.getViewDetailPanelBtns()}
        </R.Panel>;
    }

    exportLimit = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_HS_Export_DataLimit.format(this.exportLimitCount),
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
        };
        $$.messagedialog(true, args);
    }

    confirmMovePhysicalRecords = (moveData, errorCallBack) => {
        this.args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_Confirm_MovePhyRecord,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => this.sendMoveRequest(moveData, errorCallBack)
                }
            ]
        };
        $$.messagedialog(true, this.args);
    };

    onExportClick = () => {
        let columnsList = [];
        if (this.state.isCurrentViewChecked) {
            this.props.getExportInfo().tableColumns.forEach(index => {
                if (index.header != "" && index.visible) {
                    columnsList.push({ DisplayName: index.header, UniqueId: index.NameHash });
                }
            });
        } else {
            this.props.getExportInfo().tableColumns.forEach(index => {
                if (index.header != "") {
                    columnsList.push({ DisplayName: index.header, UniqueId: index.NameHash });
                }
            });
        }
        // $$.loading(true);
        let orderColumn = {
            OrderAsc: this.props.getExportInfo().orderAsc,
            Column: this.props.getExportInfo().orderColumns
        };
        let exportObj = {};
        exportObj.SelectedColumns = columnsList;
        exportObj.FilterInfo = {
            QueryOption: {
                OrderColumn: this.props.getExportInfo().orderColumns ? orderColumn : null,
                Values: this.searchOption
            }
        };
        this.handleExportLocal(exportObj);
        this.setState({ exportDialogShow: { show: false } });
    }

    onCancelClick = () => {
        this.setState({ exportDialogShow: { show: false } });
    }

    onHide = () => {
        this.setState({ showAuditPanel: false });
    }

    renderExportDialog() {
        return <R.Dialog
            id={'normal_dialog'}
            header={RMResx.RM_HS_Export}
            width={464}
            status={this.state.exportDialogShow}
        >
            {this.exportDialogContent()}
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelClick} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_RDM_Explorer_ExportBarcode_DialogExportBtn} onClick={this.onExportClick} />
            </>
        </R.Dialog>;
    }

    async handleExportLocal(value) {
        let requestOption = {
            url: "/api/GlobalSearchApi/StartExportSearchResultJob",
            data: value,
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);

        if (result.MessageType === 0) {
            showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
            </$g.I18NProvider>);
        } else {
            showToast.error(result.ErrorMessage);
        }
    }

    renderExportForm() {
        let requestVerificationToken = getRequestVerificationToken();
        return <form id="exportForm" method="post" action="">
            <input type="hidden" id="exportFlag" name="exportFlag" value="" />
            <input type="hidden" id="globalSearchExport" name="globalSearchExport" value="" />
            <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
        </form>;
    }

    renderDownloadForm() {
        let requestVerificationToken = getRequestVerificationToken();
        return <form id="downloadFile" method="post" action="">
            <input type="hidden" id="downloadFlag" name="fileIdString" value="" />
            <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
        </form>;
    }

    renderAuditPanel(){
        let selectItem = this.props.getSelectedItems();
        return (
            <ShowAuditInfoPanel
                item={selectItem}
                show={this.state.showAuditPanel}
                onHide={this.onHide}
            />)
    }

    render() {
        return <div id={this.props.id}>
            {this.renderExplorerTableNavBar()}
            {this.renderReclassifyPanel()}
            {this.renderRelatedRecordsPanel()}
            {this.renderManagePermissionPanel()}
            {this.renderMovePanel()}
            {this.renderPhyMovePanel()}
            {this.renderFormPanel()}
            {this.renderLoanPanel()}
            {this.renderHoldPanel()}
            {this.renderRemoveHoldDialog()}
            {this.renderRemoveHoldSelectDialog()}
            {this.renderViewDetailPanel()}
            {this.renderPhyViewDetailPanel()}
            {this.renderRemovePersonHoldDialog()}
            {this.renderExportDialog()}
            {this.renderExportForm()}
            {this.renderDownloadForm()}
            {this.renderBulkUpdateFormPanel()}
            {this.renderAuditPanel()}
            {this.renderPhyMoveRequestPanel()}
        </div>;
    }
}