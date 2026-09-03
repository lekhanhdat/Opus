import { Component } from "react";
import { Prompt } from 'react-router';
import { bindEvents, showToast, getRequestVerificationToken } from "../../Utilities/CommonUtil";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import TreeNodeContent from "../Common/Tree/NodeContents/TermManagementNodeContent";
import StringUtil from '../../Utilities/StringUtil';
import "../../Less/PRM/LocationManagement.less";

const NodeType = {
    Root: 'RootLocation',
    Normal: 'NormalLocation',
    Min: 'MininumLocation'
};

const NodeTypeEnum = {
    Root: 9000,
    Normal: 9100,
    Min: 9200
};

const RAMessageType = {
    Successful: 0,
    Failed: 1,
    Exception: 2
};
const DefaultSuiteUniqueIds = ["6feecea2-2076-4557-ae9c-a90f9eb91617", "c7a9a849-c9a3-4c0b-ba38-ba0db43af048"];
export default class LocationManagement extends Component {
    constructor(props) {
        super(props);
        this.initBindings();
        this.getTreeData();
        this.treeContext = this.getTreeContext();
        this.state = {
            showTip: false,
            tipType: "success",
            tipMsg: "",
            treeData: [],
            selectedItem: null, //selected item
            currentItem: null,  //current selected item, clone from "selectedItem"
            itemSettingChanged: false, //if current selected item's setting changed
            allSuiteItems: [],
            showImportPanel: { show: false },
            files: [],
        };
        this.uploaderRef = React.createRef();
    }

    componentDidMount() {
        this.loadAllSuiteItems();
    }

    loadAllSuiteItems() {
        let option = {
            url: "/api/TemplateManagementApi/GetAllSimplifySuites",
            method: "post",
        };
        fetchUtility(option).then((result) => {
            if(result && result.length > 0)
            {
                this.setState({
                    allSuiteItems: result
                });
            }
        }).catch((e) => {

        });
    }

    copyProps(fromObj, toObj, propNames) {
        if (fromObj && toObj && propNames) {
            for (var i = 0; i < propNames.length; i++) {
                toObj[propNames[i]] = fromObj[propNames[i]];
            }
        }
    }

    getTreeContext() {
        return {
            treeType: 2,    //1:TermManagement, 2:LocationManagement
            searchKey: "",
            nodeContentComponent: TreeNodeContent,
            singleSelection: true,
            transToTreeNodeObject(oitem) {
                let itemsCount = !this.pagerByServer ? (!oitem.SubLocations ? 0 : oitem.SubLocations.length) : oitem.SubLocationCount;
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.NodeType == NodeTypeEnum.Root ? NodeType.Root : oitem.NodeType == NodeTypeEnum.Normal ? NodeType.Normal : NodeType.Min,
                    text: oitem.Name,
                    disableSelect: this.isDisableSelect(oitem),
                    expanded: (!!this.searchKey && oitem.hasMatchChildren) || oitem.NodeType == NodeTypeEnum.Root,
                    loaded: !!this.searchKey || oitem.SubLocationCount == 0 || !!oitem.SubLocations,
                    enableContextMenu: true,
                    isAllowEditName: true,
                    items: oitem.SubLocations,
                    itemsCount: itemsCount,
                    hasChildren: itemsCount > 0,
                    pagerByServer: true,
                    pagerSize: 15,
                    pagerIndex: 0
                };
            },
            isDisableSelect(oitem){
                return oitem.NodeType == NodeTypeEnum.Root;
            },
            sortChild(a, b) {
                if (a.IsDefaultTerm) {
                    return -1;
                } else if (b.IsDefaultTerm) {
                    return 1;
                }

                if (a.Name == b.Name) {
                    return 0;
                } else if (a.Name > b.Name) {
                    return 1;
                } else {
                    return -1;
                }
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let oItem = parentItem.origin;
                $.ajax({
                    type: "GET",
                    url: "/api/LocationManagementApi/GetChildrenByDB",
                    contentType: "application/json;charset=utf-8",
                    data: "PageIndex=" + (parentItem.pagerIndex + 1) + "&PageSize=" + parentItem.pagerSize
                        + "&NodeId=" + oItem.Id + "&NodeType=" + oItem.Type,
                    async: true,
                    //beforeSend: function () {
                    //    $$.loading(true);
                    //},
                    //complete: function () {
                    //    $$.loading(false);
                    //},
                    success: function (data) {
                        let items = $.parseJSON(data);  // Fortify Issue Type: JSON Injection; Sink Details: tree data; Ignore Reason: 前后台对象存在对应关系
                        funcSuccess(items);
                    },
                    error: function (msg) {
                        funcFail(msg.responseText);
                    },
                    dataType: "json"
                });
                //return children node items
                return [];
            },
            confirmOnNodeSelected: (item, funcAllow) => this.onNodeSelected(item.origin, funcAllow),
            refreshSelectedNodeInfo: this.refreshSelectedNodeInfo.bind(this),
            showMessageTip: this.showMessageTip,
            hideMessageTip: this.hideMessageTip
        };
    }

    getTreeData() {
        let getListData = "PageIndex=1&PageSize=15&NodeId=Root&NodeType=Root";
        $.ajax({
            type: "GET",
            url: "/api/LocationManagementApi/GetChildrenByDB",
            contentType: "application/json;charset=utf-8",
            data: getListData,
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                this.treeContext.searchKey = "";
                this.treeContext.pagerByServer = true;
                this.resetTreeData(data);
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    handleTermSync() {
        $$.messagedialog(true, {
            // classify: "info",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_TM_ConfirmSynchroniseMsg,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.hideMessagebox },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.synchronise },
            ]
        });

    }

    hideMessagebox() {
        $$.messagedialog(false);
    }

    hideMessageTip() {
        this.setState({ showTip: false });
    }

    initBindings() {
        bindEvents(this, "onSearch", "handleTermSync", "onSpaceChange", "onDescriptionChange", "onMinimumLocationSettingChange", "synchronise",
            "showMessageTip", "hideMessageTip", "onSaveSettingClick", "onCancelChangedClick", "handleSuiteCheckChanged","handleLocationsImport",
            "handleImportCancelClick","handleImportSaveClick");
    }

    onCancelChangedClick(e) {
        let args = {
            // classify: "info",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_TM_CancelClickMsg,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.hideMessagebox },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        this.setNewSelectedItem(this.state.selectedItem);
                        this.hideMessagebox();
                    }
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    onNodeSelected(item, funcAllow) {
        if (this.state.itemSettingChanged) {
            this.showIfLeaveWithoutSaveMsg((allow) => {
                this.hideMessagebox();
                if (funcAllow) {
                    funcAllow(allow);
                }
                if (allow) {
                    this.setNewSelectedItem(item);
                }
            });
        } else {
            if (funcAllow) {
                funcAllow(true);
            }
            this.setNewSelectedItem(item);
        }
    }

    onSaveSettingClick(e) {
        let selItem = this.state.currentItem,
            availableSpace = $.trim(selItem.AvailableSpace),
            description = selItem.Description,
            nodeType = selItem.NodeType,
            suiteIds = selItem.RMLocationSuiteAssociationIds;
        if (selItem.Id == "" || selItem.Id == null) {
            return;
        }
        if (availableSpace < 0 || isNaN(availableSpace)) {
            showToast.error(RMResx.RM_LM_SpaceValueInvalid);
            return;
        }

        if (nodeType == NodeTypeEnum.Min && suiteIds.length == 0) {
            showToast.warn(RMResx.RM_LM_SelectAtLeastOneTip);
            return;
        }

        let termObj = {
            LocationId: selItem.Id,
            Name: selItem.Name,
            Description: description,
            NodeType: nodeType,
            AssociationSuites: suiteIds
        };

        if(availableSpace)
        {
            termObj.AvailableSpace = availableSpace;
        }

        // return;
        $.ajax({
            type: "POST",
            url: "/api/LocationManagementApi/SaveLocationSetting",
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify(termObj),
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                if (data) {
                    switch (data.MessageType) {
                        case RAMessageType.Successful:
                            this.UpdateLocationItemProps();
                            this.setState({ itemSettingChanged: false });
                            showToast.success(RMResx.RM_JS_TM_SaveSucessMsg);
                            break;
                        case RAMessageType.Failed:
                        case RAMessageType.Exception:
                            showToast.error(data.ErrorMessage);
                            break;
                    }
                } else {
                    showToast.error(RMResx.RM_JS_TM_SaveFailedMsg);
                }
            },
            error: (msg) => {
                showToast.error(RMResx.RM_JS_TM_SaveFailedMsg);
            },
            dataType: "json"
        });
    }

    UpdateLocationItemProps() {
        let selItem = this.state.currentItem,
            propNames = ["AvailableSpace", "NodeType", "Description", "RMLocationSuiteAssociationIds"],
            newItem = {
                AvailableSpace: $.trim(selItem.AvailableSpace),
                NodeType: selItem.NodeType,
                Description: selItem.Description,
                RMLocationSuiteAssociationIds: selItem.RMLocationSuiteAssociationIds
            };
        this.copyProps(
            newItem,
            this.state.selectedItem,
            propNames);
        this.copyProps(
            newItem,
            this.state.currentItem,
            propNames);
    }

    onSearch(args) {
        this.searchData(args);
    }

    onSpaceChange(value) {
        let curItem = this.state.currentItem;
        curItem.AvailableSpace = value;
        this.setState({
            itemSettingChanged: true,
            currentItem: curItem
        });
    }

    onDescriptionChange(value) {
        let curItem = this.state.currentItem;
        curItem.Description = value;
        this.setState({
            itemSettingChanged: true,
            currentItem: curItem
        });
    }

    onMinimumLocationSettingChange(checked) {
        let curItem = this.state.currentItem;
        curItem.NodeType = checked ? NodeTypeEnum.Min : NodeTypeEnum.Normal;
        if (checked) {
            if (!curItem.RMLocationSuiteAssociationIds || curItem.RMLocationSuiteAssociationIds.length == 0) {
                curItem.RMLocationSuiteAssociationIds = DefaultSuiteUniqueIds;
            }
        } else {
            curItem.RMLocationSuiteAssociationIds = [];
        }

        this.setState({
            itemSettingChanged: true,
            currentItem: curItem
        });
    }

    handleSuiteCheckChanged(ids) {
        // console.log(args);
        let curItem = this.state.currentItem;
        // let checkedItems = args.newValue.filter(t => t.checked == true);
        // let ids = checkedItems.map(t => { return t.value; });
        curItem.RMLocationSuiteAssociationIds = ids;
        this.setState({
            itemSettingChanged: true,
            currentItem: curItem
        });
    }

    initSuiteCheckBoxSource() {
        let allSuiteItems = this.state.allSuiteItems;
        let itemSource = [];
        allSuiteItems.forEach((item) => {
            itemSource.push({
                text: this.wrapperI18N(item.Name),
                value: item.UniqueId,
                checked: this.isCheckedSuiteItem(item.UniqueId)
            });
        });
        return itemSource;
    }

    isCheckedSuiteItem(uniqueId) {
        let selCheckedIds = this.state.currentItem.RMLocationSuiteAssociationIds;
        if (selCheckedIds && selCheckedIds.length > 0) {
            return selCheckedIds.indexOf(uniqueId) > -1;
        }
        return false;
    }

    processHasMatchChildren(item) {
        let hasMatchChildren = false;
        if (item && item.SubLocations) {
            item.SubLocations.forEach((subitem) => {
                if (!hasMatchChildren && subitem.Name.indexOf(this.treeContext.searchKey) > -1) {
                    hasMatchChildren = true;
                }
                hasMatchChildren |= this.processHasMatchChildren(subitem);
            });
        }
        return item.hasMatchChildren = hasMatchChildren;
    }

    //actionType: 1=rename, 2=retire, 3=reactive, 4=delete item
    refreshSelectedNodeInfo(item, actionType) {
        let props;
        switch (actionType) {
            case 4:
                this.setState({
                    itemSettingChanged: false,
                    selectedItem: null,
                    currentItem: null
                });
                return;
            case 1:
                props = ["Name"];
                break;
            default:
                props = [];
                break;
        }

        this.copyProps(item, this.state.selectedItem, props);
        this.copyProps(item, this.state.currentItem, props);

        this.setState({
            selectedItem: this.state.selectedItem,
            currentItem: this.state.currentItem
        });
    }

    replaceSpecialCharacters(str) {
        var reg1 = new RegExp("&", "ig");
        var reg2 = new RegExp("\"", "ig");
        str = str.replace(reg1, "＆");
        str = str.replace(reg2, "＂");
        return str;
    }

    resetTreeData(data) {
        let treeData = $.parseJSON(data);   // Fortify Issue Type: JSON Injection; Sink Details: reset tree data; Ignore Reason: 前后台对象存在对应关系
        if (this.treeContext.searchKey) {
            if (treeData) {
                this.processHasMatchChildren(treeData);
                treeData = [treeData];
            } else {
                treeData = [];
            }
        }
        this.setState({ treeData: treeData });
    }

    searchData(key) {
        key = !key ? "" : key.trim();
        if (key.length == 0) {
            this.getTreeData();
        } else {
            $.ajax({
                type: "GET",
                url: "/api/LocationManagementApi/Search",
                //contentType: 'application/json;charset=utf-8',
                data: "locationStr=" + this.replaceSpecialCharacters(key),
                async: true,
                beforeSend: function () {
                    $$.loading(true);
                },
                complete: function () {
                    $$.loading(false);
                },
                success: (data) => {
                    this.treeContext.searchKey = key;
                    this.treeContext.pagerByServer = false;
                    this.resetTreeData(data);
                },
                error: (msg) => {
                    //alert(msg.responseText);
                },
                dataType: "json"
            });
        }
    }

    setNewSelectedItem(item) {
        this.setState({
            selectedItem: item,
            currentItem: JSON.parse(JSON.stringify(item)),
            selectedRuleLevel: { name: "", value: "" },
            itemSettingChanged: false
        });
        this.hideMessageTip();
    }

    showIfLeaveWithoutSaveMsg(funcAllow) {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_TM_WithoutSavingMsg,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: () => funcAllow(false) },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => funcAllow(true) },
            ]
        };
        $$.messagedialog(true, args);
    }

    showMessageTip(type, msg) {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    synchronise() {
        $$.messagedialog(false);
        $.ajax({
            type: "GET",
            dataType: "JSON",
            url: "/api/LocationSynchronizationApi/RunSync",
            data: "fromTimerJobPage=false",
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: () => {
                showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>);
            },
            error: () => {
                showToast.error(RMResx.RM_JS_BCM_TermSync_SyncFailMessage);
            }
        });
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }


    handleLocationsImport(e){
        this.setState({ showImportPanel: { show: true } });
    }
    handleDownloadTemplate = (e) => {
        let downloadTemplate = StringUtil.newGuid();
        var $downloadStatusKey = $("#importDownloadFlag");
        $downloadStatusKey.val(downloadTemplate);

        $("#ph-form-download")
            .attr("action", "/api/LocationManagementApi/DownloadTemplate")
            .submit();
    }

    handleUpload(args) {
        const isSucceed = args.isSucceed;
        $$.log(isSucceed ? 'uploadSuccess:' : 'uploadError', args);
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


    
    handleImportCancelClick(e) {
        this.setState({ showImportPanel: { show: false } });
    }

    handleImportSaveClick = (e) => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        const formData = new FormData();
        formData.append('locationFileUp', this.files.file, this.files.fileName);
        fetch('/api/LocationManagementApi/ImportData', {
            method: 'POST',
            body: formData,
        })
            .then(async function (response) {
                return await response.text();
            })
            .then(function (data) {
                $$.loading(false);
                if (data == "ok") {
                    showToast.success(RMResx.RM_SPS_Location_SaveImportSuccess);
                }
                else {
                    showToast.error(data);
                }
                // return result;
            }).catch((e) => {
                $$.loading(false);
                showToast.error(RMResx.RM_SPS_Location_SaveImportFailed);
            });
        this.setState({ showImportPanel: { show: false } });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    renderSelectedItemName() {
        let selItem = this.state.currentItem;
        if (!selItem || selItem.NodeType == NodeTypeEnum.Root) {
            return null;
        }
        let labelName = RMResx.RM_LM_LocationNameLabel;
        //let typeName = labelName.replace(":", "");
        return <div
            className="normal-label-font1"
            //data-tooltip={typeName}
            style={{ marginBottom: "20px" }}>
            {selItem.Name}
        </div>;
    }

    renderSelectedItemDes() {
        let selItem = this.state.currentItem;
        if (!selItem || selItem.NodeType == NodeTypeEnum.Root) {
            return null;
        }

        return <React.Fragment>
            <div className="normal-label-font1" style={{margin:"24px 0 8px"}}>
                <span>{StringUtil.trimEndColon(RMResx.RM_TM_TermDescLabel)}</span>
            </div>
            <div>
                <R.Input
                    type="textarea"
                    height={88}
                    value={!selItem.Description ? "" : selItem.Description}
                    onChange={this.onDescriptionChange}
                    aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_TM_TermDescLabel) }}
                />
                <$g.ValidationMsg
                    show={!!selItem.Description && selItem.Description.length > 1000}>
                    {RMResx.RM_JS_BCM_Save_Validation_DescriptionInvalid}
                </$g.ValidationMsg>
            </div>
        </React.Fragment>;
    }

    renderSelectedItemSpace() {
        let selItem = this.state.currentItem;
        if (!selItem || selItem.NodeType == NodeTypeEnum.Root) {
            return null;
        }
        let space = !selItem.AvailableSpace ? null : selItem.AvailableSpace;

        return <React.Fragment>
            <div className="normal-label-font1 margin-top-24">
                <span>{StringUtil.trimEndColon(RMResx.RM_LM_LocationSettingLabelInfo)}</span>
            </div>
            <div className="normal-label-font2" style={{margin: "8px 0 24px"}}>
                <span>{RMResx.RM_LM_LocationSettingTotalSpace}</span>
                <span style={{marginLeft:"10px", marginRight:"6px"}}>
                    <R.Input
                        id="raLMTotalSpaceIpt"
                        type="number"
                        hasControl
                        width={150}
                        title={space}
                        value={space}
                        maxlength={9}
                        float={2}
                        fixFloat={false}
                        onChange={this.onSpaceChange}
                        autoOnChange={true}
                        aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_LM_LocationSettingLabelInfo) }}
                    />
                </span>
                
                <span tabIndex='0'>{RMResx.RM_LM_LocationSettingMeters}</span>
            </div>
        </React.Fragment>;
    }

    renderSelectedItemSetting() {
        let selItem = this.state.currentItem;
        let selSuiteTip = StringUtil.trimEndColon(RMResx.RM_LM_SelectSuiteTip);
        if (!selItem || selItem.NodeType == NodeTypeEnum.Root) {
            return null;
        }
        let isMin = selItem.NodeType == NodeTypeEnum.Min ? true : false;
        return <React.Fragment>
            <div>
                <div className=' inline-block vertical-middle'>
                    <R.Checkbox
                        id="raLMAllowContainCreate"
                        text={RMResx.RM_LM_MinimumLocationSettingDesc}
                        title={RMResx.RM_LM_MinimumLocationSettingDesc}
                        checked={isMin}
                        onChange={this.onMinimumLocationSettingChange} />
                <$g.Popover>{RMResx.RM_LM_MinimumLocationSettingTip}</$g.Popover>
                </div>
                {isMin && <div>
                    <div className="require normal-label-font1 margin-bottom-10 margin-top-24" title={selSuiteTip}>
                        <span id="ariaSelectSuite">{selSuiteTip}</span>
                    </div>
                    <R.Checkbox.Group
                        block={true}
                        name="ck-group-suite"
                        items={this.initSuiteCheckBoxSource()}
                        onChange={this.handleSuiteCheckChanged}
                        aria={{
                            ariaLabelledby: "ariaSelectSuite",
                            ariaRequired: true
                        }}
                    />
                </div>}
            </div>
        </React.Fragment>;
    }


    renderImportPanel() {
        let requestVerificationToken = getRequestVerificationToken();
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
                            <form id="ph-form-download" method="POST" action="">
                                <input type="hidden" id="importDownloadFlag" name="importDownloadFlag" value="" />
                                <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                            </form>
                            <span className="tm-import-download-span" onClick={this.handleDownloadTemplate} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_JS_TM_DownLoadTemplate}</span>
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
                    </div>
                </R.Validation>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleImportCancelClick} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleImportSaveClick} />
            </>
        </R.Panel>;
    }



    render() {
        let changeSetting = this.state.itemSettingChanged;
        let selectedItem = this.state.selectedItem;
        let showBtns = selectedItem &&  selectedItem.NodeType != NodeTypeEnum.Root;
        return <div id="rmLocationManagement" className="rm-tm-main-container">
            <section className="rm-tm-header">
                <Prompt message={RMResx.RM_TM_WithoutSavingMsg} when={changeSetting} />
                <$g.SiteMap data={[SiteMapLinks.PRM_LocationManagement]} />
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    onClose={this.hideMessageTip}
                    status={{ show: this.state.showTip }}
                />
                
                <div id="location-menu" className="margin-top-m margin-bottom-m">
                    <R.Button
                        icon="fia-import"
                        text={RMResx.RM_JS_TM_Import}
                        onClick={this.handleLocationsImport} />
                </div>
                    
            </section>
            <section className="rm-tm-content">
                <div className="rm-tm-splitter-container">
                    <R.Splitter minAsize="25%" minBsize="60%" defaultAsize="40%">
                        <div className="ra-splitter-left">
                            <div>
                                <div className="ra-splitter-search">
                                    <R.Searchbox
                                        title={RMResx.RM_JS_TM_SearchTxt}
                                        width='100%'
                                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                                        disabled={false}
                                        onSearch={this.onSearch}
                                    />
                                </div>
                            </div>
                            <div id="rmLocationManagementTree">
                                <$g.TreeView
                                    id="treeview"
                                    classicMode
                                    items={this.state.treeData}
                                    searchKey={this.state.searchKey}
                                    treeContext={this.treeContext}
                                />
                            </div>
                        </div>
                        <div className="ra-splitter-right rm-settings-container">
                            <div className="rm-settings-header">
                                <div className="ra-splitter-head-title">{RMResx.RM_TM_GSetingLabel}</div>
                            </div>
                            <div className="rm-settings-content">
                                {this.renderSelectedItemName()}
                                {this.renderSelectedItemDes()}
                                {this.renderSelectedItemSpace()}
                                {this.renderSelectedItemSetting()}
                            </div>
                            {showBtns && <div className="rm-settings-footer">
                                <div className="tm-settings-footer-button">
                                    <R.Button
                                        text={RMResx.RM_JS_Common_Cancel}
                                        disabled={!changeSetting}
                                        onClick={this.onCancelChangedClick} />
                                    <R.Button
                                        id="raLMSaveBtn"
                                        primary={true}
                                        classify="theme"
                                        text={RMResx.RM_JS_Common_Save}
                                        disabled={!changeSetting}
                                        onClick={this.onSaveSettingClick} />
                                </div>
                            </div>
                            }
                        </div>
                    </R.Splitter>
                </div>
            </section>
            {this.renderImportPanel()}
        </div>;
    }
}