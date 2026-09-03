import SiteMapLinks from "../../../../../Constants/SiteMapLinks";
import GroupTable from "../../../../BCM/FSConnGroup/Components/Table/GroupTable";
import ConnectionTable from "../../../../BCM/FSConnGroup/Components/Table/ConnectionTable";
import ConnectionSettings from "../../../../BCM/FSConnGroup/Components/ConnectionSettings";
import CorrelateConnections from "../../../../BCM/FSConnGroup/Components/CorrelateConnections";
import AddCorrelateConnections from "../../../../BCM/FSConnGroup/Components/AddCorrelateConnections";
import TableDataCache from "../../../../BCM/FSConnGroup/Components/TableDataCache";
import { ConnectionConfiguration } from "../../../../BCM/FSConnGroup/Components/ConnectionConfiguration";
import DiscoveryAndAnalysisNavigation from "../../../Navigation";
import { Office365AnalysisConfigurationEditPage } from "../Office365";
import { SalesforceAnalysisConfigurationEditPage } from "../Salesforce";
import { GoogleDriveAnalysisConfigurationEditPage } from "../GoogleDrive";
import RouterUrls from "../../../../../Constants/RouterUrls";
import { DiscoveryDataSource } from "../Constants";

import "../../../../../Less/CP/ConnectionGroupSettings.less";
import { LicenseHelper, isEnableMultiGeoFeature } from "../../../../../Utilities/CommonUtil";

const enableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();
const enableMultiGeoFeature = isEnableMultiGeoFeature();
export default class AnalysisConfigurationConnectionPage extends R.Component {
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
        this.groupTableId = 'ra-conn-group-table';
        this.connectionTableID = 'ra-conn-table';
        this.groupSettingsPanelId = 'ra-conn-group-settings-panel';
        this.connectionSettingsPanelId = 'ra-connection-settings-panel';
        this.correlateConnectionPanelId = 'ra-correlate-connections-panel';
        this.addCorrelateConnectionPanelId = 'ra-add-correlate-connections-panel';
        this.connectionListLoaded = false;
        this.groupCacheData = new TableDataCache();
        this.connectionCacheData = new TableDataCache();
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
            selectedDataSource: DiscoveryDataSource.None,
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
                header: RMResx.RM_FS_Register_ConnectionName,
                width: 260,
                resizeable: true,
                isVisible: true
            }, {
                header: RMResx.RM_FS_Register_JPMCId,
                width: 260,
                resizeable: true,
                isVisible: enableJPMCFeature
            }, {
                header: RMResx.RM_FS_Register_Description,
                width: 260,
                resizeable: true,
                isVisible: true
            }, {
                header: pathColumnHeader,
                width: 360,
                resizeable: true,
                isVisible: true
            }, {
                header: RMResx.RM_FS_Register_Information_Owner,
                width: 260,
                resizeable: true,
                isVisible: enableJPMCFeature
            }, {
                header: RMResx.RM_FS_Register_Records_Owner,
                width: 260,
                resizeable: true,
                isVisible: enableJPMCFeature
            }, {
                header: RMResx.RM_FS_Register_GroupName,
                width: 260,
                resizeable: true,
                isVisible: true
            }, {
                header: RMResx.RM_FS_Register_LastModifiedTime,
                resizeable: true,
                width: 260,
                isVisible: true
            },                 {
                header: RMResx.RM_FS_Register_Monitor,
                resizeable: true,
                width: 220,
                isVisible: enableJPMCFeature
            }, {
                header: RMResx.RM_FS_Register_LastSyncTime,
                resizeable: true,
                width: 260,
                isVisible: enableJPMCFeature
            }
        ]
        this.connectionTableColumns = connectionTableColumns.filter(column => column.isVisible);
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
                <R.Button
                    slot="buttons"
                    id="raConnGroupSaveBtn"
                    text={RMResx.RM_JS_Common_Save}
                    primary={true}
                    classify="theme"
                    onClick={this.handleUpdateGroup}
                />
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
                <R.Button
                    slot="buttons"
                    id="raFsConnEditPanelSaveBtn"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.handleUpdateConnection}
                />
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

    getConnectionsFromServer(pagerIndex, pagerSize) {
        if (!pagerIndex) {
            pagerIndex = 0;
        }
        if (!pagerSize) {
            pagerSize = 10;
        }
        $$.loading(true);
        let requestParam = {
            PageIndex: pagerIndex + 1,
            PageSize: pagerSize,
        };
        let option = {
            url: this.getAllConnectionUrl,
            method: "POST",
            data: requestParam
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
            this.dispatch(this.connectionTableID, data.ConnectionList, this.connectionTableColumns);
            this.connectionListLoaded = true;

            this.setState({
                connectionPagerIndex: pagerIndex,
                connectionPagerSize: pagerSize,
                connectionPagerTotal: data.TotalCount,
                connectionShownCount: data.ConnectionList.length,
                connectionActionButtonsDisable: this.connectionCacheData.getSelectedItems() == 0,
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
        // this.getNoGroupConnectionsFromServer(); //This feature is no longer supported, so there is no need to call the API to optimize performance.
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
            this.refreshAllData();
            $$.loading(false);
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
        let callback = () => {
        };
        this.setState({
            connectionPanelTitle: RMResx.RM_FS_Register_EditConnection,
            showConnectionSettingsPanel: { show: true }
        }, () => {
            this.dispatch(this.connectionSettingsPanelId, 'onSaveInit', callback, this.agentOptions, this.groups, connection);
        });
    }



    handleUpdateGroup = async () => {
        var succeed = await this.groupConfigRef.current.Save();
        if (succeed) {
            this.setState({ showGroupSettingsPanel: { show: false } });
            this.refreshAllData();
        }
        return succeed;
    }

    handleUpdateConnection = () => {
        let callback = (item, showMessageFunc) => {
            if (this.connections.find(c => c.Name == item.Name && c.Id != item.Id)) {
                showMessageFunc(RMResx.RM_FS_Register_SameConnectionNameErrorMessage);
                return;
            }
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
        }
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
        this.getConnectionsFromServer(pagerIndex, pagerSize);
        callback(true);
    };

    onDataSourceChange = (dataSource) => {
        this.setState({ selectedDataSource: dataSource });
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
        const { history } = this.props;
        const { selectedDataSource } = this.state;

        return <React.Fragment>
            <DiscoveryAndAnalysisNavigation
              history={history}
              redirect={{ need: true, url: RouterUrls.FA_Discovery }}
              onChange={this.onDataSourceChange}
              dataSources={[
                  DiscoveryDataSource.Office365,
                  DiscoveryDataSource.Salesforce,
                  DiscoveryDataSource.Google,
                  DiscoveryDataSource.FileSystem,
              ]}
            />
            {selectedDataSource === DiscoveryDataSource.Office365 && (
                <Office365AnalysisConfigurationEditPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.Salesforce && (
                <SalesforceAnalysisConfigurationEditPage history={history} />
            )}
            {selectedDataSource === DiscoveryDataSource.Google && (
                <GoogleDriveAnalysisConfigurationEditPage history={history} />
            )}
            <div id='ra-connection-management'>
                <$g.SiteMap data={[SiteMapLinks.FA_Discovery_FS, SiteMapLinks.FA_Discovery_FS_ConfigConnection]} />
                <div className="ra-page-container">
                    <R.Tabcontrol
                        active={this.state.selectedTabIndex}
                        onChange={this.handleChangedTab}
                        type="underline"
                    >
                        <R.TabPanel key={0} tab={RMResx.RM_FS_Register_Tab_ConnectionGroup}>
                            <section style={{ height: "40px" }} className="margin-bottom-15">
                                <div className="ra-main-navbar ra-border-none">
                                    <div className='pull-left'>
                                        {this.state.groupActionButtonsDisable &&
                                            <R.Button
                                                id="raFsConnCreateBtn"
                                                text={RMResx.RM_JS_Common_Create}
                                                primary={true}
                                                classify="theme"
                                                onClick={this.onShowNewGroupPanel} />

                                        }

                                        {!this.state.groupActionButtonsDisable &&
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
                            <div className="ra-main-table">
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
                        </R.TabPanel>

                        <R.TabPanel key={1} tab={RMResx.RM_FS_Register_Tab_Connections}>
                            <section style={{ height: "40px" }} className="margin-bottom-15">
                                <div className="ra-main-navbar ra-border-none">
                                    <div className='pull-left'>
                                        {this.state.connectionActionButtonsDisable &&
                                            <R.Button
                                                id="raFsConnNewConnection"
                                                text={RMResx.RM_JS_Common_Create}
                                                primary={true}
                                                classify="theme"
                                                onClick={this.onShowNewConnectionPanel} />
                                        }
                                        {!this.state.connectionActionButtonsDisable &&
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
                            <div className="ra-main-table">
                                <ConnectionTable
                                    id={this.connectionTableID}
                                    columnInfo={this.connectionTableColumns}
                                    onCheckChanged={this.onConnectionCheckChanged}
                                    cellClick={this.onConnectionCellClick}
                                    onSort={this.onSort}
                                >
                                </ConnectionTable>

                                <div className="ra-main-footer">
                                    <$g.Pager
                                        itemsCount={this.state.connectionPagerTotal}
                                        pagerIndex={this.state.connectionPagerIndex}
                                        pagerSize={this.state.connectionPagerSize}
                                        showPagerCounter={true}
                                        showPagerSize={true}
                                        pagerSizeOptions={[5, 10, 15]}
                                        onChange={this.handleConnectionPageChange} />
                                </div>
                            </div>
                        </R.TabPanel>
                    </R.Tabcontrol>
                </div>
                {this.renderConnGroupSettingsPanel()}
                {this.renderConnectionSettingsPanel()}
                {this.renderCorrelateConnectionsPanel()}
                {this.renderAddCorrelateConnectionsPanel()}
            </div>
        </React.Fragment>;
    }
}