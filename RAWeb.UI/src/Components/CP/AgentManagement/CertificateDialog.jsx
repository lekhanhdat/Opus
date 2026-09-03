import React from "react";
import Download from "../../Common/Download";
import {CertificatesTempalte} from "../AgentManagement/RowTemplate";

const updateCertificateResultCode = {
    AllSucceed: 0, //全部成功
    NoDefaultCertificate: 1, //没有设置default证书
    CertificateExpired: 2, //default证书过期
    NoActiveAgent: 3, //没有可用的agent
    AllFailed: 4, //更新证书到agent，全部失败
    HasFailed: 5 //更新证书到agent，部分失败
};
export default class CertificateDialog extends R.Component {
    idAttr = true;
    componentCreate () {
        this.state = {
            certificateDiaShow: false,
            certificateList: [],
            columns: this.getColumns(),
            tipMsg: "",
            tipType: "",
            showTip: "",
            isAllowUpdateCertificate: true
        };
    }

    componentReceive () {
        this.setState({ certificateDiaShow: true },()=>{
            this.initData();
        });
    }

    getColumns() {
        let column = [
            {
                header: RMResx.RM_CP_AM_Certificate_Column_Default,
                resizeable: true,
                width: 95
            },{
                header: RMResx.RM_CP_AM_Certificate_Column_Thumbprint,
                resizeable: true,
                width: 170
            }, {
                headerTemplate: RMResx.RM_CP_AM_Certificate_Column_ExpriationDate,
                width: 135,
                resizeable: true
            }, {
                header: RMResx.RM_CP_AM_Certificate_Column_Status,
                width: 110,
                resizeable: true,
            },  {
                header: "",
                resizeable: true,
                width: 100
            }
        ];
        return column;
    }

    initData(){
        this.setState({showTip: false});
        this.setCertificateList();
    }

    showMsgTip(tipMsg, tipType){
        this.setState({
            tipMsg: tipMsg,
            tipType: tipType,
            showTip: true
        });
    }

    hideMsgTip = () =>{
        this.setState({showTip: false}); 
    }

    setCertificateList(){
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/GetAllCertificatesInfo",
            method: "POST",
            data: true
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setState({ certificateList: res });
            this.canUpdateCertificate();
        }).catch((e) => {
            $$.loading(false);
        });
    }

    canUpdateCertificate(){
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/CanUpdateCertificate2Agents",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setState({isAllowUpdateCertificate: res});
        }).catch((e) => {
            $$.loading(false);
        });
    }

    showCreateCertificateTipMsgBox = () => {
        $$.messagedialog(true, {
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{RMResx.RM_CP_AM_Certificate_MsgBoxTip_Create}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: ()=>{ $$.messagedialog(false);}},
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.createCertificate}
            ],
        });
    }

    createCertificate = () =>{
        $$.loading(true);
        $$.messagedialog(false);
        let option = {
            url: "/api/CPAgentMgmtApi/CreateCertificate",
            method: "POST",
            data: true
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res && res == "0"){
                this.setCertificateList();
            }else if(res && res == "-1"){
                this.showMsgTip(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage, "error");
            }else{
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_OperationFailed_Tip, "error");
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    showDeleteCertificateTipMsgBox = (args) =>{
        $$.messagedialog(true, {
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{RMResx.RM_CP_AM_Certificate_MsgBoxTip_Delete}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: ()=>{ $$.messagedialog(false);}},
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: ()=>{ this.getAllAgentsByCertificateId(args);}}
            ],
        });
    }

    getAllAgentsByCertificateId(args){
        $$.loading(true);
        $$.messagedialog(false);
        let option = {
            url: "/api/CPAgentMgmtApi/GetAllAgentsByCertificate",
            method: "POST",
            data: args.Id
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(!res){
                this.deleteCertificate(args);
            }else{
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_Tip_Delete_CurrentBeUsed, "error");
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    deleteCertificate = (args) =>{
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/DeleteCertificate",
            method: "POST",
            data: args.Id
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res){
                this.setCertificateList();
            }else{
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_OperationFailed_Tip, "error");
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    showDefaultCertificateTipMsgBox = (args) =>{
        $$.messagedialog(true, {
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{RMResx.RM_CP_AM_Certificate_MsgBoxTip_SetDefault}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: ()=>{ $$.messagedialog(false);}},
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: ()=>{ this.setDefaultCertificate(args);}}
            ],
        });
    }

    setDefaultCertificate = (args) =>{
        $$.loading(true);
        $$.messagedialog(false);
        let option = {
            url: "/api/CPAgentMgmtApi/SetAsDefaultCertificate",
            method: "POST",
            data: args.Id
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res){
                this.setCertificateList();
            }else{
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_OperationFailed_Tip, "error");
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    showUploadToAllAgentsTipMsgBox = () =>{
        $$.messagedialog(true, {
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{RMResx.RM_CP_AM_Certificate_MsgBoxTip_UploadToAllAgents}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: ()=>{ $$.messagedialog(false);}},
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.uploadToAllAgents}
            ],
        });
        return false;
    }

    uploadToAllAgents = () =>{
        $$.loading(true);
        $$.messagedialog(false);
        let option = {
            url: "/api/CPAgentMgmtApi/UpdateCertificate2Agents",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setCertificateList();
            this.showUploadCertificateMsgTip(res);
        }).catch((e) => {
            $$.loading(false);
        });

        return false;
    }

    showUploadCertificateMsgTip(args){
        switch(args.ResultCode){
            case updateCertificateResultCode.AllSucceed:
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_Tip_UploadToAllAgentsSuccess, "success"); 
                break;  
            case updateCertificateResultCode.NoDefaultCertificate:
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_Tip_UploadToAllAgentsFailed_NotHasDefault, "error"); 
                break;  
            case updateCertificateResultCode.CertificateExpired:
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_Tip_UploadToAllAgentsFailed_Expired, "error"); 
                break;  
            case updateCertificateResultCode.NoActiveAgent:
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_Tip_UploadToAllAgentsFailed_NoActiveAgent, "error"); 
                break; 
            case updateCertificateResultCode.AllFailed:
                this.showMsgTip(RMResx.RM_CP_AM_Certificate_Tip_UploadToAllAgentsFailed_AllFailed, "error"); 
                break;
            case updateCertificateResultCode.HasFailed: 
                var errorMsgTip = this.getPartialFailureMsgTip(args);
                this.showMsgTip(errorMsgTip, "error"); 
                break;
        }
    }

    getPartialFailureMsgTip(args){
        //"0"成功，"1"失败
        let agentResults = args.Agents;
        let updatedSuccessfulCertiAgentsList = agentResults.filter((item)=>{return item.Result === "0";}).map((item)=>{ return item.AgentName; });
        let updatedFailedCertiAgentsList = agentResults.filter((item)=>{return item.Result === "1";}).map((item)=>{ return item.AgentName; });
        let updatedSuccessfulCertiTip = RMResx.RM_CP_AM_Certificate_Tip_UploadToWhichAgentsSuccess.format(updatedSuccessfulCertiAgentsList.join(", "));
        let updatedFailedCertiTip = RMResx.RM_CP_AM_Certificate_Tip_UploadToWhichAgentsFail.format(updatedFailedCertiAgentsList.join(", "));
        let updatedCertiTip = <div>
            <div>{updatedSuccessfulCertiTip}</div>
            <div>{updatedFailedCertiTip}</div>
        </div>;
        return updatedCertiTip;
    }

    downloadCertificate = (args) => {
        this.dispatch('raCPAgentDownloadCertificate', '/api/CPAgentMgmtApi/DownloadCertById', [{name:"certId",value: args.Id}]);
    };

    closeCertificateDialog = () => {
        this.setState({ certificateDiaShow: false});
    };

    onRowEvent = (args) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'setDefault':
                this.showDefaultCertificateTipMsgBox(rowData);
                break;
            case 'delete':
                this.showDeleteCertificateTipMsgBox(rowData);
                break;
            case 'download':
                this.downloadCertificate(rowData);
                break;
            default:
                break;
        }
    };

    renderMsgTip(){
        if(this.state.showTip){
            return <div className="margin-bottom-s">
                <R.Messagebar
                    className="margin-bottom-s"
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                    onClose={this.hideMsgTip}
                />
            </div>;
        }else{
            return "";
        }
    }

    renderDownloadCertificate() {
        return <Download id="raCPAgentDownloadCertificate"></Download>;
    }

    renderAosAppRegistrationLink(){
        let appRegisterURL = this.props.appRegisterURL;
        return <div tabIndex="0" className="ra-main-italics"> 
            <$g.I18NProvider msg={RMResx.RM_CP_Agent_CertDialog_RegistIntroduce}>
                <a className="ra-link-a" href={appRegisterURL} target="blank" tabIndex="0">{RMResx.RM_CP_Agent_CertDialog_RegistLink}</a>
            </$g.I18NProvider>
            <div className="margin-top-s"> 
                {RMResx.RM_CP_AM_Certificate_UploadToAllAgentsExplain}
            </div>
        </div>;
    }

    renderActions(){
        return <div className="agent-certificate-action">
            <R.Button id="raCpAgentAddCertBtn" text={RMResx.RM_CP_AM_Certificate_CreateBtn} primary={true} classify="theme" onClick={this.showCreateCertificateTipMsgBox} />
        </div>;
    }

    renderRegistrationsTable(){
        return <div>
            <R.Table
                id="raRegistrationsTable"
                columns={this.state.columns}
                rowTemplate={CertificatesTempalte}
                items={this.state.certificateList}
                onRowEvent={this.onRowEvent}
            />
        </div>;
    }

    render () {
        return <div id={this.props.id}>
            <R.Panel
                id="raCPAgentCertificate"
                header={RMResx.RM_CP_Agent_Certificate}
                size={664}
                status={{ show: this.state.certificateDiaShow }}
                destroy={true}
                onHide={this.closeCertificateDialog}
            >
                <div>
                    {this.renderMsgTip()}
                    {this.renderAosAppRegistrationLink()}
                    {this.renderActions()}
                    {this.renderRegistrationsTable()}
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Close} onClick={this.closeCertificateDialog} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_CP_AM_Certificate_UploadToAllAgentsBtn} disabled={!this.state.isAllowUpdateCertificate} onClick={this.showUploadToAllAgentsTipMsgBox} />
                </>
            </R.Panel>
            {this.renderDownloadCertificate()}
        </div>;
    }
}