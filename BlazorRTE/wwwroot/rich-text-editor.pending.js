// ===== PENDING FORMAT SUPPORT =====
// Handles keyboard-only formatting scenarios

let pendingFormatState = {
    formats: [],
    textColor: null,
    backgroundColor: null,
    fontSize: null,
    fontFamily: null,
    isApplying: false
};

export function hasTextSelection() {
    const selection = window.getSelection();
    return selection && selection.rangeCount > 0 && !selection.isCollapsed;
}

export function keepSelectionAfterFormat() {
    const selection = window.getSelection();
    if (selection && selection.rangeCount > 0) {
        const range = selection.getRangeAt(0);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

export function collapseSelectionToEnd(element) {
    const selection = window.getSelection();
    if (selection && selection.rangeCount > 0) {
        const range = selection.getRangeAt(0);
        range.collapse(false);
        selection.removeAllRanges();
        selection.addRange(range);
    }
    element.focus();
}

export function applyPendingFormats(pendingData) {
    if (!pendingData) return;

    const hasFormats = (pendingData.formats?.length > 0)
        || pendingData.textColor
        || pendingData.backgroundColor
        || pendingData.fontSize
        || pendingData.fontFamily;

    // Chrome/Edge: when turning OFF pending formats, Chrome inherits the caret
    // formatting from adjacent elements (e.g. cursor after <strong> still types bold).
    // execCommand resets the internal caret state so the next typed char is plain.
    // queryCommandState guard prevents accidentally toggling ON in Firefox (where
    // the state is already false and calling execCommand would re-enable the format).
    if (!hasFormats && pendingFormatState.formats.length > 0) {
        for (const fmt of pendingFormatState.formats) {
            try {
                if (document.queryCommandState(fmt)) {
                    document.execCommand(fmt, false, null);
                }
            } catch (e) { /* execCommand deprecated - ignore */ }
        }
    }

    pendingFormatState = {
        formats:         pendingData.formats || [],
        textColor:       pendingData.textColor,
        backgroundColor: pendingData.backgroundColor,
        fontSize:        pendingData.fontSize,
        fontFamily:      pendingData.fontFamily,
        isApplying:      hasFormats
    };
}

export function handleKeyPressWithPendingFormats(element, event, dotNetRef) {
    if (!pendingFormatState.isApplying) return false;

    if (event.key.length !== 1 || event.ctrlKey || event.metaKey || event.altKey) {
        return false;
    }

    // ── Extend existing run ───────────────────────────────────────────────────
    // If the cursor is already inside elements that cover all pending formats,
    // let the browser insert natively so the character is appended to the SAME
    // element rather than each character getting its own wrapper tag.
    //
    //   Old output: <strong>B</strong><strong>r</strong><strong>o</strong>…
    //   New output: <strong>Bro…</strong>
    //
    // Space is still intercepted even in this case because a trailing regular
    // space inside an inline element collapses in Chrome/Edge; &nbsp; prevents it.
    // ─────────────────────────────────────────────────────────────────────────
    if (cursorIsInsideMatchingFormats()) {
        if (event.key === ' ') {
            event.preventDefault();
            document.execCommand('insertText', false, '\u00A0');
            dotNetRef.invokeMethodAsync('HandleContentChangedFromJs', element.innerHTML)
                .catch(err => console.error('Content change error:', err));
            return true;
        }
        // Non-space: browser handles naturally; Blazor OnInput tracks the change.
        return false;
    }
    // ─────────────────────────────────────────────────────────────────────────

    event.preventDefault();

    // First character of a new format run — build the wrapper element.
    // Use non-breaking space so a space character is visible even at the
    // trailing edge of an inline element (CSS would collapse a regular space).
    const charToRender = event.key === ' ' ? '\u00A0' : event.key;
    let html = escapeHtml(charToRender);

    // Build nested formatting tags (outermost → innermost wrapping order)
    if (pendingFormatState.fontFamily) {
        html = `<font face="${pendingFormatState.fontFamily}">${html}</font>`;
    }
    if (pendingFormatState.fontSize) {
        html = `<font size="${pendingFormatState.fontSize}">${html}</font>`;
    }
    if (pendingFormatState.textColor) {
        html = `<font color="${pendingFormatState.textColor}">${html}</font>`;
    }
    if (pendingFormatState.backgroundColor) {
        html = `<span style="background-color: ${pendingFormatState.backgroundColor}">${html}</span>`;
    }
    for (const format of pendingFormatState.formats) {
        switch (format) {
            case 'bold':          html = `<strong>${html}</strong>`; break;
            case 'italic':        html = `<em>${html}</em>`;         break;
            case 'underline':     html = `<u>${html}</u>`;           break;
            case 'strikeThrough': html = `<del>${html}</del>`;       break;
            case 'subscript':     html = `<sub>${html}</sub>`;       break;
            case 'superscript':   html = `<sup>${html}</sup>`;       break;
        }
    }

    insertHtmlAtCursor(html);

    dotNetRef.invokeMethodAsync('HandleContentChangedFromJs', element.innerHTML)
        .catch(err => console.error('Content change error:', err));

    return true;
}

export function clearPendingFormats() {
    pendingFormatState = {
        formats: [],
        textColor: null,
        backgroundColor: null,
        fontSize: null,
        fontFamily: null,
        isApplying: false
    };
}

export function getPendingFormatState() {
    return pendingFormatState;
}

// ===== HELPER FUNCTIONS =====

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Returns true when the cursor sits inside ancestor elements that collectively
 * satisfy every pending format requirement (bold, italic, color, etc.).
 * When true, the next keypress can be handled natively by the browser — the
 * character will be appended inside the existing element automatically.
 */
function cursorIsInsideMatchingFormats() {
    const selection = window.getSelection();
    if (!selection.rangeCount || !selection.isCollapsed) return false;

    const range = selection.getRangeAt(0);
    let node = range.startContainer;
    if (node.nodeType === Node.TEXT_NODE) node = node.parentElement;

    const remaining = new Set(pendingFormatState.formats);
    let needsColor  = !!pendingFormatState.textColor;
    let needsBg     = !!pendingFormatState.backgroundColor;
    let needsSize   = !!pendingFormatState.fontSize;
    let needsFamily = !!pendingFormatState.fontFamily;

    let current = node;
    while (current && current !== document.body) {
        const tag = current.tagName?.toLowerCase();
        if (tag === 'strong' || tag === 'b')  remaining.delete('bold');
        if (tag === 'em'     || tag === 'i')  remaining.delete('italic');
        if (tag === 'u')                      remaining.delete('underline');
        if (tag === 'del'    || tag === 's')  remaining.delete('strikeThrough');
        if (tag === 'sub')                    remaining.delete('subscript');
        if (tag === 'sup')                    remaining.delete('superscript');
        if (tag === 'font') {
            if (current.color) needsColor  = false;
            if (current.size)  needsSize   = false;
            if (current.face)  needsFamily = false;
        }
        if (current.style?.backgroundColor)  needsBg = false;
        current = current.parentElement;
    }

    return remaining.size === 0 && !needsColor && !needsBg && !needsSize && !needsFamily;
}

function insertHtmlAtCursor(html) {
    const selection = window.getSelection();
    if (!selection.rangeCount) return;

    const range = selection.getRangeAt(0);
    range.deleteContents();

    const fragment = document.createRange().createContextualFragment(html);
    const lastNode = fragment.lastChild;
    range.insertNode(fragment);

    if (lastNode) {
        // Place cursor INSIDE the element at the end of its deepest text node,
        // not AFTER the element. This is the key change that prevents fragmentation:
        // the next keypress finds cursorIsInsideMatchingFormats() === true and lets
        // the browser append to the same element instead of creating a new wrapper.
        const innerText = getDeepestLastTextNode(lastNode);
        const newRange  = document.createRange();
        if (innerText) {
            newRange.setStart(innerText, innerText.textContent.length);
        } else {
            newRange.setStart(lastNode, lastNode.childNodes.length);
        }
        newRange.collapse(true);
        selection.removeAllRanges();
        selection.addRange(newRange);
    }
}

/** Walks to the deepest last-child text node of a given node. */
function getDeepestLastTextNode(node) {
    if (node.nodeType === Node.TEXT_NODE) return node;
    const last = node.lastChild;
    return last ? getDeepestLastTextNode(last) : null;
}

export function handleTabBackToEditor(element, hadSelection) {
    element.focus();
    if (hadSelection) {
        collapseSelectionToEnd(element);
    }
}