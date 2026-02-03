let editorInstances = new Map();
let savedSelection = null;

export function initializeEditor(element, dotNetRef) {
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

    element.addEventListener('blur', () => saveSelection());

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

    element.addEventListener('drop', (e) => e.preventDefault());

    const toolbar = document.querySelector('.rte-toolbar');
    if (toolbar) {
        toolbar.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
                e.preventDefault();
            }
        }, true);
    }
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
    }
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

    const foreColor = document.queryCommandValue('foreColor');
    if (foreColor && !isDefaultTextColor(foreColor) && !insideLink) formats.push('foreColor');

    const backColor = document.queryCommandValue('backColor');
    if (backColor && !isDefaultBackgroundColor(backColor)) formats.push('backColor');

    if (document.queryCommandState('justifyCenter')) formats.push('justifyCenter');
    else if (document.queryCommandState('justifyRight')) formats.push('justifyRight');
    else if (document.queryCommandState('justifyFull')) formats.push('justifyFull');
    else if (document.queryCommandState('justifyLeft')) formats.push('justifyLeft');

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
    const c = color.toLowerCase().replace(/\s/g, '');
    return c === 'rgb(0,0,0)' || c === '#000000' || c === '#000' || c === 'black' ||
        c === '' || c === 'rgb(17,24,39)' || c === 'rgb(31,41,55)' ||
        c === 'rgb(55,65,81)' || c === 'rgb(0,0,0,1)' || c === 'rgba(0,0,0,1)';
}

function isDefaultBackgroundColor(color) {
    if (!color) return true;
    const c = color.toLowerCase().replace(/\s/g, '');
    return c === 'rgba(0,0,0,0)' || c === 'transparent' || c === 'rgb(255,255,255)' ||
        c === '#ffffff' || c === '#fff' || c === 'white' || c === '' ||
        c === 'initial' || c === 'inherit';
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