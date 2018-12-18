"""
Model specification
"""
import theano
import theano.tensor as tensor
import numpy

from collections import OrderedDict
from theano.sandbox.rng_mrg import MRG_RandomStreams as RandomStreams

from skipthoughts_vectors.encdec_functs.utils import pref, ortho_weight, norm_weight, tanh, relu
from skipthoughts_vectors.encdec_functs.layers import get_layer, param_init_fflayer, fflayer, param_init_gru, gru_layer

def init_params(options, preemb=None):
    """
    Initialize all parameters
    """
    params = OrderedDict()

    # Word embedding
    if preemb == None:
        params['Wemb'] = norm_weight(options['n_words'], options['dim_word'])
    else:
        params['Wemb'] = preemb

    # init state
    params = get_layer('ff')[0](options, params, prefix='ff_state', nin=options['dimctx'], nout=options['dim'])

    # Decoder
    params = get_layer(options['decoder'])[0](options, params, prefix='decoder',
                                              nin=options['dim_word'], dim=options['dim'])

    # Output layer
    if options['doutput']:
        params = get_layer('ff')[0](options, params, prefix='ff_hid', nin=options['dim'], nout=options['dim_word'])
        params = get_layer('ff')[0](options, params, prefix='ff_logit', nin=options['dim_word'], nout=options['n_words'])
    else:
        params = get_layer('ff')[0](options, params, prefix='ff_logit', nin=options['dim'], nout=options['n_words'])

    return params

def build_model(tparams, options):
    """
    Computation graph for the model
    """

    trng = RandomStreams(1234)

    # description string: #words x #samples
    x = tensor.matrix('x', dtype='int64')
    mask = tensor.matrix('mask', dtype='float32')
    ctx = tensor.matrix('ctx', dtype='float32')

    n_timesteps = x.shape[0]
    n_samples = x.shape[1]

    # Index into the word embedding matrix, shift it forward in time
    emb = tparams['Wemb'][x.flatten()].reshape([n_timesteps, n_samples, options['dim_word']])
    emb_shifted = tensor.zeros_like(emb)
    emb_shifted = tensor.set_subtensor(emb_shifted[1:], emb[:-1])
    emb = emb_shifted

    # Init state
    init_state = get_layer('ff')[1](tparams, ctx, options, prefix='ff_state', activ='tanh')

    kwargs = {'mask' : mask}
    # Decoder
    proj = get_layer(options['decoder'])[1](tparams, emb, init_state, options,
                                            prefix='decoder', **kwargs)

    # Compute word probabilities
    if options['doutput']:
        hid = get_layer('ff')[1](tparams, proj[0], options, prefix='ff_hid', activ='tanh')
        logit = get_layer('ff')[1](tparams, hid, options, prefix='ff_logit', activ='linear')
    else:
        logit = get_layer('ff')[1](tparams, proj[0], options, prefix='ff_logit', activ='linear')
    logit_shp = logit.shape
    probs = tensor.nnet.softmax(logit.reshape([logit_shp[0]*logit_shp[1], logit_shp[2]]))

    # Cost
    x_flat = x.flatten()
    p_flat = probs.flatten()
    cost = -tensor.log(p_flat[tensor.arange(x_flat.shape[0])*probs.shape[1]+x_flat]+1e-8)
    cost = cost.reshape([x.shape[0], x.shape[1]])
    cost = (cost * mask).sum(0)
    cost = cost.sum()

    return trng, [x, mask, ctx], cost

def build_sampler(tparams, options, trng):
    """
    Forward sampling
    """
    ctx = tensor.matrix('ctx', dtype='float32')

    init_state = get_layer('ff')[1](tparams, ctx, options, prefix='ff_state', activ='tanh')
    f_init = theano.function([ctx], init_state, name='f_init', profile=False)

    # x: 1 x 1
    y = tensor.vector('y_sampler', dtype='int64')
    init_state = tensor.matrix('init_state', dtype='float32')

    # if it's the first word, emb should be all zero
    emb = tensor.switch(y[:,None] < 0, tensor.alloc(0., 1, tparams['Wemb'].shape[1]),
                        tparams['Wemb'][y])

    # decoder
    proj = get_layer(options['decoder'])[1](tparams, emb, init_state, options,
                                            prefix='decoder',
                                            mask=None,
                                            one_step=True)
    next_state = proj[0]

    # output
    if options['doutput']:
        hid = get_layer('ff')[1](tparams, next_state, options, prefix='ff_hid', activ='tanh')
        logit = get_layer('ff')[1](tparams, hid, options, prefix='ff_logit', activ='linear')
    else:
        logit = get_layer('ff')[1](tparams, next_state, options, prefix='ff_logit', activ='linear')
    next_probs = tensor.nnet.softmax(logit)
    next_sample = trng.multinomial(pvals=next_probs).argmax(1)

    # next word probability
    inps = [y, init_state]
    outs = [next_probs, next_sample, next_state]
    f_next = theano.function(inps, outs, name='f_next', profile=False)

    return f_init, f_next


