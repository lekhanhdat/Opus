import ManageRelatedRecordNav from "./ManageRelatedRecordNav";
import ManageRelatedRecordDetail from "./ManageRelatedRecordDetail";
export default class ManageRelatedRecords extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            index: 0,
        };
    }

    render() {
        return <div id="raManageRelatedRecords">
            <ManageRelatedRecordNav relatedInfos={this.props.relatedInfos} />
            <ManageRelatedRecordDetail/>
        </div>;
    }
}  