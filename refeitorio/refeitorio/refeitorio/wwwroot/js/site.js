// POST AJAX com JSON
function ajaxPost(url, data) {
    return fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    }).then(r => r.json());
}

function toggleFavorite(productId, button) {
    const formData = new FormData();
    formData.append('productId', productId);
    
    fetch('/Favorites/Toggle', {
        method: 'POST',
        headers: { 
            'X-Requested-With': 'XMLHttpRequest',
            'Accept': 'application/json'
        },
        body: formData
    })
    .then(r => r.json())
    .then(data => {
        button.classList.toggle('btn-danger', data.isFavorite);
        button.classList.toggle('btn-outline-danger', !data.isFavorite);
        button.innerHTML = data.isFavorite ? '<span>❤️</span>' : '<span>🤍</span>';
    })
    .catch(error => {
        console.error('Error toggling favorite:', error);
        showAlert('danger', 'Erro ao atualizar favorito. Tente novamente.');
    });
}

function addToCart(productId, button) {
    const formData = new FormData();
    formData.append('productId', productId);
    
    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: { 
            'X-Requested-With': 'XMLHttpRequest',
            'Accept': 'application/json'
        },
        body: formData
    })
    .then(r => r.json())
    .then(data => {
        updateCartCounter(data.cartCount);
        button.disabled = true;
        button.textContent = 'Adicionado!';
        setTimeout(() => {
            button.disabled = false;
            button.textContent = 'Adicionar novamente';
        }, 1000);
    })
    .catch(error => {
        console.error('Error adding to cart:', error);
        showAlert('danger', 'Erro ao adicionar ao carrinho. Tente novamente.');
        button.disabled = false;
    });
}

function updateQuantity(productId, change, price) {
    ajaxPost('/Cart/UpdateQuantity', { productId, change })
    .then(data => {
        const quantityInput = document.querySelector(`#quantity-${productId}`);
        if (quantityInput) quantityInput.value = data.itemQuantity;
        
        const subtotal = document.querySelector(`#subtotal-${productId}`);
        if (subtotal) subtotal.textContent = '€' + (price * data.itemQuantity).toFixed(2);
        
        updateCartCounter(data.cartCount);
        updateCartTotal(data.cartTotal);
        
        if (data.itemRemoved) {
            const row = document.querySelector(`#cart-row-${productId}`);
            if (row) row.remove();
            if (data.cartCount === 0) location.reload();
        }
    })
    .catch(error => {
        console.error('Error updating quantity:', error);
        showAlert('danger', 'Erro ao atualizar quantidade. Tente novamente.');
    });
}

function updateCartCounter(count) {
    const cartCounter = document.querySelector('#cart-counter');
    if (cartCounter) {
        cartCounter.textContent = count;
        cartCounter.style.display = count > 0 ? 'inline' : 'none';
    }
}

function updateCartTotal(total) {
    ['#cart-total', '#cart-total-main'].forEach(id => {
        const el = document.querySelector(id);
        if (el) el.textContent = '€' + total.toFixed(2);
    });
}

function updateBalance(balance) {
    const balanceEl = document.querySelector('#user-balance');
    if (balanceEl) balanceEl.textContent = '€' + balance.toFixed(2);
}

// Handler genérico para formulários com class="ajax-form"
function handleAjaxForm(form, button) {
    const formData = new FormData(form);
    const url = form.action;
    const originalText = button.innerHTML;
    
    button.disabled = true;
    button.innerHTML = 'Aguarde...';
    
    fetch(url, {
        method: 'POST',
        headers: { 
            'X-Requested-With': 'XMLHttpRequest',
            'Accept': 'application/json'
        },
        body: formData
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            if (data.message) {
                showAlert('success', data.message);
            }
            
            if (data.cartCount !== undefined) {
                updateCartCounter(data.cartCount);
            }
            
            if (data.cartTotal !== undefined) {
                updateCartTotal(data.cartTotal);
            }
            
            if (data.balance !== undefined) {
                updateBalance(data.balance);
            }
            
            if (data.redirect) {
                setTimeout(() => window.location.href = data.redirect, 1000);
                return;
            }
            
            if (data.reload) {
                location.reload();
                return;
            }
            
            if (data.removeElement) {
                const el = document.querySelector(data.removeElement);
                if (el) el.remove();
            }
            
            if (data.updateElement) {
                const el = document.querySelector(data.updateElement.selector);
                if (el) el.innerHTML = data.updateElement.content;
            }
        } else {
            showAlert('danger', data.message || 'Erro ao processar pedido');
        }
        
        button.disabled = false;
        button.innerHTML = originalText;
    })
    .catch(error => {
        console.error('AJAX Error:', error);
        showAlert('danger', 'Erro ao processar pedido. Tente novamente.');
        button.disabled = false;
        button.innerHTML = originalText;
    });
}

// Mostra alerta no topo da página
function showAlert(type, message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const container = document.querySelector('.container') || document.body;
    container.insertBefore(alertDiv, container.firstChild);
    
    setTimeout(() => alertDiv.remove(), 5000);
}

// Inicialização
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.ajax-form-favorite').forEach(form =>
        form.addEventListener('submit', e => {
            e.preventDefault();
            const btn = form.querySelector('.btn-favorite');
            toggleFavorite(+btn.dataset.productId, btn);
        })
    );
    
    document.querySelectorAll('.ajax-form-cart').forEach(form =>
        form.addEventListener('submit', e => {
            e.preventDefault();
            const btn = form.querySelector('.btn-add-cart');
            addToCart(+btn.dataset.productId, btn);
        })
    );
    
    document.querySelectorAll('.btn-quantity-change').forEach(btn =>
        btn.addEventListener('click', e => {
            e.preventDefault();
            updateQuantity(+btn.dataset.productId, +btn.dataset.change, +btn.dataset.price);
        })
    );
    
    document.querySelectorAll('.ajax-form').forEach(form =>
        form.addEventListener('submit', e => {
            e.preventDefault();
            const btn = form.querySelector('button[type="submit"]');
            handleAjaxForm(form, btn);
        })
    );
});
