"""
Decoder
"""
import theano
import theano.tensor as tensor
from theano.sandbox.rng_mrg import MRG_RandomStreams as RandomStreams
import pickle as pkl
import numpy
from preprocessing.text_moderator import text_moderator
from skipthoughts_vectors.decoding.search import GenSample
from collections import OrderedDict
from skipthoughts_vectors.encdec_functs.layers import get_layer, param_init_fflayer, fflayer, param_init_gru, gru_layer
from skipthoughts_vectors.encdec_functs.utils import pref, init_tparams, load_params, tanh, linear, ortho_weight, norm_weight
from skipthoughts_vectors.decoding.model import init_params, build_sampler

def load_model(path_to_model, path_to_dictionary):
    """
    Load a trained model for decoding
    """
    # Load the worddict
    with open(path_to_dictionary, 'rb') as f:
        worddict = pkl.load(f)

    # Create inverted dictionary
    word_idict = dict()
    for kk, vv in worddict.items():
        word_idict[vv] = kk
    word_idict[0] = '<eos>'
    word_idict[1] = 'UNK'

    # Load model options
    with open('%s.pkl'%path_to_model, 'rb') as f:
        options = pkl.load(f)
    if 'doutput' not in options.keys():
        options['doutput'] = True

    # Load parameters
    params = init_params(options)
    params = load_params(path_to_model, params)
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

def run_sampler(st_gen, image_data, image_loc, dec, c, beam_width=1,  use_unk=False):
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
    #Sort beams by their NLL, return the best result
    lengths = numpy.array([len(s.split()) for s in text])
    if lengths[0] == 0:  # in case the model only predicts <eos>
        lengths = lengths[1:]
        score = score[1:]
        text = text[1:]
    sidx = numpy.argmin(score)
    passage = text[sidx]
    score = score[sidx]
    if text_moderator(passage.encode('utf-8')) or len(passage.split(' '))<15:
        passage = ''
        for t in text:
            if not text_moderator(t.encode('utf-8')) and len(t.split(' '))>15:
                    passage = t
                    break    
    if passage == '':
        if beam_width < 50:
            passage = st_gen.story(image_data=image_data,image_loc = image_loc,bw=50)
        else:
            passage = 'Story can not be generated'
    passage = check_text(passage)
    return passage

layers = {'ff': ('param_init_fflayer', 'fflayer'),
          'gru': ('param_init_gru', 'gru_layer')}


def check_text(passage):
    prev_word = ''
    to_del = []
    passage = passage.split(' ')
    for i in range(len(passage)):
        if prev_word == passage[i] and passage[i].isalpha():
            to_del.append(i)
        else:
            prev_word = passage[i] 
    for i in reversed(range(len(to_del))):
        del passage[to_del[i]]
    passage = ' '.join(passage)
    passage=list(passage)
    upper_flat = True   
    for i in range(len(passage)):
        if passage[i].isalpha() and upper_flat:
            passage[i] = passage[i].upper()
            upper_flat = False
        if passage[i] == '.':
            upper_flat = True     
    passage = check_pun(passage)
    return passage


def check_pun(passage):
    for i in reversed(range(len(passage))):
        if passage[i] == '.' or passage[i] == "!" or passage[i] == '?':
            break
        if passage[i].isalpha() or passage[i] == "'" or passage[i] == '"':
            passage.append('.')
            break  
    passage = ''.join(passage)
    return passage

