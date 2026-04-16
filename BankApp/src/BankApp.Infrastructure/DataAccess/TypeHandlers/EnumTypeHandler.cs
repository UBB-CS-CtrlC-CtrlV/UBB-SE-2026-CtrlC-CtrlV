using System.Data;
using Dapper;

namespace BankApp.Infrastructure.DataAccess.TypeHandlers;

/// <summary>
/// Convert SQL types to corresponding Enum.
/// </summary>
/// <typeparam name="T">The enum to handle.</typeparam>
public class EnumTypeHandler<T> : SqlMapper.TypeHandler<T>
    where T : struct, Enum
{
    /// <inheritdoc />
    public override void SetValue(IDbDataParameter parameter, T value)
        => parameter.Value = value.ToString();

    /// <inheritdoc />
    public override T Parse(object value) => Enum.Parse<T>((string)value, ignoreCase: true);
}