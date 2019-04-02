/**
 * Object that represents the RESPONSE and RESPONSE configuration parameters. 
 * These config parameters will help in parsing the raw API response (JSON)
 * to a RESPONSE object.
 */


export interface GraphEdge {
  source: number;
  target: number;
}

export interface GraphNode {
  name: string;
}

export interface GraphResponse {
  edges: GraphEdge[];
  nodes: GraphNode[];
}

export interface GraphResponseConfig {
  edgesAccessor: string;
  edgeSourceAccessor: string;
  edgeTargetAccessor: string;
  nodesAccessor: string;
  nodeNameAccessor: string;
}

export const defaultGraphResponseConfig: GraphResponseConfig = {
  edgesAccessor: "edges",
  edgeSourceAccessor: "source",
  edgeTargetAccessor: "target",
  nodesAccessor: "nodes",
  nodeNameAccessor: "name",
}
