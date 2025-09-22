using System.Globalization;
using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Services.Implementations;

public static class CurrencyFormatter
{
    // Return currency code for enum
    public static string ToCode(Currency currency) => currency switch
    {
        Currency.USD => "USD",
        Currency.EUR => "EUR",
        Currency.XAF => "XAF",
        _ => "XAF"
    };

    // Return a symbol or prefix to display for the currency
    public static string ToSymbol(Currency currency) => currency switch
    {
        Currency.USD => "$",
        Currency.EUR => "€",
        Currency.XAF => "FCFA ", // using prefix with space for readability
        _ => ""
    };

    // Excel number format string for a given currency. XAF has no decimals.
    public static string ToExcelNumberFormat(Currency currency) => currency switch
    {
        Currency.USD => "\"$\"#,##0.00",
        Currency.EUR => "\"€\"#,##0.00",
        Currency.XAF => "\"FCFA \"#,##0",
        _ => "#,##0.00"
    };

    // Format for PDF/text rendering
    public static string FormatAmount(decimal amount, Currency currency)
    {
        if (currency == Currency.XAF)
            return $"{ToSymbol(currency)}{decimal.Truncate(amount):N0}";

        var culture = currency == Currency.EUR ? CultureInfo.GetCultureInfo("fr-FR") : CultureInfo.GetCultureInfo("en-US");
        return ToSymbol(currency) + amount.ToString("N2", culture);
    }
}
