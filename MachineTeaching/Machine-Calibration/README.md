# Motion Calibration Demo
CNC machines cut metal with spinning tools. Friction reduces precision and periodically demands recalibration. An expert operator must travel to calibrate the machine, repeatedly turn the knobs and take measurements until the machine regains precision.

## LOCAL (CLI) GUIDE

### CLI INSTALLATION
1. Install the Bonsai CLI by following our [detailed CLI installation guide](https://docs.bons.ai/guides/cli-install-guide.html)

### CREATE YOUR BRAIN
1. Setup your BRAIN's local project folder.
       `bonsai create <your_brain_name>`
2. Run this command to install additional requirements for training your BRAIN.
       `pip3 install -r requirements.txt`


### HOW TO TRAIN YOUR BRAIN
1. Upload Inkling and simulation files to the Bonsai server with one command.
       `bonsai push`
2. Run this command to start training mode for your BRAIN.
       `bonsai train start`
   If you want to run this remotely on the Bonsai server use the `--remote` option.
       `bonsai train start --remote`
3. Connect the simulator for training.
       `python3 cnc_simulator.py` or `python cnc_simulator.py`
4. When training has hit a sufficient accuracy for prediction, which is dependent on your project, stop training your BRAIN.
       `bonsai train stop`

### GET PREDICTIONS
1. Run the simulator using predictions from your BRAIN. You can now see AI in action!
       `python cnc_simulator.py --predict`