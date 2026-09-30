import { createServer } from 'node:http';
import { randomInt } from 'node:crypto';
import { readFile } from 'node:fs/promises';
import { extname, join, normalize } from 'node:path';
import { fileURLToPath } from 'node:url';
import { WebSocketServer, WebSocket } from 'ws';

const projectDirectory = fileURLToPath(new URL('.', import.meta.url));
const publicDirectory = join(projectDirectory, 'public');
const port = Number(process.env.PORT) || 3000;
const rooms = new Map();
const roomCodeCharacters = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';

const contentTypes = {
  '.css': 'text/css; charset=utf-8',
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
};

/**
 * A room exists in memory while it has at least one connected player.
 * The first player to join becomes its host.
 *
 * @typedef {object} Room
 * @property {Set<WebSocket>} members Open WebSocket connections in the room.
 * @property {WebSocket | null} host The current host connection, if any.
 */

/**
 * The small JSON message protocol accepted from a browser client.
 * A client first sends `join`; after joining, it may send `chat` messages.
 *
 * @typedef {object} ClientMessage
 * @property {'join' | 'chat'} type Which action the client is requesting.
 * @property {string} [code] Room code, required for `join` messages.
 * @property {string} [name] Player name, required for `join` messages.
 * @property {string} [text] Chat text, required for `chat` messages.
 */

/**
 * Sends one JSON message to one connected browser.
 * A WebSocket is a persistent, two-way connection, but it can only send while
 * its ready state is OPEN. JSON gives both sides a simple shared data format.
 *
 * @param {WebSocket} clientSocket The connection to send to.
 * @param {object} payload The object to serialize and send.
 * @see https://developer.mozilla.org/en-US/docs/Web/API/WebSocket/readyState
 * @see https://github.com/websockets/ws#websocket
 */
function send(clientSocket, payload) {
  if (clientSocket.readyState === WebSocket.OPEN) {
    clientSocket.send(JSON.stringify(payload));
  }
}

/**
 * Sends the same event to every connection currently in a room.
 * This is how a message from one player appears on every player's screen.
 *
 * @param {Room} room The room whose members should receive the event.
 * @param {object} roomEvent The event to send to each member.
 */
function broadcast(room, roomEvent) {
  for (const memberSocket of room.members) send(memberSocket, roomEvent);
}

/**
 * Creates a short, hard-to-guess room code that is not already in use.
 * `randomInt` avoids the modulo bias that can occur with simpler random math.
 *
 * @returns {string} A unique six-character code.
 * @see https://nodejs.org/api/crypto.html#cryptorandomintmin-max-callback
 */
function makeRoomCode() {
  let roomCode;
  do {
    roomCode = Array.from({ length: 6 }, () =>
      roomCodeCharacters[randomInt(roomCodeCharacters.length)],
    ).join('');
  } while (rooms.has(roomCode));
  return roomCode;
}

/**
 * Reads and returns one file from the public folder for an HTTP GET request.
 * Normalizing the path and checking its folder prefix prevents requests from
 * using `..` to read files outside the browser app.
 *
 * @param {import('node:http').ServerResponse} response The response to complete.
 * @param {string} pathname The URL path requested by the browser.
 * @returns {Promise<void>}
 * @see https://nodejs.org/api/fs.html#promises-api
 */
async function serveFile(response, pathname) {
  const requestedPath = pathname === '/' ? '/index.html' : pathname;
  const filePath = normalize(join(publicDirectory, requestedPath));

  if (!filePath.startsWith(`${publicDirectory}/`)) {
    response.writeHead(404).end('Not found');
    return;
  }

  try {
    const fileContents = await readFile(filePath);
    response.writeHead(200, {
      'Content-Type': contentTypes[extname(filePath)] ?? 'application/octet-stream',
      'X-Content-Type-Options': 'nosniff',
    });
    response.end(fileContents);
  } catch {
    response.writeHead(404).end('Not found');
  }
}

/**
 * Handles ordinary HTTP requests: static files use GET, and creating a room
 * uses POST /api/rooms. Creating a room stores it in memory; the browser then
 * opens a separate WebSocket connection to join and exchange live events.
 *
 * @param {import('node:http').IncomingMessage} request The incoming HTTP request.
 * @param {import('node:http').ServerResponse} response The response to send back.
 * @returns {Promise<void>}
 * @see https://nodejs.org/api/http.html
 */
async function handleHttpRequest(request, response) {
  const url = new URL(request.url, `http://${request.headers.host}`);

  if (request.method === 'POST' && url.pathname === '/api/rooms') {
    const roomCode = makeRoomCode();
    rooms.set(roomCode, { members: new Set(), host: null });
    response.writeHead(201, { 'Content-Type': 'application/json' });
    response.end(JSON.stringify({ code: roomCode }));
    return;
  }

  if (request.method === 'GET') {
    await serveFile(response, decodeURIComponent(url.pathname));
    return;
  }

  response.writeHead(404).end('Not found');
}

const server = createServer(handleHttpRequest);

// Attach WebSockets to this HTTP server. The browser uses the same host and
// port, but the /chat path upgrades its connection from HTTP to WebSocket.
const webSocketServer = new WebSocketServer({ server, path: '/chat' });

/**
 * Runs when a browser finishes opening its WebSocket connection.
 * The server then listens for messages and for the connection closing.
 *
 * @param {WebSocket} clientSocket The new browser connection.
 * @see https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API
 * @see https://github.com/websockets/ws#readme
 */
function handleWebSocketConnection(clientSocket) {
  /**
   * Parses one JSON message and handles either the first room join or chat.
   * This listener stays active for the lifetime of the WebSocket connection.
   *
   * @param {Buffer} rawMessage The bytes received from the browser.
   */
  function handleClientMessage(rawMessage) {
    let clientMessage;
    try {
      clientMessage = JSON.parse(rawMessage.toString());
    } catch {
      send(clientSocket, { type: 'error', message: 'That message could not be read.' });
      return;
    }

    if (!clientSocket.room) {
      if (clientMessage.type !== 'join') return;
      const roomCode = String(clientMessage.code ?? '').trim().toUpperCase();
      const playerName = String(clientMessage.name ?? '').trim().slice(0, 24);
      const room = rooms.get(roomCode);

      if (!room) {
        send(clientSocket, { type: 'error', message: 'Room not found. Check the code and try again.' });
        clientSocket.close();
        return;
      }
      if (!playerName) {
        send(clientSocket, { type: 'error', message: 'Enter a name to join the room.' });
        clientSocket.close();
        return;
      }

      // Keep these values on this connection so later chat/disconnect events
      // know who sent them and which room membership to update.
      clientSocket.room = room;
      clientSocket.playerName = playerName;
      room.members.add(clientSocket);
      if (!room.host) room.host = clientSocket;

      broadcast(room, { type: 'welcome', name: playerName });
      broadcast(room, {
        type: 'players',
        players: Array.from(room.members, (memberSocket) => ({
          name: memberSocket.playerName,
          isHost: memberSocket === room.host,
        })),
      });
      return;
    }

    if (clientMessage.type === 'chat') {
      const messageText = String(clientMessage.text ?? '').trim().slice(0, 500);
      if (messageText) {
        broadcast(clientSocket.room, {
          type: 'chat',
          name: clientSocket.playerName,
          text: messageText,
          sentAt: new Date().toISOString(),
        });
      }
    }
  }

  /**
   * Removes a disconnected player and updates everyone still in the room.
   * Empty rooms are deleted so their codes can eventually be reused.
   */
  function handleClientDisconnect() {
    const room = clientSocket.room;
    if (!room) return;

    room.members.delete(clientSocket);
    if (room.host === clientSocket) room.host = room.members.values().next().value ?? null;
    if (room.members.size === 0) {
      for (const [roomCode, roomInMap] of rooms) {
        if (roomInMap === room) rooms.delete(roomCode);
      }
      return;
    }

    broadcast(room, { type: 'notice', message: `${clientSocket.playerName} left the room.` });
    broadcast(room, {
      type: 'players',
      players: Array.from(room.members, (memberSocket) => ({
        name: memberSocket.playerName,
        isHost: memberSocket === room.host,
      })),
    });
  }

  clientSocket.on('message', handleClientMessage);
  clientSocket.on('close', handleClientDisconnect);
}

webSocketServer.on('connection', handleWebSocketConnection);

server.listen(port, () => {
  console.log(`Hogwarts MUD chat is running at http://localhost:${port}`);
});