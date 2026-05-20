// Конфигурация ролей
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

// Навигация по разделам
function setupNavigation() {
    const navLinks = document.querySelectorAll('.sidebar-nav a');
    const role = getCurrentRole();
    const config = roleConfig[role] || roleConfig['Staff'];
    const allowedModules = config.modules;

    navLinks.forEach(link => {
        const section = link.dataset.section;
        if (section && allowedModules.includes(section)) {
            link.style.display = 'flex';
        } else {
            link.style.display = 'none';
        }

        link.addEventListener('click', function (e) {
            e.preventDefault();

            navLinks.forEach(l => l.classList.remove('active'));
            this.classList.add('active');

            const sectionId = this.dataset.section + '-section';
            document.querySelectorAll('.content-section').forEach(section => {
                section.classList.remove('active');
            });
            const targetSection = document.getElementById(sectionId);
            if (targetSection) targetSection.classList.add('active');
        });
    });
}

// Выход из системы
function setupLogout() {
    const logoutBtns = document.querySelectorAll('.logout-btn');
    logoutBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            if (confirm('Вы уверены, что хотите выйти?')) {
                window.location.href = '/Account/Logout';
            }
        });
    });
}

// Инициализация дашборда
document.addEventListener('DOMContentLoaded', function () {
    setupNavigation();
    setupLogout();

    // Создаём звёзды на странице дашборда
    if (typeof createStars === 'function') {
        createStars();
    }
});