import React, { Component } from "react";
import Action from "./Actions";
import { ActionTypes, EmailTemplateType } from "./Contains";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import RouterUrls from "../../../Constants/RouterUrls";
import { bindEvents, isShowActionByDC } from "../../../Utilities/CommonUtil";
import { NormalCell } from "../../Common/TableTemplateCell";
import { showToast } from "../../../Utilities/CommonUtil";
import "../../../Less/CP/EmailTemplate.less";
import { Messagebox } from "../../Common/Messagebox";

const isMultiGeoMainDC = isShowActionByDC();
class EmailTemplateTableRow extends R.TableRow {

    onAction = (actionType) => {
        this.dispatch(actionType);
    };

    specificTemplateActions(type) {
        if (type === EmailTemplateType.RecordsForReview) {
            return <React.Fragment>
                <R.Button
                    text={RMResx.RM_JS_Common_Copy}
                    title={RMResx.RM_JS_Common_Copy}
                    onClick={this.onAction.bind(this, ActionTypes.COPY)}
                />
                <R.Button
                    text={RMResx.RM_JS_Common_Edit}
                    title={RMResx.RM_JS_Common_Edit}
                    onClick={this.onAction.bind(this, ActionTypes.EDIT)}
                />
            </React.Fragment>;
        } else {
            return <R.Button
                text={RMResx.RM_JS_Common_Edit}
                title={RMResx.RM_JS_Common_Edit}
                icon="fia-edit"
                onClick={this.onAction.bind(this, ActionTypes.EDIT)}
            />;
        }
    }

    customTemplateActions(){
        return (
            <React.Fragment>
                <R.Button
                    text={RMResx.RM_JS_Common_Copy}
                    title={RMResx.RM_JS_Common_Copy}
                    onClick={this.onAction.bind(this, ActionTypes.COPY)}
                />
                <R.Button 
                    text={RMResx.RM_JS_Common_Edit} 
                    title={RMResx.RM_JS_Common_Edit} 
                    onClick={this.onAction.bind(this, ActionTypes.EDIT)} 
                /> 
                <R.Button 
                    text={RMResx.RM_JS_Common_Delete} 
                    title={RMResx.RM_JS_Common_Delete} 
                    onClick={this.onAction.bind(this, ActionTypes.DELETE)} 
                />
            </React.Fragment>
        );
    }

    render(Row, Cell) {
        const { Name, DisplayType, IsCustomTemplate, Type } = this.props.rowData;
        const templateActions = IsCustomTemplate ? 
            this.customTemplateActions() : 
            this.specificTemplateActions(Type);
        return (
            <Row action={isMultiGeoMainDC ? templateActions : null}>
                <NormalCell Cell={Cell} contentText={Name}></NormalCell>
                <NormalCell Cell={Cell} contentText={DisplayType}></NormalCell>
            </Row>
        );
    }
}

export default class EmailTemplate extends Component {
    constructor(props) {
        super(props);
        this.state = {
            noneMessage: RMResx.RM_JS_JM_Tableview_Nodata,
            rowData: [],
            MessageTipInfo: {
                showTip: false,
                type: "success",
                content: "",
            },
            totalCount: 0,
            pagerIndex: 0,
            pagerSize: 15
        };
        this.initBingEvents();
        this.columns = this.initColumns();
    }
    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }

    componentDidMount() {
        this.initRowData();
    }

    initBingEvents() {
        bindEvents(this, "onRowEvent", "hideMessageTip");
    }

    Type = {
        1: RMResx.RM_JS_CP_EamilTemplate_BoxOrFile,
        2: RMResx.RM_JS_CP_EamilTemplate_BoxOrFileOrRecord,
    };

    initRowData() {
        $$.loading(true);
        let urlData = "/api/CPApi/GetAllEmailTemplate";
        let option = {
            url: urlData,
            method: "POST",
            data: {
                PagerIndex: this.state.pagerIndex,
                PagerSize: this.state.pagerSize
            }
        };
        fetchUtility(option)
            .then((res) => {
                $$.loading(false);
                let templateInfo = res;
                let templates = templateInfo.Items;
                let pagerInfo = templateInfo.PagerInfo;
                if (templates.length != 0) {
                    for (let item of templates) {
                        item.DisplayType = this.Type[item.Type];
                    }
                }
                this.setState({
                    rowData: templates,
                    totalCount: pagerInfo.TotalCount
                });
            })
            .catch((e) => {
                $$.loading(false);
            });
    }
    initColumns() {
        return [
            {
                headerTemplate: RMResx.RM_JS_CP_EamilTemplate_Name,
                width: [150],
                resizeable: true,
            },
            {
                header: RMResx.RM_JS_CP_EamilTemplate_Type,
                width: [150],
                resizeable: true,
            }
        ];
    }

    editCurrentRow(item) {
        this.props.history.push({
            pathname: RouterUrls.CP_EditEmailTemplate + `/?id=${item.Id}`
        });
    }

    copyCurrentRow(item) {
        this.props.history.push({
            pathname: RouterUrls.CP_CreateEmailTemplate + `/?sourceid=${item.Id}`
        });
    }

    openDeleteMessagebox = (item) => {
        Messagebox({ content: RMResx.RM_JS_CP_EamilTemplate_DeleteMsg, actionFun: this.onDeleteItem.bind(this, item) });
    };

    onDeleteItem = (item) => {
        $$.loading(true);
        let urlData = "/api/CPApi/DeleteEmailTemplate";
        let option = {
            url: urlData,
            method: "POST",
            data: item.UniqueId
        };
        fetchUtility(option)
            .then((res) => {
                $$.loading(false);
                if(res === ""){
                    showToast.success(RMResx.RM_JS_CP_EamilTemplate_DeleteSuccess);
                    this.setPager(0, this.state.pagerSize);
                    return;
                } else if (res === "-2") {
                    showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
                }
                showToast.error(res);
            })
            .catch((e) => {
                $$.loading(false);
                showToast.error(RMResx.RM_RC_Common_Msg_ShowReportError);
            });
    }

    doAction = ({type}) =>{
        switch(type){
            case ActionTypes.ADD:
                this.initRowData();
                break;
            default:
        }
        this.setPager(0, this.state.pagerSize);
    }

    onRowEvent(args) {
        let rowData = args.rowData;
        switch (args.type) {
            case ActionTypes.COPY:
                this.copyCurrentRow(rowData);
                this.setPager(0, this.state.pagerSize);
                break;
            case ActionTypes.EDIT:
                this.editCurrentRow(rowData);
                this.setPager(0, this.state.pagerSize);
                break;
            case ActionTypes.DELETE:
                this.openDeleteMessagebox(rowData);
                break;
            default:
                break;
        }
    }

    hideMessageTip() {
        this.setState({
            MessageTipInfo: { showTip: false },
        });
    }

    onPagerChange = (pagerIndex, pagerSize, callback) => {
        this.setPager(pagerIndex, pagerSize);
        callback(true);
    }

    setPager(pagerIndex, pagerSize){
        this.setState({
            pagerIndex: pagerIndex,
            pagerSize: pagerSize,
        },()=>{
            this.initRowData();
        });
    }

    render() {
        return (
            <div id="rmEmailTemplate">
                <$g.SiteMap
                    data={[SiteMapLinks.CP, SiteMapLinks.CP_EmailTemplate]}
                />
                <div className="ra-page-container">
                    <div className="ra-main-navbar">
                        {isMultiGeoMainDC && <Action doAction={this.doAction} />}
                    </div>
                    <div className="ra-main-table">
                        <R.Table
                            id="Id"
                            height="auto"
                            columns={this.columns}
                            rowTemplate={EmailTemplateTableRow}
                            items={this.state.rowData}
                            onRowEvent={this.onRowEvent}
                            noneMessage={this.state.noneMessage}
                        />
                    </div>
                    <div className="ra-main-footer">
                        <$g.Pager
                            itemsCount={this.state.totalCount}
                            showPagerSize={true}
                            showPagerCounter={true}
                            pagerIndex={this.state.pagerIndex}
                            pagerSize={this.state.pagerSize}
                            pagerSizeOptions={[5, 10, 15, 50]}
                            onChange={this.onPagerChange} />
                    </div>
                </div>
            </div>
        );
    }
}
