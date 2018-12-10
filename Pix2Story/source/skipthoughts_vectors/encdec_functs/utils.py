"""
Helper functions for skip-thoughts
"""
import theano
import theano.tensor as tensor
import numpy
import pickle as pkl
import os
from collections import OrderedDict
from collections import defaultdict
from skipthoughts_vectors.decoding.search import GenSample

def zipp(params, tparams):
    """
    Push parameters to Theano shared variables
    """
    for kk, vv in params.items():
        tparams[kk].set_value(vv)

def unzip(zipped):
    """
    Pull parameters from Theano shared variables
    """
    new_params = OrderedDict()
    for kk, vv in zipped.items():
        new_params[kk] = vv.get_value()
    return new_params

def itemlist(tparams):
    """
    Get the list of parameters. 
    Note that tparams must be OrderedDict
    """
    return [vv for kk, vv in tparams.items()]

def pref(pp, name):
    """
    Make prefix-appended name
    """
    return '%s_%s'%(pp, name)

def init_tparams(params, target=None):
    """
    Initialize Theano shared variables according to the initial parameters
    """
    if target is not None:
        tparams = OrderedDict()
        for kk, pp in params.items():
           tparams[kk] = theano.shared(params[kk], name=kk, target=target)
    else:
        tparams = OrderedDict()
        for kk, pp in params.items():
            tparams[kk] = theano.shared(params[kk], name=kk)
    return tparams

def load_params(path, params):
    """
    Load parameters
    """
    pp = numpy.load(path, encoding='latin1')
    for kk, vv in params.items():
        if kk not in pp:
            warnings.warn('%s is not in the archive'%kk)
            continue
        params[kk] = pp[kk]
    return params

def ortho_weight(ndim):
    """
    Orthogonal weight init, for recurrent layers
    """
    W = numpy.random.randn(ndim, ndim)
    u, s, v = numpy.linalg.svd(W)
    return u.astype('float32')

def norm_weight(nin,nout=None, scale=0.1, ortho=True):
    """
    Uniform initalization from [-scale, scale]
    If matrix is square and ortho=True, use ortho instead
    """
    if nout == None:
        nout = nin
    if nout == nin and ortho:
        W = ortho_weight(nin)
    else:
        W = numpy.random.uniform(low=-scale, high=scale, size=(nin, nout))
    return W.astype('float32')

def l2norm(x):
    """
    Compute L2 norm, row-wise
    """
    norm = tensor.sqrt(tensor.pow(x, 2).sum(1))
    x /= norm[:, None]
    return x

def xavier_weight(nin,nout=None):
    """
    Xavier init
    """
    if nout == None:
        nout = nin
    r = numpy.sqrt(6.) / numpy.sqrt(nin + nout)
    W = numpy.random.rand(nin, nout) * 2 * r - r
    return W.astype('float32')

def tanh(x):
    """
    Tanh activation function
    """
    return tensor.tanh(x)

def relu(x):
    """
    ReLU activation function
    """
    return x * (x > 0)

def linear(x):
    """
    Linear activation function
    """
    return x

def concatenate(tensor_list, axis=0):
    """
    Alternative implementation of `theano.tensor.concatenate`.
    """
    concat_size = sum(tt.shape[axis] for tt in tensor_list)

    output_shape = ()
    for k in range(axis):
        output_shape += (tensor_list[0].shape[k],)
    output_shape += (concat_size,)
    for k in range(axis + 1, tensor_list[0].ndim):
        output_shape += (tensor_list[0].shape[k],)

    out = tensor.zeros(output_shape)
    offset = 0
    for tt in tensor_list:
        indices = ()
        for k in range(axis):
            indices += (slice(None),)
        indices += (slice(offset, offset + tt.shape[axis]),)
        for k in range(axis + 1, tensor_list[0].ndim):
            indices += (slice(None),)

        out = tensor.set_subtensor(out[indices], tt)
        offset += tt.shape[axis]

    return out

def reload_opts(model_options):
    saveto = model_options['saveto']
    reload_ = model_options['reload_']
    if reload_ and os.path.exists(saveto):
        print ('reloading...' + saveto)
        with open('%s.pkl'%saveto, 'rb') as f:
            model_options = pkl.load(f)
    return model_options


def weight_decay(decay_c, tparams, cost):
    if decay_c > 0.:
        decay_c = theano.shared(numpy.float32(decay_c), name='decay_c')
        weight_decay = 0.
        for kk, vv in tparams.items():
            weight_decay += (vv ** 2).sum()
        weight_decay *= decay_c
        cost += weight_decay
    return cost


def pre_emb(embeddings,model_options, worddict):
    n_words = model_options['n_words']
    if embeddings != None:
        print ('Loading embeddings...')
        with open(embeddings, 'rb') as f:
            embed_map = pkl.load(f)
        dim_word = len(embed_map.values()[0])
        model_options['dim_word'] = dim_word
        preemb = norm_weight(n_words, dim_word)
        pz = defaultdict(lambda : 0)
        for w in embed_map.keys():
            pz[w] = 1
        for w in worddict.keys()[:n_words-2]:
            if pz[w] > 0:
                preemb[worddict[w]] = embed_map[w]
    else:
        preemb = None

    return preemb, model_options


def f_grad_clip(grad_clip,grads):
    if grad_clip > 0.:
        g2 = 0.
        for g in grads:
            g2 += (g**2).sum()
        new_grads = []
        for g in grads:
            new_grads.append(tensor.switch(g2 > (grad_clip**2),
                                           g / tensor.sqrt(g2) * grad_clip,
                                           g))
        grads = new_grads
    return grads


def check_disp(uidx, disp_freq, eidx, cost, ud):
     if numpy.mod(uidx, disp_freq) == 0:
            print ('Epoch ', eidx, 'Update ', uidx, 'Cost ', cost, 'UD ', ud)


def check_save(uidx, save_freq, tparams, saveto, model_options):
    if numpy.mod(uidx, save_freq) == 0:
        print ('Saving...')
        params = unzip(tparams)
        numpy.savez(saveto, history_errs=[], **params)
        pkl.dump(model_options, open('%s.pkl'%saveto, 'wb'))
        print ('Done')


def show_samples(list_xmc, trng, tparams, f_init, f_next, model_options, word_idict):  
        x_s = list_xmc[0]
        ctx_s = list_xmc[2]
        for jj in range(numpy.minimum(10, len(ctx_s))):
            kwargs = {'tparams' : tparams, 'f_init' : f_init, 'f_next' : f_next,
              'ctx' : ctx_s[jj].reshape(1, model_options['dimctx']), 'options' : model_options,
               'trng' : trng, 'k' : 1, 'maxlen' : model_options['maxlen_w'], 'argmax' : False,
                'use_unk' : False}
            sample, score = GenSample(**kwargs).gen_sample()
            print ('Truth ',jj,': ',)
            sent = ''
            for vv in x_s[:,jj]:
                if vv == 0:
                    break
                if vv in word_idict:
                    sent += word_idict[vv] + ' '
                else:
                    sent += 'UNK' + ' '
            print(sent)
            print_samples(sample, jj, word_idict)


def print_samples(sample, jj, word_idict):
    for kk, ss in enumerate([sample[0]]):
        print ('Sample (', kk,') ', jj, ': ')
        sent = ''
        for vv in ss:
            if vv == 0:
                break
            if vv in word_idict:
                sent += word_idict[vv] + ' '
            else:
                sent += 'UNK' + ' '
        print(sent)