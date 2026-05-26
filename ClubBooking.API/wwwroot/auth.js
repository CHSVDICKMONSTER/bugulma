async function register(nickname, email, password) {
    const data = await apiRequest('/auth/register', 'POST', { nickname, email, password });
    setAuthToken(data.token);
    currentUser = { nickname: data.nickname, role: data.role };
    showDashboard();
}

async function login(login, password) {
    const data = await apiRequest('/auth/login', 'POST', { login, password });
    setAuthToken(data.token);
    currentUser = { nickname: data.nickname, role: data.role };
    showDashboard();
}

function logout() {
    setAuthToken(null);
    currentUser = null;
    showAuthForm();
}

function showAuthForm() {
    const main = document.getElementById('mainContent');
    main.innerHTML = `
        <div class="card">
            <h2>Вход / Регистрация</h2>
            <div class="nav-buttons">
                <button id="showLoginBtn">Вход</button>
                <button id="showRegisterBtn">Регистрация</button>
            </div>
            <div id="authFormContainer">
                <!-- форма будет динамически подставлена -->
            </div>
        </div>
    `;
    const loginBtn = document.getElementById('showLoginBtn');
    const registerBtn = document.getElementById('showRegisterBtn');
    const container = document.getElementById('authFormContainer');
    
    function renderLogin() {
        container.innerHTML = `
            <div class="form-group">
                <label>Логин (email или ник)</label>
                <input type="text" id="loginLogin" placeholder="email или никнейм">
            </div>
            <div class="form-group">
                <label>Пароль</label>
                <input type="password" id="loginPassword" placeholder="пароль">
            </div>
            <button id="submitLogin">Войти</button>
        `;
        document.getElementById('submitLogin').onclick = () => {
            const loginVal = document.getElementById('loginLogin').value;
            const passwordVal = document.getElementById('loginPassword').value;
            login(loginVal, passwordVal).catch(alert);
        };
    }
    
    function renderRegister() {
        container.innerHTML = `
            <div class="form-group">
                <label>Никнейм</label>
                <input type="text" id="regNickname" placeholder="никнейм">
            </div>
            <div class="form-group">
                <label>Email</label>
                <input type="email" id="regEmail" placeholder="email">
            </div>
            <div class="form-group">
                <label>Пароль</label>
                <input type="password" id="regPassword" placeholder="пароль">
            </div>
            <button id="submitRegister">Зарегистрироваться</button>
        `;
        document.getElementById('submitRegister').onclick = () => {
            const nickname = document.getElementById('regNickname').value;
            const email = document.getElementById('regEmail').value;
            const password = document.getElementById('regPassword').value;
            register(nickname, email, password).catch(alert);
        };
    }
    
    loginBtn.onclick = renderLogin;
    registerBtn.onclick = renderRegister;
    renderLogin();
}