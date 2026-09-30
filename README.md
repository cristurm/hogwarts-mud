Hogwarts MUD
============

A small browser-based chat room inspired by the original Unity MUD. A host creates a room and shares its six-character code; friends join with the code and a name.

## Run locally

Requires Node.js 20 or newer.

```sh
npm install
npm start
```

Open [http://localhost:3000](http://localhost:3000) in your browser. For friends to join from other devices, deploy this app to a server reachable by everyone and have them open the same URL. Room codes and chat are held in memory and disappear when the room empties or the server restarts.