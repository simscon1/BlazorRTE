using BlazorRTE.HelperClasses;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorRTE.Components;

/// <summary>
/// Partial class handling pending format state for keyboard-only formatting.
/// Scenario 1: Text selected + click button → apply immediately, keep text selected
/// Scenario 2: No text selected + click button → queue pending format for next typed text
/// </summary>
public partial class RichTextEditor
{
    // ===== PENDING FORMAT STATE =====
    private HashSet<string> _pendingFormats = [];

    /// <summary>
    /// Formats the user has explicitly turned OFF while the cursor sits inside a
    /// matching element. Prevents _activeFormats (DOM query) from overriding the
    /// user's intent after UpdateToolbarState() fires (e.g. after color is applied).
    /// Cleared automatically when the cursor leaves the formatted region.
    /// </summary>
    private HashSet<string> _suppressedFormats = [];

    private string? _pendingTextColor;
    private string? _pendingBackgroundColor;
    private string? _pendingFontSize;
    private string? _pendingFontFamily;
    private bool _suppressBackgroundColor = false;
    private bool _hasPendingFormats => _pendingFormats.Count > 0
        || _suppressedFormats.Count > 0
        || _suppressBackgroundColor
        || _pendingTextColor != null
        || _pendingBackgroundColor != null
        || _pendingFontSize != null
        || _pendingFontFamily != null;

    /// <summary>
    /// Handles toolbar button clicks with pending format logic.
    /// </summary>
    private async Task ExecuteCommandWithPendingFormat(FormatCommand command)
    {
        if (_jsModule == null) return;

        var hasSelection = await _jsModule.InvokeAsync<bool>("hasTextSelection");

        if (hasSelection)
        {
            // Scenario 1: Text is selected → apply immediately, keep selection
            await ExecuteCommand(command);
            await _jsModule.InvokeVoidAsync("keepSelectionAfterFormat");
        }
        else
        {
            // Scenario 2: No text selected → toggle pending format.
            // Use IsFormatActiveOrPending (not just _pendingFormats) to determine
            // direction, so clicking bold OFF while cursor is inside a bold element
            // correctly suppresses it instead of re-adding it to pending.
            var formatName = GetFormatName(command);
            if (formatName != null)
            {
                if (IsFormatActiveOrPending(formatName))
                {
                    // Currently appears ON → user intends to turn it OFF
                    _pendingFormats.Remove(formatName);
                    _suppressedFormats.Add(formatName);
                }
                else
                {
                    // Currently appears OFF → user intends to turn it ON
                    _pendingFormats.Add(formatName);
                    _suppressedFormats.Remove(formatName);
                }

                await SyncPendingFormatsToJs();
                await UpdateToolbarForPendingFormats();
            }
        }

        // After command execution, refocus toolbar button
        if (_jsModule != null && _toolbarFocusIndex >= 0 && _toolbarFocusIndex < _toolbarButtonIds.Count)
        {
            await _jsModule.InvokeVoidAsync("focusElementById", _toolbarButtonIds[_toolbarFocusIndex]);
        }
    }

    /// <summary>
    /// Pushes current C# pending state to the JS module so the keypress
    /// handler can wrap typed characters without a JS→C# round-trip.
    /// </summary>
    private async Task SyncPendingFormatsToJs()
    {
        if (_jsModule == null) return;

        await _jsModule.InvokeVoidAsync("applyPendingFormats", new PendingFormatData
        {
            Formats           = [.. _pendingFormats],
            SuppressedFormats = [.. _suppressedFormats],
            TextColor         = _pendingTextColor,
            BackgroundColor   = _pendingBackgroundColor,
            SuppressBackgroundColor = _suppressBackgroundColor,
            FontSize          = _pendingFontSize,
            FontFamily        = _pendingFontFamily
        });
    }
     
    /// <summary>
    /// Handles color selection with pending format logic.
    /// </summary>
    private async Task SelectTextColorWithPending(string color)
    {
        if (_jsModule == null) return;

        var hasSelection = await _jsModule.InvokeAsync<bool>("hasTextSelection");

        if (hasSelection)
        {
            await SelectTextColor(color);
            await _jsModule.InvokeVoidAsync("keepSelectionAfterFormat");
        }
        else
        {
            _pendingTextColor = _pendingTextColor == color ? null : color;
            _currentTextColor = color;
            await SyncPendingFormatsToJs();
            _showTextColorPicker = false;  // ← ADD: Close the picker
            StateHasChanged();
        }

        // Refocus toolbar button after selection
        await FocusToolbarButton(_toolbarFocusIndex);
    }

    /// <summary>
    /// Handles background color selection with pending format logic.
    /// </summary>
    private async Task SelectBackgroundColorWithPending(string color)
    {
        if (_jsModule == null) return;

        var hasSelection = await _jsModule.InvokeAsync<bool>("hasTextSelection");

        if (hasSelection)
        {
            await SelectBackgroundColor(color);
            await _jsModule.InvokeVoidAsync("keepSelectionAfterFormat");
        }
        else
        {
            bool turningOff = _pendingBackgroundColor == color || color == "transparent";
            
            if (turningOff)
            {
                _pendingBackgroundColor = null;
                _suppressBackgroundColor = true;
            }
            else
            {
                _pendingBackgroundColor = color;
                _suppressBackgroundColor = false;
            }
            
            _currentHighlightColor = color;
            await SyncPendingFormatsToJs();
            _showBackgroundColorPicker = false;
            StateHasChanged();
        }

        // Refocus toolbar button after selection
        await FocusToolbarButton(_toolbarFocusIndex);
    }

    /// <summary>
    /// Handles font size selection with pending format logic.
    /// </summary>
    private async Task SelectFontSizeWithPending(string size)
    {
        if (_jsModule == null) return;

        var hasSelection = await _jsModule.InvokeAsync<bool>("hasTextSelection");

        if (hasSelection)
        {
            await SelectFontSize(size);
            await _jsModule.InvokeVoidAsync("keepSelectionAfterFormat");
        }
        else
        {
            _pendingFontSize = _pendingFontSize == size ? null : size;
            _currentFontSize = size;
            StateHasChanged();
        }

        _showFontSizePicker = false;
    }

    /// <summary>
    /// Handles font family selection with pending format logic.
    /// </summary>
    private async Task SelectFontFamilyWithPending(string fontFamily)
    {
        if (_jsModule == null) return;

        var hasSelection = await _jsModule.InvokeAsync<bool>("hasTextSelection");

        if (hasSelection)
        {
            await SelectFontFamily(fontFamily);
            await _jsModule.InvokeVoidAsync("keepSelectionAfterFormat");
        }
        else
        {
            _pendingFontFamily = _pendingFontFamily == fontFamily ? null : fontFamily;
            StateHasChanged();
        }

        _showFontFamilyPicker = false;
    }

    private void TogglePendingFormat(string formatName)
    {
        if (_pendingFormats.Contains(formatName))
        {
            _pendingFormats.Remove(formatName);
        }
        else
        {
            _pendingFormats.Add(formatName);
        }
    }

    private Task UpdateToolbarForPendingFormats()
    {
        // Do NOT merge into _activeFormats - IsFormatActiveOrPending() checks _pendingFormats
        // directly, so adding them here only causes stale state after ClearPendingFormats().
        StateHasChanged();
        return Task.CompletedTask;
    }

    private static string? GetFormatName(FormatCommand command) => command switch
    {
        FormatCommand.Bold => "bold",
        FormatCommand.Italic => "italic",
        FormatCommand.Underline => "underline",
        FormatCommand.Strikethrough => "strikeThrough",
        FormatCommand.Subscript => "subscript",
        FormatCommand.Superscript => "superscript",
        _ => null
    };

    /// <summary>
    /// Called from JS when user starts typing - applies pending formats.
    /// </summary>
    [JSInvokable]
    public async Task ApplyPendingFormatsOnInput()
    {
        if (!_hasPendingFormats || _jsModule == null) return;

        try
        {
            var pendingData = new PendingFormatData
            {
                Formats           = [.. _pendingFormats],
                SuppressedFormats = [.. _suppressedFormats],
                TextColor         = _pendingTextColor,
                BackgroundColor   = _pendingBackgroundColor,
                SuppressBackgroundColor = _suppressBackgroundColor,
                FontSize          = _pendingFontSize,
                FontFamily        = _pendingFontFamily
            };

            await _jsModule.InvokeVoidAsync("applyPendingFormats", pendingData);
            ClearPendingFormats();
            await InvokeAsync(UpdateToolbarState); // ← re-sync toolbar with actual DOM state
        }
        catch (Exception ex)
        {
            await RaiseErrorEvent("Failed to apply pending formats", ex);
        }
    }

    /// <summary>
    /// Called when tabbing back into editor or pressing arrow keys.
    /// </summary>
    [JSInvokable]
    public Task OnEditorReentryFromToolbar(bool hadSelection)
    {
        if (hadSelection)
        {
            // Move cursor to end of previously selected text, clear selection
            // This is handled in JS
        }
        // Pending formats remain active for next typed text
        return Task.CompletedTask;
    }

    private void ClearPendingFormats()
    {
        _pendingFormats.Clear();
        _suppressedFormats.Clear();
        _pendingTextColor = null;
        _pendingBackgroundColor = null;
        _suppressBackgroundColor = false;
        _pendingFontSize = null;
        _pendingFontFamily = null;
    }

    /// <summary>
    /// Checks if a format is pending (for toolbar button state).
    /// </summary>
    protected bool IsFormatPending(string formatName) =>
        _pendingFormats.Contains(formatName);

    /// <summary>
    /// Gets combined active/pending state for toolbar visual.
    /// A format that is in _activeFormats but also in _suppressedFormats is treated
    /// as OFF — the user explicitly clicked it off and we must honor that intent even
    /// when the cursor is still physically inside the formatted element.
    /// </summary>
    protected bool IsFormatActiveOrPending(string formatName) =>
        (_activeFormats.Contains(formatName) && !_suppressedFormats.Contains(formatName))
        || _pendingFormats.Contains(formatName);
}

/// <summary>
/// Data class for pending format transfer to JS.
/// </summary>
public class PendingFormatData
{
    public string[] Formats { get; set; } = [];

    /// <summary>
    /// Formats the user explicitly turned off while the cursor is inside a matching
    /// element. Passed to JS so Chrome/Edge's caret-inheritance can be reset via
    /// execCommand before the next typed character.
    /// </summary>
    public string[] SuppressedFormats { get; set; } = [];

    public string? TextColor { get; set; }
    public string? BackgroundColor { get; set; }
    public bool SuppressBackgroundColor { get; set; }
    public string? FontSize { get; set; }
    public string? FontFamily { get; set; }
}