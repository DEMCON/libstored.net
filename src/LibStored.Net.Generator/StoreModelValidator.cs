
namespace LibStored.Net.Generator;

internal static class StoreModelValidator
{
    public static void Validate(StoreModel model)
    {
        ValidateNotEmpty(model.Name, nameof(model.Name));
        ValidateNotEmpty(model.Hash, nameof(model.Hash));

        if (!model.LittleEndian)
        {
            throw new ArgumentException("Only Little Endian is supported");
        }

        foreach (var variable in model.Variables)
        {
            ValidateNotEmpty(variable.Name, nameof(variable.Name));
            ValidateNotEmpty(variable.Cname, nameof(variable.Cname));
            ValidateNotEmpty(variable.Type, nameof(variable.Type));
        }
    }

    private static void ValidateNotEmpty(string variable, string name)
    {
        if (string.IsNullOrEmpty(variable))
        {
            throw new ArgumentException($"{name} cannot be null or empty");
        }
    }
}
