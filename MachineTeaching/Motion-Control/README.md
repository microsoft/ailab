# Motion Control Demo
One area of Autonomous Systems is Motion Control. Typically, when you think of controlling the motion of things using AI you think of drones and vehicles, but in fact there are many other types of motion control such as Horizontal Oil Drilling. In Horizontal Oil Drilling you actually “drive” or “fly” the drill head underground in any direction. The scenario is not unlike flying a drone in the air, it’s just in this case flying in the 3D space under ground. Today the process starts with an expert defining a drill plan. The drill plan is a 3d map of where the oil repositories are located. Next, the drill operator will use that plan to fly the drill head manually using a game controller, like an XBOX controller. The goal is to drill as fast and as precisely according to the plan. In this demo you will see how machine teaching and reinforcement learning can greatly increase the speed and precision of the drill run.


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
       `python3 drill_simulator.py` or `python drill_simulator.py`
4. When training has hit a sufficient accuracy for prediction, which is dependent on your project, stop training your BRAIN.
       `bonsai train stop`

### GET PREDICTIONS
1. Run the simulator using predictions from your BRAIN. You can now see AI in action!
       `python drill_simulator.py --predict`