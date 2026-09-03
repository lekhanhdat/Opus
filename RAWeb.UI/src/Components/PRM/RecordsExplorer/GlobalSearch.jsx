import HybridSearch from "../../Common/HybridSearch/HybridSearch";
import RouterUrls from "../../../Constants/RouterUrls";

export default class GlobalSearch extends R.Component {
    idAttr = true;
    componentCreate () {
        this.state = {};
    }

    render () {
        let HybridSearchComponent = HybridSearch;
        return <div id={this.props.id}>
            <HybridSearchComponent
                backUrl={RouterUrls.PRM_RecordsExplorer}
                history={this.props.history}
            />
        </div>;
    }
}