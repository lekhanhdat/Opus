const RequestDetailItem = ({name, value, clickFun}) => {
    if(name.endsWith(":")){
        name = name.slice(0, name.length-1);
    }
    return (<div className="ra-requestd-group">
        <div className='ra-requestd-title'>{name + ":"}</div>
        <div className='ra-requestd-value' onClick={clickFun}>{value}</div>
    </div>)
}

export default RequestDetailItem;