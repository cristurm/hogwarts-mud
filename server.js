import { createServer } from 'node:http';
import { randomInt } from 'node:crypto';
import { readFile } from 'node:fs/promises';
import { extname, join, normalize } from 'node:path';
import { fileURLToPath } from 'node:url';
import { WebSocketServer, WebSocket } from 'ws';

const root = fileURLToPath(new URL('.', import.meta.url));
const publicDirectory = join(root, 'public');
const port = Number(process.env.PORT) || 3000;
const rooms = new Map();
const codeCharacters = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';

const contentTypes = {
  '.css': 'text/css; charset=utf-8',
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
};

function send(socket, event) {
  if (socket.readyState === WebSocket.OPEN) {
    socket.send(JSON.stringify(event));
  }
}

function broadcast(room, event) {
  for (const member of room.members) send(member, event);
}

function makeRoomCode() {
  let code;
  do {
    code = Array.from({ length: 6 }, () =>
      codeCharacters[randomInt(codeCharacters.length)],
    ).join('');
  } while (rooms.has(code));
  return code;
}

async function serveFile(response, pathname) {
  const requestedPath = pathname === '/' ? '/index.html' : pathname;
  const filePath = normalize(join(publicDirectory, requestedPath));

  if (!filePath.startsWith(`${publicDirectory}/`)) {
    response.writeHead(404).end('Not found');
    return;
  }

  try {
    const contents = await readFile(filePath);
    response.writeHead(200, {
      'Content-Type': contentTypes[extname(filePath)] ?? 'application/octet-stream',
      'X-Content-Type-Options': 'nosniff',
    });
    response.end(contents);
  } catch {
    response.writeHead(404).end('Not found');
  }
}

const server = createServer(async (request, response) => {
  const url = new URL(request.url, `http://${request.headers.host}`);

  if (request.method === 'POST' && url.pathname === '/api/rooms') {
    const code = makeRoomCode();
    rooms.set(code, { members: new Set(), host: null });
    response.writeHead(201, { 'Content-Type': 'application/json' });
    response.end(JSON.stringify({ code }));
    return;
  }

  if (request.method === 'GET') {
    await serveFile(response, decodeURIComponent(url.pathname));
    return;
  }

  response.writeHead(404).end('Not found');
});

const webSocketServer = new WebSocketServer({ server, path: '/chat' });

webSocketServer.on('connection', (socket) => {
  socket.on('message', (rawMessage) => {
    let event;
    try {
      event = JSON.parse(rawMessage.toString());
    } catch {
      send(socket, { type: 'error', message: 'That message could not be read.' });
      return;
    }

    if (!socket.room) {
      if (event.type !== 'join') return;
      const code = String(event.code ?? '').trim().toUpperCase();
      const name = String(event.name ?? '').trim().slice(0, 24);
      const room = rooms.get(code);

      if (!room) {
        send(socket, { type: 'error', message: 'Room not found. Check the code and try again.' });
        socket.close();
        return;
      }
      if (!name) {
        send(socket, { type: 'error', message: 'Enter a name to join the room.' });
        socket.close();
        return;
      }

      socket.room = room;
      socket.name = name;
      room.members.add(socket);
      if (!room.host) room.host = socket;

      broadcast(room, { type: 'welcome', name });
      broadcast(room, {
        type: 'players',
        players: Array.from(room.members, (member) => ({
          name: member.name,
          isHost: member === room.host,
        })),
      });
      return;
    }

    if (event.type === 'chat') {
      const text = String(event.text ?? '').trim().slice(0, 500);
      if (text) {
        broadcast(socket.room, {
          type: 'chat',
          name: socket.name,
          text,
          sentAt: new Date().toISOString(),
        });
      }
    }
  });

  socket.on('close', () => {
    const room = socket.room;
    if (!room) return;

    room.members.delete(socket);
    if (room.host === socket) room.host = room.members.values().next().value ?? null;
    if (room.members.size === 0) {
      for (const [code, candidate] of rooms) {
        if (candidate === room) rooms.delete(code);
      }
      return;
    }

    broadcast(room, { type: 'notice', message: `${socket.name} left the room.` });
    broadcast(room, {
      type: 'players',
      players: Array.from(room.members, (member) => ({
        name: member.name,
        isHost: member === room.host,
      })),
    });
  });
});

server.listen(port, () => {
  console.log(`Hogwarts MUD chat is running at http://localhost:${port}`);
});