namespace Mimo.Application.Helper.Payment
{
    public class VnPayOptions
    {
        public const string SectionName = "Vnpay";

        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string FrontendReturnUrl { get; set; } = "http://localhost:5173/payment-result";
        public string Version { get; set; } = "2.1.0";
        public string Command { get; set; } = "pay";
        public string CurrCode { get; set; } = "VND";
        public string Locale { get; set; } = "vn";
    }
}
