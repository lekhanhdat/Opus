import PropTypes from 'prop-types';


class IconButton extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        let { disabled, iconClass, text, onClick, ...args } = this.props;
        return <React.Fragment>
            <button type="button" role="button" disabled={disabled} onClick={onClick}
                className="ra-iconbtn" tabIndex="0" aria-label={text} {...args}>
                <span className={iconClass}></span>
                <span className="ra-iconbtn-text">{text}</span>
            </button>
        </React.Fragment>;
    }
}

const propTypes = {
    disabled: PropTypes.bool,
    text: PropTypes.string,
    iconClass: PropTypes.string,
    onClick: PropTypes.func
};

const defaultProps = {
    disabled: false,
    text: '',
    iconClass: '',
    onClick: null
};

IconButton.propTypes = propTypes;
IconButton.defaultProps = defaultProps;

export { IconButton };