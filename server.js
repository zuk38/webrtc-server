var fs = require('fs');
var WebSocket = require('ws');

// Yes, SSL is required
var serverConfig = {
    key: fs.readFileSync('key.pem'),
    cert: fs.readFileSync('cert.pem'),
};

// ----------------------------------------------------------------------------------------

// Create a server for handling websocket calls
var wss = new WebSocket.Server({port: 8444});

wss.on('connection', function(client) {
    client.on('message', function(message) {
        // Broadcast any received message to all clients
        console.log('received: %s', message);
        wss.broadcast(message, client);
    });
});

wss.broadcast = function(data,exclude) {

    for (let item of this.clients) {

        if (item === exclude) continue;

        console.log("#############"+item);
        item.send(data);
    }
 
};


console.log('Server running.');
