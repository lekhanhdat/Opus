import PropTypes from 'prop-types';


class DetailList extends React.Component {
    constructor(props) {
        super(props);
    }

    renderChildren(node) {
        if (!node) {
            return null;
        } else if (node.type == DetailRow) {
            return React.cloneElement(node, {
                labelWidth: this.props.labelWidth
            });
        } else if (node.props && node.props.children) {
            return React.Children.map(node.props.children, (child, i) => {
                return this.renderChildren(child);
            });
        } else {
            return node;
        }
    }

    render() {
        let className = "ra-details";
        if(this.props.className) {
            className += " " + this.props.className;
        }
        return <div className={className}>
            {this.renderChildren(this)}
        </div>;
    }
}
DetailList.propTypes = {
    className: PropTypes.string,
    labelWidth: PropTypes.number
};

class DetailRow extends React.Component {
    constructor(props) {
        super(props);
    }

    renderChildren(node) {
        if (!node) {
            return null;
        } else if (node.type == DetailCell) {
            return React.cloneElement(node, {
                labelWidth: this.props.labelWidth
            });
        } else if (node.props && node.props.children) {
            return React.Children.map(node.props.children, (child, i) => {
                return this.renderChildren(child);
            });
        } else {
            return node;
        }
    }

    render() {
        return <React.Fragment>
            <div className="ra-row">
                {this.renderChildren(this)}
            </div>
            <div className="ra-row">
                <div className="ra-row-split"></div>
            </div>
        </React.Fragment>;
    }
}
DetailRow.propTypes = {
    labelWidth: PropTypes.number
};

class DetailCell extends React.Component {
    constructor(props) {
        super(props);
    }

    renderValue() {
        if(this.props.children) {
            return this.props.children;
        } else {
            return <span tabIndex="0" style={{ whiteSpace: 'pre-wrap' }}>{this.props.value}</span>;
        }
    }

    render() {
        let labelProps = {};
        let { labelWidth, label, require } = this.props;
        if(labelWidth) {
            labelProps.style = {width: labelWidth + "px" };
        }
        return <React.Fragment>
            <div className={"ra-label" + (require ? " require" : "")} {...labelProps}>
                <span tabIndex="0">{label}</span>
            </div>
            <div className="ra-value">
                {this.renderValue()}
            </div>
        </React.Fragment>;
    }
}
DetailCell.propTypes = {
    label: PropTypes.string,
    value: PropTypes.any
};
DetailCell.defaultProps = {
    label: "",
    value: ""
};

export { DetailList, DetailRow, DetailCell };