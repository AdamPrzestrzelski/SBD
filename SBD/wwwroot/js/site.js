// Funkcja przełączania zakładek w panelu finansowym
function switchFinTab(tabId) {
    // Ukryj wszystkie zawartości zakładek
    const contents = document.querySelectorAll('.tab-content');
    contents.forEach(content => {
        content.style.display = 'none';
    });

    // Usuń klasę active ze wszystkich przycisków
    const buttons = document.querySelectorAll('.tab-btn');
    buttons.forEach(btn => {
        btn.classList.remove('active');
    });

    // Pokaż wybraną zakładkę i aktywuj przycisk
    const activeContent = document.getElementById(tabId);
    if (activeContent) {
        activeContent.style.display = 'block';
    }

    // Znajdź przycisk, który wywołał zdarzenie i dodaj klasę active
    const eventBtn = event.currentTarget;
    if (eventBtn) {
        eventBtn.classList.add('active');
    }
}

// Funkcja wyszukiwania w tabeli (globalny filtr)
function filterTableBySearch() {
    const input = document.getElementById('global-search-input');
    const filter = input.value.toUpperCase();
    
    // Znajdź aktywną/widoczną tabelę w kontenerze głównym
    const tables = document.querySelectorAll('.table');
    
    tables.forEach(table => {
        // Pomiń tabele, które są ukryte (np. w nieaktywnych zakładkach)
        let parent = table.parentElement;
        let isHidden = false;
        while (parent && parent.tagName !== 'BODY') {
            if (parent.style.display === 'none') {
                isHidden = true;
                break;
            }
            parent = parent.parentElement;
        }
        
        if (isHidden) return;

        const trs = table.getElementsByTagName('tr');
        
        // Pętla od 1, aby pominąć nagłówek (th)
        for (let i = 1; i < trs.length; i++) {
            const tr = trs[i];
            
            // Pomiń wiersze formularzy rozszerzeń
            if (tr.classList.contains('extension-form-row')) continue;
            
            let matchFound = false;
            const tds = tr.getElementsByTagName('td');
            
            for (let j = 0; j < tds.length; j++) {
                const td = tds[j];
                if (td) {
                    const textValue = td.textContent || td.innerText;
                    if (textValue.toUpperCase().indexOf(filter) > -1) {
                        matchFound = true;
                        break;
                    }
                }
            }
            
            if (matchFound) {
                tr.style.display = '';
            } else {
                tr.style.display = 'none';
            }
        }
    });
}

// Funkcja pokazywania/ukrywania formularza przedłużenia wypożyczenia
function toggleExtensionForm(rentalId) {
    const formRow = document.getElementById('extension-form-' + rentalId);
    if (formRow) {
        if (formRow.style.display === 'none' || formRow.style.display === '') {
            formRow.style.display = 'table-row';
            formRow.style.animation = 'slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards';
        } else {
            formRow.style.display = 'none';
        }
    }
}

// Automatyczne znikanie powiadomień po 8 sekundach
document.addEventListener("DOMContentLoaded", function() {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s ease-out, transform 0.5s ease-out';
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                alert.remove();
            }, 500);
        }, 8000);
    });
});
