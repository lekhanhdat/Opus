import WorkFlow from './WorkFlow';
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import "../../../../Less/RDM/workFlow.less";

export default class CreateWorkflow extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {};
        this.currentWorkflowId = RM.Url.getParam(window.location.href, "id");
    }

    componentInit() {

    }

    render() {
        let workFlowLink = !this.currentWorkflowId ? SiteMapLinks.RDM_CreateWorkFlow : SiteMapLinks.RDM_EditWorkFlow;
        return <div>
            <div id='raWorkFlow'>
                <$g.SiteMap data={[SiteMapLinks.RDM_WorkFlowManagement, workFlowLink]}/>
                <WorkFlow optionType='setUpWorkFlow' workFlowId={this.currentWorkflowId} history={this.props.history}></WorkFlow>
            </div>
        </div>;
    }
}
