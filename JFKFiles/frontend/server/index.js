var express = require('express');
var path = require('path');

var app = express();
var distPath = path.resolve(__dirname, '../dist');

app.use(express.static(distPath));
app.listen(process.env.PORT, function() {
  console.log('Server running on port ' + process.env.PORT);
});