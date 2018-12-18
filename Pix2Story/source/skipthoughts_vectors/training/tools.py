"""
A selection of functions for extracting vectors
Encoder + vocab expansion
"""
import theano
import theano.tensor as tensor
from theano.sandbox.rng_mrg import MRG_RandomStreams as RandomStreams
from skipthoughts_vectors.encdec_functs.utils import pref, ortho_weight, norm_weight, tanh, linear
import pickle as pkl
import numpy
import nltk
from gensim.models import KeyedVectors
from collections import OrderedDict, defaultdict
from nltk.tokenize import word_tokenize
from scipy.linalg import norm
from gensim.models import Word2Vec as word2vec
from sklearn.linear_model import LinearRegression
import config
from skipthoughts_vectors.encdec_functs.utils import load_params, init_tparams
from skipthoughts_vectors.training.model import init_params, build_encoder, build_encoder_w2v


def load_model(embed_map=None):
    """
    Load all model components + apply vocab expansion
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
    with open('%s.pkl'%config.paths['skmodels'], 'rb') as f:
        options = pkl.load(f)

    # Load parameters
    print ('Loading model parameters...')
    params = init_params(options)
    params = load_params(config.paths['skmodels'], params)
    tparams = init_tparams(params)

    # Extractor functions
    print ('Compiling encoder...')
    trng = RandomStreams(1234)
    trng, x, x_mask, ctx, emb = build_encoder(tparams, options)
    f_enc = theano.function([x, x_mask], ctx, name='f_enc')
    f_emb = theano.function([x], emb, name='f_emb')
    trng, embedding, x_mask, ctxw2v = build_encoder_w2v(tparams, options)
    f_w2v = theano.function([embedding, x_mask], ctxw2v, name='f_w2v')

    # Load word2vec, if applicable
    if embed_map == None:
        print ('Loading word2vec embeddings...')
        embed_map = load_googlenews_vectors(config.paths['v_expansion'])

    # Lookup table using vocab expansion trick
    print ('Creating word lookup tables...')
    table = lookup_table(options, embed_map, worddict, word_idict, f_emb)

    # Store everything we need in a dictionary
    print ('Packing up...')
    model = {}
    model['options'] = options
    model['table'] = table
    model['f_w2v'] = f_w2v

    return model

def encode(model, text, use_norm=False, verbose=True, batch_size=128, use_eos=False):
    """
    Encode sentences in the list X. Each entry will return a vector
    """
    eos_norm = [use_eos, use_norm]
    # first, do preprocessing
    text = preprocess(text)
    # word dictionary and init
    d = defaultdict(lambda : 0)
    for w in model['table'].keys():
        d[w] = 1
    features = numpy.zeros((len(text), model['options']['dim']), dtype='float32')
    # length dictionary
    ds = defaultdict(list)
    captions = [s.split() for s in text]
    for i,s in enumerate(captions):
        if s != 0:
            ds[len(s)].append(i)
    # Get features. This encodes by length, in order to avoid wasting computation
    features = encode_by_length(ds, eos_norm, model, d, captions, batch_size, features)
    return features


def encode_by_length(ds, eos_norm, model, d, captions, batch_size, features):
    for k in ds.keys():
        numbatches = int(len(ds[k]) / batch_size + 1)
        for minibatch in range(numbatches):
            caps = ds[k][minibatch::numbatches]
            if eos_norm[0]:
                embedding = numpy.zeros((k+1, len(caps), model['options']['dim_word']), dtype='float32')
            else:
                embedding = numpy.zeros((k, len(caps), model['options']['dim_word']), dtype='float32')
            embedding, caption = generate_embedding(caps, d, model, eos_norm, embedding, captions)
            try:
                features = calculate_features(eos_norm, model, caption, caps, embedding, features)
            except: 
                print(caption)
    return features


def generate_embedding(caps, d, model, eos_norm, embedding, captions):
    for ind, c in enumerate(caps):
        caption = captions[c]
        for j in range(len(caption)):
            if d[caption[j]] > 0:
                embedding[j,ind] = model['table'][caption[j]]       
            else:
                embedding[j,ind] = model['table']['-']   
        if eos_norm[0]:
            embedding[-1,ind] = model['table']['<eos>']  
    return embedding, caption


def calculate_features(eos_norm, model, caption, caps, embedding, features):
    if eos_norm[0]:
        uff = model['f_w2v'](embedding, numpy.ones((len(caption)+1,len(caps)), dtype='float32'))
    else:
        uff = model['f_w2v'](embedding, numpy.ones((len(caption),len(caps)), dtype='float32'))
    if eos_norm[1]:
        uff = norm(uff)
    for ind, c in enumerate(caps):
        features[c] = uff[ind]
    return features


def normalize(uff):
    for j in range(len(uff)):
        uff[j] /= norm(uff[j])
    return uff


def preprocess(text):
    """
    Preprocess text for encoder
    """
    processed_text = []
    sent_detector = nltk.data.load('tokenizers/punkt/english.pickle')
    for t in text:
        sents = sent_detector.tokenize(t)
        result = ''
        for s in sents:
            tokens = word_tokenize(s)
            result += ' ' + ' '.join(tokens)
        processed_text.append(result)
    return processed_text


def load_googlenews_vectors(path_to_word2vec):
    """
    load the word2vec GoogleNews vectors
    """
    embed_map = KeyedVectors.load_word2vec_format(config.paths['v_expansion'], binary=True)
    return embed_map

def lookup_table(options, embed_map, worddict, word_idict, f_emb, use_norm=False):
    """
    Create a lookup table from linear mapping of word2vec into RNN word space
    """
    wordvecs = get_embeddings(options, word_idict, f_emb)
    clf = train_regressor(options, embed_map, wordvecs, worddict)
    table = apply_regressor(clf, embed_map, use_norm=use_norm)
    for i in range(options['n_words']):
        w = word_idict[i]
        table[w] = wordvecs[w]
        if use_norm:
            table[w] /= norm(table[w])
    return table

def get_embeddings(options, word_idict, f_emb, use_norm=False):
    """
    Extract the RNN embeddings from the model
    """
    d = OrderedDict()
    for i in range(options['n_words']):
        caption = [i]
        ff = f_emb(numpy.array(caption).reshape(1,1)).flatten()
        if use_norm:
            ff /= norm(ff)
        d[word_idict[i]] = ff
    return d

def train_regressor(options, embed_map, wordvecs, worddict):
    """
    Return regressor to map word2vec to RNN word space
    """
    # Gather all words from word2vec that appear in wordvecs
    d = defaultdict(lambda : 0)
    for w in embed_map.vocab.keys():
        d[w] = 1
    shared = OrderedDict()
    count = 0
    for w in list(worddict.keys())[:options['n_words']-2]: #
        if d[w] > 0:
            shared[w] = count
            count += 1
    # Get the vectors for all words in 'shared'
    w2v = numpy.zeros((len(shared), 300), dtype='float32') #300
    sg = numpy.zeros((len(shared),options['dim_word'] ), dtype='float32') #options['dim_word']
    for w in shared.keys():
        w2v[shared[w]] = embed_map[w]
        sg[shared[w]] = wordvecs[w]
    clf = LinearRegression()
    clf.fit(w2v, sg)
    return clf

def apply_regressor(clf, embed_map, use_norm=False):
    """
    Map words from word2vec into RNN word space
    """
    wordvecs = OrderedDict()
    for i, w in enumerate(embed_map.vocab.keys()):
        if '_' not in w:
            wordvecs[w] = clf.predict(embed_map[w].reshape(1,-1)).astype('float32')
            if use_norm:
                wordvecs[w] /= norm(wordvecs[w])
    return wordvecs

