"""
Model specification
"""
import theano
import theano.tensor as tensor
import numpy

from collections import OrderedDict
from theano.sandbox.rng_mrg import MRG_RandomStreams as RandomStreams

from skipthoughts_vectors.encdec_functs.utils import pref, ortho_weight, norm_weight, tanh
from skipthoughts_vectors.encdec_functs.layers import get_layer, param_init_fflayer, fflayer, param_init_gru


def init_params(options):
    """
    Initialize all parameters
    """
    params = OrderedDict()

    # Word embedding
    params['Wemb'] = norm_weight(options['n_words'], options['dim_word'])

    # Encoder
    params = get_layer(options['encoder'])[0](options, params, prefix='encoder',
                                              nin=options['dim_word'], dim=options['dim'])

    # Decoder: next sentence
    params = get_layer(options['decoder'])[0](options, params, prefix='decoder_f',
                                              nin=options['dim_word'], dim=options['dim'])
    # Decoder: previous sentence
    params = get_layer(options['decoder'])[0](options, params, prefix='decoder_b',
                                              nin=options['dim_word'], dim=options['dim'])

    # Output layer
    params = get_layer('ff')[0](options, params, prefix='ff_logit', nin=options['dim'], nout=options['n_words'])

    return params

def build_model(tparams, options):
    """
    Computation graph for the model
    """
    opt_ret = dict()

    trng = RandomStreams(1234)

    # description string: #words x #samples
    # x: current sentence
    # y: next sentence
    # z: previous sentence
    x = tensor.matrix('x', dtype='int64')
    x_mask = tensor.matrix('x_mask', dtype='float32')
    y = tensor.matrix('y', dtype='int64')
    y_mask = tensor.matrix('y_mask', dtype='float32')
    z = tensor.matrix('z', dtype='int64')
    z_mask = tensor.matrix('z_mask', dtype='float32')

    n_timesteps = x.shape[0]
    n_timesteps_f = y.shape[0]
    n_timesteps_b = z.shape[0]
    n_samples = x.shape[1]

    # Word embedding (source)
    emb = tparams['Wemb'][x.flatten()].reshape([n_timesteps, n_samples, options['dim_word']])

    # encoder
    proj = get_layer(options['encoder'])[1](tparams, emb, None, options,
                                            prefix='encoder',
                                            mask=x_mask)
    ctx = proj[0][-1]
    dec_ctx = ctx

    # Word embedding (ahead)
    embf = tparams['Wemb'][y.flatten()].reshape([n_timesteps_f, n_samples, options['dim_word']])
    embf_shifted = tensor.zeros_like(embf)
    embf_shifted = tensor.set_subtensor(embf_shifted[1:], embf[:-1])
    embf = embf_shifted

    # Word embedding (behind)
    embb = tparams['Wemb'][z.flatten()].reshape([n_timesteps_b, n_samples, options['dim_word']])
    embb_shifted = tensor.zeros_like(embb)
    embb_shifted = tensor.set_subtensor(embb_shifted[1:], embb[:-1])
    embb = embb_shifted

    # decoder (ahead)
    projf = get_layer(options['decoder'])[1](tparams, embf, dec_ctx, options,
                                             prefix='decoder_f',
                                             mask=y_mask)

    # decoder (behind)
    projb = get_layer(options['decoder'])[1](tparams, embb, dec_ctx, options,
                                             prefix='decoder_b',
                                             mask=z_mask)

    # compute word probabilities (ahead)
    logit = get_layer('ff')[1](tparams, projf[0], options, prefix='ff_logit', activ='linear')
    logit_shp = logit.shape
    probs = tensor.nnet.softmax(logit.reshape([logit_shp[0]*logit_shp[1], logit_shp[2]]))

    # cost (ahead)
    y_flat = y.flatten()
    y_flat_idx = tensor.arange(y_flat.shape[0]) * options['n_words'] + y_flat
    costf = -tensor.log(probs.flatten()[y_flat_idx]+1e-8)
    costf = costf.reshape([y.shape[0],y.shape[1]])
    costf = (costf * y_mask).sum(0)
    costf = costf.sum()

    # compute word probabilities (behind)
    logit = get_layer('ff')[1](tparams, projb[0], options, prefix='ff_logit', activ='linear')
    logit_shp = logit.shape
    probs = tensor.nnet.softmax(logit.reshape([logit_shp[0]*logit_shp[1], logit_shp[2]]))

    # cost (behind)
    z_flat = z.flatten()
    z_flat_idx = tensor.arange(z_flat.shape[0]) * options['n_words'] + z_flat
    costb = -tensor.log(probs.flatten()[z_flat_idx]+1e-8)
    costb = costb.reshape([z.shape[0],z.shape[1]])
    costb = (costb * z_mask).sum(0)
    costb = costb.sum()

    # total cost
    cost = costf + costb

    return trng, x, x_mask, y, y_mask, z, z_mask, opt_ret, cost

def build_encoder(tparams, options):
    """
    Computation graph, encoder only
    """

    trng = RandomStreams(1234)

    # description string: #words x #samples
    x = tensor.matrix('x', dtype='int64')
    x_mask = tensor.matrix('x_mask', dtype='float32')

    n_timesteps = x.shape[0]
    n_samples = x.shape[1]

    # word embedding (source)
    emb = tparams['Wemb'][x.flatten()].reshape([n_timesteps, n_samples, options['dim_word']])

    # encoder
    proj = get_layer(options['encoder'])[1](tparams, emb, None, options,
                                            prefix='encoder',
                                            mask=x_mask)
    ctx = proj[0][-1]

    return trng, x, x_mask, ctx, emb

def build_encoder_w2v(tparams, options):
    """
    Computation graph for encoder, given pre-trained word embeddings
    """

    trng = RandomStreams(1234)

    # word embedding (source)
    embedding = tensor.tensor3('embedding', dtype='float32')
    x_mask = tensor.matrix('x_mask', dtype='float32')

    # encoder
    proj = get_layer(options['encoder'])[1](tparams, embedding, None, options,
                                            prefix='encoder',
                                            mask=x_mask)
    ctx = proj[0][-1]

    return trng, embedding, x_mask, ctx

