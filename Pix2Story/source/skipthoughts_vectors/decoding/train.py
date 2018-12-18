"""
Main trainer function
"""
import theano
import theano.tensor as tensor

import pickle as pkl
import numpy
import copy

import os
import warnings
import sys
import time

from skipthoughts_vectors.decoding import homogeneous_data

from theano.sandbox.rng_mrg import MRG_RandomStreams as RandomStreams
from collections import defaultdict

from skipthoughts_vectors.encdec_functs.utils import *
from skipthoughts_vectors.encdec_functs.layers import get_layer, param_init_fflayer, fflayer, param_init_gru, gru_layer
from skipthoughts_vectors.encdec_functs.optim import adam
from skipthoughts_vectors.decoding.model import init_params, build_model, build_sampler
from skipthoughts_vectors.encdec_functs.vocab import load_dictionary


# main trainer
def trainer(text_source, text_target, stmodel, training_settings):

    dimctx = training_settings['dimctx']
    dim_word = training_settings['dim_word']
    dim =  training_settings['dim']
    encoder =  training_settings['encoder']
    decoder =  training_settings['decoder']
    doutput = training_settings['doutput']
    max_epochs =  training_settings['max_epochs']
    disp_freq =  training_settings['disp_freq']
    decay_c =  training_settings['decay_c']
    grad_clip =  training_settings['grad_clip']
    n_words =  training_settings['n_words']
    maxlen_w =  training_settings['maxlen_w']
    optimizer =  training_settings['optimizer']
    batch_size =  training_settings['batch_size']
    saveto =  training_settings['saveto']
    dictionary =  training_settings['dictionary']
    embeddings = training_settings['embeddings']
    save_freq =  training_settings['save_freq']
    sample_freq = training_settings['sample_freq']
    reload_ =  training_settings['reload_']

    model_options = {}
    model_options['dimctx'] = dimctx
    model_options['dim_word'] = dim_word
    model_options['dim'] = dim
    model_options['encoder'] = encoder
    model_options['decoder'] = decoder
    model_options['doutput'] = doutput
    model_options['max_epochs'] = max_epochs
    model_options['dispFreq'] = disp_freq
    model_options['decay_c'] = decay_c
    model_options['grad_clip'] = grad_clip
    model_options['n_words'] = n_words
    model_options['maxlen_w'] = maxlen_w
    model_options['optimizer'] = optimizer
    model_options['batch_size'] = batch_size
    model_options['saveto'] = saveto
    model_options['dictionary'] = dictionary
    model_options['embeddings'] = embeddings
    model_options['saveFreq'] = save_freq
    model_options['sampleFreq'] = sample_freq
    model_options['reload_'] = reload_


    print (model_options)

    # reload options
    model_options = reload_opts(model_options)

    # load dictionary
    print ('Loading dictionary...')
    worddict = load_dictionary(dictionary)

    # Load pre-trained embeddings, if applicable

    preemb, model_options = pre_emb(embeddings, model_options, worddict)

    # Inverse dictionary
    word_idict = dict()
    for kk, vv in worddict.items():
        word_idict[vv] = kk
    word_idict[0] = '<eos>'
    word_idict[1] = 'UNK'
    print ('Building model')
    params = init_params(model_options, preemb=preemb)
    # reload parameters
    if reload_ and os.path.exists(saveto):
        params = load_params(saveto, params)

    tparams = init_tparams(params)

    trng, inps, cost = build_model(tparams, model_options)

    print ('Building sampler')
    f_init, f_next = build_sampler(tparams, model_options, trng)

    # before any regularizer
    print ('Building f_log_probs...')
    f_log_probs = theano.function(inps, cost, profile=False)
    print ('Done')

    # weight decay, if applicable
   
    cost = weight_decay(decay_c, tparams, cost)

    # after any regularizer
    print ('Building f_cost...')
    f_cost = theano.function(inps, cost, profile=False)
    print ('Done')

    print ('Done')
    print ('Building f_grad...')
    grads = tensor.grad(cost, wrt=itemlist(tparams))
    f_grad_norm = theano.function(inps, [(g**2).sum() for g in grads], profile=False)
    f_weight_norm = theano.function([], [(t**2).sum() for k,t in tparams.items()], profile=False)

    grads = f_grad_clip(grad_clip, grads)

    lr = tensor.scalar(name='lr')
    print ('Building optimizers...')
    # (compute gradients), (updates parameters)
    f_grad_shared, f_update = eval(optimizer)(lr, tparams, grads, inps, cost)

    print ('Optimization')

    # Each sentence in the minibatch have same length (for encoder)
    train_iter = homogeneous_data.HomogeneousData([text_source,text_target], batch_size=batch_size, maxlen=maxlen_w)
    uidx = 0
    lrate = 0.01
    for eidx in range(max_epochs):
        print ('Epoch ', eidx)
        for x, c in train_iter:
            uidx += 1
            x, mask, ctx = homogeneous_data.prepare_data(x, c, worddict, stmodel, maxlen=maxlen_w, n_words=n_words)
            if type(x) != numpy.ndarray:
                print ('Minibatch with zero sample under length ', maxlen_w)
                uidx -= 1
                continue
                
            ud_start = time.time()
            cost = f_grad_shared(x, mask, ctx)
            f_update(lrate)
            ud = time.time() - ud_start

            check_disp(uidx, disp_freq, eidx, cost, ud)
            check_save(uidx, save_freq, tparams, saveto, model_options)
            if numpy.mod(uidx, sample_freq) == 0:
                show_samples([x,mask,ctx],trng, tparams, f_init, f_next, model_options, word_idict)


