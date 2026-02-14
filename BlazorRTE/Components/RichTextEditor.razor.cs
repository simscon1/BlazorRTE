using BlazorEmo.Models;
using BlazorEmo.Services;
using BlazorRTE.EventArgs;
using BlazorRTE.HelperClasses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorRTE.Components
{
    public partial class RichTextEditor : ComponentBase, IAsyncDisposable
    {
        // ===== EXISTING PARAMETERS =====
        [Parameter] public string Value { get; set; } = string.Empty;
        [Parameter] public EventCallback<string> ValueChanged { get; set; }
        [Parameter] public string Placeholder { get; set; } = "Type your message...";
        [Parameter] public string AriaLabel { get; set; } = "Rich text editor";
        [Parameter] public bool ShowToolbar { get; set; } = true;
        [Parameter] public int MaxLength { get; set; } = 5000;
        [Parameter] public EventCallback OnFocusChanged { get; set; }
        [Parameter] public bool ShowCharacterCount { get; set; } = true;
        [Parameter] public string MinHeight { get; set; } = "200px";
        [Parameter] public string MaxHeight { get; set; } = "600px";
        [Parameter] public bool EnableEmojiShortcodes { get; set; } = true;

        /// <summary>
        /// Enable dark mode styling for the editor
        /// </summary>
        [Parameter]
        public bool DarkMode { get; set; } = false;

        // ===== NEW PARAMETERS =====

        /// <summary>
        /// When true, pressing Enter without Shift will trigger OnEnterKeyPressed instead of creating a newline.
        /// Press Shift+Enter to create a newline.
        /// </summary>
        [Parameter]
        public bool BypassEnterKey { get; set; } = false;

        /// <summary>
        /// Event callback triggered when Enter key is pressed and BypassEnterKey is true.
        /// Use this to send messages in chat applications.
        /// </summary>
        [Parameter]
        public EventCallback OnEnterKeyPressed { get; set; }

        // ===== EVENT CALLBACKS =====
        [Parameter] public EventCallback<string> OnContentChanged { get; set; }
        [Parameter] public EventCallback<HtmlChangedEventArgs> OnHtmlChanged { get; set; }
        [Parameter] public EventCallback<SelectionChangedEventArgs> OnSelectionChanged { get; set; }
        [Parameter] public EventCallback<string> OnSelectedTextChanged { get; set; }
        [Parameter] public EventCallback<FormatChangedEventArgs> OnFormatChanged { get; set; }
        [Parameter] public EventCallback<CommandEventArgs> OnBeforeCommand { get; set; }
        [Parameter] public EventCallback<CommandEventArgs> OnAfterCommand { get; set; }
        [Parameter] public EventCallback OnFocused { get; set; }
        [Parameter] public EventCallback OnBlurred { get; set; }
        [Parameter] public EventCallback<PasteEventArgs> OnBeforePaste { get; set; }
        [Parameter] public EventCallback<PasteEventArgs> OnAfterPaste { get; set; }
        [Parameter] public EventCallback<ShortcutEventArgs> OnShortcutPressed { get; set; }
        [Parameter] public EventCallback OnMaxLengthReached { get; set; }
        [Parameter] public EventCallback<int> OnCharacterCountChanged { get; set; }
        [Parameter] public EventCallback<int> OnWordCountChanged { get; set; }
        [Parameter] public EventCallback<LinkEventArgs> OnLinkCreated { get; set; }
        [Parameter] public EventCallback<LinkEventArgs> OnLinkRemoved { get; set; }
        [Parameter] public EventCallback<LinkEventArgs> OnLinkClicked { get; set; }
        [Parameter] public EventCallback OnUndo { get; set; }
        [Parameter] public EventCallback OnRedo { get; set; }
        [Parameter] public EventCallback<bool> OnDirtyStateChanged { get; set; }
        [Parameter] public EventCallback<EmojiEventArgs> OnEmojiInserted { get; set; }
        [Parameter] public EventCallback OnEmojiPickerOpened { get; set; }
        [Parameter] public EventCallback OnEmojiPickerClosed { get; set; }
        [Parameter] public EventCallback<string> OnEmojiShortcodeProcessed { get; set; }
        [Parameter] public EventCallback<string> OnColorPickerOpened { get; set; }
        [Parameter] public EventCallback<string> OnColorPickerClosed { get; set; }
        [Parameter] public EventCallback<EventArgs.ErrorEventArgs> OnError { get; set; }

        // ===== ONLY INJECT JSRuntime - NO SERVICES! =====
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        // ===== Use simple provider instead of injected service =====
        private readonly EmojiProvider _emojiProvider = new();

        protected ElementReference _editorRef;
        protected bool IsFocused { get; set; }

        private IJSObjectReference? _jsModule;
        private DotNetObjectReference<RichTextEditor>? _dotNetRef;
        private HashSet<string> _activeFormats = [];
        private bool _isUpdating;
        private string _previousValue = string.Empty;
        private string _currentHeadingLevel = "";
        private bool _showTextColorPicker;
        private bool _showBackgroundColorPicker;
        private bool _showFontSizePicker;
        private bool _showFontFamilyPicker;
        private bool _showHeadingPicker;
        private ElementReference _headingButton;
        private ElementReference _headingPalette;
        private string _currentFontSize = "3"; // Default to Normal (14px = size 3)
                                               // Add to the RichTextEditor class properties section (around line 15-20):

        protected readonly string _editorId = $"rte-{Guid.NewGuid():N}";

        private readonly Dictionary<string, string> _fontSizes = new()
        {
            { "1", "Small (10px)" },
            { "3", "Normal (14px)" },
            { "4", "Medium (16px)" },
            { "5", "Large (18px)" },
            { "6", "X-Large (24px)" },
            { "7", "XX-Large (32px)" }
        };

        private readonly Dictionary<string, string> _fontFamilies = new()
        {
            { "Arial", "Arial" },
            { "Courier New", "Courier New" },
            { "Garamond", "Garamond" },
            { "Georgia", "Georgia" },
            { "Helvetica", "Helvetica" },
            { "Impact", "Impact" },
            { "Tahoma", "Tahoma" },
            { "Times New Roman", "Times New Roman" },
            { "Trebuchet MS", "Trebuchet MS" },
            { "Verdana", "Verdana" }
        };

        private string alignment = "left";
        private int focusedIndex;
        private const int ToolbarButtonCount = 25;

        private ElementReference _fontFamilyButton;
        private ElementReference _fontFamilyPalette;
        private ElementReference _textColorButton;
        private ElementReference _textColorPalette;
        private ElementReference _bgColorButton;
        private ElementReference _bgColorPalette;
        private ElementReference _fontSizeButton;
        private ElementReference _fontSizePalette;

        private const int ColorGridColumns = 3;

        private string _currentTextColor = "#000000";
        private string _currentHighlightColor = "#FFFFFF";

        private bool _showEmojiPicker;
        private ElementReference _emojiButton;

        private Dictionary<string, string>? _emojiShortcodeMap;

        private bool _isEmojiSelected;

        private bool _preventTabDefault = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    _dotNetRef = DotNetObjectReference.Create(this);
                    _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorRTE/rich-text-editor.js");
                    await _jsModule.InvokeVoidAsync("initializeEditor", _editorRef, _dotNetRef, _editorId); // Pass editor ID

                    if (!string.IsNullOrEmpty(Value))
                    {
                        await SetHtmlAsync(Value);
                        _previousValue = Value;
                    }

                    if (EnableEmojiShortcodes)
                    {
                        _ = LoadEmojiShortcodesAsync();
                    }
                }
                catch
                {
                    // Initialization failed silently
                }
            }

            // Position emoji picker if it's open
            if (_showEmojiPicker && _jsModule != null && _emojiButton.Id != null)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustEmojiPickerPositionByQuery", _emojiButton);
                }
                catch { }
            }
        }

        private bool _previousDarkMode = false;

        protected override async Task OnParametersSetAsync()
        {
            if (_jsModule != null && Value != _previousValue && !_isUpdating)
            {
                _isUpdating = true;
                try
                {
                    var sanitized = HtmlSanitizer.Sanitize(Value);
                    await _jsModule.InvokeVoidAsync("setHtml", _editorRef, sanitized);

                    if (sanitized != Value)
                    {
                        Value = sanitized;
                        _previousValue = sanitized;
                        await ValueChanged.InvokeAsync(sanitized);
                    }
                    else
                    {
                        _previousValue = Value;
                    }
                }
                catch
                {
                    // Parameter update failed silently
                }
                finally
                {
                    _isUpdating = false;
                }
            }

            // FIXED: Use InvokeAsync to ensure re-render happens on UI thread
            if (_previousDarkMode != DarkMode)
            {
                _previousDarkMode = DarkMode;
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task LoadEmojiShortcodesAsync()
        {
            if (_emojiShortcodeMap != null) return;

            _emojiShortcodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Add common emoticons first (these take priority)
            _emojiShortcodeMap["D"] = "😀";  // :D
            _emojiShortcodeMap["P"] = "😛";  // :P
            _emojiShortcodeMap["p"] = "😛";  // :p
            _emojiShortcodeMap["O"] = "😮";  // :O
            _emojiShortcodeMap["o"] = "😮";  // :o
            _emojiShortcodeMap[")"] = "😊";  // :)
            _emojiShortcodeMap["("] = "😞";  // :(
            _emojiShortcodeMap["/"] = "😕";  // :/
            _emojiShortcodeMap["|"] = "😐";  // :|

            try
            {
                // Use the new EmojiProvider - no HTTP calls!
                var categories = await _emojiProvider.GetAllCategoriesAsync();

                foreach (var category in categories)
                {
                    foreach (var emoji in category.Emojis)
                    {
                        if (!string.IsNullOrEmpty(emoji.Code))
                            _emojiShortcodeMap.TryAdd(emoji.Code, emoji.Char);

                        if (!string.IsNullOrEmpty(emoji.Name))
                            _emojiShortcodeMap.TryAdd(emoji.Name.ToLowerInvariant().Replace(' ', '_'), emoji.Char);

                        foreach (var keyword in emoji.Keywords)
                        {
                            if (!string.IsNullOrEmpty(keyword))
                                _emojiShortcodeMap.TryAdd(keyword.ToLowerInvariant(), emoji.Char);
                        }
                    }
                }
            }
            catch { }
        }

        [JSInvokable]
        public async Task<string?> ProcessEmojiShortcode(string shortcode)
        {
            if (!EnableEmojiShortcodes || string.IsNullOrEmpty(shortcode))
                return null;

            if (_emojiShortcodeMap == null)
                await LoadEmojiShortcodesAsync();

            var emoji = _emojiShortcodeMap?.GetValueOrDefault(shortcode);

            if (emoji != null && OnEmojiShortcodeProcessed.HasDelegate)
            {
                await OnEmojiShortcodeProcessed.InvokeAsync(shortcode);
            }

            return emoji;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_jsModule != null)
                {
                    await _jsModule.InvokeVoidAsync("disposeEditor", _editorRef);
                    await _jsModule.DisposeAsync();
                }
                _dotNetRef?.Dispose();
            }
            catch { }
        }

        protected async Task OnInput(ChangeEventArgs e)
        {
            if (_isUpdating) return;

            try
            {
                _isUpdating = true;

                var html = await GetHtmlAsync();
                var sanitized = HtmlSanitizer.Sanitize(html);
                var textOnly = HtmlSanitizer.StripHtml(sanitized);

                if (textOnly.Length > MaxLength)
                {
                    // NEW: Raise max length event
                    if (OnMaxLengthReached.HasDelegate)
                    {
                        await OnMaxLengthReached.InvokeAsync();
                    }
                    return;
                }

                Value = sanitized;
                _previousValue = sanitized;
                await ValueChanged.InvokeAsync(sanitized);

                // NEW: Raise content changed events
                await RaiseContentChangedEvent(ChangeSource.User);

                await UpdateHeadingState();
            }
            catch (Exception ex)
            {
                // NEW: Raise error event
                await RaiseErrorEvent("Input handling failed", ex);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        protected async Task OnKeyDown(KeyboardEventArgs e)
        {
            // NEW: Handle Enter key bypass for chat-style send functionality
            if (BypassEnterKey && e.Key == "Enter" && !e.ShiftKey && !e.CtrlKey && !e.MetaKey)
            {
                // Prevent default newline behavior
                if (OnEnterKeyPressed.HasDelegate)
                {
                    await OnEnterKeyPressed.InvokeAsync();
                }
                return;
            }

            if (e.CtrlKey || e.MetaKey)
            {
                // Handle Ctrl+Alt shortcuts (Headings)
                if (e.AltKey)
                {
                    switch (e.Key)
                    {
                        case "0":
                            await ExecuteCommand(FormatCommand.Paragraph);
                            return;
                        case "1":
                            await ExecuteCommand(FormatCommand.HeadingH1);
                            return;
                        case "2":
                            await ExecuteCommand(FormatCommand.HeadingH2);
                            return;
                        case "3":
                            await ExecuteCommand(FormatCommand.HeadingH3);
                            return;
                    }
                }

                // Handle Ctrl+Shift shortcuts
                if (e.ShiftKey)
                {
                    switch (e.Key.ToLower())
                    {
                        case "x":
                            await ExecuteCommand(FormatCommand.Strikethrough);
                            return;
                        case "z":
                            await ExecuteCommand(FormatCommand.Redo);
                            return;
                        case "k":
                            await RemoveLink();
                            return;
                        case "=":
                        case "+":
                            await ExecuteCommand(FormatCommand.Superscript);
                            return;
                        case "*":
                        case "8":
                            await ExecuteCommand(FormatCommand.InsertUnorderedList);
                            return;
                        case "&":
                        case "7":
                            await ExecuteCommand(FormatCommand.InsertOrderedList);
                            return;
                    }

                    if (e.Key == ">" || e.Key == ".")
                    {
                        await IncreaseFontSize();
                        return;
                    }
                    if (e.Key == "<" || e.Key == ",")
                    {
                        await DecreaseFontSize();
                        return;
                    }
                }

                // Handle regular Ctrl shortcuts
                switch (e.Key.ToLower())
                {
                    case "b":
                        await ExecuteCommand(FormatCommand.Bold);
                        break;
                    case "i":
                        await ExecuteCommand(FormatCommand.Italic);
                        break;
                    case "u":
                        await ExecuteCommand(FormatCommand.Underline);
                        break;
                    case "z":
                        await ExecuteCommand(FormatCommand.Undo);
                        break;
                    case "y":
                        await ExecuteCommand(FormatCommand.Redo);
                        break;
                    case "l":
                        await ExecuteCommand(FormatCommand.AlignLeft);
                        break;
                    case "e":
                        await ExecuteCommand(FormatCommand.AlignCenter);
                        break;
                    case "r":
                        await ExecuteCommand(FormatCommand.AlignRight);
                        break;
                    case "j":
                        await ExecuteCommand(FormatCommand.AlignJustify);
                        break;
                    case "[":
                        await ExecuteCommand(FormatCommand.Outdent);
                        break;
                    case "]":
                        await ExecuteCommand(FormatCommand.Indent);
                        break;
                    case "\\":
                        await ExecuteCommand(FormatCommand.RemoveFormat);
                        break;
                    case "=":
                        await ExecuteCommand(FormatCommand.Subscript);
                        break;
                }
            }

            if ((e.CtrlKey || e.MetaKey) && e.Key == "Enter")
            {
                await ExecuteCommand(FormatCommand.HorizontalRule);
                return;
            }

            // After handling keyboard shortcuts, update colors
            if (e.Key == "ArrowLeft" || e.Key == "ArrowRight" || 
                e.Key == "ArrowUp" || e.Key == "ArrowDown" ||
                e.Key == "Home" || e.Key == "End")
            {
                // Delay slightly to let cursor move first
                await Task.Delay(10);
                await UpdateToolbarState(); // ✅ This already updates colors!
            }
        }

        protected Task OnPaste(ClipboardEventArgs e) => Task.CompletedTask;

        protected async Task OnFocus()
        {
            IsFocused = true;
            await OnFocusChanged.InvokeAsync();

            // NEW: Raise focused event
            if (OnFocused.HasDelegate)
            {
                await OnFocused.InvokeAsync();
            }
        }

        protected async Task OnBlur()
        {
            IsFocused = false;
            await OnFocusChanged.InvokeAsync();

            // NEW: Raise blurred event
            if (OnBlurred.HasDelegate)
            {
                await OnBlurred.InvokeAsync();
            }
        }

        protected async Task OnToolbarMouseDown(MouseEventArgs e)
        {
            // Save selection when interacting with toolbar
            if (_jsModule != null)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("saveSelection");
                }
                catch { }
            }
        }

        protected async Task UpdateToolbarState()
        {
            if (_jsModule == null) return;
            try
            {
                var formats = await _jsModule.InvokeAsync<string[]>("getActiveFormats");
                _activeFormats = new HashSet<string>(formats);

                if (_activeFormats.Contains("justifyCenter"))
                    alignment = "center";
                else if (_activeFormats.Contains("justifyRight"))
                    alignment = "right";
                else if (_activeFormats.Contains("justifyFull"))
                    alignment = "justify";
                else
                    alignment = "left";

                try
                {
                    _currentTextColor = await _jsModule.InvokeAsync<string>("getCurrentTextColor");
                }
                catch
                {
                    _currentTextColor = "#FF0000";
                }

                try
                {
                    _currentHighlightColor = await _jsModule.InvokeAsync<string>("getCurrentBackgroundColor");
                }
                catch
                {
                    _currentHighlightColor = "#FFFF00";
                }

                try
                {
                    _isEmojiSelected = await _jsModule.InvokeAsync<bool>("isEmojiSelected");
                }
                catch
                {
                    _isEmojiSelected = false;
                }

                // NEW: Get current font size
                try
                {
                    _currentFontSize = await _jsModule.InvokeAsync<string>("getCurrentFontSize");
                }
                catch
                {
                    _currentFontSize = "3"; // Default to Normal
                }

                await UpdateHeadingState();

                // NEW: Raise format changed event
                await RaiseFormatChangedEvent();

                // NEW: Raise selection changed event  
                await RaiseSelectionChangedEvent();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                await RaiseErrorEvent("Failed to update toolbar state", ex);
            }
        }

        protected async Task UpdateHeadingState()
        {
            if (_jsModule == null) return;
            try
            {
                _currentHeadingLevel = await _jsModule.InvokeAsync<string>("getCurrentBlock");
            }
            catch { }
        }

        // NEW: Update current text color based on cursor position
        private async Task UpdateCurrentTextColorAsync()
        {
            try
            {
                if (_jsModule != null)
                {
                    var color = await _jsModule.InvokeAsync<string>("getCurrentTextColor");
                    if (!string.IsNullOrEmpty(color))
                    {
                        _currentTextColor = color;
                        StateHasChanged();
                    }
                }
            }
            catch { /* Silently fail */ }
        }

        // NEW: Update current background color based on cursor position
        private async Task UpdateCurrentBackColorAsync()
        {
            try
            {
                if (_jsModule != null)
                {
                    var color = await _jsModule.InvokeAsync<string>("getCurrentBackgroundColor");
                    if (!string.IsNullOrEmpty(color))
                    {
                        _currentHighlightColor = color;
                        StateHasChanged();
                    }
                }
            }
            catch { /* Silently fail */ }
        }

        protected async Task ExecuteCommand(FormatCommand command)
        {
            if (_jsModule == null) return;

            try
            {
                var commandName = command switch
                {
                    FormatCommand.Bold => "bold",
                    FormatCommand.Italic => "italic",
                    FormatCommand.Underline => "underline",
                    FormatCommand.Strikethrough => "strikeThrough",
                    FormatCommand.InsertUnorderedList => "insertUnorderedList",
                    FormatCommand.InsertOrderedList => "insertOrderedList",
                    FormatCommand.RemoveFormat => "removeFormat",
                    FormatCommand.Undo => "undo",
                    FormatCommand.Redo => "redo",
                    FormatCommand.HeadingH1 => "formatBlock:h1",
                    FormatCommand.HeadingH2 => "formatBlock:h2",
                    FormatCommand.HeadingH3 => "formatBlock:h3",
                    FormatCommand.Paragraph => "formatBlock:p",
                    FormatCommand.HorizontalRule => "insertHorizontalRule",
                    FormatCommand.Subscript => "subscript",
                    FormatCommand.Superscript => "superscript",
                    FormatCommand.Indent => "indent",
                    FormatCommand.Outdent => "outdent",
                    FormatCommand.AlignLeft => "justifyLeft",
                    FormatCommand.AlignCenter => "justifyCenter",
                    FormatCommand.AlignRight => "justifyRight",
                    FormatCommand.AlignJustify => "justifyFull",
                    FormatCommand.FontSizeSmall => "fontSize:1",
                    FormatCommand.FontSizeNormal => "fontSize:3",
                    FormatCommand.FontSizeMedium => "fontSize:4",
                    FormatCommand.FontSizeLarge => "fontSize:5",
                    FormatCommand.FontSizeXLarge => "fontSize:6",
                    FormatCommand.FontSizeXXLarge => "fontSize:7",
                    FormatCommand.FontFamilyArial => "fontName:Arial",
                    FormatCommand.FontFamilyCourierNew => "fontName:Courier New",
                    FormatCommand.FontFamilyGaramond => "fontName:Garamond",
                    FormatCommand.FontFamilyGeorgia => "fontName:Georgia",
                    FormatCommand.FontFamilyHelvetica => "fontName:Helvetica",
                    FormatCommand.FontFamilyImpact => "fontName:Impact",
                    FormatCommand.FontFamilyTahoma => "fontName:Tahoma",
                    FormatCommand.FontFamilyTimesNewRoman => "fontName:Times New Roman",
                    FormatCommand.FontFamilyTrebuchet => "fontName:Trebuchet MS",
                    FormatCommand.FontFamilyVerdana => "fontName:Verdana",
                    _ => null
                };

                if (commandName != null)
                {
                    // NEW: Raise before command event
                    var canExecute = await RaiseBeforeCommandEvent(command, commandName);
                    if (!canExecute) return;

                    // NEW: Raise undo/redo events
                    if (command == FormatCommand.Undo && OnUndo.HasDelegate)
                    {
                        await OnUndo.InvokeAsync();
                    }
                    else if (command == FormatCommand.Redo && OnRedo.HasDelegate)
                    {
                        await OnRedo.InvokeAsync();
                    }

                    // Execute the command
                    if (commandName.StartsWith("formatBlock:"))
                    {
                        var blockType = commandName.Split(':')[1];
                        await _jsModule.InvokeVoidAsync("executeFormatBlock", blockType);
                    }
                    else if (commandName.StartsWith("fontSize:"))
                    {
                        var size = commandName.Split(':')[1];
                        await _jsModule.InvokeVoidAsync("executeFontSize", size);
                    }
                    else if (commandName.StartsWith("fontName:"))
                    {
                        var fontName = commandName.Split(':')[1];
                        await _jsModule.InvokeVoidAsync("executeFontName", fontName);
                    }
                    else
                    {
                        await _jsModule.InvokeVoidAsync("executeCommand", commandName);
                    }

                    await UpdateToolbarState();

                    await Task.Delay(50);
                    var html = await GetHtmlAsync();

                    _isUpdating = true;
                    Value = HtmlSanitizer.Sanitize(html);
                    _previousValue = Value;
                    await ValueChanged.InvokeAsync(Value);
                    _isUpdating = false;

                    // NEW: Raise content changed and after command events
                    await RaiseContentChangedEvent(ChangeSource.Command);
                    await RaiseAfterCommandEvent(command, commandName, true);
                }
            }
            catch (Exception ex)
            {
                await RaiseErrorEvent($"Command execution failed: {command}", ex);
                await RaiseAfterCommandEvent(command, command.ToString(), false, ex.Message);
            }
        }

        protected async Task CreateLink()
        {
            if (_jsModule == null) return;

            var selectedText = await _jsModule.InvokeAsync<string>("getSelectedText");

            // Check if cursor is inside existing link
            await UpdateToolbarState();
            if (_activeFormats.Contains("link"))
            {
                // Show option to edit or remove
                var action = await JS.InvokeAsync<bool>("confirm", "Remove existing link?");
                if (action)
                {
                    await RemoveLink();
                }
                return;
            }

            // If no text is selected, show helpful message
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                await JS.InvokeAsync<object>("alert", "Please select text first, then click the link button.");
                await ReturnFocusToEditor();
                return;
            }

            // Prompt user for URL
            var url = await JS.InvokeAsync<string>("prompt", $"Enter URL for \"{selectedText}\":", "https://");
            
            if (string.IsNullOrWhiteSpace(url))
            {
                await ReturnFocusToEditor();
                return;
            }

            // Add https:// if missing
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            // Raise before command event
            await RaiseBeforeCommandEvent(FormatCommand.CreateLink, url);

            await _jsModule.InvokeVoidAsync("createLink", url);

            var html = await GetHtmlAsync();
            _isUpdating = true;
            Value = HtmlSanitizer.Sanitize(html);
            _previousValue = Value;
            await ValueChanged.InvokeAsync(Value);
            _isUpdating = false;

            // Raise link created event
            if (OnLinkCreated.HasDelegate)
            {
                var linkEventArgs = new LinkEventArgs
                {
                    Url = url,
                    DisplayText = selectedText  
                };
                await OnLinkCreated.InvokeAsync(linkEventArgs);
            }

            // Raise after command event
            await RaiseAfterCommandEvent(FormatCommand.CreateLink, url, true);

            await UpdateToolbarState();
            await ReturnFocusToEditor();
        }

        protected async Task RemoveLink()
        {
            if (_jsModule == null) return;

            await UpdateToolbarState();

            if (_activeFormats.Contains("link"))
            {
                await _jsModule.InvokeVoidAsync("removeLink");

                await Task.Delay(50);
                var html = await GetHtmlAsync();

                _isUpdating = true;
                Value = HtmlSanitizer.Sanitize(html);
                _previousValue = Value;
                await ValueChanged.InvokeAsync(Value);
                _isUpdating = false;

                await UpdateToolbarState();
            }
        }

        protected bool IsFormatActive(string format) => _activeFormats.Contains(format);

        protected bool IsHeadingActive(string heading) => _currentHeadingLevel == heading;

        protected int GetCharacterCount()
        {
            var plainText = HtmlSanitizer.StripHtml(Value);
            return plainText.Length;
        }

        protected int GetWordCount()
        {
            var plainText = HtmlSanitizer.StripHtml(Value);
            if (string.IsNullOrWhiteSpace(plainText)) return 0;

            var words = plainText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        // ===== EVENT HELPER METHODS =====

        private int _previousCharCount = 0;
        private int _previousWordCount = 0;
        private bool _isDirty = false;

        private async Task RaiseContentChangedEvent(ChangeSource source)
        {
            if (OnContentChanged.HasDelegate)
            {
                await OnContentChanged.InvokeAsync(Value);
            }

            if (OnHtmlChanged.HasDelegate)
            {
                var eventArgs = new HtmlChangedEventArgs
                {
                    OldValue = _previousValue,
                    NewValue = Value,
                    CharacterCount = GetCharacterCount(),
                    WordCount = GetWordCount(),
                    Source = source
                };
                await OnHtmlChanged.InvokeAsync(eventArgs);
            }

            await CheckCharacterWordCountChanged();
            await CheckDirtyStateChanged();
        }

        private async Task CheckCharacterWordCountChanged()
        {
            var currentCharCount = GetCharacterCount();
            var currentWordCount = GetWordCount();

            if (OnCharacterCountChanged.HasDelegate && currentCharCount != _previousCharCount)
            {
                await OnCharacterCountChanged.InvokeAsync(currentCharCount);
                _previousCharCount = currentCharCount;
            }

            if (OnWordCountChanged.HasDelegate && currentWordCount != _previousWordCount)
            {
                await OnWordCountChanged.InvokeAsync(currentWordCount);
                _previousWordCount = currentWordCount;
            }
        }

        private async Task CheckDirtyStateChanged()
        {
            var wasDirty = _isDirty;
            _isDirty = Value != _previousValue;

            if (OnDirtyStateChanged.HasDelegate && wasDirty != _isDirty)
            {
                await OnDirtyStateChanged.InvokeAsync(_isDirty);
            }
        }

        private async Task RaiseSelectionChangedEvent()
        {
            if (!OnSelectionChanged.HasDelegate && !OnSelectedTextChanged.HasDelegate)
                return;

            try
            {
                if (_jsModule == null) return;

                var selectedText = await _jsModule.InvokeAsync<string>("getSelectedText");

                if (OnSelectedTextChanged.HasDelegate)
                {
                    await OnSelectedTextChanged.InvokeAsync(selectedText);
                }

                if (OnSelectionChanged.HasDelegate)
                {
                    var eventArgs = new SelectionChangedEventArgs
                    {
                        SelectedText = selectedText,
                        HasSelection = !string.IsNullOrEmpty(selectedText),
                        ActiveFormats = _activeFormats.ToList()
                    };
                    await OnSelectionChanged.InvokeAsync(eventArgs);
                }
            }
            catch { }
        }

        private async Task RaiseFormatChangedEvent()
        {
            if (!OnFormatChanged.HasDelegate) return;

            var eventArgs = new FormatChangedEventArgs
            {
                ActiveFormats = _activeFormats.ToList(),
                TextColor = _currentTextColor,
                BackgroundColor = _currentHighlightColor,
                Alignment = alignment,
                HeadingLevel = _currentHeadingLevel
            };

            await OnFormatChanged.InvokeAsync(eventArgs);
        }

        private async Task<bool> RaiseBeforeCommandEvent(FormatCommand command, string commandName, object? value = null)
        {
            if (!OnBeforeCommand.HasDelegate) return true;

            var eventArgs = new CommandEventArgs
            {
                Command = command,
                CommandName = commandName,
                Value = value,
                Success = true,
                Cancel = false
            };

            await OnBeforeCommand.InvokeAsync(eventArgs);
            return !eventArgs.Cancel;
        }

        private async Task RaiseAfterCommandEvent(FormatCommand command, string commandName, bool success, string? error = null)
        {
            if (!OnAfterCommand.HasDelegate) return;

            var eventArgs = new CommandEventArgs
            {
                Command = command,
                CommandName = commandName,
                Success = success,
                Error = error
            };

            await OnAfterCommand.InvokeAsync(eventArgs);
        }

        private async Task RaiseErrorEvent(string message, Exception? exception = null, ErrorSeverity severity = ErrorSeverity.Error)
        {
            if (!OnError.HasDelegate) return;

            var eventArgs = new EventArgs.ErrorEventArgs
            {
                Message = message,
                Exception = exception,
                Severity = severity
            };

            await OnError.InvokeAsync(eventArgs);
        }

        [JSInvokable]
        public async Task HandleContentChangedFromJs(string html)  // RENAMED
        {
            if (_isUpdating) return;
            _isUpdating = true;
            var sanitized = HtmlSanitizer.Sanitize(html);
            Value = sanitized;
            _previousValue = sanitized;
            await ValueChanged.InvokeAsync(sanitized);

            // NEW: Raise the event callback
            await RaiseContentChangedEvent(ChangeSource.EmojiShortcode);

            _isUpdating = false;
        }

        [JSInvokable]
        public void OnPastePlainText(string text) { }

        [JSInvokable]
        public async Task HandleCtrlK()
        {
            await UpdateToolbarState();

            if (_activeFormats.Contains("link"))
            {
                await EditLink();
            }
            else
            {
                await CreateLink();
            }
        }

        [JSInvokable]
        public async Task HandleCtrlShiftK()
        {
            await RemoveLink();
        }

        [JSInvokable]
        public async Task HandleCtrlShiftE()
        {
            await ToggleEmojiPicker();
        }

        private async Task<string> GetHtmlAsync()
        {
            if (_jsModule == null) return string.Empty;

            try
            {
                return await _jsModule.InvokeAsync<string>("getHtml", _editorRef);
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task SetHtmlAsync(string html)
        {
            if (_jsModule == null) return;
            try
            {
                _isUpdating = true;
                var sanitized = HtmlSanitizer.Sanitize(html);
                await _jsModule.InvokeVoidAsync("setHtml", _editorRef, sanitized);
            }
            catch { }
            finally
            {
                _isUpdating = false;
            }
        }

        public async Task ClearAsync()
        {
            await SetHtmlAsync("");
            _isUpdating = true;
            Value = "";
            _previousValue = "";
            await ValueChanged.InvokeAsync("");
            _isUpdating = false;
        }

        public async Task FocusAsync()
        {
            if (_jsModule == null) return;
            try
            {
                await _jsModule.InvokeVoidAsync("focusEditor", _editorRef);
            }
            catch { }
        }

        public string GetPlainText() => HtmlSanitizer.StripHtml(Value);

        protected async Task ToggleTextColorPicker()
        {
            var wasOpen = _showTextColorPicker;
            _showTextColorPicker = !_showTextColorPicker;
            _showBackgroundColorPicker = false;
            _showFontSizePicker = false;
            _showFontFamilyPicker = false;

            if (_showTextColorPicker && _jsModule != null)
            {
                // NEW: Raise color picker opened event
                if (OnColorPickerOpened.HasDelegate)
                {
                    await OnColorPickerOpened.InvokeAsync("text");
                }

                StateHasChanged();
                await Task.Delay(50);
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
                    await SetupColorPickerNavigation(_textColorPalette, ColorGridColumns);
                }
                catch { }
            }
            else if (!_showTextColorPicker && wasOpen)
            {
                // NEW: Raise color picker closed event
                if (OnColorPickerClosed.HasDelegate)
                {
                    await OnColorPickerClosed.InvokeAsync("text");
                }
            }
        }

        protected async Task ToggleBackgroundColorPicker()
        {
            var wasOpen = _showBackgroundColorPicker;
            _showBackgroundColorPicker = !_showBackgroundColorPicker;
            _showTextColorPicker = false;
            _showFontSizePicker = false;
            _showFontFamilyPicker = false;

            if (_showBackgroundColorPicker && _jsModule != null)
            {
                // NEW: Raise color picker opened event
                if (OnColorPickerOpened.HasDelegate)
                {
                    await OnColorPickerOpened.InvokeAsync("background");
                }

                StateHasChanged();
                await Task.Delay(50);
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
                    await SetupColorPickerNavigation(_bgColorPalette, ColorGridColumns);
                }
                catch { }
            }
            else if (!_showBackgroundColorPicker && wasOpen)
            {
                // NEW: Raise color picker closed event
                if (OnColorPickerClosed.HasDelegate)
                {
                    await OnColorPickerClosed.InvokeAsync("background");
                }
            }
        }

        protected async Task ToggleFontSizePicker()
        {
            _showFontSizePicker = !_showFontSizePicker;
            _showFontFamilyPicker = false;
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;

            if (_showFontSizePicker && _jsModule != null)
            {
                StateHasChanged();
                await Task.Delay(50);
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
                    await SetupListPickerNavigation(_fontSizePalette);
                }
                catch { }
            }
        }

        protected async Task ToggleFontFamilyPicker()
        {
            _showFontFamilyPicker = !_showFontFamilyPicker;
            _showFontSizePicker = false;
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;

            if (_showFontFamilyPicker && _jsModule != null)
            {
                StateHasChanged();
                await Task.Delay(50);
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
                    await SetupListPickerNavigation(_fontFamilyPalette);
                }
                catch { }
            }
        }

        protected async Task ToggleHeadingPicker()
        {
            _showHeadingPicker = !_showHeadingPicker;
            _showFontFamilyPicker = false;
            _showFontSizePicker = false;
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;

            if (_showHeadingPicker && _jsModule != null)
            {
                StateHasChanged();
                await Task.Delay(50);
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
                    await SetupListPickerNavigation(_headingPalette);
                }
                catch { }
            }
        }

        protected async Task SelectHeading(string level)
        {
            var command = level switch
            {
                "h1" => FormatCommand.HeadingH1,
                "h2" => FormatCommand.HeadingH2,
                "h3" => FormatCommand.HeadingH3,
                _ => FormatCommand.Paragraph
            };

            await ExecuteCommand(command);
            _showHeadingPicker = false;
            StateHasChanged();
            await Task.Delay(10);
            await FocusElement(_headingButton);
        }

        protected string GetCurrentHeadingLabel()
        {
            return _currentHeadingLevel switch
            {
                "h1" => "H1",
                "h2" => "H2",
                "h3" => "H3",
                _ => "¶"
            };
        }

        protected void CloseColorPickers()
        {
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;
            _showFontSizePicker = false;
            _showFontFamilyPicker = false;
            _showHeadingPicker = false;
            _showEmojiPicker = false;
        }

        protected async Task SelectTextColor(string color)
        {
            await ApplyTextColor(color);
            _currentTextColor = color; // Update immediately
            _showTextColorPicker = false;
            StateHasChanged();
            await Task.Delay(10);
            await FocusElement(_textColorButton);
        }

        protected async Task SelectBackgroundColor(string color)
        {
            await ApplyBackgroundColor(color);
            _currentHighlightColor = color; // Update immediately
            _showBackgroundColorPicker = false;
            StateHasChanged();
            await Task.Delay(10);
            await FocusElement(_bgColorButton);
        }

        protected async Task SelectFontSize(string size)
        {
            var command = size switch
            {
                "1" => FormatCommand.FontSizeSmall,
                "3" => FormatCommand.FontSizeNormal,
                "4" => FormatCommand.FontSizeMedium,
                "5" => FormatCommand.FontSizeLarge,
                "6" => FormatCommand.FontSizeXLarge,
                "7" => FormatCommand.FontSizeXXLarge,
                _ => (FormatCommand?)null
            };

            if (command.HasValue)
            {
                await ExecuteCommand(command.Value);
                _showFontSizePicker = false;
                StateHasChanged();
                await Task.Delay(10);
                await FocusElement(_fontSizeButton);
            }
        }

        protected async Task SelectFontFamily(string fontName)
        {
            var command = fontName switch
            {
                "Arial" => FormatCommand.FontFamilyArial,
                "Courier New" => FormatCommand.FontFamilyCourierNew,
                "Garamond" => FormatCommand.FontFamilyGaramond,
                "Georgia" => FormatCommand.FontFamilyGeorgia,
                "Helvetica" => FormatCommand.FontFamilyHelvetica,
                "Impact" => FormatCommand.FontFamilyImpact,
                "Tahoma" => FormatCommand.FontFamilyTahoma,
                "Times New Roman" => FormatCommand.FontFamilyTimesNewRoman,
                "Trebuchet MS" => FormatCommand.FontFamilyTrebuchet,
                "Verdana" => FormatCommand.FontFamilyVerdana,
                _ => (FormatCommand?)null
            };

            if (command.HasValue)
            {
                await ExecuteCommand(command.Value);
                _showFontFamilyPicker = false;
                StateHasChanged();
                await Task.Delay(10);
                await FocusElement(_fontFamilyButton);
            }
        }

        private async Task ApplyTextColor(string color)
        {
            if (_jsModule == null) return;

            try
            {
                await _jsModule.InvokeVoidAsync("executeForeColor", color);

                await UpdateToolbarState();
                await Task.Delay(50);
                var html = await GetHtmlAsync();

                _isUpdating = true;
                Value = HtmlSanitizer.Sanitize(html);
                _previousValue = Value;
                await ValueChanged.InvokeAsync(Value);
                _isUpdating = false;
            }
            catch { }
        }

        private async Task ApplyBackgroundColor(string color)
        {
            if (_jsModule == null) return;

            try
            {
                await _jsModule.InvokeVoidAsync("executeBackColor", color);

                await UpdateToolbarState();
                await Task.Delay(50);
                var html = await GetHtmlAsync();

                _isUpdating = true;
                Value = HtmlSanitizer.Sanitize(html);
                _previousValue = Value;
                await ValueChanged.InvokeAsync(Value);
                _isUpdating = false;
            }
            catch { }
        }

        protected string GetEditorStyle()
        {
            return $"--rte-min-height: {EnsureUnit(MinHeight)}; --rte-max-height: {EnsureUnit(MaxHeight)};";
        }

        private string EnsureUnit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "200px";

            value = value.Trim();

            if (char.IsLetter(value[^1]) || value.EndsWith("%"))
                return value;

            return value + "px";
        }

        private async Task HandleToolbarKeydown(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "ArrowRight":
                    focusedIndex = (focusedIndex + 1) % ToolbarButtonCount;
                    await FocusToolbarButton();
                    break;
                case "ArrowLeft":
                    focusedIndex = (focusedIndex - 1 + ToolbarButtonCount) % ToolbarButtonCount;
                    await FocusToolbarButton();
                    break;
                case "Home":
                    focusedIndex = 0;
                    await FocusToolbarButton();
                    break;
                case "End":
                    focusedIndex = ToolbarButtonCount - 1;
                    await FocusToolbarButton();
                    break;
                case "Escape":
                    // Close any open pickers and return focus to toolbar
                    CloseColorPickers();
                    await FocusToolbarButton();
                    break;
                case "Tab":
                    if (!e.ShiftKey)
                    {
                        // Tab forward - move to editor
                        await FocusAsync();
                    }
                    break;
            }
        }

        private async Task FocusToolbarButton()
        {
            if (_jsModule == null) return;
            try
            {
                await _jsModule.InvokeVoidAsync("focusToolbarButton", focusedIndex);
            }
            catch { }
        }

        private async Task OnEditorClick()
        {
            await UpdateToolbarState();
        }

        private async Task HandleColorPickerKeydown(KeyboardEventArgs e, string pickerType)
        {
            if (_jsModule == null) return;

            try
            {
                if (e.Key == "Escape")
                {
                    await ClosePickerAndFocusButton(pickerType);
                    return;
                }

                if (e.Key.StartsWith("Arrow") || e.Key == "Home" || e.Key == "End")
                {
                    await _jsModule.InvokeVoidAsync("handleColorPickerKeydown", e, ColorGridColumns);
                }
            }
            catch { }
        }

        private async Task HandleListPickerKeydown(KeyboardEventArgs e, string pickerType)
        {
            if (_jsModule == null) return;

            try
            {
                if (e.Key == "Escape")
                {
                    await ClosePickerAndFocusButton(pickerType);
                    return;
                }

                if (e.Key.StartsWith("Arrow") || e.Key == "Home" || e.Key == "End")
                {
                    await _jsModule.InvokeVoidAsync("handleListPickerKeydown", e);
                }
            }
            catch { }
        }

        private async Task ClosePickerAndFocusButton(string pickerType)
        {
            switch (pickerType)
            {
                case "textColor":
                    _showTextColorPicker = false;
                    await Task.Delay(10);
                    await FocusElement(_textColorButton);
                    break;
                case "backgroundColor":
                    _showBackgroundColorPicker = false;
                    await Task.Delay(10);
                    await FocusElement(_bgColorButton);
                    break;
                case "fontFamily":
                    _showFontFamilyPicker = false;
                    await Task.Delay(10);
                    await FocusElement(_fontFamilyButton);
                    break;
                case "fontSize":
                    _showFontSizePicker = false;
                    await Task.Delay(10);
                    await FocusElement(_fontSizeButton);
                    break;
                case "heading":
                    _showHeadingPicker = false;
                    await Task.Delay(10);
                    await FocusElement(_headingButton);
                    break;
            }
            StateHasChanged();
        }

        private async Task FocusElement(ElementReference elementRef)
        {
            if (_jsModule == null) return;

            try
            {
                await _jsModule.InvokeVoidAsync("focusElement", elementRef);
            }
            catch { }
        }

        private async Task SetupColorPickerNavigation(ElementReference palette, int columns)
        {
            if (_jsModule == null) return;

            try
            {
                var buttonRef = palette.Equals(_textColorPalette) ? _textColorButton : _bgColorButton;
                await _jsModule.InvokeVoidAsync("setupColorPickerNavigation", palette, columns, buttonRef);
            }
            catch { }
        }

        private async Task SetupListPickerNavigation(ElementReference palette)
        {
            if (_jsModule == null) return;

            try
            {
                var buttonRef = palette.Equals(_fontFamilyPalette) ? _fontFamilyButton : _fontSizeButton;
                await _jsModule.InvokeVoidAsync("setupListPickerNavigation", palette, buttonRef);
            }
            catch { }
        }

        private async Task ReturnFocusToEditor()
        {
            if (_jsModule != null)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("focusEditor", _editorRef);
                }
                catch { }
            }
        }

        private async Task IncreaseFontSize()
        {
            var sizes = new[] { "1", "3", "4", "5", "6", "7" };
            var commands = new[]
            {
                FormatCommand.FontSizeSmall,
                FormatCommand.FontSizeNormal,
                FormatCommand.FontSizeMedium,
                FormatCommand.FontSizeLarge,
                FormatCommand.FontSizeXLarge,
                FormatCommand.FontSizeXXLarge
            };

            var currentSize = await GetCurrentFontSize();
            var currentIndex = Array.IndexOf(sizes, currentSize);

            if (currentIndex == -1)
            {
                await ExecuteCommand(FormatCommand.FontSizeMedium);
            }
            else if (currentIndex < commands.Length - 1)
            {
                await ExecuteCommand(commands[currentIndex + 1]);
            }
        }

        private async Task DecreaseFontSize()
        {
            var sizes = new[] { "1", "3", "4", "5", "6", "7" };
            var commands = new[]
            {
                FormatCommand.FontSizeSmall,
                FormatCommand.FontSizeNormal,
                FormatCommand.FontSizeMedium,
                FormatCommand.FontSizeLarge,
                FormatCommand.FontSizeXLarge,
                FormatCommand.FontSizeXXLarge
            };

            var currentSize = await GetCurrentFontSize();
            var currentIndex = Array.IndexOf(sizes, currentSize);

            if (currentIndex == -1)
            {
                await ExecuteCommand(FormatCommand.FontSizeNormal);
            }
            else if (currentIndex > 0)
            {
                await ExecuteCommand(commands[currentIndex - 1]);
            }
        }

        private async Task<string> GetCurrentFontSize()
        {
            if (_jsModule == null) return "3";

            try
            {
                return await _jsModule.InvokeAsync<string>("getCurrentFontSize");
            }
            catch
            {
                return "3";
            }
        }

        protected async Task EditLink()
        {
            if (_jsModule == null) return;

            try
            {
                await _jsModule.InvokeAsync<string>("getSelectedText");
                await _jsModule.InvokeVoidAsync("removeLink");

                // TODO v1.2.0: Show dialog to edit the URL
                string url = "https://example.com";

                await _jsModule.InvokeVoidAsync("createLink", url);

                var html = await GetHtmlAsync();
                _isUpdating = true;
                Value = HtmlSanitizer.Sanitize(html);
                _previousValue = Value;
                await ValueChanged.InvokeAsync(Value);
                _isUpdating = false;

                await ReturnFocusToEditor();
            }
            catch { }
        }

        private async Task ToggleBold()
        {
            await ExecuteCommand(FormatCommand.Bold);
        }

        [JSInvokable]
        protected async Task ToggleEmojiPicker()
        {
            _showEmojiPicker = !_showEmojiPicker;

            if (_showEmojiPicker)
            {
                // IMPORTANT: Save the current selection BEFORE opening the picker
                if (_jsModule != null)
                {
                    try
                    {
                        await _jsModule.InvokeVoidAsync("saveSelection");
                        Console.WriteLine("Selection saved before opening emoji picker");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to save selection: {ex.Message}");
                    }
                }

                // Close other pickers
                _showTextColorPicker = false;
                _showBackgroundColorPicker = false;
                _showFontSizePicker = false;
                _showFontFamilyPicker = false;
                _showHeadingPicker = false;

                // Raise event
                if (OnEmojiPickerOpened.HasDelegate)
                {
                    await OnEmojiPickerOpened.InvokeAsync();
                }

                // Position the picker
                if (_jsModule != null && _showEmojiPicker)
                {
                    await Task.Delay(10);
                    try
                    {
                        await _jsModule.InvokeVoidAsync("adjustEmojiPickerPositionByQuery", _emojiButton);
                    }
                    catch { }
                }
            }
            else
            {
                // Raise event
                if (OnEmojiPickerClosed.HasDelegate)
                {
                    await OnEmojiPickerClosed.InvokeAsync();
                }
            }

            StateHasChanged();
        }

        protected void CloseEmojiPicker()
        {
            _showEmojiPicker = false;
            StateHasChanged();
        }

        protected async Task InsertEmoji(BlazorEmo.Models.Emo emoji)
        {
            if (_jsModule == null)
            {
                Console.WriteLine("[InsertEmoji] ERROR: _jsModule is null!");
                return;
            }

            Console.WriteLine($"[InsertEmoji] ===== STARTING EMOJI INSERTION =====");
            Console.WriteLine($"[InsertEmoji] Emoji: {emoji.Char}");
            Console.WriteLine($"[InsertEmoji] Current editor focus: {IsFocused}");

            try
            {
                // Step 1: Insert the emoji via JavaScript
                Console.WriteLine("[InsertEmoji] Step 1: Calling JavaScript insertText");
                await _jsModule.InvokeVoidAsync("insertText", emoji.Char);
                Console.WriteLine("[InsertEmoji] Step 1: Complete");

                // Step 2: Close the picker FIRST (before any other operations)
                Console.WriteLine("[InsertEmoji] Step 2: Closing emoji picker");
                _showEmojiPicker = false;

                // Step 3: Force a render to close the picker UI
                StateHasChanged();
                Console.WriteLine("[InsertEmoji] Step 2: Picker closed, UI updated");

                // Step 4: Wait for the DOM to settle
                Console.WriteLine("[InsertEmoji] Step 3: Waiting 100ms for DOM to settle");
                await Task.Delay(100);

                // Step 5: Get updated HTML
                Console.WriteLine("[InsertEmoji] Step 4: Getting HTML from editor");
                var html = await GetHtmlAsync();
                Console.WriteLine($"[InsertEmoji] Step 4: Got HTML, length={html?.Length ?? 0}");

                // Step 6: Update the value
                Console.WriteLine("[InsertEmoji] Step 5: Updating editor value");
                _isUpdating = true;
                Value = HtmlSanitizer.Sanitize(html);
                _previousValue = Value;
                await ValueChanged.InvokeAsync(Value);
                _isUpdating = false;
                Console.WriteLine("[InsertEmoji] Step 5: Value updated");

                // Step 7: Return focus to editor
                Console.WriteLine("[InsertEmoji] Step 6: Returning focus to editor");
                await ReturnFocusToEditor();
                Console.WriteLine("[InsertEmoji] Step 6: Focus returned");

                // DO NOT RAISE ANY EVENTS - they cause re-renders
                // if (OnEmojiInserted.HasDelegate) { ... }
                // await RaiseContentChangedEvent(ChangeSource.EmojiPicker);

                Console.WriteLine($"[InsertEmoji] ===== SUCCESS! Emoji inserted =====");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InsertEmoji] ===== ERROR =====");
                Console.WriteLine($"[InsertEmoji] Message: {ex.Message}");
                Console.WriteLine($"[InsertEmoji] Type: {ex.GetType().Name}");
                Console.WriteLine($"[InsertEmoji] Stack: {ex.StackTrace}");
            }
        }

        // Add to RichTextEditor.razor.cs

        private EmojiAutocomplete? emojiAutocomplete;

        [JSInvokable]
        public async Task ShowEmojiAutocomplete(string query, JsonElement positionJson)
        {
            if (emojiAutocomplete == null) return;

            var position = new EmojiAutocomplete.Position
            {
                X = positionJson.GetProperty("x").GetDouble(),
                Y = positionJson.GetProperty("y").GetDouble()
            };

            await emojiAutocomplete.ShowAsync(query, position);
        }

        [JSInvokable]
        public async Task HideEmojiAutocomplete()
        {
            if (emojiAutocomplete != null)
            {
                await emojiAutocomplete.HideAsync();
            }
        }

        [JSInvokable]
        public async Task HandleAutocompleteKey(string key)
        {
            if (emojiAutocomplete != null)
            {
                await emojiAutocomplete.HandleKeyAsync(key);
            }
        }

        private async Task OnEmojiSelected(string emoji)
        {
            Console.WriteLine($"[C#] OnEmojiSelected called with emoji: {emoji}");
            
            if (_jsModule == null)
            {
                Console.WriteLine("[C#] ERROR: _jsModule is null!");
                return;
            }
            
            try
            {
                Console.WriteLine("[C#] Calling insertEmojiAtShortcode...");
                await _jsModule.InvokeVoidAsync("insertEmojiAtShortcode", emoji);
                Console.WriteLine("[C#] insertEmojiAtShortcode completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[C#] ERROR in OnEmojiSelected: {ex.Message}");
                Console.WriteLine($"[C#] Stack: {ex.StackTrace}");
            }
        }

        private async Task HandleButtonKeyDown(KeyboardEventArgs e, Func<Task> action)
        {
            // Only handle Enter and Space - let arrow keys bubble to HandleToolbarKeydown
            if (e.Key == "Enter" || e.Key == " ")
            {
                // Execute the button's action
                await action();

                // CRITICAL: Re-focus the current toolbar button after action completes
                // This is necessary because StateHasChanged() causes re-render which loses focus
                await Task.Delay(10); // Allow render to complete
                await FocusToolbarButton();
            }
        }

        private async Task HandleColorButtonKeyDown(KeyboardEventArgs e, Func<Task> action)
        {
            // Handle Enter or Space to select color
            if (e.Key == "Enter" || e.Key == " ")
            {
                await action();
            }
        }

        protected string GetCurrentFontSizeLabel()
        {
            return _currentFontSize switch
            {
                "1" => "10",  // Small
                "3" => "14",  // Normal
                "4" => "16",  // Medium
                "5" => "18",  // Large
                "6" => "24",  // X-Large
                "7" => "32",  // XX-Large
                _ => "14"     // Default
            };
        }

        // Add helper method to determine when to prevent default key behavior:

        protected bool ShouldPreventDefaultKey(KeyboardEventArgs e)
        {
            // Prevent Enter when BypassEnterKey is true and no modifiers (except Shift is allowed for newlines)
            return BypassEnterKey && e.Key == "Enter" && !e.ShiftKey && !e.CtrlKey && !e.MetaKey;
        }
    }
}