// Конфигурация ролей (оставлена для будущего использования, но не скрывает пункты)
const roleConfig = {
    'Admin': { modules: ['dashboard', 'profile', 'schedule', 'roles', 'tickets', 'reports', 'settings'] },
    'Director': { modules: ['dashboard', 'profile', 'schedule', 'roles', 'reports'] },
    'Cashier': { modules: ['dashboard', 'profile', 'schedule', 'tickets'] },
    'Actor': { modules: ['dashboard', 'profile', 'schedule', 'roles'] },
    'Musician': { modules: ['dashboard', 'profile', 'schedule'] },
    'Staff': { modules: ['dashboard', 'profile', 'schedule'] }
};

// Получить текущую роль
function getCurrentRole() {
    const roleElement = document.querySelector('[data-user-role]');
    return roleElement?.getAttribute('data-user-role') || 'Staff';
}

// Навигация по разделам (без скрытия пунктов!)
function setupNavigation() {
    const navLinks = document.querySelectorAll('.sidebar-nav a');

    // Добавляем атрибут data-user-role к body для определения роли (опционально)
    if (!document.querySelector('[data-user-role]')) {
        const role = getCurrentRole();
        document.body.setAttribute('data-user-role', role);
    }

    navLinks.forEach(link => {
        // Не скрываем пункты меню, просто добавляем обработчики для якорных ссылок
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');

            // Если это якорная ссылка (начинается с #) - обрабатываем
            if (href && href.startsWith('#')) {
                e.preventDefault();

                // Убираем active со всех ссылок
                navLinks.forEach(l => l.classList.remove('active'));
                this.classList.add('active');

                // Показываем соответствующую секцию
                const section = this.dataset.section;
                if (section) {
                    document.querySelectorAll('.content-section').forEach(sectionEl => {
                        sectionEl.classList.remove('active');
                    });
                    const targetSection = document.getElementById(section + '-section');
                    if (targetSection) targetSection.classList.add('active');
                }
            }
            // Для обычных ссылок (не якорных) - ничего не делаем, пусть идут по href
        });
    });

    // Активируем первую секцию, если есть якорная ссылка active
    const activeLink = document.querySelector('.sidebar-nav a.active');
    if (activeLink && activeLink.getAttribute('href')?.startsWith('#')) {
        const section = activeLink.dataset.section;
        if (section) {
            const targetSection = document.getElementById(section + '-section');
            if (targetSection) targetSection.classList.add('active');
        }
    }
}

// Выход из системы
function setupLogout() {
    const logoutBtns = document.querySelectorAll('.logout-btn, .logout-header-btn');
    logoutBtns.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            if (confirm('Вы уверены, что хотите выйти?')) {
                window.location.href = '/Account/Logout';
            }
        });
    });
}

// Создание звёздного неба
function createStars() {
    const container = document.getElementById('stars');
    if (!container) return;
    container.innerHTML = '';
    for (let i = 0; i < 150; i++) {
        const star = document.createElement('div');
        star.classList.add('star');
        star.style.width = Math.random() * 3 + 'px';
        star.style.height = star.style.width;
        star.style.left = Math.random() * 100 + '%';
        star.style.top = Math.random() * 100 + '%';
        star.style.animationDelay = Math.random() * 5 + 's';
        container.appendChild(star);
    }
}

// Инициализация дашборда
document.addEventListener('DOMContentLoaded', function () {
    setupNavigation();
    setupLogout();
    createStars();
});