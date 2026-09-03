import {Component} from "react";
import {bindEvents} from "../../../Utilities/CommonUtil";
import PropTypes from "prop-types";

let maxIdIndex = 0;
let shownCalloutId = 0;

class Callout extends Component {
    constructor(props) {
        super(props);
        this.currentId = ++maxIdIndex;
        if(this.props.showCallout) {
            shownCalloutId = this.currentId;
        }
        this.state = {
            shownCallout: false,
        };
        bindEvents(this, 'onClick', 'calloutClick');
    }

    UNSAFE_componentWillReceiveProps(nextProps, nextContext) {
        if(nextProps.showCallout) {
            shownCalloutId = this.currentId;
        }

        if (this.props.showCallout != nextProps.showCallout) {
            this.setState({
                shownCallout: nextProps.showCallout
            })
        }
    }

    onClick(e) {
        if (this.currentId == shownCalloutId) {
            e.nativeEvent.stopImmediatePropagation();
        }
    }

    //隐藏
    handleDocumentClick() {
        if (this.currentId == shownCalloutId){
            if(this.props.onTrigger){
                this.props.onTrigger(false);
            }
        }
    }

    calloutClick(e) {
        this.stopPropagation(e);
    }

    //阻止冒泡
    stopPropagation(e) {
        e.nativeEvent.stopImmediatePropagation();
    }

    render() {
        return <div
            ref={r => this.container = r}
            className={this.props.showCallout ? 'block ra-calloutButton-popup' : 'none'}
            onClick={this.props.onClick}>
            <div className={'block ra-calloutButton-content'}
                 style={this.props.popupPosition}>
                <div className='ra-calloutButton-popup-triangle' style={this.props.arrowPosition}></div>
                {this.props.children}
            </div>
        </div>
    }
}

Callout.propTypes = {
    popupPosition: PropTypes.object,
    arrowPosition: PropTypes.object,
    showCallout: PropTypes.bool,
    onClick: PropTypes.func
};
Callout.defaultProps = {
    popupPosition: {},
    arrowPosition: {},
    showCallout: false
};
export {Callout};