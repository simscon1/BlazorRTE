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
    
    if (document.queryCommandState('bold')) formats.push('bold');
    if (document.queryCommandState('italic')) formats.push('italic');
    if (document.queryCommandState('underline')) formats.push('underline');
    if (document.queryCommandState('strikeThrough')) formats.push('strikeThrough');
    if (document.queryCommandState('insertUnorderedList')) formats.push('insertUnorderedList');
    if (document.queryCommandState('insertOrderedList')) formats.push('insertOrderedList');
    
    return formats;
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