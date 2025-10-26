// Core/Entities/DataMaskingAttribute.cs
[AttributeUsage(AttributeTargets.Property)]
public class DataMaskingAttribute : JsonConverterAttribute
{
    private readonly MaskingRule _rule;
    public DataMaskingAttribute(MaskingRule rule = MaskingRule.MaskAll)
    {
        _rule = rule;
    }

    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        return new DataMaskingConverterFactory(_rule);
    }
}

// Factory để inject IHttpContextAccessor
public class DataMaskingConverterFactory : JsonConverterFactory
{
    private readonly MaskingRule _rule;
    public DataMaskingConverterFactory(MaskingRule rule) { _rule = rule; }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(string);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Lấy IHttpContextAccessor từ service provider
        var httpContextAccessor = ((IObjectFactory)options).ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        return new DataMaskingConverter(httpContextAccessor, _rule);
    }
}

// Helper để lấy service provider từ JsonSerializerOptions
public interface IObjectFactory
{
    IServiceProvider ServiceProvider { get; }
}

public class ObjectFactory : IObjectFactory
{
    public IServiceProvider ServiceProvider { get; }
    public ObjectFactory(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;
}