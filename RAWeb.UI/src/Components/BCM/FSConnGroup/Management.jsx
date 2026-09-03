import SiteMapLinks from "../../../Constants/SiteMapLinks";
//import RouterUrls from "../../../Constants/RouterUrls";
import GroupTable from "./Components/Table/GroupTable";
import ConnectionTable from "./Components/Table/ConnectionTable";
import ConnectionGroupSettings from "./Components/ConnectionGroupSettings";
import ConnectionSettings from "./Components/ConnectionSettings";
import CorrelateConnections from "./Components/CorrelateConnections";
import AddCorrelateConnections from "./Components/AddCorrelateConnections";
import TableDataCache from "./Components/TableDataCache";
import "../../../Less/CP/ConnectionGroupSettings.less";

import { ConnectionConfiguration } from "./Components/ConnectionConfiguration/index";
import RouterUrls from "../../../Constants/RouterUrls";
import { isShowActionByDC, isEnableMultiGeoFeature, LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import { cacheFilterDataType, filterConnectionCacheNamePrefix, manageColumnConnectionCacheName } from "./ConnectionDetails/Constants";
import ConnectionFilterForm from "./ConnectionDetails/ConnectionFillerForm";
import ViewConnectionDetailsPanel from "./Components/ViewConnectonDetailsPanel";

const enableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();
const enableMultiGeoFeature = isEnableMultiGeoFeature();
const defaultManagedColumns = [
    { isChecked: true, value: RMResx.RM_FS_Register_ConnectionName, Id: 0, isDynamic: true },
    { isChecked: true, value: RMResx.RM_FS_Register_JPMCId, Id: 1, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_Description, Id: 2, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_Path, Id: 3, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_Information_Owner, Id: 4, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_Records_Owner, Id: 5, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_GroupName, Id: 6, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_LastModifiedTime, Id: 7, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_Monitor, Id: 8, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_LastSyncTime, Id: 9, isDynamic: false },
];

const isMultiGeoMainDC = isShowActionByDC();
export default class FSConnGroupManagement extends R.Component {
    idAttr = true;
    componentCreate() {
        this.groupConfigRef = React.createRef();
        this.getDataUrl = '/api/ConnectionRegisterApi/GetAllGroups';
        this.getAllConnectionUrl = '/api/ConnectionRegisterApi/QueryConnectionsPager';
        this.getAllNoGroupConnectionUrl = '/api/ConnectionRegisterApi/GetAllNoGroupConnections';
        this.saveGroupUrl = '/api/ConnectionRegisterApi/SaveConnectionGroup';
        this.saveConnectionUrl = '/api/ConnectionRegisterApi/SaveConnection';
        this.getAllAgentsUrl = '/api/ConnectionRegisterApi/GetAllAgents';
        this.deleteConnectionUrl = '/api/ConnectionRegisterApi/DeleteConnection';
        this.deleteGroupUrl = '/api/ConnectionRegisterApi/DeleteGroup';
        this.correlateConnectionUrl = '/api/ConnectionRegisterApi/CorrelateConnection';
        this.checkConnectionHasSettings = '/api/ConnectionRegisterApi/CheckConnectionSettings';
        this.getAllGroupConnectionUrl = '/api/ConnectionRegisterApi/GetAllConnectionGroupNames';
        this.groupTableId = 'ra-conn-group-table';
        this.connectionTableID = 'ra-conn-table';
        this.groupSettingsPanelId = 'ra-conn-group-settings-panel';
        this.connectionSettingsPanelId = 'ra-connection-settings-panel';
        this.correlateConnectionPanelId = 'ra-correlate-connections-panel';
        this.addCorrelateConnectionPanelId = 'ra-add-correlate-connections-panel';
        this.connectionListLoaded = false;
        this.groupCacheData = new TableDataCache();
        this.connectionCacheData = new TableDataCache();
        this.cacheFilterData = RM.getSessionStorage(`${filterConnectionCacheNamePrefix}_FSFilterData`) || [];
        this.cacheManagedColumnsIds = RM.getSessionStorage(manageColumnConnectionCacheName);
        this.state = {
            groupPagerIndex: 0,
            groupPagerSize: 10,
            groupPagerTotal: 0,
            groupShownCount: 0,
            groupPanelTitle: '',
            connectionPanelTitle: '',
            correlateConnectionPanelTitle: RMResx.RM_FS_Register_EditCorrelateConnections,
            showGroupSettingsPanel: { show: false },
            showConnectionSettingsPanel: { show: false },
            showCorrelateConnectionPanel: { show: false },
            showAddCorrelateConnectionPanel: { show: false },
            selectedTabIndex: 0,
            connectionActionButtonsDisable: true,
            groupActionButtonsDisable: true,
            connectionPagerIndex: 0,
            connectionPagerSize: 10,
            connectionPagerTotal: 0,
            connectionShownCount: 0,
            groupName: '',
            checkGroupCount: 0,
            checkConnectionCount: 0,
            clickedGroupId: null,
            isFiltered: false,
            showFilterPanel: false,
            managedColumns: this.getCacheManagedColumns(),
            connectionGroupOptions: [],
            filterOptionsInfo: {},
            connectionColumn: [],
            filterData: {
                PageSize: 10,
                PageIndex: 1,
                SearchKey: "",
                Filters: this.cacheFilterData,
                Order: {
                    ColumnName: '',
                    IsDesc: false,
                }
            },
            showConnectionDetailsPanel: false,
        };
    }

    componentInit() {
        this.getGroupsFromServer();
        // this.getNoGroupConnectionsFromServer();  //This feature is no longer supported, so there is no need to call the API to optimize performance.
        this.initAgentOptions();
        this.initTableColumns();
    }

    initTableColumns() {

        const dataCenterColumn = {
            header: RMResx.RM_FS_Register_DataCenter,
            resizeable: true, 
            width: 260
        };

        this.groupTableColumns = [
            {
                header: RMResx.RM_FS_Register_GroupName,
                width: 260,
                resizeable: true
            }, {
                header: RMResx.RM_FS_Register_Description,
                width: 300,
                resizeable: true
            }, {
                header: RMResx.RM_FS_Register_Connections,
                resizeable: true,
                width: 300
            },
            ...(enableMultiGeoFeature ? [dataCenterColumn] : []),
            {
                header: RMResx.RM_FS_Register_Agent,
                resizeable: true,
                width: 260
            },
            {
                header: RMResx.RM_FS_Register_LastModifiedTime,
                resizeable: true,
                width: 300
            }];
        
        const pathColumnHeader = enableJPMCFeature ? RMResx.RM_FS_Register_Path : RMResx.RM_FS_Register_UNCPath;
        const connectionTableColumns = [
            {
                id: 1,
                header: RMResx.RM_FS_Register_ConnectionName,
                width: 260,
                resizeable: true,
                visible: true,
                sortable: enableJPMCFeature,
                valuePath: "Name",
            }, {
                id: 2,
                header: RMResx.RM_FS_Register_JPMCId,
                width: 260,
                resizeable: true,
                visible: enableJPMCFeature,
                sortable: enableJPMCFeature,
                valuePath: "JPMCConnectionId",
            }, {
                id: 3,
                header: RMResx.RM_FS_Register_Description,
                width: 260,
                resizeable: true,
                visible: true,
                sortable: enableJPMCFeature,
                valuePath: "Description",
            }, {
                id: 4,
                header: pathColumnHeader,
                width: 360,
                resizeable: true,
                visible: true,
                sortable: enableJPMCFeature,
                valuePath: "UNCPath",
            }, {
                id: 5,
                header: RMResx.RM_FS_Register_Information_Owner,
                width: 260,
                resizeable: true,
                visible: enableJPMCFeature,
                sortable: false,
                valuePath: "InformationOwners",
            }, {
                id: 6,
                header: RMResx.RM_FS_Register_Records_Owner,
                width: 260,
                resizeable: true,
                visible: enableJPMCFeature,
                sortable: false,
                valuePath: "RecordOwners",
            }, {
                id: 7,
                header: RMResx.RM_FS_Register_GroupName,
                width: 260,
                resizeable: true,
                visible: true,
                sortable: enableJPMCFeature,
                valuePath: "GroupName",
            }, {
                id: 8,
                header: RMResx.RM_FS_Register_LastModifiedTime,
                resizeable: true,
                width: 260,
                visible: true,
                sortable: enableJPMCFeature,
                valuePath: "LastModifiedTime",
            }, {
                id: 9,
                header: RMResx.RM_FS_Register_Monitor,
                resizeable: true,
                width: 220,
                visible: enableJPMCFeature,
                sortable: enableJPMCFeature,
                valuePath: "Monitor",
            }, {
                id: 10,
                header: RMResx.RM_FS_Register_LastSyncTime,
                resizeable: true,
                width: 260,
                visible: enableJPMCFeature,
                sortable: enableJPMCFeature,
                valuePath: "LastSyncTime",
            }
        ]
        this.connectionTableColumns = connectionTableColumns.filter(c => c.visible);
        this.setState({ connectionColumn: this.connectionTableColumns });
    }

    groupPanelFooter = () => {
        return (
            <>
                <R.Button
                    slot="footer"
                    id="raConnGroupTestBtn"
                    text={RMResx.RM_FS_Register_ValidationTest}
                    onClick={() => { this.groupConfigRef.current.OnValidateConnectionTest() }}
                />
                <R.Button
                    slot="buttons"
                    id="raConnGroupCancleBtn"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => { this.setState({ showGroupSettingsPanel: { show: false } }) }}
                />
                {isMultiGeoMainDC && (
                    <R.Button
                        slot="buttons"
                        id="raConnGroupSaveBtn"
                        text={RMResx.RM_JS_Common_Save}
                        primary={true}
                        classify="theme"
                        onClick={this.handleUpdateGroup}
                    />
                )}
            </>
        );
    };

    connectionPanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    id="raFsConnEditPanelCancleBtn"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showConnectionSettingsPanel: { show: false } });
                    }}
                />
                {isMultiGeoMainDC && (
                    <R.Button
                        slot="buttons"
                        id="raFsConnEditPanelSaveBtn"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.handleUpdateConnection}
                    />
                )}
            </>
        );
    };

    correlatePanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    id="raFsConnCorrelatePanelCancleBtn"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showCorrelateConnectionPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    id="raFsConnCorrelatePanelSaveBtn"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.handleUpdateCorrelate}
                />
            </>
        );
    };

    addCorrelatePanelBtns = () => {
        return (
            <>
                <R.Button
                    slot="buttons"
                    id="raFsConnAddCorrelatePanelCancleBtn"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => {
                        this.setState({ showAddCorrelateConnectionPanel: { show: false } });
                    }}
                />
                <R.Button
                    slot="buttons"
                    id="raFsConnAddCorrelatePanelSaveBtn"
                    primary
                    classify="theme"
                    text={RMResx.RM_FS_Register_Add}
                    onClick={this.handleAddCorrelate}
                />
            </>
        );
    };

    initAgentOptions() {
        let option = {
            url: `${this.getAllAgentsUrl}`,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            this.agentOptions = [];
            data.forEach(agent => {
                this.agentOptions.push({
                    title: agent.AgentId,
                    text: agent.AgentId,
                    value: agent.AgentId,
                });
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getGroupsFromServer() {
        let loadingtimer = setTimeout(function () {
            $$.loading(true);
        }, 100);
        let option = {
            url: this.getDataUrl,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (!data) {
                data = [];
            }
            this.groups = data;
            data.map(d => {
                this.groupCacheData.addCacheItem(d);
            });

            let currentPageItems = this.groups.slice(0, this.state.connectionPagerSize);
            this.setState({
                groupPagerTotal: this.groups.length,
                groupShownCount: currentPageItems.length,
            });
            this.dispatch(this.groupTableId, currentPageItems, this.groupTableColumns);

            clearTimeout(loadingtimer);
            $$.loading(false);
        }).catch((e) => {
            clearTimeout(loadingtimer);
            $$.loading(false);
        });
    }

    getConnectionsFromServer() {
        let payload = this.state.filterData;
        if(!enableJPMCFeature) {
            payload.Filters = [],
            payload.Order = {
                ColumnName: '',
                IsDesc: false,
            };
        }
        $$.loading(true);
        let option = {
            url: this.getAllConnectionUrl,
            method: "POST",
            data: payload
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (!data.ConnectionList) {
                data.ConnectionList = [];
            }
            this.connections = data.ConnectionList;
            data.ConnectionList.map(d => {
                this.connectionCacheData.addCacheItem(d);
            });
            let allColumns = enableJPMCFeature ? this.state.connectionColumn : this.connectionTableColumns;
            this.dispatch(this.connectionTableID, data.ConnectionList, allColumns);
            this.connectionListLoaded = true;
            RM.setSessionStorage(`${filterConnectionCacheNamePrefix}_FSFilterData`, this.state.filterData.Filters);
            this.setState({
                // connectionPagerIndex: pagerIndex,
                // connectionPagerSize: pagerSize,
                connectionPagerTotal: data.TotalCount,
                connectionShownCount: data.ConnectionList.length,
                connectionActionButtonsDisable: this.connectionCacheData.getSelectedItems() == 0,
                isFiltered: this.state.filterData.Filters.length > 0,
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getNoGroupConnectionsFromServer() {
        let loadingtimer = setTimeout(function () {
            $$.loading(true);
        }, 100);
        let option = {
            url: this.getAllNoGroupConnectionUrl,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (!data) {
                data = [];
            }
            this.noGroupConnection = data;
            clearTimeout(loadingtimer);
            $$.loading(false);
        }).catch((e) => {
            clearTimeout(loadingtimer);
            $$.loading(false);
        });
    }

    handleChangedTab = (index) => {
        if (index == 1 && !this.connectionListLoaded) {
            this.getConnectionsFromServer();
        }
        if(this.cacheManagedColumnsIds && enableJPMCFeature) {
            this.setTableColumnByManagedColumns(this.cacheManagedColumnsIds);
        }
        this.setState({
            selectedTabIndex: index
        });
    }

    resetSelectedStatus() {
        this.groupCacheData.clearCacheItems();
        this.connectionCacheData.clearCacheItems();
        this.setState({
            groupPagerIndex: 0,
            connectionPagerIndex: 0,
            groupActionButtonsDisable: true,
            connectionActionButtonsDisable: true,
        });
    }

    refreshAllData() {
        this.resetSelectedStatus();
        this.getConnectionsFromServer();
        this.getGroupsFromServer();
        // this.getNoGroupConnectionsFromServer();  //This feature is no longer supported, so there is no need to call the API to optimize performance.
    }

    onDeleteGroupSure = () => {
        $$.messagedialog(false, this.args);
        $$.loading(true);
        let option = {
            url: this.deleteGroupUrl,
            method: "POST",
            data: this.groupCacheData.getSelectedItemIds()
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            const result = JSON.parse(res);
            if (result.MessageType == 1) {
                showToast.error(result.ErrorMessage);
                return;
            }
            this.refreshAllData();
        }).catch((e) => {
            $$.loading(false);
        });
    }

    deleteGroupMsg = () => {
        let deleteMsgContent = '';
        deleteMsgContent = RMResx.RM_FS_Register_ConfirmDeleteGroup;
        //TODO xwwang check correlate connections
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_FS_Register_DeleteGroupMessageTitle,
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
                    onClick: this.onDeleteGroupSure
                }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onDeleteConnectionSure = () => {
        $$.messagedialog(false, this.args);
        $$.loading(true);
        let option = {
            url: this.deleteConnectionUrl,
            method: "POST",
            data: this.connectionCacheData.getSelectedItemIds()
        };
        fetchUtility(option).then((res) => {
            this.refreshAllData();
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    deleteConnectionMsgPre = () => {
        $$.loading(true);
        let option = {
            url: this.checkConnectionHasSettings,
            method: "POST",
            data: this.connectionCacheData.getSelectedItemIds()
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            this.deleteConnectionMsg(JSON.parse(result));
        });
    }

    deleteConnectionMsg = (result) => {
        let deleteMsgContent = '';
        if (result) {
            deleteMsgContent = <React.Fragment>
                <div>{RMResx.RM_FS_Register_DeleteUsingConnection}</div>
                <div>{RMResx.RM_PRM_PRE_Msg_ConfirmDeletePhyObj}</div>
            </React.Fragment>;
        } else {
            deleteMsgContent = <div>{RMResx.RM_PRM_PRE_Msg_ConfirmDeletePhyObj}</div>;
        }
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_FS_Register_DeleteConnectionMessageTitle,
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
                    onClick: this.onDeleteConnectionSure
                }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onShowNewGroupPanel = () => {
        this.setState({
            groupPanelTitle: RMResx.RM_FS_Register_CreateConnectionGroup,
            showGroupSettingsPanel: { show: true },
            clickedGroupId: null
        });
    }

    onShowEditGroupPanel = (item) => {
        // let callback = () => {
        // };
        // this.setState({
        //     groupPanelTitle: RMResx.RM_FS_Register_EditConnectionGroup,
        //     showGroupSettingsPanel: { show: true }
        // }, () => {
        //     this.dispatch(this.groupSettingsPanelId, 'onEditInit', callback, item);
        // });
        this.setState({
            groupPanelTitle: RMResx.RM_FS_Register_EditConnectionGroup,
            showGroupSettingsPanel: { show: true },
            clickedGroupId: item.Id
        });
    }

    onShowCorrelateConnectionsPanel = (item) => {
        let callback = () => {
        };
        this.setState({
            showCorrelateConnectionPanel: { show: true },
            groupName: item.Name
        }, () => {
            this.dispatch(this.correlateConnectionPanelId, 'onInit', callback, RM.deepcopy(item));
        });
    }

    onShowNewConnectionPanel = (groupId) => {
        let callback = () => {
        };
        this.setState({
            connectionPanelTitle: RMResx.RM_FS_Register_CreateConnection,
            showConnectionSettingsPanel: { show: true }
        }, () => {
            this.dispatch(this.connectionSettingsPanelId, 'onSaveInit', callback, this.agentOptions, this.groups);
        });
    }

    onShowEditConnectionPanel = (connection) => {
        const clonedConnection = RM.deepcopy(connection);
        let callback = () => {
        };
        this.setState({
            connectionPanelTitle: RMResx.RM_FS_Register_EditConnection,
            showConnectionSettingsPanel: { show: true }
        }, () => {
            this.dispatch(this.connectionSettingsPanelId, 'onSaveInit', callback, this.agentOptions, this.groups, clonedConnection);
        });
    }



    handleUpdateGroup = async () => {
        var succeed = await this.groupConfigRef.current.Save();
        if (succeed) {
            this.setState({ showGroupSettingsPanel: { show: false } });
            this.refreshAllData();
        }
        return succeed;
        // let callback = (item, showMessageFunc) => {
        //     if (this.groups.find(g => g.Name == item.Name && g.Id != item.Id)) {
        //         return false;
        //     }
        //     $$.loading(true);
        //     let option = {
        //         url: this.saveGroupUrl,
        //         method: "POST",
        //         data: item
        //     };
        //     fetchUtility(option).then((res) => {
        //         let returnMessage = JSON.parse(res);
        //         if (returnMessage.MessageType == 1) {
        //             showMessageFunc(returnMessage.ErrorMessage);
        //         } else {
        //             this.setState({ showGroupSettingsPanel: { show: false } });
        //             this.refreshAllData();
        //         }
        //         $$.loading(false);
        //     }).catch((e) => {
        //         $$.loading(false);
        //     });
        //     return true;
        // };
        // this.dispatch(this.groupSettingsPanelId, 'onSave', callback);
    }

    handleUpdateConnection = () => {
        let callback = (item, showMessageFunc) => {
            if (item.Name.trim().length > 255) {
                showMessageFunc(RMResx.RM_JS_Common_Msg_CannotExceed255);
                return;
            }
            $$.loading(true);
            let option = {
                url: this.saveConnectionUrl,
                method: "POST",
                data: item
            };
            fetchUtility(option).then((res) => {
                let returnMessage = JSON.parse(res);
                if (returnMessage.MessageType == 1) {
                    showMessageFunc(returnMessage.ErrorMessage);
                } else {
                    this.setState({ showConnectionSettingsPanel: { show: false } });
                    //TODO xwwang is exist add to group, if exist refresh group list.
                    this.refreshAllData();
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        };
        this.dispatch(this.connectionSettingsPanelId, 'onSave', callback);
        return false;
    }

    handleUpdateCorrelate = () => {
        let callback = (connection, groupId) => {
            $$.loading(true);
            let option = {
                url: this.correlateConnectionUrl,
                method: "POST",
                data: { GroupId: groupId, ConnectionIdList: connection.map(c => c.Id) }
            };
            fetchUtility(option).then((res) => {
                this.setState({ showCorrelateConnectionPanel: { show: false } });
                this.refreshAllData();
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        };
        this.dispatch(this.correlateConnectionPanelId, 'onSave', callback);
        return false;
    }

    handleAddCorrelate = () => {
        let callback = (selected) => {
            this.setState({ showAddCorrelateConnectionPanel: { show: false } });
            this.dispatch(this.correlateConnectionPanelId, 'onPushConnListToCorrPanel', selected);
        };
        this.dispatch(this.addCorrelateConnectionPanelId, 'onAdd', callback);
        return false;
    }

    onGroupCheckChanged = (items) => {
        let selectedGroups = items.slice();
        this.groupCacheData.updateCacheItemsStatus(selectedGroups);
        this.setState({
            groupActionButtonsDisable: this.groupCacheData.getSelectedItems() == 0,
            checkGroupCount: this.groupCacheData.getSelectedItems().length,
        });
    }

    onGroupCellClick = (data, operationOption) => {
        if (operationOption == 1) {
            this.onShowEditGroupPanel(data);
        } else if (operationOption == 2) {
            this.onShowCorrelateConnectionsPanel(data);
        } else if (operationOption == 3) {
            this.onShowConnectionDetailsPanel(data);
        }
    }

    onShowConnectionDetailsPanel = (data) => {
        this.setState({ showConnectionDetailsPanel: { show: true } }, () => {
            this.dispatch("viewConnectionDetailsPanelId", 'onInit', data.Name);
        });
    }

    onConnectionCheckChanged = (items) => {
        let selectedConnections = items.slice();
        this.connectionCacheData.updateCacheItemsStatus(selectedConnections);
        this.setState({
            connectionActionButtonsDisable: this.connectionCacheData.getSelectedItems() == 0,
            checkConnectionCount: this.connectionCacheData.getSelectedItems().length,
        });
    }

    onConnectionCellClick = (data, operationOption) => {
        this.onShowEditConnectionPanel(data);
    }

    onShowConnectionDetails = (data) => { 
        this.props.history.push({
            pathname: RouterUrls.BCM_FSConnection_JobMonitor,
            state: data
        })
    }

    panelAddAction = (currentConnection) => {
        this.setState({
            showAddCorrelateConnectionPanel: { show: true }
        }, () => {
            this.dispatch(this.addCorrelateConnectionPanelId, 'onInit', this.noGroupConnection, currentConnection);
        });
    }

    handleGroupPageChange = (pagerIndex, pagerSize, callback) => {
        let currentPageItems = this.groups.slice(pagerIndex * pagerSize, (pagerIndex + 1) * pagerSize);
        this.setState({
            groupPagerIndex: pagerIndex,
            groupPagerSize: pagerSize,
            groupShownCount: currentPageItems.length,
        });
        this.dispatch(this.groupTableId, currentPageItems, this.groupTableColumns);
        callback(true);
    };

    handleConnectionPageChange = (pagerIndex, pagerSize, callback) => {
        this.setState(prevData => ({
            filterData: {
                ...prevData.filterData,
                PageIndex: pagerIndex + 1,
                PageSize: pagerSize
            }
        }), () => {
            this.getConnectionsFromServer();
            callback(true);
        });
    };

    renderConnGroupSettingsPanel() {
        return <R.Panel
            id="ra-connection-group-edit"
            header={this.state.groupPanelTitle}
            size={600}
            status={this.state.showGroupSettingsPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="panel-description-header">{RMResx.RM_FS_Register_CreateConnectionGroup_SubTitle}</span>
            </div>
            <ConnectionConfiguration groupId={this.state.clickedGroupId} ref={this.groupConfigRef} />
            {this.groupPanelFooter()}
        </R.Panel>;
    }

    renderConnectionSettingsPanel() {
        return <R.Panel
            id="ra-connection-edit"
            header={this.state.connectionPanelTitle}
            size={600}
            status={this.state.showConnectionSettingsPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="panel-description-header">{RMResx.RM_FS_Register_CreateConnection_SubTitle}</span>
            </div>
            <div>
                <ConnectionSettings
                    id={this.connectionSettingsPanelId}
                >
                </ConnectionSettings>
            </div>
            {this.connectionPanelBtns()}
        </R.Panel>;
    }

    renderCorrelateConnectionsPanel() {
        return <R.Panel
            id="ra-correlate-connection"
            header={this.state.correlateConnectionPanelTitle}
            size={600}
            status={this.state.showCorrelateConnectionPanel}
            destroy={true}
        >
            <div className="br" slot="header">
                <span className="panel-description-header" data-tooltip>{this.state.groupName}</span>
            </div>
            <div>
                <CorrelateConnections
                    id={this.correlateConnectionPanelId}
                    addAction={this.panelAddAction}
                >
                </CorrelateConnections>
            </div>
            {this.correlatePanelBtns()}
        </R.Panel>;
    }

    renderAddCorrelateConnectionsPanel() {
        return <R.Panel
            id="ra-add-correlate-connection"
            header={RMResx.RM_FS_Register_Add}
            size={600}
            status={this.state.showAddCorrelateConnectionPanel}
            destroy={true}
            actionType="back"
        >
            <div className="br" slot="header">
                <span className="panel-description-header">{RMResx.RM_FS_Register_EditCorrelateConnections_SubTitle}</span>
            </div>
            <div>
                <AddCorrelateConnections
                    groupId={this.addCorrelateConnectionPanelId}
                >
                </AddCorrelateConnections>
            </div>
            {this.addCorrelatePanelBtns()}
        </R.Panel>;
    }

    onSearchStart = (args) => {
        this.setState(prevData => ({
            filterData: {
                ...prevData.filterData,
                PageIndex: 1,
                SearchKey: args,
            }
        }), () => {
            this.getConnectionsFromServer();
        });
    }

    openFilterPanel = () => {
        this.setConnectionGroupOptions();
    }

    hideFilterPanel = () => {
        this.setState({ showFilterPanel: false });
    }

    onFilter = () => {
        let callback = (filterOptionsInfo) => {
            const clonedFilterData = this.state.filterData;
            clonedFilterData.Filters = [];
            clonedFilterData.PageIndex = 1;
 
            for (let key in filterOptionsInfo) {
                let filterParam = { ColumnName: key, ColumnValues: [] };
                let filterOptions = filterOptionsInfo[key];
                let filterOptionValues;
 
                // Case for Modify time and Last sync time
                if (key == cacheFilterDataType.modifiedTime || key == cacheFilterDataType.lastSyncTime) {
                    filterOptionValues = filterOptions.length ? filterOptions.map(item => item.Value) : [];
                    filterParam.ColumnValues = filterOptions.length ? [...filterOptionValues] : filterOptionValues;
                } else {
                    // For others case
                    filterOptionValues = filterOptions.filter((item) => item.isChecked || item.Checked).map((option) => {
                        const returnValue = {
                            [cacheFilterDataType.groupName]: option.id,
                        }
 
                        return returnValue[key];
                    });
                    for (let value of filterOptionValues) {
                        if (value && value.split(',').length > 0) {
                            filterParam.ColumnValues.push(...value.split(','));
                        } else {
                            filterParam.ColumnValues.push(value);
                        }
                    }
                }
 
                if (filterOptionValues.length > 0) {
                    clonedFilterData.Filters = [...clonedFilterData.Filters, filterParam];
                }
            }
            this.setState({
                filterOptionsInfo: filterOptionsInfo,
                filterData: clonedFilterData,
                showFilterPanel: false,
                isFiltered: clonedFilterData.Filters.length > 0
            }, () => this.getConnectionsFromServer());
        };
        this.dispatch("connectionFilterFormId", callback);
    }

    onSort = (isDesc, columnName) => {
        this.setState(prevData => ({
                filterData: {
                    ...prevData.filterData,
                    PageIndex: 1,
                    Order: {
                        ColumnName: columnName,
                        IsDesc: isDesc
                    }
                }
            }), () =>  this.getConnectionsFromServer());
    }

    getCacheManagedColumns(){
        let managedColumns = RM.deepcopy(defaultManagedColumns);

        if(this.cacheManagedColumnsIds){
            managedColumns = managedColumns.map((item)=>{
                item.isChecked = this.cacheManagedColumnsIds.includes(item.Id);
                return item;
            });
        }
        return managedColumns; 
    }

    managedColumnChanged = (args) => {
        let managedColumnIds = args.newValue.map((item) => { return item.Id; });
        this.setTableColumnByManagedColumns(managedColumnIds);
        RM.setSessionStorage(manageColumnConnectionCacheName, managedColumnIds);
    }

    setTableColumnByManagedColumns(managedColumnIds){
        let allColumn = RM.deepcopy(this.connectionTableColumns);
        allColumn.map((item, index) => { item.visible = managedColumnIds.includes(item.id - 1); });
        this.setState({
            connectionColumn: allColumn
        }, () => {
            this.dispatch(this.connectionTableID, this.connections, allColumn);
        });
    }

    setConnectionGroupOptions() {
        let option = {
            url: this.getAllGroupConnectionUrl,
            method: "GET",
        };
        fetchUtility(option).then((res) => {;
            let connectionGroupOptions = [];
            res.forEach((item) => {
                let optionItem = {};
                optionItem.id = item;
                optionItem.value = item;
                connectionGroupOptions.push(optionItem);
            });
            connectionGroupOptions.sort((prevOption, nextOption) => prevOption.value.localeCompare(nextOption.value));
            this.setState({
                connectionGroupOptions: connectionGroupOptions,
                showFilterPanel: true
            });
        });
    }

    renderHeader() {
        return <div className="ra-main-header">
            <div>   
                <R.Searchbox
                    placeholder={RMResx.RM_JS_FS_Placeholder_SearchBox}
                    disabled={false}
                    onSearch={this.onSearchStart}
                    width={380}
                />
            </div>
            <div className="flex" style={{ columnGap: "8px" }}>
                <R.Button
                    className="filtered-button"
                    icon="fia-filter"
                    primary={this.state.isFiltered}
                    classify={this.state.isFiltered ? "theme" : "default"}
                    text={this.state.isFiltered ? RMResx.RM_MA_Filtered : RMResx.RM_Common_Filter}
                    onClick={this.openFilterPanel}
                />
                <R.Multicombobox
                    checkedField="isChecked"
                    textField="value"
                    valueField="Id"
                    hasFilter={false}
                    required={true}
                    hasSelectAll={true}
                    clearable={true}
                    customTrigger={true}
                    items={this.state.managedColumns}
                    noneText={RMResx.RM_JS_JM_CustomColumns}
                    allText={RMResx.RM_JS_JM_CustomColumns}
                    selectedItemsTemplate={RMResx.RM_JS_JM_CustomColumns}
                    selectedItemTemplate={RMResx.RM_JS_JM_CustomColumns}
                    disabledField='isDynamic'
                    onChange={this.managedColumnChanged}
                    triggerBySource={true}
                >
                    <R.Button icon="fia-manage-column" text={RMResx.RM_JS_JM_CustomColumns} tooltip={RMResx.RM_JS_JM_CustomColumns} />
                </R.Multicombobox>
            </div>
        </div>;
    }

    renderFilterPanel() {
        return <R.Panel
            header={RMResx.RM_Common_Filter}
            size={664}
            onHide={this.hideFilterPanel}
            status={{ show: this.state.showFilterPanel }}
            destroy={true}
        >
            <ConnectionFilterForm
                id="connectionFilterFormId"
                filterOptionsInfo={this.state.filterOptionsInfo}
                connectionGroupOptions={this.state.connectionGroupOptions}
            />
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.hideFilterPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onFilter} />
            </>
        </R.Panel>;
    }

    renderViewConnectionDetailsPanel() {
        return <R.Panel
            header={RMResx.RM_JS_FS_ConnectionDetailsTitle}
            size={664}
            status={this.state.showConnectionDetailsPanel}
            destroy={true}
        >
            <ViewConnectionDetailsPanel id="viewConnectionDetailsPanelId" />
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={() => this.setState({ showConnectionDetailsPanel: false })} />
        </R.Panel>;
    }

    render() {
        const groupPagerTotal = this.state.groupPagerTotal || 0;
        const connectionPagerTotal = this.state.connectionPagerTotal || 0;
        if (groupPagerTotal === 0) {
            this.setState({ checkGroupCount: 0 });
        }
        let selectGroupItemsCount = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.checkGroupCount, groupPagerTotal);
        if (connectionPagerTotal === 0) {
            this.setState({ checkConnectionCount: 0 });
        }
        let selectConnectionItemsCount = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.checkConnectionCount, connectionPagerTotal);

        const tabs = [
            { 
                id: 'connectionGroup',
                tabTitle: RMResx.RM_FS_Register_Tab_ConnectionGroup,
                content: (
                    <React.Fragment>
                        <section style={{ height: "40px" }} className="margin-bottom-15">
                            <div className="ra-main-navbar ra-border-none">
                                <div className='pull-left'>
                                    {this.state.groupActionButtonsDisable && isMultiGeoMainDC &&
                                        <R.Button
                                            id="raFsConnCreateBtn"
                                            text={RMResx.RM_JS_Common_Create}
                                            primary={true}
                                            classify="theme"
                                            onClick={this.onShowNewGroupPanel} />

                                    }

                                    {!this.state.groupActionButtonsDisable && isMultiGeoMainDC &&
                                        <R.Button
                                            id="raFsConnDeleteBtn"
                                            icon="fia-delete"
                                            text={RMResx.RM_JS_Common_Delete}
                                            onClick={this.deleteGroupMsg} />
                                    }
                                </div>
                                <div className="ra-main-selected-counter">{selectGroupItemsCount}</div>
                            </div>
                        </section>
                        <div className="ra-table-main">
                            <GroupTable
                                id={this.groupTableId}
                                columnInfo={this.groupTableColumns}
                                onCheckChanged={this.onGroupCheckChanged}
                                cellClick={this.onGroupCellClick}
                                onSort={this.onSort}
                            />

                            <div className="ra-main-footer">
                                <$g.Pager
                                    itemsCount={this.state.groupPagerTotal}
                                    pagerIndex={this.state.groupPagerIndex}
                                    pagerSize={this.state.groupPagerSize}
                                    showPagerCounter={true}
                                    showPagerSize={true}
                                    pagerSizeOptions={[5, 10, 15]}
                                    onChange={this.handleGroupPageChange} />
                            </div>
                        </div>
                    </React.Fragment>
                )
            },
            {
                id: 'connections',
                tabTitle: RMResx.RM_FS_Register_Tab_Connections,
                content: (
                    <React.Fragment>
                        <section>
                            {enableJPMCFeature && this.renderHeader()}
                            <div className={`ra-main-navbar ${enableJPMCFeature ? "" : "ra-border-none"}`}>
                                <div className='pull-left'>
                                    {this.state.connectionActionButtonsDisable && isMultiGeoMainDC &&
                                        <R.Button
                                            id="raFsConnNewConnection"
                                            text={RMResx.RM_JS_Common_Create}
                                            primary={true}
                                            classify="theme"
                                            onClick={this.onShowNewConnectionPanel} />
                                    }
                                    {!this.state.connectionActionButtonsDisable && isMultiGeoMainDC &&
                                        <R.Button
                                            id="raFsConnDeleteConnection"
                                            icon="fia-delete"
                                            text={RMResx.RM_JS_Common_Delete}
                                            tooltip={RMResx.RM_JS_Common_Delete}
                                            onClick={this.deleteConnectionMsgPre} />
                                    }
                                </div>
                                <div className="ra-main-selected-counter">{selectConnectionItemsCount}</div>
                            </div>
                        </section>
                        <div className="ra-table-main">
                            <ConnectionTable
                                id={this.connectionTableID}
                                columnInfo={enableJPMCFeature ? this.state.connectionColumn : this.connectionTableColumns}
                                onCheckChanged={this.onConnectionCheckChanged}
                                cellClick={this.onConnectionCellClick}
                                showDetails={this.onShowConnectionDetails}
                                onSort={this.onSort}
                            >
                            </ConnectionTable>

                            <div className="ra-main-footer">
                                <$g.Pager
                                    itemsCount={this.state.connectionPagerTotal}
                                    pagerIndex={this.state.filterData.PageIndex - 1}
                                    pagerSize={this.state.filterData.PageSize}
                                    showPagerCounter={true}
                                    showPagerSize={true}
                                    pagerSizeOptions={[5, 10, 15]}
                                    onChange={this.handleConnectionPageChange} />
                            </div>
                        </div>
                    </React.Fragment>
                )
            }
        ]

        return <React.Fragment>
            <div id='ra-connection-management'>
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_FS, SiteMapLinks.BCM_FSConnGroup]} />
                <div className="ra-page-container">
                    <div className="ra-tab-header-wrapper">
                        <R.Tabcontrol
                            active={this.state.selectedTabIndex}
                            onChange={this.handleChangedTab}
                            type="underline"
                        >
                            {tabs.map((tab) => (
                                <R.TabPanel key={tab.id} tab={tab.tabTitle} />
                            ))}
                        </R.Tabcontrol>
                    </div>

                    <div className="ra-tab-content-wrapper">
                        {tabs.map((tab, index) => (
                            <div 
                                key={tab.id} 
                                style={{ display: this.state.selectedTabIndex === index ? 'block' : 'none' }}
                            >
                                {tab.content}
                            </div>
                        ))}
                    </div>
                </div>
                {this.renderConnGroupSettingsPanel()}
                {this.renderConnectionSettingsPanel()}
                {this.renderCorrelateConnectionsPanel()}
                {this.renderAddCorrelateConnectionsPanel()}
                {enableJPMCFeature && this.renderFilterPanel()}
                {enableJPMCFeature && this.renderViewConnectionDetailsPanel()}
            </div>
        </React.Fragment>;
    }
}