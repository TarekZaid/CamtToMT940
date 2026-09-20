namespace CamtToMT940.Models
{
    public class ConversionResult
    {
        public string SourcePath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public static ConversionResult Ok(string source, string output)
            => new() { SourcePath = source, OutputPath = output, Success = true };

        public static ConversionResult Fail(string source, string message)
            => new() { SourcePath = source, Success = false, ErrorMessage = message };
    }
}