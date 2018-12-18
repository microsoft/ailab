'''
Skip-thought vectors
'''
import os

import theano
import theano.tensor as tensor

import pickle as pkl
import numpy
import copy
import nltk
import config
from collections import OrderedDict, defaultdict
from scipy.linalg import norm
from nltk.tokenize import word_tokenize
from skipthoughts_vectors.training.tools import encode, preprocess
from skipthoughts_vectors.encdec_functs.utils import pref, init_tparams, load_params, ortho_weight, norm_weight
profile = False

layers = {'gru': ('param_init_gru', 'gru_layer')}


def load_model(path_to_model,path_to_tables):
    """
    Load the model with saved tables
    """
    # Load model options
    print ('Loading model parameters...')
    with open('%s.pkl'%path_to_model, 'rb') as f:
        options = pkl.load(f)
    
    # Load parameters
    params = init_params(options)
    params = load_params(path_to_model, params)
    tparams = init_tparams(params)

    # Extractor functions
    print ('Compiling encoders...')
    embedding, x_mask, ctxw2v = build_encoder(tparams, options)
    f_w2v = theano.function([embedding, x_mask], ctxw2v, name='f_w2v')

    # Tables
    print ('Loading tables...')
    table = load_tables() 

    # Store everything we need in a dictionary
    print ('Packing up...')
    model = {}
    model['options'] = options
    model['table'] = table
    model['f_w2v'] = f_w2v

    return model


def load_tables():
    """
    Load the tables
    """
    words = []
    table = numpy.load(config.paths['sktables'] + 'table.npy',encoding='latin1')
    f = open(config.paths['sktables'] + 'dictionary.txt', 'rb')
    for line in f:
        words.append(line.decode('utf-8').strip())
    f.close()
    table = OrderedDict(zip(words, table))
    return table 

class Encoder(object):
    """
    Sentence encoder.
    """
    def __init__(self, model):
      self._model = model

    def encode(self, text, use_norm=False, verbose=True, batch_size=128, use_eos=False):
      """
      Encode sentences in the list X. Each entry will return a vector
      """
      return encode(self._model, text, use_norm, verbose, batch_size, use_eos)


def word_features(table):
    """
    Extract word features into a normalized matrix
    """
    features = numpy.zeros((len(table), 620), dtype='float32')
    keys = table.keys()
    for i in range(len(table)):
        f = table[keys[i]]
        features[i] = f / norm(f)
    return features


def get_layer(name):
    fns = layers[name]
    return (eval(fns[0]), eval(fns[1]))


def init_params(options):
    """
    initialize all parameters needed for the encoder
    """
    params = OrderedDict()

    # embedding
    params['Wemb'] = norm_weight(options['n_words'], options['dim_word'])

    # encoder: GRU
    params = get_layer(options['encoder'])[0](options, params, prefix='encoder',
                                              nin=options['dim_word'], dim=options['dim'])
    return params


def build_encoder(tparams, options):
    """
    build an encoder, given pre-computed word embeddings
    """
    # word embedding (source)
    embedding = tensor.tensor3('embedding', dtype='float32',)
    x_mask = tensor.matrix('x_mask', dtype='float32')

    # encoder
    proj = get_layer(options['encoder'])[1](tparams, embedding, options,
                                            prefix='encoder',
                                            mask=x_mask)
    ctx = proj[0][-1]

    return embedding, x_mask, ctx



def param_init_gru(options, params, prefix='gru', nin=None, dim=None):
    """
    parameter init for GRU
    """
    if nin == None:
        nin = options['dim_proj']
    if dim == None:
        dim = options['dim_proj']
    w = numpy.concatenate([norm_weight(nin,dim),
                           norm_weight(nin,dim)], axis=1)
    params[pref(prefix,'W')] = w
    params[pref(prefix,'b')] = numpy.zeros((2 * dim,)).astype('float32')
    u = numpy.concatenate([ortho_weight(dim),
                           ortho_weight(dim)], axis=1)
    params[pref(prefix,'U')] = u

    wx = norm_weight(nin, dim)
    params[pref(prefix,'Wx')] = wx
    ux = ortho_weight(dim)
    params[pref(prefix,'Ux')] = ux
    params[pref(prefix,'bx')] = numpy.zeros((dim,)).astype('float32')

    return params


def gru_layer(tparams, state_below, options, prefix='gru', mask=None, **kwargs):
    """
    Forward pass through GRU layer
    """
    nsteps = state_below.shape[0]
    if state_below.ndim == 3:
        n_samples = state_below.shape[1]
    else:
        n_samples = 1

    dim = tparams[pref(prefix,'Ux')].shape[1]

    if mask == None:
        mask = tensor.alloc(1., state_below.shape[0], 1)

    def _slice(_x, n, dim):
        if _x.ndim == 3:
            return _x[:, :, n*dim:(n+1)*dim]
        return _x[:, n*dim:(n+1)*dim]

    state_below_ = tensor.dot(state_below, tparams[pref(prefix, 'W')]) + tparams[pref(prefix, 'b')]
    state_belowx = tensor.dot(state_below, tparams[pref(prefix, 'Wx')]) + tparams[pref(prefix, 'bx')]
    u = tparams[pref(prefix, 'U')]
    ux = tparams[pref(prefix, 'Ux')]

    def _step_slice(m_, x_, xx_, h_, u, ux):
        preact = tensor.dot(h_, u)
        preact += x_

        r = tensor.nnet.sigmoid(_slice(preact, 0, dim))
        u = tensor.nnet.sigmoid(_slice(preact, 1, dim))

        preactx = tensor.dot(h_, ux)
        preactx = preactx * r
        preactx = preactx + xx_

        h = tensor.tanh(preactx)

        h = u * h_ + (1. - u) * h
        h = m_[:,None] * h + (1. - m_)[:,None] * h_

        return h

    seqs = [mask, state_below_, state_belowx]
    _step = _step_slice

    rval, updates = theano.scan(_step,
                                sequences=seqs,
                                outputs_info = [tensor.alloc(0., n_samples, dim)],
                                non_sequences = [tparams[pref(prefix, 'U')],
                                                 tparams[pref(prefix, 'Ux')]],
                                name=pref(prefix, '_layers'),
                                n_steps=nsteps,
                                profile=profile,
                                strict=True)
    rval = [rval]
    return rval