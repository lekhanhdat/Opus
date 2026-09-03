import PropTypes from 'prop-types';


class Container extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        let {
            show,
            id,
            className,
            tabIndex,
            style,
            children
        } = this.props;

        if (!className) {
            className = "";
        }
        if (!show) {
            className += " none";
        }

        var attrs = {};
        if (id) {
            attrs.id = id;
        }
        if (className) {
            attrs.className = className;
        }
        if (tabIndex || tabIndex === 0) {
            attrs.tabIndex = tabIndex;
        }
        if (style) {
            attrs.style = style;
        }
        attrs.className = className;
        
        return (
            <div {...attrs}>
                {children}
            </div>
        );
    }
}

const propTypes = {
    show: PropTypes.bool,
    id: PropTypes.string,
    className: PropTypes.string,
    style: PropTypes.object,
    tabIndex: PropTypes.number
};

const defaultProps = {
    show: true
};

Container.propTypes = propTypes;
Container.defaultProps = defaultProps;

export { Container };