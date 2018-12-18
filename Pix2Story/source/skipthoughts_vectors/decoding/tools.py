"""
A selection of functions for the decoder
Loading models, generating text
"""
import theano
import theano.tensor as tensor
from theano.sandbox.rng_mrg import MRG_RandomStreams as RandomStreams

import pickle as pkl
import numpy

from skipthoughts_vectors.encdec_functs.utils import load_params, init_tparams
from skipthoughts_vectors.decoding.model import init_params, build_sampler
from skipthoughts_vectors.decoding.search import GenSample
import config


def load_model():
    """
    Load a trained model for decoding
    """
    # Load the worddict
    print ('Loading dictionary...')
    with open(config.paths['dictionary'], 'rb') as f:
        worddict = pkl.load(f)

    # Create inverted dictionary
    print ('Creating inverted dictionary...')
    word_idict = dict()
    for kk, vv in worddict.items():
        word_idict[vv] = kk
    word_idict[0] = '<eos>'
    word_idict[1] = 'UNK'

    # Load model options
    print ('Loading model options...')
    with open('%s.pkl'%config.paths['skvmodels'], 'rb') as f:
        options = pkl.load(f)

    # Load parameters
    print ('Loading model parameters...')
    params = init_params(options)
    params = load_params(config.paths['skvmodels'], params)
    tparams = init_tparams(params)

    # Sampler.
    trng = RandomStreams(1234)
    f_init, f_next = build_sampler(tparams, options, trng)

    # Pack everything up
    dec = dict()
    dec['options'] = options
    dec['trng'] = trng
    dec['worddict'] = worddict
    dec['word_idict'] = word_idict
    dec['tparams'] = tparams
    dec['f_init'] = f_init
    dec['f_next'] = f_next
    return dec

def run_sampler(dec, c, beam_width=1, use_unk=False):
    """
    Generate text conditioned on c
    """
    kwargs = {'tparams' : dec['tparams'], 'f_init' : dec['f_init'], 'f_next' : dec['f_next'],
              'ctx' : c.reshape(1, dec['options']['dimctx']), 'options' : dec['options'],
               'trng' : dec['trng'], 'k' : beam_width, 'maxlen' : 1000, 'argmax' : False,
                'use_unk' : use_unk}
    sample, score = GenSample(**kwargs).gen_sample()
    text = []
    for c in sample:
        text.append(' '.join([dec['word_idict'][w] for w in c[:-1]]))
    return text


