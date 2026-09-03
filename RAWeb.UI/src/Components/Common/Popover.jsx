import PropTypes from 'prop-types';
import StringUtil from '../../Utilities/StringUtil';
class Popover extends R.Component {
    render() {
        let { width, height, icon, classify, position, style } = this.props;
        let id = StringUtil.newGuid();
        return <span className="ra-popover" style={style}>
            <R.Popover
                width={width}
                height={height}
                classify={classify}
                position={position}
            >
                <span className={icon} tabIndex={0} aria-label="infos" aria-describedby={"aria-popover-content-" + id} />
                <span id={"aria-popover-content-" + id}>{this.props.children}</span>
            </R.Popover>
        </span>;
    }
}

Popover.propTypes = {
    icon: PropTypes.string,
    classify: PropTypes.string
};
Popover.defaultProps = {
    icon: "fia-status-info",
    classify: "gray" 
};

export { Popover };
