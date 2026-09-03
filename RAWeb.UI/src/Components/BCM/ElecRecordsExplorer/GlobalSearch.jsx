import HybridSearch from "../../Common/HybridSearch/HybridSearch";

export default class GlobalSearch extends R.Component {
    idAttr = true;
    componentCreate () {
        this.state = {};
    }

    render () {
        return <div id={this.props.id}>
            <HybridSearch
                history={this.props.history}
            />
        </div>;
    }
}