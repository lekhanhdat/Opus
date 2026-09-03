import {Component} from "react";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import {GridCellButtonType} from "../../Constants/Constants";
import {bindEvents} from "../../Utilities/CommonUtil";
import {TextCell} from "../Common/Datagrid/Components/TextCell";
import ButtonCell from "../Common/Datagrid/Components/ButtonCell";
import "../../Less/PRM/ContainerSize.less";


export default class ContainerSize extends Component {
    constructor(props) {
        super(props);
        this.state = {
            noneMessage: RMResx.RM_JS_JM_Tableview_Nodata,
            gridData: []
        };
        this.initBingEvents();
        this.getGridData();
        this.columns = this.getGridColumns();
        this.cells = this.getGridCells();
        this.tempIdIndex = -1;
    }

    componentDidMount() {
    }

    initBingEvents() {
        bindEvents(this, "isGridRowEditing", "showDefaultBtns", "showEditingBtns",
            "onGridCellTextChange", "onBoxTypeItemEditClick", "onBoxTypeItemDelClick",
            "onBoxTypeItemSaveClick", "onBoxTypeItemCancelClick", "onCreateClick",
            "onSetDefaultClick"
        );
    }

    getGridData() {
        $$.loading(true);
        let urlData = "/api/ContainerApi/GetAllContainers";
        let option = {
            url: urlData,
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            //刷新列表
            $$.loading(false);
            let data = JSON.parse(res);
            for (let item of data) {
                item.origin = Object.assign({}, item);
            }
            this.setState({
                gridData: data
            });

        }).catch((e) => {
            $$.loading(false);
        });
    }

    getGridColumns() {
        return [
            {
                headerTemplate: RMResx.RM_JS_PRM_CZ_ContentType,
                width: 20,
                isResizable: true
            }, {
                header: RMResx.RM_JS_PRM_CZ_Size,
                width: 15,
                isResizable: true
            }, {
                header: RMResx.RM_JS_PRM_CZ_Description,
                isResizable: true,
                width: 30
            }, {
                header: RMResx.RM_JS_PRM_CZ_Action,
                isResizable: true,
                width: 25
            }, {
                header: RMResx.RM_JS_PRM_CZ_Default,
                isResizable: true,
                width: 10
            }
        ];
    }

    getGridCells() {
        return [
            {
                cellComponent: TextCell,
                isEditing: this.isGridRowEditing,
                props: {
                    name: "TypeName",
                    onChange: this.onGridCellTextChange
                }
            },
            {
                cellComponent: TextCell,
                isEditing: this.isGridRowEditing,
                props: {
                    name: "Size",
                    onChange: this.onGridCellTextChange
                }
            },
            {
                cellComponent: TextCell,
                isEditing: this.isGridRowEditing,
                props: {
                    name: "Description",
                    onChange: this.onGridCellTextChange
                }
            },
            {
                cellComponent: ButtonCell,
                isEditing: this.isGridRowEditing,
                props: {
                    buttons: [
                        {
                            isShow: this.showDefaultBtns,
                            props: {
                                iconClass: "ra-iconbtn-icon-edit",
                                text: RMResx.RM_JS_Common_Edit,
                                onClick: this.onBoxTypeItemEditClick
                            }
                        },
                        {
                            isShow: this.showDefaultBtns,
                            props: {
                                iconClass: "ra-iconbtn-icon-del",
                                text: RMResx.RM_JS_Common_Delete,
                                onClick: this.onBoxTypeItemDelClick
                            }
                        },
                        {
                            isShow: this.showEditingBtns,
                            props: {
                                iconClass: "ra-iconbtn-icon-save",
                                text: RMResx.RM_JS_Common_Save,
                                onClick: this.onBoxTypeItemSaveClick
                            }
                        },
                        {
                            isShow: this.showEditingBtns,
                            props: {
                                iconClass: "ra-iconbtn-icon-cancel",
                                text: RMResx.RM_JS_Common_Cancel,
                                onClick: this.onBoxTypeItemCancelClick
                            }
                        }
                    ]
                }
            },
            {
                cellComponent: ButtonCell,
                props: {
                    buttons: [
                        {
                            buttonType: GridCellButtonType.Switch,
                            isChecked: (item) => !!item.IsDefault,
                            props: {
                                onChange: this.onSetDefaultClick
                            }
                        }
                    ]
                }
            }
        ];
    }

    getGridItem(id) {
        let gridData = this.state.gridData;
        for (let item of gridData) {
            if (item.Id == id) {
                return item;
            }
        }
        return null;
    }

    removeGridItem(id) {
        let gridData = this.state.gridData,
            newData = [];
        for (let item of gridData) {
            if (item.Id != id) {
                newData.push(item);
            }
        }
        //this.state.gridData = newData;
        this.setState({gridData: newData});
    }

    isGridRowEditing(item) {
        return item.__Editing;
    }

    showDefaultBtns(item) {
        return !this.isGridRowEditing(item);
    }

    showEditingBtns(item) {
        return this.isGridRowEditing(item);
    }

    onGridCellTextChange(e, args) {
        let cellFieldName = args.fieldName,
            newVal = args.newValue,
            item = this.getGridItem(args.item.Id);
        item[cellFieldName] = newVal;
        this.resetGridData();
    }

    onBoxTypeItemEditClick(item) {
        let oitem = this.getGridItem(item.Id);
        oitem.__Editing = true;
        this.resetGridData();
        setTimeout(() => {
            $(".datagrid_cell_0").last()
                .find(".ra-input").first().focus();
        }, 200);
    }

    onBoxTypeItemDelClick(item) {
        let itemId = item.Id;
        this.showIfDeleteMsg(() => {
            this.hideMesagebox();
            $.ajax({
                type: "POST",
                url: "/api/ContainerApi/DeleteContainerType",
                contentType: "application/json;charset=utf-8",
                data: JSON.stringify(itemId),
                //beforeSend: function () {
                //    $$.loading(true);
                //},
                //complete: function () {
                //    $$.loading(false);
                //},
                success: (data) => {
                    this.removeGridItem(itemId);
                },
                error: function (msg) {
                },
                dataType: "json"
            });
        });

    }

    onBoxTypeItemSaveClick(item) {
        let oitem = this.getGridItem(item.Id),
            size = oitem.Size,
            typeName = $.trim(oitem.TypeName);
        if (size == "" || isNaN(size) || size <= 0 || size > 3.402823E+38) {
            this.showAlertMsg(RMResx.RM_CZ_SizeValueInvalid);
            return;
        }
        if (typeName == "") {
            this.showAlertMsg(RMResx.RM_CZ_NameIsNull);
            return;
        }

        let postUrl,
            postData = {
                TypeName: typeName,
                Size: size,
                Description: oitem.Description,
                IsDefault: oitem.IsDefault
            };
        if (oitem.Id < 0) {
            postUrl = "/api/ContainerApi/SaveContainerType";
        } else {
            postUrl = "/api/ContainerApi/UpdateContainerType";
            postData.ContainerId = oitem.Id;
        }
        $.ajax({
            type: "POST",
            url: postUrl,
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify(postData),
            //beforeSend: function () {
            //    $$.loading(true);
            //},
            //complete: function () {
            //    $$.loading(false);
            //},
            success: (resultData) => {
                var result = $.parseJSON(resultData);   // Fortify Issue Type: JSON Injection; Ignore Reason: 前后台对象存在对应关系
                if (result.message == "container has same name") {
                    this.showAlertMsg(RMResx.RM_CZ_NameRepetedMsg);
                } else {
                    Object.assign(oitem, result);
                    oitem.origin = result;
                    oitem.__Editing = false;
                    this.resetGridData();
                }
            },
            error: function (msg) {
            },
            dataType: "json"
        });
    }

    onBoxTypeItemCancelClick(item) {
        let itemId = item.Id;
        if (itemId < 0) {
            this.removeGridItem(itemId);
        } else {
            let oitem = this.getGridItem(itemId);
            Object.assign(oitem, oitem.origin);
            oitem.__Editing = false;
            this.resetGridData();
        }
    }

    setItemAsDefault(checked, item) {
        let itemId = item.Id,
            gridData = this.state.gridData;
        if (checked) {
            for (let gridItem of gridData) {
                gridItem.IsDefault = gridItem.Id == itemId;
            }
        } else {
            let gridItem = this.getGridItem(itemId);
            gridItem.IsDefault = false;
        }
        this.resetGridData();
    }

    onSetDefaultClick(checked, e, item) {
        let itemId = item.Id;
        //新增Item，未保存时，置为Default时，等Save时一起保存。
        if (itemId < 0) {
            this.setItemAsDefault(checked, item);
        } else {
            var postData = {
                ContainerId: itemId,
                IsDefault: checked
            };
            $.ajax({
                type: "POST",
                url: "/api/ContainerApi/UpdateContainerIsDefault",
                contentType: "application/json;charset=utf-8",
                data: JSON.stringify(postData),
                //beforeSend: function () {
                //    $$.loading(true);
                //},
                //complete: function () {
                //    $$.loading(false);
                //},
                success: (data) => {
                    this.setItemAsDefault(checked, item);
                },
                error: function (msg) {
                },
                dataType: "json"
            });
        }
    }

    onCreateClick(e) {
        let gridData = this.state.gridData;
        if (gridData.length > 0) {
            if (gridData[gridData.length - 1].Id < 0) {
                this.showAlertMsg(RMResx.RM_CZ_AddNextMsg);
                return;
            }
        }
        gridData.push({
            Id: this.tempIdIndex--,
            __Editing: true
        });
        this.resetGridData();
        setTimeout(() => {
            $(".datagrid_cell_0").last()
                .find(".ra-input").first().focus();
        }, 200);
    }

    resetGridData() {
        let gridData = this.state.gridData.slice();
        this.setState({
            gridData: gridData
        });
    }

    showIfDeleteMsg(funcDoDel) {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_CZ_DeleteMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => this.hideMesagebox()
                },
                {text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: funcDoDel},
            ]
        };
        $$.messagedialog(true, args);
    }

    showAlertMsg(content) {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: content,
            buttons: [
                {text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => this.hideMesagebox()}
            ]
        };
        $$.messagedialog(true, args);
    }

    hideMesagebox() {
        $$.messagedialog(false);
    }


    render() {
        return <div id="rmContainerSize">
            <$g.SiteMap data={[SiteMapLinks.PRM_ContainerSize]}/>
            <div className="container-size-container">
                <div className="ra-page-title1 ra-require" tabIndex="0">{RMResx.RM_CZ_ContentTitle}</div>
                <div className="container-size-grid">
                    <$g.Datagrid
                        rowId='Id'
                        horizontalFlag='persent'
                        height='auto'
                        noneMessage={this.state.noneMessage}
                        columns={this.columns}
                        cells={this.cells}
                        items={this.state.gridData}
                    />
                </div>
                <div>
                    <$g.IconButton
                        iconClass="ra-iconbtn-icon-create"
                        text={RMResx.RM_JS_Common_Create}
                        onClick={this.onCreateClick}
                    />
                </div>
            </div>
        </div>;
    }
}