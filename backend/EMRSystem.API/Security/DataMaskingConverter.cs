// API/Security/DataMaskingConverter.cs
public class DataMaskingConverter : JsonConverter<string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _maskingChar;
    private readonly MaskingRule _rule;

    public DataMaskingConverter(IHttpContextAccessor httpContextAccessor, MaskingRule rule = MaskingRule.MaskAll, string maskChar = "*")
    {
        _httpContextAccessor = httpContextAccessor;
        _rule = rule;
        _maskingChar = maskChar;
    }

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString(); // Không thay đổi khi đọc
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        // Kiểm tra quyền: Nếu user có quyền "ViewFullPII", không che dữ liệu
        if (httpContext?.User.HasClaim("permission", "ViewFullPII") == true)
        {
            writer.WriteStringValue(value);
            return;
        }

        writer.WriteStringValue(MaskValue(value));
    }

    private string MaskValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        return _rule switch
        {
            MaskingRule.ShowLast4 => value.Length > 4 ? new string(_maskingChar[0], value.Length - 4) + value.Substring(value.Length - 4) : new string(_maskingChar[0], value.Length),
            MaskingRule.ShowFirst4 => value.Length > 4 ? value.Substring(0, 4) + new string(_maskingChar[0], value.Length - 4) : new string(_maskingChar[0], value.Length),
            MaskingRule.Email => Regex.Replace(value, @"(.).*?@.*?\.(.*)", "$1***@***.$2"),
            _ => new string(_maskingChar[0], value.Length)
        };
    }
}

public enum MaskingRule
{
    MaskAll,
    ShowLast4,
    ShowFirst4,
    Email
}