import {Component} from "react";
import {bindEvents} from "../../../Utilities/CommonUtil";
import PropTypes from "prop-types";
import {Callout} from "./Callout";

class CalloutButton extends Component {
    constructor(props) {
        super(props);
        bindEvents(this, "onClick", "handleDocumentClick");
    }

    componentDidMount() {
        document.addEventListener("click", this.handleDocumentClick);
    }

    // 组件销毁监听
    componentWillUnmount() {
        document.removeEventListener("click", this.handleDocumentClick, false);
    }

    onClick(e) {
        if(this.props.onTrigger){
            this.props.onTrigger(!this.props.showCallout);
        }
        e.stopPropagation();
    }

    render() {
        return <div className='ra-calloutButton'>
            <div className='ra-calloutButton-btn' style={this.props.buttonPosition}>
                <R.Button {...this.props.buttonAttribute} onClick={this.onClick}/>
            </div>
            <Callout
                ref='Callout'
                showCallout={this.props.showCallout}
                popupPosition={this.props.popupPosition}
                arrowPosition={this.props.arrowPosition}
                onTrigger={this.props.onTrigger}
            >
                {this.props.children}
            </Callout>
        </div>;
    }
}

CalloutButton.propTypes = {
    buttonAttribute: PropTypes.object,
    buttonPosition: PropTypes.object,
};
CalloutButton.defaultProps = {
    buttonAttribute: {type: "bald", icon: "fia-actionmenu"},
    buttonPosition: {},
};
export {CalloutButton};