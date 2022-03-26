// DTO (Data transfer objects) for JSON magic

using System.Text.Json.Serialization;

record Job(string Name, string Command) : IJsonOnDeserialized
{
    // JSON deserializer does NOT respect the non-nullability attributes,
    // so we check it during OnDeserialized.  As well as doing other validation.
    //
    // Unfortunately .NET allows extra unknown JSON fields without raising an error :-|
    //
    void IJsonOnDeserialized.OnDeserialized()
    {
        MyValidate.ValidateString(Name, nameof(Name), min_len:1, max_len:10);
        MyValidate.ValidateString(Command, nameof(Command), min_len:1, max_len:10);
    }
}

record TakeJobRequest(string Name) : IJsonOnDeserialized
{
    void IJsonOnDeserialized.OnDeserialized()
    {
        MyValidate.ValidateString(Name, nameof(Name), min_len:1, max_len:10);
    }
}

record TakeJobResponse(
        string Command,
        string ExecutionId
        )
{
    // nothing to validate for a response
}

class MyValidate
{
    public static void ValidateString(string? val, string val_name,
            int min_len, int max_len,
            bool nullable=false
            )
    {
        if (val is null) {
            if (!nullable) {
                throw new MyWebException(400, $"can't be null {val_name}");
            } else {
                return;
            }
        }
        if (val.Length < min_len) {
            throw new MyWebException(400, $"too short: {val_name}");
        }
        if (val.Length > max_len) {
            throw new MyWebException(400, $"too long: {val_name}");
        }
    }
}

