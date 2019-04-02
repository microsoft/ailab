import * as d3 from "d3";
import { GraphResponse, GraphEdge, GraphNode } from "../../../../graph-api";
import { Theme } from "material-ui/styles";
import { createDragBehaviour } from "./graph-view.handlers";

const style = require("./graph-view.style.scss");


/**
 * Graph configuration parameters.
 */
const nodeRadius = 15;
const nodeSeparationFactor = 1;
const nodeChargeStrength = -950; // Being negative Charge = Repulsion.
const nodeChargeAccuracy = 0.4;

const colorizeNode = (theme: Theme) => (d, i) => 
  (i == 0) ? theme.palette.secondary.main : theme.palette.primary.main;

/**
 * Graph Utils.
 */

const getSvgBbox = (svg) => (svg.node() as Element).getBoundingClientRect();

const createKeepCoordInCanvas = (svgRect) => (n: number, dim: "X" | "Y", margin: number): number => {
  return (dim === "X") ? Math.max(margin, Math.min(svgRect.width - margin, n)) 
    : Math.max(margin, Math.min(svgRect.height - margin, n));
}

const navigateToSelectedTerm = (onGraphNodeDblClick: (string) => void) => (d) => {
  onGraphNodeDblClick(d.name);
}


/**
 * Graph definitions.
 */

const createSvg = (containerNodeId: string, theme: Theme) => {
  return d3.select(`#${containerNodeId}`)
    .append("svg")
      .style("flex", "1 1 auto")
      .style("font-family", theme.typography.fontFamily);
}

const createDefs = (svg) => {
  const defs = svg.append("defs");

  const arrow = defs
    .append("marker")
      .attr("id", "arrowhead")
      .attr("viewBox", "-0 -5 10 10")
      .attr("refX", 25)
      .attr("refY", 0)
      .attr("orient", "auto")
      .attr("markerWidth", 10)
      .attr("markerHeight", 10)
      .attr("xoverflow", "visible")
    .append("svg:path")
      .attr("class", style.arrow)      
      .attr("d", "M 0,-5 L 10 ,0 L 0,5");

  const dragFilter = defs
    .append("filter")
      .attr("id", "dragFilter")
    .append("feColorMatrix")
      .attr("type", "saturate")
      .attr("values", 10);
      
    return defs;
  
}

const createEdges = (svg, graphDescriptor: GraphResponse) => {
  return svg
    .append("g")
      .attr("class", "edges")
    .selectAll(style.edge)
    .data(graphDescriptor.edges)
    .enter().append("line")
      .attr("class", style.edge)
      .attr("id", function (d, i) { return "edge" + i })
      .attr("marker-end", "url(#arrowhead)");
}

const createNodes = (svg, graphDescriptor: GraphResponse, onGraphNodeDblClick: (string) => void, theme: Theme) => {
  const nodes = svg  
    .append("g")      
      .attr("class", "nodes")
    .selectAll("circle")
    .data(graphDescriptor.nodes)
    .enter().append("circle")
      .attr("class", style.node)
      .attr("node", d => d.name)
      .attr("r", nodeRadius)
      .style("fill", colorizeNode(theme))
      .on("dblclick", navigateToSelectedTerm(onGraphNodeDblClick));

  return nodes;
}

const createLabels = (svg, graphDescriptor: GraphResponse, onGraphNodeDblClick: (string) => void, theme: Theme) => {
  const ellipticalArc = `M${-6*nodeRadius},0 A${6*nodeRadius},${2*nodeRadius} 0, 0,0 ${6*nodeRadius},0`;
  
  const labelArcs = svg
    .append("g")
      .attr("class", style.labelarc)  
    .selectAll(style.labelarc)
    .data(graphDescriptor.nodes)
    .enter().append("path")
      .attr("id", (d, i) => `labelarc${i}`)
      .attr("d", ellipticalArc);

  const labelGroup = svg
    .append("g")
      .attr("class", "labels");
  
  const labelTexts = labelGroup
    .selectAll(style.label)
    .data(graphDescriptor.nodes)
    .enter()
    .append("text")
      .attr("class", style.label)
      .attr("node", d => d.name)
      .on("dblclick", navigateToSelectedTerm(onGraphNodeDblClick));
  
  const labelStrokes = labelTexts
    .append("textPath") 
      .attr("class", style.labelStroke)
      .attr("xlink:href", (d, i) => `#labelarc${i}`)
      .attr("startOffset", "50%")
      .text(d => d.name);

  const labelInners = labelTexts
    .append("textPath")
      .attr("class", style.labelInner)
      .attr("xlink:href", (d, i) => `#labelarc${i}`)
      .attr("startOffset", "50%")
      .text(d => d.name);
  
  return {labelStrokes, labelInners, labelArcs};
}


/**
 * Graph implementation.
 */

export const resetGraph = (containerNodeId: string) => {
  d3.select(`#${containerNodeId} > *`).remove();
};

export const loadGraph = (containerNodeId: string, graphDescriptor: GraphResponse, onGraphNodeDblClick: (string) => void, theme: Theme) => {
  resetGraph(containerNodeId);
  
  const svg = createSvg(containerNodeId, theme);
  const defs = createDefs(svg);
  const edges = createEdges(svg, graphDescriptor);
  const nodes = createNodes(svg, graphDescriptor, onGraphNodeDblClick, theme);
  const {labelStrokes, labelInners, labelArcs} = createLabels(svg, graphDescriptor, onGraphNodeDblClick, theme);

  const svgRect = getSvgBbox(svg);
  const nodeDistance = nodeSeparationFactor * Math.min(svgRect.width, svgRect.height) / 5;
  const keepCoordInCanvas = createKeepCoordInCanvas(svgRect);

  const ticked = () => {
    nodes
      .attr("cx", (d: any) => keepCoordInCanvas(d.x, "X", nodeRadius))
      .attr("cy", (d: any) => keepCoordInCanvas(d.y, "Y", nodeRadius));
    edges
      .attr("x1", (d: any) => keepCoordInCanvas(d.source.x, "X", nodeRadius))
      .attr("y1", (d: any) => keepCoordInCanvas(d.source.y, "Y", nodeRadius))
      .attr("x2", (d: any) => keepCoordInCanvas(d.target.x, "X", nodeRadius))
      .attr("y2", (d: any) => keepCoordInCanvas(d.target.y, "Y", nodeRadius));
    labelArcs
      .attr("transform", (d: any) => `translate(
          ${keepCoordInCanvas(d.x, "X", nodeRadius)},
          ${keepCoordInCanvas(d.y, "Y", nodeRadius)})`);
    labelStrokes
      .attr("x", (d: any) => keepCoordInCanvas(d.x, "X", nodeRadius))
      .attr("y", (d: any) => keepCoordInCanvas(d.y, "Y", nodeRadius));
    labelInners
      .attr("x", (d: any) => keepCoordInCanvas(d.x, "X", nodeRadius))
      .attr("y", (d: any) => keepCoordInCanvas(d.y, "Y", nodeRadius));
  }


  const simulation = d3.forceSimulation()
    .nodes(graphDescriptor.nodes)  
    .force("link", d3.forceLink(graphDescriptor.edges)
          .id((d: any) => d.index)
          .distance(nodeDistance))
    .force("charge", d3.forceManyBody()
          .strength(nodeChargeStrength)
          .theta(nodeChargeAccuracy)
          .distanceMax(2 * nodeDistance))
    .force("center", d3.forceCenter(svgRect.width / 2, svgRect.height / 2))
    .force("collide", d3.forceCollide(nodeRadius))
    .on("tick", ticked)    
    ;

  const dragBehaviour = createDragBehaviour(simulation);
  
  nodes
    .call(dragBehaviour);
  labelInners
    .call(dragBehaviour);
  labelStrokes
    .call(dragBehaviour);
};
