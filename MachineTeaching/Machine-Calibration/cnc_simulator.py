import csv
import os
import sys
import glob

from bonsai_ai import Brain, Config, Simulator

class CncSimulator(Simulator):

    ITERATION_LENGTH = 150
    episode_count = 0 
    iteration_count = 0 
    step_count = 0
    episode_terminal = False
    current_rowx = 0
    rows = []

    def episode_start(self, parameters=None):
        print("**Start Episode**: "  + self.rows[self.current_rowx]['episode'])
       
        episode_state = {
            'error':self.rows[self.current_rowx]['error'],
            'time':self.rows[self.current_rowx]['time']
            }

        self.current_rowx+=1

        return episode_state

    def simulate(self, action):
        self.step_count +=1
        state = {
            'error':self.rows[self.current_rowx]['error'],
            'time':self.rows[self.current_rowx]['time']
            }

        reward = float(self.rows[self.current_rowx]['reward'])
        iteration = self.rows[self.current_rowx]['iteration']

        #we have multiple steps per iteration and only 1 episode. We can stop once the file is complete.
        done = self.current_rowx >= (len(self.rows)-1)

        print("Episode:" + self.rows[self.current_rowx]['episode'] + " " + "Iteration:" + iteration + " " +  "Step:" + str(self.step_count))

        # move the iteration pointer to the next row
        if done != True:
            self.current_rowx+=1
        
        #reset step count if iteration is finished
        if(self.step_count >= self.ITERATION_LENGTH):
            self.step_count = 0

        return state, reward, done

    def episode_finish(self):
        print("Episode Finished")
        self.episode_terminal = True
        return None
    
    def load_data(self):
        print("Loading CSV Data")

        dirname = os.path.dirname(os.path.abspath(__file__))
        os.chdir(dirname)
        filenames = [i for i in glob.glob('*.{}'.format('csv'))]

        for filename in filenames:
            print('Loading: ' + filename)
             # resolve the relative paths    
            data_file_path = os.path.join(dirname, filename)

            with open(data_file_path) as csv_file:
                csv_dict_reader = csv.DictReader(csv_file, delimiter=',')
                #read the csv file and dump it into an array
                for row in csv_dict_reader:
                    self.rows.append(row)
        
        print("Finished loading CSV Data - Row Count: " + str(len(self.rows)))

def main():
    print("CNC Simulator Starting. . .")
    
    config = Config()
    brain = Brain(config)

    sim = CncSimulator(brain, "cnc_simulator")
    sim.load_data()

    while sim.run():
        if sim.episode_terminal:
            break

    print('CNC Simulator Finished.')


if __name__ == '__main__':
    main()
