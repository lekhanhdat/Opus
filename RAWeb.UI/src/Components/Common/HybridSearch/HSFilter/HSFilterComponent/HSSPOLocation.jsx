import { LocationOperationLogic, ToSearchComponentDispatchType } from '../../Constants';
import { flushSync } from 'react-dom';
import { TreeType } from "../../../../../Constants/Constants";
import SPTree from "../../../Tree/Instances/SPTree/ReportSPTree";
import { NodeLevel } from '../../../../../Constants/DAEnums';
import { LicenseHelper } from '../../../../../Utilities/CommonUtil';
let idCount = 0;
export default class HSSPOLocation extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            selectedFSText: RMResx.RM_JS_Common_None,
            isShowFsTree: false,
            fsTreeValid: true,
            comboboxOffClose: false,
            conditionOptions: this.buildConditionOptions(),
            conditionValue: LocationOperationLogic.Contains
        };
        this.selectedItems = [];
        this.fsTreeData = null;
        this.nextIndex = '';
        this.fsTreeId = "spoTree" + idCount++;
        this.onDocumentMouseDown = this.onDocumentMouseDown.bind(this);
        window.addEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentDestroy() {
        window.removeEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentReceive(type, data) {
        switch (type) {
            case ToSearchComponentDispatchType.InitData:
                const { TreeData, ColumnOperationLogic, Value } = data;
                this.fsTreeData = TreeData || [];
                this.selectedItems = Value || [];
                this.setState({
                    fsTreeData: RM.deepcopy(this.fsTreeData),
                    conditionValue: ColumnOperationLogic || LocationOperationLogic.Contains,
                });
                this.setSelectedFSText();
                break;
            case ToSearchComponentDispatchType.Valid:
                this.showValidMsg();
                break;
        }
    }

    buildConditionOptions() {
        return [
            { name: RMResx.RM_HS_Contains, value: LocationOperationLogic.Contains, checked: true },
            { name: RMResx.RM_HS_Within, value: LocationOperationLogic.Within, checked: false },
        ];
    }

    onConditionChange = (args) => {
        let value = args.newValue.value;
        this.setState({
            selectedFSText: RMResx.RM_JS_Common_None,
            isShowFsTree: false,
            fsTreeValid: true,
            conditionValue: value,
        });
    }

    onSearch = async (args) => {
        let { key: searchValue, start } = args;
        const selectedIds = new Set(this.selectedItems.map(item => item.ListId));

        if (start === 0) {
            this.nextIndex = '';
        } else if (start > 0 && this.nextIndex === '') {
            return [];
        }

        const data = await new Promise((resolve) => {
            if (this.searchTimeout) {
                clearTimeout(this.searchTimeout);
            }

            if (!searchValue) {
                resolve([]);
                return;
            }

            this.searchTimeout = setTimeout(() => {
                let urlData = '/api/SPSettingApi/BrowseSPAndODSuggestion';
                let option = {
                    url: urlData,
                    method: "post",
                    data: {
                        PagingInfo: { PageIndex: this.nextIndex, PageSize: 10 },
                        QueryOption: {
                            OrderColumn: null,
                            Values: [{
                                Value: JSON.stringify(searchValue),
                                ColumnOperationLogic: 0,
                                ColumnsLogic: 0,
                                Column: { Id: "a6c8f7d1-8d5f-4eb4-9dcb-2b0f3f9c5d62" }
                            }]
                        }
                    }
                };

                fetchUtility(option).then((res) => {
                    const parsedResponse = JSON.parse(res);
                    if (parsedResponse && parsedResponse.pagingInfo) { 
                        this.nextIndex = parsedResponse.pagingInfo.PageIndex ?? '';
                    }
                    const newData = parsedResponse.datas?.map(data => ({
                        name: data.FullPath,
                        value: data.Id,
                        tooltip: data.FullPath,
                        data: data,
                        readonly: selectedIds.has(data.ListId)
                    }));
                    resolve(newData);
                }).catch(() => {
                    resolve([]);
                });
            }, 500);
        });
        return data;
    }

    selectionChanged = (args) => {
        let selections = RM.deepcopy(args.newValue);
        this.selectedItems = selections?.map(item => item.data);
        const data = {
            ColumnOperationLogic: this.state.conditionValue,
            Value: this.selectedItems
        }
        this.props.onChange(data, null);
    }

    onApplyClick = () => {
        this.fsTreeData = this.refFsTree.getTreeData().items;
        let selectedRootNode = false;
        let selectedCount = 0;
        let rootNode = null;
        for (let item of this.fsTreeData) {
            if (item.CheckNumber == 1) {
                selectedCount++;
                if (item.Level == NodeLevel.Farm) {
                    selectedRootNode = true;
                    rootNode = item;
                }
            }
        }
        if (selectedRootNode && selectedCount == this.fsTreeData.length && selectedCount > 1) {
            rootNode.CheckNumber = 0;
        }
        let spNodes = this.getSelectedTreeNode();
        this.setSelectedFSText();
        const args = {
            ColumnOperationLogic: this.state.conditionValue,
            Value: spNodes
        }
        this.props.onChange(args, this.fsTreeData);
    }

    getSelectedTreeNode() {
        // let selectedFsTreeNodeId = null;
        // for (let item of this.fsTreeData) {
        //     if (item.CheckNumber == 1) {
        //         selectedFsTreeNodeId = item.Id;
        //         break;
        //     }
        // }
        // return selectedFsTreeNodeId;

        let spNodes = [];
        for (let item of this.fsTreeData) {
            if (item.CheckNumber == 1) {
                spNodes.push({ Id: item.Id, Level: item.Level });
            }
        }
        return spNodes;
    }

    setSelectedFSText() {
        let selectedFSText = RMResx.RM_JS_Common_None;
        let fsTreeValid = false;
        let selectedRootNode = false;
        let selectedCount = 0;
        let itemName = "";
        for (let item of this.fsTreeData) {
            if (item.CheckNumber == 1) {
                selectedCount++;
                itemName = item.Name;
                fsTreeValid = true;
                if (item.Level == NodeLevel.Farm) {
                    selectedRootNode = true;
                }
            }
        }
        if (selectedCount > 0) {
            if (selectedCount == 1) {
                selectedFSText = itemName;
                if (selectedRootNode) {
                    selectedFSText = RMResx.RM_JS_BCM_Explorer_Filter_All;
                }
            } else {
                selectedFSText = RMResx.RM_Common_Combobox_SelectedXItems.format(selectedCount);
            }
        }
        this.setState({
            selectedFSText: selectedFSText,
            fsTreeValid: fsTreeValid
        });
    }

    onDocumentMouseDown(e) {
        this.mouseDownTarget = e.target;
    }

    isTreeRefreshClick(target) {
        let $target = $(target);
        return $target.closest(".ra-tree-menu-expand").length > 0
    }

    onWillHideFSFilterPopup = () => {
        let isTreeRefreshClick = this.isTreeRefreshClick(this.mouseDownTarget);
        this.mouseDownTarget = null;
        if (isTreeRefreshClick) {
            return false;
        }
    }

    onShowFSFilterPopup = () => {
        this.setState({
            isShowFsTree: true,
            fsTreeData: RM.deepcopy(this.fsTreeData)
        });
    }

    onHideFSFilterPopup = () => {
        this.setState({
            isShowFsTree: false
        });
    }

    showValidMsg() {
        this.setState({ fsTreeValid: false });
    }

    render() {
        return <div className="flex">
            <div className="flex-1">
                {LicenseHelper.EnableRecordsArchiver() ? (
                    <R.Combobox
                        searchable={false}
                        height={40}
                        width={"100%"}
                        textField='name'
                        valueField='value'
                        checkedField='checked'
                        items={this.state.conditionOptions}
                        onChange={this.onConditionChange}
                    />
                ) : (
                    <R.Input
                        type="text"
                        value={RMResx.RM_HS_Contains}
                        width={"100%"}
                        height={40}
                        readonly={true}
                    />
                )}
            </div>
            <div className="flex-1 margin-left-m width-0">
                {this.state.conditionValue == LocationOperationLogic.Contains ? (
                    <>
                        <R.ComboboxShell
                            dynamicSize
                            content={this.state.selectedFSText}
                            height={40}
                            popupHeight={[, 300]}
                            popupWidth={[, '100%']}
                            width={"100%"}
                            id={this.fsTreeId}
                            block={false}
                            triggerType="all"
                            status={{ show: this.state.isShowFsTree }}
                            willHide={this.onWillHideFSFilterPopup}
                            onHide={this.onHideFSFilterPopup}
                            onShow={this.onShowFSFilterPopup}
                        >
                            <div id="hsFilterFileSystem" className="padding-m">
                                {/* {
                                    this.state.isShowFsTree && <SPTree
                                        ref={r => this.refFsTree = r}
                                        treeData={this.state.fsTreeData}
                                        type={TreeType.Filter}
                                    />
                                }
                                */}
                                <SPTree
                                    ref={r => this.refFsTree = r}
                                    // searchKey={this.state.spTreeSearchKey}
                                    data={this.state.fsTreeData}
                                    treeType={TreeType.Filter}
                                />
                            </div>
                            <>
                                <R.Button
                                    slot="buttons"
                                    name="cancel"
                                    text={RMResx.RM_JS_Common_Cancel}
                                    value="close"
                                />
                                <R.Button
                                    slot="buttons"
                                    name="save"
                                    primary={true}
                                    classify="theme"
                                    text={RMResx.RM_JS_Common_Save}
                                    value="close"
                                    onClick={this.onApplyClick}
                                />
                            </>
                        </R.ComboboxShell>
                        <R.ValidationFaker valid={this.state.fsTreeValid} of={`#${this.fsTreeId}`} message={RMResx.RM_HS_NoSearchColValValidMsg} />
                    </>
                ) : (
                    <R.Validation element="RichCombobox" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                        <R.RichCombobox
                            asyncSearch
                            id={"hsFilterLocationRichCombobox"}
                            searchMinChars={3}
                            value={this.selectedItems.map(item => ({ name: item.FullPath, value: item.Id, tooltip: item.FullPath, data: item }))}
                            width="100%"
                            height={40}
                            popupMaxHeight={250}
                            popupWidth={450}
                            lazyStep={true}
                            searchPlaceholder={RMResx.RM_Common_PeoplePicker_Watermark}
                            tooltipField="tooltip"
                            textField="name"
                            valueField="value"
                            doLoad={this.onSearch}
                            onChange={this.selectionChanged}
                        />
                    </R.Validation>
                )}
            </div>
        </div>;
    }
}
