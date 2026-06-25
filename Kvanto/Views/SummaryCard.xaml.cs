using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kvanto.Views;

public sealed partial class SummaryCard : UserControl
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(SummaryCard),
            new PropertyMetadata(string.Empty, OnIconChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(SummaryCard),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(SummaryCard),
            new PropertyMetadata(null, OnValueChanged));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public SummaryCard()
    {
        InitializeComponent();
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SummaryCard card)
            card.CardIcon.Glyph = (string)e.NewValue;
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SummaryCard card)
            card.CardLabel.Text = (string)e.NewValue;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SummaryCard card)
            card.CardValue.Text = e.NewValue?.ToString() ?? string.Empty;
    }
}
