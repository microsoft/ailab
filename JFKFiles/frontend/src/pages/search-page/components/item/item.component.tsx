import * as React from "react"
import { Item } from "../../view-model";
import { Chevron } from "../../../../common/components/chevron";
import { HocrPreviewComponent } from "../../../../common/components/hocr";
import Card, { CardActions, CardContent, CardMedia } from "material-ui/Card";
import List, { ListItem, ListItemIcon, ListItemText} from 'material-ui/List';
import Collapse from "material-ui/transitions/Collapse";
import Typography from "material-ui/Typography";
import Chip from 'material-ui/Chip';
import StarIcon from "material-ui-icons/Star";
import { cnc } from "../../../../util";

const style = require("./item.style.scss");


interface ItemProps {
  item: Item;
  listMode?: boolean;
  activeSearch?: string;
  targetWords?: string[];
  onClick?: (item: Item) => void;
  simplePreview?: boolean;
}

interface State {
  expanded: boolean;
}

const handleOnClick = ({item, onClick}) => () => onClick? onClick(item) : null;

const ratingStars = (item: Item) => ((item.rating >= 1.0) ? 
  Array(Math.floor(item.rating)).fill(0).map((item, index) => (
    <StarIcon key={index} classes={{root: style.star}} color="secondary" />
  )) : null
);

const ItemMediaThumbnail: React.StatelessComponent<ItemProps> = ({ item, onClick }) => {
  return (
    item.thumbnail ? 
    <CardMedia className={style.media}
      component="img"
      src={item.thumbnail}        
      title={item.title}
      onClick={handleOnClick({ item, onClick })}
    /> : null
  );
}

const ItemMediaHocrPreview: React.StatelessComponent<ItemProps> = ({ item, activeSearch, targetWords, onClick }) => {
  const isPhoto = item.type && item.type.toLowerCase() === "photo";
  return (
    <div className={isPhoto ? style.mediaExtended : style.media}
     onClick={handleOnClick({ item, onClick })}
    >
      <HocrPreviewComponent
        hocr={item.metadata}
        pageIndex={item.demoInitialPage}
        zoomMode={isPhoto ? "original" : "page-width"}
        targetWords={targetWords}
        renderOnlyTargetWords={true}
        disabelScroll={true}
      />
    </div>
  );
}

const ItemMedia: React.StatelessComponent<ItemProps> = (
  { item, activeSearch, targetWords, onClick, simplePreview }) => {
  return (
    simplePreview ? 
      <ItemMediaThumbnail
        item={item}
        onClick={onClick}
      /> :
      <ItemMediaHocrPreview
        item={item}
        activeSearch={activeSearch}
        targetWords={targetWords}
        onClick={onClick}
      />
  );
}

const ItemCaption: React.StatelessComponent<ItemProps> = ({ item, onClick }) => {
  return (
    <CardContent 
      classes={{root: style.caption}}
      onClick={handleOnClick({ item, onClick })}
    >
      <Typography variant="headline" component="h2" color="inherit">
        {item.title} 
        <span className={style.subtitle}>
          {item.subtitle}
        </span>
      </Typography>        
      <Typography component="p" color="inherit">
        {item.excerpt}
      </Typography>
    </CardContent>
  );
}

const generateExtraFieldContent = (field: any) => {
  if (typeof field == "string") {
    return <ListItemText primary={field} />
  } else if (field instanceof Array) {
    return (
      <div className={style.tagContainer}>
        {field.map((tag, tagIndex) => 
          <Chip label={tag} key={tagIndex} classes={{root: style.tag}}/>
        )}
      </div>);
  } else {
    return null;
  }
}

const generateExtraField = (field: any, index: number) => (
  field ? (
    <ListItem key={index}>
      { generateExtraFieldContent(field) }
    </ListItem>
  ) : null
);

const ItemExtraFieldList: React.StatelessComponent<ItemProps> = ({ item }) => {
  if (item.extraFields) {
    return (
      <CardContent>
        <List>
          {
            item.extraFields.map((field, fieldIndex) => 
              generateExtraField(field, fieldIndex))
          }
        </List>
      </CardContent>
    );
  } else {
    return null;
  }
}

export class ItemComponent extends React.Component<ItemProps, State> {
  constructor(props) {
    super(props);

    this.state = {
      expanded: false,
    }
  }

  private toggleExpand = () => {
    this.setState({
      ...this.state,
      expanded: !this.state.expanded,
    });
  }
    
  public render() {
    const {item, activeSearch, targetWords, onClick } = this.props;

    return (
      <Card classes={{root: cnc(style.card, this.props.listMode && style.listMode)}}
        elevation={8}>
        <ItemMedia
          item={item}
          activeSearch={activeSearch}
          targetWords={targetWords}
          onClick={onClick}
        />
        <ItemCaption item={item} onClick={onClick} />
        <CardActions classes={{root: style.actions}}>
          <div className={style.rating}>
            {ratingStars(item)}
          </div>          
          <Chevron className={style.chevron}
            onClick={this.toggleExpand} expanded={this.state.expanded} />
        </CardActions>
        <Collapse 
          classes={{container: style.collapse}}
          in={this.state.expanded}
          timeout="auto"
          unmountOnExit
        >
          <ItemExtraFieldList item={item} />
        </Collapse>  
      </Card>
    );
  }  
}
