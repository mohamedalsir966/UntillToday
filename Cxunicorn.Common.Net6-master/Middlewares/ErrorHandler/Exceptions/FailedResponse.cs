namespace Cxunicorn.Common.Middlewares.ErrorHandler.Exceptions;

public class FailedResponse : Exception
{
    public int Code { get; set; } = 400;
    public string Message { get; set; }

}
