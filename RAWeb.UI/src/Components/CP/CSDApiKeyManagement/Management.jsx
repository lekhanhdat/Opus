import SiteMapLinks from "../../../Constants/SiteMapLinks";
import {TableRow} from "./RowTemplate";
import ApiKeyForm from "./ApiKeyForm";
import "../../../Less/CP/csdApiKeyManagement.less";

export const apiKeyActions = {
    Create: 'create',
    Edit: 'edit',
    Delete: 'delete',
    Copy: 'copy',
    Check: 'check',
};

export default class ApiKeyManagement extends R.Component {
    componentCreate () {
        this.state = {
            tableItems: [],
            selectedItems: [],
            editingItem: null,
            shownCount: 0,
            totalCount: 0,
            pagerIndex: 0,
            pagerSize: 10,
            isSelectAll: false,
            hasNewlyKey: false,
            formPanelShow: false,

            showTip: false,
            tipType: "success",
            tipMsg: ""
        };
        this.state.columns = this.getGridColumns(false);
    }


    componentInit () {
        this.setTableData(0, this.state.pagerSize);
    }

    getGridColumns(isSelectAll) {
        return [
            {
                headerTemplate: (<R.Checkbox
                    checked={isSelectAll}
                    onChange={this.onSelectAllChanged.bind(this)}
                />),
                resizeable: false,
                width: 50
            },
            {
                header: RMResx.RM_JS_CP_CSDAK_KeyName,
                width: [200],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_CP_CSDAK_KeyValue,
                width: [200],
                resizeable: true
            },
            {
                header: RMResx.RM_JS_CP_CSDAK_KeyOperator,
                width: [200],
                resizeable: true
            },
            {
                header: RMResx.RM_JS_CP_CSDAK_KeyExpired,
                resizeable: true,
                width: [200]
            },
            {
                header: RMResx.RM_JS_CP_CSDAK_KeyCreated,
                resizeable: true,
                width: [200]
            },
            {
                header: RMResx.RM_JS_CP_CSDAK_KeyModified,
                resizeable: true,
                width: [200]
            }
        ];
    }

    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }

    showMessageBar = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    handleHideMessageBar = () => {
        this.setState({ showTip: false });
    }

    setTableData = (pagerIndex, pagerSize, callback) => {
        $$.loading(true);
        let option = {
            url: "/api/CPApi/GetCSDKeys",
            method: "POST",
            data: {
                PageIndex: pagerIndex + 1,
                PageSize: pagerSize
            }
        };
        fetchUtility(option)
            .then((res) => {
                if(res) {
                    let hasNewlyKey = false;
                    let listItems = res.Data.map((item) => {
                        if (item.Value.indexOf("*") < 0) {
                            item.showValue = true;
                            hasNewlyKey = true;
                        }
                        item.displayExpired = RM.TimeUtil.dateToString(item.Expired, null, true);
                        item.displayCreated = RM.TimeUtil.dateToString(item.Created, null, true);
                        item.displayModified = RM.TimeUtil.dateToString(item.Modified, null, true);
                        return item;
                    });

                    this.refreshSelectedItems(listItems);
                    this.setState({
                        tableItems: listItems,
                        pagerIndex: res.PageIndex - 1,
                        pagerSize: res.PageSize,
                        totalCount: res.TotalCount,
                    });

                    if (hasNewlyKey) {
                        this.showMessageBar("warn", RMResx.RM_JS_CP_CSDAK_AlertCopyKeyValue);
                    }

                    callback && callback(true);
                }
                
                $$.loading(false);
            })
            .catch((e) => {
                $$.loading(false);
            });
    }

    onRowEvent = (args) => {
        let rowData = args.rowData;
        switch (args.type) {
            case apiKeyActions.Copy:
                RM.CopyToClipboard(rowData.Value);
                break;
            case apiKeyActions.Delete:
                this.showDelKeyMsgBox([rowData]);
                break;
            case apiKeyActions.Edit:
                this.showApiKeyForm(rowData);
                break;
            case apiKeyActions.Check:
                this.onSelectedItemsChange([rowData]);
                break;
        }
    };

    onSelectAllChanged(checked) {
        for (const item of this.state.tableItems) {
            item.isChecked = checked;
        }
        this.onSelectedItemsChange(this.state.tableItems);
        this.setState({tableItems: RM.deepcopy(this.state.tableItems)});
    }

    onSelectedItemsChange = (changedItems, viewItems) => {
        let changedItemsMap = {};
        for (const item of changedItems) {
            changedItemsMap[item.Id] = item;
        }

        let selItems = [];
        for (const item of this.state.selectedItems) {
            if(!changedItemsMap[item.Id]) {
                selItems.push(item);
            } 
        }
        
        for (const item of changedItems) {
            if(item.isChecked) {
                selItems.push(item);
            }
        }

        let updateInfo = {
            selectedItems: selItems,
            isSelectAll: this.isAllViewItemSelected(viewItems, selItems), 
        };
        if(updateInfo.isSelectAll != this.state.isSelectAll) {
            updateInfo.columns = this.getGridColumns(updateInfo.isSelectAll);
        }
        this.setState(updateInfo);
    }

    isAllViewItemSelected(viewItems, selItems) {
        viewItems = viewItems || this.state.tableItems;
        if(viewItems.length == 0) {
            return false;
        }

        let selItemsMap = {};
        for (const item of selItems) {
            selItemsMap[item.Id] = item;
        }

        let selectAll = true;
        for (const item of viewItems) {
            if(!selItemsMap[item.Id]) {
                selectAll = false;
                break;
            }
        }

        return selectAll;
    }

    clearSelectedItems = () => {
        for (const item of this.state.selectedItems) {
            item.isChecked = false;
        }
        this.setState({selectedItems: []});
    }

    refreshSelectedItems = (viewItems) => {
        let viewItemsMap = {};
        for (const item of viewItems) {
            viewItemsMap[item.Id] = item;
        }
        let selViewItems = [];
        for (const item of this.state.selectedItems) {
            let viewItem = viewItemsMap[item.Id];
            if(viewItem) {
                viewItem.isChecked = true;
                selViewItems.push(viewItem);
            }
        }
        this.onSelectedItemsChange(selViewItems, viewItems);
    }

    deleteApiKey = (deletingItems) => {
        $$.loading(true);
        let option = {
            url: "/api/CPApi/RemoveCSDKey",
            method: "POST",
            data: deletingItems
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res == "true") {
                this.clearSelectedItems();
                this.setTableData(0, this.state.pagerSize);
                this.showMsgToast(RMResx.RM_CP_CSDAK_Msg_Success_DelKey, "success");
            }
            $$.messagedialog(false);
        }).catch((e) => {
            $$.loading(false);
        });
    };

    showAddApiKeyForm = () => {
        this.showApiKeyForm();
    }

    showApiKeyForm = (editItem) => {
        this.setState({
            formPanelShow: true,
            editingItem: editItem || {}
        });
    };

    onFormPanelHide = (success, isNewKeyForm) => {
        this.setState({formPanelShow: false});
        if(success) {
            this.clearSelectedItems();
            this.setTableData(isNewKeyForm ? 0 : this.state.pagerIndex, this.state.pagerSize);
            this.showMsgToast(isNewKeyForm ? RMResx.RM_CP_CSDAK_Msg_Success_AddKey :  RMResx.RM_CP_CSDAK_Msg_Success_EditKey, "success");
        }
    }

    showDelKeyMsgBox = (deletingItems) => {
        let cloneDeletingItems = deletingItems;
        deletingItems = this.state.selectedItems.map(function (item) {
            return {
                Id: item.Id,
                Name: item.Name
            };
        });
        $$.messagedialog(true, {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_CP_CSDAK_Title_RemoveKey,
            content: RMResx.RM_CP_CSDAK_RemoveKeyTips,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: ()=>{
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => this.deleteApiKey(deletingItems)
                }
            ]
        });
    };

    onDelBtnClick = () => {
        this.showDelKeyMsgBox(this.state.selectedItems);
    }

    renderSiteMap () {
        return <div>
            <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_CSDApiKeyManagement]} />
        </div>;
    }

    renderMessageBar () {
        return <R.Messagebar
            message={this.state.tipMsg}
            classify={this.state.tipType}
            status={{ show: this.state.showTip }}
            onClose={this.handleHideMessageBar}
        />;
    }

    renderNavBar() {
        return (
            <div className="ra-main-navbar">
                <div className="ra-nav-bar-left">
                    <R.Button
                        id="csdBtnAdd"
                        className = "ra-nav-bar-btn"
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_CP_CSDAK_AddKey}
                        onClick={this.showAddApiKeyForm}
                    />
                    <R.Button
                        id="csdBtnDelete"
                        className = "ra-nav-bar-btn"
                        type="button"
                        icon="fia-delete"
                        disabled={this.state.selectedItems.length == 0}
                        text={RMResx.RM_JS_Common_Delete}
                        tooltip={RMResx.RM_JS_Common_Delete}
                        onClick={this.onDelBtnClick}
                    />
                </div>
                <div className="ra-nav-bar-right">
                    <div className={"ra-selection-describe"}>
                        {RMResx.RM_Common_SelectTableItemsCounter.format(this.state.selectedItems.length, this.state.totalCount)}
                    </div>
                </div>
            </div>
        );
    }

    renderTable() {
        return (
            <div className="ra-main-table">
                <div>
                    <R.Table
                        id="csdApiKeyManagementTable"
                        columns={this.state.columns}
                        rowTemplate={TableRow}
                        items={this.state.tableItems}
                        onRowEvent={this.onRowEvent}
                    />
                </div>
            </div>
        );
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
                    onChange={this.setTableData}
                />
            </div>
        );
    }

    renderApiKeyFormPanel() {
        return this.state.formPanelShow && <ApiKeyForm
            item={this.state.editingItem}
            showPanel={this.state.formPanelShow}
            onHidePanel={this.onFormPanelHide}
        ></ApiKeyForm>;
    }

    render() {
        return (
            <div id="csdApiKeyManagement">
                {this.renderSiteMap()}
                {this.renderMessageBar()}
                <div className="ra-page-container">
                    {this.renderNavBar()}
                    {this.renderTable()}
                    {this.renderPager()}
                    {this.renderApiKeyFormPanel()}
                </div>
            </div>
        );
    }
}


