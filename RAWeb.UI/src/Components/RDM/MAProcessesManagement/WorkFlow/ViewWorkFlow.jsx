import WorkFlow from './WorkFlow';
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import "../../../../Less/RDM/workFlow.less";

export default class ViewWorkFlow extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {};
        this.currentWorkflowId = RM.Url.getParam(window.location.href, "id");
    }

    componentInit() {

    }

    render() {
        return <div>
            <div id='raWorkFlow'>
                <$g.SiteMap data={[SiteMapLinks.RDM_WorkFlowManagement, SiteMapLinks.RDM_ViewWorkFlow]}/>
                <WorkFlow optionType='viewDetail' workFlowId={this.currentWorkflowId} history={this.props.history}></WorkFlow>
            </div>
        </div>;
    }
}
