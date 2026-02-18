let editorInstances = new Map();
let savedSelection = null; 

// ===== PENDING FORMAT MODULE =====
import * as pendingModule from './rich-text-editor.pending.js';

// Re-export for C# JSInterop
export const hasTextSelection = pendingModule.hasTextSelection;
export const keepSelectionAfterFormat = pendingModule.keepSelectionAfterFormat;
export const applyPendingFormats = pendingModule.applyPendingFormats;
export const clearPendingFormats = pendingModule.clearPendingFormats;

let knownFontFamilies = []; // Module-level variable

export function initializeEditor(element, dotNetRef, editorId, fontFamilies = []) {
    if (!element) {
        console.error("Element is null!");
        return;
    }

    // Store known fonts passed from C#
    if (fontFamilies && fontFamilies.length > 0) {
        knownFontFamilies = fontFamilies;
    }

    editorInstances.set(element, { dotNetRef });
 
    
    // *** Autocomplete variables (inside initializeEditor scope) ***
    let shortcodeStart = -1;
    let shortcodeText = '';
    
    // *** Autocomplete helper functions ***
    function checkShortcode() {
        const selection = window.getSelection();
        if (!selection.rangeCount) return;
        
        const range = selection.getRangeAt(0);
        if (!range.collapsed) return;
        
        const textNode = range.startContainer;
        if (textNode.nodeType !== Node.TEXT_NODE) {
            clearShortcode();
            return;
        }
        
        const text = textNode.textContent;
        const offset = range.startOffset;
        
        // Find the last colon before cursor
        let colonPos = -1;
        for (let i = offset - 1; i >= 0; i--) {
            if (text[i] === ':') {
                colonPos = i;
                break;
            }
            if (text[i] === ' ' || text[i] === '\n') {
                break;
            }
        }
        
        if (colonPos === -1) {
            if (shortcodeStart !== -1) {
                clearShortcode();
                dotNetRef.invokeMethodAsync('HideEmojiAutocomplete');
            }
            return;
        }
        
        // Get text after colon
        const afterColon = text.substring(colonPos + 1, offset);
        
        // Minimum 2 characters after colon to trigger autocomplete
        if (afterColon.length >= 2) {
            shortcodeStart = colonPos;
            shortcodeText = afterColon;
            
            // Get cursor position in VIEWPORT coordinates
            const tempRange = document.createRange();
            tempRange.setStart(textNode, offset);
            tempRange.setEnd(textNode, offset);
            
            const cursorRect = tempRange.getBoundingClientRect();
            
            // Popup dimensions
            const popupWidth = 320;
            const popupHeight = 380;
            const gapFromCursor = 8;
            
            // Use viewport coordinates directly
            let x = cursorRect.left;
            let y = cursorRect.bottom + gapFromCursor;
            
            // Check if popup would go off the right edge
            if (x + popupWidth > window.innerWidth - 16) {
                x = window.innerWidth - popupWidth - 16;
            }
            
            // Check if popup would go off the left edge
            if (x < 16) {
                x = 16;
            }
            
            // Check if popup would go off the bottom
            if (y + popupHeight > window.innerHeight - 16) {
                // Show above cursor instead
                y = cursorRect.top - popupHeight - gapFromCursor;
                
                // If still off screen, show at top
                if (y < 16) {
                    y = 16;
                }
            }
            
            dotNetRef.invokeMethodAsync('ShowEmojiAutocomplete', afterColon, { x, y });
        } else if (afterColon.length === 0) {
            clearShortcode();
            dotNetRef.invokeMethodAsync('HideEmojiAutocomplete');
        }
    }
    
    function clearShortcode() {
        shortcodeStart = -1;
        shortcodeText = '';
    }
    
    function insertEmojiAtShortcode(emoji) {
        if (shortcodeStart === -1) {
            return;
        }
        
        element.focus();
        
        const selection = window.getSelection();
        if (!selection.rangeCount) {
            clearShortcode();
            return;
        }
        
        // Find text node with shortcode
        let textNode = null;
        let cursorOffset = 0;
        
        const allTextNodes = getTextNodesIn(element);
        for (let node of allTextNodes) {
            const text = node.textContent;
            for (let i = 0; i < text.length; i++) {
                if (text[i] === ':') {
                    const afterColon = text.substring(i + 1);
                    if (afterColon.length >= 1) {
                        textNode = node;
                        cursorOffset = text.length;
                        break;
                    }
                }
            }
            if (textNode) break;
        }
        
        if (!textNode) {
            clearShortcode();
            return;
        }
        
        const text = textNode.textContent;
        
        // Find colon position
        let actualColonPos = -1;
        for (let i = Math.min(cursorOffset, text.length) - 1; i >= 0; i--) {
            if (text[i] === ':') {
                actualColonPos = i;
                break;
            }
            if (text[i] === ' ' || text[i] === '\n') {
                break;
            }
        }
        
        if (actualColonPos === -1) {
            clearShortcode();
            return;
        }
        
        // Replace shortcode with emoji
        const beforeShortcode = text.substring(0, actualColonPos);
        const afterCursor = text.substring(Math.min(cursorOffset, text.length));
        const newText = beforeShortcode + emoji + ' ' + afterCursor;
        
        textNode.textContent = newText;
        
        // Set cursor position after emoji
        const newOffset = actualColonPos + emoji.length + 1;
        
        if (newOffset <= textNode.textContent.length) {
            try {
                const newRange = document.createRange();
                newRange.setStart(textNode, newOffset);
                newRange.setEnd(textNode, newOffset);
                selection.removeAllRanges();
                selection.addRange(newRange);
            } catch (e) { }
        }
        
        clearShortcode();
        
        dotNetRef.invokeMethodAsync('HandleContentChangedFromJs', element.innerHTML)
            .catch(err => console.error('[Autocomplete] Content change error:', err));
    }

    function getTextNodesIn(node) {
        const textNodes = [];
        if (node.nodeType === Node.TEXT_NODE) {
            textNodes.push(node);
        } else {
            for (let child of node.childNodes) {
                textNodes.push(...getTextNodesIn(child));
            }
        }
        return textNodes;
    }
    
    // Store function for external access
    element._insertEmojiAtShortcode = insertEmojiAtShortcode;
 
    element.addEventListener('blur', (e) => {
        // Always save selection on blur
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

    element.addEventListener('drop', (e) => e.preventDefault());
 

    // Input event for autocomplete
    element.addEventListener('input', function(e) {
        checkShortcode();
    });

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

        const match = beforeCursor.match(/:([^\s:]+)$/);
        if (!match) return;

        const shortcode = match[1];
        const fullMatch = match[0];

        // Skip if 2+ characters (autocomplete handles it)
        if (shortcode.length >= 2) {
            return;
        }

        // Process single-character shortcodes
        try {
            const emojiChar = await dotNetRef.invokeMethodAsync('ProcessEmojiShortcode', shortcode);
            if (emojiChar) {
                const shortcodeStartPos = cursorPos - fullMatch.length;
                const newText = text.substring(0, shortcodeStartPos) + emojiChar + text.substring(cursorPos);
                textNode.textContent = newText;

                const newRange = document.createRange();
                newRange.setStart(textNode, shortcodeStartPos + emojiChar.length);
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
        } catch (e) {
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
            return true;
        } catch (e) {
            savedSelection = null;
            return false;
        }
    }
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

    // Handle subscript/superscript - manual implementation due to browser quirks
    if (command === 'subscript' || command === 'superscript') {
        const selection = window.getSelection();
        if (!selection.rangeCount) {
            saveSelection();
            return;
        }
        
        const myTag = command === 'subscript' ? 'SUB' : 'SUP';
        const oppositeTag = command === 'subscript' ? 'SUP' : 'SUB';
        
        // Save the current selection text for re-selection
        const selectedText = selection.toString();
        const range = selection.getRangeAt(0);
        
        // Check if we're inside the tags
        let node = range.startContainer;
        let myElement = null;
        let oppositeElement = null;
        
        while (node && node !== document.body) {
            if (node.nodeType === Node.ELEMENT_NODE) {
                if (node.tagName === myTag && !myElement) myElement = node;
                if (node.tagName === oppositeTag && !oppositeElement) oppositeElement = node;
            }
            node = node.parentNode;
        }
        
        // Remove opposite format if present
        if (oppositeElement) {
            const parent = oppositeElement.parentNode;
            const firstChild = oppositeElement.firstChild;
            while (oppositeElement.firstChild) {
                parent.insertBefore(oppositeElement.firstChild, oppositeElement);
            }
            parent.removeChild(oppositeElement);
            
            // Re-select the text
            if (firstChild && selectedText) {
                const newRange = document.createRange();
                newRange.selectNodeContents(firstChild.parentNode.contains(firstChild) ? firstChild : parent);
                selection.removeAllRanges();
                selection.addRange(newRange);
            }
        }
        
        // Toggle same format
        if (myElement) {
            // Remove (toggle off) - preserve selection
            const parent = myElement.parentNode;
            const textContent = myElement.textContent;
            const firstChild = myElement.firstChild;
            
            while (myElement.firstChild) {
                parent.insertBefore(myElement.firstChild, myElement);
            }
            parent.removeChild(myElement);
            
            // Re-select the unwrapped text
            if (firstChild && firstChild.nodeType === Node.TEXT_NODE) {
                const newRange = document.createRange();
                newRange.setStart(firstChild, 0);
                newRange.setEnd(firstChild, firstChild.textContent.length);
                selection.removeAllRanges();
                selection.addRange(newRange);
            }
        } else {
            // Apply format
            document.execCommand(command, false, null);
        }
        
        saveSelection();
        return;
    }
    
    // Handle alignment commands - they are mutually exclusive
    if (['justifyLeft', 'justifyCenter', 'justifyRight', 'justifyFull'].includes(command)) {
        document.execCommand(command, false, null);
    } else {
        document.execCommand(command, false, value);
    }
    
    saveSelection();
}

export function executeFormatBlock(blockType) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('formatBlock', false, blockType);
    saveSelection();
}

export function executeFontSize(size) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('fontSize', false, size);
    saveSelection();
}

export function executeFontName(fontName) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('fontName', false, fontName);
    saveSelection();
}

export function executeForeColor(color) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('foreColor', false, color);
    saveSelection();
}

export function executeBackColor(color) {
    const currentSelection = window.getSelection();
    if (!currentSelection || currentSelection.rangeCount === 0 || currentSelection.isCollapsed) {
        restoreSelection();
    }
    document.execCommand('backColor', false, color);
    saveSelection();
}

export function getHtml(element) {
    return element ? element.innerHTML : '';
}

export function setHtml(element, html) {
    if (element) element.innerHTML = html;
}

// Restore focus to the editor with saved selection
export function focusEditor(element) {
    element.focus();

    // Restore saved selection if available
    if (savedSelection) {
        try {
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(savedSelection.cloneRange());
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

    // Check for subscript/superscript - use both tag check AND queryCommandState
    let hasSubscript = false;
    let hasSuperscript = false;


    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
        let node = selection.getRangeAt(0).startContainer;
        while (node && node !== document.body) {
            if (node.nodeType === Node.ELEMENT_NODE) {
                const tagName = node.tagName ? node.tagName.toLowerCase() : '';
                if (tagName === 'sub') hasSubscript = true;
                if (tagName === 'sup') hasSuperscript = true;
            }
            node = node.parentNode;
        }
    }

    // Fallback to queryCommandState
    if (!hasSubscript && document.queryCommandState('subscript')) hasSubscript = true;
    if (!hasSuperscript && document.queryCommandState('superscript')) hasSuperscript = true;

    if (hasSubscript) formats.push('subscript');
    if (hasSuperscript) formats.push('superscript');

    if (document.queryCommandState('insertUnorderedList')) formats.push('insertUnorderedList');
    if (document.queryCommandState('insertOrderedList')) formats.push('insertOrderedList');

    const foreColor = document.queryCommandValue('foreColor');
    if (foreColor && !isDefaultTextColor(foreColor) && !insideLink) {
        formats.push('foreColor');
    }

    const backColor = document.queryCommandValue('backColor');
    if (backColor && !isDefaultBackgroundColor(backColor)) {
        formats.push('backColor');
    }

    // Check alignment - use queryCommandState for each
    // Note: justifyLeft often returns false when text is default left-aligned
    const isCenter = document.queryCommandState('justifyCenter');
    const isRight = document.queryCommandState('justifyRight');
    const isFull = document.queryCommandState('justifyFull');
    const isLeft = document.queryCommandState('justifyLeft');
    
    if (isCenter) formats.push('justifyCenter');
    else if (isRight) formats.push('justifyRight');
    else if (isFull) formats.push('justifyFull');
    else if (isLeft) formats.push('justifyLeft');
    // If none are active, default to left (don't push anything - C# defaults to "left")

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
        saveSelection();
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
    saveSelection();
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
    const normalized = color.toLowerCase().replace(/\s+/g, '');
    const defaultColors = [
        'rgb(0,0,0)', 'rgb(0,0,0,1)', 'rgba(0,0,0,1)', '#000000', '#000', 'black', '',
        'inherit', 'initial', 'currentcolor', 'rgb(33,37,41)', 'rgb(52,58,64)', 'rgb(73,80,87)',
        'rgb(17,24,39)', 'rgb(31,41,55)', 'rgb(55,65,81)', 'rgb(75,85,99)',
        '#212529', '#343a40', '#495057'
    ];
    return defaultColors.includes(normalized);
}

function isDefaultBackgroundColor(color) {
    if (!color) return true;
    const normalized = color.toLowerCase().replace(/\s+/g, '');
    const defaultBackgrounds = [
        'rgba(0,0,0,0)', 'transparent', 'rgb(255,255,255)', '#ffffff', '#fff',
        'white', '', 'initial', 'inherit', 'none'
    ];
    return defaultBackgrounds.includes(normalized);
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
    const selection = window.getSelection();
    if (!selection.rangeCount) return '3'; // Default Normal
    
    const range = selection.getRangeAt(0);
    const parent = range.commonAncestorContainer.parentElement;
    
    // Check for <font size="X">
    let element = parent;
    while (element && element !== document.body) {
        if (element.tagName === 'FONT' && element.size) {
            return element.size;
        }
        element = element.parentElement;
    }
    
    // Check computed font size
    const fontSize = window.getComputedStyle(parent).fontSize;
    const pxSize = parseFloat(fontSize);
    
    // Map px to font size numbers
    if (pxSize <= 10) return '1';
    if (pxSize <= 14) return '3';
    if (pxSize <= 16) return '4';
    if (pxSize <= 18) return '5';
    if (pxSize <= 24) return '6';
    return '7';
}

export function insertText(text) {
    const restored = restoreSelection();
    
    if (!restored) {
        const editor = document.querySelector('[contenteditable="true"]');
        if (editor) {
            editor.focus();
            const range = document.createRange();
            const selection = window.getSelection();
            range.selectNodeContents(editor);
            range.collapse(false);
            selection.removeAllRanges();
            selection.addRange(range);
        }
    }
    
    try {
        const result = document.execCommand('insertText', false, text);
        if (!result) {
            document.execCommand('insertHTML', false, text);
        }
    } catch (e) {
        const selection = window.getSelection();
        if (selection.rangeCount > 0) {
            const range = selection.getRangeAt(0);
            range.deleteContents();
            range.insertNode(document.createTextNode(text));
        }
    }
    
    saveSelection();
}

export function adjustEmojiPickerPositionByQuery(buttonElement) {
    if (!buttonElement) return;
    setTimeout(() => {
        const pickerElement = document.querySelector('.emoji-picker');
        if (pickerElement) {
            adjustEmojiPickerPosition(pickerElement, buttonElement);
        }
    }, 10);
}

export function adjustEmojiPickerPosition(pickerElement, buttonElement) {
    if (!pickerElement || !buttonElement) return;
    pickerElement.classList.remove('align-left', 'align-right', 'align-center');
    pickerElement.classList.remove('position-top', 'position-bottom');
    const buttonRect = buttonElement.getBoundingClientRect();
    const pickerWidth = 320;
    const pickerHeight = 400;
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const margin = 16;
    const pickerHalfWidth = pickerWidth / 2;
    const buttonCenter = buttonRect.left + (buttonRect.width / 2);
    const centeredLeft = buttonCenter - pickerHalfWidth;
    const centeredRight = buttonCenter + pickerHalfWidth;
    if (centeredRight > viewportWidth - margin) {
        pickerElement.classList.add('align-right');
    } else if (centeredLeft < margin) {
        pickerElement.classList.add('align-left');
    } else {
        pickerElement.classList.add('align-center');
    }
    const spaceBelow = viewportHeight - buttonRect.bottom;
    const spaceAbove = buttonRect.top;
    if (spaceBelow < pickerHeight + margin && spaceAbove > spaceBelow) {
        pickerElement.classList.add('position-top');
    } else {
        pickerElement.classList.add('position-bottom');
    }
}

export function isEmojiSelected() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return false;
    const range = selection.getRangeAt(0);
    if (range.collapsed) {
        const node = range.startContainer;
        if (node.nodeType === Node.TEXT_NODE) {
            const offset = range.startOffset;
            const text = node.textContent;
            if (offset > 0) {
                const charBefore = text.charAt(offset - 1);
                if (isEmoji(charBefore)) return true;
            }
            if (offset < text.length) {
                const charAt = text.charAt(offset);
                if (isEmoji(charAt)) return true;
            }
        }
        return false;
    }
    const selectedText = selection.toString();
    return containsEmoji(selectedText);
}

function isEmoji(char) {
    const emojiRegex = /(\p{Emoji_Presentation}|\p{Extended_Pictographic})/gu;
    return emojiRegex.test(char);
}

function containsEmoji(text) {
    const emojiRegex = /(\p{Emoji_Presentation}|\p{Extended_Pictographic})/gu;
    return emojiRegex.test(text);
}

export function insertEmojiAtShortcode(emoji) {
    const editor = document.querySelector('[contenteditable="true"]');
    if (editor && editor._insertEmojiAtShortcode) {
        editor._insertEmojiAtShortcode(emoji);
    }
}

export function focusElementById(elementId) {
    const el = document.getElementById(elementId);
    if (el) el.focus();
}

export function focusFirstInElement(elementId) {
    const container = document.getElementById(elementId);
    if (!container) return;

    const focusable = container.querySelector('button, [tabindex="0"]');
    if (focusable) {
        focusable.focus();
    }
}

export function navigateDropdown(elementId, direction) {
    const container = document.getElementById(elementId);
    if (!container) return;

    // Check for nested grid element (color palettes have grid inside listbox)
    const gridElement = container.querySelector('[role="grid"]');
    const isGrid = gridElement !== null;
    
    // Get buttons from grid if it exists, otherwise from container
    const buttonContainer = gridElement || container;
    const buttons = Array.from(buttonContainer.querySelectorAll('button'));
    if (buttons.length === 0) return;

    const currentIndex = buttons.findIndex(b => b === document.activeElement);
    if (currentIndex === -1) {
        buttons[0]?.focus();
        return;
    }

    let nextIndex;
    
    if (isGrid) {
        // For color grids: detect columns by checking button positions
        const firstButton = buttons[0];
        const firstTop = firstButton.getBoundingClientRect().top;
        let columnsInRow = 0;
        for (const btn of buttons) {
            if (Math.abs(btn.getBoundingClientRect().top - firstTop) < 2) { // Allow small tolerance
                columnsInRow++;
            } else {
                break;
            }
        }
        columnsInRow = columnsInRow || 1;

        switch (direction) {
            case 'right':
                nextIndex = currentIndex < buttons.length - 1 ? currentIndex + 1 : 0;
                break;
            case 'left':
                nextIndex = currentIndex > 0 ? currentIndex - 1 : buttons.length - 1;
                break;
            case 'down':
                nextIndex = currentIndex + columnsInRow;
                if (nextIndex >= buttons.length) nextIndex = currentIndex % columnsInRow;
                break;
            case 'up':
                nextIndex = currentIndex - columnsInRow;
                if (nextIndex < 0) {
                    // Go to last row, same column
                    const lastRowStart = Math.floor((buttons.length - 1) / columnsInRow) * columnsInRow;
                    nextIndex = lastRowStart + (currentIndex % columnsInRow);
                    if (nextIndex >= buttons.length) nextIndex = buttons.length - 1;
                }
                break;
            default:
                return;
        }
    } else {
        // For listbox dropdowns: linear navigation
        if (direction === 'down' || direction === 'right') {
            nextIndex = currentIndex < buttons.length - 1 ? currentIndex + 1 : 0;
        } else {
            nextIndex = currentIndex > 0 ? currentIndex - 1 : buttons.length - 1;
        }
    }

    buttons[nextIndex]?.focus();
}

export function clickFocusedElement() {
    const focused = document.activeElement;
    if (focused && focused.tagName === 'BUTTON') {
        focused.click();
    }
}

// Helper function to unwrap an element (remove tag but keep contents)
function unwrapElement(element) {
    const parent = element.parentNode;
    while (element.firstChild) {
        parent.insertBefore(element.firstChild, element);
    }
    parent.removeChild(element);
}

export function getCurrentFontFamily() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return '';

    const range = selection.getRangeAt(0);
    let element = range.commonAncestorContainer;

    if (element.nodeType === Node.TEXT_NODE) {
        element = element.parentElement;
    }

    if (!element) return '';

    // Check for <font face="..."> tag
    let node = element;
    while (node && node !== document.body) {
        if (node.tagName === 'FONT' && node.face) {
            return node.face;
        }
        node = node.parentElement;
    }

    // Get computed font family
    const computedFont = window.getComputedStyle(element).fontFamily;
    if (!computedFont) return '';

    const firstFont = computedFont.split(',')[0].trim().replace(/['"]/g, '');

    // Use fonts from C# helper class
    for (const font of knownFontFamilies) {
        if (firstFont.toLowerCase() === font.toLowerCase()) {
            return font;
        }
    }

    const lowerComputed = computedFont.toLowerCase();
    for (const font of knownFontFamilies) {
        if (lowerComputed.includes(font.toLowerCase())) {
            return font;
        }
    }

    return '';
}
// New function to scroll selected element into view
export function scrollSelectedIntoView(elementId) {
    const container = document.getElementById(elementId);
    if (!container) return;
    
    // Find the selected item
    const selected = container.querySelector('.selected, [aria-selected="true"]');
    if (selected) {
        // Scroll the selected item into view (center it if possible)
        selected.scrollIntoView({ block: 'nearest', behavior: 'instant' });
        
        // Also focus it for keyboard navigation
        if (selected.tagName === 'BUTTON') {
            selected.focus();
        }
    }
}