import numpy
import copy
import sys
from generation import skipthoughts


class HomogeneousData():

    def __init__(self, data, batch_size=128, maxlen=None):
        self.data = data
        self.batch_size = batch_size
        self.maxlen = maxlen

        self.prepare()
        self.reset()

    def prepare(self):
        self.caps = self.data[0]
        self.feats = self.data[1]

        # find the unique lengths
        self.lengths = [len(cc.split()) for cc in self.caps]
        self.len_unique = numpy.unique(self.lengths)
        # remove any overly long sentences
        if self.maxlen:
            self.len_unique = [ll for ll in self.len_unique if ll <= self.maxlen]

        # indices of unique lengths
        self.len_indices = dict()
        self.len_counts = dict()
        for ll in self.len_unique:
            self.len_indices[ll] = numpy.where(self.lengths == ll)[0]
            self.len_counts[ll] = len(self.len_indices[ll])

        # current counter
        self.len_curr_counts = copy.copy(self.len_counts)

    def reset(self):
        self.len_curr_counts = copy.copy(self.len_counts)
        self.len_unique = numpy.random.permutation(self.len_unique)
        self.len_indices_pos = dict()
        for ll in self.len_unique:
            self.len_indices_pos[ll] = 0
            self.len_indices[ll] = numpy.random.permutation(self.len_indices[ll])
        self.len_idx = -1

    def __next__(self):
        count = 0
        while True:
            self.len_idx = numpy.mod(self.len_idx+1, len(self.len_unique))
            if self.len_curr_counts[self.len_unique[self.len_idx]] > 0:
                break
            count += 1
            if count >= len(self.len_unique):
                break
        if count >= len(self.len_unique):
            self.reset()
            raise StopIteration()

        # get the batch size
        curr_batch_size = numpy.minimum(self.batch_size, self.len_curr_counts[self.len_unique[self.len_idx]])
        curr_pos = self.len_indices_pos[self.len_unique[self.len_idx]]
        # get the indices for the current batch
        curr_indices = self.len_indices[self.len_unique[self.len_idx]][curr_pos:curr_pos+curr_batch_size]
        self.len_indices_pos[self.len_unique[self.len_idx]] += curr_batch_size
        self.len_curr_counts[self.len_unique[self.len_idx]] -= curr_batch_size

        caps = [self.caps[ii] for ii in curr_indices]
        feats = [self.feats[ii] for ii in curr_indices]

        return caps, feats

    def __iter__(self):
        return self

def prepare_data(caps, features, worddict, model, maxlen=None, n_words=10000):
    """
    Put data into format useable by the model
    """
    seqs = []
    feat_list = []
    for i, cc in enumerate(caps):
        seqs.append([worddict[w] if worddict[w] < n_words else 1 for w in cc.split()])
        feat_list.append(features[i])

    lengths = [len(s) for s in seqs]

    if maxlen != None and numpy.max(lengths) >= maxlen:
        new_seqs = []
        new_feat_list = []
        new_lengths = []
        for l, s, y in zip(lengths, seqs, feat_list):
            if l < maxlen:
                new_seqs.append(s)
                new_feat_list.append(y)
                new_lengths.append(l)
        lengths = new_lengths
        feat_list = new_feat_list
        seqs = new_seqs

        if len(lengths) < 1:
            return None, None, None

    # Compute skip-thought vectors for this mini-batch
    feat_list = skipthoughts.encode(model, feat_list, use_eos=False, verbose=False)

    y = numpy.zeros((len(feat_list), len(feat_list[0]))).astype('float32')
    for idx, ff in enumerate(feat_list):
        y[idx,:] = ff

    n_samples = len(seqs)
    maxlen = numpy.max(lengths)+1

    x = numpy.zeros((maxlen, n_samples)).astype('int64')
    x_mask = numpy.zeros((maxlen, n_samples)).astype('float32')
    for idx, s in enumerate(seqs):
        x[:lengths[idx],idx] = s
        x_mask[:lengths[idx]+1,idx] = 1.

    return x, x_mask, y

