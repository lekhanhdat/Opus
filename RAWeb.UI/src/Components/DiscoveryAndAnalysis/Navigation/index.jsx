import _ from 'lodash';
import { DiscoveryDataSource, DiscoveryDataSourceI18ns } from "../Discovery/AnalysisConfigurator/Constants";
import "./index.less";

export default class DiscoveryAndAnalysisNavigation extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedDataSource: DiscoveryDataSource.None,
        };
        this.licenseCheckMap = new Map([
            [DiscoveryDataSource.Office365, () => RM.gData.hasDiscoveryLicense],
            [DiscoveryDataSource.Salesforce, () => RM.gData.hasDiscoverySalesforceLicense],
            [DiscoveryDataSource.Google, () => RM.gData.hasDiscoveryGoogleLicense],
            [DiscoveryDataSource.FileSystem, () => RM.gData.hasDiscoveryFileSystemLicense],
        ]);
        this.avaliableDataSources = this.props.dataSources.filter((item) => this.licenseCheckMap.get(item)());
    }

    onNavigationClick = (dataSource) => {
        if(dataSource === this.state.selectedDataSource) {
            return;
        }        

        this.setState({
            selectedDataSource: dataSource,
        });
        if(this.props.redirect && this.props.history && this.props.redirect.need && this.props.redirect.url) {
            this.props.history.push({
                pathname: this.props.redirect.url,
                search: `?dataSource=${dataSource}`
            });
        }
        else {
            this.props.onChange && this.props.onChange(dataSource);
        }
    };

    onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    renderNavigations = () => {
        return (
            <div className="reco-discovery-navigation">
                <div className="reco-discovery-navigation-title">
                    {RMResx.RM_FA_Discovery_Common_SourceTitle}
                </div>
                <div className="reco-discovery-navigation-items">
                    {this.avaliableDataSources.map((item) => (
                        <div
                            key={item}
                            className={
                                this.state.selectedDataSource === item
                                    ? "reco-discovery-navigation-item-active"
                                    : "reco-discovery-navigation-item-inactive"
                            }
                            onKeyDown={this.onKeyDown}
                            onClick={() => this.onNavigationClick(item)}
                        >
                            {DiscoveryDataSourceI18ns.get(item)}
                        </div>
                    ))}
                </div>
            </div>
        );
    };

    componentDidMount() {
        const url = new URL(window.location.href);
        let dataSource = this.avaliableDataSources[0];
        if(url.searchParams.has('dataSource')) {
            dataSource = Number.parseInt(url.searchParams.get('dataSource'));
        }

        if (this.avaliableDataSources.indexOf(dataSource) < 0) {
            window.location.href = window.location.origin + "/ErrorPage/NoPermission";
        }

        this.setState({
            selectedDataSource: dataSource
        }, () => {
            this.dispatch("raScopeSource", this.avaliableDataSources.length > 1, this.avaliableDataSources.length > 1 ? this.renderNavigations() : <></>);
            this.props.onChange && this.props.onChange(dataSource);
        })
    }

    componentDidUpdate() {
        this.dispatch("raScopeSource", this.avaliableDataSources.length > 1, this.avaliableDataSources.length > 1 ? this.renderNavigations() : <></>);
    }

    componentDestroy() {
        this.dispatch("raScopeSource", false, <></>);
    }

    render() {
        return <></>;
    }
}
