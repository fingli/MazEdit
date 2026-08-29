using System.Globalization;
using System.Windows.Data;

namespace MazEdit
{
    public sealed class SummaryPart
    {
        public string Name { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
    }

    public sealed class SummaryPartsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string summary || string.IsNullOrWhiteSpace(summary))
                return Array.Empty<SummaryPart>();

            var parts = new List<SummaryPart>();
            foreach (var field in summary.Split("  ", StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = field.IndexOf('=');
                if (eq <= 0)
                {
                    parts.Add(new SummaryPart { Value = field });
                    continue;
                }

                parts.Add(new SummaryPart
                {
                    Name = field[..eq] + "=",
                    Value = field[(eq + 1)..]
                });
            }

            return parts;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
