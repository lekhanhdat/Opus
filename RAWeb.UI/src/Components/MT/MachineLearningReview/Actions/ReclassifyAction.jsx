import ReclassifyForm from "../../../PRM/RecordsExplorer/Components/PhyReclassify";
import _ from "lodash";
import { ActionSuccessfulNeedJobToast } from "../Common";
import { showToast } from "../../../../Utilities/CommonUtil";
import { Source } from "../Constants/Source";
import "../../../../Less/PRM/Reclassify.less";

export default class ReclassifyPanel extends R.Component{
    constructor(props) {
        super(props);
        this.state = {
            showPanel: false,
        };
    }

    selectedTerm = {}

    notificationCacheData = [];

    notAllowReclassifyTermType = ['Root', 'TermGroup',  'TermSet'];

    componentDestroy() {
        this.dispatch('raNotificationMenu', 'close');
        this.dispatch('raNotification', []);
        this.dispatch('rmSuiteBar', true);
    }

    onHide = () => {
        this.setState({
            showPanel: false
        });
    };

    onShow = () => {
        this.setState({
            showPanel: true,
        });
    }

    onSaveReclassify = () => {
        let callback = (selectedTermItem, errorCallBack) => {
            if (this.notAllowReclassifyTermType.includes(selectedTermItem.Type)) {
                errorCallBack(RMResx.RM_JS_PRM_Msg_ReclassifyNoSelecteTermLevel);
                return false;
            }
            this.props.isCheckedAll ? 
                this.sendRunJobReclassifyRequest(selectedTermItem) : 
                this.sendReclassifyRequest(selectedTermItem);
        };
        this.dispatch("raReclassifyForm", 'onSave', callback);
        return false;
    }

    getSelectedTermParam = (selectedTermItem) => {
        let selectedTermParam = {};
        if(selectedTermItem){
            selectedTermParam = {
                TermInfo: {
                    Id: selectedTermItem.Id,
                    Name: selectedTermItem.Name,
                    UniqueId: selectedTermItem.UniqueId
                },
                Comment: selectedTermItem.Comment
            };
        }
        return selectedTermParam;
    }

    getRequestParam = (needRunJob, selectedTermItem) =>{
        let requestParam = this.getSelectedTermParam(selectedTermItem);
        if(needRunJob){
            requestParam.QueryDefintion = this.props.queryDefintion;
        }else{
            let selectedTableItems = this.props.checkedItems;
            requestParam.RecordIds = selectedTableItems.filter( item => 
                item.sourceFlag == Source.SharePoint).map(item => item.id);
            requestParam.OneDriveRecordIds = selectedTableItems.filter( item => 
                item.sourceFlag == Source.OneDrive).map(item => item.id);
            requestParam.TeamsRecordIds = selectedTableItems.filter(item =>
                item.sourceFlag == Source.Teams).map(item => item.id);
            requestParam.GoogleDriveRecordIds = selectedTableItems.filter(item =>
                item.sourceFlag == Source.GoogleDrive).map(item => item.id);
        }
        return requestParam;
    }

    handleError(response) {
        $$.loading(false);
        if (response.status == 403) {
            $$.messagedialog(true, {
                classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_Common_NoPermissionLicense,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => { $$.messagedialog(false); }
                    }
                ]
            });
        }
    }

    sendRunJobReclassifyRequest = (selectedTermItem) => {
        let url = `/api/MLManualApproval/StartReclassifyJob`;
        let option = {
            url: url,
            method: "POST",
            data: this.getRequestParam(true, selectedTermItem)
        };
        $$.loading(true);
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            $$.loading(false);
            let resultJson = JSON.parse(result);
            if (resultJson.MessageType == "0") {
                resultJson.Extension && ActionSuccessfulNeedJobToast();
            }else {
                showToast.error(resultJson.ErrorMessage);
                return;
            }
            this.props.callback();
            this.setState({ showPanel: false });
        });
    }

    sendReclassifyRequest = (selectedTermItem) => {
        let option = {
            url: `/api/MLManualApproval/ChangeTerm`,
            method: "POST",
            data: this.getRequestParam(false, selectedTermItem)
        };  
        $$.loading(true);
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((res) => {
            $$.loading(false);
            let result = JSON.parse(res);
            if (result.MessageType == 1) {
                showToast.error(result.ErrorMessage);
                return;
            } 
            this.props.callback(result.Extension);
            this.setState({ showPanel: false });
        });
    }

    getPropsCheckedItems = () => {
        let propsCheckedItems = _.cloneDeep(this.props.checkedItems);
        if(propsCheckedItems.length == 1){
            propsCheckedItems[0].TermId = propsCheckedItems[0].predictTermId;
        }
        return propsCheckedItems;
    }

    render(){
        return <R.Panel
            header={RMResx.RM_JS_BCM_Explorer_ChangeTerm}
            size={664}
            status={{show: this.state.showPanel}}
            destroy={true}
            onHide={this.onHide}
        >
            <div id="reclassify-content">
                <ReclassifyForm
                    id="raReclassifyForm"
                    data={this.getPropsCheckedItems()}
                    hideRuleInfo
                    displayingPage={this.props.displayingPage}
                >
                </ReclassifyForm>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveReclassify} />
            </>
        </R.Panel>;
    }
}