import PropTypes from 'prop-types';


class TopMessageBar extends React.Component {
    constructor(props) {
        super(props);
        this.onCloseClick = this.onCloseClick.bind(this);
    }

    onCloseClick(e) {
        if (this.props.didClose) {
            this.props.didClose(e);
        }
    }
    onCloseKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    render() {
        let {
            show,
            type,
            className,
            showClose,
            children
        } = this.props;
        let iconStyle = { backgroundPosition: "-16px 0px" };
        let barStyle = null;
        switch (type) {
            case 'info':
                iconStyle.backgroundPosition = "-16px 0";
                barStyle = { backgroundColor: "#ecf9ff", border: "1px solid #54abe8" };
                break;
            case 'error':
                iconStyle.backgroundPosition = "0 0";
                barStyle = { backgroundColor: "#ffecec", border: "1px solid #ff6f6f" };
                break;
            case 'warning':
                iconStyle.backgroundPosition = "-48px 0";
                barStyle = { backgroundColor: "#fff2e2", border: "1px solid #ff9a48" };
                break;
            case 'success':
                iconStyle.backgroundPosition = "-32px 0";
                barStyle = { backgroundColor: "#f0ffe7", border: "1px solid #77cc50" };
                break;
            default:
                barStyle = {};
                break;
        }
        barStyle.display = show ? "block" : "none";

        return <React.Fragment>
            <div className={"ra-topMessageBar " + className} style={barStyle}>
                <div className="ra-topMessageBar-icon" style={iconStyle}></div>
                <div className="ra-topMessageBar-message" tabIndex="0">
                    {children}
                </div>
                {showClose &&
                    <div className="ra-topMessageBar-close-border">
                        <div className="ra-topMessageBar-close" tabIndex="0" role="button" aria-label={RMResx.RM_JS_Common_Close}
                            onClick={this.onCloseClick} onKeyDown={this.onCloseKeyDown} ></div>
                    </div>
                }
            </div>
        </React.Fragment>;
    }
}

const propTypes = {
    show: PropTypes.bool
};

const defaultProps = {
    show: false,
    type: "info",
    className: "",
    showClose: false,
    didClose: null
};

TopMessageBar.propTypes = propTypes;
TopMessageBar.defaultProps = defaultProps;

export { TopMessageBar };