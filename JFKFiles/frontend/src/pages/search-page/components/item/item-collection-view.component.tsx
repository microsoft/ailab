import * as React from "react";
import { ItemComponent } from "./item.component";
import { ItemCollection, Item } from "../../view-model";
import { cnc, getUniqueStrings } from "../../../../util";

const style = require("./item-collection-view.style.scss");


interface ItemViewProps {
  items?: ItemCollection;
  listMode?: boolean;
  activeSearch?: string;
  targetWords: string[];
  onClick?: (item: Item) => void;
}

export class ItemCollectionViewComponent extends React.Component<ItemViewProps, {}> {
  public constructor(props) {
    super(props);
  }
  
  private injectHighlightWords = (targetWords: string[], highlightWords: string[]): string[] => {
    return getUniqueStrings([...targetWords, ...highlightWords]);
  }
  
  public render() {
    return (    
      <div className={cnc(style.container, this.props.listMode && style.containerList)}>
        { this.props.items ? 
          this.props.items.map((child, index) => (
            <ItemComponent
              item={child}
              listMode={this.props.listMode}
              activeSearch={this.props.activeSearch}
              targetWords={this.injectHighlightWords(this.props.targetWords, child.highlightWords)}
              onClick={this.props.onClick}
              key={index}
            />
          ))
        : null }
      </div>
    );
  }  
}
