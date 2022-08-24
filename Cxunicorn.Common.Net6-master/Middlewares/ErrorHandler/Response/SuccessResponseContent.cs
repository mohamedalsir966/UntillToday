namespace Cxunicorn.Common.Middlewares.ErrorHandler.Response;

public class SuccessResponseContent
{
    public static SuccessResponseContent Create(object result = null, int code = 200) =>
        new SuccessResponseContent(result, code);

    public int Code { get; set; } = 200;
    public object? Result { get; set; }

    public SuccessResponseContent(object? result = null, int code = 200)
    {
        Result = result;
        Code = code;
    }

}
