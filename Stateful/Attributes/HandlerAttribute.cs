namespace Stateful.Attributes; 

/// <summary>
/// Marks the method as an update handler.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HandlerAttribute : Attribute {
    /// <summary>
    /// Condition to match for
    /// </summary>
    protected Func<UpdateHandler, Task<bool>> Condition { get; init; }

    /// <summary>
    /// Does condition match
    /// </summary>
    /// <param name="ctx">Context</param>
    /// <returns>True if matches</returns>
    internal bool Match(UpdateHandler ctx)
        => Condition.Invoke(ctx).GetAwaiter().GetResult();
}