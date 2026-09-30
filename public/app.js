/**
 * Browser-side controller for the room lobby and live chat.
 * Room creation uses HTTP; after that, one WebSocket stays open for room events.
 * @see https://developer.mozilla.org/en-US/docs/Web/API/WebSocket
 * @see https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API
 */
const lobby = document.querySelector('#lobby');
const chatRoom = document.querySelector('#chat-room');
const createRoomOption = document.querySelector('#create-tab');
const joinRoomOption = document.querySelector('#join-tab');
const createForm = document.querySelector('#create-panel');
const joinForm = document.querySelector('#join-panel');
const errorRegion = document.querySelector('#form-error');
const roomCodeDisplay = document.querySelector('#display-code');
const roomDescription = document.querySelector('#room-role');
const roomCodeInput = document.querySelector('#room-code');
const messageList = document.querySelector('#message-list');
const emptyState = document.querySelector('#empty-state');
const peopleList = document.querySelector('#people-list');
const peopleCount = document.querySelector('#people-count');
const messageForm = document.querySelector('#message-form');
const messageInput = document.querySelector('#message-input');
const copyCodeButton = document.querySelector('#copy-code');
const leaveRoomButton = document.querySelector('#leave-room');

let chatSocket;
let currentRoomCode;

/**
 * Writes a connection or form error into the page's live alert region.
 * `textContent` treats the message as text, not HTML markup.
 *
 * @param {string} errorText The message to announce to the user.
 */
function showError(errorText) {
  errorRegion.textContent = errorText;
}

/**
 * Shows the form selected by the native radio group.
 * Native radio buttons already support arrow-key navigation and announce their
 * checked state to assistive technology; this function only switches forms.
 *
 * @param {'create' | 'join'} selectedMode Which form should be visible.
 */
function setEntryMode(selectedMode) {
  const isCreateMode = selectedMode === 'create';
  createRoomOption.checked = isCreateMode;
  joinRoomOption.checked = !isCreateMode;
  createForm.hidden = !isCreateMode;
  joinForm.hidden = isCreateMode;
  showError('');
}

/**
 * Changes from the lobby to chat after the server confirms a successful join.
 *
 * @param {string} roomCode The code used to join this room.
 * @param {boolean} isHost Whether this browser created the room.
 */
function showChat(roomCode, isHost) {
  currentRoomCode = roomCode;
  roomCodeDisplay.textContent = roomCode;
  roomDescription.textContent = isHost ? 'Your common room' : 'Visiting this room';
  lobby.hidden = true;
  chatRoom.hidden = false;
  messageInput.focus();
}

/**
 * Adds a non-chat status line, such as a player leaving the room.
 *
 * @param {string} noticeText The status to display.
 */
function addNotice(noticeText) {
  const notice = document.createElement('p');
  notice.className = 'room-notice';
  notice.textContent = noticeText;
  messageList.append(notice);
  messageList.scrollTop = messageList.scrollHeight;
}

/**
 * Adds the server's welcome event to the chat log.
 *
 * @param {string} playerName The name the server accepted for the new player.
 */
function addWelcome(playerName) {
  emptyState.hidden = true;
  const welcomeMessage = document.createElement('p');
  welcomeMessage.className = 'room-notice';
  welcomeMessage.textContent = `Welcome ${playerName}!`;
  messageList.append(welcomeMessage);
  messageList.scrollTop = messageList.scrollHeight;
}

/**
 * Builds one chat entry from a server event and scrolls it into view.
 * User-provided text is inserted with `textContent`, so it cannot become HTML.
 *
 * @param {{name: string, text: string, sentAt: string}} chatEvent The server's chat event.
 */
function addChatMessage({ name: playerName, text: messageText, sentAt }) {
  emptyState.hidden = true;
  const messageArticle = document.createElement('article');
  messageArticle.className = 'message';

  const messageDetails = document.createElement('div');
  messageDetails.className = 'message-meta';
  const senderName = document.createElement('span');
  senderName.className = 'message-name';
  senderName.textContent = playerName;
  const sentTime = document.createElement('time');
  sentTime.className = 'message-time';
  sentTime.dateTime = sentAt;
  sentTime.textContent = new Intl.DateTimeFormat(undefined, {
    hour: 'numeric',
    minute: '2-digit',
  }).format(new Date(sentAt));

  const messageBody = document.createElement('p');
  messageBody.className = 'message-text';
  messageBody.textContent = messageText;
  messageDetails.append(senderName, sentTime);
  messageArticle.append(messageDetails, messageBody);
  messageList.append(messageArticle);
  messageList.scrollTop = messageList.scrollHeight;
}

/**
 * Replaces the participant list with the latest roster sent by the server.
 *
 * @param {Array<{name: string, isHost: boolean}>} players Current room members.
 */
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

/**
 * Opens the live WebSocket connection and asks the server to join a room.
 * The `open` event means the WebSocket handshake succeeded; only then do we
 * send the JSON join message. Later `message` events carry server updates.
 *
 * @param {string} roomCode The room the player wants to enter.
 * @param {string} playerName The name to show to other room members.
 * @param {boolean} isHost Whether this browser created the room over HTTP.
 * @see https://developer.mozilla.org/en-US/docs/Web/API/WebSocket/open_event
 * @see https://developer.mozilla.org/en-US/docs/Web/API/WebSocket/message_event
 */
function connectToRoom(roomCode, playerName, isHost) {
  showError('');
  const webSocketProtocol = location.protocol === 'https:' ? 'wss:' : 'ws:';
  chatSocket = new WebSocket(`${webSocketProtocol}//${location.host}/chat`);

  chatSocket.addEventListener('open', () => {
    chatSocket.send(JSON.stringify({ type: 'join', code: roomCode, name: playerName }));
  });
  chatSocket.addEventListener('message', ({ data }) => {
    const serverEvent = JSON.parse(data);
    if (serverEvent.type === 'error') {
      showError(serverEvent.message);
      chatSocket.close();
    } else if (serverEvent.type === 'welcome') {
      showChat(roomCode, isHost);
      addWelcome(serverEvent.name);
    } else if (serverEvent.type === 'chat') {
      addChatMessage(serverEvent);
    } else if (serverEvent.type === 'notice') {
      addNotice(serverEvent.message);
    } else if (serverEvent.type === 'players') {
      renderPlayers(serverEvent.players);
    }
  });
  chatSocket.addEventListener('error', () => {
    showError('Could not connect to the room. Please try again.');
  });
  chatSocket.addEventListener('close', () => {
    if (!chatRoom.hidden) return;
    currentRoomCode = undefined;
  });
}

// The browser handles arrow-key selection for this native radio group.
createRoomOption.addEventListener('change', () => setEntryMode('create'));
joinRoomOption.addEventListener('change', () => setEntryMode('join'));

// Create the room over HTTP first; its response contains the code used by the
// separate WebSocket connection that follows.
createForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  const playerName = new FormData(createForm).get('name').trim();
  if (!playerName) return;

  const createRoomButton = createForm.querySelector('button[type="submit"]');
  createRoomButton.disabled = true;
  showError('');
  try {
    const response = await fetch('/api/rooms', { method: 'POST' });
    if (!response.ok) throw new Error('Room could not be created. Please try again.');
    const { code: roomCode } = await response.json();
    connectToRoom(roomCode, playerName, true);
  } catch (error) {
    showError(error.message);
  } finally {
    createRoomButton.disabled = false;
  }
});

joinForm.addEventListener('submit', (event) => {
  event.preventDefault();
  const joinFormData = new FormData(joinForm);
  const playerName = joinFormData.get('name').trim();
  const roomCode = joinFormData.get('code').trim().toUpperCase();
  if (playerName && roomCode) connectToRoom(roomCode, playerName, false);
});

roomCodeInput.addEventListener('input', (inputEvent) => {
  inputEvent.currentTarget.value = inputEvent.currentTarget.value.toUpperCase();
});

messageForm.addEventListener('submit', (event) => {
  event.preventDefault();
  const messageText = messageInput.value.trim();
  if (!messageText || chatSocket?.readyState !== WebSocket.OPEN) return;
  chatSocket.send(JSON.stringify({ type: 'chat', text: messageText }));
  messageInput.value = '';
  messageInput.focus();
});

copyCodeButton.addEventListener('click', async () => {
  try {
    await navigator.clipboard.writeText(currentRoomCode);
    copyCodeButton.textContent = 'Copied';
    setTimeout(() => {
      copyCodeButton.textContent = 'Copy code';
    }, 1600);
  } catch {
    addNotice(`Share this room code: ${currentRoomCode}`);
  }
});

leaveRoomButton.addEventListener('click', () => {
  chatSocket?.close();
  chatSocket = undefined;
  currentRoomCode = undefined;
  messageList.replaceChildren(emptyState);
  emptyState.hidden = false;
  peopleList.replaceChildren();
  peopleCount.textContent = '0';
  chatRoom.hidden = true;
  lobby.hidden = false;
  setEntryMode('create');
});