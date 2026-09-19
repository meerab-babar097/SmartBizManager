(function () {
    const products = JSON.parse(document.getElementById('products-data').textContent || '[]');
    const tbody = document.getElementById('items-body');
    const rowTemplate = document.getElementById('row-template');

    function money(n) { return 'Rs ' + Number(n || 0).toLocaleString(); }

    function buildProductOptions(select) {
        let html = '<option value="">-- select product --</option>';
        products.forEach(p => {
            html += `<option value="${p.id}" data-price="${p.price}" data-stock="${p.stock}">${p.name} (Rs ${p.price}, ${p.stock} left)</option>`;
        });
        select.innerHTML = html;
    }

    function reindexRows() {
        const rows = tbody.querySelectorAll('.item-row');
        rows.forEach((row, i) => {
            row.querySelector('.product-select').name = `Items[${i}].ProductId`;
            row.querySelector('.qty-input').name = `Items[${i}].Quantity`;
        });
    }

    function recalcRow(row) {
        const select = row.querySelector('.product-select');
        const qtyInput = row.querySelector('.qty-input');
        const selected = select.options[select.selectedIndex];
        const price = selected ? Number(selected.dataset.price || 0) : 0;
        const stock = selected ? Number(selected.dataset.stock || 0) : 0;
        let qty = Number(qtyInput.value || 0);

        // Client-side cap, purely for a responsive UI - the server re-checks
        // stock for real inside the transaction, so this is not the security boundary.
        if (qty > stock) { qty = stock; qtyInput.value = stock; }

        row.querySelector('.price-display').textContent = money(price);
        row.querySelector('.line-total-display').textContent = money(price * qty);
        recalcTotals();
    }

    function recalcTotals() {
        let subtotal = 0;
        tbody.querySelectorAll('.item-row').forEach(row => {
            const select = row.querySelector('.product-select');
            const selected = select.options[select.selectedIndex];
            const price = selected ? Number(selected.dataset.price || 0) : 0;
            const qty = Number(row.querySelector('.qty-input').value || 0);
            subtotal += price * qty;
        });

        const discount = Number(document.getElementById('discount-input').value || 0);
        const total = Math.max(subtotal - discount, 0);
        const paid = Number(document.getElementById('paid-input').value || 0);

        document.getElementById('subtotal-display').textContent = money(subtotal);
        document.getElementById('total-display').textContent = money(total);
        document.getElementById('remaining-display').textContent = money(total - paid);
    }

    function addRow() {
        const clone = rowTemplate.content.cloneNode(true);
        const row = clone.querySelector('.item-row');
        buildProductOptions(row.querySelector('.product-select'));

        row.querySelector('.product-select').addEventListener('change', () => recalcRow(row));
        row.querySelector('.qty-input').addEventListener('input', () => recalcRow(row));
        row.querySelector('.remove-row').addEventListener('click', () => {
            row.remove();
            reindexRows();
            recalcTotals();
        });

        tbody.appendChild(row);
        reindexRows();
    }

    document.getElementById('add-row').addEventListener('click', addRow);
    document.getElementById('discount-input').addEventListener('input', recalcTotals);
    document.getElementById('paid-input').addEventListener('input', recalcTotals);

    addRow(); // start with one empty row so the form isn't blank
})();