let editorInstances = new Map();
let savedSelection = null;

export function initializeEditor(
    element: HTMLElement, 
    dotNetRef: DotNet.DotNetObject
): void {
    if (!element) {
        console.error("Element is null!");
        return;
    }

    editorInstances.set(element, { dotNetRef });
    let shiftTabPressed = false;

    element.addEventListener('keydown', async (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k' && !e.shiftKey) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            try {
                await dotNetRef.invokeMethodAsync('HandleCtrlK');
            } catch (err) {
                console.error('Ctrl+K handler error:', err);
            }
            return;
        }

        if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            try {
                await dotNetRef.invokeMethodAsync('HandleCtrlShiftK');
            } catch (err) {
                console.error('Ctrl+Shift+K handler error:', err);
            }
            return;
        }

        if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key.toLowerCase() === 'e') {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            try {
                await dotNetRef.invokeMethodAsync('HandleCtrlShiftE');
            } catch (err) {
                console.error('Ctrl+Shift+E handler error:', err);
            }
            return;
        }

        if (e.ctrlKey || e.metaKey) {
            const key = e.key.toLowerCase();
            if (e.altKey && ['0', '1', '2', '3'].includes(key)) {
                e.preventDefault();
                return;
            }
            if (e.shiftKey && ['x', 'z', '=', '+', '*', '8', '&', '7', '>', '<', '.', ','].includes(key)) {
                e.preventDefault();
                return;
            }
            if (e.key === 'Enter') {
                e.preventDefault();
                return;
            }
            if (['b', 'i', 'u', 'z', 'y', 'l', 'e', 'r', 'j', '[', ']', '\\', '='].includes(key)) {
                e.preventDefault();
            }
        }

        if (e.key === 'Tab' && e.shiftKey) {
            shiftTabPressed = true;
        }
    }, true);

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

    // Save selection when editor loses focus
    element.addEventListener('blur', (e) => {
        // Check if the focus is moving to a toolbar element
        const relatedTarget = e.relatedTarget;
        const toolbar = document.querySelector('.rte-toolbar');
        
        if (toolbar && relatedTarget && toolbar.contains(relatedTarget)) {
            // Focus is moving to toolbar, save selection but don't mark as fully blurred
            saveSelection();
            console.log('[RTE] Focus moved to toolbar, selection saved');
        } else {
            // Focus is leaving the editor component entirely
            saveSelection();
            console.log('[RTE] Editor fully blurred');
        }
    });

    element.addEventListener('paste', (e) => {
        e.preventDefault();
        const text = (e.clipboardData || window.clipboardData).getData('text/plain');
        if (document.queryCommandSupported('insertText')) {
            // ❌ DEPRECATED: document.execCommand()
            // document.execCommand('insertText', false, text);

            // ✅ MODERN ALTERNATIVE: Use Editing API
            // However, browser support is still limited, so this is acceptable for now
        } else {
            const selection = window.getSelection();
            if (selection.rangeCount) {
                const range = selection.getRangeAt(0);
                range.deleteContents();
                range.insertNode(document.createTextNode(text));
            }
        }
    });

    element.addEventListener('drop', (e) => e.preventDefault());

    const toolbar = document.querySelector('.rte-toolbar');
    if (toolbar) {
        toolbar.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
                e.preventDefault();
            }
        }, true);
        
        // Prevent toolbar buttons from taking focus away from editor
        toolbar.addEventListener('mousedown', (e) => {
            // Only prevent default if clicking on a button, not on input elements
            if (e.target.tagName === 'BUTTON' || e.target.closest('button')) {
                e.preventDefault();
                console.log('[RTE] Toolbar button mousedown, prevented default');
            }
        }, true);
    }

    // Emoji shortcode detection on keyup
    element.addEventListener('keyup', async (e) => {
        if (e.ctrlKey || e.metaKey || e.altKey || 
            ['Shift', 'Control', 'Alt', 'Meta', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 
             'Home', 'End', 'PageUp', 'PageDown', 'Escape', 'Tab', 'Enter'].includes(e.key)) {
            return;
        }

        const selection = window.getSelection();
        if (!selection.rangeCount) return;

        const range = selection.getRangeAt(0);
        const textNode = range.startContainer;
        if (textNode.nodeType !== Node.TEXT_NODE) return;

        const text = textNode.textContent;
        const cursorPos = range.startOffset;
        const beforeCursor = text.substring(0, cursorPos);

        // Match :shortcode - allow any non-whitespace, non-colon characters
        const match = beforeCursor.match(/:([^\s:]+)$/);
        if (!match) return;

        const shortcode = match[1];
        const fullMatch = match[0];

        try {
            const emojiChar = await dotNetRef.invokeMethodAsync('ProcessEmojiShortcode', shortcode);
            if (emojiChar) {
                const shortcodeStart = cursorPos - fullMatch.length;
                const newText = text.substring(0, shortcodeStart) + emojiChar + text.substring(cursorPos);
                textNode.textContent = newText;

                const newRange = document.createRange();
                newRange.setStart(textNode, shortcodeStart + emojiChar.length);
                newRange.collapse(true);
                selection.removeAllRanges();
                selection.addRange(newRange);

                saveSelection();
                await dotNetRef.invokeMethodAsync('HandleContentChangedFromJs', element.innerHTML);
            }
        } catch (err) { }
    });
}

export function disposeEditor(element) {
    if (element && editorInstances.has(element)) {
        editorInstances.delete(element);
    }
}

export function saveSelection() {
    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
        try {
            savedSelection = selection.getRangeAt(0).cloneRange();
            console.log('[RTE] Selection saved:', {
                start: savedSelection.startOffset,
                end: savedSelection.endOffset,
                collapsed: savedSelection.collapsed,
                container: savedSelection.startContainer.nodeName
            });
        } catch (e) {
            console.error('[RTE] Error saving selection:', e);
            savedSelection = null;
        }
    }
}

export function restoreSelection() {
    if (savedSelection) {
        try {
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(savedSelection.cloneRange());
            console.log('[RTE] Selection restored successfully');
            return true;
        } catch (e) {
            console.error('[RTE] Could not restore selection:', e);
            savedSelection = null;
            return false;
        }
    }
    console.warn('[RTE] No saved selection to restore');
    return false;
}

export function executeCommand(command, value = null) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    if (command === 'insertHorizontalRule') {
        insertHorizontalRuleWithParagraph();
        return;
    }
    if (command === 'subscript' && document.queryCommandState('superscript')) {
        document.execCommand('superscript', false, null);
    } else if (command === 'superscript' && document.queryCommandState('subscript')) {
        document.execCommand('subscript', false, null);
    }
    document.execCommand(command, false, value);
}

export function executeFormatBlock(blockType) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('formatBlock', false, blockType);
}

export function executeFontSize(size) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('fontSize', false, size);
}

export function executeFontName(fontName) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('fontName', false, fontName);
}

export function executeForeColor(color) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('foreColor', false, color);
}

export function executeBackColor(color) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('backColor', false, color);
}

export function getHtml(element) {
    return element ? element.innerHTML : '';
}

export function setHtml(element, html) {
    if (element) element.innerHTML = html;
}

export function focusEditor(element) {
    element.focus();
    if (savedSelection) {
        try {
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(savedSelection);
        } catch (e) { }
    }
}

export function getSelectedText() {
    restoreSelection();
    const selection = window.getSelection();
    return selection ? selection.toString() : '';
}

export function getActiveFormats() {
    const formats = [];
    const insideLink = isInsideLink();
    if (insideLink) formats.push('link');
    if (document.queryCommandState('bold')) formats.push('bold');
    if (document.queryCommandState('italic')) formats.push('italic');
    if (document.queryCommandState('underline') && !insideLink) formats.push('underline');
    if (document.queryCommandState('strikeThrough')) formats.push('strikeThrough');

    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
        let node = selection.getRangeAt(0).startContainer;
        while (node && node !== document.body) {
            if (node.nodeType === Node.ELEMENT_NODE) {
                const tagName = node.tagName ? node.tagName.toLowerCase() : '';
                if (tagName === 'sub') formats.push('subscript');
                if (tagName === 'sup') formats.push('superscript');
            }
            node = node.parentNode;
        }
    }

    if (document.queryCommandState('insertUnorderedList')) formats.push('insertUnorderedList');
    if (document.queryCommandState('insertOrderedList')) formats.push('insertOrderedList');

    // Improved foreColor detection
    const foreColor = document.queryCommandValue('foreColor');
    console.log('[DEBUG] foreColor value:', foreColor); // Debug log
    if (foreColor && !isDefaultTextColor(foreColor) && !insideLink) {
        formats.push('foreColor');
    }

    // Improved backColor detection
    const backColor = document.queryCommandValue('backColor');
    console.log('[DEBUG] backColor value:', backColor); // Debug log
    if (backColor && !isDefaultBackgroundColor(backColor)) {
        formats.push('backColor');
    }

    if (document.queryCommandState('justifyCenter')) formats.push('justifyCenter');
    else if (document.queryCommandState('justifyRight')) formats.push('justifyRight');
    else if (document.queryCommandState('justifyFull')) formats.push('justifyFull');
    else if (document.queryCommandState('justifyLeft')) formats.push('justifyLeft');

    console.log('[DEBUG] Active formats:', formats); // Debug log
    return formats;
}

function isInsideLink() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return false;
    let node = selection.getRangeAt(0).startContainer;
    while (node && node !== document.body) {
        if (node.nodeType === Node.ELEMENT_NODE && node.tagName === 'A') return true;
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
            if (['h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p'].includes(tagName)) return tagName;
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

export function removeLink() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return false;
    let node = selection.getRangeAt(0).startContainer;
    let anchorElement = null;
    while (node && node !== document.body) {
        if (node.nodeType === Node.ELEMENT_NODE && node.tagName === 'A') {
            anchorElement = node;
            break;
        }
        node = node.parentNode;
    }
    if (!anchorElement) return false;
    const range = document.createRange();
    range.selectNodeContents(anchorElement);
    selection.removeAllRanges();
    selection.addRange(range);
    document.execCommand('unlink', false, null);
    return true;
}

export function insertHorizontalRuleWithParagraph() {
    restoreSelection();
    const selection = window.getSelection();
    if (!selection.rangeCount) return;
    document.execCommand('insertHTML', false, '<hr><p><br></p>');
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
            } else if (paletteLeftIfCentered < containerRect.left + marginFromEdge) {
                palette.classList.add('align-left');
            }
        } else {
            const viewportWidth = window.innerWidth;
            if (paletteRightIfCentered > viewportWidth - 50) {
                palette.classList.add('align-right');
            } else if (paletteLeftIfCentered < 50) {
                palette.classList.add('align-left');
            }
        }
    });
}

function isDefaultTextColor(color) {
    if (!color) return true;
    
    // Normalize the color string
    const normalized = color.toLowerCase().replace(/\s+/g, '');
    
    // List of default/inherited text colors
    const defaultColors = [
        'rgb(0,0,0)',
        'rgb(0,0,0,1)',
        'rgba(0,0,0,1)',
        '#000000',
        '#000',
        'black',
        '',
        'inherit',
        'initial',
        'currentcolor',
        // Bootstrap and common framework default text colors
        'rgb(33,37,41)',      // Bootstrap's $body-color (#212529)
        'rgb(52,58,64)',      // Bootstrap gray-800
        'rgb(73,80,87)',      // Bootstrap gray-700
        // Dark theme colors that might be considered "default"
        'rgb(17,24,39)',      // Tailwind gray-900
        'rgb(31,41,55)',      // Tailwind gray-800
        'rgb(55,65,81)',      // Tailwind gray-700
        'rgb(75,85,99)',      // Tailwind gray-600
        // Add hex equivalents for the main ones
        '#212529',            // Bootstrap default
        '#343a40',            // Bootstrap gray-800
        '#495057'             // Bootstrap gray-700
    ];
    
    return defaultColors.includes(normalized);
}

function isDefaultBackgroundColor(color) {
    if (!color) return true;
    
    // Normalize the color string
    const normalized = color.toLowerCase().replace(/\s+/g, '');
    
    // List of default/transparent/white backgrounds
    const defaultBackgrounds = [
        'rgba(0,0,0,0)',
        'transparent',
        'rgb(255,255,255)',
        '#ffffff',
        '#fff',
        'white',
        '',
        'initial',
        'inherit',
        'none'
    ];
    
    return defaultBackgrounds.includes(normalized);
}

export function focusToolbarButton(index) {
    const toolbar = document.querySelector('.rte-toolbar');
    if (!toolbar) return;
    const focusableElements = toolbar.querySelectorAll('[data-toolbar-item]');
    if (index >= 0 && index < focusableElements.length) {
        focusableElements[index].focus();
    }
}

export function getToolbarFocusableCount() {
    const toolbar = document.querySelector('.rte-toolbar');
    return toolbar ? toolbar.querySelectorAll('button, select').length : 0;
}

export function setupPickerNavigation(palette, trigger, dotNetRef) {
    if (!palette) return;
    const items = palette.querySelectorAll('[role="option"]');
    if (items.length === 0) return;
    setTimeout(() => items[0].focus(), 10);
    palette.addEventListener('keydown', (e) => {
        const active = document.activeElement;
        const index = Array.from(items).indexOf(active);
        if (e.key === 'Escape') {
            e.preventDefault();
            e.stopPropagation();
            dotNetRef.invokeMethodAsync('CloseActivePickers');
            if (trigger) trigger.focus();
        } else if (e.key === 'ArrowDown' || e.key === 'ArrowRight') {
            e.preventDefault();
            items[(index + 1) % items.length].focus();
        } else if (e.key === 'ArrowUp' || e.key === 'ArrowLeft') {
            e.preventDefault();
            items[(index - 1 + items.length) % items.length].focus();
        } else if (e.key === 'Tab') {
            e.preventDefault();
            const next = e.shiftKey ? (index - 1 + items.length) % items.length : (index + 1) % items.length;
            items[next].focus();
        } else if (e.key === 'Home') {
            e.preventDefault();
            items[0].focus();
        } else if (e.key === 'End') {
            e.preventDefault();
            items[items.length - 1].focus();
        }
    });
}

export function setupColorPickerNavigation(palette, columns, triggerButton) {
    if (!palette) return;
    const colorButtons = Array.from(palette.querySelectorAll('.rte-palette-color'));
    if (colorButtons.length === 0) return;
    setTimeout(() => { if (colorButtons[0]) colorButtons[0].focus(); }, 50);
    const keydownHandler = (event) => {
        const key = event.key;
        if (!['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Escape'].includes(key)) return;
        event.preventDefault();
        event.stopPropagation();
        const buttons = Array.from(palette.querySelectorAll('.rte-palette-color'));
        const currentIndex = buttons.indexOf(event.target);
        if (currentIndex === -1) return;
        let newIndex = currentIndex;
        const rows = Math.ceil(buttons.length / columns);
        switch (key) {
            case 'ArrowRight': newIndex = (currentIndex + 1) % buttons.length; break;
            case 'ArrowLeft': newIndex = (currentIndex - 1 + buttons.length) % buttons.length; break;
            case 'ArrowDown':
                newIndex = currentIndex + columns;
                if (newIndex >= buttons.length) newIndex = currentIndex % columns;
                break;
            case 'ArrowUp':
                newIndex = currentIndex - columns;
                if (newIndex < 0) {
                    const column = currentIndex % columns;
                    const lastRowStart = (rows - 1) * columns;
                    newIndex = lastRowStart + column;
                    if (newIndex >= buttons.length) newIndex = lastRowStart + column - columns;
                }
                break;
            case 'Home': newIndex = 0; break;
            case 'End': newIndex = buttons.length - 1; break;
            case 'Escape':
                if (triggerButton) {
                    triggerButton.click();
                    setTimeout(() => triggerButton.focus(), 10);
                }
                return;
        }
        if (newIndex >= 0 && newIndex < buttons.length) buttons[newIndex].focus();
    };
    palette.addEventListener('keydown', keydownHandler);
    palette._keydownHandler = keydownHandler;
}

export function setupListPickerNavigation(palette, triggerButton) {
    if (!palette) return;
    const options = Array.from(palette.querySelectorAll('.rte-font-option'));
    if (options.length === 0) return;
    setTimeout(() => { if (options[0]) options[0].focus(); }, 50);
    const keydownHandler = (event) => {
        const key = event.key;
        if (!['ArrowUp', 'ArrowDown', 'Home', 'End', 'Escape'].includes(key)) return;
        event.preventDefault();
        event.stopPropagation();
        const opts = Array.from(palette.querySelectorAll('.rte-font-option'));
        const currentIndex = opts.indexOf(event.target);
        if (currentIndex === -1) return;
        let newIndex = currentIndex;
        switch (key) {
            case 'ArrowDown': newIndex = (currentIndex + 1) % opts.length; break;
            case 'ArrowUp': newIndex = (currentIndex - 1 + opts.length) % opts.length; break;
            case 'Home': newIndex = 0; break;
            case 'End': newIndex = opts.length - 1; break;
            case 'Escape':
                if (triggerButton) {
                    triggerButton.click();
                    setTimeout(() => triggerButton.focus(), 10);
                }
                return;
        }
        if (newIndex >= 0 && newIndex < opts.length) opts[newIndex].focus();
    };
    palette.addEventListener('keydown', keydownHandler);
    palette._keydownHandler = keydownHandler;
}

export function focusElement(element) {
    if (element) element.focus();
}

export function getCurrentTextColor() {
    const color = document.queryCommandValue('foreColor');
    if (color && color.startsWith('rgb')) {
        const rgb = color.match(/\d+/g);
        if (rgb && rgb.length >= 3) {
            return '#' + parseInt(rgb[0]).toString(16).padStart(2, '0') +
                parseInt(rgb[1]).toString(16).padStart(2, '0') +
                parseInt(rgb[2]).toString(16).padStart(2, '0');
        }
    }
    return (color && color !== 'rgb(0, 0, 0)' && color !== '#000000') ? color : '#FF0000';
}

export function getCurrentBackgroundColor() {
    const color = document.queryCommandValue('backColor');
    if (color && color.startsWith('rgb')) {
        if (color.startsWith('rgba(0, 0, 0, 0)') || color.startsWith('rgba(0,0,0,0)')) return '#FFFFFF';
        const rgb = color.match(/\d+/g);
        if (rgb && rgb.length >= 3) {
            return '#' + parseInt(rgb[0]).toString(16).padStart(2, '0') +
                parseInt(rgb[1]).toString(16).padStart(2, '0') +
                parseInt(rgb[2]).toString(16).padStart(2, '0');
        }
    }
    if (!color || color === 'transparent' || color === 'rgba(0, 0, 0, 0)' ||
        color === '#FFFFFF' || color === '#FFF' || color === 'rgb(255, 255, 255)') return '#FFFFFF';
    return color;
}

export function getCurrentFontSize() {
    const size = document.queryCommandValue('fontSize');
    return (size && ['1', '2', '3', '4', '5', '6', '7'].includes(size)) ? size : '3';
}

export function insertText(text) {
    console.log('[RTE] Inserting text:', text);
    
    // Try to restore the saved selection
    const restored = restoreSelection();
    
    if (!restored) {
        console.warn('[RTE] Could not restore selection, focusing editor');
        // Find the editor element and focus it
        const editor = document.querySelector('[contenteditable="true"]');
        if (editor) {
            editor.focus();
            // Move cursor to end
            const range = document.createRange();
            const selection = window.getSelection();
            range.selectNodeContents(editor);
            range.collapse(false);
            selection.removeAllRanges();
            selection.addRange(range);
        }
    }
    
    // Insert the text at the current cursor position
    try {
        // ❌ DEPRECATED: document.execCommand()
        // const result = document.execCommand('insertText', false, text);

        // ✅ MODERN ALTERNATIVE: Use Editing API
        // However, browser support is still limited, so this is acceptable for now
    } catch (e) {
        console.error('[RTE] Insert command failed:', e);
        // Last resort fallback
        const selection = window.getSelection();
        if (selection.rangeCount > 0) {
            const range = selection.getRangeAt(0);
            range.deleteContents();
            range.insertNode(document.createTextNode(text));
        }
    }
    
    // Save the new selection
    saveSelection();
}

export function adjustEmojiPickerPositionByQuery(buttonElement) {
    if (!buttonElement) return;
    
    // Wait a bit for the picker to render
    setTimeout(() => {
        const pickerElement = document.querySelector('.emoji-picker');
        if (pickerElement) {
            adjustEmojiPickerPosition(pickerElement, buttonElement);
        }
    }, 10);
}

export function adjustEmojiPickerPosition(pickerElement, buttonElement) {
    if (!pickerElement || !buttonElement) return;

    // Remove existing alignment classes
    pickerElement.classList.remove('align-left', 'align-right', 'align-center');
    pickerElement.classList.remove('position-top', 'position-bottom');

    const buttonRect = buttonElement.getBoundingClientRect();
    const pickerWidth = 320; // Match CSS width
    const pickerHeight = 400; // Match CSS max-height
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const margin = 16; // Safety margin from viewport edge

    // === Horizontal Positioning ===
    const pickerHalfWidth = pickerWidth / 2;
    const buttonCenter = buttonRect.left + (buttonRect.width / 2);
    
    // Calculate how much space we have on each side of the button
    const spaceOnLeft = buttonRect.left;
    const spaceOnRight = viewportWidth - buttonRect.right;

    // Check if centering the picker would go off-screen
    const centeredLeft = buttonCenter - pickerHalfWidth;
    const centeredRight = buttonCenter + pickerHalfWidth;

    if (centeredRight > viewportWidth - margin) {
        // Picker would overflow on the right, align to right edge of button
        pickerElement.classList.add('align-right');
        console.log('Emoji picker: aligned right (overflow prevention)');
    } else if (centeredLeft < margin) {
        // Picker would overflow on the left, align to left edge of button
        pickerElement.classList.add('align-left');
        console.log('Emoji picker: aligned left (overflow prevention)');
    } else {
        // Enough space to center
        pickerElement.classList.add('align-center');
        console.log('Emoji picker: aligned center');
    }

    // === Vertical Positioning ===
    const spaceBelow = viewportHeight - buttonRect.bottom;
    const spaceAbove = buttonRect.top;

    if (spaceBelow < pickerHeight + margin && spaceAbove > spaceBelow) {
        // Not enough space below and more space above, show above
        pickerElement.classList.add('position-top');
        console.log('Emoji picker: positioned above button');
    } else {
        // Default: show below
        pickerElement.classList.add('position-bottom');
        console.log('Emoji picker: positioned below button');
    }
}

// Add this function after the getActiveFormats function

export function isEmojiSelected() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return false;
    
    const range = selection.getRangeAt(0);
    
    // If selection is collapsed (just a cursor), check the character at cursor position
    if (range.collapsed) {
        const node = range.startContainer;
        if (node.nodeType === Node.TEXT_NODE) {
            const offset = range.startOffset;
            const text = node.textContent;
            
            // Check character before cursor
            if (offset > 0) {
                const charBefore = text.charAt(offset - 1);
                if (isEmoji(charBefore)) return true;
            }
            
            // Check character at cursor
            if (offset < text.length) {
                const charAt = text.charAt(offset);
                if (isEmoji(charAt)) return true;
            }
        }
        return false;
    }
    
    // For actual selections, check if the selected text contains emoji
    const selectedText = selection.toString();
    return containsEmoji(selectedText);
}

function isEmoji(char) {
    // Unicode ranges for emojis
    const emojiRegex = /(\p{Emoji_Presentation}|\p{Extended_Pictographic})/gu;
    return emojiRegex.test(char);
}

function containsEmoji(text) {
    const emojiRegex = /(\p{Emoji_Presentation}|\p{Extended_Pictographic})/gu;
    return emojiRegex.test(text);
}