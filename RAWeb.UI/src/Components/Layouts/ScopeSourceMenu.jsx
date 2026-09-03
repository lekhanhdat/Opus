export default class ScopeSourceMenu extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            hasScopeSourceMenu: false,
            content: "",
        };
    }

    componentReceive(show, element) {
        this.setState({
            hasScopeSourceMenu: show,
            content: element,
        });
    }

    renderScopeSourceMenu = () => {
        if (!this.state.hasScopeSourceMenu) {
            return <></>;
        }

        return this.state.content;
    };

    render() {
        return (
            <div id="raScopeSource" className="bg-white">{this.renderScopeSourceMenu()}</div>
        );
    }
}
