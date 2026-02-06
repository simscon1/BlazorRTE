using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using BlazorRTE.HelperClasses;
using BlazorEmo.Models;
using BlazorEmo.Services;

namespace BlazorRTE.Components
{
    // TODO: Add emoji support for rich text editor and text area
    //TODO : Add XML comments
    //TODO : Support for typing emoji short codes (e.g., :smile:) and converting them to emojis in the editor
    // TODO: Accessibility improvements (v1.1.0)
    // - High contrast mode: Add forced-colors media queries
    //   - Test with Windows High Contrast (Dev Tools > Rendering > Emulate forced colors)
    //   - Ensure UI remains usable when system colors override

    // - Voice control: Ensure visible labels match aria-labels for voice users
    // - Screen reader testing: Test with NVDA, JAWS, VoiceOver

    public partial class RichTextEditor : ComponentBase, IAsyncDisposable
    {
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
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected IEmoService EmoService { get; set; } = default!;

        protected ElementReference _editorRef;
        protected bool IsFocused { get; set; }

        private IJSObjectReference? _jsModule;
        private DotNetObjectReference<RichTextEditor>? _dotNetRef;
        private HashSet<string> _activeFormats = new();
        private bool _isUpdating;
        private string _previousValue = string.Empty;
        private string _currentHeadingLevel = "";
        private bool _showTextColorPicker = false;
        private bool _showBackgroundColorPicker = false;
        private bool _showFontSizePicker = false;
        private bool _showFontFamilyPicker = false;
        private bool _showHeadingPicker = false;
        private ElementReference _headingButton;
        private ElementReference _headingPalette;

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
        private int focusedIndex = 0;
        private const int ToolbarButtonCount = 25;

        private ElementReference _fontFamilyButton;
        private ElementReference _fontFamilyPalette;
        private ElementReference _fontSizeButton;
        private ElementReference _fontSizePalette;
        private ElementReference _textColorButton;
        private ElementReference _textColorPalette;
        private ElementReference _bgColorButton;
        private ElementReference _bgColorPalette;

        private const int ColorGridColumns = 3;

        private string _currentTextColor = "#000000";
        private string _currentHighlightColor = "#FFFFFF";

        private bool _showEmojiPicker = false;
        private ElementReference _emojiButton;
        private ElementReference _emojiPickerContainer;
     
        private Dictionary<string, string>? _emojiShortcodeMap;

        private bool _isEmojiSelected = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    _dotNetRef = DotNetObjectReference.Create(this);
                    _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorRTE/rich-text-editor.js");
                    await _jsModule.InvokeVoidAsync("initializeEditor", _editorRef, _dotNetRef);

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
        }

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
                var categories = await EmoService.GetAllCategoriesAsync(useCompleteDataset: false);
                
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

            return _emojiShortcodeMap?.GetValueOrDefault(shortcode);
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
                    return;
                }

                Value = sanitized;
                _previousValue = sanitized;
                await ValueChanged.InvokeAsync(sanitized);
                await UpdateHeadingState();
            }
            catch
            {
                // Input handling failed silently
            }
            finally
            {
                _isUpdating = false;
            }
        }

        protected async Task OnKeyDown(KeyboardEventArgs e)
        {
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
        }

        protected Task OnPaste(ClipboardEventArgs e) => Task.CompletedTask;

        protected void OnFocus()
        {
            IsFocused = true;
            OnFocusChanged.InvokeAsync();
        }

        protected void OnBlur()
        {
            IsFocused = false;
            OnFocusChanged.InvokeAsync();
        }

        protected async Task OnToolbarMouseDown(MouseEventArgs e)
        {
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

                // Check if emoji is selected
                try
                {
                    _isEmojiSelected = await _jsModule.InvokeAsync<bool>("isEmojiSelected");
                }
                catch
                {
                    _isEmojiSelected = false;
                }

                await UpdateHeadingState();
                StateHasChanged();
            }
            catch { }
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
                }
            }
            catch { }
        }

        protected async Task CreateLink()
        {
            if (_jsModule == null) return;
            
            await _jsModule.InvokeAsync<string>("getSelectedText");
            
            // TODO: Show modal dialog for URL input
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

        [JSInvokable]
        public async Task OnContentChanged(string html)
        {
            if (_isUpdating) return;
            _isUpdating = true;
            var sanitized = HtmlSanitizer.Sanitize(html);
            Value = sanitized;
            _previousValue = sanitized;
            await ValueChanged.InvokeAsync(sanitized);
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
            _showTextColorPicker = !_showTextColorPicker;
            _showBackgroundColorPicker = false;
            _showFontSizePicker = false;
            _showFontFamilyPicker = false;

            if (_showTextColorPicker && _jsModule != null)
            {
                StateHasChanged();
                await Task.Delay(50);
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
                    await SetupColorPickerNavigation(_textColorPalette, ColorGridColumns);
                }
                catch { }
            }
        }

        protected async Task ToggleBackgroundColorPicker()
        {
            _showBackgroundColorPicker = !_showBackgroundColorPicker;
            _showTextColorPicker = false;
            _showFontSizePicker = false;
            _showFontFamilyPicker = false;

            if (_showBackgroundColorPicker && _jsModule != null)
            {
                StateHasChanged();
                await Task.Delay(50);
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
                    await SetupColorPickerNavigation(_bgColorPalette, ColorGridColumns);
                }
                catch { }
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
            await ReturnFocusToEditor();
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
            _showTextColorPicker = false;
            await ReturnFocusToEditor();
        }

        protected async Task SelectBackgroundColor(string color)
        {
            await ApplyBackgroundColor(color);
            _showBackgroundColorPicker = false;
            await ReturnFocusToEditor();
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
                await ReturnFocusToEditor();
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
                await ReturnFocusToEditor();
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
            
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;
            _showFontSizePicker = false;
            _showFontFamilyPicker = false;
            _showHeadingPicker = false;
            
            StateHasChanged();
            
            if (_showEmojiPicker && _jsModule != null)
            {
                await Task.Delay(100); // Increased delay to ensure rendering
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustEmojiPickerPositionByQuery", _emojiButton);
                }
                catch { }
            }
        }

        protected void CloseEmojiPicker()
        {
            _showEmojiPicker = false;
            StateHasChanged();
        }
 
        protected async Task InsertEmoji(BlazorEmo.Models.Emo emoji)
        {
            if (_jsModule == null) return;
            
            try
            {
                // Insert emoji at cursor position
                await _jsModule.InvokeVoidAsync("insertText", emoji.Char);
                
                // Close picker
                _showEmojiPicker = false;
                
                // Update editor value
                await Task.Delay(50);
                var html = await GetHtmlAsync();
                
                _isUpdating = true;
                Value = HtmlSanitizer.Sanitize(html);
                _previousValue = Value;
                await ValueChanged.InvokeAsync(Value);
                _isUpdating = false;
                
                // Return focus to editor
                await ReturnFocusToEditor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting emoji: {ex.Message}");
            }
        }
    }
}