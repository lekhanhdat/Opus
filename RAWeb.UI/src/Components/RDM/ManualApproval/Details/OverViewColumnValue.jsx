import { SourceI18Ns,SourceIcons} from "../Constants/Source.js";
import { WrapperLinkUrl } from "../../../../Utilities/CommonUtil";

const OverviewColumnValue = ({column, value , type}) =>{

    let soruceIcon = "fia-connecter";
    let soruceValue = value;
    switch(type){
        case "source" : 
            if(SourceIcons.get(value)){
                soruceIcon = SourceIcons.get(value);
                soruceValue = SourceI18Ns.get(value);
            }
            return(
                <$g.DetailRow>
                    <$g.DetailCell label={column}>
                        <div>
                            <span className={`reco-manual-review-icon ${soruceIcon}`}>
                                <span className="path1"></span>
                                <span className="path2"></span>
                                <span className="path3"></span>
                                <span className="path4"></span>
                                <span className="path5"></span>
                                <span className="path6"></span>
                            </span>
                            <span className='ra-source-text'>
                                {soruceValue}
                            </span>
                        </div>
                    </$g.DetailCell>
                </$g.DetailRow>
            );
        case "link" : 
            return (
                <$g.DetailRow>
                    <$g.DetailCell label={column}>          
                        <div>
                            <a className="ra-link-a" tabIndex="0" href={WrapperLinkUrl(value)} target="_blank" rel='noreferrer noopener'>
                                {value}
                            </a>
                        </div>
                    </$g.DetailCell>
                </$g.DetailRow>
            );
        case "itemLink" : 
            return (
                <$g.DetailRow>
                    <$g.DetailCell label={column}>          
                        <div>
                            <a className="ra-link-a" tabIndex="0" href={value} target="_blank" rel='noreferrer noopener'>
                                {value}
                            </a>
                        </div>
                    </$g.DetailCell>
                </$g.DetailRow>
            );
        case "related" :
            return (
                <$g.DetailRow>
                    <$g.DetailCell label={column}>          
                        {
                            value.map((item, index) =>
                                <a
                                    key={index}
                                    href={item.Url.indexOf("Root/PRM/RecordsExplorer") != -1 ? item.Url :  item.Url + "?web=1"}
                                    data-tooltip
                                    aria-label={item.Url.indexOf("Root/PRM/RecordsExplorer") != -1 ? item.Name : item.Url}
                                    className="reco-manual-review-table-link"
                                    target="_blank" 
                                    rel='noreferrer noopener'
                                >
                                    {item.Name}
                                </a>
                            )
                        }
                    </$g.DetailCell>
                </$g.DetailRow>
            );
        default :
            return (
                <$g.DetailRow>
                    <$g.DetailCell 
                        label={column}
                        value={value}>
                    </$g.DetailCell>
                </$g.DetailRow>
            );
    }
};

export default OverviewColumnValue;