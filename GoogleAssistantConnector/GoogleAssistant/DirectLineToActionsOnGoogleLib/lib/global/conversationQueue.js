
class ConversationQueue {
    constructor() {
        this.globalJob;
        this._queue = new Map();
        this._interval = setInterval(this._processJobs.bind(this), process.env.POLLING_INTERVAL);
    }

    _processJobs() {
        this._queue.forEach((entry, key )=> {
            if (entry.waitToMoreActivities) {
                entry.waitToMoreActivities = false;
            }
            else {
                entry.job(entry.messages);
                this._queue.delete(key);
            }
        });
        if (this.globalJob)
            this.globalJob();
    };

    addActivity(conversationId, activity, job) {
        let entry = this._queue.get(conversationId) || {};
        entry.messages = [].concat(entry.messages || [], activity);
        entry.job = job;
        entry.waitToMoreActivities = true;
        return this._queue.set(conversationId, entry);
    };
}

module.exports = new ConversationQueue();