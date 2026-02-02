console.log("rich-text-editor.js loaded");

let editorInstances = new Map();
let savedSelection = null;

export function initializeEditor(element, dotNetRef) {
    console.log("initializeEditor called", element);
    if (!element) {
        console.error("Element is null!");
        return;
    }

    editorInstances.set(element, { dotNetRef });

    // Track if Shift+Tab was pressed to navigate backwards
    let shiftTabPressed = false;

    element.addEventListener('keydown', (e) => {
        // Prevent default for shortcuts we handle
        if ((e.ctrlKey || e.metaKey) && ['b', 'i', 'u', 'z', 'y', 'k'].includes(e.key.toLowerCase())) {
            e.preventDefault();
        }

        if (e.key === 'Tab' && e.shiftKey) {
            shiftTabPressed = true;
        }
        
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            e.stopPropagation();
            console.log("Ctrl+K intercepted");
        }
    }, true);

    // When editor gains focus via Shift+Tab, focus the last toolbar button
    element.addEventListener('focus', (e) => {
        if (shiftTabPressed) {
            shiftTabPressed = false;
            const toolbar = document.querySelector('.rte-toolbar');
            if (toolbar) {
                const toolbarItems = toolbar.querySelectorAll('[data-toolbar-item]');
                if (toolbarItems.length > 0) {
                    e.preventDefault();
                    toolbarItems[toolbarItems.length - 1].focus();
                }
            }
        }
    });

    // Save selection when toolbar is clicked
    element.addEventListener('blur', () => {
        saveSelection();
    });

    element.addEventListener('paste', (e) => {
        e.preventDefault();
        const text = (e.clipboardData || window.clipboardData).getData('text/plain');
        
        if (document.queryCommandSupported('insertText')) {
            document.execCommand('insertText', false, text);
        } else {
            const selection = window.getSelection();
            if (selection.rangeCount) {
                const range = selection.getRangeAt(0);
                range.deleteContents();
                range.insertNode(document.createTextNode(text));
            }
        }
    });

    element.addEventListener('drop', (e) => {
        e.preventDefault();
    });

    // Prevent left/right arrows from changing select value in toolbar
    const toolbar = document.querySelector('.rte-toolbar');
    if (toolbar) {
        toolbar.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
                e.preventDefault();
            }
        }, true);
    }
    
    console.log("Editor initialized successfully");
}

export function disposeEditor(element) {
    if (element && editorInstances.has(element)) {
        editorInstances.delete(element);
    }
}

export function saveSelection() {
    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
        savedSelection = selection.getRangeAt(0).cloneRange();
    }
}

export function restoreSelection() {
    if (savedSelection) {
        const sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(savedSelection);
        console.log("Selection restored");
    }
}

export function executeCommand(command, value = null) {
    console.log("executeCommand:", command);
    restoreSelection();

    // Special handling for horizontal rule
    if (command === 'insertHorizontalRule') {
        insertHorizontalRuleWithParagraph();
        return;
    }

    // NEW: Handle subscript/superscript mutual exclusivity
    if (command === 'subscript') {
        // If superscript is active, turn it off first
        if (document.queryCommandState('superscript')) {
            document.execCommand('superscript', false, null);
        }
    } else if (command === 'superscript') {
        // If subscript is active, turn it off first
        if (document.queryCommandState('subscript')) {
            document.execCommand('subscript', false, null);
        }
    }

    document.execCommand(command, false, value);
}

export function executeFormatBlock(blockType) {
    console.log("executeFormatBlock:", blockType);
    restoreSelection();
    document.execCommand('formatBlock', false, blockType);
}

export function executeFontSize(size) {
    console.log("executeFontSize:", size);
    restoreSelection();
    document.execCommand('fontSize', false, size);
}

export function executeFontName(fontName) {
    console.log("executeFontName:", fontName);
    restoreSelection();
    document.execCommand('fontName', false, fontName);
}

export function executeForeColor(color) {
    console.log("executeForeColor:", color);
    restoreSelection();
    document.execCommand('foreColor', false, color);
}
 
export function executeBackColor(color) {
    console.log("executeBackColor:", color);
    restoreSelection();
    document.execCommand('backColor', false, color);
}

export function getHtml(element) {
    const html = element ? element.innerHTML : '';
    console.log("getHtml returning:", html);
    return html;
}

export function setHtml(element, html) {
    console.log("setHtml:", html);
    if (element) {
        element.innerHTML = html;
    }
}

export function focusEditor(element) {
    element.focus();
    
    // Restore the previously saved selection
    if (savedSelection) {
        try {
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(savedSelection);
        } catch (e) {
            console.warn('Could not restore selection:', e);
        }
    }
}

export function getSelectedText() {
    restoreSelection();
    const selection = window.getSelection();
    return selection ? selection.toString() : '';
}

export function getActiveFormats() {
    const formats = [];
    
    // Check if cursor is inside a link first
    const insideLink = isInsideLink();
    if (insideLink) formats.push('link');
    
    if (document.queryCommandState('bold')) formats.push('bold');
    if (document.queryCommandState('italic')) formats.push('italic');
    
    // Only report underline if NOT inside a link
    if (document.queryCommandState('underline') && !insideLink) {
        formats.push('underline');
    }
    
    if (document.queryCommandState('strikeThrough')) formats.push('strikeThrough');
    
    // FIXED: Always check DOM for subscript/superscript (queryCommandState is unreliable)
    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
        let node = selection.getRangeAt(0).startContainer;
        
        // Walk up the DOM tree to check for sub/sup tags
        while (node && node !== document.body) {
            if (node.nodeType === Node.ELEMENT_NODE) {
                const tagName = node.tagName?.toLowerCase();
                if (tagName === 'sub') {
                    formats.push('subscript');
                    console.log('✓ Subscript detected via DOM'); // DEBUG
                }
                if (tagName === 'sup') {
                    formats.push('superscript');
                    console.log('✓ Superscript detected via DOM'); // DEBUG
                }
            }
            node = node.parentNode;
        }
    }
    
    if (document.queryCommandState('insertUnorderedList')) formats.push('insertUnorderedList');
    if (document.queryCommandState('insertOrderedList')) formats.push('insertOrderedList');
    
    // Check if text has a foreground color applied (not black, not inside link)
    const foreColor = document.queryCommandValue('foreColor');
    if (foreColor && !isDefaultTextColor(foreColor) && !insideLink) {
        formats.push('foreColor');
    }
    
    // Check if text has a background/highlight color applied
    const backColor = document.queryCommandValue('backColor');
    if (backColor && !isDefaultBackgroundColor(backColor)) {
        formats.push('backColor');
    }
    
    // Alignment - only report ONE
    if (document.queryCommandState('justifyCenter')) {
        formats.push('justifyCenter');
    } else if (document.queryCommandState('justifyRight')) {
        formats.push('justifyRight');
    } else if (document.queryCommandState('justifyFull')) {
        formats.push('justifyFull');
    } else if (document.queryCommandState('justifyLeft')) {
        formats.push('justifyLeft');
    }
    
    console.log('Active formats:', formats); // DEBUG
    return formats;
}

// Helper function to check if cursor is inside a link
function isInsideLink() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return false;
    
    let node = selection.getRangeAt(0).startContainer;
    while (node && node !== document.body) {
        if (node.nodeType === Node.ELEMENT_NODE && node.tagName === 'A') {
            return true;
        }
        node = node.parentNode;
    }
    return false;
}

export function getCurrentBlock() {
    const selection = window.getSelection();
    if (selection.rangeCount === 0) return '';
    
    let node = selection.getRangeAt(0).startContainer;
    
    while (node && node !== document.body) {
        if (node.nodeType === Node.ELEMENT_NODE) {
            const tagName = node.tagName.toLowerCase();
            if (['h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p'].includes(tagName)) {
                return tagName;
            }
        }
        node = node.parentNode;
    }
    
    return '';
}

export function createLink(url) {
    restoreSelection();
    const selection = window.getSelection();
    if (selection.rangeCount > 0 && selection.toString().length > 0) {
        document.execCommand('createLink', false, url);
    }
}

export function insertHorizontalRuleWithParagraph() {
    restoreSelection();

    const selection = window.getSelection();
    if (!selection.rangeCount) {
        console.log("No selection for HR insert");
        return;
    }

    // Use insertHTML command so it integrates with undo stack properly
    // Insert HR followed by a paragraph with a zero-width space for cursor placement
    const htmlToInsert = '<hr><p><br></p>';

    document.execCommand('insertHTML', false, htmlToInsert);

    console.log("Horizontal rule inserted with paragraph after");
}

export function adjustColorPalettePosition() {
    const dropdowns = document.querySelectorAll('.rte-color-dropdown');
    
    dropdowns.forEach((dropdown) => {
        const palette = dropdown.querySelector('.rte-color-palette');
        if (!palette) return;
        
        palette.classList.remove('align-left', 'align-right');
        
        const button = dropdown.querySelector('.rte-btn');
        const buttonRect = button.getBoundingClientRect();
        const rteContainer = button.closest('.rich-text-editor');
        const containerRect = rteContainer ? rteContainer.getBoundingClientRect() : null;
        
        const paletteWidth = 198;
        const paletteHalfWidth = paletteWidth / 2;
        const buttonCenter = buttonRect.left + (buttonRect.width / 2);
        const paletteLeftIfCentered = buttonCenter - paletteHalfWidth;
        const paletteRightIfCentered = buttonCenter + paletteHalfWidth;
        const marginFromEdge = 10;
        
        if (containerRect) {
            if (paletteRightIfCentered > containerRect.right - marginFromEdge) {
                palette.classList.add('align-right');
            }
            else if (paletteLeftIfCentered < containerRect.left + marginFromEdge) {
                palette.classList.add('align-left');
            }
        } else {
            // Fallback to viewport if container not found
            const viewportWidth = window.innerWidth;
            if (paletteRightIfCentered > viewportWidth - 50) {
                palette.classList.add('align-right');
            }
            else if (paletteLeftIfCentered < 50) {
                palette.classList.add('align-left');
            }
        }
    });
}

// Helper to check if color is default/black text
function isDefaultTextColor(color) {
    if (!color) return true;
    const c = color.toLowerCase().replace(/\s/g, ''); // Remove all whitespace
    
    // Check for various black representations
    return c === 'rgb(0,0,0)' || 
           c === '#000000' || 
           c === '#000' || 
           c === 'black' ||
           c === '' ||
           // Also check for very dark colors that are essentially black
           c === 'rgb(17,24,39)' ||  // Tailwind gray-900
           c === 'rgb(31,41,55)' ||  // Tailwind gray-800
           c === 'rgb(55,65,81)' ||  // Tailwind gray-700
           c === 'rgb(0,0,0,1)' ||   // With alpha
           c === 'rgba(0,0,0,1)';
}

// Helper to check if background is default/transparent/white
function isDefaultBackgroundColor(color) {
    if (!color) return true;
    const c = color.toLowerCase().replace(/\s/g, ''); // Remove all whitespace
    
    return c === 'rgba(0,0,0,0)' || 
           c === 'transparent' || 
           c === 'rgb(255,255,255)' || 
           c === '#ffffff' || 
           c === '#fff' || 
           c === 'white' ||
           c === '' ||
           c === 'initial' ||
           c === 'inherit';
}

export function focusToolbarButton(index) {
    const toolbar = document.querySelector('.rte-toolbar');
    if (!toolbar) return;
    
    // Only select elements marked as toolbar items (excludes picker buttons)
    const focusableElements = toolbar.querySelectorAll('[data-toolbar-item]');
    
    if (index >= 0 && index < focusableElements.length) {
        focusableElements[index].focus();
    }
}

export function getToolbarFocusableCount() {
    const toolbar = document.querySelector('.rte-toolbar');
    if (!toolbar) return 0;
    
    return toolbar.querySelectorAll('button, select').length;
}

// NEW: Accessibility helper for picker navigation
export function setupPickerNavigation(palette, trigger, dotNetRef) {
    if (!palette) return;

    // Find all focusable options (buttons with role="option")
    const items = palette.querySelectorAll('[role="option"]');
    if (items.length === 0) return;

    // Focus the first item immediately
    setTimeout(() => items[0].focus(), 10);

    palette.addEventListener('keydown', (e) => {
        const active = document.activeElement;
        // Convert NodeList to Array to find index
        const index = Array.from(items).indexOf(active);

        // Escape: Close picker and return focus to trigger
        if (e.key === 'Escape') {
            e.preventDefault();
            e.stopPropagation();
            dotNetRef.invokeMethodAsync('CloseActivePickers');
            if (trigger) trigger.focus();
        }
        // Arrows: Navigate grid/list
        else if (e.key === 'ArrowDown' || e.key === 'ArrowRight') {
            e.preventDefault();
            const next = (index + 1) % items.length;
            items[next].focus();
        }
        else if (e.key === 'ArrowUp' || e.key === 'ArrowLeft') {
            e.preventDefault();
            const prev = (index - 1 + items.length) % items.length;
            items[prev].focus();
        }
        // Tab: Trap focus inside popup
        else if (e.key === 'Tab') {
            e.preventDefault();
            if (e.shiftKey) {
                const prev = (index - 1 + items.length) % items.length;
                items[prev].focus();
            } else {
                const next = (index + 1) % items.length;
                items[next].focus();
            }
        }
        // Home: First item
        else if (e.key === 'Home') {
            e.preventDefault();
            items[0].focus();
        }
        // End: Last item
        else if (e.key === 'End') {
            e.preventDefault();
            items[items.length - 1].focus();
        }
    });
}

/**
 * Setup keyboard navigation for color picker grids
 * @param {HTMLElement} palette - The color palette container
 * @param {number} columns - Number of columns in the grid
 * @param {HTMLElement} triggerButton - The button that opened the picker
 */
export function setupColorPickerNavigation(palette, columns, triggerButton) {
    if (!palette) return;

    const colorButtons = Array.from(palette.querySelectorAll('.rte-palette-color'));
    if (colorButtons.length === 0) return;

    // Focus the first color button when palette opens
    setTimeout(() => {
        if (colorButtons[0]) {
            colorButtons[0].focus();
        }
    }, 50);

    // Attach keyboard handler directly to palette
    const keydownHandler = (event) => {
        const key = event.key;
        if (!['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Escape'].includes(key)) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        const buttons = Array.from(palette.querySelectorAll('.rte-palette-color'));
        const currentIndex = buttons.indexOf(event.target);
        if (currentIndex === -1) return;  // ← FIXED: Added opening parenthesis

        let newIndex = currentIndex;
        const rows = Math.ceil(buttons.length / columns);

        switch (key) {
            case 'ArrowRight':
                newIndex = (currentIndex + 1) % buttons.length;
                break;
            case 'ArrowLeft':
                newIndex = (currentIndex - 1 + buttons.length) % buttons.length;
                break;
            case 'ArrowDown':
                newIndex = currentIndex + columns;
                if (newIndex >= buttons.length) {
                    // Wrap to top of same column
                    newIndex = currentIndex % columns;
                }
                break;
            case 'ArrowUp':
                newIndex = currentIndex - columns;
                if (newIndex < 0) {
                    // Wrap to bottom of same column
                    const column = currentIndex % columns;
                    const lastRowStart = (rows - 1) * columns;
                    newIndex = lastRowStart + column;
                    if (newIndex >= buttons.length) {
                        newIndex = lastRowStart + column - columns;
                    }
                }
                break;
            case 'Home':
                newIndex = 0;
                break;
            case 'End':
                newIndex = buttons.length - 1;
                break;
            case 'Escape':
                if (triggerButton) {
                    triggerButton.click(); // Close by clicking trigger again
                    setTimeout(() => triggerButton.focus(), 10);
                }
                return;
        }

        if (newIndex >= 0 && newIndex < buttons.length) {
            buttons[newIndex].focus();
        }
    };

    palette.addEventListener('keydown', keydownHandler);
    
    // Store handler so it can be removed later
    palette._keydownHandler = keydownHandler;
}

/**
 * Setup keyboard navigation for font/size pickers (vertical list)
 * @param {HTMLElement} palette - The palette container
 * @param {HTMLElement} triggerButton - The button that opened the picker
 */
export function setupListPickerNavigation(palette, triggerButton) {
    if (!palette) return;

    const options = Array.from(palette.querySelectorAll('.rte-font-option'));
    if (options.length === 0) return;

    // Focus the first option when palette opens
    setTimeout(() => {
        if (options[0]) {
            options[0].focus();
        }
    }, 50);

    // Attach keyboard handler directly to palette
    const keydownHandler = (event) => {
        const key = event.key;
        if (!['ArrowUp', 'ArrowDown', 'Home', 'End', 'Escape'].includes(key)) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        const opts = Array.from(palette.querySelectorAll('.rte-font-option'));
        const currentIndex = opts.indexOf(event.target);
        if (currentIndex === -1) return;

        let newIndex = currentIndex;

        switch (key) {
            case 'ArrowDown':
                newIndex = (currentIndex + 1) % opts.length;
                break;
            case 'ArrowUp':
                newIndex = (currentIndex - 1 + opts.length) % opts.length;
                break;
            case 'Home':
                newIndex = 0;
                break;
            case 'End':
                newIndex = opts.length - 1;
                break;
            case 'Escape':
                if (triggerButton) {
                    triggerButton.click(); // Close by clicking trigger again
                    setTimeout(() => triggerButton.focus(), 10);
                }
                return;
        }

        if (newIndex >= 0 && newIndex < opts.length) {
            opts[newIndex].focus();
        }
    };

    palette.addEventListener('keydown', keydownHandler);
    
    // Store handler so it can be removed later
    palette._keydownHandler = keydownHandler;
}

/**
 * Focus an element
 */
export function focusElement(element) {
    if (element) {
        element.focus();
    }
}

// Add this function after getActiveFormats()
export function getCurrentTextColor() {
    const color = document.queryCommandValue('foreColor');
    
    // Convert to hex if it's RGB format
    if (color && color.startsWith('rgb')) {
        const rgb = color.match(/\d+/g);
        if (rgb && rgb.length >= 3) {
            const hex = '#' + 
                parseInt(rgb[0]).toString(16).padStart(2, '0') +
                parseInt(rgb[1]).toString(16).padStart(2, '0') +
                parseInt(rgb[2]).toString(16).padStart(2, '0');
            return hex.toUpperCase();
        }
    }
    
    // Return the color as-is (might be hex already) or default to red
    return color && color !== 'rgb(0, 0, 0)' && color !== '#000000' ? color : '#FF0000';
}

// Add this function after getCurrentTextColor()
export function getCurrentBackgroundColor() {
    const color = document.queryCommandValue('backColor');
    
    // Convert to hex if it's RGB format
    if (color && color.startsWith('rgb')) {
        // Check for rgba with transparency
        if (color.startsWith('rgba(0, 0, 0, 0)') || color.startsWith('rgba(0,0,0,0)')) {
            return '#FFFFFF';
        }
        
        const rgb = color.match(/\d+/g);
        if (rgb && rgb.length >= 3) {
            const hex = '#' + 
                parseInt(rgb[0]).toString(16).padStart(2, '0') +
                parseInt(rgb[1]).toString(16).padStart(2, '0') +
                parseInt(rgb[2]).toString(16).padStart(2, '0');
            return hex.toUpperCase();
        }
    }
    
    // Handle transparent/white as white (no highlight applied)
    if (!color || color === 'transparent' || color === 'rgba(0, 0, 0, 0)' || 
        color === '#FFFFFF' || color === '#FFF' || color === 'rgb(255, 255, 255)') {
        return '#FFFFFF'; // Default to white
    }
    
    return color;
}