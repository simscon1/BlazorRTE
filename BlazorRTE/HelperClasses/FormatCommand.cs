namespace BlazorRTE.HelperClasses
{
    /// <summary>
    /// Formatting commands supported by RichTextEditor
    /// </summary>
    public enum FormatCommand
    {
        // Basic text formatting
        Bold,
        Italic,
        Underline,
        Strikethrough,

        // Advanced text formatting
        Subscript,
        Superscript,

        // Lists
        InsertUnorderedList,
        InsertOrderedList,

        // Links and special content
        CreateLink,
        RemoveFormat,
        HorizontalRule,

        // History
        Undo,
        Redo,

        // Block formatting
        HeadingH1,
        HeadingH2,
        HeadingH3,
        Paragraph,

        // Indentation
        Indent,
        Outdent,

        // Alignment
        AlignLeft,
        AlignCenter,
        AlignRight,
        AlignJustify,

        // Font sizes
        FontSizeSmall,
        FontSizeNormal,
        FontSizeMedium,
        FontSizeLarge,
        FontSizeXLarge,
        FontSizeXXLarge,

        // Font families
        FontFamilyArial,
        FontFamilyCourierNew,
        FontFamilyGaramond,
        FontFamilyGeorgia,
        FontFamilyHelvetica,
        FontFamilyImpact,
        FontFamilyTahoma,
        FontFamilyTimesNewRoman,
        FontFamilyTrebuchet,
        FontFamilyVerdana
    }
}
