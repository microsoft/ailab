import csv
import os
import sys
import glob

from bonsai_ai import Brain, Config, Simulator

class HvacSimulator(Simulator):
    ENERGY_COST = 0.24155 # Average cost per KWh over a year
    ITERATION_LENGTH = 288

    episode_count = 0
    episode_terminal = False
    iteration_count = 0
    current_rowx = 0
    current_hour = 1
    rows = []

    def episode_start(self, parameters=None):
        print("**Start Episode** " + str(self.episode_count))
        
        if int(self.iteration_count) == 0:
            episode_state = {
                'energy_cost' :self.ENERGY_COST,
                'hour': self.current_hour,
                'outdoor_temperature':self.rows[self.current_rowx]['temp_extAir'],
                'occupancy':self.rows[self.current_rowx]['occupancy'],
                'air_recycled':self.rows[self.current_rowx]['QAir']
                }

        self.current_rowx+=1

        return episode_state

    def simulate(self, action):
        if self.iteration_count%12 == 0:
            self.current_hour+=1
            #make sure can't go above 24 hours
            if self.current_hour > 24:
                self.current_hour = 24

        state = {
            'energy_cost':self.ENERGY_COST,
            'hour': self.current_hour,
            'outdoor_temperature':self.rows[self.current_rowx]['temp_extAir'],
            'occupancy':self.rows[self.current_rowx]['occupancy'],
            'air_recycled':self.rows[self.current_rowx]['QAir']
            }
        reward = float(self.rows[self.current_rowx]['reward'])
        iteration = self.iteration_count

        #check when at the end of an iteration.
        done = (int(iteration)==self.ITERATION_LENGTH)

        print("Episode:" + str(self.episode_count) + " " + "Iteration:" + str(iteration))

        # move the iteration pointer to the next row
        self.current_rowx+=1

        return state, reward, done

    def episode_finish(self):
        print("Episode Finished")
        self.current_hour = 1 #reset the hour
        #check if we are at the end
        if self.current_rowx >= len(self.rows):
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
    print("HVAC Simulator Starting. . .")
    
    config = Config()
    brain = Brain(config)

    sim = HvacSimulator(brain, "hvac_simulator")
    sim.load_data()

    while sim.run():
        if sim.episode_terminal:
            break

    print('HVAC Simulator Finished.')


if __name__ == '__main__':
    main()
