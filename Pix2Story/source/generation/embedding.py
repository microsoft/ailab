"""
Joint image-sentence embedding space
"""
import theano
import theano.tensor as tensor
from theano.sandbox.rng_mrg import MRG_RandomStreams as RandomStreams

import pickle as pkl
import numpy
import nltk
from skipthoughts_vectors.encdec_functs.layers import get_layer, param_init_fflayer, fflayer, param_init_gru, gru_layer
from collections import OrderedDict, defaultdict
from scipy.linalg import norm
from skipthoughts_vectors.encdec_functs.utils import xavier_weight, l2norm, pref, init_tparams, load_params, linear, tanh, ortho_weight, norm_weight


def load_model(path_to_model):
    """
    Load all model components
    """
    # Load the worddict
    with open('%s.dictionary.pkl'%path_to_model, 'rb') as f:
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

    # Load parameters
    params = init_params(options)
    params = load_params(path_to_model, params)
    tparams = init_tparams(params)

    # Extractor functions
    trng = RandomStreams(1234)
    trng, [x, x_mask], sentences = build_sentence_encoder(tparams, options)
    f_senc = theano.function([x, x_mask], sentences, name='f_senc')

    trng, [im], images = build_image_encoder(tparams, options)
    f_ienc = theano.function([im], images, name='f_ienc')

    # Store everything we need in a dictionary
    model = {}
    model['options'] = options
    model['worddict'] = worddict
    model['word_idict'] = word_idict
    model['f_senc'] = f_senc
    model['f_ienc'] = f_ienc
    return model

def encode_sentences(model, text, verbose=False, batch_size=128):
    """
    Encode sentences into the joint embedding space
    """
    features = numpy.zeros((len(text), model['options']['dim']), dtype='float32')

    # length dictionary
    ds = defaultdict(list)
    captions = [s.split() for s in text]
    for i,s in enumerate(captions):
        ds[len(s)].append(i)

    # quick check if a word is in the dictionary
    d = defaultdict(lambda : 0)
    for w in model['worddict'].keys():
        d[w] = 1

    # Get features. This encodes by length, in order to avoid wasting computation
    return same_length_encoding(ds,d,captions,model,batch_size,features)


def same_length_encoding(ds,d,captions,model,batch_size,features):
    for k in ds.keys():
        numbatches = int(len(ds[k]) / batch_size + 1)
        for minibatch in range(numbatches):
            caps = ds[k][minibatch::numbatches]
            caption = [captions[c] for c in caps]
            seqs = []
            for i, cc in enumerate(caption):
                seqs.append([model['worddict'][w] if d[w] > 0 and model['worddict'][w] < model['options']['n_words'] else 1 for w in cc])
            x = numpy.zeros((k+1, len(caption))).astype('int64')
            x_mask = numpy.zeros((k+1, len(caption))).astype('float32')
            for idx, s in enumerate(seqs):
                x[:k,idx] = s
                x_mask[:k+1,idx] = 1.
            ff = model['f_senc'](x, x_mask)
            features = fill_features(ff,caps,features)
    return features


def fill_features(ff,caps,features):
    for ind, c in enumerate(caps):
            features[c] = ff[ind]
    return features


def encode_images(model, im):
    """
    Encode images into the joint embedding space
    """
    images = model['f_ienc'](im)
    return images


layers = {'ff': ('param_init_fflayer', 'fflayer'),
          'gru': ('param_init_gru', 'gru_layer')}


def init_params(options):
    """
    Initialize all parameters
    """
    params = OrderedDict()

    # Word embedding
    params['Wemb'] = norm_weight(options['n_words'], options['dim_word'])

    # Sentence encoder
    params = get_layer(options['encoder'])[0](options, params, prefix='encoder',
                                              nin=options['dim_word'], dim=options['dim'])

    # Image encoder
    params = get_layer('ff')[0](options, params, prefix='ff_image', nin=options['dim_image'], nout=options['dim'])

    return params

def build_sentence_encoder(tparams, options):
    """
    Encoder only, for sentences
    """

    trng = RandomStreams(1234)

    # description string: #words x #samples
    x = tensor.matrix('x', dtype='int64')
    mask = tensor.matrix('x_mask', dtype='float32')

    n_timesteps = x.shape[0]
    n_samples = x.shape[1]

    # Word embedding
    emb = tparams['Wemb'][x.flatten()].reshape([n_timesteps, n_samples, options['dim_word']])

    kwargs = {'mask' : mask}
    # Encode sentences
    proj = get_layer(options['encoder'])[1](tparams, emb, None, options,
                                            prefix='encoder', **kwargs)
    sents = proj[0][-1]
    sents = l2norm(sents)

    return trng, [x, mask], sents

def build_image_encoder(tparams, options):
    """
    Encoder only, for images
    """

    trng = RandomStreams(1234)

    # image features
    im = tensor.matrix('im', dtype='float32')

    # Encode images
    images = get_layer('ff')[1](tparams, im, options, prefix='ff_image', activ='linear')
    images = l2norm(images)

    return trng, [im], images





