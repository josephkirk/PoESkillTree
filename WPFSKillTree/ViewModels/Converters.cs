﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.ViewModels.ItemAttribute;
using System.Text.RegularExpressions;

namespace POESKillTree.ViewModels
{
    class AttributeToTextblockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var attr = value as Attribute;
            if (attr == null)
                throw new NotImplementedException();

            var tb = new TextBlock() { TextWrapping = TextWrapping.Wrap };
            var txt = new Run(attr.Text);
            if (attr.Missing)
                txt.Foreground = Brushes.Red;

            tb.Inlines.Add(txt);

            foreach (var i in attr.Deltas)
            {
                if (i != 0)
                {
                    tb.Inlines.Add(" ");
                    txt = new Run(i.ToString("+#;-#;0"));
                    txt.Foreground = (i < 0) ? Brushes.Red : Brushes.Green;
                    tb.Inlines.Add(txt);
                }
            }

            return tb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class DebugViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class Attached
    {
        public static readonly DependencyProperty TextBlockInlinesProperty = DependencyProperty.RegisterAttached(
            "TextBlockInlines",
            typeof(IEnumerable<Inline>),
            typeof(Attached),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, TextboxInlinesPropertyChanged));


        public static IEnumerable<Inline> GetTextBlockInlines(TextBlock textBlock)
        {
            return textBlock.Inlines.ToList();
        }

        public static void SetTextBlockInlines(TextBlock textBlock, IEnumerable<Inline> value)
        {
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange(value);
        }

        private static void TextboxInlinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = d as TextBlock;
            if (textBlock == null)
                return;
            SetTextBlockInlines(textBlock, e.NewValue as IEnumerable<Inline>);
        }
    }

    [ValueConversion(typeof(ItemMod), typeof(IEnumerable<Inline>))]
    class ItemModToInlinesConverter : IValueConverter
    {
        private static SolidColorBrush locallyAffectedColor = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0xFF));
        private static SolidColorBrush fireAffectedColor = new SolidColorBrush(Color.FromRgb(0x96, 0x00, 0x04));
        private static SolidColorBrush coldAffectedColor = new SolidColorBrush(Color.FromRgb(0x36, 0x64, 0x92));
        private static SolidColorBrush lightningAffectedColor = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mod = value as ItemMod;
            if (mod == null)
                throw new NotImplementedException();

            var inlines = new List<Inline>();

            var backrep = ItemAttributes.Attribute.Backreplace;

            var matches = backrep.Matches(mod.Attribute).Cast<Match>().ToArray();
            int from = 0;
            for (int i = 0; i < matches.Length && i < mod.Value.Count; i++)
            {
                var m = matches[i];
                var istring = mod.Attribute.Substring(from, m.Index - from);
                var r = new Run(istring);

                SolidColorBrush clr = null;

                if (mod.ValueColor.Count > i)
                    switch (mod.ValueColor[i])
                    {
                        case ItemMod.ValueColoring.LocallyAffected:
                            clr = locallyAffectedColor;
                            break;
                        case ItemMod.ValueColoring.Fire:
                            clr = fireAffectedColor;
                            break;
                        case ItemMod.ValueColoring.Cold:
                            clr = coldAffectedColor;
                            break;
                        case ItemMod.ValueColoring.Lightning:
                            clr = lightningAffectedColor;
                            break;
                    }


                if ((istring == "-" || istring == "/") && parameter != null)
                    if (clr != null)
                        r.Foreground = clr;
                    else
                        r.Foreground = Brushes.White;

                inlines.Add(r);

                r = new Run(mod.Value[i].ToString());
                if (parameter != null)
                    if (clr != null)
                        r.Foreground = clr;
                    else
                        r.Foreground = Brushes.White;
                inlines.Add(r);

                from = m.Index + m.Length;
            }
            inlines.Add(new Run(mod.Attribute.Substring(from, mod.Attribute.Length - from)));


            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
