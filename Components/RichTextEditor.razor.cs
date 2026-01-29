using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace BlazorRTE.Components
{
    public enum FormatCommand
    {
        Bold,
        Italic,
        Underline,
        Strikethrough,
        InsertUnorderedList,
        InsertOrderedList,
        CreateLink,
        RemoveFormat,
        Undo,
        Redo,
        HeadingH1,
        HeadingH2,
        HeadingH3,
        Paragraph,
        HorizontalRule,
        Subscript,
        Superscript,
        Indent,
        Outdent,
        AlignLeft,
        AlignCenter,
        AlignRight,
        AlignJustify,
        FontSizeSmall,
        FontSizeNormal,
        FontSizeMedium,
        FontSizeLarge,
        FontSizeXLarge,
        FontSizeXXLarge,
        FontFamilyArial,         // ADD
        FontFamilyCourierNew,    // ADD
        FontFamilyGaramond,      // ADD
        FontFamilyGeorgia,       // ADD
        FontFamilyHelvetica,     // ADD
        FontFamilyImpact,        // ADD
        FontFamilyTahoma,        // ADD
        FontFamilyTimesNewRoman, // ADD
        FontFamilyTrebuchet,     // ADD
        FontFamilyVerdana,        // ADD
    }

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

        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected ElementReference _editorRef;
        protected bool IsFocused { get; set; }

        private IJSObjectReference? _jsModule;
        private DotNetObjectReference<RichTextEditor>? _dotNetRef;
        private HashSet<string> _activeFormats = new();
        private bool _isUpdating;
        private string _previousValue = string.Empty;
        private string _currentHeadingLevel = "";
        private bool _showTextColorPicker = false;      // ADD THIS
        private bool _showBackgroundColorPicker = false; // ADD THIS

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
                    var sanitized = SanitizeHtml(Value);
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
                var sanitized = SanitizeHtml(html);
                var textOnly = StripHtml(sanitized);

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
                var formats = await _jsModule.InvokeAsync<string[]>("getActiveFormats");
                _activeFormats = new HashSet<string>(formats);
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
                    FormatCommand.FontFamilyArial => "fontName:Arial",                    // ADD
                    FormatCommand.FontFamilyCourierNew => "fontName:Courier New",         // ADD
                    FormatCommand.FontFamilyGaramond => "fontName:Garamond",              // ADD
                    FormatCommand.FontFamilyGeorgia => "fontName:Georgia",                // ADD
                    FormatCommand.FontFamilyHelvetica => "fontName:Helvetica",            // ADD
                    FormatCommand.FontFamilyImpact => "fontName:Impact",                  // ADD
                    FormatCommand.FontFamilyTahoma => "fontName:Tahoma",                  // ADD
                    FormatCommand.FontFamilyTimesNewRoman => "fontName:Times New Roman",  // ADD
                    FormatCommand.FontFamilyTrebuchet => "fontName:Trebuchet MS",         // ADD
                    FormatCommand.FontFamilyVerdana => "fontName:Verdana",                // ADD
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
                    else if (commandName.StartsWith("fontName:"))    // ADD THIS BLOCK
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
                    Value = SanitizeHtml(html);
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
                Value = SanitizeHtml(html);
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
                "" => FormatCommand.Paragraph,      // ADD THIS - handles "Normal" selection
                "p" => FormatCommand.Paragraph,     // ADD THIS - alternative
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
            var plainText = StripHtml(Value);
            return plainText.Length;
        }

        protected int GetWordCount()
        {
            var plainText = StripHtml(Value);
            if (string.IsNullOrWhiteSpace(plainText)) return 0;

            var words = plainText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        [JSInvokable]
        public async Task OnContentChanged(string html)
        {
            if (_isUpdating) return;
            _isUpdating = true;
            var sanitized = SanitizeHtml(html);
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
                var sanitized = SanitizeHtml(html);
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

        private string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            html = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"\s*on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"\s*on\w+\s*=\s*\S+", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"javascript:", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<(iframe|object|embed|applet|meta|link|style)[^>]*>.*?</\1>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<(iframe|object|embed|applet|meta|link|style)[^>]*/>", "", RegexOptions.IgnoreCase);

            // ADD "sub" and "sup" to allowed tags
            var allowedTags = new[] { "p", "br", "strong", "b", "em", "i", "u", "s", "strike", "ul", "ol", "li", "a", "span", "div", "h1", "h2", "h3", "hr", "sub", "sup", "font" };
            var allowedPattern = string.Join("|", allowedTags);
            html = Regex.Replace(html, $@"<(?!/?({allowedPattern})\b)[^>]+>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<(\w+)[^>]*>\s*</\1>", "");

            return html.Trim();
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            return Regex.Replace(html, @"<[^>]+>", "");
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

        public string GetPlainText() => StripHtml(Value);

        protected async Task ToggleTextColorPicker()
        {
            _showTextColorPicker = !_showTextColorPicker;
            _showBackgroundColorPicker = false;
            
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

        protected void CloseColorPickers()
        {
            _showTextColorPicker = false;
            _showBackgroundColorPicker = false;
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
                Value = SanitizeHtml(html);
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
                Value = SanitizeHtml(html);
                _previousValue = Value;
                await ValueChanged.InvokeAsync(Value);
                _isUpdating = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApplyBackgroundColor error: {ex.Message}");
            }
        }
    }
}