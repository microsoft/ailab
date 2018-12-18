"""
Code for sequence generation
"""
import numpy
import copy

class GenSample(object):

    def __init__(self, **kwargs):
        self.tparams = kwargs.pop('tparams')
        self.f_init = kwargs.pop('f_init')
        self.f_next = kwargs.pop('f_next')
        self.ctx = kwargs.pop('ctx')
        self.trng = kwargs.pop('trng')
        self.k = kwargs.pop('k')
        self.maxlen = kwargs.pop('maxlen')
        self.argmax = kwargs.pop('argmax')
        self.use_unk = kwargs.pop('use_unk')

    def gen_sample(self):

        sample = []
        sample_score = []

        live_k = 1
        dead_k = 0

        hyp_samples = [[]] * live_k
        hyp_scores = numpy.zeros(live_k).astype('float32')
        hyp_states = []
        next_state = self.f_init(self.ctx)
        next_w = -1 * numpy.ones((1,)).astype('int64')
        for ii in range(int(self.maxlen)):
            inps = [next_w, next_state]
            ret = self.f_next(*inps)
            next_p, next_w, next_state = ret[0], ret[1], ret[2]
            cand_scores = hyp_scores[:,None] - numpy.log(next_p)
            cand_flat = cand_scores.flatten()
            if not self.use_unk:
                voc_size = next_p.shape[1]
                for xx in range(int(len(cand_flat) / voc_size)):
                    cand_flat[voc_size * xx + 1] = 1e20

            ranks_flat = cand_flat.argsort()[:(self.k-dead_k)]

            voc_size = next_p.shape[1]
            trans_indices = ranks_flat / voc_size
            word_indices = ranks_flat % voc_size
            costs = cand_flat[ranks_flat]

            new_hyp_samples = []
            new_hyp_scores = numpy.zeros(self.k-dead_k).astype('float32')
            new_hyp_states = []

            for idx, [ti, wi] in enumerate(zip(trans_indices, word_indices)):
                new_hyp_samples.append(hyp_samples[int(ti)]+[int(wi)])
                new_hyp_scores[idx] = copy.copy(costs[int(idx)])
                new_hyp_states.append(copy.copy(next_state[int(ti)]))
            sample, sample_score, hyp_samples, hyp_scores, hyp_states, new_live_k, dead_k = self.__check_finish_samples( sample, sample_score,
            new_hyp_samples, new_hyp_scores, new_hyp_states, dead_k)

            hyp_scores = numpy.array(hyp_scores)
            live_k = new_live_k

            if new_live_k < 1:
                break
            if dead_k >= self.k:
                break

            next_w = numpy.array([w[-1] for w in hyp_samples])
            next_state = numpy.array(hyp_states)

        if live_k > 0:
                for idx in range(int(live_k)):
                    idx=int(idx)
                    sample.append(hyp_samples[idx])
                    sample_score.append(hyp_scores[idx])

        return sample, sample_score


    def __check_finish_samples(self, sample, sample_score, new_hyp_samples, new_hyp_scores, new_hyp_states, dead_k):  
        new_live_k = 0
        hyp_samples = []
        hyp_scores = []
        hyp_states = []

        for idx in range(len(new_hyp_samples)):
            idx = int(idx)
            if new_hyp_samples[idx][-1] == 0:
                sample.append(new_hyp_samples[idx])
                sample_score.append(new_hyp_scores[idx])
                dead_k += 1
            else:
                new_live_k += 1
                hyp_samples.append(new_hyp_samples[idx])
                hyp_scores.append(new_hyp_scores[idx])
                hyp_states.append(new_hyp_states[idx])

        return sample, sample_score, hyp_samples, hyp_scores, hyp_states, new_live_k, dead_k