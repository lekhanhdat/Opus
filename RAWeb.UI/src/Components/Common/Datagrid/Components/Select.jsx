import {Component} from 'react';
import PropTypes from 'prop-types';

class Select extends Component {
    constructor(props) {
        super(props);
        this.state = {};
        this.initBinding();
    }

    initBinding() {
        const eventsArr = ['onChange'];
        eventsArr.forEach((ev) => {
            this[ev] = this[ev].bind(this);
        });
    }

    //check方法
    onChange(agu) {
        this.props.onChange(agu);
    }

    //点击列
    render() {
        return <div className="ra-grid-select">
            <R.Checkbox
                name="select"
                checked={this.props.isChecked}
                onChange={this.onChange}
                separate={true}
            />
        </div>;

    }
}

Select.propTypes = {
    isChecked: PropTypes.bool
};
Select.defaultProps = {
    isChecked: false
};
Select.type = 'SelectAll';   //用于templateRow的判断
export {Select};
