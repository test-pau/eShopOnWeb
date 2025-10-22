# Dark Theme Feature

This document describes the dark theme implementation for eShopOnWeb.

## Overview

The application now supports three theme modes:
- **Light Theme**: Traditional light background with dark text
- **Dark Theme**: Dark background with light text for reduced eye strain
- **Auto Mode**: Automatically follows the user's browser/system theme preference

## User Interface

A theme toggle button is located in the header next to the eShop logo. Clicking the button cycles through the three theme options:
1. Light (☀️ Light)
2. Dark (🌙 Dark)
3. Auto (🔄 Auto)

## Technical Implementation

### Files Modified
- `/src/Web/Views/Shared/_Layout.cshtml` - Added theme toggle button and script references
- `/src/Web/wwwroot/css/themes.css` - CSS custom properties for theme colors
- `/src/Web/wwwroot/js/theme.js` - Theme management logic

### How It Works

1. **CSS Custom Properties**: The theme uses CSS custom properties (CSS variables) to define colors for both light and dark themes.

2. **JavaScript Theme Manager**: The `theme.js` file provides:
   - Theme detection (system preference)
   - Theme persistence (localStorage)
   - Theme cycling logic
   - Automatic updates when system theme changes (in Auto mode)

3. **Data Attribute**: The theme is applied by setting/removing the `data-theme="dark"` attribute on the `<html>` element.

## Theme Persistence

The user's theme preference is stored in the browser's localStorage and persists across:
- Page navigation
- Browser sessions
- Page reloads

## Browser Compatibility

The theme works in all modern browsers that support:
- CSS Custom Properties
- localStorage
- matchMedia API (for system theme detection)

## Testing

Playwright tests have been added to verify:
- Theme toggle functionality
- Theme persistence across navigation
- All application features work with both themes
- Theme preference is saved and restored

See `/tests/PlaywrightTests/ThemeTests.cs` for test implementation.

## Screenshots

### Light Theme
The default light theme provides a bright, clean interface:

![Light Theme](https://github.com/user-attachments/assets/c8f23954-0fb6-4853-be2c-feabd7e5e4fb)

### Dark Theme
The dark theme reduces eye strain with a darker color scheme:

![Dark Theme](https://github.com/user-attachments/assets/d6dfc21b-ca71-40c4-a340-4e44d9f1dab0)
