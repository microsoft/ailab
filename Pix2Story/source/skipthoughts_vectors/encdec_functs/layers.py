import theano
import theano.tensor as tensor
import numpy
from skipthoughts_vectors.encdec_functs.utils import pref, ortho_weight, norm_weight, tanh, linear


layers = {'ff': ('param_init_fflayer', 'fflayer'),
          'gru': ('param_init_gru', 'gru_layer'),
          }

def get_layer(name):
    """
    Return param init and feedforward functions for the given layer name
    """
    fns = layers[name]
    return (eval(fns[0]), eval(fns[1]))

# Feedforward layer
def param_init_fflayer(options, params, prefix='ff', nin=None, nout=None, ortho=True):
    """
    Affine transformation + point-wise nonlinearity
    """
    if nin == None:
        nin = options['dim_proj']
    if nout == None:
        nout = options['dim_proj']
    params[pref(prefix,'W')] = norm_weight(nin, nout, ortho=ortho)
    params[pref(prefix,'b')] = numpy.zeros((nout,)).astype('float32')

    return params

def fflayer(tparams, state_below, options, prefix='rconv', activ='lambda x: tensor.tanh(x)', **kwargs):
    """
    Feedforward pass
    """
    return eval(activ)(tensor.dot(state_below, tparams[pref(prefix,'W')])+tparams[pref(prefix,'b')])

# GRU layer
def param_init_gru(options, params, prefix='gru', nin=None, dim=None):
    """
    Gated Recurrent Unit (GRU)
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


def gru_layer(tparams, state_below, init_state, options, prefix='gru', one_step=False, **kwargs):
    """
    Feedforward pass through GRU
    """
    mask = kwargs.pop('mask')
    nsteps = state_below.shape[0]
    if state_below.ndim == 3:
        n_samples = state_below.shape[1]
    else:
        n_samples = 1

    dim = tparams[pref(prefix,'Ux')].shape[1]

    if init_state == None:
        init_state = tensor.alloc(0., n_samples, dim)

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

    if one_step:
        rval = _step(*(seqs+[init_state, tparams[pref(prefix, 'U')], tparams[pref(prefix, 'Ux')]]))
    else:
        rval, updates = theano.scan(_step,
                                    sequences=seqs,
                                    outputs_info = [init_state],
                                    non_sequences = [tparams[pref(prefix, 'U')],
                                                     tparams[pref(prefix, 'Ux')]],
                                    name=pref(prefix, '_layers'),
                                    n_steps=nsteps,
                                    profile=False,
                                    strict=True)
    rval = [rval]
    return rval
