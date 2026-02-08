namespace BlazorRTE.EventArgs;

/// <summary>
/// Event args for HTML content changes
/// </summary>
public class HtmlChangedEventArgs
{
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
    public int WordCount { get; set; }
    public ChangeSource Source { get; set; }
}

/// <summary>
/// Source of content change
/// </summary>
public enum ChangeSource
{
    User,
    Paste,
    Programmatic,
    Command,
    EmojiShortcode,
    EmojiPicker
}

/// <summary>
/// Event args for selection changes
/// </summary>
public class SelectionChangedEventArgs
{
    public string SelectedText { get; set; } = string.Empty;
    public bool HasSelection { get; set; }
    public List<string> ActiveFormats { get; set; } = new();
}

/// <summary>
/// Event args for format changes
/// </summary>
public class FormatChangedEventArgs
{
    public List<string> ActiveFormats { get; set; } = new();
    public string TextColor { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string Alignment { get; set; } = string.Empty;
    public string HeadingLevel { get; set; } = string.Empty;
}

/// <summary>
/// Event args for paste operations
/// </summary>
public class PasteEventArgs
{
    public string ClipboardContent { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public bool Cancel { get; set; }
}

/// <summary>
/// Event args for keyboard shortcuts
/// </summary>
public class ShortcutEventArgs
{
    public string Shortcut { get; set; } = string.Empty;
    public HelperClasses.FormatCommand? Command { get; set; }
    public bool Handled { get; set; }
}

/// <summary>
/// Event args for link operations
/// </summary>
public class LinkEventArgs
{
    public string Url { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public bool Cancel { get; set; }
}

/// <summary>
/// Event args for command execution
/// </summary>
public class CommandEventArgs
{
    public HelperClasses.FormatCommand Command { get; set; }
    public string CommandName { get; set; } = string.Empty;
    public object? Value { get; set; }
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
    public bool Cancel { get; set; }
}

/// <summary>
/// Event args for emoji operations
/// </summary>
public class EmojiEventArgs
{
    public string Emoji { get; set; } = string.Empty;
    public string? Shortcode { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Event args for validation failures
/// </summary>
public class ValidationEventArgs
{
    public string ErrorMessage { get; set; } = string.Empty;
    public ValidationType Type { get; set; }
}

/// <summary>
/// Type of validation error
/// </summary>
public enum ValidationType
{
    MaxLength,
    InvalidHtml,
    Other
}

/// <summary>
/// Event args for errors
/// </summary>
public class ErrorEventArgs
{
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public ErrorSeverity Severity { get; set; }
}

/// <summary>
/// Error severity levels
/// </summary>
public enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical
}