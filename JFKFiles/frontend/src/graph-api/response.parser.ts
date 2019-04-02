import {
  GraphResponseConfig,
  GraphResponse,
  GraphNode,
  GraphEdge,
} from "./response.model";
import { isArrayEmpty } from "../util";

/**
 * Parser for Response.
 * It will transform a raw JSON response from server to a Response object.
 */

const parseEdge = (rawEdge: any, config: GraphResponseConfig): GraphEdge => {
  return {
    source: parseInt(rawEdge[config.edgeSourceAccessor]),
    target: parseInt(rawEdge[config.edgeTargetAccessor]),
  }
}

const parseNode = (rawNode: any, config: GraphResponseConfig): GraphNode => {
  return {
    name: rawNode[config.nodeNameAccessor],
  }
}

export const parseResponse = async (response: Response, config: GraphResponseConfig
): Promise<GraphResponse> => {
  const jsonObject = await response.json();

  if (!response.ok) {
    console.debug(jsonObject);
    throw new Error(`${response.status} - ${response.statusText}
      Message: ${jsonObject.error.message}`);
  }

  return {
    edges: jsonObject[config.edgesAccessor].map(e => parseEdge(e, config)),
    nodes: jsonObject[config.nodesAccessor].map(n => parseNode(n, config)),
  };
};
