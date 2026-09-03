import { Component } from "react";
import {withRouter} from 'react-router-dom';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import {TemplateCardView} from "../../PRM/TemplateCommon.jsx";
import {bindEvents, isShowActionByDC} from "../../../Utilities/CommonUtil";
import RouterUrls from "../../../Constants/RouterUrls";
import {MAProcessCardView} from "./MAProcessCardView.jsx";
// import * as Constants from "./Constants";
import "../../../Less/RDM/MAProcessesManagement.less";
import ManualApproveSettings from "../ManualApproval/Settings";

const isMultiGeoMainDc = isShowActionByDC();
export default class MAProcessesManagement extends Component{
    constructor(props){
        super(props);
        this.state = {
            processItems:[],
            pageIndex: 1,
            pageSize: 10000,
            totalCount: 0,
            pageItemCount: 0,
            searchValue: "",
            noData: false,
            showTip: false,
            tipType: "success",
            tipMsg: "",
        };
        this.maProcessesListContainerRef = React.createRef();
        this.timer = null;
        bindEvents(this, "handleSearch","handleStopSearch","handleNewProcess","handleEditProcess","handleDelProcess",
            "handleViewProcess","onScroll","handleShowMessageBar", "handleHideMessageBar"
        );
    }

    componentDidMount(){
        this.initData();
        window.addEventListener('scroll', this.bindScroll);
        this.checkRequestStatus();
    }

    componentWillUnmount() {
        window.removeEventListener('scroll', this.bindScroll);
        // 卸载异步操作设置状态
        this.setState = (state, callback) => {
            return;
        };
    }

    checkRequestStatus = () => {
        var status = RM.CommStatus.get();
        if (status) {
            var contentMessage = status == RM.CommStatus.CreateSuccess ? RMResx.RM_RDM_MAProcess_Msg_CreatedSuc : RMResx.RM_RDM_MAProcess_Msg_EditedSuc;
            this.showMsgToast(contentMessage,"success");
            RM.CommStatus.remove();
        }
    };

    handleNewProcess(){
        // window.location.href = `${RouterUrls.RDM_CreateWorkFlow}`;
        this.redirectTo(RouterUrls.RDM_CreateWorkFlow);
    }

    handleEditProcess(Id){
        //window.location.href = `${RouterUrls.RDM_CreateWorkFlow}?id=${Id}`;
        this.redirectTo(`${RouterUrls.RDM_CreateWorkFlow}/?id=${Id}`);
    }

    handleDelProcess(Id){
        this.handleHideMsgBox();
        $$.loading(true);
        let option = {
            url:`/Api/CPApi/DeleteManualProcess`,
            method: "post",
            data: Id
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res.MessageType === 0){
                this.showMsgToast(RMResx.RM_RDM_MAProcess_Msg_DeletedSuc,"success");
                this.refreshPage();
            }else{
                var errorMsg = RMResx.RM_RDM_MAProcess_Msg_DeletedFail;
                if(res.FaildType == 17){
                    errorMsg = RMResx.RM_RDM_MAProcess_Msg_DeletedFailForRunningJob;
                }
                this.showMsgToast(errorMsg,"error");
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleViewProcess(Id){
        //window.location.href = `/Root/RDM/ViewWorkFlow?id=${Id}`;
        this.redirectTo(`${RouterUrls.RDM_ViewWorkFlow}/?id=${Id}`);
    }

    handleSearch(args) {
        let key = !args ? "" : args.trim();
        if(key.length > 0){
            this.setState({
                searchValue: key
            },()=>{
                this.initData();
            });
        }
    }

    handleStopSearch(args) {
        this.setState({
            searchValue: ""
        },()=>{
            this.initData();
        });
    }

    handleShowMessageBar = (type, msg) =>{
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    showMsgToast(content,type){
        let option = {
            content : content,
            classify : type
        };
        $$.toast(option);
    }

    handleHideMessageBar(){
        this.setState({showTip: false});
    }

    newGuid(){
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    redirectTo(url) {
        this.props.history.push({
            pathname: url
        });
    }


    initData(){
        //TODO first load data
        let reqOption = this.getRequestOption();
        $$.loading(true);
        fetchUtility(reqOption).then((result) => {
            if(result.ResultList && result.ResultList.length > 0)
            {
                this.setState({
                    processItems: result.ResultList,
                    totalCount: result.TotalCount,
                    pageItemCount: result.ResultList.length,
                    noData: false
                });
            }else{
                this.setState({
                    processItems: [],
                    pageIndex: 1,
                    totalCount: 0,
                    pageItemCount: 0,
                    noData: true
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    loadNewData(){
        let reqOption = this.getRequestOption();
        let pageItemCount = this.state.pageItemCount;
        this.timer = setTimeout(()=>{
            fetchUtility(reqOption).then((result) => {
                let newPageItems = result.ResultList;
                if(newPageItems && newPageItems.length > 0){
                    let beforePageItems = RM.deepcopy(this.state.processItems);
                    pageItemCount += newPageItems.length;
                    this.setState({
                        processItems: beforePageItems.concat(newPageItems),
                        pageItemCount: pageItemCount
                    });
                }
            }).catch((e) => {

            });

        }, 500);
    }


    handleShowDelMsgBox = (id)=> {
        let msgContent = RMResx.RM_RDM_MAProcess_MsgBox_Delete_Content;
        let buttons = [
            { text: RMResx.RM_JS_Common_Cancel, onClick: this.handleHideMsgBox },
            { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.handleDelProcess.bind(this, id) }];

        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: msgContent,
            buttons: buttons
        };
        $$.messagedialog(true, this.args);
    }

    handleHideMsgBox = ()=>{
        $$.messagedialog(false);
    }

    refreshPage(){
        this.setState({
            processItems: [],
            pageIndex: 1,
            totalCount: 0,
            pageItemCount: 0,
        },()=>{
            this.initData();
        });
    }

    getRequestOption(){
        let reqUrl = "/api/CPApi/GetManualProcesses";
        let reqOption = {
            url: reqUrl,
            method: "post",
            data: {
                PageIndex: this.state.pageIndex,
                PageSize: this.state.pageSize,
                SearchValue: this.state.searchValue
            }
        };
        return reqOption;
    }

    onScroll(){
        if ((this.maProcessesListContainerRef.current.scrollHeight - this.maProcessesListContainerRef.current.clientHeight ) > this.maProcessesListContainerRef.current.scrollTop) {
            //未到底
        } else {
        //已到底部
            if(this.timer){
                clearTimeout(this.timer);
            }
            let itemTotalCount = this.state.totalCount;
            let pageItemCount = this.state.pageItemCount;
            if(itemTotalCount ==  pageItemCount){
                console.log('no data need to load.');
            }else{
                let pageIndex = this.state.pageIndex;
                ++pageIndex;
                this.setState({
                    pageIndex: pageIndex
                },() => {
                    this.loadNewData();
                });
            }
        }
    }

    renderNavBar() {
        return <div className='reco-workflow-header'>
            <div className='navbar-search'>
                <R.Searchbox
                    placeholder={RMResx.RM_JS_TM_SearchTxt}
                    disabled={false}
                    onSearch={(args) => (args || "").trim() === "" ? this.handleStopSearch(args) : this.handleSearch(args)}
                    width={380}
                />
            </div>
            {isMultiGeoMainDc && (
                <R.Button primary={true} classify="theme" text={RMResx.RM_RDM_MAProcess_NewProcess} onClick={this.handleNewProcess} />
            )}     
        </div>;
    }

    renderProcessCards(){
        let processItems = RM.deepcopy(this.state.processItems);
        let cardComponents = [];
        processItems.map((item, index) =>{
            cardComponents.push(
                <MAProcessCardView
                    key={this.newGuid()}
                    item={item}
                    handleEditProcess={this.handleEditProcess}
                    handleDelProcess={this.handleShowDelMsgBox}
                    handleViewProcess={this.handleViewProcess}
                />
            );
        });
        return cardComponents;
    }

    render(){
        return <React.Fragment>
            <div className='raMAProcessesManagement'>
                <div className="reco-workflow-flex">
                    <$g.SiteMap data={[SiteMapLinks.RDM_WorkFlowManagement]} />
                    <ManualApproveSettings/>
                </div>
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{show: this.state.showTip}}
                    onClose={this.handleHideMessageBar}
                />
                <div id='maProcessesListMain'>
                    {this.renderNavBar()}
                    <div id="maProcessesListContainer" className="row row-xlg"
                        ref={this.maProcessesListContainerRef}
                        // onScroll={this.onScroll}
                    >
                        {this.renderProcessCards()}
                        {this.state.noData && <div className="temp-nodata-info">
                            <span tabIndex="0">{RMResx.RM_RDM_MAProcess_NoProcesses}</span>
                        </div>}
                    </div>
                </div>
            </div>
        </React.Fragment>;
    }
}