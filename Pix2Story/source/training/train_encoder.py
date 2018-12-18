from skipthoughts_vectors.training import homogeneous_data
import pickle as pkl
from skipthoughts_vectors.training.train import trainer
import config
from preprocessing.read_book_data import read_data
import os
from skipthoughts_vectors.encdec_functs.vocab import build_dictionary, save_dictionary
from skipthoughts_vectors.training.tools import load_model
import numpy as np

def books_2_text(path=config.paths['books']):
    text = read_data(path)
    with open(config.paths['text'], 'wb') as fp:
        print ('saving')
        pkl.dump(text, fp)
    return text

def load_text():
    with open(config.paths['text'], 'rb') as f:
        print('loading text')
        text = pkl.load(f)
    return text

class EncoderTrainer(object):
    def __init__(self, **kwargs):

        if os.path.exists(config.paths['text']): 
            self.text = load_text()
        else:
            self.text = books_2_text()
        if not os.path.exists(config.paths['dictionary']):
                worddict, wordcount = build_dictionary(self.text)
                save_dictionary(worddict, wordcount, config.paths['dictionary'])
        if 'training_options' in kwargs:
            self.training_options = kwargs.pop('training_options')
        else:
            self.training_options = config.settings['encoder']

    def train(self):
        trainer(self.text, self.training_options)


    def generate_table(self):
        model = load_model()
        np.save(config.paths['sktables'] + 'table.npy', np.array(list(model['table'].values())))
        with open(config.paths['sktables'] +"dictionary.txt",'a') as f:
            for word in list(model['table'].keys()):
                f.write(word + '\n')



