import PropTypes from 'prop-types';
import React from 'react';

const messageBarBgColor={
    info: "#CC00FF",
    warn: "#F7A10029",
    error: "#D01B1B",
    success: "#00A84E"
};

class MessageBar extends R.Component {
    componentCreate(){
        this.state = {
            show: this.props.show
        };
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.show != this.props.show) {
            this.setState({show: nextProps.show});
        }
    }

    onClose = () =>{
        this.setState({ show: false });
    }

    renderContent(){
        let { children, type, hasClose } = this.props;
        if(this.state.show){
            return <div className="ra-messagebar" style={{background: `${messageBarBgColor[type]}`}}>
                {children}
                {hasClose && <span onClick={this.onClose} className="ra-messagebar-close fia-close"></span>}
            </div>;
        }
    }

    render() {
        return <React.Fragment>
            {this.renderContent()}
        </React.Fragment>;

    }
}

MessageBar.propTypes = {
    type: PropTypes.string,
    hasClose: PropTypes.bool
};
MessageBar.defaultProps = {
    type: "warn",
    hasClose: false
};

export { MessageBar };
