import "../../../Less/CP/accountManagement.less";
import TableRow from "./RowTemplate";
import AccessPermissionForm from "./AccessPermissionForm";
import {SourceFlags, SubPermission, PhyUserRoleType} from "../../../Constants/Constants";
export default class UsersManagement extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            columns: this.initTableColumns(),
            userList: [],
            shownCount: 0,
            totalCount: 0,
            pagerIndex: 0,
            pagerSize: 10,
            searchValue: "",
            sortBy: "",
            isAscending: false,
            isShowPermissionPanel: {show: false},
            userPermissionInfo: [],
            spContainerItems: this.props.spContainerItems,
            exoContainerItems: this.props.exoContainerItems,
            oneDriveContainerItems: this.props.oneDriveContainerItems,
            teamsContainerItems: this.props.teamsContainerItems,
            phyContainerItems: this.props.phyContainerItems,
            isAdmin: false,
            scopePermissionInfo: [],
            isUseReportingPermissionControl: false,
            reportingPermission: 0,
            isEnableManageHolds: false,
            isEnableManageApprovalSettings: false,
        };
        this.bind(["handlePageChange", "cellClick", "onSearch", "onTableSort", "getUserPermissionItems"]);
    }

    componentInit() {
        this.loadUsers();
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        this.setState({
            spContainerItems: nextProps.spContainerItems.slice(),
            exoContainerItems: nextProps.exoContainerItems.slice(),
            oneDriveContainerItems: nextProps.oneDriveContainerItems.slice(),
            // teamsContainerItems: nextProps.teamsContainerItems.slice(),
            phyContainerItems: nextProps.phyContainerItems.slice(),
        });
    }

    initTableColumns() {
        return [
            {
                headerTemplate: RMResx.RM_CP_AM_Table_Column_UserName,
                width: 320,
                isResizable: true,
                sortable: true,
                valuePath: "DisplayName"
            }, {
                header: RMResx.RM_CP_AM_Table_Column_ParentGroups,
                width: 560,
                isResizable: true
            }, {
                header: RMResx.RM_CP_AccountManagement_Column_Action,
                width: 240,
                isResizable: true
            }
        ];
    }

    loadUsers() {
        $$.loading(true);
        let option = {
            url: "/api/CPApi/QueryUsers",
            method: "POST",
            data: {
                PageIndex: this.state.pagerIndex + 1,
                PageSize: this.state.pagerSize,
                SearchValue: this.state.searchValue,
                SortBy: this.state.sortBy,
                IsAscending: this.state.isAscending
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setState({
                shownCount: res.Users.length,
                totalCount: res.TotalCount,
                userList: res.Users
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onTableSort(args) {
        this.setState({
            sortBy: args.column.valuePath,
            isAscending: args.status == "asc" ? true : false
        }, () => {
            this.loadUsers();
        });
    }

    onRowEvent = (args) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.cellClick(rowData);
                break;
        }
    }

    cellClick(rowData) {
        $$.loading(true);
        let option = {
            url: `/api/CPApi/GetUserPermissionScopes?id=${rowData.UserId}`,
            method: "Get",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res)
            {
                var data = JSON.parse(res);
                this.setState({
                    userPermissionInfo: this.getUserPermissionItems(data),
                    isShowPermissionPanel: {show: true},
                    isAdmin: data.IsAdmin,
                    scopePermissionInfo: data.ScopePermissionInfo,
                    functionMoudleRestoreCenter : data.FunctionMoudleRestoreCenter,
                    isUseReportingPermissionControl: data.IsUseReportingPermissionControl,
                    reportingPermission: data.ReportingPermission,
                    isEnableManageHolds : data.IsEnableManageHold,
                    isEnableManageApprovalSettings : data.IsEnableApprovalSetting,
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getUserPermissionItems(data)
    {
        let result = {
            ScopePermissionInfo: [],
            TermPermissionInfo: data.TermPermissionInfo,
            RulePermissionInfo: data.RulePermissionInfo
        };
        let items = this.getDefaultUserPermissionItems();
        data.ScopePermissionInfo.map((o)=>{
            var permissionItem = items.find(t => t.dataSourceType == o.DataSourceType);
            if(data.IsAdmin)
            {
                permissionItem.showExpander = true;
                permissionItem.scopesNameOrPath = [RMResx.RM_CP_AM_AllScope_Title];
            }else {
                permissionItem.showExpander = o.IsScopeAdmin? true : false;
                switch(o.DataSourceType)
                {
                    case SourceFlags.SP:
                        permissionItem.scopesNameOrPath = this.getContainerNames(o.ScopeIds, RM.deepcopy(this.state.spContainerItems));
                        break;
                    case SourceFlags.OneDrive:
                        permissionItem.scopesNameOrPath = this.getContainerNames(o.ScopeIds, RM.deepcopy(this.state.oneDriveContainerItems));
                        break;
                    case SourceFlags.Exo:
                        permissionItem.scopesNameOrPath = this.getContainerNames(o.ScopeIds, RM.deepcopy(this.state.exoContainerItems));
                        break;
                    case SourceFlags.Teams:
                        permissionItem.scopesNameOrPath = this.getContainerNames(o.ScopeIds, RM.deepcopy(this.state.teamsContainerItems));
                        break;
                    case SourceFlags.Phy:
                        if(o.IsScopeAdmin)
                        {
                            if(o.SubPermission == PhyUserRoleType.Admin)
                            {
                                permissionItem.scopesNameOrPath = this.getContainerNames(o.ScopeIds, RM.deepcopy(this.state.phyContainerItems));
                                permissionItem.userRoleType = PhyUserRoleType.Admin;

                            }else {
                                permissionItem.scopesNameOrPath = [RMResx.RM_CP_AM_AllScope_Title];
                                permissionItem.userRoleType = PhyUserRoleType.EndUser;
                                permissionItem.phySubPermissions = this.getPhySubPermissionNames(o);
                            }
                        }
                        break;
                    case SourceFlags.FS:
                    case SourceFlags.SPLocal:
                    case SourceFlags.AzureFile:
                    case SourceFlags.Box:
                    case SourceFlags.Google:
                        if(o.IsScopeAdmin)
                        {
                            permissionItem.scopesNameOrPath = [RMResx.RM_CP_AM_AllScope_Title];
                        }
                        break;
                }
            }
        });
        result.ScopePermissionInfo = items;
        return result;
    }

    getPhySubPermissionNames(phyScopeInfo)
    {
        let names = [];
        let subPermissions = phyScopeInfo.SubPermissions;
        if(subPermissions && subPermissions.length > 0)
        {
            subPermissions.map(o => {
                switch(o)
                {
                    case SubPermission.SetAccessControl:
                        names.push(RMResx.RM_CP_AM_Phy_SubPermission_SetAccessControl);
                        break;
                    case SubPermission.FolderCreationRequest:
                        names.push(RMResx.RM_CP_AM_Phy_SubPermission_FolderCreationRequest);
                        break;
                    case SubPermission.FolderLoanRequest:
                        names.push(RMResx.RM_CP_AM_Phy_SubPermission_FolderLoanRequest);
                        break;
                    case SubPermission.BoxCreationRequest:
                        names.push(RMResx.RM_CP_AM_Phy_SubPermission_BoxCreationRequest);
                        break;
                    case SubPermission.FolderLoanReturn:
                        names.push(RMResx.RM_CP_AM_Phy_SubPermission_FolderLoanReturn);
                        break;
                }
            });
        }
        return names;
    }

    getDefaultUserPermissionItems()
    {
        return  [
            {
                dataSourceType: SourceFlags.SP,
                title: RMResx.RM_JS_SPS_TabLabel_SP,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            },
            {
                dataSourceType: SourceFlags.OneDrive,
                title: RMResx.RM_JS_SPS_TabLabel_OneDrive,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            },
            {
                dataSourceType: SourceFlags.Teams,
                title: RMResx.RM_JS_SPS_TabLabel_Teams,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            },
            {
                dataSourceType: SourceFlags.Exo,
                title: RMResx.RM_JS_SPS_TabLabel_EXO,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            },
            {
                dataSourceType: SourceFlags.Phy,
                title: RMResx.RM_JS_SPS_TabLabel_Physical,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            },
            {
                dataSourceType: SourceFlags.FS,
                title: RMResx.RM_JS_SPS_TabLabel_FS,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            },
            {
                dataSourceType: SourceFlags.SPLocal,
                title: RMResx.RM_JS_SPS_TabLabel_SPLocal,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            },
            {
                dataSourceType: SourceFlags.AzureFile,
                title: RMResx.RM_JS_SPS_TabLabel_AZS,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            },
            {
                dataSourceType: SourceFlags.Box,
                title: RMResx.RM_JS_SPS_TabLabel_Box,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            }, 
            {
                dataSourceType: SourceFlags.Google,
                title: RMResx.RM_JS_SPS_TabLabel_Google,
                scopesNameOrPath: [],
                showExpander: false,
                userRoleType: 0
            }
        ];
    }

    getContainerNames(scopeIds, allContainers)
    {
        let names = [];
        var containerItems = allContainers.filter(o => scopeIds.indexOf(o.Id) > -1);
        if(containerItems && containerItems.length > 0)
        {
            names = containerItems.map((item) => {return item.Name;});
        }
        return names;
    }

    handlePageChange = (pagerIndex, pagerSize, callback) => {
        this.getPager(pagerIndex, pagerSize);
        callback(true);
    };

    onSearch(args) {
        let searchValue = $.trim(args);

        this.setState({
            searchValue: searchValue,
            pagerIndex: 0
        }, () => {
            this.loadUsers();
        });
    }

    getPager(pagerIndex, pagerSize) {
        this.setState(
            {
                pagerIndex: pagerIndex,
                pagerSize: pagerSize,
            },
            () => {
                this.loadUsers();
            }
        );
    }

    renderTable() {
        return <div className='ra-main-table'>
            <R.Table
                id="raAccountManagementTable"
                rootData={this.state.rootData}
                columns={this.state.columns}
                rowTemplate={TableRow}
                items={this.state.userList}
                onRowEvent={this.onRowEvent}
                doSort={this.onTableSort}
            />
        </div>;
    }

    renderPager() {
        return (
            <div className="ra-main-footer">
                <$g.Pager
                    itemsCount={this.state.totalCount}
                    pagerIndex={this.state.pagerIndex}
                    pagerSize={this.state.pagerSize}
                    showPagerSize={true}
                    showPagerCounter={true}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={this.handlePageChange}
                />
            </div>
            // </div>
        );
    }

    renderAccessPermissionPanel() {
        return <div>
            <R.Panel
                id="raViewAccessPermission"
                header={RMResx.RM_CP_AccountManagement_ViewAccessLocation}
                size={600}
                status={this.state.isShowPermissionPanel}
                destroy={true}
            >
                <div>
                    <AccessPermissionForm
                        userPermissionInfo={this.state.userPermissionInfo}
                        isAdmin={this.state.isAdmin}
                        scopePermissionInfo={this.state.scopePermissionInfo}
                        functionMoudleRestoreCenter={this.state.functionMoudleRestoreCenter}
                        isUseReportingPermissionControl={this.state.isUseReportingPermissionControl}
                        reportingPermission={this.state.reportingPermission}
                        isEnableManageHolds={this.state.isEnableManageHolds}
                        isEnableManageApprovalSettings={this.state.isEnableManageApprovalSettings}
                    ></AccessPermissionForm>
                </div>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Close}
                    primary={true}
                    classify="theme"
                    onClick={() => {
                        this.setState({ isShowPermissionPanel: { show: false } });
                    }}
                />
            </R.Panel>
        </div>;
    }

    renderNavBar() {
        return (
            <div className="ra-main-header">
                <div className="navbar-search">
                    <R.Searchbox
                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                        disabled={false}
                        onSearch={this.onSearch}
                        width={380}
                    />
                </div>
            </div>
        );
    }

    render() {
        return (
            <div id="raUserManagement">
                {this.renderNavBar()}
                {this.renderTable()}
                {this.renderPager()}
                {this.renderAccessPermissionPanel()}
            </div>
        );
    }
}