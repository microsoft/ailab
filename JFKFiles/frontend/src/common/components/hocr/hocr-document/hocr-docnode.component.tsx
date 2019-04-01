import * as React from "react";
import { WordComparator, resolveNodeEntity } from "../util/common-util";
import { HocrDocumentStyleMap } from "./hocr-document.style";
import { cnc } from "../../../../util";


/**
 * HOCR Document Nodes
 * A document node is a generic component that represents an entity or node from the original
 * HOCR input. This entity could be a page, an area, a paragraph, line or word. Each of them
 * would render differently. E.g: a page is represented as a card, an area provides a visual
 * indication, lines and words are hoverable, etc.
 */

interface HocrDocNodeProps {
  node: Element;
  index: number;
  wordCompare: WordComparator;
  userStyle: HocrDocumentStyleMap;
  onWordHover?: (wordId: string) => void;
  onPageHover?: (pageIndex: number) => void;
}

const HocrDocNodeComponent: React.StatelessComponent<HocrDocNodeProps> = (props) => {
  const nodeChildren = getDocNodeChildrenComponents(props);
  const entity = resolveNodeEntity(props.node);
  const isTarget = (entity === "word") && props.wordCompare && props.wordCompare(props.node.textContent);
  const className = cnc(props.userStyle[entity], isTarget && props.userStyle["target"]);
  const NodeType = resolveTypeFromEntity(entity);
  const nodeProps = {
      className,
      id: props.node.id,
      index: props.index,
    ...resolveEventHandlersFromEntity(entity, props),
  }

  const reactElement = <NodeType {...nodeProps}>{nodeChildren}</NodeType>

  return (NodeType === "span") ? 
    <>{reactElement}{" "}</>  // Add literal whitespace to span ending.
    : reactElement;
}

const resolveTypeFromEntity = (entity: string): string => {
  switch (entity) {
    case "word":
    case "line":
      return "span";
    case "paragraph":
      return "p";
    default:
      return "div";
  }
}

const resolveEventHandlersFromEntity = (entity: string, props: HocrDocNodeProps) => {
  if (entity === "word") {
    return {
      onMouseEnter: () => props.onWordHover && props.onWordHover(props.node.id),
      onMouseLeave: () => props.onWordHover && props.onWordHover(null),
    }
  } else if (entity === "page") {
    return {
      onMouseEnter: () => props.onPageHover && props.onPageHover(props.index),
    }
  }
  return null;
}

export const getDocNodeChildrenComponents = (props: HocrDocNodeProps) => {
  return (props.node.children && props.node.children.length) ? 
    Array.from(props.node.children).map((child, index) => 
      <HocrDocNodeComponent
        {...props}
        node={child}
        key={index}
        index={index}
      />
    ) : props.node.textContent;
  }
  