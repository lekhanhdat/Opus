import PropTypes from 'prop-types';


class ValidationMsg extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const {
            show,
            children
        } = this.props;
        return <React.Fragment>
            {
                show &&
                <div className="ra-validation">
                    <div className="ra-validation-msg" tabIndex={-1} role='alert'>
                        {children}
                    </div>
                </div>
            }
        </React.Fragment>;
    }
}

const propTypes = {
    show: PropTypes.bool
};

const defaultProps = {
    show: false
};

ValidationMsg.propTypes = propTypes;
ValidationMsg.defaultProps = defaultProps;

export { ValidationMsg };