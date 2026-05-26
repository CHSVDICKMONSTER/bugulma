// app.js - запуск приложения
document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (token) {
        // Пробуем декодировать JWT и восстановить пользователя
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            // Имена claims могут отличаться, у нас они такие:
            const nickname = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
            const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
            if (nickname && role) {
                currentUser = { nickname, role };
                setAuthToken(token);
                showDashboard();
                return;
            }
        } catch (e) {
            console.error('Ошибка декодирования токена', e);
        }
        // Если что-то не так, очищаем токен
        localStorage.removeItem('token');
    }
    // Если токена нет или он невалидный – показываем форму входа
    showAuthForm();
});