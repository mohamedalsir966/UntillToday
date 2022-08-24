namespace Cxunicorn.Common.Middlewares.ErrorHandler.Exceptions;

public class NotFoundResponse : Exception
{
    public int Code { get; set; } = 404;
    public string Message { get; set; }

}
