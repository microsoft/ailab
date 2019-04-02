import * as React from "react";
import { DialogComponent } from "./dialog.component";
import { AzureButtonComponent } from './azure-button.component';

interface State {
  isDialogOpen: boolean;
}

export class PlaceholderComponent extends React.PureComponent<{}, State> {
  constructor(props) {
    super(props);

    this.state = {
      isDialogOpen: false,
    };
  }

  render() {
    return (
      <>
        <DialogComponent
          open={this.state.isDialogOpen}
          onClose={this.handleClose}
        />
        <AzureButtonComponent
          onClick={this.handleClickOpen}
        />
      </>
    );
  }

  private handleClose = () => {
    this.setState({ isDialogOpen: false });
  }

  private handleClickOpen = () => {
    this.setState({ isDialogOpen: true });
  }
}
