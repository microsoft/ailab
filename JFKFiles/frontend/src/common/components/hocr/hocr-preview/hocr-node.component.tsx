import * as React from "react";
import { HocrPreviewStyleMap } from "./hocr-preview.style";
import { SvgRectComponent, SvgGroupComponent } from "./hocr-svg.component";
import { 
  WordComparator,
  getNodeId,
  getNodeOptions,
  resolveNodeEntity,
  composeId,
  bboxToPosSize,  
} from "../util/common-util";
import { cnc } from "../../../../util";
import { RectangleProps } from "./rectangleProps";


/**
 * HOCR Node
 * Generic component to graphically represent placeholders for the different entities parsed
 * from the original HOCR input. Given an HOCR input, we can have pages, areas, paragraphs,
 * lines or words, in that order, creating a tree of nodes. Branch nodes in the tree (area,
 * paragraph and lines) will be rendered as SVG groups (<g>), the leafs of the tree (words)
 * will be represented with a rectangle placeholder (<rect>). 
 */

export interface HocrNodeProps {
  node: Element;
  key?: number;
  wordCompare: WordComparator;
  idSuffix: string;
  renderOnlyTargetWords?: boolean;
  userStyle?: HocrPreviewStyleMap;
  onWordHover?: (rectangleProps: RectangleProps) => void;
}

interface HocrGroupProps extends HocrNodeProps {
  entity: string;
}

export const HocrNodeComponent: React.StatelessComponent<HocrNodeProps> = (props) => {
  const entity = resolveNodeEntity(props.node);
  if (!entity) return null;
  
  return (entity === "word") ? 
    <HocrWordComponent {...props}/> : 
    <HocrGroupComponent {...props} entity={entity}/>
}

const HocrWordComponent: React.StatelessComponent<HocrNodeProps> = (props) => {
  const isTarget = props.wordCompare && props.wordCompare(props.node.textContent);
  const shouldRenderSvg = (!props.renderOnlyTargetWords || (props.renderOnlyTargetWords && isTarget));
  
  return shouldRenderSvg ?
    <SvgRectComponent 
      node={props.node}
      className={cnc(props.userStyle["word"], isTarget && props.userStyle["target"])}
      idSuffix={props.idSuffix}
      onHover={props.onWordHover}
    />
  : null;
}

const HocrGroupComponent: React.StatelessComponent<HocrGroupProps> = (props) => {
  const shouldRenderSvg = !props.renderOnlyTargetWords;
  const childrenComponents = getNodeChildrenComponents(props);

  return shouldRenderSvg ? 
    <SvgGroupComponent className={props.userStyle[props.entity]}>
      <SvgRectComponent
        node={props.node}
        className={props.userStyle[props.entity]}
        idSuffix={props.idSuffix}
        onHover={null}
      />
      {childrenComponents}
    </SvgGroupComponent>
  : <>{childrenComponents}</>;
}

export const getNodeChildrenComponents = (props: HocrNodeProps) => {
  return (props.node.children && props.node.children.length) ? 
    Array.from(props.node.children).map((child, index) => 
      <HocrNodeComponent
        {...props}
        node={child}
        key={index}
        userStyle={props.userStyle}
      />
    ) : null;
}
