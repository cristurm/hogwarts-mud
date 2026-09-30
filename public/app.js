const lobby = document.querySelector('#lobby');
const chatRoom = document.querySelector('#chat-room');
const createTab = document.querySelector('#create-tab');
const joinTab = document.querySelector('#join-tab');
const createForm = document.querySelector('#create-panel');
const joinForm = document.querySelector('#join-panel');
const errorMessage = document.querySelector('#form-error');
const roomCodeDisplay = document.querySelector('#display-code');
const roomRole = document.querySelector('#room-role');
const messageList = document.querySelector('#message-list');
const emptyState = document.querySelector('#empty-state');
const peopleList = document.querySelector('#people-list');
const peopleCount = document.querySelector('#people-count');
const messageForm = document.querySelector('#message-form');
const messageInput = document.querySelector('#message-input');

let connection;
let activeCode;

function showError(message) {
  errorMessage.textContent = message;
}

function setEntryMode(mode) {
  const createMode = mode === 'create';
  createTab.checked = createMode;
  joinTab.checked = !createMode;
  createForm.hidden = !createMode;
  joinForm.hidden = createMode;
  showError('');
}

function showChat(code, isHost) {
  activeCode = code;
  roomCodeDisplay.textContent = code;
  roomRole.textContent = isHost ? 'Your common room' : 'Visiting this room';
  lobby.hidden = true;
  chatRoom.hidden = false;
  messageInput.focus();
}

function addNotice(message) {
  const notice = document.createElement('p');
  notice.className = 'room-notice';
  notice.textContent = message;
  messageList.append(notice);
  messageList.scrollTop = messageList.scrollHeight;
}

function addWelcome(name) {
  emptyState.hidden = true;
  const item = document.createElement('p');
  item.className = 'room-notice';
  item.textContent = `Welcome ${name}!`;
  messageList.append(item);
  messageList.scrollTop = messageList.scrollHeight;
}

function addChatMessage({ name, text, sentAt }) {
  emptyState.hidden = true;
  const article = document.createElement('article');
  article.className = 'message';

  const meta = document.createElement('div');
  meta.className = 'message-meta';
  const sender = document.createElement('span');
  sender.className = 'message-name';
  sender.textContent = name;
  const time = document.createElement('time');
  time.className = 'message-time';
  time.dateTime = sentAt;
  time.textContent = new Intl.DateTimeFormat(undefined, {
    hour: 'numeric',
    minute: '2-digit',
  }).format(new Date(sentAt));

  const body = document.createElement('p');
  body.className = 'message-text';
  body.textContent = text;
  meta.append(sender, time);
  article.append(meta, body);
  messageList.append(article);
  messageList.scrollTop = messageList.scrollHeight;
}

function renderPlayers(players) {
  peopleCount.textContent = String(players.length);
  peopleList.replaceChildren();

  for (const player of players) {
    const item = document.createElement('li');
    item.className = 'person';
    const mark = document.createElement('span');
    mark.className = 'person-mark';
    mark.setAttribute('aria-hidden', 'true');
    mark.textContent = player.name.charAt(0).toUpperCase();
    const name = document.createElement('span');
    name.textContent = player.name;
    item.append(mark, name);

    if (player.isHost) {
      const hostTag = document.createElement('span');
      hostTag.className = 'host-tag';
      hostTag.textContent = 'HOST';
      item.append(hostTag);
    }
    peopleList.append(item);
  }
}

function connectToRoom(code, name, isHost) {
  showError('');
  const protocol = location.protocol === 'https:' ? 'wss:' : 'ws:';
  connection = new WebSocket(`${protocol}//${location.host}/chat`);

  connection.addEventListener('open', () => {
    connection.send(JSON.stringify({ type: 'join', code, name }));
  });
  connection.addEventListener('message', ({ data }) => {
    const event = JSON.parse(data);
    if (event.type === 'error') {
      showError(event.message);
      connection.close();
    } else if (event.type === 'welcome') {
      showChat(code, isHost);
      addWelcome(event.name);
    } else if (event.type === 'chat') {
      addChatMessage(event);
    } else if (event.type === 'notice') {
      addNotice(event.message);
    } else if (event.type === 'players') {
      renderPlayers(event.players);
    }
  });
  connection.addEventListener('error', () => {
    showError('Could not connect to the room. Please try again.');
  });
  connection.addEventListener('close', () => {
    if (!chatRoom.hidden) return;
    activeCode = undefined;
  });
}

createTab.addEventListener('change', () => setEntryMode('create'));
joinTab.addEventListener('change', () => setEntryMode('join'));

createForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  const name = new FormData(createForm).get('name').trim();
  if (!name) return;

  const button = createForm.querySelector('button[type="submit"]');
  button.disabled = true;
  showError('');
  try {
    const response = await fetch('/api/rooms', { method: 'POST' });
    if (!response.ok) throw new Error('Room could not be created. Please try again.');
    const { code } = await response.json();
    connectToRoom(code, name, true);
  } catch (error) {
    showError(error.message);
  } finally {
    button.disabled = false;
  }
});

joinForm.addEventListener('submit', (event) => {
  event.preventDefault();
  const form = new FormData(joinForm);
  const name = form.get('name').trim();
  const code = form.get('code').trim().toUpperCase();
  if (name && code) connectToRoom(code, name, false);
});

document.querySelector('#room-code').addEventListener('input', (event) => {
  event.currentTarget.value = event.currentTarget.value.toUpperCase();
});

messageForm.addEventListener('submit', (event) => {
  event.preventDefault();
  const text = messageInput.value.trim();
  if (!text || connection?.readyState !== WebSocket.OPEN) return;
  connection.send(JSON.stringify({ type: 'chat', text }));
  messageInput.value = '';
  messageInput.focus();
});

document.querySelector('#copy-code').addEventListener('click', async () => {
  const button = document.querySelector('#copy-code');
  try {
    await navigator.clipboard.writeText(activeCode);
    button.textContent = 'Copied';
    setTimeout(() => {
      button.textContent = 'Copy code';
    }, 1600);
  } catch {
    addNotice(`Share this room code: ${activeCode}`);
  }
});

document.querySelector('#leave-room').addEventListener('click', () => {
  connection?.close();
  connection = undefined;
  activeCode = undefined;
  messageList.replaceChildren(emptyState);
  emptyState.hidden = false;
  peopleList.replaceChildren();
  peopleCount.textContent = '0';
  chatRoom.hidden = true;
  lobby.hidden = false;
  setEntryMode('create');
});