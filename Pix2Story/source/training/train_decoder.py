from skipthoughts_vectors.decoding.train import trainer
import pickle as pkl
import config
import os
from generation import skipthoughts
from training.train_encoder import load_text, books_2_text
from skipthoughts_vectors.encdec_functs.vocab import build_dictionary, save_dictionary

class DecoderTrainer(object):
    def __init__(self, **kwargs):
        if os.path.exists(config.paths['text']): 
            self.text = load_text()
        else:
            self.text = books_2_text()
        self.model = skipthoughts.load_model(config.paths['skmodels'],
                                            config.paths['sktables'])
        if not os.path.exists(config.paths['dictionary']):
                worddict, wordcount = build_dictionary(self.text)
                save_dictionary(worddict, wordcount, config.paths['dictionary'])

        if 'training_options' in kwargs:
            self.training_options = kwargs.pop('training_options')
        else:
            self.training_options = config.settings['decoder']

    def train(self):

            trainer(self.text, self.text, self.model, self.training_options)

      
    
        
