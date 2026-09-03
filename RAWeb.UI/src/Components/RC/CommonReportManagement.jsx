import { withRouter } from 'react-router-dom';
import ReportTable from "./Components/Table";
import { checkPermission } from '../../Utilities/permissionManager';
import TermUsageViewDetail from "./TermUsageReport/ViewDetail";
import DueDisposalViewDetail from "./DueDisposalReport/ViewDetail";
import CreationAndDestructionViewDetail from "./CreationAndDestructionReport/ViewDetail";
import AvailiableSpaceViewDetail from "./AvailiableSpaceReport/ViewDetail";
import ActionAuditViewDetail from "./ActionAuditReport/ViewDetail";
import RestoreReportViewDetail from "./RestoreReport/ViewDetail";

import "../../Less/RC/commonReportManagement.less";
import { Fragment } from 'react';
import { ReportType } from './Constants';
import { EnvironmentHelper, LicenseHelper, showToast } from '../../Utilities/CommonUtil';
import { SourceFlag } from '../Common/Constants';
import StorageOptimizationViewDetail from './SOReport/Profile/ViewDetail';

const isEnableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();
export default withRouter(class CommonReportManagement extends R.Component {
    idAttr = true;
    componentCreate() {
        this.deleteMessageBoxArgs = {};
        this.deleteErrorMessageBoxArgs = {};
        this.filterData = this.getDefaultPager();
        this.isSelectAll = false;

        this.cacheItems = [];
        this.tableColumns = this.getColums();
        this.state = {
            batchActionDisable: false,      //编辑按钮置灰
            singleSelection: false,
            batchSelection: false,
            dialogShow: false,          //dialog显示隐藏
            selectedItems: [],
            shownCount: 0,
            profilesCount: 0,             //分页数据总数
            profilesPagerIndex: 0,         //分页每页的条数
            profilesPagerSize: 10,         //分页每页条数
            MessageTipInfo: {
                showTip: false,
                type: "success",
                content: ""
            },
            viewRowId: -1,
            panelShow: false,
            noneMessage: RMResx.RM_JS_RC_RUR_SearchNoData,
            isDeleteRelated: false,
            menuDisable: true,
            createItems:
                [
                    { text: RMResx.RM_JS_Common_ReportType_ForSharePoint, func: this.handleNewSPReport, disabled: false },
                    {
                        text: RMResx.RM_JS_Common_ReportType_ForExchangeOnline,
                        func: this.handleNewEXOReport,
                        disabled: false
                    }
                ]
        };
        //bindEvents(this, "onCheckChanged", );
    }

    getColums() {
        return [
            {
                header: RMResx.RM_JS_RC_DueDisposal_ProfileName,
                width: 260,
                resizeable: true
            }, {
                header: RMResx.RM_JS_RC_Common_ReportType,
                width: 260,
                resizeable: true
            }, {
                header: RMResx.RM_JS_RC_DueDisposal_Description,
                resizeable: true,
                width: 460
            }, {
                header: RMResx.RM_JS_RC_ReportColumn_LastModifiedTime,
                valuePath: "Modified",
                sortable: true,
                resizeable: true,
                width: 500
            }];
    }

    componentInit() {
        this.checkProfileStatus();
        this.getProfilesFromServer();
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl
        });
    }

    checkProfileStatus() {
        var status = RM.CommStatus.get();
        if (status) {
            var contentMessage = status == RM.CommStatus.CreateSuccess ? RMResx.RM_JS_RC_TUR_CreateProfileSuccess : RMResx.RM_JS_RC_TUR_EditProfileSuccess;
            showToast._showMsg("success", contentMessage);
            RM.CommStatus.remove();
        }
    }

    getDefaultPager() {
        let pager = {
            PageIndex: 1,
            PageSize: 10,
            TotalCount: 0,
            CheckedCount: 0,
            Type: this.props.type,
            IsDesc: true,
            Reports: [],
            Search: []
        };
        return pager;
    }

    resetCacheData(items) {
        let deletedItemIds = [];
        let deletedItem = [];
        for (let item of items) {
            deletedItemIds.push(item.Id);
        }
        for (let key in this.cacheItems) {
            if (deletedItemIds.indexOf(this.cacheItems[key].Id) != -1) {
                delete this.cacheItems[key];
            }
            if (this.cacheItems[key]) {
                deletedItem.push(this.cacheItems[key]);
            }
        }
        this.cacheItems = deletedItem;
    }

    getProfilesFromServer(isKeepSelectedPagerIndex) {
        $$.loading(true);
        let pager = this.filterData;
        if (!isKeepSelectedPagerIndex) { pager.PageIndex = 1; }
        let option = {
            url: this.props.getDataUrl,
            method: "POST",
            data: pager
        };
        fetchUtility(option).then((res) => {
            //刷新列表
            let data = JSON.parse(res);
            if (data.TotalCount != 0) {
                for (let prof of data.Profiles) {
                    this.addCacheItem(prof);
                    if (prof.Type == 14 || (prof.Type >= 4000 && prof.Type < 5000)) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_Physical;
                    } else if (prof.Type < 1000 || prof.Type == 8000 || prof.Type == 11600) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_SharePoint;
                    } else if (prof.Type >= 1000 && prof.Type < 3000) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_Exchange;
                    } else if (prof.Type >= 5000 && prof.Type < 5100) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_FileSystem;
                    } else if ((prof.Type >= 6100 && prof.Type < 6200) || prof.Type == 8019 || prof.Type == 11601) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_OneDrive;
                    } else if (prof.Type >= 5510 && prof.Type < 5515) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_SPOnPrem;
                    } else if (prof.Type >= 10101 && prof.Type < 10110) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_Box;
                    } else if (prof.Type >= 10200 && prof.Type < 10300 || prof.Type == 11603) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_GoogleDrive;
                    } else if (prof.Type >= 10300 && prof.Type < 10400 || prof.Type == 11602) {
                        prof.ReportType = RMResx.RM_JS_Common_ReportType_Teams;
                    }
                }
            }
            this.dispatch("reportTable", data.Profiles, this.tableColumns);
            this.setState({
                profilesCount: data.TotalCount,
                profilesPagerIndex: data.PageIndex - 1,
                shownCount: data.Profiles.length,
                deleteBtnDisabled: true,
                editBtnDisabled: true,
                selectedItems: this.cacheItems.filter(t => t.isChecked)
            });
            this.setBtnState(this.cacheItems.filter(t => t.isChecked));
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    deleteMessageBox(items) {
        let profileNames = "";
        for (let i = 0; i < items.length; i++) {
            let data = items[i];
            profileNames += i != items.length - 1 ? `${data.ProfileName}, ` : data.ProfileName;
        }
        this.deleteMessageBoxArgs = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content:
                <div>
                    <div>
                        <$g.I18NProvider msg={RMResx.RM_JS_RC_TUR_DeleteProfileMsg}>{profileNames}</$g.I18NProvider>
                    </div>
                    <div className="ra-report-deleteJob">
                        <R.Checkbox
                            id="raRcManagementDeleteJob"
                            name="checkbox-demo1"
                            text={RMResx.RM_JS_RC_TUR_DeleteJobs}
                            title={RMResx.RM_JS_RC_TUR_DeleteJobs}
                            checked={this.state.isDeleteRelated}
                            onChange={this.handleDeleteRelatedChange}
                        />
                    </div>
                </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: this.handleDeleteCancelClick
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.handleDeleteSureClick.bind(this, items)
                }
            ]
        };
        this.setState({ isDeleteRelated: false });
        $$.messagedialog(true, this.deleteMessageBoxArgs);
    }

    getProfilesPager(pagerIndex, pagerSize) {
        let pager = this.filterData;
        pager.PageIndex = pagerIndex + 1;
        pager.PageSize = pagerSize;
        this.setState({
            profilesPagerIndex: pagerIndex,
            profilesPagerSize: pagerSize
        });
        this.getProfilesFromServer(true);
    }

    // handle event

    handleNewSPReport = () => {
        this.routerTo(this.props.newSPUrl);
    };

    handleNewEXOReport = () => {
        this.routerTo(this.props.newEXOUrl);
    };

    handleNewPhysicalReport = () => {
        this.routerTo(this.props.newPhysicalUrl);
    };

    handleNewFSReport = () => {
        this.routerTo(this.props.newFSUrl);
    };

    handleNewOneDriveReport = () => {
        this.routerTo(this.props.newOneDriveUrl);
    }

    handleNewGoogleReport = () => {
        this.routerTo(this.props.newGoogleUrl);
    }

    handleNewTeamsReport = () => {
        this.routerTo(this.props.newTeamsUrl);
    }

    handleNewSPOnPremiseReport = () => {
        this.routerTo(this.props.newSPOnPremiseUrl);
    }
    
    handleNewBoxReport = () => {
        this.routerTo(this.props.newBoxUrl);
    }

    handleNewGoogleDriveReport = () => {
        this.routerTo(this.props.newGoogleDriveUrl);
    }

    handleEditProfile = (isCellOperate, item) => {
        if (isCellOperate) {
            this.routerTo(`${this.props.editUrl}?type=${item.Type}&id=${item.Id}`);
        } else {
            let sel = this.getSelectedItems()[0];
            this.routerTo(`${this.props.editUrl}?type=${sel.Type}&id=${sel.Id}`);
        }
    };

    handleDeleteProfile = (isCellOperate, items) => {
        this.setState({
            isDeleteRelated: false
        }, () => {
            if (isCellOperate) {
                this.deleteMessageBox(items);
            } else {
                this.deleteMessageBox(this.getSelectedItems());
            }
        });

    };

    handleDeleteRelatedChange = () => {
        this.setState({ isDeleteRelated: !this.state.isDeleteRelated });
    };

    handleDeleteSureClick = (items) => {
        $$.messagedialog(false);
        $$.loading(true);
        let idArray = [];
        let nameArray = [];
        let jobType = items.length > 0 ? items[0].Type : -1;
        items.filter((item) => {
            idArray.push(item.Id);
            nameArray.push(item.ProfileName);
            return idArray;
        });

        let delDto = { Ids: idArray, Names: nameArray, DeleteJobs: this.state.isDeleteRelated, Type: jobType };
        let option = {
            url: this.props.deleteUrl,
            method: "POST",
            data: delDto
        };
        fetchUtility(option).then((resultList) => {
            $$.loading(false);
            if (resultList != null && resultList.length > 0) {
                this.deleteErrorMessageBoxArgs = {
                    // classify: "info",
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content:
                        <div>
                            <div>
                                <$g.I18NProvider msg={RMResx.RM_JS_RC_CantDeleteProfileInProgress}></$g.I18NProvider>
                            </div>
                            <ul style={{ paddingLeft: "60px" }}>{resultList.map((r, index) =>
                                <li key={r}>{r}</li>
                            )}</ul>
                        </div>,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_OK,
                            primary: true,
                            classify: "theme",
                            onClick: this.handleDeleteCancelClick
                        }
                    ]
                };
                $$.messagedialog(true, this.deleteErrorMessageBoxArgs);
            } else {
                showToast._showMsg("success", RMResx.RM_JS_RC_DueDisposal_DeleteProfileSucces);
                this.resetCacheData(items);
                this.getProfilesFromServer();
            }
        });
    };

    handleDeleteCancelClick = () => {
        $$.messagedialog(false);
    };

    showRunReportErrorMessage() {
        showToast._showMsg("error", RMResx.RM_JS_RC_DueDisposal_GenerateReportFailed);
    }
    showRunReportCommonErrorMessage() {
        showToast._showMsg("error", RMResx.RM_JS_RC_DueDisposal_GenerateReportFailedCommon);
    }

    handleGenerateReport = (isCellOperate, item) => {
        let selectData = null;
        if (isCellOperate) {
            selectData = item;
        } else {
            let selectDatas = this.getSelectedItems();
            if (selectDatas.length == 1) {
                selectData = selectDatas[0];
            } else {
                return;
            }
        }
        let profile = {
            Id: selectData.Id,
            Type: selectData.Type,
        };
        let option = {
            url: this.props.generateUrl,
            method: "POST",
            data: profile
        };
        fetchUtility(option, response => {
            if (response.status === 403) {
                showToast.error(RMResx.RM_JS_RC_DueDisposal_GenerateReportForbiden);
            } else {
                this.showRunReportCommonErrorMessage();
            }
        }).then((res) => {
            if (!res) {
                this.showRunReportErrorMessage();
            } else {
                let messageContent = <$g.I18NProvider msg={RMResx.RM_JS_RC_DueDisposal_GenerateReportSuccess}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(messageContent);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    };

    handleShowReport = (isCellOperate, item) => {
        let selectedItem = [];
        if (isCellOperate) {
            selectedItem = item;
        } else {
            selectedItem = this.getSelectedItems()[0];
        }
        $$.loading(true);
        let option = {
            url: `/api/RCApi/CommonShowReport?reportType=${this.props.type}&profileId=${selectedItem.Id}`,
            method: "GET",
        };
        fetchUtility(option).then((data) => {
            if (data.HasRanJob) {
                location.href = `${this.props.showReportUrl}?type=${selectedItem.Type}&id=${selectedItem.Id}`;
            } else {
                $$.loading(false);
                showToast.warn(RMResx.RM_RC_Msg_HasNoRunJob);
            }
        });
    };

    onSearchStart = (args) => {
        let searchValue = args;
        if (searchValue && searchValue != "") {
            let pager = this.filterData;
            pager.PageIndex = 1;
            pager.SearchValue = searchValue;
            this.clearCacheItems();
            this.getProfilesFromServer();
        }
    };

    onSearchStop = () => {
        let searchValue = "";
        let pager = this.filterData;
        pager.PageIndex = 1;
        pager.SearchValue = searchValue;
        this.getProfilesFromServer();
    };

    onSort = (args) => {
        let pager = this.filterData;
        pager.IsSort = true;
        pager.JumpPage = 1;
        pager.IsDesc = !(args.status === "asc");
        pager.SortBy = args.column.valuePath;
        this.getProfilesFromServer();


        // var arr = $$.sort(this.state.products, args.status === "asc", args.column.valuePath);
        // this.setState({ products: arr.slice() });
    };

    handleProfilesPageChange = (pagerIndex, pagerSize, callback) => {
        this.getProfilesPager(pagerIndex, pagerSize);
        callback(true);
    };

    handleReportDetailClick = (row) => {
        this.setState({ panelShow: true, viewRowId: row.Id });
    };

    handleReportDetailKeyDown(row, e) {
        if (e.keyCode == '13') {
            location.href = `${this.props.viewDetailsUrl}?id=${row.Id}`;
            //this.routerTo(`${this.props.viewDetailsUrl}?id=${row.Id}`);
        }
    }

    onCheckChanged = (items) => {
        let currentPageItems = items.slice();
        this.updateCacheItemsStatus(currentPageItems);
        this.setBtnState(this.cacheItems.filter(t => t.isChecked));
        this.setState({ selectedItems: this.cacheItems.filter(t => t.isChecked) });
    };

    cellOperate = (args, tableSelectedOption) => {
        switch (tableSelectedOption.index) {
            case 1: //edit
                this.handleEditProfile(true, args);
                break;
            case 2: //delete
                this.handleDeleteProfile(true, [args]);
                break;
            case 3: //generate
                this.handleGenerateReport(true, args);
                break;
            case 4: //delete
                this.handleShowReport(true, args);
                break;
        }
    };

    cellClick = (data, action) => {
        this.handleReportDetailClick(data);
    };

    setBtnState(items) {
        let selectOne = false;
        let selectMany = false;
        if (items.length == 1) {
            selectOne = true;
        } else if (items.length > 1) {
            selectMany = true;
        }
        this.setState({
            batchSelection: selectMany,
            singleSelection: selectOne,
            //batchActionDisable: !selectOne,
        });
    }

    addCacheItem(item) {
        let isExits = this.cacheItems.find(r => r.Id == item.Id);
        if (item.Id != '') {
            if (isExits == undefined) {
                item.isChecked = false;
                this.cacheItems.push(item);
            } else {
                this.updateItemCheckedStatus(item);
            }
        }
    }

    clearCacheItems() {
        this.cacheItems = [];
    }

    updateItemCheckedStatus(item) {
        let cacheItem = this.cacheItems.find(r => r.Id == item.Id);
        if (cacheItem !== undefined) {
            item.isChecked = cacheItem.isChecked;
        }
    }

    updateCacheItemsStatus(rowItems) {
        if (rowItems && rowItems.length > 0) {
            this.cacheItems.forEach((item, key) => {
                let rowItem = rowItems.find(t => t.Id == item.Id);
                if (rowItem !== undefined) {
                    item.isChecked = rowItem.isChecked;
                }
            });
        }
    }

    getSelectedItems() {
        return this.cacheItems.filter(t => t.isChecked);
    }

    isIncludeSource(specialSource, source) {
        if (specialSource == null || specialSource.length == 0) {
            return true;
        } else if (specialSource.find(s => s == source)) {
            return true;
        } else {
            return false;
        }
    }

    renderCreatButton = () => {

        if (this.state.selectedItems.length > 0) {
            return;
        }

        if (this.props.isSingleBtn) {
            return (
                <R.Button
                    id="raRcNewProfileBtn"
                    text={RMResx.RM_JS_Common_Create}
                    primary={true}
                    classify="theme"
                    onClick={this.handleNewSPReport} />
            );
        }
        const specialSource = this.props.specialSource;
        const createButtonList = [];
        if (LicenseHelper.HasOpusILLicense()) {
            if (this.isIncludeSource(specialSource, SourceFlag.Teams) && checkPermission("Source_Teams", RM.UserResources) && LicenseHelper.HasUpgradeTeams()) {
                createButtonList.push(
                    {
                        id: "raRcNewTeamsProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForTeams,
                        onClick: this.handleNewTeamsReport
                    }
                );
            }
            if (this.isIncludeSource(specialSource, SourceFlag.SharePoint) && checkPermission("Source_SP", RM.UserResources)) {
                createButtonList.push(
                    {
                        id: "raRcNewSpoProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForSharePoint,
                        onClick: this.handleNewSPReport
                    }
                );
            }
            if (this.isIncludeSource(specialSource, SourceFlag.OneDrive) && checkPermission("Source_OneDrive", RM.UserResources)) {
                createButtonList.push(
                    {
                        id: "raRcNewOneDriveProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForOneDrive,
                        onClick: this.handleNewOneDriveReport
                    }
                );
            }
            if (this.isIncludeSource(specialSource, SourceFlag.Exchange) && checkPermission("Source_EXO", RM.UserResources)) {
                createButtonList.push(
                    {
                        id: "raRcNewExoProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForExchangeOnline,
                        onClick: this.handleNewEXOReport
                    }
                );
            }
        }
        
        if (this.isIncludeSource(specialSource, SourceFlag.FileSystem) && checkPermission("Source_FS", RM.UserResources) && !EnvironmentHelper.IsGCPEnvironment && !isEnableJPMCFeature) {
            createButtonList.push(
                {
                    id: "raRcNewFsProfileBtn",
                    text: RMResx.RM_JS_Common_ReportType_ForFileSystem,
                    onClick: this.handleNewFSReport
                }
            );
        }
        if (this.isIncludeSource(specialSource, SourceFlag.SharePointOnPrem) && checkPermission("Source_LSP", RM.UserResources) && !EnvironmentHelper.IsGCPEnvironment) {
            createButtonList.push(
                {
                    id: "raRcNewLspProfileBtn",
                    text: RMResx.RM_JS_Common_ReportType_ForSPOnPrem,
                    onClick: this.handleNewSPOnPremiseReport
                }
            );
        }
        if (this.isIncludeSource(specialSource, SourceFlag.Box) && checkPermission("Source_Box", RM.UserResources)) {
            createButtonList.push(
                {
                    id: "raRcNewBoxProfileBtn",
                    text: RMResx.RM_JS_Common_ReportType_ForBox,
                    onClick: this.handleNewBoxReport
                }
            );
        }
        if (this.isIncludeSource(specialSource, SourceFlag.Physical) && checkPermission("Source_Phy", RM.UserResources)) {
            createButtonList.push(
                {
                    id: "raRcNewPhyProfileBtn",
                    text: RMResx.RM_JS_Common_ReportType_ForPhysical,
                    onClick: this.handleNewPhysicalReport
                }
            );
        }

        if (LicenseHelper.HasOpusGoogleLicense()) {
            if (this.isIncludeSource(specialSource, SourceFlag.GoogleDrive) && checkPermission("Source_Google", RM.UserResources)) {
                createButtonList.push(
                    {
                        id: "raRcNewGoogleDriveProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForGoogleDrive,
                        onClick: this.handleNewGoogleDriveReport
                    }
                );
            }
        }

        //Create group button for action audit and restored data report
        const SOReportTypeList = [ReportType.RestoreReport, ReportType.SPOActionAuditReport, ReportType.StorageOptimizationReport];
        if (SOReportTypeList.includes(this.props.type) && (LicenseHelper.HasOpusSOLicense() || LicenseHelper.HasOpusGoogleLicense())) {
            createButtonList.length = 0;
            if (checkPermission("Source_Teams", RM.UserResources) && LicenseHelper.HasUpgradeTeams()) { // Available in August
                createButtonList.push(
                    {
                        id: "raRcNewTeamsProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForTeams,
                        onClick: this.handleNewTeamsReport
                    }
                );
            }
            if (checkPermission("Source_SP", RM.UserResources)) {
                createButtonList.push(
                    {
                        id: "raRcNewSpoProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForSharePoint,
                        onClick: this.handleNewSPReport
                    }
                );
            }
            if (checkPermission("Source_OneDrive", RM.UserResources)) {
                createButtonList.push(
                    {
                        id: "raRcNewOneDriveProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForOneDrive,
                        onClick: this.handleNewOneDriveReport
                    }
                );
            }
            if (LicenseHelper.HasOpusGoogleLicense() && checkPermission("Source_Google", RM.UserResources) && (this.props.type === ReportType.RestoreReport)) {
                createButtonList.push(
                    {
                        id: "raRcNewGoogleProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForGoogleDrive,
                        onClick: this.handleNewGoogleReport
                    }
                );
            }

            if (LicenseHelper.HasOpusGoogleLicense()) {
                if (this.isIncludeSource(specialSource, SourceFlag.GoogleDrive) && checkPermission("Source_Google", RM.UserResources) && this.props.type === ReportType.StorageOptimizationReport) {
                createButtonList.push(
                    {
                        id: "raRcNewGoogleDriveProfileBtn",
                        text: RMResx.RM_JS_Common_ReportType_ForGoogleDrive,
                        onClick: this.handleNewGoogleDriveReport
                    }
                );
            }
        }
        }
        
        return (
            <span className="reco-report-management-creat-btn">
                <R.ButtonGroup
                    type="button"
                    classify="theme"
                    height={200}
                    text={RMResx.RM_JS_Common_Create}
                    tooltip={RMResx.RM_JS_Common_Create}
                >
                    {createButtonList.map((item, index) => {
                        return <R.Button
                            id={item.id}
                            key={item.text}
                            text={item.text}
                            onClick={item.onClick} />;
                    })}
                </R.ButtonGroup>
            </span>
        );
    }

    renderOtherButton = () => {
        const selectItemCount = this.state.selectedItems.length;

        if (selectItemCount === 0) {
            return;
        }

        if (selectItemCount === 1) {
            return (
                <Fragment>
                    <R.Button
                        id="raRcManagementEditBtn"
                        icon="fia-edit"
                        primary={false}
                        classify="default"
                        text={RMResx.RM_JS_Common_Edit}
                        onClick={this.handleEditProfile.bind(this, false)} />
                    <R.Button
                        id="raRcManagementGenerateBtn"
                        icon="fia-generate"
                        primary={false}
                        classify="default"
                        text={RMResx.RM_JS_Common_GenerateReport}
                        onClick={this.handleGenerateReport.bind(this, false)} />
                    <R.Button
                        id="raRcManagementShowReportBtn"
                        icon="fia-eye"
                        primary={false}
                        classify="default"
                        text={RMResx.RM_JS_Common_ShowReport}
                        onClick={this.handleShowReport.bind(this, false)} />
                    <R.ButtonGroup
                        id="raRcManagementMoreBtn"
                        type="action"
                        classify="default"
                        tooltip={RMResx.RM_PRM_PRE_More}
                    >
                        <R.Button
                            id="raRcManagementDeleteBtn"
                            text={RMResx.RM_JS_Common_Delete}
                            onClick={this.handleDeleteProfile.bind(this, false)} />
                    </R.ButtonGroup>
                </Fragment>
            );
        }

        return (
            <R.Button
                id="raRcManagementDeleteBtn"
                icon="fia-delete"
                primary={false}
                classify="default"
                text={RMResx.RM_JS_Common_Delete}
                onClick={this.handleDeleteProfile.bind(this, false)} />
        );
    };

    getViewDetailComponent(viewRowId) {
        if (viewRowId === -1) {
            return;
        }
        if (this.props.type === ReportType.BCSTermUsageReport) {
            return <TermUsageViewDetail key={viewRowId} viewRowId={viewRowId} />;
        }
        else if (this.props.type === ReportType.AvailableSpaceReport) {
            return <AvailiableSpaceViewDetail key={viewRowId} viewRowId={viewRowId} />;
        }
        else if (this.props.type === ReportType.ItemDueForDisposalReport) {
            return <DueDisposalViewDetail key={viewRowId} viewRowId={viewRowId} />;
        }
        else if (this.props.type === ReportType.CreationAndDestructionReport) {
            return <CreationAndDestructionViewDetail key={viewRowId} viewRowId={viewRowId} />;
        }
        else if (this.props.type === ReportType.SPOActionAuditReport) {
            return <ActionAuditViewDetail key={viewRowId} viewRowId={viewRowId} />;
        }
        else if (this.props.type === ReportType.RestoreReport) {
            return <RestoreReportViewDetail key={viewRowId} viewRowId={viewRowId} />;
        }
        else if(this.props.type === ReportType.StorageOptimizationReport) {
            return <StorageOptimizationViewDetail key={viewRowId} viewRowId={viewRowId} />;
        }
    }

    render() {

        return (
            <React.Fragment>
                <div className='reco-report-management-wrapper' id={this.props.id}>
                    <section className="reco-report-management-search-section">
                        <R.Searchbox
                            placeholder={RMResx.RM_JS_TM_SearchTxt}
                            disabled={false}
                            onSearch={(args) => (args || "").trim() === "" ? this.onSearchStop() : this.onSearchStart(args)}
                            width={380}
                            height={34}
                        />
                    </section>
                    <section className="reco-report-management-actions-section">
                        <div className="reco-report-management-actions">
                            {this.renderCreatButton()}
                            {this.renderOtherButton()}
                        </div>
                        <div className="reco-report-management-action-description">
                            {RMResx.RM_Common_SelectTableItemsCounter.format(this.state.selectedItems.length, this.state.profilesCount)}
                        </div>
                    </section>
                    <section className="reco-report-management-table-section">
                        <ReportTable
                            id="reportTable"
                            columnInfo={this.tableColumns}
                            onCheckChanged={this.onCheckChanged}
                            cellOperate={this.cellOperate}
                            cellClick={this.cellClick}
                            onSort={this.onSort}
                        />
                    </section>
                    <section className="reco-report-management-footer-section">
                        <$g.Pager
                            itemsCount={this.state.profilesCount}
                            pagerIndex={this.state.profilesPagerIndex}
                            pagerSize={this.state.profilesPagerSize}
                            showPagerSize={true}
                            showPagerCounter={true}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={this.handleProfilesPageChange} />
                    </section>
                </div>
                <R.Panel
                    header={RMResx.RM_JM_DetailsTitle}
                    status={{ show: this.state.panelShow }}
                    size={664}
                    onClose={() => this.setState({ panelShow: false })}
                    onHide={() => this.setState({ panelShow: false })}
                >
                    <div className="reco-report-detail-wrapper">
                        {this.getViewDetailComponent(this.state.viewRowId)}
                    </div>
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={() => this.setState({ panelShow: false })} />
                </R.Panel>
            </React.Fragment>
        );
    }
});