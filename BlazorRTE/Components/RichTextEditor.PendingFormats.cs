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
    private string? _pendingTextColor;
    private string? _pendingBackgroundColor;
    private string? _pendingFontSize;
    private string? _pendingFontFamily;
    private bool _hasPendingFormats => _pendingFormats.Count > 0 
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
            // Scenario 2: No text selected → toggle pending format
            var formatName = GetFormatName(command);
            if (formatName != null)
            {
                TogglePendingFormat(formatName);
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
            StateHasChanged();
        }
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
            _pendingBackgroundColor = _pendingBackgroundColor == color ? null : color;
            _currentHighlightColor = color;
            StateHasChanged();
        }
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

    private async Task UpdateToolbarForPendingFormats()
    {
        // Merge pending formats with active formats for visual feedback
        foreach (var format in _pendingFormats)
        {
            _activeFormats.Add(format);
        }
        StateHasChanged();
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
            // Build pending format data for JS
            var pendingData = new PendingFormatData
            {
                Formats = [.. _pendingFormats],
                TextColor = _pendingTextColor,
                BackgroundColor = _pendingBackgroundColor,
                FontSize = _pendingFontSize,
                FontFamily = _pendingFontFamily
            };

            await _jsModule.InvokeVoidAsync("applyPendingFormats", pendingData);
            ClearPendingFormats();
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
        _pendingTextColor = null;
        _pendingBackgroundColor = null;
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
    /// </summary>
    protected bool IsFormatActiveOrPending(string formatName) =>
        _activeFormats.Contains(formatName) || _pendingFormats.Contains(formatName);
}

/// <summary>
/// Data class for pending format transfer to JS.
/// </summary>
public class PendingFormatData
{
    public string[] Formats { get; set; } = [];
    public string? TextColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? FontSize { get; set; }
    public string? FontFamily { get; set; }
}