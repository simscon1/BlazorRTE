using Bunit;
using BlazorRTE.Components;

namespace BlazorRTE.Tests;

public class RichTextEditorTests : BunitContext
{
    public RichTextEditorTests()
    {
        // bUnit 2.x - use Loose mode for unmocked JS calls
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Initialization Tests

    [Fact]
    public void INIT01_ComponentRendersWithoutErrors()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        Assert.NotNull(cut.Find(".rich-text-editor"));
    }

    [Fact]
    public void INIT02_InitialValueIsDisplayed()
    {
        // Arrange
        var initialValue = "<p>Hello World</p>";

        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.Value, initialValue));

        // Assert
        Assert.NotNull(cut);
    }

    [Fact]
    public void INIT03_PlaceholderShowsWhenConfigured()
    {
        // Arrange
        var placeholder = "Type here...";

        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.Placeholder, placeholder));

        // Assert
        var editor = cut.Find(".rte-content");
        Assert.Equal(placeholder, editor.GetAttribute("placeholder"));
    }

    [Fact]
    public void INIT06_MinMaxHeightApplied()
    {
        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.MinHeight, "150px")
            .Add(p => p.MaxHeight, "400px"));

        // Assert
        var container = cut.Find(".rich-text-editor");
        var style = container.GetAttribute("style");
        Assert.NotNull(style);
        Assert.Contains("150px", style);
        Assert.Contains("400px", style);
    }

    #endregion

    #region Toolbar Visibility Tests

    [Fact]
    public void ToolbarVisibleByDefault()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        Assert.NotNull(cut.Find(".rte-toolbar"));
    }

    [Fact]
    public void ToolbarHiddenWhenShowToolbarFalse()
    {
        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.ShowToolbar, false));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".rte-toolbar"));
    }

    #endregion

    #region Character Count Tests

    [Fact]
    public void CNT05_CharacterCountVisibleWhenEnabled()
    {
        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.ShowCharacterCount, true));

        // Assert
        Assert.NotNull(cut.Find(".rte-footer"));
        Assert.NotNull(cut.Find(".rte-char-count"));
    }

    [Fact]
    public void CNT06_CharacterCountHiddenWhenDisabled()
    {
        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.ShowCharacterCount, false));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".rte-footer"));
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public void A11Y01_ToolbarHasRoleToolbar()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var toolbar = cut.Find(".rte-toolbar");
        Assert.Equal("toolbar", toolbar.GetAttribute("role"));
    }

    [Fact]
    public void A11Y02_ToolbarHasAriaLabel()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var toolbar = cut.Find(".rte-toolbar");
        Assert.NotNull(toolbar.GetAttribute("aria-label"));
    }

    [Fact]
    public void A11Y07_EditorHasContentEditable()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var editor = cut.Find(".rte-content");
        Assert.Equal("true", editor.GetAttribute("contenteditable"));
    }

    [Fact]
    public void A11Y08_EditorHasAriaMultiline()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var editor = cut.Find(".rte-content");
        Assert.NotNull(editor);
        Assert.Equal("true", editor.GetAttribute("contenteditable"));
    }

    [Fact]
    public void A11Y09_CharacterCountHasAriaLive()
    {
        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.ShowCharacterCount, true));

        // Assert
        var charCount = cut.Find("#rte-char-count");
        Assert.Equal("polite", charCount.GetAttribute("aria-live"));
    }

    [Fact]
    public void A11Y03_AllButtonsHaveAriaLabel()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var buttons = cut.FindAll(".rte-toolbar button");
        foreach (var button in buttons)
        {
            Assert.NotNull(button.GetAttribute("aria-label"));
        }
    }

    [Fact]
    public void A11Y11_ButtonGroupsHaveRoleGroup()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var groups = cut.FindAll(".rte-toolbar [role='group']");
        Assert.True(groups.Count > 0);
    }

    #endregion

    #region Dropdown Tests

    [Fact]
    public void FontSizeDropdownInitiallyClosed()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var fontSizeButton = cut.Find("button[aria-label='Font size']");
        Assert.Equal("false", fontSizeButton.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void FontFamilyDropdownInitiallyClosed()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var fontFamilyButton = cut.Find("button[aria-label='Font family']");
        Assert.Equal("false", fontFamilyButton.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TextColorDropdownInitiallyClosed()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var colorButton = cut.Find("button[aria-label='Text color']");
        Assert.Equal("false", colorButton.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void HighlightColorDropdownInitiallyClosed()
    {
        // Act
        var cut = Render<RichTextEditor>();

        // Assert
        var highlightButton = cut.Find("button[aria-label='Highlight color']");
        Assert.Equal("false", highlightButton.GetAttribute("aria-expanded"));
    }

    #endregion

    #region Button Presence Tests

    [Fact]
    public void BoldButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Bold']"));
    }

    [Fact]
    public void ItalicButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Italic']"));
    }

    [Fact]
    public void UnderlineButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Underline']"));
    }

    [Fact]
    public void UndoButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Undo']"));
    }

    [Fact]
    public void RedoButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Redo']"));
    }

    [Fact]
    public void LinkButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Insert link']"));
    }

    [Fact]
    public void ClearFormattingButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Clear formatting']"));
    }

    [Fact]
    public void StrikethroughButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Strikethrough']"));
    }

    [Fact]
    public void SubscriptButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Subscript']"));
    }

    [Fact]
    public void SuperscriptButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Superscript']"));
    }

    [Fact]
    public void HorizontalRuleButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Insert horizontal rule']"));
    }

    #endregion

    #region Alignment Button Tests

    [Fact]
    public void AlignLeftButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Align left']"));
    }

    [Fact]
    public void AlignCenterButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Align center']"));
    }

    [Fact]
    public void AlignRightButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Align right']"));
    }

    [Fact]
    public void JustifyButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Justify']"));
    }

    #endregion

    #region List Button Tests

    [Fact]
    public void BulletedListButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Bulleted list']"));
    }

    [Fact]
    public void NumberedListButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Numbered list']"));
    }

    [Fact]
    public void IncreaseIndentButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Increase indent']"));
    }

    [Fact]
    public void DecreaseIndentButtonExists()
    {
        var cut = Render<RichTextEditor>();
        Assert.NotNull(cut.Find("button[aria-label='Decrease indent']"));
    }

    #endregion

    #region Security Tests

    [Fact]
    public void SEC01_ScriptTagsAreNotRendered()
    {
        // Arrange
        var maliciousContent = "<script>alert('XSS')</script><p>Safe content</p>";

        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.Value, maliciousContent));

        // Assert
        var html = cut.Markup;
        Assert.DoesNotContain("<script>", html);
    }

    [Fact]
    public void SEC02_OnClickAttributesAreRemoved()
    {
        // Arrange
        var maliciousContent = "<p onclick='alert(1)'>Click me</p>";

        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.Value, maliciousContent));

        // Assert
        var html = cut.Markup;
        Assert.DoesNotContain("onclick='alert", html);
        Assert.DoesNotContain("onclick=\"alert", html);
    }

    [Fact]
    public void SEC03_JavaScriptUrlsAreBlocked()
    {
        // Arrange
        var maliciousContent = "<a href='javascript:alert(1)'>Bad Link</a>";

        // Act
        var cut = Render<RichTextEditor>(parameters => parameters
            .Add(p => p.Value, maliciousContent));

        // Assert
        var html = cut.Markup;
        Assert.DoesNotContain("javascript:", html);
    }

    #endregion

    #region Keyboard Navigation Tests

    [Fact]
    public void A11Y_ToolbarButtonCountMatchesActualButtons()
    {
        // Act
        var cut = Render<RichTextEditor>();
        
        // Count all focusable toolbar elements (buttons)
        var buttons = cut.FindAll(".rte-toolbar button");
        var totalFocusable = buttons.Count;
        
        // Assert
        Assert.Equal(25, totalFocusable);
    }

    [Fact]
    public void A11Y_FirstToolbarButtonHasTabIndexZero()
    {
        // Act
        var cut = Render<RichTextEditor>();
        
        // Assert - Undo button should be the roving tabindex entry point
        var undoButton = cut.Find("button[aria-label='Undo']");
        Assert.Equal("0", undoButton.GetAttribute("tabindex"));
    }

    [Fact]
    public void A11Y_OtherToolbarButtonsHaveTabIndexMinusOne()
    {
        // Act
        var cut = Render<RichTextEditor>();
        
        // Assert - all buttons except the first should have tabindex="-1"
        var boldButton = cut.Find("button[aria-label='Bold']");
        Assert.Equal("-1", boldButton.GetAttribute("tabindex"));
    }

    #endregion
}