// Создание звездного неба
function createStars() {
    const starsContainer = document.getElementById('stars');
    if (!starsContainer) return;

    starsContainer.innerHTML = '';
    const starsCount = 200;

    for (let i = 0; i < starsCount; i++) {
        const star = document.createElement('div');
        star.classList.add('star');

        const size = Math.random() * 3;
        const left = Math.random() * 100;
        const top = Math.random() * 100;
        const delay = Math.random() * 5;

        star.style.width = `${size}px`;
        star.style.height = `${size}px`;
        star.style.left = `${left}%`;
        star.style.top = `${top}%`;
        star.style.animationDelay = `${delay}s`;

        starsContainer.appendChild(star);
    }
}

// Загрузка спектаклей из БД
function loadPlays() {
    const container = document.getElementById('plays-container');
    if (!container) return;

    container.innerHTML = '<div class="loading">Загрузка спектаклей...</div>';

    fetch('/api/plays/upcoming')
        .then(response => response.json())
        .then(data => {
            if (!data || data.length === 0) {
                container.innerHTML = '<p style="text-align:center;">Ближайших спектаклей нет</p>';
                return;
            }

            container.innerHTML = data.map(play => {
                const title = escapeHtml(play.title || 'Без названия');
                const description = play.description ? escapeHtml(play.description.substring(0, 80)) : '';
                const authorName = play.author?.fio ? escapeHtml(play.author.fio) : 'Неизвестен';
                const genreName = play.genre?.name ? escapeHtml(play.genre.name) : 'Не указан';
                const performanceId = play.performanceId;

                return `
                    <div class="play-card">
                        <div class="play-image">${title}</div>
                        <div class="play-info">
                            <h3>${title}</h3>
                            <p>${description}...</p>
                            <p><strong>Автор:</strong> ${authorName}</p>
                            <p><strong>Жанр:</strong> ${genreName}</p>
                            <a href="/Tickets/Purchase?performanceId=${performanceId}" class="play-button">Купить билет</a>
                        </div>
                    </div>
                `;
            }).join('');
        })
        .catch(error => {
            console.error('Ошибка загрузки спектаклей:', error);
            container.innerHTML = '<p style="text-align:center;">Ошибка загрузки афиши. Попробуйте позже.</p>';
        });
}

// Отправка кода подтверждения
function sendVerificationCode(email) {
    return fetch('/Account/SendVerificationCode', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: email })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(`Ваш код подтверждения: ${data.code}\nВведите его в поле "Код подтверждения".`);
                return true;
            } else {
                alert(data.message || 'Ошибка отправки кода');
                return false;
            }
        })
        .catch(error => {
            console.error('Ошибка:', error);
            alert('Ошибка отправки кода');
            return false;
        });
}

// Защита от XSS
function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

// Плавная прокрутка
function setupSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const targetId = this.getAttribute('href');
            if (targetId === '#') return;

            const targetElement = document.querySelector(targetId);
            if (targetElement && !this.classList.contains('play-button')) {
                e.preventDefault();
                const headerHeight = document.querySelector('header')?.offsetHeight || 80;
                window.scrollTo({
                    top: targetElement.offsetTop - headerHeight - 20,
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Инициализация
document.addEventListener('DOMContentLoaded', function () {
    createStars();
    loadPlays();
    setupSmoothScroll();
});