import csv
import os
import sys
import glob

from bonsai_ai import Brain, Config, Simulator


class DrillSimulator(Simulator):
   
    ITERATION_LENGTH = 101
    episode_count = 0 
    iteration_count = 0 
    episode_terminal = False
    current_rowx = 0
    rows = []

    def episode_start(self, parameters=None):
        self.episode_count += 1

        print("**Start Episode**: " + str(self.rows[self.current_rowx]['episode'])) 
 
        if int(self.rows[self.current_rowx]['iteration']) == 0:
            episode_state = {
                'side_force':self.rows[self.current_rowx]['sideforce'],
                'inclination':self.rows[self.current_rowx]['inclination']
                }

        self.current_rowx+=1

        return episode_state

    def simulate(self, action):
        state = {
            'side_force':self.rows[self.current_rowx]['sideforce'],
            'inclination':self.rows[self.current_rowx]['inclination']
            }

        reward = float(self.rows[self.current_rowx]['reward'])
        iteration = self.rows[self.current_rowx]['iteration']

        #check when at the end of an iteration.
        done = (int(iteration)==self.ITERATION_LENGTH)

        print("Episode:" + self.rows[self.current_rowx]['episode'] + " " + "Iteration:" + iteration)

        # move the iteration pointer to the next row
        self.current_rowx+=1

        return state, reward, done

    def episode_finish(self):
        print("Episode Finished")
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
    print("Drill Simulator Starting. . .")

    config = Config()
    brain = Brain(config)

    sim = DrillSimulator(brain, "drill_simulator")
    sim.load_data()
        
    while sim.run():
        if sim.episode_terminal:
            break

    print('Drill Simulator Finished.')

if __name__ == '__main__':
    main()
