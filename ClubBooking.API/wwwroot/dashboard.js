// dashboard.js – полная исправленная версия

/********************** ОБЩИЕ УТИЛИТЫ **********************/

function formatDateTime(isoString) {
    const d = new Date(isoString);
    return d.toLocaleString();
}

function showMessage(msg, isError = true) {
    const existing = document.getElementById('floatingMessage');
    if (existing) existing.remove();
    const div = document.createElement('div');
    div.id = 'floatingMessage';
    div.textContent = msg;
    div.style.position = 'fixed';
    div.style.bottom = '20px';
    div.style.right = '20px';
    div.style.backgroundColor = isError ? '#dc3545' : '#28a745';
    div.style.color = 'white';
    div.style.padding = '10px 20px';
    div.style.borderRadius = '5px';
    div.style.zIndex = '9999';
    document.body.appendChild(div);
    setTimeout(() => div.remove(), 3000);
}

/********************** КЛИЕНТ **********************/

async function renderSeats(seats) {
    const container = document.getElementById('seatsContainer');
    if (!container) return;
    if (!seats.length) {
        container.innerHTML = '<p>Нет мест в этом клубе</p>';
        return;
    }
    let html = '<div class="seats-grid">';
    seats.forEach(seat => {
        let statusClass = '';
        let statusText = '';
        const formatTime = (isoString) => {
            if (!isoString) return '';
            const date = new Date(isoString);
            return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        };
        switch (seat.status) {
            case 'Free':
                statusClass = 'free';
                statusText = 'Свободно';
                break;
            case 'BusyNow':
                statusClass = 'busy';
                statusText = `Занято до ${formatTime(seat.availableFrom)}`;
                break;
            case 'BookedFuture':
                statusClass = 'booked';
                statusText = `Занято с ${formatTime(seat.availableFrom)}`;
                break;
            default:
                statusClass = 'free';
                statusText = 'Свободно';
        }
        // ВСТРОЕННЫЙ onclick
        html += `<div class="seat ${statusClass}" data-seat-id="${seat.id}" onclick="showBookingModal('${seat.id}')">
                    Место ${seat.seatNumber}<br>${statusText}
                 </div>`;
    });
    html += '</div>';
    container.innerHTML = html;
}

async function loadMyBookings() {
    const container = document.getElementById('myBookingsList');
    if (!container) return;
    try {
        const bookings = await apiRequest('/bookings/my');
        if (!bookings.length) {
            container.innerHTML = '<p>Нет броней</p>';
            return;
        }
        container.innerHTML = '<ul class="booking-list">' + bookings.map(b => `
            <li>
                <span>Клуб: ${b.clubAddress}, Место ${b.seatNumber}, ${formatDateTime(b.startTime)} - ${formatDateTime(b.endTime)}</span>
                <button class="delete-btn" data-booking-id="${b.id}">Отменить</button>
            </li>
        `).join('') + '</ul>';
        document.querySelectorAll('.delete-btn').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                const id = btn.dataset.bookingId;
                await apiRequest(`/bookings/${id}`, 'DELETE');
                showMessage('Бронь отменена', false);
                await loadMyBookings();
                const clubSelect = document.getElementById('clubSelect');
                if (clubSelect && clubSelect.value) {
                    const seats = await apiRequest(`/seats/club/${clubSelect.value}`);
                    await renderSeats(seats);
                }
            });
        });
    } catch (err) {
        showMessage(err.message);
    }
}

// Показывает красивое модальное окно для выбора даты и времени бронирования
async function showBookingModal(seatId) {
     alert('showBookingModal вызвана!');
    // Создаём модальное окно
    const modal = document.createElement('div');
    modal.className = 'booking-modal';
    modal.innerHTML = `
        <div class="booking-modal-content">
            <div class="booking-modal-header">
                <h3>📅 Бронирование места</h3>
                <button class="booking-modal-close">&times;</button>
            </div>
            <div class="booking-modal-body">
                <div class="form-group">
                    <label>📆 Дата и время начала</label>
                    <input type="datetime-local" id="bookingStart" class="booking-datetime" step="60">
                </div>
                <div class="form-group">
                    <label>⏳ Дата и время окончания</label>
                    <input type="datetime-local" id="bookingEnd" class="booking-datetime" step="60">
                </div>
                <div class="booking-error" id="bookingError"></div>
            </div>
            <div class="booking-modal-footer">
                <button id="confirmBooking" class="booking-btn booking-btn-primary">✅ Подтвердить</button>
                <button id="cancelBooking" class="booking-btn booking-btn-secondary">❌ Отмена</button>
            </div>
        </div>
    `;

    document.body.appendChild(modal);

    // Элементы
    const startInput = modal.querySelector('#bookingStart');
    const endInput = modal.querySelector('#bookingEnd');
    const errorDiv = modal.querySelector('#bookingError');
    const confirmBtn = modal.querySelector('#confirmBooking');
    const cancelBtn = modal.querySelector('#cancelBooking');
    const closeBtn = modal.querySelector('.booking-modal-close');

    // Устанавливаем минимальную дату – сейчас + 1 час (чтобы нельзя было выбрать прошлое)
    const now = new Date();
    now.setHours(now.getHours() + 1);
    const minDateTime = now.toISOString().slice(0, 16);
    startInput.min = minDateTime;
    endInput.min = minDateTime;

    // Автоматически подставляем конечное время +2 часа, если выбрано начало
    startInput.addEventListener('change', () => {
        if (startInput.value) {
            const startDate = new Date(startInput.value);
            const endDate = new Date(startDate.getTime() + 2 * 60 * 60 * 1000); // +2 часа
            endInput.value = endDate.toISOString().slice(0, 16);
        }
    });

    // Функция закрытия
    const closeModal = () => modal.remove();

    confirmBtn.onclick = async () => {
        const start = startInput.value;
        const end = endInput.value;
        if (!start || !end) {
            errorDiv.textContent = '❌ Заполните оба поля';
            return;
        }

        const startTime = new Date(start);
        const endTime = new Date(end);
        const nowUtc = new Date();

        // Валидация
        if (startTime < nowUtc) {
            errorDiv.textContent = '❌ Начало брони должно быть в будущем';
            return;
        }
        if (endTime <= startTime) {
            errorDiv.textContent = '❌ Конец брони должен быть позже начала';
            return;
        }
        const durationHours = (endTime - startTime) / (1000 * 60 * 60);
        if (durationHours > 4) {
            errorDiv.textContent = '❌ Максимальная длительность брони – 4 часа';
            return;
        }

        // Преобразуем локальное время в UTC для отправки на сервер
        const startISO = startTime.toISOString();
        const endISO = endTime.toISOString();

        errorDiv.textContent = '';
        confirmBtn.disabled = true;
        confirmBtn.textContent = '⏳ Обработка...';

        try {
            await apiRequest('/bookings', 'POST', { seatId, startTime: startISO, endTime: endISO });
            alert('✅ Бронь успешно создана!');
            closeModal();
            // Обновляем места и список броней
            const clubSelect = document.getElementById('clubSelect');
            if (clubSelect && clubSelect.value) {
                const seats = await apiRequest(`/seats/club/${clubSelect.value}`);
                await renderSeats(seats);
                await loadMyBookings();
            }
        } catch (err) {
            errorDiv.textContent = '❌ ' + err.message;
            confirmBtn.disabled = false;
            confirmBtn.textContent = '✅ Подтвердить';
        }
    };

    cancelBtn.onclick = closeModal;
    closeBtn.onclick = closeModal;

    // Закрытие по Escape
    window.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.parentNode) closeModal();
    }, { once: true });
}

/********************** АДМИНИСТРАТОР **********************/

async function loadClients() {
    const container = document.getElementById('clientsList');
    if (!container) return;
    try {
        const clients = await apiRequest('/users/clients');
        if (!clients.length) {
            container.innerHTML = '<p>Нет клиентов</p>';
            return;
        }
        container.innerHTML = '<ul class="user-list">' + clients.map(u => `
            <li>
                <span>${u.nickname} (${u.email})</span>
                <button class="delete-btn" data-user-id="${u.id}">Удалить</button>
            </li>
        `).join('') + '</ul>';
        document.querySelectorAll('[data-user-id]').forEach(btn => {
            btn.addEventListener('click', async () => {
                const userId = btn.dataset.userId;
                await apiRequest(`/users/${userId}`, 'DELETE');
                showMessage('Пользователь удалён', false);
                await loadClients();
            });
        });
    } catch (err) {
        showMessage(err.message);
    }
}

async function renderAdminBookings(bookings) {
    const container = document.getElementById('adminBookingsList');
    if (!container) return;
    if (!bookings.length) {
        container.innerHTML = '<p>Нет броней в этом клубе</p>';
        return;
    }
    container.innerHTML = '<ul class="booking-list">' + bookings.map(b => `
        <li>
            <span>Клиент: ${b.userNickname}, Место ${b.seatNumber}, ${formatDateTime(b.startTime)} - ${formatDateTime(b.endTime)}</span>
            <button class="delete-btn" data-booking-id="${b.id}">Удалить</button>
        </li>
    `).join('') + '</ul>';
    document.querySelectorAll('.delete-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            const id = btn.dataset.bookingId;
            await apiRequest(`/bookings/${id}`, 'DELETE');
            showMessage('Бронь удалена', false);
            const clubSelect = document.getElementById('clubSelectAdmin');
            if (clubSelect && clubSelect.value) {
                const newBookings = await apiRequest(`/bookings/club/${clubSelect.value}`);
                await renderAdminBookings(newBookings);
            }
        });
    });
}

async function showCreateBookingModal() {
    const clients = await apiRequest('/users/clients');
    const clubs = await apiRequest('/clubs');
    if (!clients.length) {
        showMessage('Нет клиентов для бронирования');
        return;
    }
    let seats = [];
    if (clubs.length) {
        seats = await apiRequest(`/seats/club/${clubs[0].id}`);
    }
    const modalHtml = `
        <div id="adminBookingModal" class="modal">
            <div class="modal-content">
                <h3>Создать бронь для клиента</h3>
                <div class="form-group">
                    <label>Клиент</label>
                    <select id="adminUserId">
                        ${clients.map(c => `<option value="${c.id}">${c.nickname} (${c.email})</option>`).join('')}
                    </select>
                </div>
                <div class="form-group">
                    <label>Клуб</label>
                    <select id="adminClubId">
                        ${clubs.map(c => `<option value="${c.id}">${c.address}</option>`).join('')}
                    </select>
                </div>
                <div class="form-group">
                    <label>Место</label>
                    <select id="adminSeatId">
                        ${seats.map(s => `<option value="${s.id}">Место ${s.seatNumber}</option>`).join('')}
                    </select>
                </div>
                <div class="form-group">
                    <label>Начало (локальное время)</label>
                    <input type="datetime-local" id="adminStart">
                </div>
                <div class="form-group">
                    <label>Конец (локальное время)</label>
                    <input type="datetime-local" id="adminEnd">
                </div>
                <div style="display: flex; gap: 10px;">
                    <button id="confirmAdminBooking">Создать</button>
                    <button id="cancelAdminBooking" class="btn-secondary">Отмена</button>
                </div>
            </div>
        </div>
    `;
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modal = document.getElementById('adminBookingModal');
    const clubSelect = document.getElementById('adminClubId');
    const seatSelect = document.getElementById('adminSeatId');
    
    clubSelect.addEventListener('change', async () => {
        const clubId = clubSelect.value;
        const seatsData = await apiRequest(`/seats/club/${clubId}`);
        seatSelect.innerHTML = seatsData.map(s => `<option value="${s.id}">Место ${s.seatNumber}</option>`).join('');
    });
    
    document.getElementById('confirmAdminBooking').onclick = async () => {
        const userId = document.getElementById('adminUserId').value;
        const seatId = document.getElementById('adminSeatId').value;
        const start = document.getElementById('adminStart').value;
        const end = document.getElementById('adminEnd').value;
        if (!start || !end) {
            showMessage('Заполните время');
            return;
        }
        const startISO = new Date(start).toISOString();
        const endISO = new Date(end).toISOString();
        try {
            await apiRequest('/bookings/admin', 'POST', { userId, seatId, startTime: startISO, endTime: endISO });
            showMessage('Бронь создана', false);
            modal.remove();
            const clubSelect = document.getElementById('clubSelectAdmin');
            if (clubSelect && clubSelect.value) {
                const bookings = await apiRequest(`/bookings/club/${clubSelect.value}`);
                await renderAdminBookings(bookings);
            }
        } catch (err) {
            showMessage(err.message);
        }
    };
    document.getElementById('cancelAdminBooking').onclick = () => modal.remove();
}

/********************** ГЛАВНЫЙ АДМИНИСТРАТОР **********************/

async function loadClubs() {
    const container = document.getElementById('clubsList');
    if (!container) return;
    try {
        const clubs = await apiRequest('/clubs');
        if (!clubs.length) {
            container.innerHTML = '<p>Нет клубов</p>';
            return;
        }
        container.innerHTML = '<ul class="club-list">' + clubs.map(c => `
            <li>
                <span>${c.address}</span>
                <button class="delete-btn" data-club-id="${c.id}">Удалить</button>
            </li>
        `).join('') + '</ul>';
        document.querySelectorAll('[data-club-id]').forEach(btn => {
            btn.addEventListener('click', async () => {
                const clubId = btn.dataset.clubId;
                await apiRequest(`/clubs/${clubId}`, 'DELETE');
                showMessage('Клуб удалён', false);
                await loadClubs();
            });
        });
    } catch (err) {
        showMessage(err.message);
    }
}

async function loadAllBookings() {
    const container = document.getElementById('allBookingsList');
    if (!container) return;
    try {
        const clubs = await apiRequest('/clubs');
        let allBookingsHtml = '';
        for (const club of clubs) {
            const bookings = await apiRequest(`/bookings/club/${club.id}`);
            if (bookings.length) {
                allBookingsHtml += `<h4>Клуб: ${club.address}</h4><ul class="booking-list">`;
                bookings.forEach(b => {
                    allBookingsHtml += `<li>Клиент: ${b.userNickname}, Место ${b.seatNumber}, ${formatDateTime(b.startTime)} - ${formatDateTime(b.endTime)} 
                        <button class="delete-btn" data-booking-id="${b.id}">Удалить</button></li>`;
                });
                allBookingsHtml += `</ul>`;
            } else {
                allBookingsHtml += `<h4>Клуб: ${club.address}</h4><p>Нет броней</p>`;
            }
        }
        container.innerHTML = allBookingsHtml || '<p>Нет броней</p>';
        document.querySelectorAll('[data-booking-id]').forEach(btn => {
            btn.addEventListener('click', async () => {
                const id = btn.dataset.bookingId;
                await apiRequest(`/bookings/${id}`, 'DELETE');
                showMessage('Бронь удалена', false);
                await loadAllBookings();
            });
        });
    } catch (err) {
        showMessage(err.message);
    }
}

async function loadAllUsers() {
    const container = document.getElementById('usersList');
    if (!container) return;
    try {
        const users = await apiRequest('/users');
        if (!users.length) {
            container.innerHTML = '<p>Нет пользователей</p>';
            return;
        }
        container.innerHTML = '<ul class="user-list">' + users.map(u => `
            <li>
                <span>${u.nickname} (${u.email}) - ${u.role}</span>
                <button class="delete-btn" data-user-id="${u.id}">Удалить</button>
            </li>
        `).join('') + '</ul>';
        document.querySelectorAll('[data-user-id]').forEach(btn => {
            btn.addEventListener('click', async () => {
                const userId = btn.dataset.userId;
                await apiRequest(`/users/${userId}`, 'DELETE');
                showMessage('Пользователь удалён', false);
                await loadAllUsers();
            });
        });
    } catch (err) {
        showMessage(err.message);
    }
}

/********************** ОТОБРАЖЕНИЕ ДАШБОРДА ПО РОЛИ **********************/

async function showDashboard() {
    const main = document.getElementById('mainContent');
    const role = currentUser.role;
    const clubs = await apiRequest('/clubs');
    let html = `<div class="card"><h2>Добро пожаловать, ${currentUser.nickname} (${role})</h2><button id="logoutBtn">Выйти</button></div>`;
    if (role === 'Client') {
        html += `<div class="card"><h3>Выберите клуб</h3><select id="clubSelect">${clubs.map(c => `<option value="${c.id}">${c.address}</option>`).join('')}</select><div id="seatsContainer"></div></div>
                 <div class="card"><h3>Мои брони</h3><div id="myBookingsList"></div></div>`;
    } else if (role === 'Admin') {
        html += `<div class="card">
                    <h3>Управление бронями</h3>
                    <div class="admin-select"><label>Клуб:</label><select id="adminClubSelect">${clubs.map(c => `<option value="${c.id}">${c.address}</option>`).join('')}</select></div>
                    <button id="showBookingsBtn">Показать все брони клуба</button>
                    <div id="adminBookingsList"></div>
                 </div>
                 <div class="card">
                    <h3>Клиенты</h3>
                    <button id="loadClientsBtn">Загрузить клиентов</button>
                    <div id="clientsList"></div>
                 </div>`;
    } else if (role === 'SuperAdmin') {
        html += `<div class="card">
                    <h3>Управление бронями (SuperAdmin)</h3>
                    <div class="admin-select"><label>Клуб:</label><select id="adminClubSelect">${clubs.map(c => `<option value="${c.id}">${c.address}</option>`).join('')}</select></div>
                    <button id="showBookingsBtn">Показать все брони клуба</button>
                    <div id="adminBookingsList"></div>
                 </div>
                 <div class="card">
                    <h3>Клиенты</h3>
                    <button id="loadClientsBtn">Загрузить клиентов</button>
                    <div id="clientsList"></div>
                 </div>
                 <div class="card">
                    <h3>Управление клубами (SuperAdmin)</h3>
                    <button id="manageClubsBtn">Управление клубами</button>
                    <div id="clubsManagement"></div>
                 </div>`;
    }
    main.innerHTML = html;
    document.getElementById('logoutBtn')?.addEventListener('click', logout);
    if (role === 'Client') {
        const clubSelect = document.getElementById('clubSelect');
        const loadSeats = async () => {
            const seats = await apiRequest(`/seats/club/${clubSelect.value}`);
            renderSeats(seats);
        };
        clubSelect.addEventListener('change', loadSeats);
        await loadSeats();
        await loadMyBookings();
    } else if (role === 'Admin') {
        const adminClubSelect = document.getElementById('adminClubSelect');
        document.getElementById('showBookingsBtn').onclick = async () => {
            const clubId = adminClubSelect.value;
            await loadAdminBookings(clubId);
        };
        document.getElementById('loadClientsBtn').onclick = async () => {
            await loadClientsList();
        };
    } else if (role === 'SuperAdmin') {
        const adminClubSelect = document.getElementById('adminClubSelect');
        if (adminClubSelect) {
            document.getElementById('showBookingsBtn').onclick = async () => {
                const clubId = adminClubSelect.value;
                await loadAdminBookings(clubId);
            };
            document.getElementById('loadClientsBtn').onclick = async () => {
                await loadClientsList();
            };
            document.getElementById('manageClubsBtn').onclick = async () => {
                const clubsData = await apiRequest('/clubs');
                const div = document.getElementById('clubsManagement');
                div.innerHTML = `
                    <h4>Существующие клубы</h4>
                    <ul>${clubsData.map(c => `<li>${c.address} <button onclick="apiRequest('/clubs/${c.id}','DELETE').then(()=>location.reload())">Удалить</button></li>`).join('')}</ul>
                    <h4>Создать новый клуб</h4>
                    <div class="form-group"><label>Адрес</label><input id="newClubAddress" placeholder="Например, ул. Центральная, 5"></div>
                    <div class="form-group"><label>Количество мест (2-6)</label>
                        <select id="newClubSeats">
                            <option value="2">2 места</option>
                            <option value="3">3 места</option>
                            <option value="4" selected>4 места</option>
                            <option value="5">5 мест</option>
                            <option value="6">6 мест</option>
                        </select>
                    </div>
                    <button id="addClubBtn">Добавить клуб</button>
                `;
                document.getElementById('addClubBtn')?.addEventListener('click', async () => {
                    const address = document.getElementById('newClubAddress').value;
                    const seats = parseInt(document.getElementById('newClubSeats').value, 10);
                    if (!address) { alert('Введите адрес'); return; }
                    await apiRequest('/clubs', 'POST', { address, numberOfSeats: seats });
                    location.reload();
                });
            };
        }
    }
}