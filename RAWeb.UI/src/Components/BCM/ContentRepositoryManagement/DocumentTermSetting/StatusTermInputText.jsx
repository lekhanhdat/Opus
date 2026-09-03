import React from "react";

export default class TermStatusForInputText extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            termRemoved: this.props.termRemoved,
            termDeprecated: this.props.termDeprecated
        };
    }

    componentDidMount() {
    }

    setStatus(isRemoved, isDeprecated) {
        this.setState({
            termRemoved: isRemoved,
            termDeprecated: isDeprecated
        });
    }
    
    clearStatus(){
        this.setStatus(false, false);
    }

    render() {
        return <div className="inline-block">
            {this.state.termRemoved && <span className="class-selector-retired">{RMResx.RM_JS_SPS_TermDelete}</span>}
            {!this.state.termRemoved && this.state.termDeprecated && <span className="class-selector-retired">{RMResx.RM_JS_SPS_IsTermRetired}</span>}
        </div>;
    }
}