'use strict';
//=========================================================
// Import modules
//=========================================================
const fulfilment = require('./lib/fulfilment');
const express = require('express');
const bodyParser = require('body-parser');

const router = express.Router();

let middelware = (directlineSecret, messagesObj = null) => {

    if (!directlineSecret) {
        throw new Error('Direct Line secret not defined.');
    }

    router.use(bodyParser.json());
    fulfilment(directlineSecret, messagesObj, router);

    return { router };
};


module.exports = middelware;