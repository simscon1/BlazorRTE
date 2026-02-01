using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;
using BlazorRTE;

namespace BlazorRTE.Components
{

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
        [Parameter] public string MaxHeight { get; set; } = "600px"; // ← Changed from 300px

        [Inject] protected IJSRuntime JS { get; set; } = default!;

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

        // Accessibility state tracking
        private bool isBold = false;
        private bool isItalic = false;
        private bool isUnderline = false;
        private string alignment = "left";
        private int focusedIndex = 0;
        private const int ToolbarButtonCount = 6;

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

                    Console.WriteLine("RTE initialized successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RTE initialization error: {ex.Message}");
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
                catch (Exception ex)
                {
                    Console.WriteLine($"OnParametersSetAsync error: {ex.Message}");
                }
                finally
                {
                    _isUpdating = false;
                }
            }
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
            catch (Exception ex)
            {
                Console.WriteLine($"OnInput error: {ex.Message}");
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
                        if (!e.ShiftKey)
                            await ExecuteCommand(FormatCommand.Undo);
                        break;
                    case "y":
                        await ExecuteCommand(FormatCommand.Redo);
                        break;
                }
            }
            else if (e.CtrlKey && e.ShiftKey && e.Key.ToLower() == "z")
            {
                await ExecuteCommand(FormatCommand.Redo);
            }
            // Accessibility - keyboard navigation for toolbar
            switch (e.Key)
            {
                case "ArrowRight":
                case "ArrowDown":
                    focusedIndex = (focusedIndex + 1) % ToolbarButtonCount;
                    StateHasChanged();
                    break;
                case "ArrowLeft":
                case "ArrowUp":
                    focusedIndex = (focusedIndex - 1 + ToolbarButtonCount) % ToolbarButtonCount;
                    StateHasChanged();
                    break;
                case "Home":
                    focusedIndex = 0;
                    StateHasChanged();
                    break;
                case "End":
                    focusedIndex = ToolbarButtonCount - 1;
                    StateHasChanged();
                    break;
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Save selection error: {ex.Message}");
                }
            }
        }

        protected async Task UpdateToolbarState()
        {
            if (_jsModule == null) return;
            try
            {
                Console.WriteLine("UpdateToolbarState called"); // Debug
                var formats = await _jsModule.InvokeAsync<string[]>("getActiveFormats");
                Console.WriteLine($"Formats received: {string.Join(", ", formats)}"); // Debug
                
                _activeFormats = new HashSet<string>(formats);
                
                // Update accessibility state
                isBold = _activeFormats.Contains("bold");
                isItalic = _activeFormats.Contains("italic");
                isUnderline = _activeFormats.Contains("underline");
                
                // Update alignment state - only one should be active
                if (_activeFormats.Contains("justifyCenter"))
                    alignment = "center";
                else if (_activeFormats.Contains("justifyRight"))
                    alignment = "right";
                else if (_activeFormats.Contains("justifyFull"))
                    alignment = "justify";
                else
                    alignment = "left"; // Default to left
                
                await UpdateHeadingState();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateToolbarState error: {ex.Message}");
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

        private async Task<bool> IsFormatActiveAsync(string format)
        {
            if (_jsModule == null) return false;
            try
            {
                var formats = await _jsModule.InvokeAsync<string[]>("getActiveFormats");
                return formats.Contains(format, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        protected async Task ExecuteCommand(FormatCommand command)
        {
            if (_jsModule == null)
            {
                Console.WriteLine("JS module is null!");
                return;
            }

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
                    Console.WriteLine($"Executing command: {commandName}");

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
            catch (Exception ex)
            {
                Console.WriteLine($"ExecuteCommand error: {ex.Message}");
            }
        }

        protected async Task CreateLink()
        {
            if (_jsModule == null) return;
            try
            {
                var selectedText = await _jsModule.InvokeAsync<string>("getSelectedText");

                string url;

                if (!string.IsNullOrWhiteSpace(selectedText))
                {
                    if (selectedText.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        selectedText.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        url = selectedText;
                    }
                    else if (selectedText.Contains(".") && !selectedText.Contains(" "))
                    {
                        url = $"https://{selectedText}";
                    }
                    else
                    {
                        url = "https://example.com";
                    }
                }
                else
                {
                    url = "https://example.com";
                }

                await _jsModule.InvokeVoidAsync("createLink", url);

                var html = await GetHtmlAsync();
                _isUpdating = true;
                Value = HtmlSanitizer.Sanitize(html);
                _previousValue = Value;
                await ValueChanged.InvokeAsync(Value);
                _isUpdating = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateLink error: {ex.Message}");
            }
        }

        protected async Task OnHeadingChanged(ChangeEventArgs e)
        {
            var value = e.Value?.ToString() ?? "";

            var command = value switch
            {
                "h1" => FormatCommand.HeadingH1,
                "h2" => FormatCommand.HeadingH2,
                "h3" => FormatCommand.HeadingH3,
                "" => FormatCommand.Paragraph,
                "p" => FormatCommand.Paragraph,
                _ => (FormatCommand?)null
            };

            if (command.HasValue)
            {
                await ExecuteCommand(command.Value);
            }
        }

        protected async Task OnFontSizeChanged(ChangeEventArgs e)
        {
            var value = e.Value?.ToString() ?? "";

            var command = value switch
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
            }
        }

        protected async Task OnFontFamilyChanged(ChangeEventArgs e)
        {
            var value = e.Value?.ToString() ?? "";

            var command = value switch
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
            catch (Exception ex)
            {
                Console.WriteLine($"SetHtmlAsync error: {ex.Message}");
            }
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
            try { await _jsModule.InvokeVoidAsync("focusEditor", _editorRef); }
            catch (Exception ex)
            {
                Console.WriteLine($"FocusAsync error: {ex.Message}");
            }
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
                await Task.Delay(50); // Let palette render
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
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
                await Task.Delay(50); // Let palette render
                try
                {
                    await _jsModule.InvokeVoidAsync("adjustColorPalettePosition");
                }
                catch { }
            }
        }

        protected async Task ToggleFontSizePicker()
        {
            _showFontSizePicker = !_showFontSizePicker;
            _showFontFamilyPicker = false; // Close other pickers
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;

            if (_showFontSizePicker && _jsModule != null)
            {
                StateHasChanged();
                await Task.Delay(50);
                try { await _jsModule.InvokeVoidAsync("adjustColorPalettePosition"); }
                catch { }
            }
        }

        protected async Task ToggleFontFamilyPicker()
        {
            _showFontFamilyPicker = !_showFontFamilyPicker;
            _showFontSizePicker = false; // Close other pickers
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;

            if (_showFontFamilyPicker && _jsModule != null)
            {
                StateHasChanged();
                await Task.Delay(50);
                try { await _jsModule.InvokeVoidAsync("adjustColorPalettePosition"); }
                catch { }
            }
        }

        protected void CloseColorPickers()
        {
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;
            _showFontSizePicker = false;        // ADD
            _showFontFamilyPicker = false;      // ADD
        }

        protected async Task SelectTextColor(string color)
        {
            await ApplyTextColor(color);
            _showTextColorPicker = false; // Close after selection
        }

        protected async Task SelectBackgroundColor(string color)
        {
            await ApplyBackgroundColor(color);
            _showBackgroundColorPicker = false; // Close after selection
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
                _showFontSizePicker = false; // Close after selection
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
                _showFontFamilyPicker = false; // Close after selection
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
            catch (Exception ex)
            {
                Console.WriteLine($"ApplyTextColor error: {ex.Message}");
            }
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
            catch (Exception ex)
            {
                Console.WriteLine($"ApplyBackgroundColor error: {ex.Message}");
            }
        }

        protected string GetEditorStyle()
        {
            return $"--rte-min-height: {EnsureUnit(MinHeight)}; --rte-max-height: {EnsureUnit(MaxHeight)};";
        }

        private string EnsureUnit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "200px"; // Fallback default

            value = value.Trim();
            
            // Check if value already has a unit (px, em, rem, %, vh, etc.)
            if (char.IsLetter(value[^1]) || value.EndsWith("%"))
                return value;
            
            // No unit found, assume pixels
            return value + "px";
        }

        private async Task ToggleBold()
        {
            await ExecuteCommand(FormatCommand.Bold);
        }

        private async Task ToggleItalic()
        {
            await ExecuteCommand(FormatCommand.Italic);
        }

        private async Task ToggleUnderline()
        {
            await ExecuteCommand(FormatCommand.Underline);
        }

        private async Task SetAlignment(string align)
        {
            // If clicking the same alignment, toggle back to left
            if (alignment == align && align != "left")
            {
                alignment = "left";
                await ExecuteCommand(FormatCommand.AlignLeft);
            }
            else
            {
                alignment = align;
                var command = align switch
                {
                    "left" => FormatCommand.AlignLeft,
                    "center" => FormatCommand.AlignCenter,
                    "right" => FormatCommand.AlignRight,
                    "justify" => FormatCommand.AlignJustify,
                    _ => FormatCommand.AlignLeft
                };
                await ExecuteCommand(command);
            }
        }

        private void HandleToolbarKeydown(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "ArrowRight":
                case "ArrowDown":
                    focusedIndex = (focusedIndex + 1) % ToolbarButtonCount;
                    StateHasChanged();
                    break;
                case "ArrowLeft":
                case "ArrowUp":
                    focusedIndex = (focusedIndex - 1 + ToolbarButtonCount) % ToolbarButtonCount;
                    StateHasChanged();
                    break;
                case "Home":
                    focusedIndex = 0;
                    StateHasChanged();
                    break;
                case "End":
                    focusedIndex = ToolbarButtonCount - 1;
                    StateHasChanged();
                    break;
            }
        }

        private async Task OnEditorClick()
        {
            Console.WriteLine("Editor clicked!");
            await UpdateToolbarState();
        }
    }
}