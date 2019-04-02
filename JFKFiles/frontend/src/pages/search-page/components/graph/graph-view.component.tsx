import * as React from "react";
import { loadGraph, resetGraph } from "./graph-view.business";
import { withTheme, WithTheme } from "material-ui/styles";
import {
  GraphApi,
  CreateGraphApi,
  GraphConfig,
  GraphResponse
} from "../../../../graph-api";
import { jfkServiceConfig } from "../../service/jfk";
import { cnc } from "../../../../util";

const style = require("./graph-view.style.scss");


interface GraphViewProps extends WithTheme {
  searchValue: string;
  graphConfig?: GraphConfig;
  onGraphNodeDblClick : (searchValue : string) => string;
  className?: string;
}

interface GraphViewState {
  graphApi: GraphApi;
  graphDescriptor: GraphResponse;
}

const containerId = "fdGraphId";

class GraphView extends React.Component<GraphViewProps, GraphViewState> {
  constructor(props) {
    super(props);
  
    this.state = {
      graphApi: CreateGraphApi(jfkServiceConfig.graphConfig),
      graphDescriptor: null,
    }
  }

  private fetchGraphDescriptor = async (searchValue: string) => {
    if (!this.state.graphApi || !searchValue) return Promise.resolve(null);

    try {
      const payload = {search: searchValue};
      return await this.state.graphApi.runQuery(payload);
    } catch (e) {
      throw e;
    }
  };

  private updateGraphDescriptor = (searchValue: string) => {
    this.fetchGraphDescriptor(searchValue)
      .then(graphDescriptor => this.setState({
        ...this.state,
        graphDescriptor, 
      }))
      .catch(e => console.log(e));
  }

  private updateGraphApiAndDescriptor = (graphConfig: GraphConfig, searchValue: string) => {
    this.setState({
      ...this.state,
      graphApi: CreateGraphApi(graphConfig || jfkServiceConfig.graphConfig),
    }, () => this.updateGraphDescriptor(searchValue));
  }

  public componentDidMount() {
    this.updateGraphApiAndDescriptor(this.props.graphConfig, this.props.searchValue);
  };

  public componentWillReceiveProps(nextProps: GraphViewProps) {
    if (this.props.searchValue != nextProps.searchValue) {
      this.updateGraphDescriptor(nextProps.searchValue);
    } else if (this.props.graphConfig != nextProps.graphConfig) {
      this.updateGraphApiAndDescriptor(nextProps.graphConfig, nextProps.searchValue);
    }
  }

  public shouldComponentUpdate(nextProps: GraphViewProps, nextState: GraphViewState) {
    return this.state.graphDescriptor != nextState.graphDescriptor
  }

  public componentDidUpdate(prevProps: GraphViewProps, prevState: GraphViewState) {
    if (this.state.graphDescriptor != prevState.graphDescriptor) {
      loadGraph(containerId, this.state.graphDescriptor, this.props.onGraphNodeDblClick, this.props.theme);
    }      
  }

  public componentWillUnmount() {
    resetGraph(containerId);
  }

  public render() {
    return (
      <div
        className={cnc(style.container, this.props.className)}
        id={containerId}
      >
      </div>
    );
  }
}

export const GraphViewComponent = withTheme()(GraphView);