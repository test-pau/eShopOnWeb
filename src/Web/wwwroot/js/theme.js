// Theme management
(function() {
    'use strict';

    const THEME_KEY = 'theme-preference';
    const THEME_ATTR = 'data-theme';
    
    const Themes = {
        LIGHT: 'light',
        DARK: 'dark',
        AUTO: 'auto'
    };

    function getSystemTheme() {
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches 
            ? Themes.DARK 
            : Themes.LIGHT;
    }

    function getStoredTheme() {
        return localStorage.getItem(THEME_KEY) || Themes.AUTO;
    }

    function getEffectiveTheme(preference) {
        if (preference === Themes.AUTO) {
            return getSystemTheme();
        }
        return preference;
    }

    function applyTheme(theme) {
        const effectiveTheme = getEffectiveTheme(theme);
        if (effectiveTheme === Themes.DARK) {
            document.documentElement.setAttribute(THEME_ATTR, Themes.DARK);
        } else {
            document.documentElement.removeAttribute(THEME_ATTR);
        }
    }

    function setTheme(preference) {
        localStorage.setItem(THEME_KEY, preference);
        applyTheme(preference);
        updateToggleButton(preference);
    }

    function cycleTheme() {
        const current = getStoredTheme();
        let next;
        
        switch(current) {
            case Themes.LIGHT:
                next = Themes.DARK;
                break;
            case Themes.DARK:
                next = Themes.AUTO;
                break;
            case Themes.AUTO:
            default:
                next = Themes.LIGHT;
                break;
        }
        
        setTheme(next);
    }

    function getThemeLabel(preference) {
        const effectiveTheme = getEffectiveTheme(preference);
        const labels = {
            [Themes.LIGHT]: '☀️ Light',
            [Themes.DARK]: '🌙 Dark',
            [Themes.AUTO]: '🔄 Auto'
        };
        
        if (preference === Themes.AUTO) {
            return `${labels[preference]} (${effectiveTheme === Themes.DARK ? 'Dark' : 'Light'})`;
        }
        
        return labels[preference] || labels[Themes.AUTO];
    }

    function updateToggleButton(preference) {
        const button = document.getElementById('theme-toggle');
        if (button) {
            button.textContent = getThemeLabel(preference);
            button.setAttribute('aria-label', `Current theme: ${preference}. Click to cycle themes.`);
        }
    }

    function initTheme() {
        const savedTheme = getStoredTheme();
        applyTheme(savedTheme);
        
        // Update button when DOM is ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                updateToggleButton(savedTheme);
            });
        } else {
            updateToggleButton(savedTheme);
        }
        
        // Listen for system theme changes
        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function() {
                const current = getStoredTheme();
                if (current === Themes.AUTO) {
                    applyTheme(Themes.AUTO);
                    updateToggleButton(Themes.AUTO);
                }
            });
        }
    }

    // Expose functions globally
    window.themeManager = {
        cycleTheme: cycleTheme,
        setTheme: setTheme,
        getStoredTheme: getStoredTheme,
        init: initTheme
    };

    // Initialize immediately to prevent flash
    initTheme();
})();
