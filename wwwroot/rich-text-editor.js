console.log("rich-text-editor.js loaded");

let editorInstances = new Map();
let savedSelection = null;

// Track shortcode typing for autocomplete
let shortcodeStart = -1;
let shortcodeText = '';

export function initializeEditor(element, dotNetRef) {
    console.log("initializeEditor called", element);
    if (!element) {
        console.error("Element is null!");
        return;
    }

    editorInstances.set(element, { dotNetRef });

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
    
    // Add input event listener for shortcode checking
    element.addEventListener('input', async function(e) {
        if (e.inputType === 'insertText' && e.data === ' ') {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;
            
            const range = selection.getRangeAt(0);
            const textNode = range.startContainer;
            
            if (textNode.nodeType !== Node.TEXT_NODE) return;
            
            const text = textNode.textContent;
            const cursorPos = range.startOffset;
            
            // Find the last colon before the space
            let colonIndex = -1;
            for (let i = cursorPos - 2; i >= 0; i--) {
                if (text[i] === ':') {
                    colonIndex = i;
                    break;
                }
                if (text[i] === ' ') break;
            }
            
            if (colonIndex === -1) return;
            
            const shortcode = text.substring(colonIndex + 1, cursorPos - 1);
            
            // *** NEW: Skip if shortcode is 2+ characters - let autocomplete handle it ***
            if (shortcode.length >= 2) {
                return; // Don't process, let autocomplete show instead
            }
            
            // Only process single-character shortcodes (emoticons)
            if (shortcode.length === 0) return;
            
            try {
                const emoji = await dotNetHelper.invokeMethodAsync('ProcessEmojiShortcode', shortcode);
                
                if (emoji) {
                    // Replace :shortcode with emoji
                    const beforeColon = text.substring(0, colonIndex);
                    const afterSpace = text.substring(cursorPos);
                    
                    textNode.textContent = beforeColon + emoji + ' ' + afterSpace;
                    
                    // Set cursor after emoji
                    const newPos = colonIndex + emoji.length + 1;
                    range.setStart(textNode, newPos);
                    range.setEnd(textNode, newPos);
                    
                    // Notify C# of content change
                    const html = editor.innerHTML;
                    await dotNetHelper.invokeMethodAsync('HandleContentChangedFromJs', html);
                }
            } catch (err) {
                console.error('Shortcode processing failed:', err);
            }
        }
        
        // Check for shortcode autocomplete
        checkShortcode();
    });

    element.addEventListener('keydown', function(e) {
        // Existing keydown handling...
        
        // Handle escape to close autocomplete
        if (e.key === 'Escape' && shortcodeStart !== -1) {
            e.preventDefault();
            clearShortcode();
            dotNetHelper.invokeMethodAsync('HideEmojiAutocomplete');
        }
        
        // Handle arrow keys and enter in autocomplete
        if (shortcodeStart !== -1) {
            if (e.key === 'ArrowDown' || e.key === 'ArrowUp' || e.key === 'Enter') {
                e.preventDefault();
                dotNetHelper.invokeMethodAsync('HandleAutocompleteKey', e.key);
                return;
            }
        }
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

        // Match :shortcode - allow any non-whitespace, non-colon characters
        const match = beforeCursor.match(/:([^\s:]+)$/);
        if (!match) return;

        const shortcode = match[1];
        const fullMatch = match[0];

        // *** MODIFIED: Skip if shortcode is 2+ characters - let autocomplete handle it ***
        if (shortcode.length >= 2) {
            return; // Don't process, let autocomplete show instead
        }

        // Only process single-character shortcodes (emoticons like :D :P)
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

    console.log("Editor initialized successfully");
}

export function disposeEditor(element) {
    if (element && editorInstances.has(element)) {
        editorInstances.delete(element);
    }
}

export function saveSelection() {
    const sel = window.getSelection();
    if (sel.rangeCount > 0) {
        savedSelection = sel.getRangeAt(0);
        console.log("Selection saved");
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

    // Special handling for horizontal rule to prevent cursor getting stuck
    if (command === 'insertHorizontalRule') {
        insertHorizontalRuleWithParagraph();
        return;
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
    if (element) {
        element.focus();
        const range = document.createRange();
        const selection = window.getSelection();
        range.selectNodeContents(element);
        range.collapse(false);
        selection.removeAllRanges();
        selection.addRange(range);
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
    
    // Only report underline if NOT inside a link (links are underlined by default)
    if (document.queryCommandState('underline') && !insideLink) {
        formats.push('underline');
    }
    
    if (document.queryCommandState('strikeThrough')) formats.push('strikeThrough');
    if (document.queryCommandState('subscript')) formats.push('subscript');
    if (document.queryCommandState('superscript')) formats.push('superscript');
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

// Add this function:
export function focusToolbarButton(index) {
    const toolbar = document.querySelector('.rte-toolbar');
    if (!toolbar) return;
    
    const buttons = toolbar.querySelectorAll('button.rte-btn, select.rte-dropdown');
    if (index >= 0 && index < buttons.length) {
        buttons[index].focus();
    }
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

// ------ Shortcode/Autocomplete Handling ------ //

// Add to the existing rich-text-editor.js file

// Track shortcode typing
let shortcodeStart = -1;
let shortcodeText = '';

// Add to the oninput handler
editor.addEventListener('input', async function(e) {
    if (e.inputType === 'insertText' && e.data === ' ') {
        const selection = window.getSelection();
        if (!selection.rangeCount) return;
        
        const range = selection.getRangeAt(0);
        const textNode = range.startContainer;
        
        if (textNode.nodeType !== Node.TEXT_NODE) return;
        
        const text = textNode.textContent;
        const cursorPos = range.startOffset;
        
        // Find the last colon before the space
        let colonIndex = -1;
        for (let i = cursorPos - 2; i >= 0; i--) {
            if (text[i] === ':') {
                colonIndex = i;
                break;
            }
            if (text[i] === ' ') break;
        }
        
        if (colonIndex === -1) return;
        
        const shortcode = text.substring(colonIndex + 1, cursorPos - 1);
        
        // *** NEW: Skip if shortcode is 2+ characters - let autocomplete handle it ***
        if (shortcode.length >= 2) {
            return; // Don't process, let autocomplete show instead
        }
        
        // Only process single-character shortcodes (emoticons)
        if (shortcode.length === 0) return;
        
        try {
            const emoji = await dotNetHelper.invokeMethodAsync('ProcessEmojiShortcode', shortcode);
            
            if (emoji) {
                // Replace :shortcode with emoji
                const beforeColon = text.substring(0, colonIndex);
                const afterSpace = text.substring(cursorPos);
                
                textNode.textContent = beforeColon + emoji + ' ' + afterSpace;
                
                // Set cursor after emoji
                const newPos = colonIndex + emoji.length + 1;
                range.setStart(textNode, newPos);
                range.setEnd(textNode, newPos);
                
                // Notify C# of content change
                const html = editor.innerHTML;
                await dotNetHelper.invokeMethodAsync('HandleContentChangedFromJs', html);
            }
        } catch (err) {
            console.error('Shortcode processing failed:', err);
        }
    }
    
    // Check for shortcode autocomplete
    checkShortcode();
});

editor.addEventListener('keyup', async (e) => {
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

    // *** MODIFIED: Skip if shortcode is 2+ characters - let autocomplete handle it ***
    if (shortcode.length >= 2) {
        return; // Don't process, let autocomplete show instead
    }

    // Only process single-character shortcodes (emoticons like :D :P)
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
        
        // Get cursor position for popup placement
        const rect = range.getBoundingClientRect();
        const editorRect = element.getBoundingClientRect();
        
        dotNetRef.invokeMethodAsync('ShowEmojiAutocomplete', afterColon, {
            x: rect.left - editorRect.left,
            y: rect.bottom - editorRect.top
        });
    } else if (afterColon.length === 0) {
        clearShortcode();
        dotNetRef.invokeMethodAsync('HideEmojiAutocomplete');
    }
}

function clearShortcode() {
    shortcodeStart = -1;
    shortcodeText = '';
}

export function insertEmojiAtShortcode(emoji) {
    if (shortcodeStart === -1) return;
    
    const selection = window.getSelection();
    if (!selection.rangeCount) return;
    
    const range = selection.getRangeAt(0);
    const textNode = range.startContainer;
    
    if (textNode.nodeType !== Node.TEXT_NODE) return;
    
    // Remove the shortcode text (including the colon)
    const offset = range.startOffset;
    const beforeShortcode = textNode.textContent.substring(0, shortcodeStart);
    const afterCursor = textNode.textContent.substring(offset);
    
    textNode.textContent = beforeShortcode + emoji + afterCursor;
    
    // Place cursor after emoji
    const newOffset = shortcodeStart + emoji.length;
    range.setStart(textNode, newOffset);
    range.setEnd(textNode, newOffset);
    
    clearShortcode();
    element.focus();
}

//# sourceMappingURL=rich-text-editor.js.map